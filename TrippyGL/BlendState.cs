using System;
using System.Text;
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

        /// <summary>
        /// Creates a simple BlendState
        /// </summary>
        /// <param name="isOpaque">Whether this BlendState is opaque</param>
        public BlendState(bool isOpaque)
        {
            IsOpaque = isOpaque;
            EquationModeRGB = BlendEquationMode.FuncAdd;
            EquationModeAlpha = BlendEquationMode.FuncAdd;
            SourceFactorRGB = BlendingFactorSrc.Zero;
            SourceFactorAlpha = BlendingFactorSrc.Zero;
            DestFactorRGB = BlendingFactorDest.Zero;
            DestFactorAlpha = BlendingFactorDest.Zero;
        }

        /// <summary>
        /// Creates a BlendState with a simple equation
        /// </summary>
        /// <param name="isOpaque">Whether this BlendState is opaque</param>
        /// <param name="equationModeRgba">The equation mode to use for the RGBA values</param>
        /// <param name="sourceFactorRgba">The source factor to use for the RGBA values</param>
        /// <param name="destFactorRgba">The destination factor to use for the RGBA values</param>
        public BlendState(bool isOpaque, BlendEquationMode equationModeRgba, BlendingFactorSrc sourceFactorRgba, BlendingFactorDest destFactorRgba)
        {
            IsOpaque = isOpaque;
            EquationModeRGB = equationModeRgba;
            EquationModeAlpha = equationModeRgba;
            SourceFactorRGB = sourceFactorRgba;
            SourceFactorAlpha = sourceFactorRgba;
            DestFactorRGB = destFactorRgba;
            DestFactorAlpha = destFactorRgba;
        }

        /// <summary>
        /// Creates a BlendState with a simple equation and a blending color
        /// </summary>
        /// <param name="isOpaque">Whether this BlendState is opaque</param>
        /// <param name="equationModeRgba">The equation mode to use for the RGBA values</param>
        /// <param name="sourceFactorRgba">The source factor to use for the RGBA values</param>
        /// <param name="destFactorRgba">The destination factor to use for the RGBA values</param>
        /// <param name="blendColor">The equation-constant blending color</param>
        public BlendState(bool isOpaque, BlendEquationMode equationModeRgba, BlendingFactorSrc sourceFactorRgba, BlendingFactorDest destFactorRgba, Color4 blendColor)
        {
            IsOpaque = isOpaque;
            EquationModeRGB = equationModeRgba;
            EquationModeAlpha = equationModeRgba;
            SourceFactorRGB = sourceFactorRgba;
            SourceFactorAlpha = sourceFactorRgba;
            DestFactorRGB = destFactorRgba;
            DestFactorAlpha = destFactorRgba;
            BlendColor = blendColor;
        }

        /// <summary>
        /// Creates a BlendState with specified separate equations, factors and a blend color
        /// </summary>
        /// <param name="isOpaque">Whether this BlendState is opaque</param>
        /// <param name="equationModeRgb">The equation mode to use for the RGB values</param>
        /// <param name="equationModeAlpha">The equation mode to use for the Alpha value</param>
        /// <param name="sourceFactorRgb">The source factor to use for the RGB values</param>
        /// <param name="destFactorRgb">The destination factor to use for the RGB values</param>
        /// <param name="sourceFactorAlpha">The source factor to use for the Alpha value</param>
        /// <param name="destFactorAlpha">The destination factor to use for the Alpha value</param>
        public BlendState(bool isOpaque, BlendEquationMode equationModeRgb, BlendEquationMode equationModeAlpha, BlendingFactorSrc sourceFactorRgb, BlendingFactorDest destFactorRgb, BlendingFactorSrc sourceFactorAlpha, BlendingFactorDest destFactorAlpha)
        {
            IsOpaque = isOpaque;
            EquationModeRGB = equationModeRgb;
            EquationModeAlpha = equationModeAlpha;
            SourceFactorRGB = sourceFactorRgb;
            DestFactorRGB = destFactorRgb;
            SourceFactorAlpha = sourceFactorAlpha;
            DestFactorAlpha = destFactorAlpha;
        }

        /// <summary>
        /// Creates a BlendState with specified separate equations, factors and a blend color
        /// </summary>
        /// <param name="isOpaque">Whether this BlendState is opaque</param>
        /// <param name="equationModeRgb">The equation mode to use for the RGB values</param>
        /// <param name="equationModeAlpha">The equation mode to use for the Alpha value</param>
        /// <param name="sourceFactorRgb">The source factor to use for the RGB values</param>
        /// <param name="destFactorRgb">The destination factor to use for the RGB values</param>
        /// <param name="sourceFactorAlpha">The source factor to use for the Alpha value</param>
        /// <param name="destFactorAlpha">The destination factor to use for the Alpha value</param>
        /// <param name="blendColor">The equation-constant blending color</param>
        public BlendState(bool isOpaque, BlendEquationMode equationModeRgb, BlendEquationMode equationModeAlpha, BlendingFactorSrc sourceFactorRgb, BlendingFactorDest destFactorRgb, BlendingFactorSrc sourceFactorAlpha, BlendingFactorDest destFactorAlpha, Color4 blendColor)
        {
            IsOpaque = isOpaque;
            EquationModeRGB = equationModeRgb;
            EquationModeAlpha = equationModeAlpha;
            SourceFactorRGB = sourceFactorRgb;
            DestFactorRGB = destFactorRgb;
            SourceFactorAlpha = sourceFactorAlpha;
            DestFactorAlpha = destFactorAlpha;
            BlendColor = blendColor;
        }

        /// <summary>
        /// Creates a BlendState with the same values as another specified BlendState
        /// </summary>
        /// <param name="copy">The BlendState whose values to copy</param>
        public BlendState(BlendState copy)
        {
            IsOpaque = copy.IsOpaque;
            EquationModeRGB = copy.EquationModeRGB;
            EquationModeAlpha = copy.EquationModeAlpha;
            SourceFactorRGB = copy.SourceFactorRGB;
            SourceFactorAlpha = copy.SourceFactorAlpha;
            DestFactorRGB = copy.DestFactorRGB;
            DestFactorAlpha = copy.DestFactorAlpha;
            BlendColor = copy.BlendColor;
        }

        public override string ToString()
        {
            if (IsOpaque)
                return "Opaque";

            StringBuilder builder = new StringBuilder(300);

            if (EquationModeRGB == EquationModeAlpha)
            {
                builder.Append("EquationModeRGBA=\"");
                builder.Append(EquationModeRGB.ToString());
            }
            else
            {
                builder.Append("EquationModeRGB=\"");
                builder.Append(EquationModeRGB.ToString());
                builder.Append("\", EquationModeAlpha=\"");
                builder.Append(EquationModeAlpha.ToString());
            }

            if(SourceFactorRGB == SourceFactorAlpha)
            {
                builder.Append("\", SourceFactorRGBA=\"");
                builder.Append(SourceFactorRGB.ToString());
            }
            else
            {
                builder.Append("\", SourceFactorRGB=\"");
                builder.Append(SourceFactorRGB.ToString());
                builder.Append("\", SourceFactorAlpha=\"");
                builder.Append(SourceFactorAlpha.ToString());
            }

            if(DestFactorRGB == DestFactorAlpha)
            {
                builder.Append("\", DestFactorRGBA=\"");
                builder.Append(DestFactorRGB.ToString());
            }
            else
            {
                builder.Append("\", DestFactorRGB=\"");
                builder.Append(DestFactorRGB.ToString());
                builder.Append("\", DestFactorAlpha=\"");
                builder.Append(DestFactorAlpha.ToString());
            }

            builder.Append("\", BlendColor=");
            builder.Append(BlendColor.ToString());

            return builder.ToString();
        }

        #region Static Members

        public static BlendState Opaque { get { return new BlendState(true); } }

        public static BlendState AlphaBlend { get { return new BlendState(false, BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.One); } }

        public static BlendState Additive { get { return new BlendState(false, BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd, BlendingFactorSrc.One, BlendingFactorDest.One, BlendingFactorSrc.One, BlendingFactorDest.One); } }

        public static BlendState Substractive { get { return new BlendState(false, BlendEquationMode.FuncSubtract, BlendEquationMode.FuncSubtract, BlendingFactorSrc.One, BlendingFactorDest.One, BlendingFactorSrc.One, BlendingFactorDest.One); } }

        #endregion
    }
}
