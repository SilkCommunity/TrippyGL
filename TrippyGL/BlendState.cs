using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{

#pragma warning disable 0660 // Disable the warnings to override Equals() and GetHashCode()
#pragma warning disable 0661 // These appear because I overrided the == and != operators

    public class BlendState
    {
        /// <summary>Whether the blend mode is opaque. If this is true, all other BlendMode members are irrelevant</summary>
        public bool IsOpaque;

        public BlendEquationMode EquationModeRGB, EquationModeAlpha;
        public BlendingFactorSrc SourceFactorRGB, SourceFactorAlpha;
        public BlendingFactorDest DestFactorRGB, DestFactorAlpha;

        /// <summary>This color can be used for blending calculations with the blending factors for constant color</summary>
        public Color4 BlendColor;

        public static bool operator ==(BlendState x, BlendState y)
        {
            return x.IsOpaque == y.IsOpaque && x.EquationModeRGB == y.EquationModeRGB && x.EquationModeAlpha == y.EquationModeAlpha
                && x.SourceFactorRGB == y.SourceFactorRGB && x.SourceFactorAlpha == y.SourceFactorAlpha
                && x.DestFactorRGB == y.DestFactorRGB && x.DestFactorAlpha == y.DestFactorAlpha && x.BlendColor == y.BlendColor;
        }

        public static bool operator !=(BlendState x, BlendState y)
        {
            return x.IsOpaque != y.IsOpaque || x.EquationModeRGB != y.EquationModeRGB || x.EquationModeAlpha != y.EquationModeAlpha
                || x.SourceFactorRGB != y.SourceFactorRGB || x.SourceFactorAlpha != y.SourceFactorAlpha
                || x.DestFactorRGB != y.DestFactorRGB || x.DestFactorAlpha != y.DestFactorAlpha || x.BlendColor != y.BlendColor;
        }

        public BlendState(bool isOpaque)
        {
            this.IsOpaque = isOpaque;
        }

        public BlendState(bool isOpaque, BlendEquationMode equationModeRgba, BlendingFactorSrc sourceFactorRgba, BlendingFactorDest destFactorRgba)
        {
            this.IsOpaque = isOpaque;
            this.EquationModeRGB = equationModeRgba;
            this.EquationModeAlpha = equationModeRgba;
            this.SourceFactorRGB = sourceFactorRgba;
            this.SourceFactorAlpha = sourceFactorRgba;
            this.DestFactorRGB = destFactorRgba;
            this.DestFactorAlpha = destFactorRgba;
        }

        public BlendState(bool isOpaque, BlendEquationMode equationModeRgba, BlendingFactorSrc sourceFactorRgba, BlendingFactorDest destFactorRgba, Color4 blendColor)
        {
            this.IsOpaque = isOpaque;
            this.EquationModeRGB = equationModeRgba;
            this.EquationModeAlpha = equationModeRgba;
            this.SourceFactorRGB = sourceFactorRgba;
            this.SourceFactorAlpha = sourceFactorRgba;
            this.DestFactorRGB = destFactorRgba;
            this.DestFactorAlpha = destFactorRgba;
            this.BlendColor = blendColor;
        }

        public BlendState(bool isOpaque, BlendEquationMode equationModeRgb, BlendEquationMode equationModeAlpha, BlendingFactorSrc sourceFactorRgb, BlendingFactorDest destFactorRgb, BlendingFactorSrc sourceFactorAlpha, BlendingFactorDest destFactorAlpha)
        {
            this.IsOpaque = isOpaque;
            this.EquationModeRGB = equationModeRgb;
            this.EquationModeAlpha = equationModeAlpha;
            this.SourceFactorRGB = sourceFactorRgb;
            this.DestFactorRGB = destFactorRgb;
            this.SourceFactorAlpha = sourceFactorAlpha;
            this.DestFactorAlpha = destFactorAlpha;
        }

        public BlendState(bool isOpaque, BlendEquationMode equationModeRgb, BlendEquationMode equationModeAlpha, BlendingFactorSrc sourceFactorRgb, BlendingFactorDest destFactorRgb, BlendingFactorSrc sourceFactorAlpha, BlendingFactorDest destFactorAlpha, Color4 blendColor)
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

        /// <summary>
        /// Sets the current blending mode to Opaque. This acts the same as BlendMode.Opaque.Apply(); (just faster)
        /// </summary>
        public static void SetOpaque()
        {
            GL.Disable(EnableCap.Blend);
        }

        #region Static Members

        public static BlendState Opaque { get { return new BlendState(true); } }

        public static BlendState AlphaBlend { get { return new BlendState(false, BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.One); } }

        public static BlendState Additive { get { return new BlendState(false, BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd, BlendingFactorSrc.One, BlendingFactorDest.One, BlendingFactorSrc.One, BlendingFactorDest.One); } }

        #endregion
    }
}
