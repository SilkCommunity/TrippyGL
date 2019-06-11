using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// The GraphicsDevice manages an OpenGL Context and it's GraphicsResources (everything from BufferObjects to Textures to ShaderPrograms)
    /// </summary>
    public class GraphicsDevice : IDisposable
    {
        /// <summary>The OpenGL Context for this GraphicDevice</summary>
        public IGraphicsContext Context { get; private set; }

        /// <summary>Whether this GraphicsDevice has been disposed</summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a GraphicsDevice to manage the given graphics context
        /// </summary>
        /// <param name="context">The OpenGL Context for this GraphicsDevice</param>
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
            renderbufferHandle = 0;
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
            MaxTextureBufferSize = GL.GetInteger(GetPName.MaxTextureBufferSize);
            Max3DTextureSize = GL.GetInteger(GetPName.Max3DTextureSize);
            MaxCubeMapTextureSize = GL.GetInteger(GetPName.MaxCubeMapTextureSize);
            MaxRectangleTextureSize = GL.GetInteger(GetPName.MaxRectangleTextureSize);
            MaxRenderbufferSize = GL.GetInteger(GetPName.MaxRenderbufferSize);
        }

        public int GLMajorVersion { get; private set; }

        public int GLMinorVersion { get; private set; }

        public int UniformBufferOffsetAlignment { get; private set; }

        public int MaxUniformBufferBindings { get; private set; }

        public int MaxUniformBlockSize { get; private set; }

        public int MaxSamples { get; private set; }

        public int MaxTextureSize { get; private set; }

        public int Max3DTextureSize { get; private set; }

        public int MaxCubeMapTextureSize { get; private set; }

        public int MaxRectangleTextureSize { get; private set; }

        public int MaxTextureBufferSize { get; private set; }

        public int MaxTextureImageUnits { get; private set; }

        public int MaxRenderbufferSize { get; private set; }

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

        /// <summary>For the four BufferTargets that have range bindings, this is an array of four arrays that contain the bound buffer and the bound range of each binding index</summary>
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
        public void BindBuffer(BufferObject buffer)
        {
            if (bufferBindings[buffer.bufferBindingTargetIndex] != buffer.Handle)
                ForceBindBuffer(buffer);
        }

        /// <summary>
        /// Binds a buffer to it's BufferTarget without first checking whether it's already bound.
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null</param>
        internal void ForceBindBuffer(BufferObject buffer)
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
        public void BindBufferBase(BufferObject buffer, int bindingIndex)
        {
            BufferRangeBinding b = bufferRangeBindings[buffer.bufferBindingTargetIndex][bindingIndex];
            if (b.BufferHandle != buffer.Handle || b.Size != buffer.StorageLengthInBytes || b.Offset != 0)
                ForceBindBufferBase(buffer, bindingIndex);
        }

        /// <summary>
        /// Bind a buffer to a binding index on it's BufferTarget without first checking whether it's already bound.
        /// The buffer object's BufferTarget must be one with multiple binding indexes
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer will be bound</param>
        internal void ForceBindBufferBase(BufferObject buffer, int bindingIndex)
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
        public void BindBufferRange(BufferObject buffer, int bindingIndex, int offset, int size)
        {
            BufferRangeBinding b = bufferRangeBindings[buffer.bufferBindingTargetIndex][bindingIndex];
            if (b.BufferHandle != buffer.Handle || b.Size != size || b.Offset != offset)
                ForceBindBufferRange(buffer, bindingIndex, offset, size);
        }

        /// <summary>
        /// Bind a range of a buffer to a binding index on it's BufferTarget without first checking whether it's already bound.
        /// The buffer object's BufferTarget must be one with multiple binding indexes
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer will be bound</param>
        /// <param name="offset">The offset in bytes into the buffer's storage where the bind begins</param>
        /// <param name="size">The amount of bytes that can be read from the storage, starting from offset</param>
        internal void ForceBindBufferRange(BufferObject buffer, int bindingIndex, int offset, int size)
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
        public void BindVertexArray(VertexArray array)
        {
            if (vertexArrayBinding != array.Handle)
                ForceBindVertexArray(array);
        }

        /// <summary>
        /// Binds a vertex array without first checking whether it's already bound
        /// </summary>
        /// <param name="array">The array to ensure is bound. This value is assumed not to be null</param>
        internal void ForceBindVertexArray(VertexArray array)
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
        public void UseShaderProgram(ShaderProgram program)
        {
            if (shaderProgramBinding != program.Handle)
                ForceUseShaderProgram(program);
        }

        /// <summary>
        /// Installs the given program into the rendering pipeline without first checking whether it's already in use
        /// </summary>
        /// <param name="program">The shader program to use. This value is assumed not to be null</param>
        public void ForceUseShaderProgram(ShaderProgram program)
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
        public void SetActiveTexture(int textureUnit)
        {
            if (ActiveTextureUnit != textureUnit)
                ForceSetActiveTextureUnit(textureUnit);
        }

        /// <summary>
        /// Sets the active texture unit without first checking whether it's the currently active texture unit
        /// </summary>
        /// <param name="textureUnit">The index of the texture unit. Must be in the range [0, MaxTextureImageUnits)</param>
        internal void ForceSetActiveTextureUnit(int textureUnit)
        {
            if (textureUnit < 0 || textureUnit >= MaxTextureImageUnits)
                throw new ArgumentOutOfRangeException("textureUnit", textureUnit, "textureUnit must be in the range [0, MaxTextureImageUnits)");

            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            ActiveTextureUnit = textureUnit;
        }

        /// <summary>
        /// Ensures a texture is bound to any texture unit, but doesn't ensure that texture unit is the currently active one.
        /// Returns the texture unit to which the texture is bound
        /// </summary>
        /// <param name="texture">The texture to ensure is bound</param>
        public int BindTexture(Texture texture)
        {
            if (textureBindings[texture.lastBindUnit] != texture.Handle)
                return ForceBindTexture(texture);
            return texture.lastBindUnit;
        }

        /// <summary>
        /// Ensures a texture is bound to any texture unit, and that the texture unit to which the texture is bound is the currently active one.
        /// Returns the texture unit to which the texture is bound
        /// </summary>
        /// <param name="texture">The texture to ensure is bound and active</param>
        public int BindTextureSetActive(Texture texture)
        {
            if (texture.Handle == textureBindings[texture.lastBindUnit])
            {
                SetActiveTexture(texture.lastBindUnit);
                return texture.lastBindUnit;
            }

            return ForceBindTexture(texture);
        }

        /// <summary>
        /// Binds a texture to any texture unit. Returns the texture unit to which the texture is now bound to.
        /// The returned texture unit will also always be the currently active one.
        /// </summary>
        /// <param name="texture">The texture to bind</param>
        internal int ForceBindTexture(Texture texture)
        {
            SetActiveTexture(GetNextBindTextureUnit());
            GL.BindTexture(texture.TextureType, texture.Handle);
            texture.lastBindUnit = ActiveTextureUnit;
            textureBindings[ActiveTextureUnit] = texture.Handle;
            return texture.lastBindUnit;
        }

        /// <summary>
        /// Binds a texture to the current texture unit
        /// </summary>
        /// <param name="texture">The texture to bind</param>
        internal void ForceBindTextureToCurrentUnit(Texture texture)
        {
            GL.BindTexture(texture.TextureType, texture.Handle);
            texture.lastBindUnit = ActiveTextureUnit;
            textureBindings[ActiveTextureUnit] = texture.Handle;
        }

        /// <summary>
        /// Ensures all of the given textures are bound to a texture unit
        /// </summary>
        /// <param name="textures">The textures to ensure are bound</param>
        public void BindAllTextures(Texture[] textures)
        {
            if (textures.Length > textureBindings.Length)
                throw new NotSupportedException("You tried to bind more textures at the same time than this system supports");

            for (int i = 0; i < textures.Length; i++)
            {
                Texture t = textures[i];
                if (textureBindings[t.lastBindUnit] != t.Handle)
                {
                    SetActiveTexture(FindUnusedTextureUnit(textures));
                    ForceBindTextureToCurrentUnit(t);
                }
            }

            // Find a texture unit that's not in use by any of the given textures
            int FindUnusedTextureUnit(Texture[] texturesToBind)
            {
                int unit;
                do
                {
                    unit = GetNextBindTextureUnit();
                } while (IsTextureUnitInUse(unit, texturesToBind));
                return unit;
            }

            // Whether a texture unit is currently in use by any of the specified textures
            bool IsTextureUnitInUse(int unit, Texture[] texturesToBind)
            {
                for (int i = 0; i < texturesToBind.Length; i++)
                {
                    Texture t = texturesToBind[i];
                    if (t.lastBindUnit == unit && textureBindings[unit] == t.Handle)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Ensures all of the given textures are bound to a texture unit
        /// </summary>
        /// <param name="textures">The textures to ensure are bound</param>
        public void BindAllTextures(List<Texture> textures)
        {
            if (textures.Count > textureBindings.Length)
                throw new NotSupportedException("You tried to bind more textures at the same time than this system supports");

            for (int i = 0; i < textures.Count; i++)
            {
                Texture t = textures[i];
                if (textureBindings[t.lastBindUnit] != t.Handle)
                {
                    SetActiveTexture(FindUnusedTextureUnit(textures));
                    ForceBindTextureToCurrentUnit(t);
                }
            }

            // Find a texture unit that's not in use by any of the given textures
            int FindUnusedTextureUnit(List<Texture> texturesToBind)
            {
                int unit;
                do
                {
                    unit = GetNextBindTextureUnit();
                } while (IsTextureUnitInUse(unit, texturesToBind));
                return unit;
            }

            // Whether a texture unit is currently in use by any of the specified textures
            bool IsTextureUnitInUse(int unit, List<Texture> texturesToBind)
            {
                for (int i = 0; i < texturesToBind.Count; i++)
                {
                    Texture t = texturesToBind[i];
                    if (t.lastBindUnit == unit && textureBindings[unit] == t.Handle)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Returns whether a texture is the one still bound to it's last bind location
        /// </summary>
        /// <param name="texture">The texture to check if it's bound</param>
        public bool IsTextureBound(Texture texture)
        {
            return textureBindings[texture.lastBindUnit] == texture.Handle;
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

        /// <summary>The handle of the currently bound renderbuffer</summary>
        private int renderbufferHandle;

        /// <summary>
        /// Ensures a framebuffer is bound to a specified target
        /// </summary>
        /// <param name="target">The framebuffer target</param>
        /// <param name="framebuffer">The framebuffer to ensure is bound</param>
        public void BindFramebuffer(FramebufferTarget target, Framebuffer2D framebuffer)
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
        /// Binds a framebuffer to a specified target without first checking whether it's already bound.
        /// </summary>
        /// <param name="target">The framebuffer target</param>
        /// <param name="framebuffer">The framebuffer bind</param>
        internal void ForceBindFramebuffer(FramebufferTarget target, Framebuffer2D framebuffer)
        {
            int handle = framebuffer == null ? 0 : framebuffer.Handle;
            switch (target)
            {
                case FramebufferTarget.DrawFramebuffer:
                    ForceBindFramebufferDraw(handle);
                    break;
                case FramebufferTarget.ReadFramebuffer:
                    ForceBindFramebufferRead(handle);
                    break;
                default:
                    ForceBindFramebuffer(handle);
                    break;
            }
        }

        /// <summary>
        /// Ensures a framebuffer is bound to the draw and read targets
        /// </summary>
        /// <param name="framebuffer">The framebuffer to ensure is bound</param>
        public void BindFramebuffer(Framebuffer2D framebuffer)
        {
            BindFramebuffer(framebuffer == null ? 0 : framebuffer.Handle);
        }

        /// <summary>
        /// Binds a framebuffer to both draw and read targets without first checking whether it's already bound
        /// </summary>
        /// <param name="framebuffer">The framebuffer to bind</param>
        internal void ForceBindFramebuffer(Framebuffer2D framebuffer)
        {
            ForceBindFramebuffer(framebuffer == null ? 0 : framebuffer.Handle);
        }

        /// <summary>
        /// Ensures a framebuffer is bound to the draw target
        /// </summary>
        /// <param name="framebuffer">The framebuffer to ensure is bound</param>
        public void BindFramebufferDraw(Framebuffer2D framebuffer)
        {
            BindFramebufferDraw(framebuffer == null ? 0 : framebuffer.Handle);
        }

        /// <summary>
        /// Binds a framebuffer to the draw target without first checking whether it's already bound
        /// </summary>
        /// <param name="framebuffer">The framebuffer to bind</param>
        public void ForceBindFramebufferDraw(Framebuffer2D framebuffer)
        {
            ForceBindFramebufferDraw(framebuffer == null ? 0 : framebuffer.Handle);
        }

        /// <summary>
        /// Ensures a framebuffer is bound to the read target
        /// </summary>
        /// <param name="framebuffer">The framebuffer to ensure is bound</param>
        public void BindFramebufferRead(Framebuffer2D framebuffer)
        {
            BindFramebufferRead(framebuffer == null ? 0 : framebuffer.Handle);
        }

        /// <summary>
        /// Binds a framebuffer to the read target without first checking whether it's already bound
        /// </summary>
        /// <param name="framebuffer">The framebuffer to bind</param>
        public void ForceBindFramebufferRead(Framebuffer2D framebuffer)
        {
            ForceBindFramebufferRead(framebuffer == null ? 0 : framebuffer.Handle);
        }

        /// <summary>
        /// Ensures a framebuffer is bound to the draw and read targets
        /// </summary>
        /// <param name="handle">The framebuffer.s handle to ensure is bound</param>
        internal void BindFramebuffer(int handle)
        {
            if (framebufferDrawHandle != handle || framebufferReadHandle != handle)
                ForceBindFramebuffer(handle);
        }

        /// <summary>
        /// Binds a framebuffer to both draw and read targets without first checking whether it's already bound
        /// </summary>
        /// <param name="target">The framebuffer target</param>
        /// <param name="handle">The framebuffer's handle bind</param>
        internal void ForceBindFramebuffer(int handle)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);
            framebufferDrawHandle = handle;
            framebufferReadHandle = handle;
        }

        /// <summary>
        /// Ensures a framebuffer is bound to the draw target
        /// </summary>
        /// <param name="handle">The framebuffer.s handle to ensure is bound</param>
        internal void BindFramebufferDraw(int handle)
        {
            if (framebufferDrawHandle != handle)
                ForceBindFramebufferDraw(handle);
        }

        /// <summary>
        /// Binds a framebuffer to the draw target without first checking whether it's already bound
        /// </summary>
        /// <param name="handle">The framebuffer's handle bind</param>
        internal void ForceBindFramebufferDraw(int handle)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, handle);
            framebufferDrawHandle = handle;
        }

        /// <summary>
        /// Ensures a framebuffer is bound to the read target
        /// </summary>
        /// <param name="handle">The framebuffer.s handle to ensure is bound</param>
        internal void BindFramebufferRead(int handle)
        {
            if (framebufferReadHandle != handle)
                ForceBindFramebufferRead(handle);
        }

        /// <summary>
        /// Binds a framebuffer to the read target without first checking whether it's already bound
        /// </summary>
        /// <param name="handle">The framebuffer's handle bind</param>
        internal void ForceBindFramebufferRead(int handle)
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, handle);
            framebufferReadHandle = handle;
        }


        /// <summary>
        /// Ensures a renderbuffer's handle is the currently bound renderbuffer
        /// </summary>
        /// <param name="handle">The renderbuffer's handle to ensure is bound</param>
        internal void BindRenderbuffer(int handle)
        {
            if (renderbufferHandle != handle)
                ForceBindRenderbuffer(handle);
        }

        /// <summary>
        /// Binds a renderbuffer's handle to GL_RENDERBUFFER without first checking whether it's already bound
        /// </summary>
        /// <param name="handle">The renderbuffer's handle to bind</param>
        internal void ForceBindRenderbuffer(int handle)
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, handle);
            renderbufferHandle = handle;
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
            renderbufferHandle = 0;
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

        #region DrawingStates

        #region Viewport
        /// <summary>The current drawing viewport</summary>
        private Rectangle viewport;

        /// <summary>Gets or sets the viewport for drawing</summary>
        public Rectangle Viewport
        {
            get { return viewport; }
            set
            {
                if (value.X != viewport.X || value.Y != viewport.Y || value.Width != viewport.Width || value.Height != viewport.Height)
                {
                    viewport = value;
                    GL.Viewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
                }
            }
        }

        /// <summary>
        /// Sets the current viewport for drawing
        /// </summary>
        /// <param name="x">The viewport's X</param>
        /// <param name="y">The viewport's Y</param>
        /// <param name="width">The viewport's width</param>
        /// <param name="height">The viewport's height</param>
        public void SetViewport(int x, int y, int width, int height)
        {
            if (viewport.X != x || viewport.Y != y || viewport.Width != width || viewport.Height != height)
            {
                viewport.X = x;
                viewport.Y = y;
                viewport.Width = width;
                viewport.Height = height;
                GL.Viewport(x, y, width, height);
            }
        }

        #endregion Viewport

        #region BlendState

        /// <summary>The current blend state</summary>
        private BlendState blendState;

        /// <summary>Gets or sets the blend state for drawing</summary>
        public BlendState BlendState
        {
            get { return blendState; }
            set
            {
                if (blendState.IsOpaque && value.IsOpaque) //Both are opaque? Then setting it again makes no difference...
                    blendState = value; //But we'll still store the value in case somebody wants to read it back I guess
                else
                {
                    if (blendState != value)
                    {
                        if (value.IsOpaque)
                        { 
                            // Is the new state opaque? Then, if the old state wasn't opaque too let's disable blending.
                            if (!blendState.IsOpaque) // If the old state was opaque, then blending is already disabled
                                GL.Disable(EnableCap.Blend);
                        }
                        else
                        {
                            if (blendState.IsOpaque) // If the previous blend state was opaque, then blending is disabled.
                                GL.Enable(EnableCap.Blend); // So we're enable it for the new blend state

                            // We'll ignore comparing these ones... We know there's at least one difference anyway so let's not waste so much time
                            GL.BlendColor(blendState.BlendColor);
                            GL.BlendEquationSeparate(value.EquationModeRGB, value.EquationModeAlpha);
                            GL.BlendFuncSeparate(value.SourceFactorRGB, value.DestFactorRGB, value.SourceFactorAlpha, value.DestFactorAlpha);
                        }
                        blendState = value;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the current blend state as opaque
        /// </summary>
        public void SetBlendStateOpaque()
        {
            if (!blendState.IsOpaque)
            {
                blendState.IsOpaque = true;
                GL.Disable(EnableCap.Blend);
            }
        }

        #endregion BlendState

        #endregion DrawingStates

        /// <summary>
        /// Copies content from one framebuffer to another
        /// </summary>
        /// <param name="src">The framebuffer to copy from</param>
        /// <param name="dst">The framebuffer to copy to</param>
        /// <param name="srcX">The X location of the first pixel to read</param>
        /// <param name="srcY">The Y location of the first pixel to read</param>
        /// <param name="srcWidth">The width of the read rectangle</param>
        /// <param name="srcHeight">The height of the read rectangle</param>
        /// <param name="dstX">The X location of the first pixel to write</param>
        /// <param name="dstY">The Y location of the first pixel to write</param>
        /// <param name="dstWidth">The width of the write rectangle</param>
        /// <param name="dstHeight">The height of the draw rectangle</param>
        /// <param name="mask">What data to copy from the framebuffers</param>
        /// <param name="filter">Whether to use nearest or linear filtering</param>
        public void BlitFramebuffer(Framebuffer2D src, Framebuffer2D dst, int srcX, int srcY, int srcWidth, int srcHeight, int dstX, int dstY, int dstWidth, int dstHeight, ClearBufferMask mask, BlitFramebufferFilter filter)
        {
            // Blit rules:
            // General rectangle correctness rules (src and dst rectangles must be inside the framebuffers' size rectangles)
            // If mask contains Depth or Stencil, filter must be Nearest.
            // Buffers must have same image format?
            // If the framebuffers contain integer format, filter must be Nearest.
            // The blit fails if any of the following conditions about samples is true:
            //    1. Both framebuffers have different amount of samples and one of them isn't 0
            //    2. Condition 1 is true and the width and height of the src and dst rectangles don't match

            if (srcWidth <= 0 || srcWidth > src.Width)
                throw new ArgumentOutOfRangeException("srcWidth", srcWidth, "srcWidth must be in the range (0, src.Width]");

            if (srcHeight <= 0 || srcHeight > src.Height)
                throw new ArgumentOutOfRangeException("srcHeight", srcHeight, "srcHeight must be in the range (0, src.Height]");

            if (srcX < 0 || srcX > src.Width - srcWidth)
                throw new ArgumentOutOfRangeException("srcX", srcX, "srcX must be in the range [0, src.Width-srcWidth)");

            if (srcY < 0 || srcY > src.Height - srcHeight)
                throw new ArgumentOutOfRangeException("srcY", srcY, "srcY must be in the range [0, src.Height-srcHeight)");

            if (dstWidth <= 0 || dstWidth > dst.Width)
                throw new ArgumentOutOfRangeException("dstWidth", dstWidth, "dstWidth must be in the range (0, dst.Width]");

            if (dstHeight <= 0 || dstHeight > dst.Height)
                throw new ArgumentOutOfRangeException("dstHeight", dstHeight, "dstHeight must be in the range (0, dst.Height]");

            if (dstX < 0 || dstX > dst.Width - dstWidth)
                throw new ArgumentOutOfRangeException("dstX", dstX, "dstX must be in the range [0, dst.Width-dstWidth)");

            if (dstY < 0 || dstY > dst.Height - dstHeight)
                throw new ArgumentOutOfRangeException("dstY", dstY, "dstY must be in the range [0, dst.Height-dstHeight)");

            if (src.Texture.ImageFormat != dst.Texture.ImageFormat)
                throw new InvalidBlitException("You can't blit between framebuffers with different image formats");

            if ((mask & ClearBufferMask.ColorBufferBit) == ClearBufferMask.ColorBufferBit && (TrippyUtils.IsImageFormatIntegerType(src.Texture.ImageFormat) && filter != BlitFramebufferFilter.Nearest))
                throw new InvalidBlitException("When blitting with color with integer formats, you must use a nearest filter");

            if (((mask & ClearBufferMask.DepthBufferBit) | (mask & ClearBufferMask.StencilBufferBit)) != 0 && filter != BlitFramebufferFilter.Nearest)
                throw new InvalidBlitException("When using depth or stencil, the filter must be Nearest");

            //TODO: If blitting with depth mask, ensure both have depth. If blitting with stencil mask, ensure both have stencil, etc.
            
            bool areSameSize = srcWidth == dstWidth && srcHeight == dstHeight;
            
            if (src.Samples == dst.Samples)
            {
                if (src.Samples != 0 && !areSameSize)
                    throw new InvalidBlitException("When blitting between multisampled framebuffers, the src and dst rectangle sizes must match");
            }
            else //then src.Samples != dst.Samples
            {
                // We're blitting framebuffers with different amounts of samples, this can be problematic
                if (src.Samples * dst.Samples != 0) // None of the samples are 0 yet they aren't equal. This is invalid
                    throw new InvalidBlitException("You can't blit between framebuffers with different sample counts");

                if (!areSameSize)
                    throw new InvalidBlitException("The sizes of both framebuffers must be the same when using different sample counts");
            }

            // Holy unbelievable fuck those were A LOT of checks for a godfucken blit

            BindFramebufferRead(src);
            BindFramebufferDraw(dst);
            GL.BlitFramebuffer(srcX, srcY, srcWidth, srcHeight, dstX, dstY, dstWidth, dstHeight, mask, filter);
        }

        /// <summary>
        /// Copies content from one framebuffer to another
        /// </summary>
        /// <param name="src">The framebuffer to copy from</param>
        /// <param name="dst">The framebuffer to copy to</param>
        /// <param name="srcRect">The source rectangle to copy from</param>
        /// <param name="dstRect">The destination rectangle to write to</param>
        /// <param name="mask">What data to copy from the framebuffers</param>
        /// <param name="filter">Whether to use nearest or linear filtering</param>
        public void BlitFramebuffer(Framebuffer2D src, Framebuffer2D dst, Rectangle srcRect, Rectangle dstRect, ClearBufferMask mask, BlitFramebufferFilter filter)
        {
            BlitFramebuffer(src, dst, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, mask, filter);
        }

        /// <summary>
        /// Removes a GraphicsResource from it's GraphicsDevice and makes it belong to this GraphicsDevice.
        /// </summary>
        /// <param name="resource">The resource to pass over</param>
        public void MakeMine(GraphicsResource resource)
        {
            resource.GraphicsDevice = this;
        }

        /// <summary>
        /// Disposes this GraphicsDevice, it's GraphicsResource and it's context.
        /// The GraphicsDevice nor it's resources can be used once it's been disposed
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Context.Dispose();
                //TODO: dispose the GraphicResource-s. This is gonna need a list somewhere and it might be a bit ugly
            }
        }
    }
}
