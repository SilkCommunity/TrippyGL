using System.Numerics;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    public partial class GraphicsDevice
    {
        #region ClearColor

        /// <summary>Backing field for <see cref="ClearColor"/>.</summary>
        private Vector4 clearColor;

        /// <summary>Gets or sets the current color to use on clear operations.</summary>
        public Vector4 ClearColor
        {
            get => clearColor;
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

        /// <summary>Backing field for <see cref="Viewport"/>.</summary>
        private Viewport viewport;

        /// <summary>Gets or sets the current viewport for drawing.</summary>
        public Viewport Viewport
        {
            get => viewport;
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

        /// <summary>Backing field for <see cref="ScissorTestEnabled"/>.</summary>
        private bool scissorTestEnabled;

        /// <summary>Backing field for <see cref="ScissorRectangle"/>.</summary>
        private Viewport scissorRect;

        /// <summary>Gets or sets whether scissor testing is enable.</summary>
        public bool ScissorTestEnabled
        {
            get => scissorTestEnabled;
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
            get => scissorRect;
            set
            {
                if (!value.Equals(scissorRect))
                {
                    GL.Scissor(value.X, value.Y, value.Width, value.Height);
                    scissorRect = value;
                }
            }
        }

        /// <summary>
        /// Sets the current scissor rectangle.
        /// </summary>
        /// <param name="x">The scissor rectangle's X.</param>
        /// <param name="y">The scissor rectangle's Y.</param>
        /// <param name="width">The scissor rectangle's width.</param>
        /// <param name="height">The scissor rectangle's height.</param>
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
        private readonly BlendState blendState = BlendState.Opaque;

        /// <summary>Gets or sets the <see cref="TrippyGL.BlendState"/> used for drawing.</summary>
        /// <remarks>
        /// Getting this property returns a cloned object. Modifying that object doesn't
        /// automatically apply changes to this <see cref="GraphicsDevice"/>.
        /// </remarks>
        public BlendState BlendState
        {
            get => blendState.Clone(); // Since BlendState is a class we shouldn't return a reference to our instance
            set
            {
                // The specified BlendState's fields are copied into blendState, because we need to store all the
                // fields of a BlendState but if we save the same BlendState class instance, the user can modify these!
                if (blendState.IsOpaque && (value == null || value.IsOpaque)) //if the current and the new blend state are both opaque... Do nothing
                    return;

                // Either the current or new blend state is not opaque
                if (value == null || value.IsOpaque) //blendState.IsOpaque must therefore be false
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
            get => !blendState.IsOpaque;
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

        public void ResetBlendStates()
        {
            if (blendState.IsOpaque)
                GL.Disable(EnableCap.Blend);
            else
                GL.Enable(EnableCap.Blend);

            GL.BlendFuncSeparate(blendState.SourceFactorRGB, blendState.DestFactorRGB, blendState.SourceFactorAlpha, blendState.DestFactorAlpha);
            GL.BlendEquationSeparate(blendState.EquationModeRGB, blendState.EquationModeAlpha);
            GL.BlendColor(blendState.BlendColor.X, blendState.BlendColor.Y, blendState.BlendColor.Z, blendState.BlendColor.W);
        }

        #endregion BlendState

        #region DepthState

        /// <summary>The current depth state.</summary>
        private readonly DepthState depthState = new DepthState(false);

        /// <summary>Sets the <see cref="TrippyGL.DepthState"/> used for drawing.</summary>
        /// <remarks>
        /// Getting this property returns a cloned object. Modifying that object doesn't
        /// automatically apply changes to this <see cref="GraphicsDevice"/>.
        /// </remarks>
        public DepthState DepthState
        {
            get => depthState.Clone();
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
            get => depthState.DepthTestingEnabled;
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

        /// <summary>The depth value to set on a clear depth operation.</summary>
        public float ClearDepth
        {
            get => depthState.ClearDepth;
            set
            {
                if (depthState.ClearDepth != value)
                {
                    GL.ClearDepth(value);
                    depthState.ClearDepth = value;
                }
            }
        }

        /// <summary>
        /// Sets all depth states to the last values known by this <see cref="GraphicsDevice"/>.
        /// </summary>
        public void ResetDepthStates()
        {
            if (depthState.DepthTestingEnabled)
                GL.Enable(EnableCap.DepthTest);
            else
                GL.Disable(EnableCap.DepthTest);

            GL.DepthFunc(depthState.DepthComparison);
            GL.ClearDepth(depthState.ClearDepth);
            GL.DepthRange(depthState.DepthRangeNear, depthState.DepthRangeFar);
            GL.DepthMask(depthState.DepthBufferWrittingEnabled);
        }

        #endregion

        #region StencilState

        /// <summary>The current stencil state.</summary>
        private readonly StencilState stencilState = new StencilState(false);

        /// <summary>Sets the <see cref="TrippyGL.StencilState"/> used for drawing.</summary>
        /// <remarks>
        /// Getting this property returns a cloned object. Modifying that object
        /// doesn't automatically apply changes to this <see cref="GraphicsDevice"/>.
        /// </remarks>
        public StencilState StencilState
        {
            get => stencilState.Clone();
            set
            {
                if (value != null && value.StencilTestingEnabled)
                {
                    if (stencilState.StencilTestingEnabled)
                    {
                        GL.Enable(EnableCap.StencilTest);
                        stencilState.StencilTestingEnabled = true;
                    }

                    if (stencilState.ClearStencil != value.ClearStencil)
                    {
                        GL.ClearStencil(value.ClearStencil);
                        stencilState.ClearStencil = value.ClearStencil;
                    }

                    if (stencilState.FrontWriteMask != value.FrontWriteMask)
                    {
                        GL.StencilMaskSeparate(StencilFaceDirection.Front, value.FrontWriteMask);
                        stencilState.FrontWriteMask = value.FrontWriteMask;
                    }

                    if (stencilState.BackWriteMask != value.BackWriteMask)
                    {
                        GL.StencilMaskSeparate(StencilFaceDirection.Back, value.BackWriteMask);
                        stencilState.BackWriteMask = value.BackWriteMask;
                    }

                    if (stencilState.FrontFunction != value.FrontFunction || stencilState.FrontRefValue != value.FrontRefValue || stencilState.FrontTestMask != value.FrontTestMask)
                    {
                        GL.StencilFuncSeparate(StencilFaceDirection.Front, value.FrontFunction, value.FrontRefValue, value.FrontTestMask);
                        stencilState.FrontFunction = value.FrontFunction;
                        stencilState.FrontRefValue = value.FrontRefValue;
                        stencilState.FrontTestMask = value.FrontTestMask;
                    }

                    if (stencilState.BackFunction != value.BackFunction || stencilState.BackRefValue != value.BackRefValue || stencilState.BackTestMask != value.BackTestMask)
                    {
                        GL.StencilFuncSeparate(StencilFaceDirection.Back, value.BackFunction, value.BackRefValue, value.BackTestMask);
                        stencilState.BackFunction = value.BackFunction;
                        stencilState.BackRefValue = value.BackRefValue;
                        stencilState.BackTestMask = value.BackTestMask;
                    }

                    if (stencilState.FrontStencilFailOperation != value.FrontStencilFailOperation || stencilState.FrontDepthFailOperation != value.FrontDepthFailOperation || stencilState.FrontPassOperation != value.FrontPassOperation)
                    {
                        GL.StencilOpSeparate(StencilFaceDirection.Front, value.FrontStencilFailOperation, value.FrontDepthFailOperation, value.FrontPassOperation);
                        stencilState.FrontStencilFailOperation = value.FrontStencilFailOperation;
                        stencilState.FrontDepthFailOperation = value.FrontDepthFailOperation;
                        stencilState.FrontPassOperation = value.FrontPassOperation;
                    }

                    if (stencilState.BackStencilFailOperation != value.BackStencilFailOperation || stencilState.BackDepthFailOperation != value.BackDepthFailOperation || stencilState.BackPassOperation != value.BackPassOperation)
                    {
                        GL.StencilOpSeparate(StencilFaceDirection.Back, value.BackStencilFailOperation, value.BackDepthFailOperation, value.BackPassOperation);
                        stencilState.BackStencilFailOperation = value.BackStencilFailOperation;
                        stencilState.BackDepthFailOperation = value.BackDepthFailOperation;
                        stencilState.BackPassOperation = value.BackPassOperation;
                    }
                }
                else if (StencilState.StencilTestingEnabled)
                {
                    GL.Disable(EnableCap.StencilTest);
                    stencilState.StencilTestingEnabled = false;
                }
            }
        }

        /// <summary>Enables or disables stencil testing.</summary>
        public bool StencilTestingEnabled
        {
            get => stencilState.StencilTestingEnabled;
            set
            {
                if (stencilState.StencilTestingEnabled != value)
                {
                    if (value)
                        GL.Enable(EnableCap.StencilTest);
                    else
                        GL.Disable(EnableCap.StencilTest);
                    stencilState.StencilTestingEnabled = value;
                }
            }
        }

        /// <summary>The stencil value to set on a clear stencil operation.</summary>
        public int ClearStencil
        {
            get => stencilState.ClearStencil;
            set
            {
                if (stencilState.ClearStencil != value)
                {
                    GL.ClearStencil(value);
                    stencilState.ClearStencil = value;
                }
            }
        }

        /// <summary>
        /// Sets all stencil states to the last values known by this <see cref="GraphicsDevice"/>.
        /// </summary>
        public void ResetStencilStates()
        {
            if (stencilState.StencilTestingEnabled)
                GL.Enable(EnableCap.StencilTest);
            else
                GL.Disable(EnableCap.StencilTest);

            GL.ClearStencil(stencilState.ClearStencil);
            GL.StencilMaskSeparate(StencilFaceDirection.Front, stencilState.FrontWriteMask);
            GL.StencilMaskSeparate(StencilFaceDirection.Back, stencilState.BackWriteMask);
            GL.StencilFuncSeparate(StencilFaceDirection.Front, stencilState.FrontFunction, stencilState.FrontRefValue, stencilState.FrontTestMask);
            GL.StencilFuncSeparate(StencilFaceDirection.Back, stencilState.BackFunction, stencilState.BackRefValue, stencilState.BackTestMask);
            GL.StencilOpSeparate(StencilFaceDirection.Front, stencilState.FrontStencilFailOperation, stencilState.FrontDepthFailOperation, stencilState.FrontPassOperation);
            GL.StencilOpSeparate(StencilFaceDirection.Back, stencilState.BackStencilFailOperation, stencilState.BackDepthFailOperation, stencilState.BackPassOperation);
        }

        #endregion

        #region FaceCulling

        private bool faceCullingEnabled;
        private CullFaceMode cullFaceMode = CullFaceMode.Back;
        private FrontFaceDirection polygonFrontFace = FrontFaceDirection.Ccw;

        /// <summary>Enables or disables culling polygon faces.</summary>
        public bool FaceCullingEnabled
        {
            get => faceCullingEnabled;
            set
            {
                if (faceCullingEnabled != value)
                {
                    faceCullingEnabled = value;
                    ResetFaceCullingStates();
                }
            }
        }

        /// <summary>Sets which polygon face to cull when face culling is enabled.</summary>
        public CullFaceMode CullFaceMode
        {
            get => cullFaceMode;
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
            get => polygonFrontFace;
            set
            {
                if (polygonFrontFace != value)
                {
                    GL.FrontFace(value);
                    polygonFrontFace = value;
                }
            }
        }

        /// <summary>
        /// Sets all face culling states to the last values known by this <see cref="GraphicsDevice"/>.
        /// </summary>
        public void ResetFaceCullingStates()
        {
            if (faceCullingEnabled)
                GL.Enable(EnableCap.CullFace);
            else
                GL.Disable(EnableCap.CullFace);

            GL.CullFace(cullFaceMode);
            GL.FrontFace(polygonFrontFace);
        }

        #endregion FaceCulling

        #region PolygonMode

        private PolygonMode polygonMode = PolygonMode.Fill;

        /// <summary>Gets or sets the mode in which polygons are rasterized.</summary>
        public PolygonMode PolygonMode
        {
            get => polygonMode;
            set
            {
                if (value != polygonMode)
                {
                    GL.PolygonMode(GLEnum.FrontAndBack, value);
                    polygonMode = value;
                }
            }
        }

        #endregion

        #region ClipDistances

        /// <summary>An array containing whether each clip distance index is enabled.</summary>
        private readonly bool[] clipDistancesEnabled;

        /// <summary>
        /// Gets whether a gl_ClipDistance index is enabled.
        /// </summary>
        /// <param name="index">The index of the clip distance to get.</param>
        public bool IsClipDistanceEnabled(int index)
        {
            return clipDistancesEnabled[index];
        }

        /// <summary>
        /// Enables a gl_ClipDistance index.
        /// </summary>
        public void EnableClipDistance(int index)
        {
            if (!clipDistancesEnabled[index])
            {
                GL.Enable(EnableCap.ClipDistance0 + index);
                clipDistancesEnabled[index] = true;
            }
        }

        /// <summary>
        /// Disables a gl_ClipDistance index.
        /// </summary>
        public void DisableClipDistance(int index)
        {
            if (clipDistancesEnabled[index])
            {
                GL.Disable(EnableCap.ClipDistance0 + index);
                clipDistancesEnabled[index] = false;
            }
        }

        /// <summary>
        /// Enables a range of the gl_ClipDistance variables.
        /// </summary>
        /// <param name="min">The index of the first clip distance to enable.</param>
        /// <param name="max">The index of the last clip distance to enable (inclusive).</param>
        public void EnableClipDistanceRange(int min, int max)
        {
            for (int i = min; i <= max; i++)
                if (!clipDistancesEnabled[i])
                {
                    GL.Enable(EnableCap.ClipDistance0 + i);
                    clipDistancesEnabled[i] = true;
                }
        }

        /// <summary>
        /// Disables a range of the gl_ClipDistance variables.
        /// </summary>
        /// <param name="min">The index of the first clip distance to disable.</param>
        /// <param name="max">The index of the last clip distance to disable (inclusive).</param>
        public void DisableClipDistanceRange(int min, int max)
        {
            for (int i = min; i <= max; i++)
                if (clipDistancesEnabled[i])
                {
                    GL.Disable(EnableCap.ClipDistance0 + i);
                    clipDistancesEnabled[i] = false;
                }
        }

        /// <summary>
        /// Disables all of the gl_ClipDistance variables.
        /// </summary>
        public void DisableAllClipDistances()
        {
            for (int i = 0; i < MaxClipDistances; i++)
                if (clipDistancesEnabled[i])
                {
                    GL.Disable(EnableCap.ClipDistance0 + i);
                    clipDistancesEnabled[i] = false;
                }
        }

        /// <summary>
        /// Resets all clip distance states to the last values this <see cref="GraphicsDevice"/> knows.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions.
        /// </summary>
        public void ResetClipDistanceStates()
        {
            for (int i = 0; i < clipDistancesEnabled.Length; i++)
            {
                if (clipDistancesEnabled[i])
                    GL.Enable(EnableCap.ClipDistance0 + i);
                else
                    GL.Disable(EnableCap.ClipDistance0 + i);
            }
        }

        #endregion ClipDistances

        #region Misc

        private bool cubemapSeamlessEnabled = true;

        /// <summary>Enables or disables seamless sampling across cubemap faces.</summary>
        public bool TextureCubemapSeamlessEnabled
        {
            get => cubemapSeamlessEnabled;
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
            get => rasterizerEnabled;
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
    }
}
