using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Text;
using TrippyGL.Utils;

namespace TrippyGL
{
    /// <summary>
    /// Provides a simple and efficient way to draw 2D textures in batches.
    /// </summary>
    public sealed class TextureBatcher : IDisposable
    {
        /// <summary>The initial capacity for the internal batch items array.</summary>
        public const uint InitialBatchItemsCapacity = 256;

        /// <summary>The maximum capacity for the internal batch items array.</summary>
        public const uint MaxBatchItemCapacity = int.MaxValue;

        /// <summary>The initial capacity for the <see cref="VertexBuffer{T}"/> used for drawing the item's vertices.</summary>
        private const uint InitialBufferCapacity = InitialBatchItemsCapacity * 3;

        /// <summary>The maximum capacity for the <see cref="VertexBuffer{T}"/> used for drawing the item's vertices.</summary>
        private const uint MaxBufferCapacity = 32768;

        /// <summary>
        /// The <see cref="TrippyGL.GraphicsDevice"/> with which this <see cref="TextureBatcher"/> was created.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; }

        /// <summary>Used to store vertices before sending them to <see cref="vertexBuffer"/>.</summary>
        private VertexColorTexture[]? triangles;

        /// <summary>Stores the triangles for rendering.</summary>
        private readonly VertexBuffer<VertexColorTexture> vertexBuffer;

        /// <summary>Stores the batch items that haven't been flushed yet.</summary>
        private TextureBatchItem[] batchItems;
        /// <summary>The amount of batch items stored in <see cref="batchItems"/>.</summary>
        private int batchItemCount;

        /// <summary>
        /// Whether <see cref="Begin(BatcherBeginMode)"/> was called on this <see cref="TextureBatcher"/>
        /// but <see cref="End"/> hasn't yet.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// The <see cref="BatcherBeginMode"/> specified in the last <see cref="Begin(BatcherBeginMode)"/>.
        /// </summary>
        public BatcherBeginMode BeginMode { get; private set; }

        /// <summary>The <see cref="ShaderProgram"/> this <see cref="TextureBatcher"/> is currently using.</summary>
        public ShaderProgram? ShaderProgram { get; private set; }

        /// <summary>
        /// The <see cref="ShaderUniform"/> this <see cref="TextureBatcher"/> uses for setting the texture
        /// on <see cref="ShaderProgram"/>.
        /// </summary>
        public ShaderUniform TextureUniform { get; private set; }

        /// <summary>Whether this <see cref="TextureBatcher"/> has been disposed.</summary>
        public bool IsDisposed => vertexBuffer.IsDisposed;

        /// <summary>
        /// Creates a <see cref="TextureBatcher"/>, with a specified initial capacity for batch items as
        /// optional parameter.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this <see cref="TextureBatcher"/> will use.</param>
        /// <param name="initialBatchCapacity">The initial capacity for the internal batch items array.</param>
        public TextureBatcher(GraphicsDevice graphicsDevice, uint initialBatchCapacity = InitialBatchItemsCapacity)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            if (initialBatchCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(initialBatchCapacity), nameof(initialBatchCapacity) + " must be greater than 0.");

            batchItems = new TextureBatchItem[initialBatchCapacity];
            for (int i = 0; i < batchItems.Length; i++)
                batchItems[i] = new TextureBatchItem();
            batchItemCount = 0;
            IsActive = false;

            GraphicsDevice = graphicsDevice;
            vertexBuffer = new VertexBuffer<VertexColorTexture>(graphicsDevice, InitialBufferCapacity, BufferUsage.StreamDraw);
        }

        /// <summary>
        /// Sets the <see cref="TrippyGL.ShaderProgram"/> this <see cref="TextureBatcher"/> uses for rendering.
        /// </summary>
        /// <param name="simpleProgram">The <see cref="SimpleShaderProgram"/> to use.</param>
        /// <remarks>
        /// The <see cref="SimpleShaderProgram"/> doesn't need to have texture sampling and vertex colors
        /// enabled. Lighting however will not work, since the vertices lack normal data.<para/>
        /// The locations of the attributes on the program must still match 0 for position, 1 for
        /// color and 2 for texture coordinates.
        /// </remarks>
        public void SetShaderProgram(SimpleShaderProgram simpleProgram)
        {
            SetShaderProgram(simpleProgram, (simpleProgram != null && simpleProgram.TextureEnabled) ? simpleProgram.sampUniform : default);
        }

        /// <summary>
        /// Sets the <see cref="TrippyGL.ShaderProgram"/> this <see cref="TextureBatcher"/> uses for rendering.
        /// </summary>
        /// <param name="shaderProgram">The <see cref="TrippyGL.ShaderProgram"/> to use.</param>
        /// <param name="textureUniform">The <see cref="ShaderUniform"/> from which to set the textures to draw.</param>
        /// <remarks>
        /// The <see cref="TrippyGL.ShaderProgram"/> must use attribute location 0 for position.
        /// Color and TexCoords are optional, but if present they must be in attribute locations
        /// 1 and 2 respectively.<para/>
        /// textureUniform can be an empty <see cref="ShaderUniform"/>, in which case the <see cref="TextureBatcher"/>
        /// will simply not set any texture when rendering.
        /// </remarks>
        public void SetShaderProgram(ShaderProgram shaderProgram, ShaderUniform textureUniform)
        {
            if (IsActive)
                throw new InvalidOperationException(nameof(ShaderProgram) + " cant be changed while the " + nameof(TextureBatcher) + " is active.");

            if (shaderProgram == null)
            {
                ShaderProgram = null;
                TextureUniform = default;
                return;
            }

            if (shaderProgram.GraphicsDevice != GraphicsDevice)
                throw new ArgumentException(nameof(ShaderProgram) + " must belong to the same " + nameof(GraphicsDevice)
                    + " this " + nameof(TextureBatcher) + " was created with.", nameof(shaderProgram));

            // If textureUniform isn't empty, we check that it's valid.
            if (!textureUniform.IsEmpty)
            {
                if (textureUniform.OwnerProgram != shaderProgram)
                    throw new ArgumentException(nameof(textureUniform) + " must belong to the provided " + nameof(ShaderProgram) + ".", nameof(textureUniform));

                if (!TrippyUtils.IsUniformSampler2DType(textureUniform.UniformType))
                    throw new ArgumentException("The provided " + nameof(ShaderUniform) + " must be a Sampler2D type.", nameof(textureUniform));
            }

            // We check that the ShaderProgram has an attribute in location 0 and is a FloatVec3.
            if (!shaderProgram.TryFindAttributeByLocation(0, out ActiveVertexAttrib attrib) || attrib.AttribType != AttributeType.FloatVec3)
                throw new ArgumentException("The shader program's attribute at location 0 must be of type FloatVec3 (used for position).", nameof(shaderProgram));

            // If the ShaderProgram has an attribute in location 1, it has to be a FloatVec4.
            if (shaderProgram.TryFindAttributeByLocation(1, out attrib) && attrib.AttribType != AttributeType.FloatVec4)
                throw new ArgumentException("The shader program's attribute at location 1 must be of type FloatVec4 (used for color).", nameof(shaderProgram));

            // If the ShaderProgram has an attribute in location 2, it has to be a FloatVec2 used for TexCoords.
            if (shaderProgram.TryFindAttributeByLocation(2, out attrib) && attrib.AttribType != AttributeType.FloatVec2)
                throw new ArgumentException("The shader program's attribute at location 2 must be of type FloatVec2 (used for texture coordinates).", nameof(shaderProgram));

            // Everything's valid, let's store the new ShaderProgram and ShaderUniform.
            ShaderProgram = shaderProgram;
            TextureUniform = textureUniform;
        }

        /// <summary>
        /// Begins drawing a new batch of textures.
        /// </summary>
        /// <param name="beginMode">The mode in which flushing the textures is handled.</param>
        public void Begin(BatcherBeginMode beginMode = BatcherBeginMode.Deferred)
        {
            if (vertexBuffer.IsDisposed)
                throw new ObjectDisposedException(nameof(TextureBatcher));

            if (!Enum.IsDefined(typeof(BatcherBeginMode), beginMode))
                throw new ArgumentException("Invalid " + nameof(BatcherBeginMode) + " value.", nameof(beginMode));

            if (ShaderProgram == null)
                throw new InvalidOperationException("A " + nameof(ShaderProgram) + " must be specified (via " + nameof(SetShaderProgram) + "()) before using Begin().");

            if (IsActive)
                throw new InvalidOperationException("This " + nameof(TextureBatcher) + " has already begun.");

            batchItemCount = 0;
            BeginMode = beginMode;
            IsActive = true;
        }

        /// <summary>
        /// Ends drawing a batch of textures and flushes any textures that are waiting to be drawn.
        /// </summary>
        public void End()
        {
            if (!IsActive)
                throw new InvalidOperationException("Begin() must be called before End().");

            // We flush. We can ensure all the items have the same texture if BeginMode is Immediate or OnTheFly.
            Flush(BeginMode == BatcherBeginMode.Immediate || BeginMode == BatcherBeginMode.OnTheFly);

            IsActive = false;
        }

        /// <summary>
        /// Throws an exception if this <see cref="TextureBatcher"/> isn't currently active.
        /// </summary>
        private void ValidateBeginCalled()
        {
            if (!IsActive)
                throw new InvalidOperationException("Draw() must be called in between Begin() and End().");
        }

        /// <summary>
        /// Ensures that <see cref="batchItems"/> has at least the required capacity, but if the array
        /// is resized then the new capacity won't exceed <see cref="MaxBatchItemCapacity"/>.
        /// </summary>
        /// <param name="requiredCapacity">The required capacity for the <see cref="batchItems"/> array.</param>
        /// <returns>Whether the required capacity is met by the new capacity.</returns>
        private bool EnsureBatchListCapacity(int requiredCapacity)
        {
            int currentCapacity = batchItems.Length;
            if (currentCapacity == MaxBatchItemCapacity)
                return requiredCapacity <= currentCapacity;

            if (currentCapacity < requiredCapacity)
            {
                // We resize the batchItems array and fill the new elements with new TextureBatchItem
                // instances, so we never have a null inside the batchItems array.
                Array.Resize(ref batchItems, Math.Min(TrippyMath.GetNextCapacity(currentCapacity, requiredCapacity), (int)MaxBatchItemCapacity));
                for (int i = currentCapacity; i < batchItems.Length; i++)
                    batchItems[i] = new TextureBatchItem();
            }

            return requiredCapacity <= batchItems.Length;
        }

        /// <summary>
        /// Ensures that <see cref="vertexBuffer"/> and the <see cref="triangles"/> array have at least
        /// the required capacity, but if a resize is needed then the new capacity won't exceed
        /// <see cref="MaxBufferCapacity"/>.
        /// </summary>
        /// <param name="requiredCapacity">The required capacity for <see cref="vertexBuffer"/>.</param>
        /// <remarks>
        /// <see cref="triangles"/> will always be resized to have the same size as <see cref="vertexBuffer"/>.
        /// </remarks>
        [MemberNotNull(nameof(triangles))]
        private void EnsureBufferCapacity(int requiredCapacity)
        {
            uint currentCapacity = vertexBuffer.StorageLength;
            if (currentCapacity == MaxBufferCapacity)
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
// This warning complains that "triangles" is not being assigned, and that is must be a non-null value before this
// function exits (for the nullable code analyzer). However, the if condition ensures that triangles is not null.
                return;
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

            if (currentCapacity < requiredCapacity)
                vertexBuffer.RecreateStorage(Math.Min((uint)TrippyMath.GetNextCapacity((int)currentCapacity, requiredCapacity), MaxBufferCapacity));

            if (triangles == null || triangles.Length < vertexBuffer.StorageLength)
                triangles = new VertexColorTexture[vertexBuffer.StorageLength];
        }

        /// <summary>
        /// Gets a <see cref="TextureBatchItem"/> that's already in the <see cref="batchItems"/> array,
        /// in the next available position, then increments <see cref="batchItemCount"/>.
        /// </summary>
        /// <remarks>
        /// When a <see cref="TextureBatchItem"/> is returned by this method, it is already inside
        /// the <see cref="batchItems"/> array. To properly use this method, get an item and simply
        /// set it's value, without storing the item anywhere.
        /// </remarks>
        private TextureBatchItem GetNextBatchItem()
        {
            // We check that we have enough capacity for one more batch item.
            if (!EnsureBatchListCapacity(batchItemCount + 1))
            {
                // If we don't and the array can't be expanded any further, we try to flush.
                // Flushing can only occur before End() if BeginMode is OnTheFly or Immediate.
                // If BeginMode is one of these, we can also ensure that all the batch items share a texture.
                if (BeginMode == BatcherBeginMode.OnTheFly || BeginMode == BatcherBeginMode.Immediate)
                    Flush(true);
                else
                    throw new InvalidOperationException("Too many " + nameof(TextureBatcher) + " items. Try drawing less per Begin()-End() cycle or use OnTheFly or Immediate begin modes.");
            }

            // We are ensured the elements in batchItems are never null, so let's just return the next one.
            return batchItems[batchItemCount++];
        }

        /// <summary>
        /// Adds a <see cref="Texture2D"/> for drawing to the current batch, using raw vertices.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> to draw.</param>
        /// <param name="VertexTL">The top-left vertex.</param>
        /// <param name="VertexTR">The top-right vertex.</param>
        /// <param name="VertexBR">The botom-right vertex.</param>
        /// <param name="VertexBL">The bottom-left vertex.</param>
        /// <remarks>
        /// Even though the vertices are named by position, that positioning doesn't have to be followed.
        /// The vertices are drawn as two triangles composed as (TL, BR, TR) (TL, BL, BR).<para/>
        /// If sorting by depth is used, the depth for these vertices will be the Z coordinate of the
        /// top-left vertex.
        /// </remarks>
        public void DrawRaw(Texture2D texture, in VertexColorTexture VertexTL, in VertexColorTexture VertexTR,
            in VertexColorTexture VertexBR, in VertexColorTexture VertexBL)
        {
            StartDraw(texture);

            TextureBatchItem item = GetNextBatchItem();
            item.VertexTL = VertexTL;
            item.VertexTR = VertexTR;
            item.VertexBR = VertexBR;
            item.VertexBL = VertexBL;
            item.Texture = texture;

            EndDraw(item);
        }

        /// <summary>
        /// Adds a <see cref="Texture2D"/> for drawing to the current batch, using raw vertices
        /// whose position first gets transformed by a matrix.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> to draw.</param>
        /// <param name="VertexTL">The top-left vertex.</param>
        /// <param name="VertexTR">The top-right vertex.</param>
        /// <param name="VertexBR">The botom-right vertex.</param>
        /// <param name="VertexBL">The bottom-left vertex.</param>
        /// <param name="matrix">The matrix for transforming the vertex positions.</param>
        public void DrawRaw(Texture2D texture, in VertexColorTexture VertexTL, in VertexColorTexture VertexTR,
            in VertexColorTexture VertexBR, in VertexColorTexture VertexBL, in Matrix4x4 matrix)
        {
            StartDraw(texture);

            TextureBatchItem item = GetNextBatchItem();
            item.VertexTL = new VertexColorTexture(Vector3.Transform(VertexTL.Position, matrix), VertexTL.Color, VertexTL.TexCoords);
            item.VertexTR = new VertexColorTexture(Vector3.Transform(VertexTR.Position, matrix), VertexTR.Color, VertexTR.TexCoords);
            item.VertexBR = new VertexColorTexture(Vector3.Transform(VertexBR.Position, matrix), VertexBR.Color, VertexBR.TexCoords);
            item.VertexBL = new VertexColorTexture(Vector3.Transform(VertexBL.Position, matrix), VertexBL.Color, VertexBL.TexCoords);
            item.Texture = texture;

            EndDraw(item);
        }

        /// <summary>
        /// Adds a <see cref="Texture2D"/> for drawing to the current batch.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> to draw.</param>
        /// <param name="transform">The location (Scale*Rotation*Translation) at which to draw the texture.</param>
        /// <param name="source">The area of the texture to draw (or null to draw the whole texture).</param>
        /// <param name="color">The color with which to draw the texture.</param>        
        /// <param name="depth">The depth at which to draw the texture.</param>
        public void Draw(Texture2D texture, in Matrix3x2 transform, Rectangle? source, Color4b color, float depth = 0)
        {
            StartDraw(texture);

            TextureBatchItem item = GetNextBatchItem();
            item.SetValue(texture, transform, source ?? new Rectangle(0, 0, (int)texture.Width, (int)texture.Height), color, depth);

            EndDraw(item);
        }

        /// <summary>
        /// Adds a <see cref="Texture2D"/> for drawing to the current batch.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> to draw.</param>
        /// <param name="transform">The location (Scale*Rotation*Translation) at which to draw the texture.</param>
        /// <param name="source">The area of the texture to draw (or null to draw the whole texture).</param>
        /// <param name="color">The color with which to draw the texture.</param>
        /// <param name="origin">The origin for rotation and scaling in pixel coordinates.</param>
        /// <param name="depth">The depth at which to draw the texture.</param>
        public void Draw(Texture2D texture, in Matrix3x2 transform, Rectangle? source, Color4b color, Vector2 origin, float depth = 0)
        {
            StartDraw(texture);

            TextureBatchItem item = GetNextBatchItem();
            item.SetValue(texture, transform, source ?? new Rectangle(0, 0, (int)texture.Width, (int)texture.Height), color, origin, depth);

            EndDraw(item);
        }

        /// <summary>
        /// Adds a <see cref="Texture2D"/> for drawing to the current batch.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> to draw.</param>
        /// <param name="position">The position at which to draw the texture.</param>
        /// <param name="source">The area of the texture to draw (or null to draw the whole texture).</param>
        /// <param name="color">The color with which to draw the texture.</param>
        /// <param name="scale">The scale value that multiplies the size of the drawn texture.</param>
        /// <param name="rotation">The rotation to draw the texture with, measured in radians.</param>
        /// <param name="origin">The origin for rotation and scaling in pixel coordinates.</param>
        /// <param name="depth">The depth at which to draw the texture.</param>
        public void Draw(Texture2D texture, Vector2 position, Rectangle? source, Color4b color, Vector2 scale,
            float rotation, Vector2 origin = default, float depth = 0)
        {
            StartDraw(texture);

            TextureBatchItem item = GetNextBatchItem();
            if (rotation == 0)
                item.SetValue(texture, position, source, color, scale, origin, depth);
            else
                item.SetValue(texture, position, source, color, scale, rotation, origin, depth);

            EndDraw(item);
        }

        /// <summary>
        /// Adds a <see cref="Texture2D"/> for drawing to the current batch.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> to draw.</param>
        /// <param name="position">The position at which to draw the texture.</param>
        /// <param name="source">The area of the texture to draw (or null to draw the whole texture).</param>
        /// <param name="color">The color with which to draw the texture.</param>
        /// <param name="scale">The scale value that multiplies the size of the drawn texture.</param>
        /// <param name="rotation">The rotation to draw the texture with, measured in radians.</param>
        /// <param name="origin">The origin for rotation and scaling in pixel coordinates.</param>
        /// <param name="depth">The depth at which to draw the texture.</param>
        public void Draw(Texture2D texture, Vector2 position, Rectangle? source, Color4b color, float scale,
            float rotation, Vector2 origin = default, float depth = 0)
        {
            Draw(texture, position, source, color, new Vector2(scale, scale), rotation, origin, depth);
        }

        /// <summary>
        /// Adds a <see cref="Texture2D"/> for drawing to the current batch.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> to draw.</param>
        /// <param name="position">The position at which to draw the texture.</param>
        /// <param name="source">The area of the texture to draw (or null to draw the whole texture).</param>
        /// <param name="color">The color with which to draw the texture.</param>
        /// <param name="depth">The depth at which to draw the texture.</param>
        public void Draw(Texture2D texture, Vector2 position, Rectangle? source, Color4b color, float depth = 0)
        {
            StartDraw(texture);

            TextureBatchItem item = GetNextBatchItem();
            item.SetValue(texture, position, source ?? new Rectangle(0, 0, (int)texture.Width, (int)texture.Height), color, depth);

            EndDraw(item);
        }

        /// <summary>
        /// Adds a <see cref="Texture2D"/> for drawing to the current batch.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> to draw.</param>
        /// <param name="position">The position at which to draw the texture.</param>
        /// <param name="source">The area of the texture to draw (or null to draw the whole texture).</param>
        /// <param name="depth">The depth at which to draw the texture.</param>
        public void Draw(Texture2D texture, Vector2 position, Rectangle? source = null, float depth = 0)
        {
            StartDraw(texture);

            TextureBatchItem item = GetNextBatchItem();
            item.SetValue(texture, position, source ?? new Rectangle(0, 0, (int)texture.Width, (int)texture.Height), Color4b.White, depth);

            EndDraw(item);
        }

        /// <summary>
        /// Adds a <see cref="Texture2D"/> for drawing to the current batch.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> to draw.</param>
        /// <param name="position">The position at which to draw the texture.</param>
        /// <param name="color">The color with which to draw the texture.</param>
        /// <param name="depth">The depth at which to draw the texture.</param>
        public void Draw(Texture2D texture, Vector2 position, Color4b color, float depth = 0)
        {
            StartDraw(texture);

            TextureBatchItem item = GetNextBatchItem();
            item.SetValue(texture, position, new Rectangle(0, 0, (int)texture.Width, (int)texture.Height), color, depth);

            EndDraw(item);
        }

        /// <summary>
        /// Adds a <see cref="Texture2D"/> for drawing to the current batch.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> to draw.</param>
        /// <param name="destination">The destination rectangle at which the texture should be drawn.</param>
        /// <param name="source">The area of the texture to draw (or null to draw the whole texture).</param>
        /// <param name="color">The color with which to draw the texture.</param>
        /// <param name="depth">The depth at which to draw the texture.</param>
        public void Draw(Texture2D texture, RectangleF destination, Rectangle? source, Color4b color, float depth = 0)
        {
            StartDraw(texture);

            TextureBatchItem item = GetNextBatchItem();
            item.SetValue(texture, destination, source ?? new Rectangle(0, 0, (int)texture.Width, (int)texture.Height), color, depth);

            EndDraw(item);
        }

        /// <summary>
        /// Adds a <see cref="Texture2D"/> for drawing to the current batch.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> to draw.</param>
        /// <param name="destination">The destination rectangle at which the texture should be drawn.</param>
        /// <param name="color">The color with which to draw the texture.</param>
        /// <param name="depth">The depth at which to draw the texture.</param>
        public void Draw(Texture2D texture, RectangleF destination, Color4b color, float depth = 0)
        {
            Draw(texture, destination, null, color, depth);
        }

        /// <summary>
        /// Adds a <see cref="Texture2D"/> for drawing to the current batch.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> to draw.</param>
        /// <param name="destination">The destination rectangle at which the texture should be drawn.</param>
        /// <param name="depth">The depth at which to draw the texture.</param>
        public void Draw(Texture2D texture, RectangleF destination, float depth = 0)
        {
            Draw(texture, destination, null, Color4b.White, depth);
        }

        /// <summary>
        /// Adds multiple textures forming a string of text to the current batch.
        /// </summary>
        /// <param name="font">The <see cref="TextureFont"/> to draw the text with.</param>
        /// <param name="text">The string of text to draw.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="color">The color with which to draw the text.</param>
        /// <param name="scale">The scale value that multiplies the size of the drawn text.</param>
        /// <param name="rotation">The rotation with which to draw the text, measured in radians.</param>
        /// <param name="origin">The origin for rotation and scaling in pixel coordinates.</param>
        /// <param name="depth">The depth at which to draw the string of text.</param>
        public void DrawString(TextureFont font, ReadOnlySpan<char> text, Vector2 position, Color4b color, Vector2 scale, float rotation, Vector2 origin, float depth = 0)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            if (text.IsEmpty)
                return;

            StartDraw(font.Texture);

            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);

            Vector2 m = origin * scale;
            position -= new Vector2(cos * m.X - sin * m.Y, sin * m.X + cos * m.Y);

            Vector2 lineAdvance = font.LineAdvance * scale.Y * new Vector2(-sin, cos);
            Vector2 charAdvance = new Vector2(cos, sin) * scale.X;

            Vector2 linePosition = position + font.LineGap * scale.Y * new Vector2(-sin, cos);
            Vector2 penPosition = linePosition;

            bool isFirstInLine = true;
            char previousChar = default;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == TextureFont.NewlineIndicator)
                {
                    linePosition += lineAdvance;
                    penPosition = linePosition;
                    isFirstInLine = true;
                    continue;
                }

                Vector2 kroff = default;
                if (isFirstInLine)
                    isFirstInLine = false;
                else
                {
                    Vector2 koff = font.GetKerning(previousChar, c) * scale;
                    penPosition += new Vector2(cos, sin) * koff.X;
                    kroff = new Vector2(-sin, cos) * koff.Y;
                }

                Rectangle source = font.GetSource(c);
                if (source.Width != 0)
                {
                    TextureBatchItem batchItem = GetNextBatchItem();
                    Vector2 renderOffset = font.GetRenderOffset(c) * scale;
                    renderOffset = new Vector2(cos * renderOffset.X - sin * renderOffset.Y, sin * renderOffset.X + cos * renderOffset.Y);
                    batchItem.SetValue(font.Texture, penPosition + kroff + renderOffset, source, color, scale, sin, cos, depth);
                    SetItemSortKey(batchItem);
                }

                penPosition += font.GetAdvance(c) * charAdvance;
                previousChar = c;
            }

            FlushIfNeeded();
        }

        /// <summary>
        /// Adds multiple textures forming a string of text to the current batch.
        /// </summary>
        /// <param name="font">The <see cref="TextureFont"/> to draw the text with.</param>
        /// <param name="text">The string of text to draw.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="color">The color with which to draw the text.</param>
        /// <param name="scale">The scale value that multiplies the size of the drawn text.</param>
        /// <param name="rotation">The rotation with which to draw the text, measured in radians.</param>
        /// <param name="origin">The origin for rotation and scaling in pixel coordinates.</param>
        /// <param name="depth">The depth at which to draw the string of text.</param>
        public void DrawString(TextureFont font, ReadOnlySpan<char> text, Vector2 position, Color4b color, float scale, float rotation, Vector2 origin, float depth = 0)
        {
            DrawString(font, text, position, color, new Vector2(scale, scale), rotation, origin, depth);
        }

        /// <summary>
        /// Adds multiple textures forming a string of text to the current batch.
        /// </summary>
        /// <param name="font">The <see cref="TextureFont"/> to draw the text with.</param>
        /// <param name="text">The string of text to draw.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="color">The color with which to draw the text.</param>
        /// <param name="scale">The scale value that multiplies the size of the drawn text.</param>
        /// <param name="origin">The origin for rotation and scaling in pixel coordinates.</param>
        /// <param name="depth">The depth at which to draw the string of text.</param>
        public void DrawString(TextureFont font, ReadOnlySpan<char> text, Vector2 position, Color4b color, Vector2 scale, Vector2 origin, float depth = 0)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            if (text.IsEmpty)
                return;

            StartDraw(font.Texture);

            float lineAdvance = font.LineAdvance * scale.Y;

            position -= origin * scale;

            float y = position.Y + font.LineGap * scale.Y;
            float x = position.X;

            bool isFirstInLine = true;
            char previousChar = default;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == TextureFont.NewlineIndicator)
                {
                    x = position.X;
                    y += lineAdvance;
                    isFirstInLine = true;
                    continue;
                }

                Vector2 koff = default;
                if (isFirstInLine)
                    isFirstInLine = false;
                else
                {
                    koff = font.GetKerning(previousChar, c) * scale;
                    x += koff.X;
                }

                Rectangle source = font.GetSource(c);
                if (source.Width != 0)
                {
                    TextureBatchItem batchItem = GetNextBatchItem();
                    batchItem.SetValue(font.Texture, new Vector2(x, y + koff.Y) + font.GetRenderOffset(c) * scale, source, color, scale, depth);
                    SetItemSortKey(batchItem);
                }

                x += font.GetAdvance(c) * scale.X;
                previousChar = c;
            }

            FlushIfNeeded();
        }

        /// <summary>
        /// Adds multiple textures forming a string of text to the current batch.
        /// </summary>
        /// <param name="font">The <see cref="TextureFont"/> to draw the text with.</param>
        /// <param name="text">The string of text to draw.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="color">The color with which to draw the text.</param>
        /// <param name="scale">The scale value that multiplies the size of the drawn text.</param>
        /// <param name="origin">The origin for rotation and scaling in pixel coordinates.</param>
        /// <param name="depth">The depth at which to draw the string of text.</param>
        public void DrawString(TextureFont font, ReadOnlySpan<char> text, Vector2 position, Color4b color, float scale, Vector2 origin, float depth = 0)
        {
            DrawString(font, text, position, color, new Vector2(scale, scale), origin, depth);
        }

        /// <summary>
        /// Adds multiple textures forming a string of text to the current batch.
        /// </summary>
        /// <param name="font">The <see cref="TextureFont"/> to draw the text with.</param>
        /// <param name="text">The string of text to draw.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="color">The color with which to draw the text.</param>
        /// <param name="depth">The depth at which to draw the string of text.</param>
        public void DrawString(TextureFont font, ReadOnlySpan<char> text, Vector2 position, Color4b color, float depth = 0)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            if (text.IsEmpty)
                return;

            StartDraw(font.Texture);

            float y = position.Y + font.LineGap;
            float x = position.X;

            bool isFirstInLine = true;
            char previousChar = default;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == TextureFont.NewlineIndicator)
                {
                    x = position.X;
                    y += font.LineAdvance;
                    isFirstInLine = true;
                    continue;
                }

                Vector2 koff = default;
                if (isFirstInLine)
                    isFirstInLine = false;
                else
                {
                    koff = font.GetKerning(previousChar, c);
                    x += koff.X;
                }

                Rectangle source = font.GetSource(c);
                if (source.Width != 0)
                {
                    TextureBatchItem batchItem = GetNextBatchItem();
                    batchItem.SetValue(font.Texture, new Vector2(x, y + koff.Y) + font.GetRenderOffset(c), source, color, depth);
                    SetItemSortKey(batchItem);
                }

                x += font.GetAdvance(c);
                previousChar = c;
            }

            FlushIfNeeded();
        }

        /// <summary>
        /// Adds multiple textures forming a string of text to the current batch.
        /// </summary>
        /// <param name="font">The <see cref="TextureFont"/> to draw the text with.</param>
        /// <param name="text">The string of text to draw.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="depth">The depth at which to draw the string of text.</param>
        public void DrawString(TextureFont font, ReadOnlySpan<char> text, Vector2 position, float depth = 0)
        {
            DrawString(font, text, position, Color4b.White, depth);
        }

        /// <summary>
        /// Adds multiple textures forming a string of text to the current batch.
        /// </summary>
        /// <param name="font">The <see cref="TextureFont"/> to draw the text with.</param>
        /// <param name="text">The string of text to draw.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="color">The color with which to draw the text.</param>
        /// <param name="scale">The scale value that multiplies the size of the drawn text.</param>
        /// <param name="rotation">The rotation with which to draw the text, measured in radians.</param>
        /// <param name="origin">The origin for rotation and scaling in pixel coordinates.</param>
        /// <param name="depth">The depth at which to draw the string of text.</param>
        public void DrawString(TextureFont font, StringBuilder text, Vector2 position, Color4b color, Vector2 scale, float rotation, Vector2 origin, float depth = 0)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (text.Length == 0)
                return;

            StartDraw(font.Texture);

            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);

            Vector2 m = origin * scale;
            position -= new Vector2(cos * m.X - sin * m.Y, sin * m.X + cos * m.Y);

            Vector2 lineAdvance = font.LineAdvance * scale.Y * new Vector2(-sin, cos);
            Vector2 charAdvance = new Vector2(cos, sin) * scale.X;

            Vector2 linePosition = position + font.LineGap * scale.Y * new Vector2(-sin, cos);
            Vector2 penPosition = linePosition;

            bool isFirstInLine = true;
            char previousChar = default;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == TextureFont.NewlineIndicator)
                {
                    linePosition += lineAdvance;
                    penPosition = linePosition;
                    isFirstInLine = true;
                    continue;
                }

                Vector2 kroff = default;
                if (isFirstInLine)
                    isFirstInLine = false;
                else
                {
                    Vector2 koff = font.GetKerning(previousChar, c) * scale;
                    penPosition += new Vector2(cos, sin) * koff.X;
                    kroff = new Vector2(-sin, cos) * koff.Y;
                }

                Rectangle source = font.GetSource(c);
                if (source.Width != 0)
                {
                    TextureBatchItem batchItem = GetNextBatchItem();
                    Vector2 renderOffset = font.GetRenderOffset(c) * scale;
                    renderOffset = new Vector2(cos * renderOffset.X - sin * renderOffset.Y, sin * renderOffset.X + cos * renderOffset.Y);
                    batchItem.SetValue(font.Texture, penPosition + kroff + renderOffset, source, color, scale, sin, cos, depth);
                    SetItemSortKey(batchItem);
                }

                penPosition += font.GetAdvance(c) * charAdvance;
                previousChar = c;
            }

            FlushIfNeeded();
        }

        /// <summary>
        /// Adds multiple textures forming a string of text to the current batch.
        /// </summary>
        /// <param name="font">The <see cref="TextureFont"/> to draw the text with.</param>
        /// <param name="text">The string of text to draw.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="color">The color with which to draw the text.</param>
        /// <param name="scale">The scale value that multiplies the size of the drawn text.</param>
        /// <param name="rotation">The rotation with which to draw the text, measured in radians.</param>
        /// <param name="origin">The origin for rotation and scaling in pixel coordinates.</param>
        /// <param name="depth">The depth at which to draw the string of text.</param>
        public void DrawString(TextureFont font, StringBuilder text, Vector2 position, Color4b color, float scale, float rotation, Vector2 origin, float depth = 0)
        {
            DrawString(font, text, position, color, new Vector2(scale, scale), rotation, origin, depth);
        }

        /// <summary>
        /// Adds multiple textures forming a string of text to the current batch.
        /// </summary>
        /// <param name="font">The <see cref="TextureFont"/> to draw the text with.</param>
        /// <param name="text">The string of text to draw.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="color">The color with which to draw the text.</param>
        /// <param name="scale">The scale value that multiplies the size of the drawn text.</param>
        /// <param name="origin">The origin for rotation and scaling in pixel coordinates.</param>
        /// <param name="depth">The depth at which to draw the string of text.</param>
        public void DrawString(TextureFont font, StringBuilder text, Vector2 position, Color4b color, Vector2 scale, Vector2 origin, float depth = 0)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (text.Length == 0)
                return;

            StartDraw(font.Texture);

            float lineAdvance = font.LineAdvance * scale.Y;

            position -= origin * scale;

            float y = position.Y + font.LineGap * scale.Y;
            float x = position.X;

            bool isFirstInLine = true;
            char previousChar = default;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == TextureFont.NewlineIndicator)
                {
                    x = position.X;
                    y += lineAdvance;
                    isFirstInLine = true;
                    continue;
                }

                Vector2 koff = default;
                if (isFirstInLine)
                    isFirstInLine = false;
                else
                {
                    koff = font.GetKerning(previousChar, c) * scale;
                    x += koff.X;
                }

                Rectangle source = font.GetSource(c);
                if (source.Width != 0)
                {
                    TextureBatchItem batchItem = GetNextBatchItem();
                    batchItem.SetValue(font.Texture, new Vector2(x, y + koff.Y) + font.GetRenderOffset(c) * scale, source, color, scale, depth);
                    SetItemSortKey(batchItem);
                }

                x += font.GetAdvance(c) * scale.X;
                previousChar = c;
            }

            FlushIfNeeded();
        }

        /// <summary>
        /// Adds multiple textures forming a string of text to the current batch.
        /// </summary>
        /// <param name="font">The <see cref="TextureFont"/> to draw the text with.</param>
        /// <param name="text">The string of text to draw.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="color">The color with which to draw the text.</param>
        /// <param name="scale">The scale value that multiplies the size of the drawn text.</param>
        /// <param name="origin">The origin for rotation and scaling in pixel coordinates.</param>
        /// <param name="depth">The depth at which to draw the string of text.</param>
        public void DrawString(TextureFont font, StringBuilder text, Vector2 position, Color4b color, float scale, Vector2 origin, float depth = 0)
        {
            DrawString(font, text, position, color, new Vector2(scale, scale), origin, depth);
        }

        /// <summary>
        /// Adds multiple textures forming a string of text to the current batch.
        /// </summary>
        /// <param name="font">The <see cref="TextureFont"/> to draw the text with.</param>
        /// <param name="text">The string of text to draw.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="color">The color with which to draw the text.</param>
        /// <param name="depth">The depth at which to draw the string of text.</param>
        public void DrawString(TextureFont font, StringBuilder text, Vector2 position, Color4b color, float depth = 0)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (text.Length == 0)
                return;

            StartDraw(font.Texture);

            float y = position.Y + font.LineGap;
            float x = position.X;

            bool isFirstInLine = true;
            char previousChar = default;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == TextureFont.NewlineIndicator)
                {
                    x = position.X;
                    y += font.LineAdvance;
                    isFirstInLine = true;
                    continue;
                }

                Vector2 koff = default;
                if (isFirstInLine)
                    isFirstInLine = false;
                else
                {
                    koff = font.GetKerning(previousChar, c);
                    x += koff.X;
                }

                Rectangle source = font.GetSource(c);
                if (source.Width != 0)
                {
                    TextureBatchItem batchItem = GetNextBatchItem();
                    batchItem.SetValue(font.Texture, new Vector2(x, y + koff.Y) + font.GetRenderOffset(c), source, color, depth);
                    SetItemSortKey(batchItem);
                }

                x += font.GetAdvance(c);
                previousChar = c;
            }

            FlushIfNeeded();
        }

        /// <summary>
        /// Adds multiple textures forming a string of text to the current batch.
        /// </summary>
        /// <param name="font">The <see cref="TextureFont"/> to draw the text with.</param>
        /// <param name="text">The string of text to draw.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="depth">The depth at which to draw the string of text.</param>
        public void DrawString(TextureFont font, StringBuilder text, Vector2 position, float depth = 0)
        {
            DrawString(font, text, position, Color4b.White, depth);
        }

        /// <summary>
        /// Performs any operations that need to be done before more batch items can be added.
        /// This should be called at the start of any Draw() method.
        /// </summary>
        /// <param name="texture">The texture of the next batch item/s to be added.</param>
        private void StartDraw(Texture2D texture)
        {
            // We check that begin was called and if not, throw an exception.
            ValidateBeginCalled();

            // We ensure the texture isn't null
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            // If BeginMode is OnTheFly, before adding more batch items we check whether we should flush.
            if (BeginMode == BatcherBeginMode.OnTheFly && batchItemCount != 0)
            {
                // We should flush if the texture that's being added isn't the same as the texture
                // on the items already in batchItems.
                if (batchItems[0].Texture != texture)
                    Flush(true);
            }
        }

        /// <summary>
        /// Performs any operations that need to be done after a batch item was added.
        /// This should be called at the end of any Draw() method that only adds a single item.
        /// </summary>
        /// <param name="item">The <see cref="TextureBatchItem"/> that was added.</param>
        private void EndDraw(TextureBatchItem item)
        {
            if (!FlushIfNeeded())
                SetItemSortKey(item);
        }

        /// <summary>
        /// Sets the <see cref="TextureBatchItem.SortValue"/> of the given item based on
        /// the current <see cref="BeginMode"/>.
        /// </summary>
        private void SetItemSortKey(TextureBatchItem item)
        {
            item.SortValue = BeginMode switch
            {
                BatcherBeginMode.SortByTexture => item.Texture!.Handle,
                BatcherBeginMode.SortFrontToBack => item.VertexTL.Position.Z,
                BatcherBeginMode.SortBackToFront => -item.VertexTL.Position.Z,
                _ => 0
            };
        }

        /// <summary>
        /// Checks whether the <see cref="TextureBatcher"/> should be flushed after adding more batch
        /// items based on the current <see cref="BeginMode"/> and if so, flushes.
        /// </summary>
        /// <returns>Whether the <see cref="TextureBatcher"/> was flushed.</returns>
        /// <remarks>This should always be called after adding batch items of the same texture.</remarks>
        private bool FlushIfNeeded()
        {
            if (BeginMode == BatcherBeginMode.Immediate)
            {
                Flush(true);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Renders all the items in <see cref="batchItems"/>.
        /// </summary>
        /// <param name="sameTextureEnsured">
        /// Whether it is ensured that all the <see cref="TextureBatchItem"/> in <see cref="batchItems"/>
        /// have the same textures.
        /// </param>
        /// <remarks>
        /// This function assumes <see cref="Begin(BatcherBeginMode)"/> was succesfully called and
        /// that this object isn't disposed.
        /// </remarks>
        private void Flush(bool sameTextureEnsured)
        {
            if (batchItemCount == 0)
                return;

            // In order to know whether we need to sort first, we can AND the BeginMode with 1.
            if ((BeginMode & (BatcherBeginMode)1) == (BatcherBeginMode)1)
                Array.Sort(batchItems, 0, batchItemCount);

            // We set the vertex buffer and shader program onto the GraphicsDevice so we can use them.
            GraphicsDevice.VertexArray = vertexBuffer;
            GraphicsDevice.ShaderProgram = ShaderProgram;

            // We now need to draw all the textures, in the order in which they are in the array.
            // Since we can only draw with one texture per draw call, we'll have split the items
            // into batches that use the same texture.

            // We do this in the outer while loop, which keeps going while we have more items to draw.

            int itemStartIndex = 0;
            while (itemStartIndex < batchItemCount)
            {
                // We get the texture to draw with for this batch (the texture of next item to draw).
                Texture2D currentTexture = batchItems[itemStartIndex].Texture!;

                // We search for the end of this batch. That is, up to what item can we draw with this texture.
                int itemEndIndex = sameTextureEnsured ? batchItemCount : FindDifferentTexture(currentTexture, itemStartIndex + 1);
                // We make a Span<TextureBatchItem> containing all the items to draw in this batch.
                Span<TextureBatchItem> items = batchItems.AsSpan(itemStartIndex, itemEndIndex - itemStartIndex);

                // The next cycle of this loop should start drawing items where this cycle left off.
                itemStartIndex = itemEndIndex;

                // We ensure that the buffers have enough capacity, or as much as they can have.
                EnsureBufferCapacity(items.Length * 6);
                // We calculate the maximum amount of batch items that we'll be able to draw per draw call.
                int batchItemsPerDrawCall = (int)vertexBuffer.StorageLength / 6;

                // We set the texture onto the shader (if the uniform is present).
                if (!TextureUniform.IsEmpty)
                    TextureUniform.SetValueTexture(currentTexture);

                // Depending on how the constants are set up, we might be able to draw all the items
                // in a single call, or we might need to further split them up if they can't all fit
                // together into the buffer.
                for (int startIndex = 0; startIndex < items.Length; startIndex += batchItemsPerDrawCall)
                {
                    // We calculate up to which item we can draw in this draw call.
                    int endIndex = Math.Min(startIndex + batchItemsPerDrawCall, items.Length);

                    // We make the triangles for all the items and add all those vertices to
                    // the triangles array. Each item uses two triangles, or six vertices.
                    int triangleIndex = 0;
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        TextureBatchItem item = items[i];
                        triangles[triangleIndex++] = item.VertexTL;
                        triangles[triangleIndex++] = item.VertexBR;
                        triangles[triangleIndex++] = item.VertexTR;

                        triangles[triangleIndex++] = item.VertexTL;
                        triangles[triangleIndex++] = item.VertexBL;
                        triangles[triangleIndex++] = item.VertexBR;
                    }

                    // We copy the vertices over to the vertexBuffer and draw them.
                    vertexBuffer.DataSubset.SetData(triangles.AsSpan(0, triangleIndex));
                    GraphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, (uint)triangleIndex);
                }
            }

            // We reset batchItemCount to 0.
            batchItemCount = 0;
        }

        /// <summary>
        /// Returns the index of the first <see cref="TextureBatchItem"/> in <see cref="batchItems"/>
        /// that has a different texture than the one specified.
        /// </summary>
        /// <param name="currentTexture">Skips all items with this texture.</param>
        /// <param name="startIndex">The index in the array at which to start looking.</param>
        private int FindDifferentTexture(Texture2D currentTexture, int startIndex)
        {
            while (startIndex < batchItemCount && batchItems[startIndex].Texture == currentTexture)
                startIndex++;
            return startIndex;
        }

        /// <summary>
        /// Disposes the <see cref="GraphicsResource"/>-s used by this <see cref="TextureBatcher"/>.
        /// </summary>
        public void Dispose()
        {
            vertexBuffer.Dispose();
        }
    }
}