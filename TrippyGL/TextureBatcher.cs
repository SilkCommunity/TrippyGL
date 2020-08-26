using Silk.NET.OpenGL;
using System;
using System.Drawing;
using System.Numerics;

namespace TrippyGL
{
    public sealed class TextureBatcher : IDisposable
    {
        private const uint InitialBatchItemsCapacity = 64;
        private const uint MaxBatchItemCapacity = 4096;
        private const uint InitialBufferCapacity = InitialBatchItemsCapacity * 3;
        private const uint MaxBufferCapacity = MaxBatchItemCapacity * 6;

        private readonly PrimitiveBatcher<VertexColorTexture> primitiveBatcher;

        private readonly VertexBuffer<VertexColorTexture> vertexBuffer;

        private TextureBatchItem[] batchItems;
        private int batchItemCount;

        public bool IsActive { get; private set; }
        public BatcherBeginMode BeginMode { get; private set; }

        public ShaderProgram ShaderProgram { get; private set; }
        public ShaderUniform TextureUniform { get; private set; }

        public bool IsDisposed => vertexBuffer.IsDisposed;

        public TextureBatcher(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            primitiveBatcher = new PrimitiveBatcher<VertexColorTexture>(PrimitiveBatcher<int>.InitialCapacity, 0);
            batchItems = new TextureBatchItem[InitialBatchItemsCapacity];
            batchItemCount = 0;
            IsActive = false;
            vertexBuffer = new VertexBuffer<VertexColorTexture>(graphicsDevice, InitialBufferCapacity, BufferUsageARB.StreamDraw);


        }

        public void SetShaderProgram(SimpleShaderProgram simpleProgram)
        {
            if (simpleProgram == null)
                throw new ArgumentNullException(nameof(simpleProgram));

            SetShaderProgram(simpleProgram, simpleProgram.sampUniform);
        }

        public void SetShaderProgram(ShaderProgram shaderProgram, ShaderUniform textureUniform)
        {
            if (IsActive)
                throw new InvalidOperationException(nameof(ShaderProgram) + " cant be changed while the " + nameof(TextureBatcher) + " is active.");

            if (shaderProgram == null)
                throw new ArgumentNullException(nameof(shaderProgram));

            if (textureUniform.IsEmpty)
                throw new ArgumentNullException(nameof(textureUniform));

            if (!TrippyUtils.IsUniformSampler2DType(textureUniform.UniformType))
                throw new ArgumentException("The provided " + nameof(ShaderUniform) + " must be a Sampler2D type.", nameof(textureUniform));

            if (textureUniform.OwnerProgram != shaderProgram)
                throw new ArgumentException(nameof(textureUniform) + " must belong to the provided " + nameof(ShaderProgram), nameof(textureUniform));

            ActiveVertexAttrib attrib;
            if (!shaderProgram.TryFindAttributeByLocation(0, out attrib) || attrib.AttribType != AttributeType.FloatVec3)
                throw new ArgumentException("The shader program's attribute at location 0 must be of type FloatVec3 (used for position).", nameof(shaderProgram));

            if (shaderProgram.TryFindAttributeByLocation(1, out attrib) && attrib.AttribType != AttributeType.FloatVec4)
                throw new ArgumentException("The shader program's attribute at location 1 must be of type FloatVec4 (used for color).", nameof(shaderProgram));

            if (shaderProgram.TryFindAttributeByLocation(2, out attrib) && attrib.AttribType != AttributeType.FloatVec2)
                throw new ArgumentException("The shader program's attribute at location 2 must be of type FloatVec2 (used for texture coordinates).", nameof(shaderProgram));

            ShaderProgram = shaderProgram;
            TextureUniform = textureUniform;
        }

        public void Begin(BatcherBeginMode beginMode = BatcherBeginMode.Deferred)
        {
            if (vertexBuffer.IsDisposed)
                throw new ObjectDisposedException(nameof(TextureBatcher));

            if (ShaderProgram == null)
                throw new InvalidOperationException("A " + nameof(ShaderProgram) + " must be specified (via " + nameof(SetShaderProgram) + "()) before using Begin().");

            if (IsActive)
                throw new InvalidOperationException("This " + nameof(TextureBatcher) + " has already begun.");

            BeginMode = beginMode;
            IsActive = true;
        }

        private void ValidateBeginCalled()
        {
            if (!IsActive)
                throw new InvalidOperationException("Draw() must be called in between Begin() and End().");
        }

        private static void ValidateTexture(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
        }

        private bool EnsureBatchListCapacity(int requiredCapacity)
        {
            // Returns whether the required capacity can be met.
            int currentCapacity = batchItems.Length;
            if (currentCapacity == MaxBatchItemCapacity)
                return requiredCapacity <= currentCapacity;

            if (currentCapacity < requiredCapacity)
                Array.Resize(ref batchItems, Math.Min(TrippyMath.GetNextCapacity(currentCapacity, requiredCapacity), (int)MaxBatchItemCapacity));
            return requiredCapacity <= currentCapacity;
        }

        private void EnsureBufferCapacity(int requiredCapacity)
        {
            uint currentCapacity = vertexBuffer.StorageLength;
            if (currentCapacity == MaxBufferCapacity)
                return;

            if (currentCapacity < requiredCapacity)
                vertexBuffer.RecreateStorage(Math.Min((uint)TrippyMath.GetNextCapacity((int)currentCapacity, requiredCapacity), MaxBufferCapacity));
        }

        private TextureBatchItem GetNextBatchItem()
        {
            if (!EnsureBatchListCapacity(batchItemCount + 1))
            {
                if (BeginMode == BatcherBeginMode.OnTheFly || BeginMode == BatcherBeginMode.Immediate)
                    Flush();
                else
                    throw new InvalidOperationException("Too many " + nameof(TextureBatcher) + " items. Try drawing less per Begin()-End() cycle or use OnTheFly or Immediate begin modes.");
            }

            TextureBatchItem item;
            if (batchItems[batchItemCount] == null)
            {
                item = new TextureBatchItem();
                batchItems[batchItemCount] = item;
            }
            else
                item = batchItems[batchItemCount];
            batchItemCount++;
            return item;
        }

        public void Draw(Texture2D texture, Vector2 position, Rectangle? source, Color4b color, Vector2 scale,
            float rotation = 0, Vector2 origin = default, float depth = 0)
        {
            ValidateBeginCalled();
            ValidateTexture(texture);

            if (BeginMode == BatcherBeginMode.OnTheFly && batchItemCount != 0)
            {
                if (batchItems[batchItemCount - 1].Texture != texture)
                    Flush();
            }

            TextureBatchItem item = GetNextBatchItem();

            if (rotation == 0)
                item.SetValue(texture, position, source, color, scale, origin, depth);
            else
                item.SetValue(texture, position, source, color, scale, rotation, origin, depth);

            if (BeginMode == BatcherBeginMode.Immediate)
                Flush();
            else
            {
                item.SortValue = BeginMode switch
                {
                    BatcherBeginMode.SortByTexture => texture.Handle,
                    BatcherBeginMode.SortFrontToBack => depth,
                    BatcherBeginMode.SortBackToFront => -depth,
                    _ => 0
                };
            }
        }

        private void Flush()
        {
            // We ensure there is at least one batch item.
            if (batchItemCount == 0)
                return;

            if ((BeginMode & (BatcherBeginMode)1) == (BatcherBeginMode)1)
                Array.Sort(batchItems, 0, batchItemCount);

            GraphicsDevice graphicsDevice = ShaderProgram.GraphicsDevice;

            int itemStartIndex = 0;
            while (itemStartIndex < batchItemCount)
            {
                Texture2D currentTexture = batchItems[itemStartIndex].Texture;
                int itemEndIndex = FindDifferentTexture(currentTexture, itemStartIndex + 1);
                Span<TextureBatchItem> items = batchItems.AsSpan(itemStartIndex, itemEndIndex - itemStartIndex);
                itemStartIndex = itemEndIndex;

                EnsureBufferCapacity(items.Length * 6);
                int vbStorageLength = (int)vertexBuffer.StorageLength;
                int batchItemsPerDrawCall = vbStorageLength / 6;
                primitiveBatcher.EnsureTriangleSpace(vbStorageLength, false);

                graphicsDevice.VertexArray = vertexBuffer;
                graphicsDevice.ShaderProgram = ShaderProgram;
                TextureUniform.SetValueTexture(currentTexture);

                for (int startIndex = 0; startIndex < items.Length; startIndex += batchItemsPerDrawCall)
                {
                    int endIndex = Math.Min(startIndex + batchItemsPerDrawCall, items.Length);

                    primitiveBatcher.ClearTriangles();
                    for (int i = startIndex; i < endIndex; i++)
                        items[i].AddToBatcher(primitiveBatcher);

                    vertexBuffer.DataSubset.SetData(primitiveBatcher.TriangleVertices);
                    graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, (uint)primitiveBatcher.TriangleVertexCount);
                }
            }

            batchItemCount = 0;
        }

        private int FindDifferentTexture(Texture2D currentTexture, int startIndex)
        {
            while (startIndex < batchItemCount && batchItems[startIndex].Texture == currentTexture)
                startIndex++;
            return startIndex;
        }

        public void End()
        {
            if (!IsActive)
                throw new InvalidOperationException("Begin() must be called before End().");

            Flush();

            IsActive = false;
        }

        public void Dispose()
        {
            vertexBuffer.Dispose();
        }
    }

    /// <summary>
    /// Specifies options on how a <see cref="TextureBatcher"/> handles drawing textures.
    /// </summary>
    public enum BatcherBeginMode
    {
        // IMPORTANT NOTE:
        // The values are set specifically so those that require batch items to be sorted have the
        // least significant bit set to 1. This way, in order to know whether sorting is needed we
        // can just do (beginMode & 1) == 1

        /// <summary>
        /// Textures are drawn when End() is called in order of draw call, batching together where possible.
        /// </summary>
        Deferred = 0,

        /// <summary>
        /// Textures are drawn in order of draw call but the batcher doesn't wait until End() to flush all the calls.<para/>
        /// If the same texture is drawn consecutively the Draw()-s will still be batched into a single draw call.
        /// </summary>
        OnTheFly = 2,

        /// <summary>
        /// Each textures is drawn in it's own individual draw call, immediately, during Draw().
        /// </summary>
        Immediate = 4,

        /// <summary>
        /// Textures are drawn when End() is called, but first sorted by texture. This uses the least amount of draw
        /// calls, but doesn't retain order (depth testing can be used for ordering).
        /// </summary>
        SortByTexture = 1,

        /// <summary>
        /// Textures are drawn when End() is called, but first sorted by depth in back-to-front order.<para/>
        /// Textures with the same depth aren't guaranteed to retain the order in which they were Draw()-n.
        /// </summary>
        SortBackToFront = 3,

        /// <summary>
        /// Textures are drawn when End() is called, but first sorted by depth in front-to-back order.<para/>
        /// Textures with the same depth aren't guaranteed to retain the order in which they were Draw()-n.
        /// </summary>
        SortFrontToBack = 5,
    }
}
