using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Represents a way to blend colors together when rendering. The blending function uses various parameters
    /// such as the output fragment color and the current pixel color, allowing you to define your own way to blend.
    /// </summary>
    public class BlendState
    {
        /// <summary>Whether the blend mode is opaque. If this is true, all other BlendMode members are irrelevant</summary>
        public bool IsOpaque;

        /// <summary>The equation mode for the RGB color components</summary>
        public BlendEquationMode EquationModeRGB;
        /// <summary>The equation mode for the Alpha color component</summary>
        public BlendEquationMode EquationModeAlpha;

        /// <summary>The source factor for the RGB color components</summary>
        public BlendingFactorSrc SourceFactorRGB;
        /// <summary>The source factor for the Alpha color component</summary>
        public BlendingFactorSrc SourceFactorAlpha;

        /// <summary>The destination factor for the RGB color components</summary>
        public BlendingFactorDest DestFactorRGB;
        /// <summary>The destination factor for the Alpha color components</summary>
        public BlendingFactorDest DestFactorAlpha;

        /// <summary>This color can be used for blending calculations with the blending factors for constant color</summary>
        public Color4 BlendColor;

        public BlendState(bool isOpaque)
        {
            this.IsOpaque = isOpaque;
            EquationModeRGB = BlendEquationMode.FuncAdd;
            EquationModeAlpha = BlendEquationMode.FuncAdd;
            SourceFactorRGB = BlendingFactorSrc.Zero;
            SourceFactorAlpha = BlendingFactorSrc.Zero;
            DestFactorRGB = BlendingFactorDest.Zero;
            DestFactorAlpha = BlendingFactorDest.Zero;
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

        #region Static Members

        public static BlendState Opaque { get { return new BlendState(true); } }

        public static BlendState AlphaBlend { get { return new BlendState(false, BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.One); } }

        public static BlendState Additive { get { return new BlendState(false, BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd, BlendingFactorSrc.One, BlendingFactorDest.One, BlendingFactorSrc.One, BlendingFactorDest.One); } }

        public static BlendState Substractive { get { return new BlendState(false, BlendEquationMode.FuncSubtract, BlendEquationMode.FuncSubtract, BlendingFactorSrc.One, BlendingFactorDest.One, BlendingFactorSrc.One, BlendingFactorDest.One); } }

        #endregion
    }
}
