using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Represents the method that will handle an OpenGL debug message event.
    /// </summary>
    /// <param name="debugSource">Where the message originated from.</param>
    /// <param name="debugType">The type of message.</param>
    /// <param name="messageId">An identifier of the message (same messages have same identifiers).</param>
    /// <param name="debugSeverity">The severity of the message.</param>
    /// <param name="message">A human-readable text explaining the message.</param>
    public delegate void GLDebugMessageReceivedHandler(DebugSource debugSource, DebugType debugType, int messageId, DebugSeverity debugSeverity, string message);

    /// <summary>
    /// Manages an OpenGL Context and it's <see cref="GraphicsResource"/>-s.
    /// </summary>
    public sealed class GraphicsDevice : IDisposable
    {
        public readonly GL GL;

        /// <summary>Whether this <see cref="GraphicsDevice"/> instance has been disposed.</summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a <see cref="GraphicsDevice"/> to manage the given graphics context.
        /// </summary>
        /// <param name="context">The OpenGL Context for this <see cref="GraphicsDevice"/>.</param>
        /// <param name="resourceCount">An estimate of how many <see cref="GraphicsResource"/>-s you intend to use.</param>
        public GraphicsDevice(GL gl, int resourceCount = 128)
        {
            GL = gl ?? throw new ArgumentNullException(nameof(gl));
            if (resourceCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(resourceCount));

            GLMajorVersion = GL.GetInteger(GetPName.MajorVersion);
            GLMinorVersion = GL.GetInteger(GetPName.MinorVersion);

            if (!IsGLVersionAtLeast(3, 0))
                throw new PlatformNotSupportedException("TrippyGL only supports OpenGL 3.0 and up!");

            InitIsAvailableVariables();

            graphicsResources = new List<GraphicsResource>(resourceCount);

            InitGLGetVariables();
            InitBufferObjectStates();
            InitTextureStates();
            ClipDistances = new ClipDistanceManager(this);
            ResetStates();
        }

        /// <summary>
        /// Resets all GL states to the last values this <see cref="GraphicsDevice"/> knows.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions.
        /// </summary>
        public void ResetStates()
        {
            ResetBufferStates();
            ResetVertexArrayStates();
            ResetShaderProgramStates();
            ResetTextureStates();
            ResetFramebufferStates();
            ClipDistances.ResetStates();

            GL.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);

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
            GL.BlendColor(blendState.BlendColor.X, blendState.BlendColor.Y, blendState.BlendColor.Z, blendState.BlendColor.W);

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
            GL.FrontFace(polygonFrontFace);

            if (rasterizerEnabled)
                GL.Disable(EnableCap.RasterizerDiscard);
            else
                GL.Enable(EnableCap.RasterizerDiscard);
        }

        #region DebugMessaging

        private bool debugMessagingEnabled = false;

        /// <summary>An event for recieving OpenGL debug messages. Debug messaging must be enabled for this to work.</summary>
        public event GLDebugMessageReceivedHandler DebugMessage;

        /// <summary>If we don't store this delegate it gets garbage collected and dies and omg that's so sad alexa play despacito.</summary>
        private DebugProc debugProcDelegate;
        private GCHandle debugProcDelegateHandle;

        /// <summary>Whether OpenGL message debugging is enabled (using the KHR_debug extension or v4.3).</summary>
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
                        unsafe { GL.DebugMessageCallback(debugProcDelegate, (void*)0); }
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

        private void OnDebugMessageRecieved(GLEnum src, GLEnum type, int id, GLEnum sev, int length, IntPtr msg, IntPtr param)
        {
            DebugMessage?.Invoke((DebugSource)src, (DebugType)type, id, (DebugSeverity)sev, Marshal.PtrToStringAnsi(msg));
        }

        #endregion DebugMessaging

        #region GLGet

        private void InitGLGetVariables()
        {
            UniformBufferOffsetAlignment = GL.GetInteger(GetPName.UniformBufferOffsetAlignment);
            MaxUniformBufferBindings = GL.GetInteger(GetPName.MaxUniformBufferBindings);
            MaxUniformBlockSize = GL.GetInteger(GetPName.MaxUniformBlockSize);
            MaxSamples = GL.GetInteger(GLEnum.MaxSamples);
            MaxTextureSize = GL.GetInteger(GetPName.MaxTextureSize);
            MaxTextureImageUnits = GL.GetInteger(GetPName.MaxTextureImageUnits);
            MaxTextureBufferSize = GL.GetInteger(GetPName.MaxTextureBufferSize);
            Max3DTextureSize = GL.GetInteger(GetPName.Max3DTextureSize);
            MaxCubeMapTextureSize = GL.GetInteger(GetPName.MaxCubeMapTextureSize);
            MaxRectangleTextureSize = GL.GetInteger(GetPName.MaxRectangleTextureSize);
            MaxRenderbufferSize = GL.GetInteger(GetPName.MaxRenderbufferSize);
            MaxVertexAttribs = GL.GetInteger(GetPName.MaxVertexAttribs);
            MaxArrayTextureLayers = GL.GetInteger(GetPName.MaxArrayTextureLayers);
            MaxFramebufferColorAttachments = GL.GetInteger(GLEnum.MaxColorAttachments);
            MaxDrawBuffers = GL.GetInteger(GetPName.MaxDrawBuffers);
            MaxClipDistances = GL.GetInteger(GetPName.MaxClipDistances);
            MaxTransformFeedbackBuffers = GL.GetInteger(GLEnum.MaxTransformFeedbackBuffers);
            MaxTransformFeedbackInterleavedComponents = GL.GetInteger(GLEnum.MaxTransformFeedbackInterleavedComponents);
            MaxTransformFeedbackSeparateComponents = GL.GetInteger(GLEnum.MaxTransformFeedbackSeparateComponents);
            MaxTransformFeedbackSeparateAttribs = GL.GetInteger(GLEnum.MaxTransformFeedbackSeparateAttribs);
            MaxShaderStorageBufferBindings = GL.GetInteger(GLEnum.MaxShaderStorageBufferBindings);
            MaxAtomicCounterBufferBindings = GL.GetInteger(GLEnum.MaxAtomicCounterBufferBindings);
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

        public int MaxClipDistances { get; private set; }

        public int MaxTransformFeedbackBuffers { get; private set; }

        public int MaxTransformFeedbackInterleavedComponents { get; private set; }

        public int MaxTransformFeedbackSeparateComponents { get; private set; }

        public int MaxTransformFeedbackSeparateAttribs { get; private set; }

        public int MaxShaderStorageBufferBindings { get; private set; }

        public int MaxAtomicCounterBufferBindings { get; private set; }

        public string GLVersion => GL.GetString(StringName.Version);

        public string GLVendor => GL.GetString(StringName.Vendor);

        public string GLRenderer => GL.GetString(StringName.Renderer);

        public string GLShadingLanguageVersion => GL.GetString(StringName.ShadingLanguageVersion);

        #endregion GLGet

        #region IsAvailable

        private void InitIsAvailableVariables()
        {
            IsDoublePrecisionVertexAttribsAvailable = IsGLVersionAtLeast(4, 1);
            IsVertexAttribDivisorAvailable = IsGLVersionAtLeast(3, 3);
            IsInstancedDrawingAvailable = IsGLVersionAtLeast(3, 1);
            IsGeometryShaderAvailable = IsGLVersionAtLeast(3, 2);
            IsAdvancedTransformFeedbackAvailable = IsGLVersionAtLeast(4, 0);
            IsDoublePrecitionShaderVariablesAvailable = IsGLVersionAtLeast(4, 0);
        }

        public bool IsDoublePrecisionVertexAttribsAvailable { get; private set; }

        public bool IsVertexAttribDivisorAvailable { get; private set; }

        public bool IsInstancedDrawingAvailable { get; private set; }

        public bool IsGeometryShaderAvailable { get; private set; }

        public bool IsAdvancedTransformFeedbackAvailable { get; private set; }

        public bool IsDoublePrecitionShaderVariablesAvailable { get; private set; }

        #endregion

        #region BindingStates

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

        internal const BufferTargetARB DefaultBufferTarget = BufferTargetARB.ArrayBuffer;
        private const int defaultBufferTargetBindingIndex = arrayBufferIndex;

        /// <summary>
        /// Stores the handle of the last buffer bound to the <see cref="BufferTargetARB"/>
        /// found on the same index on the <see cref="bufferBindingTargets"/> array.
        /// </summary>
        private BufferObject[] bufferBindings;

        /// <summary>The <see cref="BufferTarget"/>-s for the handles found on the <see cref="bufferBindings"/> array.</summary>
        private BufferTargetARB[] bufferBindingTargets;

        /// <summary>For the four <see cref="BufferTarget"/>-s that have range bindings, this is an
        /// array of four arrays that contain the bound buffer and the bound range of each binding index.
        /// </summary>
        private BufferRangeBinding[][] bufferRangeBindings;

        /// <summary>
        /// Initializes all the fields needed for buffer binding.
        /// </summary>
        private void InitBufferObjectStates()
        {
            bufferBindingTargets = new BufferTargetARB[BufferTargetCount];

            // The first four targets need to be the ones that are managed with glBindBufferBase/glBindBufferRange
            // because these also have an index, offset and size value to them. So we need to handle more data!
            // The way it's done then, is by having the bufferRangeBindings array. The same index used to get
            // the buffer target and generic binding id used to get the BufferRangeBinding array.
            // However, trying to do this with any other target will result in an IndexOutOfRangeException

            bufferBindingTargets[uniformBufferIndex] = BufferTargetARB.UniformBuffer;
            bufferBindingTargets[shaderStorageBufferIndex] = BufferTargetARB.ShaderStorageBuffer;
            bufferBindingTargets[atomicCounterBufferIndex] = BufferTargetARB.AtomicCounterBuffer;
            bufferBindingTargets[arrayBufferIndex] = BufferTargetARB.ArrayBuffer;
            bufferBindingTargets[textureBufferIndex] = BufferTargetARB.TextureBuffer;
            bufferBindingTargets[pixelUnpackBufferIndex] = BufferTargetARB.PixelUnpackBuffer;
            bufferBindingTargets[pixelPackBufferIndex] = BufferTargetARB.PixelPackBuffer;
            bufferBindingTargets[drawIndirectBufferIndex] = BufferTargetARB.DrawIndirectBuffer;
            bufferBindingTargets[dispatchIndirectBufferIndex] = BufferTargetARB.DispatchIndirectBuffer;
            bufferBindingTargets[copyWriteBufferIndex] = BufferTargetARB.CopyWriteBuffer;
            bufferBindingTargets[copyReadBufferIndex] = BufferTargetARB.CopyReadBuffer;
            bufferBindingTargets[queryBufferIndex] = BufferTargetARB.QueryBuffer;

            bufferBindings = new BufferObject[BufferTargetCount];

            bufferRangeBindings = new BufferRangeBinding[3][];

            bufferRangeBindings[0] = new BufferRangeBinding[MaxUniformBufferBindings];
            bufferRangeBindings[1] = new BufferRangeBinding[MaxShaderStorageBufferBindings];
            bufferRangeBindings[2] = new BufferRangeBinding[MaxAtomicCounterBufferBindings];
        }

        /// <summary>Gets or sets (binds) the <see cref="BufferObject"/> currently bound to GL_ARRAY_BUFFER.</summary>
        public BufferObject ArrayBuffer
        {
            get { return bufferBindings[arrayBufferIndex]; }
            set
            {
                if (bufferBindings[arrayBufferIndex] != value)
                {
                    GL.BindBuffer(BufferTargetARB.ArrayBuffer, value == null ? 0 : value.Handle);
                    bufferBindings[arrayBufferIndex] = value;
                }
            }
        }

        /// <summary>Gets or sets (binds) the <see cref="BufferObject"/> currently bound to GL_COPY_READ_BUFFER.</summary>
        public BufferObject CopyReadBuffer
        {
            get { return bufferBindings[copyReadBufferIndex]; }
            set
            {
                if (bufferBindings[copyReadBufferIndex] != value)
                {
                    GL.BindBuffer(BufferTargetARB.CopyReadBuffer, value == null ? 0 : value.Handle);
                    bufferBindings[copyReadBufferIndex] = value;
                }
            }
        }

        /// <summary>Gets or sets (binds) the <see cref="BufferObject"/> currently bound to GL_COPY_WRITE_BUFFER.</summary>
        public BufferObject CopyWriteBuffer
        {
            get { return bufferBindings[copyWriteBufferIndex]; }
            set
            {
                if (bufferBindings[copyWriteBufferIndex] != value)
                {
                    GL.BindBuffer(BufferTargetARB.CopyWriteBuffer, value == null ? 0 : value.Handle);
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
            GL.BindBuffer(DefaultBufferTarget, buffer.Handle);
            bufferBindings[defaultBufferTargetBindingIndex] = buffer;
        }

        /// <summary>
        /// Binds a buffer subset to it's <see cref="BufferTargetARB"/>.
        /// </summary>
        /// <param name="bufferSubset">The buffer subset to bind. This value is assumed not to be null.</param>
        public void BindBuffer(BufferObjectSubset bufferSubset)
        {
            if (bufferSubset == null)
                throw new ArgumentNullException(nameof(bufferSubset));

            if (bufferBindings[bufferSubset.bufferTargetBindingIndex] != bufferSubset.Buffer)
                ForceBindBuffer(bufferSubset);
        }

        /// <summary>
        /// Binds a buffer subset to it's <see cref="BufferTargetARB"/> without first checking whether it's already bound.
        /// </summary>
        /// <param name="bufferSubset">The buffer subset to bind. This value is assumed not to be null.</param>
        internal void ForceBindBuffer(BufferObjectSubset bufferSubset)
        {
            GL.BindBuffer(bufferSubset.BufferTarget, bufferSubset.BufferHandle);
            bufferBindings[bufferSubset.bufferTargetBindingIndex] = bufferSubset.Buffer;
        }

        /// <summary>
        /// Binds a range of a buffer subset to a binding index on it's <see cref="BufferTarget"/>
        /// The buffer subset's <see cref="BufferTarget"/> must be one with multiple binding indexes.
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null.</param>
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
            GL.BindBufferRange(buffer.BufferTarget, bindingIndex, buffer.BufferHandle, (int)offset, size);
            bufferBindings[buffer.bufferTargetBindingIndex] = buffer.Buffer;
            bufferRangeBindings[buffer.bufferTargetBindingIndex][bindingIndex].SetRange(buffer, offset, size);
        }

        /// <summary>
        /// Binds a buffer to the GL_COPY_READ_BUFFER target without first checking whether it's already bound.
        /// </summary>
        /// <param name="buffer">The buffer to bind.</param>
        internal void ForceBindBufferCopyRead(BufferObject buffer)
        {
            GL.BindBuffer(BufferTargetARB.CopyReadBuffer, buffer == null ? 0 : buffer.Handle);
            bufferBindings[copyReadBufferIndex] = buffer;
        }

        /// <summary>
        /// Binds a buffer to the GL_COPY_WRITE_BUFFER taret without first checking whether it's already bound.
        /// </summary>
        /// <param name="buffer">The buffer to bind.</param>
        internal void ForceBindBufferCopyWrite(BufferObject buffer)
        {
            GL.BindBuffer(BufferTargetARB.CopyWriteBuffer, buffer == null ? 0 : buffer.Handle);
            bufferBindings[copyWriteBufferIndex] = buffer;
        }

        /// <summary>
        /// Returns whether the given buffer subset is the currently bound one for it's <see cref="BufferTarget"/>.
        /// </summary>
        /// <param name="buffer">The buffer subset to check. This value is assumed not to be null.</param>
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
        internal int GetBindingTargetIndex(BufferTargetARB bufferTarget)
        {
            for (int i = 0; i < BufferTargetCount; i++)
                if (bufferBindingTargets[i] == bufferTarget)
                    return i;
            return -1;
        }

        /// <summary>
        /// Resets all saved states for buffer objects. This is, the variables used to check whether to bind a buffer or not.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions.
        /// </summary>
        public void ResetBufferStates()
        {
            for (int i = 0; i < BufferTargetCount; i++)
                bufferBindings[i] = null;

            for (int i = 0; i < bufferRangeBindings.Length; i++)
            {
                BufferRangeBinding[] arr = bufferRangeBindings[i];
                for (int c = 0; c < arr.Length; c++)
                    arr[c].Reset();
            }
        }

        /// <summary>
        /// This struct is used to manage buffer object binding in cases where a buffer can be bound to multiple indices in the same target.
        /// Each <see cref="BufferRangeBinding"/> represents one of these binding points in a <see cref="BufferTarget"/>.
        /// Of course, this must be in a <see cref="BufferTarget"/> to which multiple buffers can be bound.
        /// </summary>
        internal struct BufferRangeBinding
        {
            public BufferObject Buffer;
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
            /// <param name="buffer".></param>
            public void SetRange(BufferObjectSubset buffer)
            {
                Buffer = buffer.Buffer;
                Offset = buffer.StorageOffsetInBytes;
                Size = buffer.StorageLengthInBytes;
            }
        }

        #endregion BufferObjectBindingStates

        #region VertexArrayBindingStates

        private VertexArray vertexArray;

        /// <summary>Gets or sets (binds) the currently bound <see cref="TrippyGL.VertexArray"/>.</summary>
        public VertexArray VertexArray
        {
            get { return vertexArray; }
            set
            {
                if (vertexArray != value)
                {
                    GL.BindVertexArray(value == null ? 0 : value.Handle);
                    vertexArray = value;
                }
            }
        }

        /// <summary>
        /// Binds a vertex array without first checking whether it's already bound.
        /// </summary>
        /// <param name="array">The array to bind.</param>
        internal void ForceBindVertexArray(VertexArray array)
        {
            GL.BindVertexArray(array == null ? 0 : array.Handle);
            vertexArray = array;
        }

        /// <summary>
        /// Resets all saved states for vertex arrays This is, the variables used to check whether to bind a vertex array or not.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions.
        /// </summary>
        public void ResetVertexArrayStates()
        {
            GL.BindVertexArray(0);
            vertexArray = null;
        }

        #endregion VertexArrayBindingStates

        #region ShaderProgramBindingStates

        private ShaderProgram shaderProgram;

        /// <summary>Gets or sets (binds) the currently bound <see cref="TrippyGL.ShaderProgram"/>.</summary>
        public ShaderProgram ShaderProgram
        {
            get { return shaderProgram; }
            set
            {
                if (shaderProgram != value)
                    ForceUseShaderProgram(value);
            }
        }

        /// <summary>
        /// Installs the given program into the rendering pipeline without first checking whether it's already in use.
        /// </summary>
        /// <param name="program">The shader program to use.</param>
        internal void ForceUseShaderProgram(ShaderProgram program)
        {
            GL.UseProgram(program == null ? 0 : program.Handle);
            shaderProgram = program;
        }

        /// <summary>
        /// Resets all saved states for shader programs. This is, the variables used to check whether to use a shader program or not.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions.
        /// </summary>
        public void ResetShaderProgramStates()
        {
            GL.UseProgram(0);
            shaderProgram = null;
        }

        #endregion ShaderProgramBindingStates

        #region TextureBindingStates

        /// <summary>The array containing for each texture unit (that is, the index of the array) which texture handle is bound to it.</summary>
        private uint[] textureBindings;

        /// <summary>This variable counts which texture unit will be used the next time a texture needs binding.</summary>
        private int nextBindUnit;

        /// <summary>The currently active texture unit.</summary>
        public int ActiveTextureUnit { get; private set; }

        /// <summary>
        /// When a texture needs a new binding, it requests a texture unit from this method.
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
        /// Ensures a texture is bound to any texture unit, but doesn't ensure that texture unit is the currently active one.
        /// Returns the texture unit to which the texture is bound.
        /// </summary>
        /// <param name="texture">The texture to ensure is bound.</param>
        public int BindTexture(Texture texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (textureBindings[texture.lastBindUnit] != texture.Handle)
                return ForceBindTexture(texture);
            return texture.lastBindUnit;
        }

        /// <summary>
        /// Ensures a texture is bound to any texture unit, and that the texture unit to which the texture is bound is the currently active one.
        /// Returns the texture unit to which the texture is bound.
        /// </summary>
        /// <param name="texture">The texture to ensure is bound and active.</param>
        public int BindTextureSetActive(Texture texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

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
        /// <param name="texture">The texture to bind.</param>
        internal int ForceBindTexture(Texture texture)
        {
            SetActiveTexture(GetNextBindTextureUnit());
            GL.BindTexture(texture.TextureType, texture.Handle);
            texture.lastBindUnit = ActiveTextureUnit;
            textureBindings[ActiveTextureUnit] = texture.Handle;
            return texture.lastBindUnit;
        }

        /// <summary>
        /// Binds a texture to the current texture unit.
        /// </summary>
        /// <param name="texture">The texture to bind.</param>
        internal void ForceBindTextureToCurrentUnit(Texture texture)
        {
            GL.BindTexture(texture.TextureType, texture.Handle);
            texture.lastBindUnit = ActiveTextureUnit;
            textureBindings[ActiveTextureUnit] = texture.Handle;
        }

        /// <summary>
        /// Ensures all of the given <see cref="Texture"/>-s are bound to a texture unit. Nulls are ignored.
        /// </summary>
        /// <param name="textures">The <see cref="Texture"/>-s to ensure are bound.</param>
        public void BindAllTextures(ReadOnlySpan<Texture> textures)
        {
            if (textures.Length > textureBindings.Length)
                throw new NotSupportedException("You tried to bind more textures at the same time than this system supports");

            for (int i = 0; i < textures.Length; i++)
            {
                Texture t = textures[i];
                if (t != null && textureBindings[t.lastBindUnit] != t.Handle)
                {
                    SetActiveTexture(FindUnusedTextureUnit(textures));
                    ForceBindTextureToCurrentUnit(t);
                }
            }

            // Find a texture unit that's not in use by any of the given textures
            int FindUnusedTextureUnit(ReadOnlySpan<Texture> texturesToBind)
            {
                int unit;
                do
                {
                    unit = GetNextBindTextureUnit();
                } while (IsTextureUnitInUse(unit, texturesToBind));
                return unit;
            }

            // Whether a texture unit is currently in use by any of the specified textures
            bool IsTextureUnitInUse(int unit, ReadOnlySpan<Texture> texturesToBind)
            {
                for (int i = 0; i < texturesToBind.Length; i++)
                {
                    Texture t = texturesToBind[i];
                    if (t != null && t.lastBindUnit == unit && textureBindings[unit] == t.Handle)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Ensures all of the given <see cref="Texture"/>-s are bound to a texture unit. Nulls are ignored.
        /// </summary>
        /// <param name="textures">The <see cref="Texture"/>-s to ensure are bound.</param>
        public void BindAllTextures(List<Texture> textures)
        {
            if (textures == null)
                throw new ArgumentNullException(nameof(textures));

            if (textures.Count > textureBindings.Length)
                throw new NotSupportedException("You tried to bind more textures at the same time than this system supports");

            for (int i = 0; i < textures.Count; i++)
            {
                Texture t = textures[i];
                if (t != null && textureBindings[t.lastBindUnit] != t.Handle)
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
                    if (t != null && t.lastBindUnit == unit && textureBindings[unit] == t.Handle)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Returns whether a texture is the one still bound to it's last bind location.
        /// </summary>
        /// <param name="texture">The texture to check if it's bound.</param>
        public bool IsTextureBound(Texture texture)
        {
            return texture != null && textureBindings[texture.lastBindUnit] == texture.Handle;
        }

        /// <summary>
        /// Initiates all variables that track texture binding states.
        /// </summary>
        private void InitTextureStates()
        {
            textureBindings = new uint[MaxTextureImageUnits];
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

        /// <summary>Gets or sets (binds) the framebuffer currently bound for drawing.</summary>
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

        /// <summary>Gets or sets (binds) the framebuffer currently bound for reading.</summary>
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

        /// <summary>Sets (binds) a framebuffer for both drawing and reading.</summary>
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

        /// <summary>Gets or sets (binds) the current renderbuffer.</summary>
        public RenderbufferObject Renderbuffer
        {
            get { return renderbuffer; }
            set
            {
                if (renderbuffer != value)
                {
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, value == null ? 0 : value.Handle);
                    renderbuffer = value;
                }
            }
        }

        /// <summary>
        /// Binds a framebuffer for drawing without first checking whether it's already bound.
        /// </summary>
        /// <param name="framebuffer">The framebuffer to bind.</param>
        internal void ForceBindDrawFramebuffer(FramebufferObject framebuffer)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, framebuffer == null ? 0 : framebuffer.Handle);
            drawFramebuffer = framebuffer;
        }

        /// <summary>
        /// Binds a framebuffer for reading without first checking whether it's already bound.
        /// </summary>
        /// <param name="framebuffer">The framebuffer to bind.</param>
        internal void ForceBindReadFramebuffer(FramebufferObject framebuffer)
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, framebuffer == null ? 0 : framebuffer.Handle);
            readFramebuffer = framebuffer;
        }

        /// <summary>
        /// Binds a renderbuffer without first checking whether it's already bound.
        /// </summary>
        /// <param name="renderbuffer">The renderbuffer to bind.</param>
        internal void ForceBindRenderbuffer(RenderbufferObject renderbuffer)
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer.Handle);
            this.renderbuffer = renderbuffer;
        }

        /// <summary>
        /// Resets all saved states for <see cref="FramebufferObject"/>-s.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions.
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

        #endregion BindingStates

        #region DrawingStates

        #region ClearColor

        /// <summary>The current clear color.</summary>
        private Vector4 clearColor;

        /// <summary>
        /// Gets or sets the current color to use on clear operations.
        /// </summary>
        public Vector4 ClearColor
        {
            get { return clearColor; }
            set
            {
                if (clearColor != value)
                {
                    GL.ClearColor(value.X, value.Y, value.Z, value.W);
                    clearColor = value;
                }
            }
        }
        #endregion

        #region Viewport
        /// <summary>The current drawing viewport.</summary>
        private Viewport viewport;

        /// <summary>Gets or sets the current viewport for drawing.</summary>
        public Viewport Viewport
        {
            get { return viewport; } //The get is OK because Rectangle is a struct so no worries about modifying it
            set
            {
                if (!value.Equals(viewport))
                {
                    GL.Viewport(value.X, value.Y, value.Width, value.Height);
                    viewport = value;
                }
            }
        }

        /// <summary>
        /// Sets the current viewport for drawing.
        /// </summary>
        /// <param name="x">The viewport's X.</param>
        /// <param name="y">The viewport's Y.</param>
        /// <param name="width">The viewport's width.</param>
        /// <param name="height">The viewport's height.</param>
        public void SetViewport(int x, int y, uint width, uint height)
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

        /// <summary>Whether scissor testing is currently enabled.</summary>
        private bool scissorTestEnabled = false;

        /// <summary>The current scissor rectangle.</summary>
        private Viewport scissorRect;

        /// <summary>Gets or sets whether scissor testing is enable.</summary>
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
        /// Gets or sets the scissor rectangle that discards fragments rendered outside it.
        /// </summary>
        public Viewport ScissorRectangle
        {
            get { return scissorRect; }
            set
            {
                if (!value.Equals(scissorRect))
                {
                    GL.Scissor(value.X, value.Y, value.Width, value.Height);
                    scissorRect = value;
                }
            }
        }

        public void SetScissorRectangle(int x, int y, uint width, uint height)
        {
            if (x != scissorRect.X || y != scissorRect.Y || width != scissorRect.Width || height != scissorRect.Height)
            {
                scissorRect.X = x;
                scissorRect.Y = y;
                scissorRect.Width = width;
                scissorRect.Height = height;
                GL.Scissor(x, y, width, height);
            }
        }

        #endregion

        #region BlendState

        /// <summary>The currently applied <see cref="TrippyGL.BlendState"/> values.</summary>
        private BlendState blendState = BlendState.Opaque;

        /// <summary>Gets or sets the <see cref="TrippyGL.BlendState"/> used for drawing.</summary>
        public BlendState BlendState
        {
            get { return blendState.Clone(); } // Since BlendState is a class we shouldn't return a reference to our instance
            set
            {
                // The specified BlendState's fields are copied into blendState, because we need to store all the
                // fields of a BlendState but if we save the same BlendState class instance, the user can modify these!
                bool isValueOpaque = value == null || value.IsOpaque;

                if (blendState.IsOpaque && isValueOpaque) //if the current and the new blend state are both opaque... Do nothing
                    return;

                // Either the current or new blend state is not opaque
                if (isValueOpaque) //blendState.IsOpaque must therefore be false
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

                    if (blendState.SourceFactorRGB != value.SourceFactorRGB || blendState.SourceFactorAlpha != value.SourceFactorAlpha
                        || blendState.DestFactorRGB != value.DestFactorRGB || blendState.DestFactorAlpha != value.DestFactorAlpha)
                    {
                        GL.BlendFuncSeparate(value.SourceFactorRGB, value.DestFactorRGB, value.SourceFactorAlpha, value.DestFactorAlpha);
                        blendState.SourceFactorRGB = value.SourceFactorRGB;
                        blendState.SourceFactorAlpha = value.SourceFactorAlpha;
                        blendState.DestFactorRGB = value.DestFactorRGB;
                        blendState.DestFactorAlpha = value.DestFactorAlpha;
                    }

                    if (blendState.BlendColor != value.BlendColor)
                    {
                        GL.BlendColor(value.BlendColor.X, value.BlendColor.Y, value.BlendColor.Z, value.BlendColor.W);
                        blendState.BlendColor = value.BlendColor;
                    }
                }
            }
        }

        /// <summary>Enables or disables color blending.</summary>
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

        /// <summary>The current depth state.</summary>
        private DepthTestingState depthState = new DepthTestingState(false);

        /// <summary>Sets the current depth testing state.</summary>
        public DepthTestingState DepthState
        {
            get { return depthState.Clone(); }
            set
            {
                if (value != null && value.DepthTestingEnabled)
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

                    if (depthState.DepthRangeNear != value.DepthRangeNear || depthState.DepthRangeFar != value.DepthRangeFar)
                    {
                        GL.DepthRange(value.DepthRangeNear, value.DepthRangeFar);
                        depthState.DepthRangeNear = value.DepthRangeNear;
                        depthState.DepthRangeFar = value.DepthRangeFar;
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
        }

        /// <summary>Enables or disables depth testing.</summary>
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

        /// <summary>The current depth to set on a clear depth operation.</summary>
        public float ClearDepth
        {
            get { return depthState.ClearDepth; }
            set
            {
                if (depthState.ClearDepth != value)
                {
                    GL.ClearDepth(value);
                    depthState.ClearDepth = value;
                }
            }
        }

        #endregion

        #region FaceCulling

        private bool faceCullingEnabled = false;
        private CullFaceMode cullFaceMode = CullFaceMode.Back;
        private FrontFaceDirection polygonFrontFace = FrontFaceDirection.Ccw;

        /// <summary>Enables or disables culling polygon faces.</summary>
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

        /// <summary>Sets the face culling mode to use when face culling is enabled.</summary>
        public CullFaceMode CullFaceMode
        {
            get { return cullFaceMode; }
            set
            {
                if (cullFaceMode != value)
                {
                    GL.CullFace(cullFaceMode);
                    cullFaceMode = value;
                }
            }
        }

        /// <summary>Sets which face of a polygon is the front one (Whether front is when vertices are aligned clockwise or counter clockwise).</summary>
        public FrontFaceDirection PolygonFrontFace
        {
            get { return polygonFrontFace; }
            set
            {
                if (polygonFrontFace != value)
                {
                    GL.FrontFace(value);
                    polygonFrontFace = value;
                }
            }
        }

        #endregion FaceCulling

        #region ClipDistances

        /// <summary>Controls to enable and/or disable clip distances.</summary>
        public ClipDistanceManager ClipDistances { get; private set; }

        /// <summary>
        /// Manages the enabling or disabling of clip distances.
        /// </summary>
        public class ClipDistanceManager
        {
            private readonly bool[] areEnabled;

            /// <summary>The maximum amount of clip distances you can use.</summary>
            public int Count => areEnabled.Length;

            private readonly GL GL;

            /// <summary>
            /// Enables or disables a gl_ClipDistance[] index.
            /// </summary>
            /// <param name="index".></param>
            public bool this[int index]
            {
                get { return areEnabled[index]; }
                set
                {
                    if (areEnabled[index] != value)
                    {
                        if (value)
                            GL.Enable(EnableCap.ClipDistance0 + index);
                        else
                            GL.Disable(EnableCap.ClipDistance0 + index);
                        areEnabled[index] = value;
                    }
                }
            }

            internal ClipDistanceManager(GraphicsDevice device)
            {
                GL = device.GL;
                areEnabled = new bool[device.MaxClipDistances];
                for (int i = 0; i < areEnabled.Length; i++)
                    areEnabled[i] = false;
            }

            /// <summary>
            /// Enables a range of clip distance variables.
            /// </summary>
            /// <param name="min">The index of the first clip distance to enable.</param>
            /// <param name="max">The index of the last clip distance to enable (inclusive).</param>
            public void EnableRange(int min, int max)
            {
                for (int i = min; i <= max; i++)
                    if (!areEnabled[i])
                    {
                        GL.Enable(EnableCap.ClipDistance0 + i);
                        areEnabled[i] = true;
                    }
            }

            /// <summary>
            /// Disables a range of clip distance variables.
            /// </summary>
            /// <param name="min">The index of the first clip distance to disable.</param>
            /// <param name="max">The index of the last clip distance to disable (inclusive).</param>
            public void DisableRange(int min, int max)
            {
                for (int i = min; i <= max; i++)
                    if (areEnabled[i])
                    {
                        GL.Disable(EnableCap.ClipDistance0 + i);
                        areEnabled[i] = false;
                    }
            }

            /// <summary>
            /// Ensures that the only enabled clip distances are the ones on the specified range.
            /// </summary>
            /// <param name="min">The index of the first clip distance to enable.</param>
            /// <param name="max">The index of the last clip distance to enable (inclusive).</param>
            public void SetEnabledRange(int min, int max)
            {
                for (int i = 0; i < areEnabled.Length; i++)
                    this[i] = i >= min && i <= max;
            }

            /// <summary>
            /// Disables all clip distances.
            /// </summary>
            public void DisableAll()
            {
                for (int i = 0; i < areEnabled.Length; i++)
                    if (areEnabled[i])
                    {
                        GL.Disable(EnableCap.ClipDistance0 + i);
                        areEnabled[i] = false;
                    }
            }

            /// <summary>
            /// Resets all the states from clip distances.
            /// </summary>
            public void ResetStates()
            {
                for (int i = 0; i < areEnabled.Length; i++)
                {
                    areEnabled[i] = false;
                    GL.Disable(EnableCap.ClipDistance0 + i);
                }
            }
        }

        #endregion ClipDistances

        #region Misc

        private bool cubemapSeamlessEnabled = false;

        /// <summary>Enables or disables seamless sampling across cubemap faces.</summary>
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

        private bool rasterizerEnabled = true;

        /// <summary>Enables or disables the pixel rasterizer.</summary>
        public bool RasterizerEnabled
        {
            get { return rasterizerEnabled; }
            set
            {
                if (rasterizerEnabled != value)
                {
                    if (value)
                        GL.Disable(EnableCap.RasterizerDiscard);
                    else
                        GL.Enable(EnableCap.RasterizerDiscard);
                    rasterizerEnabled = value;
                }
            }
        }

        #endregion

        #endregion DrawingStates

        #region DrawingFunctions

        /// <summary>
        /// Clears the current framebuffer to the specified color.
        /// </summary>
        /// <param name="mask">The masks indicating the values to clear, combined using bitwise OR.</param>
        public void Clear(ClearBufferMask mask)
        {
            GL.Clear((uint)mask);
        }

        /// <summary>
        /// Renders primitive data.
        /// </summary>
        /// <param name="primitiveType">The type of primitive to render.</param>
        /// <param name="startIndex">The index of the first vertex to render.</param>
        /// <param name="count">The amount of vertices to render.</param>
        public void DrawArrays(PrimitiveType primitiveType, int startIndex, uint count)
        {
            shaderProgram.EnsurePreDrawStates();
            GL.DrawArrays(primitiveType, startIndex, count);
        }

        /// <summary>
        /// Renders indexed primitive data.
        /// </summary>
        /// <param name="primitiveType">The type of primitive to render.</param>
        /// <param name="startIndex">The index of the first element to render.</param>
        /// <param name="count">The amount of elements to render.</param>
        public unsafe void DrawElements(PrimitiveType primitiveType, int startIndex, uint count)
        {
            shaderProgram.EnsurePreDrawStates();
            IndexBufferSubset indexSubset = vertexArray.IndexBuffer;
            GL.DrawElements(primitiveType, count, indexSubset.ElementType, (void*)(indexSubset.StorageOffsetInBytes + startIndex * indexSubset.ElementSize));
        }

        /// <summary>
        /// Renders instanced primitive data.
        /// </summary>
        /// <param name="primitiveType">The type of primitive to render.</param>
        /// <param name="startIndex">The index of the first element to render.</param>
        /// <param name="count">The amount of elements to render.</param>
        /// <param name="instanceCount">The amount of instances to render.</param>
        public void DrawArraysInstanced(PrimitiveType primitiveType, int startIndex, uint count, uint instanceCount)
        {
            shaderProgram.EnsurePreDrawStates();
            GL.DrawArraysInstanced(primitiveType, startIndex, count, instanceCount);
        }

        /// <summary>
        /// Renders indexed instanced primitive data.
        /// </summary>
        /// <param name="primitiveType">The type of primitive to render.</param>
        /// <param name="startIndex">The index of the first element to render.</param>
        /// <param name="count">The amount of elements to render.</param>
        /// <param name="instanceCount">The amount of instances to render.</param>
        public unsafe void DrawElementsInstanced(PrimitiveType primitiveType, int startIndex, uint count, uint instanceCount)
        {
            shaderProgram.EnsurePreDrawStates();
            IndexBufferSubset indexSubset = vertexArray.IndexBuffer;
            GL.DrawElementsInstanced(primitiveType, count, indexSubset.ElementType, (void*)(indexSubset.StorageOffsetInBytes + startIndex * indexSubset.ElementSize), instanceCount);
        }

        /// <summary>
        /// Copies content from the read framebuffer to the draw framebuffer.
        /// </summary>
        /// <param name="srcX">The X location of the first pixel to read.</param>
        /// <param name="srcY">The Y location of the first pixel to read.</param>
        /// <param name="srcWidth">The width of the read rectangle.</param>
        /// <param name="srcHeight">The height of the read rectangle.</param>
        /// <param name="dstX">The X location of the first pixel to write.</param>
        /// <param name="dstY">The Y location of the first pixel to write.</param>
        /// <param name="dstWidth">The width of the write rectangle.</param>
        /// <param name="dstHeight">The height of the draw rectangle.</param>
        /// <param name="mask">What data to copy from the framebuffers.</param>
        /// <param name="filter">Whether to use nearest or linear filtering.</param>
        public void BlitFramebuffer(int srcX, int srcY, int srcWidth, int srcHeight, int dstX, int dstY, int dstWidth, int dstHeight, ClearBufferMask mask, BlitFramebufferFilter filter)
        {
            // Blit rules:
            // General rectangle correctness rules (src and dst rectangles must be inside the framebuffers' size rectangles)
            // If mask contains Depth or Stencil, filter must be Nearest.
            // Buffers must have same image format?
            // If the framebuffers contain integer format, filter must be Nearest.
            // The blit fails if any of the following conditions about samples is true:
            //    1. Both framebuffers have different amount of samples and one of them isn't 0
            //    2. Condition 1 is true and the width and height of the src and dst rectangles don't match

            if (srcX < 0 || srcY < 0 || dstX < 0 || dstY < 0
                || (readFramebuffer != null && (srcX + srcWidth > readFramebuffer.Width || srcY + srcHeight > readFramebuffer.Height))
                || (drawFramebuffer != null && (dstX + dstWidth > drawFramebuffer.Width || dstY + dstHeight > drawFramebuffer.Height)))
            { //If the source of destination rectangles are outside of bounds (if a framebuffer is null, the values are just ignored.
                throw new ArgumentException("Both the source and destination rectangles must be inside their respective framebuffer's size rectangles");
            }

            if (((mask & ClearBufferMask.DepthBufferBit) | (mask & ClearBufferMask.StencilBufferBit)) != 0 && filter != BlitFramebufferFilter.Nearest)
                throw new InvalidBlitException("When using depth or stencil, the filter must be Nearest");

            //TODO: If blitting with depth mask, ensure both have depth. If blitting with stencil mask, ensure both have stencil, etc.
            //TODO: Check that the sample count for both framebuffers is valid for blitting

            GL.BlitFramebuffer(srcX, srcY, srcWidth, srcHeight, dstX, dstY, dstWidth, dstHeight, (uint)mask, filter);
        }

        /// <summary>
        /// Copies content from the read framebuffer to the draw framebuffer.
        /// </summary>
        /// <param name="srcRect">The source rectangle to copy from.</param>
        /// <param name="dstRect">The destination rectangle to write to.</param>
        /// <param name="mask">What data to copy from the framebuffers.</param>
        /// <param name="filter">Whether to use nearest or linear filtering.</param>
        public void BlitFramebuffer(Viewport srcRect, Viewport dstRect, ClearBufferMask mask, BlitFramebufferFilter filter)
        {
            BlitFramebuffer(srcRect.X, srcRect.Y, (int)srcRect.Width, (int)srcRect.Height, dstRect.X, dstRect.Y, (int)dstRect.Width, (int)dstRect.Height, mask, filter);
        }

        #endregion DrawingFunctions

        #region GraphicsResource Management

        private List<GraphicsResource> graphicsResources;

        /// <summary>
        /// This is called by all <see cref="GraphicsResource"/>-s on creation.
        /// </summary>
        /// <param name="createdResource">The newly created resource.</param>
        internal void OnResourceAdded(GraphicsResource createdResource)
        {
            EnsureNotDisposed();
            graphicsResources.Add(createdResource);
        }

        /// <summary>
        /// This is called by <see cref="GraphicsResource"/>-s on <see cref="GraphicsResource.Dispose"/>.
        /// </summary>
        /// <param name="disposedResource">The graphics resource that was just disposed.</param>
        internal void OnResourceRemoved(GraphicsResource disposedResource)
        {
            graphicsResources.Remove(disposedResource);
        }

        /// <summary>
        /// Disposes all the <see cref="GraphicsResource"/>-s owned by this <see cref="GraphicsDevice"/>. This does not dispose the
        /// <see cref="GraphicsDevice"/>, so it can still be used for new resources afterwards.
        /// </summary>
        public void DisposeAllResources()
        {
            for (int i = 0; i < graphicsResources.Count; i++)
                graphicsResources[i].DisposeByGraphicsDevice();
            graphicsResources.Clear();
        }

        #endregion GraphicsResourceManagement

        /// <summary>
        /// Ensures all OpenGL commands given before this function was called are not being queued up (doesn't wait for them to finish).
        /// </summary>
        public void FlushCommands()
        {
            GL.Flush();
        }

        /// <summary>
        /// Waits for all current OpenGL commands to finish being executed.
        /// </summary>
        public void FinishCommands()
        {
            GL.Finish();
        }

        /// <summary>
        /// Removes a <see cref="GraphicsResource"/> from it's <see cref="GraphicsDevice"/> and makes it belong to this <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <param name="resource">The resource to pass over.</param>
        public void MakeMine(GraphicsResource resource)
        {
            if (resource.GraphicsDevice != this)
            {
                resource.GraphicsDevice.OnResourceRemoved(resource);
                resource.GraphicsDevice = this;
                OnResourceAdded(resource);
            }
        }

        /// <summary>
        /// Returns whether the GL version is the specified version or newer.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        public bool IsGLVersionAtLeast(int major, int minor)
        {
            return GLMajorVersion > major || (major == GLMajorVersion && GLMinorVersion >= minor);
        }

        /// <summary>
        /// Checks whether this <see cref="GraphicsDevice"/> is already disposed and throws an exception if it is.
        /// </summary>
        private void EnsureNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(GraphicsDevice));
        }

        /// <summary>
        /// Disposes this <see cref="GraphicsDevice"/>, it's <see cref="GraphicsResource"/>-s and it's context.
        /// The <see cref="GraphicsDevice"/> nor it's resources can be used once it's been disposed.
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                DisposeAllResources();
                DebugMessagingEnabled = false; // this makes sure any GCHandle or unmanaged stuff gets released
                IsDisposed = true;
                GL.Dispose();
            }
        }
    }
}
