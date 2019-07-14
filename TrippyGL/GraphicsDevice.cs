using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public delegate void GLDebugMessage(DebugSource debugSource, DebugType debugType, int messageId, DebugSeverity debugSeverity, string message);

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
            Context = context;

            InitGLGetVariables();

            InitBufferObjectStates();
            ForceBindVertexArray(null);
            ForceUseShaderProgram(null);
            InitTextureStates();
            drawFramebuffer = null;
            readFramebuffer = null;
            renderbuffer = null;

            blendState = BlendState.Opaque;
        }

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

            GL.ClearColor(clearColor);

            GL.Viewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);

            GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
            if (scissorTestEnabled)
                GL.Enable(EnableCap.ScissorTest);
            else
                GL.Disable(EnableCap.ScissorTest);

            if (blendState.IsOpaque)
                GL.Disable(EnableCap.Blend);
            else
                GL.Enable(EnableCap.Blend);
            GL.BlendFuncSeparate(blendState.SourceFactorRGB, blendState.DestFactorRGB, blendState.SourceFactorAlpha, blendState.DestFactorAlpha);
            GL.BlendEquationSeparate(blendState.EquationModeRGB, blendState.EquationModeAlpha);
            GL.BlendColor(blendState.BlendColor);

            if (depthState.DepthTestingEnabled)
                GL.Enable(EnableCap.DepthTest);
            else
                GL.Disable(EnableCap.DepthTest);

            if (cubemapSeamlessEnabled)
                GL.Enable(EnableCap.TextureCubeMapSeamless);
            else
                GL.Disable(EnableCap.TextureCubeMapSeamless);

            if (faceCullingEnabled)
                GL.Enable(EnableCap.CullFace);
            else
                GL.Disable(EnableCap.CullFace);
            GL.CullFace(cullFaceMode);

        }

        #region DebugMessaging

        private bool debugMessagingEnabled = false;

        /// <summary>Whether OpenGL message debugging is enabled (using the KHR_debug extension or v4.3)</summary>
        public bool DebugMessagingEnabled
        {
            get { return debugMessagingEnabled; }
            set
            {
                if (value)
                {
                    if (!debugMessagingEnabled)
                    {
                        GL.Enable(EnableCap.DebugOutput);
                        GL.Enable(EnableCap.DebugOutputSynchronous);
                        debugProcDelegate = OnDebugMessageRecieved;
                        debugProcDelegateHandle = GCHandle.Alloc(debugProcDelegate);
                        GL.DebugMessageCallback(debugProcDelegate, IntPtr.Zero);
                        debugMessagingEnabled = true;
                    }
                }
                else if (debugMessagingEnabled)
                {
                    GL.Disable(EnableCap.DebugOutput);
                    GL.Disable(EnableCap.DebugOutputSynchronous);
                    debugMessagingEnabled = false;
                    debugProcDelegate = null;
                    debugProcDelegateHandle.Free();
                }
            }
        }

        /// <summary>An event for recieving OpenGL debug messages. Debug messaging must be enabled for this to work</summary>
        public event GLDebugMessage DebugMessage;

        /// <summary>If we don't store this delegate it gets garbage collected and dies and omg that's so sad alexa play despacito</summary>
        private DebugProc debugProcDelegate;
        private GCHandle debugProcDelegateHandle;

        private void OnDebugMessageRecieved(DebugSource src, DebugType type, int id, DebugSeverity sev, int length, IntPtr msg, IntPtr param)
        {
            DebugMessage?.Invoke(src, type, id, sev, Marshal.PtrToStringAnsi(msg));
        }

        #endregion DebugMessaging

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
            MaxVertexAttribs = GL.GetInteger(GetPName.MaxVertexAttribs);
            MaxArrayTextureLayers = GL.GetInteger(GetPName.MaxArrayTextureLayers);
            MaxFramebufferColorAttachments = GL.GetInteger(GetPName.MaxColorAttachments);
            MaxDrawBuffers = GL.GetInteger(GetPName.MaxDrawBuffers);
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

        public int MaxVertexAttribs { get; private set; }

        public int MaxArrayTextureLayers { get; private set; }

        public int MaxFramebufferColorAttachments { get; private set; }

        public int MaxDrawBuffers { get; private set; }

        public string GLVersion { get { return GL.GetString(StringName.Version); } }

        public string GLVendor { get { return GL.GetString(StringName.Vendor); } }

        public string GLRenderer { get { return GL.GetString(StringName.Renderer); } }

        public string GLShadingLanguageVersion { get { return GL.GetString(StringName.ShadingLanguageVersion); } }

        #endregion GLGet

        #region BindingStates

        #region BufferObjectBindingStates

        /// <summary>This constant defines the total amount of buffer targets. This defines the array sizes for the bufferBindings and bufferBindingTargets arrays</summary>
        private const int BufferTargetCount = 13;

        internal const BufferTarget DefaultBufferTarget = BufferTarget.ArrayBuffer;
        private int defaultBufferTargetBindingIndex;

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
                //BufferTarget.ElementArrayBuffer, //This target is stored within a Vertex Array Object, so a VertexArray object takes care of binding these
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
            defaultBufferTargetBindingIndex = GetBindingTargetIndex(DefaultBufferTarget);

            bufferRangeBindings = new BufferRangeBinding[4][];

            bufferRangeBindings[0] = new BufferRangeBinding[GL.GetInteger(GetPName.MaxTransformFeedbackBuffers)];
            bufferRangeBindings[1] = new BufferRangeBinding[GL.GetInteger(GetPName.MaxUniformBufferBindings)];
            bufferRangeBindings[2] = new BufferRangeBinding[GL.GetInteger((GetPName)All.MaxShaderStorageBufferBindings)]; //opentk wtf
            bufferRangeBindings[3] = new BufferRangeBinding[GL.GetInteger((GetPName)All.MaxAtomicCounterBufferBindings)];
        }

        /// <summary>
        /// Ensures a buffer is bound to the default binding location by binding it if it's not
        /// </summary>
        /// <param name="buffer">The buffer to ensure is bound. This value is assumed not to be null</param>
        internal void BindBufferObject(BufferObject buffer)
        {
            if (bufferBindings[defaultBufferTargetBindingIndex] != buffer.Handle)
                ForceBindBufferObject(buffer);
        }
        
        /// <summary>
        /// Binds a buffer to the default binding location without first checking whether it's already bound
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null</param>
        internal void ForceBindBufferObject(BufferObject buffer)
        {
            GL.BindBuffer(DefaultBufferTarget, buffer.Handle);
            bufferBindings[defaultBufferTargetBindingIndex] = buffer.Handle;
        }

        /// <summary>
        /// Ensures a buffer subset is bound to it's BufferTarget by binding it if it's not
        /// </summary>
        /// <param name="bufferSubset">The buffer subset to ensure is bound. This value is assumed not to be null</param>
        public void BindBuffer(BufferObjectSubset bufferSubset)
        {
            if (bufferBindings[bufferSubset.bufferTargetBindingIndex] != bufferSubset.BufferHandle)
                ForceBindBuffer(bufferSubset);
        }

        /// <summary>
        /// Binds a buffer subset to it's BufferTarget without first checking whether it's already bound.
        /// </summary>
        /// <param name="bufferSubset">The buffer subset to bind. This value is assumed not to be null</param>
        internal void ForceBindBuffer(BufferObjectSubset bufferSubset)
        {
            GL.BindBuffer(bufferSubset.BufferTarget, bufferSubset.BufferHandle);
            bufferBindings[bufferSubset.bufferTargetBindingIndex] = bufferSubset.BufferHandle;
        }

        /// <summary>
        /// Ensures a buffer subset is bound to a specified binding index in it's BufferTarget by binding it if it's not.
        /// The buffer subset's BufferTarget must be one with multiple binding indexes
        /// </summary>
        /// <param name="bufferSubset">The buffer subset to ensure is bound. This value is assumed not to be null</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer should be bound</param>
        public void BindBufferBase(BufferObjectSubset bufferSubset, int bindingIndex)
        {
            BufferRangeBinding b = bufferRangeBindings[bufferSubset.bufferTargetBindingIndex][bindingIndex];
            if (b.BufferHandle != bufferSubset.BufferHandle || b.Size != bufferSubset.StorageLengthInBytes || b.Offset != 0)
                ForceBindBufferBase(bufferSubset, bindingIndex);
        }

        /// <summary>
        /// Bind a buffer subset to a binding index on it's BufferTarget without first checking whether it's already bound.
        /// The buffer subset's BufferTarget must be one with multiple binding indexes
        /// </summary>
        /// <param name="bufferSubset">The buffer subset to bind. This value is assumed not to be null</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer will be bound</param>
        internal void ForceBindBufferBase(BufferObjectSubset bufferSubset, int bindingIndex)
        {
            GL.BindBufferBase((BufferRangeTarget)bufferSubset.BufferTarget, bindingIndex, bufferSubset.BufferHandle);
            bufferBindings[bufferSubset.bufferTargetBindingIndex] = bufferSubset.BufferHandle;
            bufferRangeBindings[bufferSubset.bufferTargetBindingIndex][bindingIndex].SetBase(bufferSubset);
        }

        /// <summary>
        /// Ensures a buffer subset's range is bound to a specified binding index in it's BufferTarget by binding it if it's not.
        /// The buffer subset's BufferTarget must be one with multiple binding indexes
        /// </summary>
        /// <param name="bufferSubset">The buffer subset to bind. This value is assumed not to be null</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer will be bound</param>
        /// <param name="offset">The offset in bytes into the buffer subset's storage where the bind begins</param>
        /// <param name="size">The amount of bytes that can be read from the storage, starting from offset</param>
        public void BindBufferRange(BufferObjectSubset bufferSubset, int bindingIndex, int offset, int size)
        {
            BufferRangeBinding b = bufferRangeBindings[bufferSubset.bufferTargetBindingIndex][bindingIndex];
            if (b.BufferHandle != bufferSubset.BufferHandle || b.Size != size || b.Offset != offset + bufferSubset.StorageOffsetInBytes)
                ForceBindBufferRange(bufferSubset, bindingIndex, offset, size);
        }

        /// <summary>
        /// Bind a range of a buffer to a binding index on it's BufferTarget without first checking whether it's already bound.
        /// The buffer object's BufferTarget must be one with multiple binding indexes
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer will be bound</param>
        /// <param name="offset">The offset in bytes into the buffer's storage where the bind begins</param>
        /// <param name="size">The amount of bytes that can be read from the storage, starting from offset</param>
        internal void ForceBindBufferRange(BufferObjectSubset buffer, int bindingIndex, int offset, int size)
        {
            offset += buffer.StorageOffsetInBytes;
            GL.BindBufferRange((BufferRangeTarget)buffer.BufferTarget, bindingIndex, buffer.BufferHandle, (IntPtr)offset, size);
            bufferBindings[buffer.bufferTargetBindingIndex] = buffer.BufferHandle;
            bufferRangeBindings[buffer.bufferTargetBindingIndex][bindingIndex].SetRange(buffer, offset, size);
        }

        /// <summary>
        /// Returns whether the given buffer subset is the currently bound one for it's BufferTarget
        /// </summary>
        /// <param name="buffer">The buffer subset to check. This value is assumed not to be null</param>
        public bool IsBufferCurrentlyBound(BufferObjectSubset buffer)
        {
            return bufferBindings[buffer.bufferTargetBindingIndex] == buffer.BufferHandle;
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

        private VertexArray vertexArray;

        /// <summary>Gets or sets (binds) the currently bound VertexArray</summary>
        public VertexArray VertexArray
        {
            get { return vertexArray; }
            set
            {
                if(vertexArray != value)
                {
                    GL.BindVertexArray(value == null ? 0 : value.Handle);
                    vertexArray = value;
                }
            }
        }

        /// <summary>
        /// Binds a vertex array without first checking whether it's already bound
        /// </summary>
        /// <param name="array">The array to bind</param>
        internal void ForceBindVertexArray(VertexArray array)
        {
            GL.BindVertexArray(array == null ? 0 : array.Handle);
            vertexArray = array;
        }

        /// <summary>
        /// Resets all saved states for vertex arrays This is, the variables used to check whether to bind a vertex array or not.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions
        /// </summary>
        public void ResetVertexArrayStates()
        {
            GL.BindVertexArray(0);
            vertexArray = null;
        }

        #endregion VertexArrayBindingStates

        #region ShaderProgramBindingStates

        /// <summary>The currently bound ShaderProgram's handle</summary>
        private ShaderProgram shaderProgram;

        /// <summary>Gets or sets (binds) the currently bound ShaderProgram</summary>
        public ShaderProgram ShaderProgram
        {
            get { return shaderProgram; }
            set
            {
                if(shaderProgram != value)
                {
                    GL.UseProgram(value == null ? 0 : value.Handle);
                    shaderProgram = value;
                }
            }
        }

        /// <summary>
        /// Installs the given program into the rendering pipeline without first checking whether it's already in use
        /// </summary>
        /// <param name="program">The shader program to use</param>
        public void ForceUseShaderProgram(ShaderProgram program)
        {
            GL.UseProgram(program == null ? 0 : program.Handle);
            shaderProgram = program;
        }

        /// <summary>
        /// Resets all saved states for shader programs. This is, the variables used to check whether to use a shader program or not.
        /// You should only need to call this when itneroperating with other libraries or using your own GL functions
        /// </summary>
        public void ResetShaderProgramStates()
        {
            GL.UseProgram(0);
            shaderProgram = null;
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

        private FramebufferObject drawFramebuffer;
        private FramebufferObject readFramebuffer;
        private RenderbufferObject renderbuffer;

        /// <summary>Gets or sets (binds) the framebuffer currently bound for drawing</summary>
        public FramebufferObject DrawFramebuffer
        {
            get { return drawFramebuffer; }
            set
            {
                if (drawFramebuffer != value)
                {
                    GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, value == null ? 0 : value.Handle);
                    drawFramebuffer = value;
                }
            }
        }

        /// <summary>Gets or sets (binds) the framebuffer currently bound for reading</summary>
        public FramebufferObject ReadFramebuffer
        {
            get { return readFramebuffer; }
            set
            {
                if (readFramebuffer != value)
                {
                    GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, value == null ? 0 : value.Handle);
                    readFramebuffer = value;
                }
            }
        }

        /// <summary>Sets (binds) a framebuffer for both drawing and reading</summary>
        public FramebufferObject Framebuffer
        {
            set
            {
                if (readFramebuffer != value || drawFramebuffer != value)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, value == null ? 0 : value.Handle);
                    drawFramebuffer = value;
                    readFramebuffer = value;
                }
            }
        }

        /// <summary>Gets or sets (binds) the current renderbuffer</summary>
        public RenderbufferObject Renderbuffer
        {
            get { return renderbuffer; }
            set
            {
                if(renderbuffer != value)
                {
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, value == null ? 0 : value.Handle);
                    renderbuffer = value;
                }
            }
        }
        
        /// <summary>
        /// Binds a framebuffer for drawing without first checking whether it's already bound
        /// </summary>
        /// <param name="framebuffer">The framebuffer to bind</param>
        internal void ForceBindDrawFramebuffer(FramebufferObject framebuffer)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, framebuffer == null ? 0 : framebuffer.Handle);
            drawFramebuffer = framebuffer;
        }

        /// <summary>
        /// Binds a framebuffer for reading without first checking whether it's already bound
        /// </summary>
        /// <param name="framebuffer">The framebuffer to bind</param>
        internal void ForceBindReadFramebuffer(FramebufferObject framebuffer)
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, framebuffer == null ? 0 : framebuffer.Handle);
            readFramebuffer = framebuffer;
        }

        /// <summary>
        /// Binds a renderbuffer without first checking whether it's already bound
        /// </summary>
        /// <param name="renderbuffer">The renderbuffer to bind</param>
        internal void ForceBindRenderbuffer(RenderbufferObject renderbuffer)
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer.Handle);
            this.renderbuffer = renderbuffer;
        }

        /// <summary>
        /// Resets all saved states for FramebufferObjects. This is, the variables used to check whether to use a shader program or not.
        /// You should only need to call this when itneroperating with other libraries or using your own GL functions
        /// </summary>
        public void ResetFramebufferStates()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            drawFramebuffer = null;
            readFramebuffer = null;
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            renderbuffer = null;
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
                BufferHandle = 0;
                Offset = 0;
                Size = 0;
            }

            /// <summary>
            /// Set the values of this BufferRangeBinding as for when glBindBufferBase was called
            /// </summary>
            public void SetBase(BufferObjectSubset buffer)
            {
                BufferHandle = buffer.BufferHandle;
                Offset = 0;
                Size = buffer.StorageLengthInBytes;
            }

            /// <summary>
            /// Set the values of the BufferRangeBinding
            /// </summary>
            public void SetRange(BufferObjectSubset buffer, int offset, int size)
            {
                BufferHandle = buffer.BufferHandle;
                Offset = offset;
                Size = size;
            }
        }

        #endregion BindingStates

        #region DrawingStates

        #region ClearColor

        /// <summary>The current clear color</summary>
        private Color4 clearColor;

        /// <summary>
        /// Gets or sets the current color to use on clear operations
        /// </summary>
        public Color4 ClearColor
        {
            get { return clearColor; }
            set
            {
                if (clearColor != value)
                {
                    GL.ClearColor(value);
                    clearColor = value;
                }
            }
        }
        #endregion

        #region Viewport
        /// <summary>The current drawing viewport</summary>
        private Rectangle viewport;

        /// <summary>Gets or sets the viewport for drawing</summary>
        public Rectangle Viewport
        {
            get { return viewport; } //The get is OK because Rectangle is a struct so no worries about modifying it
            set
            {
                if (value != viewport)
                {
                    GL.Viewport(value.X, value.Y, value.Width, value.Height);
                    viewport = value;
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

        #region ScissorTest

        /// <summary>Whether scissor testing is currently enabled</summary>
        private bool scissorTestEnabled = false;

        /// <summary>The current scissor rectangle</summary>
        private Rectangle scissorRect;

        /// <summary>Gets or sets whether scissor testing is enable</summary>
        public bool ScissorTestEnabled
        {
            get { return scissorTestEnabled; }
            set
            {
                if (scissorTestEnabled != value)
                {
                    if (value)
                        GL.Enable(EnableCap.ScissorTest);
                    else
                        GL.Disable(EnableCap.ScissorTest);
                    scissorTestEnabled = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the scissor rectangle that discards fragments rendered outside it
        /// </summary>
        public Rectangle ScissorRectangle
        {
            get { return scissorRect; }
            set
            {
                if (scissorRect != value)
                {
                    if (value.Width < 0 || value.Height < 0)
                        throw new ArgumentOutOfRangeException("ScissorRectangle Width and Height must be greater or equal to 0");

                    GL.Scissor(value.X, value.Y, value.Width, value.Height);
                    scissorRect = value;
                }
            }
        }

        #endregion

        #region BlendState

        /// <summary>The current blend state</summary>
        private BlendState blendState = BlendState.Opaque;

        /// <summary>Gets or sets the blend state for drawing</summary>
        public BlendState BlendState
        {
            set
            {
                // The specified BlendState's fields are copied into blendState, because we need to store all the
                // fields of a BlendState but if we save the same BlendState class instance, the user can modify these!

                if (!(blendState.IsOpaque && value.IsOpaque)) //if the current and the new blend state are both opaque... Do nothing
                {
                    // Either the current or new blend state is not opaque
                    if (value.IsOpaque) //blendState.IsOpaque must therefore be false
                    {
                        GL.Disable(EnableCap.Blend);
                        blendState.IsOpaque = true;
                        // If blending is opaque, all other blending parameters don't matter.
                    }
                    else //blendState.IsOpaque must therefore be true
                    {
                        if (blendState.IsOpaque)
                        {
                            GL.Enable(EnableCap.Blend);
                            blendState.IsOpaque = false;
                        }

                        if (blendState.EquationModeRGB != value.EquationModeRGB || blendState.EquationModeAlpha != value.EquationModeAlpha)
                        {
                            GL.BlendEquationSeparate(value.EquationModeRGB, value.EquationModeAlpha);
                            blendState.EquationModeRGB = value.EquationModeRGB;
                            blendState.EquationModeAlpha = value.EquationModeAlpha;
                        }

                        if (blendState.SourceFactorRGB != value.SourceFactorRGB || blendState.SourceFactorAlpha != value.SourceFactorAlpha || blendState.DestFactorRGB != value.DestFactorRGB || blendState.DestFactorAlpha != value.DestFactorAlpha)
                        {
                            GL.BlendFuncSeparate(value.SourceFactorRGB, value.DestFactorRGB, value.SourceFactorAlpha, value.DestFactorAlpha);
                            blendState.SourceFactorRGB = value.SourceFactorRGB;
                            blendState.SourceFactorAlpha = value.SourceFactorAlpha;
                            blendState.DestFactorRGB = value.DestFactorRGB;
                            blendState.DestFactorAlpha = value.DestFactorAlpha;
                        }

                        if (blendState.BlendColor != value.BlendColor)
                        {
                            GL.BlendColor(value.BlendColor);
                            blendState.BlendColor = value.BlendColor;
                        }
                    }
                }

                /*if (!(blendState.IsOpaque && value.IsOpaque)) //if the current and new blend state are both opaque... Do nothing
                {
                    if (blendState != value)
                    {
                        if (value.IsOpaque)
                        {
                            // Is the new state opaque? Then, if the old state wasn't opaque too let's disable blending.
                            if (!blendState.IsOpaque)
                            { // If the old state was opaque, then blending is already disabled
                                GL.Disable(EnableCap.Blend);
                                blendState.IsOpaque = true;
                            }
                        }
                        else
                        {
                            if (blendState.IsOpaque) // If the previous blend state was opaque, then blending is disabled.
                                GL.Enable(EnableCap.Blend); // So we're enable it for the new blend state

                            // We'll ignore comparing these ones... We know there's at least one difference anyway so let's not waste so much time
                            GL.BlendColor(blendState.BlendColor);
                            GL.BlendEquationSeparate(value.EquationModeRGB, value.EquationModeAlpha);
                            GL.BlendFuncSeparate(value.SourceFactorRGB, value.DestFactorRGB, value.SourceFactorAlpha, value.DestFactorAlpha);
                            blendState.CopyValuesFrom(value);
                        }
                    }
                }*/
            }
            get
            {
                return new BlendState(blendState);
            }
        }

        /// <summary>Enables or disables color blending</summary>
        public bool BlendingEnabled
        {
            get { return !blendState.IsOpaque; }
            set
            {
                if (blendState.IsOpaque == value)
                {
                    if (value)
                        GL.Enable(EnableCap.Blend);
                    else
                        GL.Disable(EnableCap.Blend);
                    blendState.IsOpaque = !value;
                }
            }
        }

        #endregion BlendState

        #region DepthTestingState

        /// <summary>The current depth state</summary>
        private DepthTestingState depthState = new DepthTestingState(false);

        /// <summary>Sets the current depth testing state</summary>
        public DepthTestingState DepthState
        {
            set
            {
                if (value.DepthTestingEnabled)
                {
                    if (!depthState.DepthTestingEnabled)
                    {
                        GL.Enable(EnableCap.DepthTest);
                        depthState.DepthTestingEnabled = true;
                    }

                    if (depthState.DepthComparison != value.DepthComparison)
                    {
                        GL.DepthFunc(value.DepthComparison);
                        depthState.DepthComparison = value.DepthComparison;
                    }

                    if (depthState.ClearDepth != value.ClearDepth)
                    {
                        GL.ClearDepth(value.ClearDepth);
                        depthState.ClearDepth = value.ClearDepth;
                    }

                    if (depthState.depthNear != value.depthNear || depthState.depthFar != value.depthFar)
                    {
                        GL.DepthRange(value.depthNear, value.depthFar);
                        depthState.depthNear = value.depthNear;
                        depthState.depthFar = value.depthFar;
                    }

                    if (depthState.DepthBufferWrittingEnabled != value.DepthBufferWrittingEnabled)
                    {
                        GL.DepthMask(value.DepthBufferWrittingEnabled);
                        depthState.DepthBufferWrittingEnabled = value.DepthBufferWrittingEnabled;
                    }
                }
                else if (depthState.DepthTestingEnabled) // value.DepthTestingEnabled is false
                {
                    GL.Disable(EnableCap.DepthTest);
                    depthState.DepthTestingEnabled = false;
                }
            }
            get
            {
                return new DepthTestingState(depthState);
            }
        }

        /// <summary>Enables or disables depth testing</summary>
        public bool DepthTestingEnabled
        {
            get { return depthState.DepthTestingEnabled; }
            set
            {
                if (depthState.DepthTestingEnabled != value)
                {
                    if (value)
                        GL.Enable(EnableCap.DepthTest);
                    else
                        GL.Disable(EnableCap.DepthTest);
                    depthState.DepthTestingEnabled = value;
                }
            }
        }

        /// <summary>The current depth to set on a clear depth operation</summary>
        public float ClearDepth
        {
            get { return depthState.ClearDepth; }
            set
            {
                if(depthState.ClearDepth != value)
                {
                    GL.ClearDepth(value);
                    depthState.ClearDepth = value;
                }
            }
        }

        #endregion

        #region Misc

        private bool cubemapSeamlessEnabled = false;

        /// <summary>Enables or disables seamless sampling across cubemap faces</summary>
        public bool TextureCubemapSeamlessEnabled
        {
            get { return cubemapSeamlessEnabled; }
            set
            {
                if (cubemapSeamlessEnabled != value)
                {
                    if (value)
                        GL.Enable(EnableCap.TextureCubeMapSeamless);
                    else
                        GL.Disable(EnableCap.TextureCubeMapSeamless);
                    cubemapSeamlessEnabled = value;
                }
            }
        }

        private bool faceCullingEnabled = false;
        private CullFaceMode cullFaceMode = CullFaceMode.Back;

        /// <summary>Enables or disables culling polygon faces</summary>
        public bool FaceCullingEnabled
        {
            get { return faceCullingEnabled; }
            set
            {
                if (faceCullingEnabled != value)
                {
                    if (value)
                        GL.Enable(EnableCap.CullFace);
                    else
                        GL.Disable(EnableCap.CullFace);
                    faceCullingEnabled = value;
                }
            }
        }

        /// <summary>Sets the face culling mode to use when face culling is enabled</summary>
        public CullFaceMode CullFaceMode
        {
            get { return cullFaceMode; }
            set
            {
                if(cullFaceMode != value)
                {
                    GL.CullFace(cullFaceMode);
                    cullFaceMode = value;
                }
            }
        }

        #endregion

        #endregion DrawingStates

        #region DrawingFunctions

        /// <summary>
        /// Clears the current framebuffer to the specified color
        /// </summary>
        /// <param name="mask">The masks indicating the values to clear, combined using bitwise OR</param>
        public void Clear(ClearBufferMask mask)
        {
            GL.Clear(mask);
        }

        /// <summary>
        /// Renders primitive data
        /// </summary>
        /// <param name="primitiveType">The type of primitive to render</param>
        /// <param name="startIndex">The index of the first vertex to render</param>
        /// <param name="count">The amount of vertices to render</param>
        public void DrawArrays(PrimitiveType primitiveType, int startIndex, int count)
        {
            GL.DrawArrays(primitiveType, startIndex, count);
        }

        /// <summary>
        /// Renders indexed primitive data
        /// </summary>
        /// <param name="type">The type of primitive to render</param>
        /// <param name="startIndex">The index of the first element to render</param>
        /// <param name="count">The amount of elements to render</param>
        public void DrawElements(PrimitiveType type, int startIndex, int count)
        {
            IndexBufferSubset indexSubset = vertexArray.IndexBuffer;
            GL.DrawElements(type, count, indexSubset.ElementType, indexSubset.StorageOffsetInBytes + startIndex * indexSubset.ElementSize);
        }

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
        public void BlitFramebuffer(FramebufferObject src, FramebufferObject dst, int srcX, int srcY, int srcWidth, int srcHeight, int dstX, int dstY, int dstWidth, int dstHeight, ClearBufferMask mask, BlitFramebufferFilter filter)
        {
            // Blit rules:
            // General rectangle correctness rules (src and dst rectangles must be inside the framebuffers' size rectangles)
            // If mask contains Depth or Stencil, filter must be Nearest.
            // Buffers must have same image format?
            // If the framebuffers contain integer format, filter must be Nearest.
            // The blit fails if any of the following conditions about samples is true:
            //    1. Both framebuffers have different amount of samples and one of them isn't 0
            //    2. Condition 1 is true and the width and height of the src and dst rectangles don't match

            if (src != null)
            {
                if (srcWidth <= 0 || srcWidth > src.Width)
                    throw new ArgumentOutOfRangeException("srcWidth", srcWidth, "srcWidth must be in the range (0, src.Width]");

                if (srcHeight <= 0 || srcHeight > src.Height)
                    throw new ArgumentOutOfRangeException("srcHeight", srcHeight, "srcHeight must be in the range (0, src.Height]");

                if (srcX < 0 || srcX > src.Width - srcWidth)
                    throw new ArgumentOutOfRangeException("srcX", srcX, "srcX must be in the range [0, src.Width-srcWidth)");

                if (srcY < 0 || srcY > src.Height - srcHeight)
                    throw new ArgumentOutOfRangeException("srcY", srcY, "srcY must be in the range [0, src.Height-srcHeight)");
            }

            if (dst != null)
            {
                if (dstWidth <= 0 || dstWidth > dst.Width)
                    throw new ArgumentOutOfRangeException("dstWidth", dstWidth, "dstWidth must be in the range (0, dst.Width]");

                if (dstHeight <= 0 || dstHeight > dst.Height)
                    throw new ArgumentOutOfRangeException("dstHeight", dstHeight, "dstHeight must be in the range (0, dst.Height]");

                if (dstX < 0 || dstX > dst.Width - dstWidth)
                    throw new ArgumentOutOfRangeException("dstX", dstX, "dstX must be in the range [0, dst.Width-dstWidth)");

                if (dstY < 0 || dstY > dst.Height - dstHeight)
                    throw new ArgumentOutOfRangeException("dstY", dstY, "dstY must be in the range [0, dst.Height-dstHeight)");
            }

            //if (src.Texture.ImageFormat != dst.Texture.ImageFormat)
            //    throw new InvalidBlitException("You can't blit between framebuffers with different image formats");

            //if ((mask & ClearBufferMask.ColorBufferBit) == ClearBufferMask.ColorBufferBit && (TrippyUtils.IsImageFormatIntegerType(src.Texture.ImageFormat) && filter != BlitFramebufferFilter.Nearest))
            //    throw new InvalidBlitException("When blitting with color with integer formats, you must use a nearest filter");

            if (((mask & ClearBufferMask.DepthBufferBit) | (mask & ClearBufferMask.StencilBufferBit)) != 0 && filter != BlitFramebufferFilter.Nearest)
                throw new InvalidBlitException("When using depth or stencil, the filter must be Nearest");

            //TODO: If blitting with depth mask, ensure both have depth. If blitting with stencil mask, ensure both have stencil, etc.

            /*bool areSameSize = srcWidth == dstWidth && srcHeight == dstHeight;
            
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
            }*/ //alright this needs rewritting

            // Holy unbelievable fuck those were A LOT of checks for a godfucken blit

            ReadFramebuffer = src;
            DrawFramebuffer = dst;
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
        public void BlitFramebuffer(FramebufferObject src, FramebufferObject dst, Rectangle srcRect, Rectangle dstRect, ClearBufferMask mask, BlitFramebufferFilter filter)
        {
            BlitFramebuffer(src, dst, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, mask, filter);
        }

        #endregion DrawingFunctions

        /// <summary>
        /// Removes a GraphicsResource from it's GraphicsDevice and makes it belong to this GraphicsDevice.
        /// </summary>
        /// <param name="resource">The resource to pass over</param>
        public void MakeMine(GraphicsResource resource)
        {
            resource.GraphicsDevice = this;
        }

        /// <summary>
        /// This is called by GraphicsResource-s on Dispose()
        /// </summary>
        /// <param name="disposedResource">The graphics resource that was just disposed</param>
        internal void OnResourceDisposed(GraphicsResource disposedResource)
        {

        }

        /// <summary>
        /// Disposes this GraphicsDevice, it's GraphicsResource and it's context.
        /// The GraphicsDevice nor it's resources can be used once it's been disposed
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                DebugMessagingEnabled = false; // this makes sure any GCHandle or unmanaged stuff gets released
                IsDisposed = true;
                Context.Dispose();
                //TODO: dispose the GraphicResource-s. This is gonna need a list somewhere and it might be a bit ugly
            }
        }
    }
}
