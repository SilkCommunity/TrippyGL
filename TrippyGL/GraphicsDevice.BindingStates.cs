using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    public partial class GraphicsDevice
    {
        #region BufferObjectBindingStates

        /// <summary>
        /// This constant defines the total amount of buffer targets. This defines the array sizes
        /// for <see cref="bufferBindings"/> and <see cref="bufferBindingTargets"/> arrays.
        /// </summary>
        private const int BufferTargetCount = 12;

        // The index on the bufferBindings array where each BufferTarget is located
        private const int uniformBufferIndex = 0; //ranged
        private const int shaderStorageBufferIndex = 1; //ranged
        private const int atomicCounterBufferIndex = 2; //ranged
        private const int arrayBufferIndex = 3;
        private const int textureBufferIndex = 4;
        private const int pixelUnpackBufferIndex = 5;
        private const int pixelPackBufferIndex = 6;
        private const int drawIndirectBufferIndex = 7;
        private const int dispatchIndirectBufferIndex = 8;
        private const int copyReadBufferIndex = 9;
        private const int copyWriteBufferIndex = 10;
        private const int queryBufferIndex = 11;

        // Other buffer binding locations and where they are stored:
        // GL_ELEMENT_ARRAY_BUFFER is stored on a Vertex Array Object
        // GL_TRANSFORM_FEEDBACK_BUFFER is stored on a Transform Feedback Object

        internal const BufferTarget DefaultBufferTarget = BufferTarget.ArrayBuffer;
        private const int defaultBufferTargetBindingIndex = arrayBufferIndex;

        /// <summary>
        /// Stores the handle of the last buffer bound to the <see cref="BufferTarget"/>
        /// found on the same index on the <see cref="bufferBindingTargets"/> array.
        /// </summary>
        private BufferObject?[] bufferBindings;

        /// <summary>The <see cref="BufferTarget"/>-s for the handles found on the <see cref="bufferBindings"/> array.</summary>
        private BufferTarget[] bufferBindingTargets;

        /// <summary>
        /// For the <see cref="BufferTarget"/>-s that have range bindings, this is an
        /// array of arrays that contain the bound buffer and the bound range of each binding index.
        /// </summary>
        private BufferRangeBinding[][] bufferRangeBindings;

        /// <summary>
        /// Initializes all the fields needed for buffer binding.
        /// </summary>
        [MemberNotNull(nameof(bufferBindingTargets), nameof(bufferBindings), nameof(bufferRangeBindings))]
        private void InitBufferObjectStates()
        {
            bufferBindingTargets = new BufferTarget[BufferTargetCount];

            // The first four targets need to be the ones that are managed with glBindBufferBase/glBindBufferRange
            // because these also have an index, offset and size value to them. So we need to handle more data!
            // The way it's done then, is by having the bufferRangeBindings array. The same index used to get
            // the buffer target and generic binding id used to get the BufferRangeBinding array.
            // However, trying to do this with any other target will result in an IndexOutOfRangeException

            bufferBindingTargets[uniformBufferIndex] = BufferTarget.UniformBuffer;
            bufferBindingTargets[shaderStorageBufferIndex] = BufferTarget.ShaderStorageBuffer;
            bufferBindingTargets[atomicCounterBufferIndex] = BufferTarget.AtomicCounterBuffer;
            bufferBindingTargets[arrayBufferIndex] = BufferTarget.ArrayBuffer;
            bufferBindingTargets[textureBufferIndex] = BufferTarget.TextureBuffer;
            bufferBindingTargets[pixelUnpackBufferIndex] = BufferTarget.PixelUnpackBuffer;
            bufferBindingTargets[pixelPackBufferIndex] = BufferTarget.PixelPackBuffer;
            bufferBindingTargets[drawIndirectBufferIndex] = BufferTarget.DrawIndirectBuffer;
            bufferBindingTargets[dispatchIndirectBufferIndex] = BufferTarget.DispatchIndirectBuffer;
            bufferBindingTargets[copyWriteBufferIndex] = BufferTarget.CopyWriteBuffer;
            bufferBindingTargets[copyReadBufferIndex] = BufferTarget.CopyReadBuffer;
            bufferBindingTargets[queryBufferIndex] = BufferTarget.QueryBuffer;

            bufferBindings = new BufferObject[BufferTargetCount];

            bufferRangeBindings = new BufferRangeBinding[3][];

            bufferRangeBindings[0] = new BufferRangeBinding[MaxUniformBufferBindings];
            bufferRangeBindings[1] = new BufferRangeBinding[MaxShaderStorageBufferBindings];
            bufferRangeBindings[2] = new BufferRangeBinding[MaxAtomicCounterBufferBindings];
        }

        /// <summary>Gets or sets (binds) the <see cref="BufferObject"/> currently bound to GL_ARRAY_BUFFER.</summary>
        public BufferObject? ArrayBuffer
        {
            get => bufferBindings[arrayBufferIndex];
            set
            {
                if (bufferBindings[arrayBufferIndex] != value)
                {
                    GL.BindBuffer(BufferTargetARB.ArrayBuffer, value?.Handle ?? 0);
                    bufferBindings[arrayBufferIndex] = value;
                }
            }
        }

        /// <summary>Gets or sets (binds) the <see cref="BufferObject"/> currently bound to GL_COPY_READ_BUFFER.</summary>
        public BufferObject? CopyReadBuffer
        {
            get => bufferBindings[copyReadBufferIndex];
            set
            {
                if (bufferBindings[copyReadBufferIndex] != value)
                {
                    GL.BindBuffer(BufferTargetARB.CopyReadBuffer, value?.Handle ?? 0);
                    bufferBindings[copyReadBufferIndex] = value;
                }
            }
        }

        /// <summary>Gets or sets (binds) the <see cref="BufferObject"/> currently bound to GL_COPY_WRITE_BUFFER.</summary>
        public BufferObject? CopyWriteBuffer
        {
            get => bufferBindings[copyWriteBufferIndex];
            set
            {
                if (bufferBindings[copyWriteBufferIndex] != value)
                {
                    GL.BindBuffer(BufferTargetARB.CopyWriteBuffer, value?.Handle ?? 0);
                    bufferBindings[copyWriteBufferIndex] = value;
                }
            }
        }

        /// <summary>
        /// Binds a buffer to the default binding location.
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null.</param>
        internal void BindBufferObject(BufferObject buffer)
        {
            if (bufferBindings[defaultBufferTargetBindingIndex] != buffer)
                ForceBindBufferObject(buffer);
        }

        /// <summary>
        /// Binds a buffer to the default binding location without first checking whether it's already bound.
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null.</param>
        internal void ForceBindBufferObject(BufferObject buffer)
        {
            GL.BindBuffer((GLEnum)DefaultBufferTarget, buffer.Handle);
            bufferBindings[defaultBufferTargetBindingIndex] = buffer;
        }

        /// <summary>
        /// Binds a buffer subset to it's <see cref="BufferTarget"/>.
        /// </summary>
        /// <param name="bufferSubset">The buffer subset to bind.</param>
        public void BindBuffer(BufferObjectSubset bufferSubset)
        {
            if (bufferSubset == null)
                throw new ArgumentNullException(nameof(bufferSubset));

            if (bufferBindings[bufferSubset.bufferTargetBindingIndex] != bufferSubset.Buffer)
                ForceBindBuffer(bufferSubset);
        }

        /// <summary>
        /// Binds a buffer subset to it's <see cref="BufferTarget"/> without first checking whether it's already bound.
        /// </summary>
        /// <param name="bufferSubset">The buffer subset to bind. This value is assumed not to be null.</param>
        internal void ForceBindBuffer(BufferObjectSubset bufferSubset)
        {
            GL.BindBuffer((GLEnum)bufferSubset.BufferTarget, bufferSubset.BufferHandle);
            bufferBindings[bufferSubset.bufferTargetBindingIndex] = bufferSubset.Buffer;
        }

        /// <summary>
        /// Binds a range of a buffer subset to a binding index on it's <see cref="BufferTarget"/>
        /// The buffer subset's <see cref="BufferTarget"/> must be one with multiple binding indexes.
        /// </summary>
        /// <param name="bufferSubset">The buffer subset to bind.</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer will be bound.</param>
        /// <param name="offset">The offset in bytes into the buffer's storage where the bind begins.</param>
        /// <param name="size">The amount of bytes that can be read from the storage, starting from offset.</param>
        public void BindBufferRange(BufferObjectSubset bufferSubset, uint bindingIndex, uint offset, uint size)
        {
            if (bufferSubset == null)
                throw new ArgumentNullException(nameof(bufferSubset));

            BufferRangeBinding b = bufferRangeBindings[bufferSubset.bufferTargetBindingIndex][bindingIndex];
            if (b.Buffer != bufferSubset.Buffer || b.Size != size || b.Offset != offset + bufferSubset.StorageOffsetInBytes)
                ForceBindBufferRange(bufferSubset, bindingIndex, offset, size);
        }

        /// <summary>
        /// Binds a range of a buffer to a binding index on it's <see cref="BufferTarget"/> without first checking whether it's already bound.
        /// The buffer object's <see cref="BufferTarget"/> must be one with multiple binding indexes.
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null.</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer will be bound.</param>
        /// <param name="offset">The offset in bytes into the buffer's storage where the bind begins.</param>
        /// <param name="size">The amount of bytes that can be read from the storage, starting from offset.</param>
        internal void ForceBindBufferRange(BufferObjectSubset buffer, uint bindingIndex, uint offset, uint size)
        {
            offset += buffer.StorageOffsetInBytes;
            GL.BindBufferRange((GLEnum)buffer.BufferTarget, bindingIndex, buffer.BufferHandle, (int)offset, size);
            bufferBindings[buffer.bufferTargetBindingIndex] = buffer.Buffer;
            bufferRangeBindings[buffer.bufferTargetBindingIndex][bindingIndex].SetRange(buffer, offset, size);
        }

        /// <summary>
        /// Binds a buffer to the GL_COPY_READ_BUFFER target without first checking whether it's already bound.
        /// </summary>
        /// <param name="buffer">The buffer to bind.</param>
        internal void ForceBindBufferCopyRead(BufferObject? buffer)
        {
            GL.BindBuffer(BufferTargetARB.CopyReadBuffer, buffer?.Handle ?? 0);
            bufferBindings[copyReadBufferIndex] = buffer;
        }

        /// <summary>
        /// Binds a buffer to the GL_COPY_WRITE_BUFFER taret without first checking whether it's already bound.
        /// </summary>
        /// <param name="buffer">The buffer to bind.</param>
        internal void ForceBindBufferCopyWrite(BufferObject? buffer)
        {
            GL.BindBuffer(BufferTargetARB.CopyWriteBuffer, buffer?.Handle ?? 0);
            bufferBindings[copyWriteBufferIndex] = buffer;
        }

        /// <summary>
        /// Returns whether the given buffer subset is the currently bound one for it's <see cref="BufferTarget"/>.
        /// </summary>
        /// <param name="buffer">The buffer subset to check.</param>
        public bool IsBufferCurrentlyBound(BufferObjectSubset buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            return bufferBindings[buffer.bufferTargetBindingIndex] == buffer.Buffer;
        }

        /// <summary>
        /// Gets the index on the <see cref="bufferBindings"/> array for the specified <see cref="BufferTarget"/>.
        /// If there's no such index, it returns -1, though this won't happen as long as you only use proper <see cref="BufferTarget"/> enum values.
        /// </summary>
        /// <param name="bufferTarget">The <see cref="BufferTarget"/> to get the binds list index for.</param>
        internal int GetBindingTargetIndex(BufferTarget bufferTarget)
        {
            for (int i = 0; i < BufferTargetCount; i++)
                if (bufferBindingTargets[i] == bufferTarget)
                    return i;
            return -1;
        }

        /// <summary>
        /// Resets all saved states for buffer objects to the last values known by this <see cref="GraphicsDevice"/>.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions.
        /// </summary>
        public void ResetBufferStates()
        {
            for (int i = 0; i < BufferTargetCount; i++)
                GL.BindBuffer((GLEnum)bufferBindingTargets[i], bufferBindings[i]?.Handle ?? 0);

            for (int i = 0; i < bufferRangeBindings.Length; i++)
            {
                BufferRangeBinding[] arr = bufferRangeBindings[i];
                for (int c = 0; c < arr.Length; c++)
                {
                    BufferRangeBinding buf = arr[c];
                    if (buf.Buffer != null)
                        GL.BindBufferRange((GLEnum)bufferBindingTargets[i], (uint)c, buf.Buffer.Handle, (int)buf.Offset, buf.Size);
                }
            }
        }

        /// <summary>
        /// This struct is used to manage buffer object binding in cases where a buffer can be bound to multiple indices in the same target.
        /// Each <see cref="BufferRangeBinding"/> represents one of these binding points in a <see cref="BufferTarget"/>.
        /// Of course, this must be in a <see cref="BufferTarget"/> to which multiple buffers can be bound.
        /// </summary>
        internal struct BufferRangeBinding
        {
            public BufferObject? Buffer;
            public uint Offset;
            public uint Size;

            public void Reset()
            {
                Buffer = null;
                Offset = 0;
                Size = 0;
            }

            /// <summary>
            /// Set the values of this <see cref="BufferRangeBinding"/> to the specified range of the given buffer.
            /// </summary>
            public void SetRange(BufferObjectSubset buffer, uint offset, uint size)
            {
                Buffer = buffer.Buffer;
                Offset = offset;
                Size = size;
            }

            /// <summary>
            /// Sets the values of this <see cref="BufferRangeBinding"/> to the entire given subset.
            /// </summary>
            public void SetRange(BufferObjectSubset buffer)
            {
                Buffer = buffer.Buffer;
                Offset = buffer.StorageOffsetInBytes;
                Size = buffer.StorageLengthInBytes;
            }
        }

        #endregion BufferObjectBindingStates

        #region VertexArrayBindingStates

        private VertexArray? vertexArray;

        /// <summary>Gets or sets (binds) the currently bound <see cref="TrippyGL.VertexArray"/>.</summary>
        public VertexArray? VertexArray
        {
            get => vertexArray;
            set
            {
                if (vertexArray != value)
                {
                    GL.BindVertexArray(value?.Handle ?? 0);
                    vertexArray = value;
                }
            }
        }

        /// <summary>
        /// Binds a <see cref="TrippyGL.VertexArray"/> without first checking whether it's already bound.
        /// </summary>
        /// <param name="array">The <see cref="TrippyGL.VertexArray"/> to bind.</param>
        internal void ForceBindVertexArray(VertexArray? array)
        {
            GL.BindVertexArray(array?.Handle ?? 0);
            vertexArray = array;
        }

        /// <summary>
        /// Resets vertex array states to the last values known by this <see cref="GraphicsDevice"/>.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions.
        /// </summary>
        public void ResetVertexArrayStates()
        {
            GL.BindVertexArray(vertexArray?.Handle ?? 0);
        }

        #endregion VertexArrayBindingStates

        #region ShaderProgramBindingStates

        private ShaderProgram? shaderProgram;

        /// <summary>Gets or sets (binds) the currently bound <see cref="TrippyGL.ShaderProgram"/>.</summary>
        public ShaderProgram? ShaderProgram
        {
            get => shaderProgram;
            set
            {
                if (shaderProgram != value)
                    ForceUseShaderProgram(value);
            }
        }

        /// <summary>
        /// Installs the given <see cref="TrippyGL.ShaderProgram"/> into the rendering
        /// pipeline without first checking whether it's already in use.
        /// </summary>
        /// <param name="program">The <see cref="TrippyGL.ShaderProgram"/> to use.</param>
        internal void ForceUseShaderProgram(ShaderProgram? program)
        {
            GL.UseProgram(program?.Handle ?? 0);
            shaderProgram = program;
        }

        /// <summary>
        /// Resets all saved states for shader programs. This is, the variables used to check whether to use a shader program or not.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions.
        /// </summary>
        public void ResetShaderProgramStates()
        {
            GL.UseProgram(shaderProgram?.Handle ?? 0);
        }

        #endregion ShaderProgramBindingStates

        #region TextureBindingStates

        /// <summary>The array containing for each texture unit (that is, the index of the array) which texture handle is bound to it.</summary>
        private Texture?[] textureBindings;

        /// <summary>This variable counts which texture unit will be used the next time a texture needs binding.</summary>
        private int nextBindUnit;

        /// <summary>The currently active texture unit.</summary>
        public int ActiveTextureUnit { get; private set; }

        /// <summary>
        /// When a <see cref="Texture"/> needs a new binding, it requests a texture unit from this method.
        /// </summary>
        private int GetNextBindTextureUnit()
        {
            // Originally, when a texture needed a new binding, we'd just bind it to the current active texture unit.
            // However, It's probably more efficient to get different texture units each time.
            // Why? Say you have 10 textures (and more texture units), then you can bind each one to a single
            // texture unit and you'll never need to change the bindings, only the active texture if you want
            // to make a modification such as setting a texture parameter.
            // If you just bound it to the currently active texture, you'd be changing the binds over and over
            // in the same texture unit.

            nextBindUnit = (nextBindUnit + 1) % textureBindings.Length;
            return nextBindUnit;
        }

        /// <summary>
        /// Ensures a texture unit is the currently active one.
        /// </summary>
        /// <param name="textureUnit">The index of the texture unit. Must be in the range [0, <see cref="MaxTextureImageUnits"/>).</param>
        public void SetActiveTexture(int textureUnit)
        {
            if (ActiveTextureUnit != textureUnit)
                ForceSetActiveTextureUnit(textureUnit);
        }

        /// <summary>
        /// Sets the active texture unit without first checking whether it's the currently active texture unit.
        /// </summary>
        /// <param name="textureUnit">The index of the texture unit. Must be in the range [0, <see cref="MaxTextureImageUnits"/>).</param>
        internal void ForceSetActiveTextureUnit(int textureUnit)
        {
            if (textureUnit < 0 || textureUnit >= MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException(nameof(textureUnit), textureUnit, nameof(textureUnit) + " must be in the range [0, " + nameof(MaxTextureImageUnits) + ")");

            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            ActiveTextureUnit = textureUnit;
        }

        /// <summary>
        /// Ensures a <see cref="Texture"/> is bound to any texture unit, but doesn't
        /// ensure that texture unit is the currently active one.
        /// </summary>
        /// <returns>The texture unit to which the <see cref="Texture"/> is bound.</returns>
        public int BindTexture(Texture texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (textureBindings[texture.lastBindUnit] != texture)
                return ForceBindTexture(texture);
            return texture.lastBindUnit;
        }

        /// <summary>
        /// Ensures a <see cref="Texture"/> is bound to any texture unit, and that the
        /// texture unit to which said <see cref="Texture"/> is bound is the currently active one.
        /// </summary>
        /// <returns>The texture unit to which the <see cref="Texture"/> is bound.</returns>
        public int BindTextureSetActive(Texture texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (texture == textureBindings[texture.lastBindUnit])
            {
                SetActiveTexture(texture.lastBindUnit);
                return texture.lastBindUnit;
            }

            return ForceBindTexture(texture);
        }

        /// <summary>
        /// Binds a <see cref="Texture"/> to any texture unit.
        /// </summary>
        /// <returns>
        /// The texture unit to which the <see cref="Texture"/> is now bound to.
        /// The returned texture unit will also always be the currently active one.
        /// </returns>
        internal int ForceBindTexture(Texture texture)
        {
            SetActiveTexture(GetNextBindTextureUnit());
            GL.BindTexture((TextureTarget)texture.TextureType, texture.Handle);
            texture.lastBindUnit = ActiveTextureUnit;
            textureBindings[ActiveTextureUnit] = texture;
            return texture.lastBindUnit;
        }

        /// <summary>
        /// Binds a <see cref="Texture"/> to the currently active texture unit.
        /// </summary>
        internal void ForceBindTextureToCurrentUnit(Texture texture)
        {
            GL.BindTexture((TextureTarget)texture.TextureType, texture.Handle);
            texture.lastBindUnit = ActiveTextureUnit;
            textureBindings[ActiveTextureUnit] = texture;
        }

        /// <summary>
        /// Ensures all of the given <see cref="Texture"/>-s are bound to a texture unit. Nulls are ignored.
        /// </summary>
        public void BindAllTextures(ReadOnlySpan<Texture?> textures)
        {
            int textureCount = 0;
            for (int i = 0; i < textures.Length; i++)
            {
                Texture? t = textures[i];

                if (t == null)
                    continue;

                textureCount++;
                if (textureCount > textureBindings.Length)
                    throw new NotSupportedException("You tried to bind more textures at the same time than this system supports");

                if (textureBindings[t.lastBindUnit] != t)
                {
                    SetActiveTexture(FindUnusedTextureUnit(textures));
                    ForceBindTextureToCurrentUnit(t);
                }
            }

            // Find a texture unit that's not in use by any of the given textures
            int FindUnusedTextureUnit(ReadOnlySpan<Texture?> texturesToBind)
            {
                int unit;
                do
                {
                    // NOTE: This will not loop forever because GetNextBindTextureUnit() passes through all the possible
                    // texture units, and if you try to bind more textures than there are texture units an exception is thrown
                    // so at least one texture unit must be not used by any texture in texturesToBind.
                    unit = GetNextBindTextureUnit();
                } while (IsTextureUnitInUse(unit, texturesToBind));
                return unit;
            }

            // Whether a texture unit is currently in use by any of the specified textures
            bool IsTextureUnitInUse(int unit, ReadOnlySpan<Texture?> texturesToBind)
            {
                for (int i = 0; i < texturesToBind.Length; i++)
                {
                    Texture? t = texturesToBind[i];
                    if (t != null && t.lastBindUnit == unit && textureBindings[unit] == t)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Ensures all of the given <see cref="Texture"/>-s are bound to a texture unit. Nulls are ignored.
        /// </summary>
        public void BindAllTextures(List<Texture?> textures)
        {
            BindAllTextures(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(textures));
        }

        /// <summary>
        /// Returns whether a <see cref="Texture"/> is the one currently bound to it's last bind location.
        /// </summary>
        public bool IsTextureBound(Texture? texture)
        {
            return texture != null && textureBindings[texture.lastBindUnit] == texture;
        }

        /// <summary>
        /// Initiates all variables that track texture binding states.
        /// </summary>
        [MemberNotNull(nameof(textureBindings))]
        private void InitTextureStates()
        {
            textureBindings = new Texture[MaxTextureImageUnits];
            ActiveTextureUnit = 0;
            GL.ActiveTexture(TextureUnit.Texture0);
            nextBindUnit = 0;
        }

        /// <summary>
        /// Resets all saved states for textures.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions.
        /// </summary>
        public void ResetTextureStates()
        {
            for (int i = 0; i < textureBindings.Length; i++)
            {
                Texture? tex = textureBindings[i];
                if (tex != null)
                    GL.BindTexture((TextureTarget)tex.TextureType, tex.Handle);
            }

            GL.ActiveTexture(TextureUnit.Texture0 + ActiveTextureUnit);
        }

        #endregion TextureBindingStates

        #region FramebufferBindings

        private FramebufferObject? drawFramebuffer;
        private FramebufferObject? readFramebuffer;
        private RenderbufferObject? renderbuffer;

        /// <summary>Gets or sets (binds) the <see cref="FramebufferObject"/> currently bound for drawing.</summary>
        public FramebufferObject? DrawFramebuffer
        {
            get => drawFramebuffer;
            set
            {
                if (drawFramebuffer != value)
                {
                    GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, value?.Handle ?? 0);
                    drawFramebuffer = value;
                }
            }
        }

        /// <summary>Gets or sets (binds) the <see cref="FramebufferObject"/> currently bound for reading.</summary>
        public FramebufferObject? ReadFramebuffer
        {
            get => readFramebuffer;
            set
            {
                if (readFramebuffer != value)
                {
                    GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, value?.Handle ?? 0);
                    readFramebuffer = value;
                }
            }
        }

        /// <summary>Sets (binds) a <see cref="FramebufferObject"/> for both drawing and reading.</summary>
        public FramebufferObject? Framebuffer
        {
            set
            {
                if (readFramebuffer != value || drawFramebuffer != value)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, value?.Handle ?? 0);
                    drawFramebuffer = value;
                    readFramebuffer = value;
                }
            }
        }

        /// <summary>Gets or sets (binds) the current <see cref="RenderbufferObject"/>.</summary>
        public RenderbufferObject? Renderbuffer
        {
            get => renderbuffer;
            set
            {
                if (renderbuffer != value)
                {
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, value?.Handle ?? 0);
                    renderbuffer = value;
                }
            }
        }

        /// <summary>
        /// Binds a <see cref="FramebufferObject"/> for drawing without first checking whether it's already bound.
        /// </summary>
        internal void ForceBindDrawFramebuffer(FramebufferObject? framebuffer)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, framebuffer?.Handle ?? 0);
            drawFramebuffer = framebuffer;
        }

        /// <summary>
        /// Binds a <see cref="FramebufferObject"/> for reading without first checking whether it's already bound.
        /// </summary>
        internal void ForceBindReadFramebuffer(FramebufferObject? framebuffer)
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, framebuffer?.Handle ?? 0);
            readFramebuffer = framebuffer;
        }

        /// <summary>
        /// Binds a <see cref="RenderbufferObject"/> without first checking whether it's already bound.
        /// </summary>
        internal void ForceBindRenderbuffer(RenderbufferObject renderbuffer)
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer.Handle);
            this.renderbuffer = renderbuffer;
        }

        /// <summary>
        /// Resets all saved states for framebuffers and renderbuffers.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions.
        /// </summary>
        public void ResetFramebufferStates()
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, drawFramebuffer?.Handle ?? 0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, readFramebuffer?.Handle ?? 0);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer?.Handle ?? 0);
        }

        #endregion
    }
}
