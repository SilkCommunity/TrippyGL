using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public class BlendMode
    {
        /// <summary>Whether the blend mode is opaque. If this is true, all other BlendMode members are irrelevant</summary>
        public bool IsOpaque;

        public BlendEquationMode EquationModeRGB, EquationModeAlpha;
        public BlendingFactorSrc SourceFactorRGB, SourceFactorAlpha;
        public BlendingFactorDest DestFactorRGB, DestFactorAlpha;

        /// <summary>This color can be used for blending calculations with the blending factors for constant color</summary>
        public Color4 BlendColor;

        public BlendMode(bool isOpaque)
        {
            this.IsOpaque = isOpaque;
        }

        public BlendMode(bool isOpaque, BlendEquationMode equationModeRgb, BlendEquationMode equationModeAlpha, BlendingFactorSrc sourceFactorRgb, BlendingFactorDest destFactorRgb, BlendingFactorSrc sourceFactorAlpha, BlendingFactorDest destFactorAlpha)
        {
            this.IsOpaque = isOpaque;
            this.EquationModeRGB = equationModeRgb;
            this.EquationModeAlpha = equationModeAlpha;
            this.SourceFactorRGB = sourceFactorRgb;
            this.DestFactorRGB = destFactorRgb;
            this.SourceFactorAlpha = sourceFactorAlpha;
            this.DestFactorAlpha = destFactorAlpha;
        }

        public BlendMode(bool isOpaque, BlendEquationMode equationModeRgb, BlendEquationMode equationModeAlpha, BlendingFactorSrc sourceFactorRgb, BlendingFactorDest destFactorRgb, BlendingFactorSrc sourceFactorAlpha, BlendingFactorDest destFactorAlpha, Color4 blendColor)
        {
            this.IsOpaque = isOpaque;
            this.EquationModeRGB = equationModeRgb;
            this.EquationModeAlpha = equationModeAlpha;
            this.SourceFactorRGB = sourceFactorRgb;
            this.DestFactorRGB = destFactorRgb;
            this.SourceFactorAlpha = sourceFactorAlpha;
            this.DestFactorAlpha = destFactorAlpha;
            this.BlendColor = blendColor;
        }

        public void Apply()
        {
            if (IsOpaque)
                GL.Disable(EnableCap.Blend);
            else
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendColor(BlendColor);
                GL.BlendEquationSeparate(EquationModeRGB, EquationModeAlpha);
                GL.BlendFuncSeparate(SourceFactorRGB, DestFactorRGB, SourceFactorAlpha, DestFactorAlpha);
            }
        }

        /// <summary>
        /// Sets the current blending mode to Opaque. This acts the same as saying BlendMode.Opaque.Apply();
        /// </summary>
        public static void SetOpaque()
        {
            GL.Disable(EnableCap.Blend);
        }


        #region Static Members

        public static BlendMode Opaque { get { return new BlendMode(true); } }

        public static BlendMode AlphaBlend { get { return new BlendMode(false, BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.One); } }

        public static BlendMode Additive { get { return new BlendMode(false, BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd, BlendingFactorSrc.One, BlendingFactorDest.One, BlendingFactorSrc.One, BlendingFactorDest.One); } }

        #endregion
    }
}
