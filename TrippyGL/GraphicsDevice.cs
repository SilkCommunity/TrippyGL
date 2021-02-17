using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    public delegate void ShaderCompiledHandler(GraphicsDevice sender, in ShaderProgramBuilder programBuilder, bool success);

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
    public sealed partial class GraphicsDevice : IDisposable
    {
        /// <summary>
        /// The <see cref="Silk.NET.OpenGL.GL"/> object that all <see cref="GraphicsResource"/>-s
        /// on this <see cref="GraphicsDevice"/> will use to call GL functions.
        /// </summary>
        public readonly GL GL;

        /// <summary>Whether this <see cref="GraphicsDevice"/> instance has been disposed.</summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a <see cref="GraphicsDevice"/> to manage the given graphics context.
        /// </summary>
        /// <param name="gl">The <see cref="Silk.NET.OpenGL.GL"/> object this <see cref="GraphicsDevice"/> will use for GL calls.</param>
        public GraphicsDevice(GL gl)
        {
            GL = gl ?? throw new ArgumentNullException(nameof(gl));

            GLMajorVersion = GL.GetInteger(GetPName.MajorVersion);
            GLMinorVersion = GL.GetInteger(GetPName.MinorVersion);

            if (!IsGLVersionAtLeast(3, 0))
                throw new PlatformNotSupportedException("TrippyGL only supports OpenGL 3.0 and up.");

            InitIsAvailableVariables();

            graphicsResources = new List<GraphicsResource>();

            InitGLGetVariables();
            InitBufferObjectStates();
            InitTextureStates();
            clipDistancesEnabled = new bool[MaxClipDistances];
            ResetStates();
        }

        /// <summary>
        /// Resets all GL states to either zero or the last values this <see cref="GraphicsDevice"/> knows.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions.
        /// </summary>
        public void ResetStates()
        {
            ResetBufferStates();
            ResetVertexArrayStates();
            ResetShaderProgramStates();
            ResetTextureStates();
            ResetFramebufferStates();
            ResetClipDistanceStates();

            GL.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);
            GL.Viewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
            GL.PolygonMode(GLEnum.FrontAndBack, polygonMode);

            if (scissorTestEnabled)
                GL.Enable(EnableCap.ScissorTest);
            else
                GL.Disable(EnableCap.ScissorTest);
            GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);

            ResetBlendStates();
            ResetDepthStates();
            ResetStencilStates();
            ResetFaceCullingStates();

            if (cubemapSeamlessEnabled)
                GL.Enable(EnableCap.TextureCubeMapSeamless);
            else
                GL.Disable(EnableCap.TextureCubeMapSeamless);

            if (rasterizerEnabled)
                GL.Disable(EnableCap.RasterizerDiscard);
            else
                GL.Enable(EnableCap.RasterizerDiscard);
        }

        #region DebugMessaging

        private bool debugMessagingEnabled;

        /// <summary>An event for recieving OpenGL debug messages. Debug messaging must be enabled for this to work.</summary>
        public event GLDebugMessageReceivedHandler DebugMessageReceived;

        /// <summary>Whether OpenGL message debugging is enabled (using the KHR_debug extension or v4.3).</summary>
        public bool DebugMessagingEnabled
        {
            get => debugMessagingEnabled;
            set
            {
                if (value)
                {
                    if (!debugMessagingEnabled)
                    {
                        GL.Enable(EnableCap.DebugOutput);
                        GL.Enable(EnableCap.DebugOutputSynchronous);
                        unsafe { GL.DebugMessageCallback(OnDebugMessageRecieved, (void*)0); }
                        debugMessagingEnabled = true;
                    }
                }
                else if (debugMessagingEnabled)
                {
                    GL.Disable(EnableCap.DebugOutput);
                    GL.Disable(EnableCap.DebugOutputSynchronous);
                    debugMessagingEnabled = false;
                }
            }
        }

        private void OnDebugMessageRecieved(GLEnum src, GLEnum type, int id, GLEnum sev, int length, IntPtr msg, IntPtr param)
        {
            DebugMessageReceived?.Invoke((DebugSource)src, (DebugType)type, id, (DebugSeverity)sev, Marshal.PtrToStringAnsi(msg));
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
            MaxFragmentUniformComponents = GL.GetInteger(GetPName.MaxFragmentUniformComponents);
            MaxUniformLocations = GL.GetInteger(GetPName.MaxUniformLocations);
            MaxVaryingComponents = GL.GetInteger(GetPName.MaxVaryingComponents);
        }

        public int GLMajorVersion { get; private set; }

        public int GLMinorVersion { get; private set; }

        public int MaxFragmentUniformComponents { get; private set; }

        public int MaxUniformLocations { get; private set; }

        public int MaxVaryingComponents { get; private set; }

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

        #region DrawingFunctions

        /// <summary>
        /// Clears the current framebuffer to the specified color.
        /// </summary>
        /// <param name="mask">The masks indicating the values to clear, combined using bitwise OR.</param>
        public void Clear(ClearBuffers mask)
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
            GL.DrawArrays((GLEnum)primitiveType, startIndex, count);
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
            GL.DrawElements((GLEnum)primitiveType, count, (GLEnum)indexSubset.ElementType, (void*)(indexSubset.StorageOffsetInBytes + startIndex * indexSubset.ElementSize));
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
            GL.DrawArraysInstanced((GLEnum)primitiveType, startIndex, count, instanceCount);
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
            GL.DrawElementsInstanced((GLEnum)primitiveType, count, (GLEnum)indexSubset.ElementType, (void*)(indexSubset.StorageOffsetInBytes + startIndex * indexSubset.ElementSize), instanceCount);
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
        public void BlitFramebuffer(int srcX, int srcY, int srcWidth, int srcHeight, int dstX, int dstY, int dstWidth, int dstHeight, ClearBuffers mask, BlitFramebufferFilter filter)
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

            if (((mask & ClearBuffers.Depth) | (mask & ClearBuffers.Stencil)) != 0 && filter != BlitFramebufferFilter.Nearest)
                throw new InvalidBlitException("When using depth or stencil, the filter must be Nearest");

            //TODO: If blitting with depth mask, ensure both have depth. If blitting with stencil mask, ensure both have stencil, etc.
            //TODO: Check that the sample count for both framebuffers is valid for blitting

            GL.BlitFramebuffer(srcX, srcY, srcWidth, srcHeight, dstX, dstY, dstWidth, dstHeight, (uint)mask, (GLEnum)filter);
        }

        /// <summary>
        /// Copies content from the read framebuffer to the draw framebuffer.
        /// </summary>
        /// <param name="srcRect">The source rectangle to copy from.</param>
        /// <param name="dstRect">The destination rectangle to write to.</param>
        /// <param name="mask">What data to copy from the framebuffers.</param>
        /// <param name="filter">Whether to use nearest or linear filtering.</param>
        public void BlitFramebuffer(Viewport srcRect, Viewport dstRect, ClearBuffers mask, BlitFramebufferFilter filter)
        {
            BlitFramebuffer(srcRect.X, srcRect.Y, (int)srcRect.Width, (int)srcRect.Height, dstRect.X, dstRect.Y, (int)dstRect.Width, (int)dstRect.Height, mask, filter);
        }

        #endregion DrawingFunctions

        #region ShaderCompiledEvent

        /// <summary>
        /// Occurs whenever a <see cref="TrippyGL.ShaderProgram"/> compiles, whether it fails or succeeds.
        /// </summary>
        /// <remarks>
        /// Even if the shaders were compiled specifying not to get compilation logs, if compilation failed
        /// then logs will be queried from OpenGL anyway, but only for the particular thing that failed.<para/>
        /// The "success" parameter will only be true if the whole operation, including program linking, succeeded.
        /// </remarks>
        public event ShaderCompiledHandler ShaderCompiled;

        /// <summary>
        /// Raises the <see cref="ShaderCompiled"/> event.
        /// </summary>
        internal void OnShaderCompiled(in ShaderProgramBuilder programBuilder, bool success)
        {
            ShaderCompiled?.Invoke(this, programBuilder, success);
        }

        #endregion

        #region GraphicsResource Management

        private readonly List<GraphicsResource> graphicsResources;

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
        /// Ensures all OpenGL commands given before this function was called are
        /// not being queued up (doesn't wait for them to finish).
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
        /// Disposes this <see cref="GraphicsDevice"/> and it's <see cref="GraphicsResource"/>-s.
        /// The <see cref="GraphicsDevice"/> nor it's resources can be used once it's been disposed.
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                DisposeAllResources();
                DebugMessagingEnabled = false; // this makes sure any GCHandle or unmanaged stuff gets released
                IsDisposed = true;
            }
        }
    }
}
