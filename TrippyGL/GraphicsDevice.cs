using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public class GraphicsDevice
    {
        public IGraphicsContext Context { get; private set; }


        public GraphicsDevice(IGraphicsContext context)
        {
            this.Context = context;

            InitGLGetVariables();

            InitBufferObjectStates();
            vertexArrayBinding = 0;
            shaderProgramBinding = 0;
            InitTextureStates();
            framebufferDrawHandle = 0;
            framebufferReadHandle = 0;
        }

        #region GLGet

        private void InitGLGetVariables()
        {
            GLMajorVersion = GL.GetInteger(GetPName.MajorVersion);
            GLMinorVersion = GL.GetInteger(GetPName.MinorVersion);

            if (GLMajorVersion < 3)
                throw new PlatformNotSupportedException("The OpenGL version must be at least 3.0");

            UniformBufferOffsetAlignment = GL.GetInteger(GetPName.UniformBufferOffsetAlignment);
            MaxUniformBufferBindings = GL.GetInteger(GetPName.MaxUniformBufferBindings);
            MaxUniformBlockSize = GL.GetInteger(GetPName.MaxUniformBlockSize);
            MaxSamples = GL.GetInteger(GetPName.MaxSamples);
            MaxTextureSize = GL.GetInteger(GetPName.MaxTextureSize);
            MaxTextureImageUnits = GL.GetInteger(GetPName.MaxTextureImageUnits);
        }

        public int GLMajorVersion { get; private set; }

        public int GLMinorVersion { get; private set; }

        public int UniformBufferOffsetAlignment { get; private set; }

        public int MaxUniformBufferBindings { get; private set; }

        public int MaxUniformBlockSize { get; private set; }

        public int MaxSamples { get; private set; }

        public int MaxTextureSize { get; private set; }

        public int MaxTextureImageUnits { get; private set; }

        public string GLVersion { get { return GL.GetString(StringName.Version); } }

        public string GLVendor { get { return GL.GetString(StringName.Vendor); } }

        public string GLRenderer { get { return GL.GetString(StringName.Renderer); } }

        public string GLShadingLanguageVersion { get { return GL.GetString(StringName.ShadingLanguageVersion); } }

        #endregion GLGet

        #region BindingStates

        /// <summary>
        /// Resets all saved states. These variables are used to prevent unnecessarily setting the same states twice.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions
        /// </summary>
        public void ResetStates()
        {
            ResetBufferStates();
            ResetVertexArrayStates();
            ResetShaderProgramStates();
            ResetTextureStates();
            ResetFramebufferStates();
        }

        #region BufferObjectBindingStates

        /// <summary>This constant defines the total amount of buffer targets. This defines the array sizes for the bufferBindings and bufferBindingTargets arrays</summary>
        private const int BufferTargetCount = 14;

        /// <summary>Stores the handle of the last buffer bound to the BufferTarget found on the same index on the bufferBindingsTarget array</summary>
        private int[] bufferBindings;

        /// <summary>The BufferTargets for the handles found on the bufferBindings array</summary>
        private BufferTarget[] bufferBindingTargets;

        /// <summary>TODO: add summary and explanation</summary>
        private BufferRangeBinding[][] bufferRangeBindings;

        private void InitBufferObjectStates()
        {
            bufferBindingTargets = new BufferTarget[BufferTargetCount]
            {
                // The first four need to be the ones that are managed with glBindBufferBase/glBindBufferRange
                // because these also have an index, offset and size value to them. So we need to handle more data!
                // The way it's done then, is by having the bufferRangeBindings array. The same index used to get
                // the buffer target and generic binding id used to get the BufferRangeBinding array.
                
                BufferTarget.TransformFeedbackBuffer,   // While not all of these might be needed, I'll put them all
                BufferTarget.UniformBuffer,             // in here to ensure compatibility if such features are ever 
                BufferTarget.ShaderStorageBuffer,       // implemented into the library or whatever.
                BufferTarget.AtomicCounterBuffer,
                BufferTarget.ArrayBuffer,
                BufferTarget.ElementArrayBuffer,
                BufferTarget.TextureBuffer,
                BufferTarget.PixelUnpackBuffer,
                BufferTarget.PixelPackBuffer,
                BufferTarget.DrawIndirectBuffer,
                BufferTarget.DispatchIndirectBuffer,
                BufferTarget.CopyWriteBuffer,
                BufferTarget.CopyReadBuffer,
                BufferTarget.QueryBuffer
            };
            bufferBindings = new int[BufferTargetCount];

            bufferRangeBindings = new BufferRangeBinding[4][];

            bufferRangeBindings[0] = new BufferRangeBinding[GL.GetInteger(GetPName.MaxTransformFeedbackBuffers)];
            bufferRangeBindings[1] = new BufferRangeBinding[GL.GetInteger(GetPName.MaxUniformBufferBindings)];
            bufferRangeBindings[2] = new BufferRangeBinding[GL.GetInteger((GetPName)All.MaxShaderStorageBufferBindings)]; //opentk wtf
            bufferRangeBindings[3] = new BufferRangeBinding[GL.GetInteger((GetPName)All.MaxAtomicCounterBufferBindings)];
        }

        /// <summary>
        /// Ensures a buffer is bound to it's BufferTarget by binding it if it's not
        /// </summary>
        /// <param name="buffer">The buffer to ensure is bound. This value is assumed not to be null</param>
        public void EnsureBufferBound(BufferObject buffer)
        {
            if (bufferBindings[buffer.bufferBindingTargetIndex] != buffer.Handle)
                BindBuffer(buffer);
        }

        /// <summary>
        /// Binds a buffer to it's BufferTarget. Prefer using EnsureBufferBound() instead to prevent unnecessary binds
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null</param>
        public void BindBuffer(BufferObject buffer)
        {
            GL.BindBuffer(buffer.BufferTarget, buffer.Handle);
            bufferBindings[buffer.bufferBindingTargetIndex] = buffer.Handle;
        }

        /// <summary>
        /// Ensures a buffer is bound to a specified binding index in it's BufferTarget by binding it if it's not.
        /// The buffer object's BufferTarget must be one with multiple binding indexes
        /// </summary>
        /// <param name="buffer">The buffer to ensure is bound. This value is assumed not to be null</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer should be bound</param>
        public void EnsureBufferBoundBase(BufferObject buffer, int bindingIndex)
        {
            BufferRangeBinding b = bufferRangeBindings[buffer.bufferBindingTargetIndex][bindingIndex];
            if (b.BufferHandle != buffer.Handle || b.Size != buffer.StorageLengthInBytes || b.Offset != 0)
                BindBufferBase(buffer, bindingIndex);
        }

        /// <summary>
        /// Bind a buffer to a binding index on it's BufferTarget. Prefer using EnsureBufferBoundBase() to prevent unnecessary binds.
        /// The buffer object's BufferTarget must be one with multiple binding indexes
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer will be bound</param>
        public void BindBufferBase(BufferObject buffer, int bindingIndex)
        {
            GL.BindBufferBase((BufferRangeTarget)buffer.BufferTarget, bindingIndex, buffer.Handle);
            bufferBindings[buffer.bufferBindingTargetIndex] = buffer.Handle;
            bufferRangeBindings[buffer.bufferBindingTargetIndex][bindingIndex].SetBase(buffer);
        }

        /// <summary>
        /// Ensures a buffer's range is bound to a specified binding index in it's BufferTarget by binding it if it's not.
        /// The buffer object's BufferTarget must be one with multiple binding indexes
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer will be bound</param>
        /// <param name="offset">The offset in bytes into the buffer's storage where the bind begins</param>
        /// <param name="size">The amount of bytes that can be read from the storage, starting from offset</param>
        public void EnsureBufferBoundRange(BufferObject buffer, int bindingIndex, int offset, int size)
        {
            BufferRangeBinding b = bufferRangeBindings[buffer.bufferBindingTargetIndex][bindingIndex];
            if (b.BufferHandle != buffer.Handle || b.Size != size || b.Offset != offset)
                BindBufferRange(buffer, bindingIndex, offset, size);
        }

        /// <summary>
        /// Bind a range of a buffer to a binding index on it's BufferTarget. Prefer using EnsureBufferBoundBase() to prevent unnecessary binds.
        /// The buffer object's BufferTarget must be one with multiple binding indexes
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer will be bound</param>
        /// <param name="offset">The offset in bytes into the buffer's storage where the bind begins</param>
        /// <param name="size">The amount of bytes that can be read from the storage, starting from offset</param>
        public void BindBufferRange(BufferObject buffer, int bindingIndex, int offset, int size)
        {
            GL.BindBufferRange((BufferRangeTarget)buffer.BufferTarget, bindingIndex, buffer.Handle, (IntPtr)offset, size);
            bufferBindings[buffer.bufferBindingTargetIndex] = buffer.Handle;
            bufferRangeBindings[buffer.bufferBindingTargetIndex][bindingIndex].SetRange(buffer, offset, size);
        }

        /// <summary>
        /// Returns whether the given buffer is the currently bound one for it's BufferTarget
        /// </summary>
        /// <param name="buffer">The buffer to check. This value is assumed not to be null</param>
        public bool IsBufferCurrentlyBound(BufferObject buffer)
        {
            return bufferBindings[buffer.bufferBindingTargetIndex] == buffer.Handle;
        }

        /// <summary>
        /// Gets the index on the 'bufferBindings' list for the specified BufferTarget.
        /// If there's no such index, it returns -1, though this won't happen as long as you only use proper BufferTarget enum values
        /// </summary>
        /// <param name="bufferTarget">The BufferTarget to get the binds list index for</param>
        internal int GetBindingTargetIndex(BufferTarget bufferTarget)
        {
            for (int i = 0; i < BufferTargetCount; i++)
                if (bufferBindingTargets[i] == bufferTarget)
                    return i;
            return -1;
        }

        /// <summary>
        /// Resets all saved states for buffer objects. This is, the variables used to check whether to bind a buffer or not.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions
        /// </summary>
        public void ResetBufferStates()
        {
            for (int i = 0; i < BufferTargetCount; i++)
                bufferBindings[i] = 0;
            for (int i = 0; i < 4; i++)
            {
                BufferRangeBinding[] arr = bufferRangeBindings[i];
                for (int c = 0; c < arr.Length; c++)
                    arr[c].Reset();
            }
        }

        #endregion BufferObjectBindingStates

        #region VertexArrayBindingStates

        /// <summary>The currently bound VertexArray's handle</summary>
        private int vertexArrayBinding;

        /// <summary>
        /// Ensures a vertex array is bound by binding it if it's not
        /// </summary>
        /// <param name="array">The array to ensure is bound. This value is assumed not to be null</param>
        public void EnsureVertexArrayBound(VertexArray array)
        {
            if (vertexArrayBinding != array.Handle)
                BindVertexArray(array);
        }

        /// <summary>
        /// Binds a vertex array. Prefer using EnsureVertexArrayBound() instead to prevent unnecessary binds
        /// </summary>
        /// <param name="array">The array to ensure is bound. This value is assumed not to be null</param>
        public void BindVertexArray(VertexArray array)
        {
            GL.BindVertexArray(array.Handle);
            vertexArrayBinding = array.Handle;
        }

        /// <summary>
        /// Resets all saved states for vertex arrays This is, the variables used to check whether to bind a vertex array or not.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions
        /// </summary>
        public void ResetVertexArrayStates()
        {
            vertexArrayBinding = 0;
        }

        /// <summary>
        /// Unbinds the currently active vertex array by binding to array 0
        /// </summary>
        public void UnbindVertexArray()
        {
            GL.BindVertexArray(0);
            vertexArrayBinding = 0;
        }

        #endregion VertexArrayBindingStates

        #region ShaderProgramBindingStates

        /// <summary>The currently bound ShaderProgram's handle</summary>
        private int shaderProgramBinding;

        /// <summary>
        /// Returns whether the given shader program is the one currently in use
        /// </summary>
        /// <param name="program">The program to check, This value is assumed to not be null</param>
        public bool IsShaderProgramInUse(ShaderProgram program)
        {
            return shaderProgramBinding == program.Handle;
        }

        /// <summary>
        /// Ensures the given ShaderProgram is the one currently in use
        /// </summary>
        /// <param name="program">The shader program to use. This value is assumed not to be null</param>
        public void EnsureShaderProgramInUse(ShaderProgram program)
        {
            if (shaderProgramBinding != program.Handle)
                UseShaderProgram(program);
        }

        /// <summary>
        /// Installs the given program into the rendering pipeline. Prefer using EnsureShaderProgramInUse() to avoid unnecessary uses
        /// </summary>
        /// <param name="program">The shader program to use. This value is assumed not to be null</param>
        public void UseShaderProgram(ShaderProgram program)
        {
            GL.UseProgram(program.Handle);
            shaderProgramBinding = program.Handle;
        }

        /// <summary>
        /// Uninstalls the current shader program from the pipeline by using program 0
        /// </summary>
        public void UninstallCurrentShaderProgram()
        {
            GL.UseProgram(0);
            shaderProgramBinding = 0;
        }

        /// <summary>
        /// Resets all saved states for shader programs. This is, the variables used to check whether to use a shader program or not.
        /// You should only need to call this when itneroperating with other libraries or using your own GL functions
        /// </summary>
        public void ResetShaderProgramStates()
        {
            shaderProgramBinding = 0;
        }

        #endregion ShaderProgramBindingStates

        #region TextureBindingStates

        /// <summary>The array containing for each texture unit (that is, the index of the array) which texture handle is bound to it</summary>
        private int[] textureBindings;

        /// <summary>This variable counts which texture unit will be used the next time a texture needs binding</summary>
        private int nextBindUnit;

        /// <summary>The currently active texture unit</summary>
        public int ActiveTextureUnit { get; private set; }

        /// <summary>The total amount of texture units. This is the maximum amount of textures that can be bound at the same time</summary>
        public int TotalTextureUnits { get { return textureBindings.Length; } }

        /// <summary>
        /// When a texture needs a new binding, it requests a texture unit from this method
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
        /// Ensures a texture unit is the currently active one
        /// </summary>
        /// <param name="textureUnit">The index of the texture unit. Must be in the range [0, TotalTextureUnits)</param>
        public void EnsureActiveTextureUnit(int textureUnit)
        {
            if (ActiveTextureUnit != textureUnit)
                SetActiveTextureUnit(textureUnit);
        }

        /// <summary>
        /// Sets the active texture unit. Prefer using EnsureActiveTextureUnit() instead to avoid unnecessary changes
        /// </summary>
        /// <param name="textureUnit">The index of the texture unit. Must be in the range [0, TotalTextureUnits)</param>
        public void SetActiveTextureUnit(int textureUnit)
        {
            if (textureUnit < 0 || textureUnit >= TotalTextureUnits)
                throw new ArgumentOutOfRangeException("textureUnit", textureUnit, "textureUnit must be in the range [0, TotalTextureUnits)");

            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            ActiveTextureUnit = textureUnit;
        }

        /// <summary>
        /// Ensures a texture is bound to any texture unit, but doesn't ensure that texture unit is the currently active one.
        /// Returns the texture unit to which the texture is bound
        /// </summary>
        /// <param name="texture">The texture to ensure is bound</param>
        public int EnsureTextureBound(Texture texture)
        {
            if (textureBindings[texture.lastBindUnit] != texture.Handle)
                return BindTexture(texture);
            return texture.lastBindUnit;
        }

        /// <summary>
        /// Ensures a texture is bound to any texture unit, and that the texture unit to which the texture is bound is the currently active one.
        /// Returns the texture unit to which the texture is bound
        /// </summary>
        /// <param name="texture">The texture to ensure is bound and active</param>
        public int EnsureTextureBoundAndActive(Texture texture)
        {
            if (texture.Handle == textureBindings[texture.lastBindUnit])
                EnsureActiveTextureUnit(texture.lastBindUnit);
            else
                return BindTexture(texture);
            return texture.lastBindUnit;
        }

        /// <summary>
        /// Binds a texture to any texture unit. Returns the texture unit to which the texture is now bound to.
        /// The returned texture unit will also always be the currently active one.
        /// </summary>
        /// <param name="texture">The texture to bind</param>
        public int BindTexture(Texture texture)
        {
            EnsureActiveTextureUnit(GetNextBindTextureUnit());
            GL.BindTexture(texture.TextureType, texture.Handle);
            texture.lastBindUnit = ActiveTextureUnit;
            textureBindings[ActiveTextureUnit] = texture.Handle;
            return texture.lastBindUnit;
        }

        /// <summary>
        /// Binds a texture to the current texture unit
        /// </summary>
        /// <param name="texture">The texture to bind</param>
        public void BindTextureToCurrentUnit(Texture texture)
        {
            GL.BindTexture(texture.TextureType, texture.Handle);
            texture.lastBindUnit = ActiveTextureUnit;
            textureBindings[ActiveTextureUnit] = texture.Handle;
        }

        public int[] EnsureAllBound(Texture[] textures)
        {
            if (textures.Length > textureBindings.Length)
                throw new NotSupportedException("You tried to bind more textures at the same time than this system supports");

            int[] units = new int[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                SetActiveTextureUnit(i);
                BindTextureToCurrentUnit(textures[i]);
                units[i] = i;
            }
            return units;
        }

        public int[] EnsureAllBound(List<Texture> textures)
        {
            if (textures.Count > textureBindings.Length)
                throw new NotSupportedException("You tried to bind more textures at the same time than this system supports");

            int[] units = new int[textures.Count];
            for (int i = 0; i < textures.Count; i++)
            {
                SetActiveTextureUnit(i);
                BindTextureToCurrentUnit(textures[i]);
                units[i] = i;
            }
            return units;
        }

        /// <summary>
        /// Initiates all variables that track texture binding states
        /// </summary>
        private void InitTextureStates()
        {
            textureBindings = new int[GL.GetInteger(GetPName.MaxTextureImageUnits)];
            ActiveTextureUnit = 0;
            GL.ActiveTexture(TextureUnit.Texture0);
            nextBindUnit = 0;
        }

        /// <summary>
        /// Resets all saved states for textures
        /// </summary>
        public void ResetTextureStates()
        {
            for (int i = 0; i < textureBindings.Length; i++)
                textureBindings[i] = 0;
            ActiveTextureUnit = 0;
            GL.ActiveTexture(TextureUnit.Texture0);
            nextBindUnit = 0;
        }

        #endregion TextureBindingStates

        #region FramebufferBindings

        /// <summary>The handle of the framebuffer currently bound to the draw target</summary>
        private int framebufferDrawHandle;

        /// <summary>The handle of the framebuffer currently bound to the read target</summary>
        private int framebufferReadHandle;

        /// <summary>
        /// Ensures a framebuffer is bound to a specified target
        /// </summary>
        /// <param name="target">The framebuffer target</param>
        /// <param name="framebuffer">The framebuffer to ensure is bound</param>
        public void EnsureFramebufferBound(FramebufferTarget target, FramebufferObject framebuffer)
        {
            int handle = framebuffer == null ? 0 : framebuffer.Handle;
            switch (target)
            {
                case FramebufferTarget.DrawFramebuffer:
                    EnsureFramebufferBoundDraw(handle);
                    break;
                case FramebufferTarget.ReadFramebuffer:
                    EnsureFramebufferBoundRead(handle);
                    break;
                default:
                    EnsureFramebufferBound(handle);
                    break;
            }
        }

        /// <summary>
        /// Binds a framebuffer to a specified target. Prefer using EnsureFramebufferBound() instead to prevent unnecessary binds
        /// </summary>
        /// <param name="target">The framebuffer target</param>
        /// <param name="framebuffer">The framebuffer bind</param>
        public void BindFramebuffer(FramebufferTarget target, FramebufferObject framebuffer)
        {
            int handle = framebuffer == null ? 0 : framebuffer.Handle;
            switch (target)
            {
                case FramebufferTarget.DrawFramebuffer:
                    BindFramebufferDraw(handle);
                    break;
                case FramebufferTarget.ReadFramebuffer:
                    BindFramebufferRead(handle);
                    break;
                default:
                    BindFramebuffer(handle);
                    break;
            }
        }

        /// <summary>
        /// Ensures a framebuffer is bound to the draw and read targets
        /// </summary>
        /// <param name="framebuffer">The framebuffer to ensure is bound</param>
        public void EnsureFramebufferBound(FramebufferObject framebuffer)
        {
            EnsureFramebufferBound(framebuffer == null ? 0 : framebuffer.Handle);
        }

        /// <summary>
        /// Binds a framebuffer to both draw and read targets. Prefer using EnsureFramebufferBound() instead to prevent unnecessary binds
        /// </summary>
        /// <param name="framebuffer">The framebuffer to bind</param>
        public void BindFramebuffer(FramebufferObject framebuffer)
        {
            BindFramebuffer(framebuffer == null ? 0 : framebuffer.Handle);
        }

        /// <summary>
        /// Ensures a framebuffer is bound to the draw target
        /// </summary>
        /// <param name="framebuffer">The framebuffer to ensure is bound</param>
        public void EnsureFramebufferBoundDraw(FramebufferObject framebuffer)
        {
            EnsureFramebufferBoundDraw(framebuffer == null ? 0 : framebuffer.Handle);
        }

        /// <summary>
        /// Binds a framebuffer to the draw target. Prefer using EnsureFramebufferBoundDraw() instead to prevent unnecessary binds
        /// </summary>
        /// <param name="framebuffer">The framebuffer to bind</param>
        public void BindFramebufferDraw(FramebufferObject framebuffer)
        {
            BindFramebufferDraw(framebuffer == null ? 0 : framebuffer.Handle);
        }

        /// <summary>
        /// Ensures a framebuffer is bound to the read target
        /// </summary>
        /// <param name="framebuffer">The framebuffer to ensure is bound</param>
        public void EnsureFramebufferBoundRead(FramebufferObject framebuffer)
        {
            EnsureFramebufferBoundRead(framebuffer == null ? 0 : framebuffer.Handle);
        }

        /// <summary>
        /// Binds a framebuffer to the read target. Prefer using EnsureFramebufferBoundRead() instead to prevent unnecessary binds
        /// </summary>
        /// <param name="framebuffer">The framebuffer to bind</param>
        public void BindFramebufferRead(FramebufferObject framebuffer)
        {
            BindFramebufferRead(framebuffer == null ? 0 : framebuffer.Handle);
        }

        /// <summary>
        /// Ensures a framebuffer is bound to the draw and read targets
        /// </summary>
        /// <param name="handle">The framebuffer.s handle to ensure is bound</param>
        internal void EnsureFramebufferBound(int handle)
        {
            if (framebufferDrawHandle != handle || framebufferReadHandle != handle)
                BindFramebuffer(handle);
        }

        /// <summary>
        /// Binds a framebuffer to a specified target. Prefer using EnsureFramebufferBound() instead to prevent unnecessary binds
        /// </summary>
        /// <param name="target">The framebuffer target</param>
        /// <param name="handle">The framebuffer's handle bind</param>
        internal void BindFramebuffer(int handle)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);
            framebufferDrawHandle = handle;
            framebufferReadHandle = handle;
        }

        /// <summary>
        /// Ensures a framebuffer is bound to the draw target
        /// </summary>
        /// <param name="handle">The framebuffer.s handle to ensure is bound</param>
        internal void EnsureFramebufferBoundDraw(int handle)
        {
            if (framebufferDrawHandle != handle)
                BindFramebufferDraw(handle);
        }

        /// <summary>
        /// Binds a framebuffer to the draw target. Prefer using EnsureFramebufferBoundDraw() instead to prevent unnecessary binds
        /// </summary>
        /// <param name="handle">The framebuffer's handle bind</param>
        internal void BindFramebufferDraw(int handle)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, handle);
            framebufferDrawHandle = handle;
        }

        /// <summary>
        /// Ensures a framebuffer is bound to the read target
        /// </summary>
        /// <param name="handle">The framebuffer.s handle to ensure is bound</param>
        internal void EnsureFramebufferBoundRead(int handle)
        {
            if (framebufferReadHandle != handle)
                BindFramebufferRead(handle);
        }

        /// <summary>
        /// Binds a framebuffer to the read target. Prefer using EnsureFramebufferBoundRead() instead to prevent unnecessary binds
        /// </summary>
        /// <param name="handle">The framebuffer's handle bind</param>
        internal void BindFramebufferRead(int handle)
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, handle);
            framebufferReadHandle = handle;
        }

        /// <summary>
        /// Resets all saved states for FramebufferObjects. This is, the variables used to check whether to use a shader program or not.
        /// You should only need to call this when itneroperating with other libraries or using your own GL functions
        /// </summary>
        public void ResetFramebufferStates()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            framebufferDrawHandle = 0;
            framebufferReadHandle = 0;
        }

        #endregion
        
        /// <summary>
        /// This struct is used to manage buffer object binding in cases where a buffer can be bound to multiple indices in the same target.
        /// Each BufferRangeBinding represents one of these binding points in a BufferTarget. Of course, this must be in a BufferTarget
        /// to which multiple buffers can be bound.
        /// </summary>
        private struct BufferRangeBinding
        {
            public int BufferHandle;
            public int Offset;
            public int Size;

            public void Reset()
            {
                this.BufferHandle = 0;
                this.Offset = 0;
                this.Size = 0;
            }

            /// <summary>
            /// Set the values of this BufferRangeBinding as for when glBindBufferBase was called
            /// </summary>
            public void SetBase(BufferObject buffer)
            {
                this.BufferHandle = buffer.Handle;
                this.Offset = 0;
                this.Size = buffer.StorageLengthInBytes;
            }

            /// <summary>
            /// Set the values of the BufferRangeBinding
            /// </summary>
            public void SetRange(BufferObject buffer, int offset, int size)
            {
                this.BufferHandle = buffer.Handle;
                this.Offset = offset;
                this.Size = size;
            }
        }

        #endregion BindingStates



        public void MakeMine(GraphicsResource resource)
        {
            resource.GraphicsDevice = this;
        }
    }
}
