using System;
using System.Numerics;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Represents a way to blend colors together when rendering. The blending function uses various parameters
    /// such as the output fragment color and the current pixel color, allowing you to define your own way to blend.
    /// </summary>
    public sealed class BlendState : IEquatable<BlendState>
    {
        /// <summary>Whether the blend mode is opaque. If this is true, all other <see cref="BlendState"/> properties are irrelevant.</summary>
        public bool IsOpaque;

        /// <summary>The equation mode for the RGB color components.</summary>
        public BlendEquationModeEXT EquationModeRGB;
        /// <summary>The equation mode for the Alpha color component.</summary>
        public BlendEquationModeEXT EquationModeAlpha;

        /// <summary>The source factor for the RGB color components.</summary>
        public BlendingFactor SourceFactorRGB;
        /// <summary>The source factor for the Alpha color component.</summary>
        public BlendingFactor SourceFactorAlpha;

        /// <summary>The destination factor for the RGB color components.</summary>
        public BlendingFactor DestFactorRGB;
        /// <summary>The destination factor for the Alpha color components.</summary>
        public BlendingFactor DestFactorAlpha;

        /// <summary>This color can be used for blending calculations with the blending factors for constant color.</summary>
        public Vector4 BlendColor;

        /// <summary>
        /// Creates a simple <see cref="BlendState"/> with additive blending equation parameters.
        /// </summary>
        /// <param name="isOpaque">Whether this <see cref="BlendState"/> is opaque.</param>
        public BlendState(bool isOpaque)
        {
            IsOpaque = isOpaque;
            EquationModeRGB = BlendEquationModeEXT.FuncAdd;
            EquationModeAlpha = BlendEquationModeEXT.FuncAdd;
            SourceFactorRGB = BlendingFactor.One;
            SourceFactorAlpha = BlendingFactor.One;
            DestFactorRGB = BlendingFactor.One;
            DestFactorAlpha = BlendingFactor.One;
        }

        /// <summary>
        /// Creates a <see cref="BlendState"/> with a simple color-blending equation.
        /// </summary>
        /// <param name="isOpaque">Whether this <see cref="BlendState"/> is opaque.</param>
        /// <param name="equationModeRgba">The equation mode to use for the RGBA values.</param>
        /// <param name="sourceFactorRgba">The source factor to use for the RGBA values.</param>
        /// <param name="destFactorRgba">The destination factor to use for the RGBA values.</param>
        /// <param name="blendColor">The equation-constant blending color.</param>
        public BlendState(bool isOpaque, BlendEquationModeEXT equationModeRgba, BlendingFactor sourceFactorRgba, BlendingFactor destFactorRgba, Vector4 blendColor = default)
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
        /// Creates a <see cref="BlendState"/> with specified separate equations, factors and a blend color.
        /// </summary>
        /// <param name="isOpaque">Whether this <see cref="BlendState"/> is opaque.</param>
        /// <param name="equationModeRgb">The equation mode to use for the RGB values.</param>
        /// <param name="equationModeAlpha">The equation mode to use for the Alpha value.</param>
        /// <param name="sourceFactorRgb">The source factor to use for the RGB values.</param>
        /// <param name="destFactorRgb">The destination factor to use for the RGB values.</param>
        /// <param name="sourceFactorAlpha">The source factor to use for the Alpha value.</param>
        /// <param name="destFactorAlpha">The destination factor to use for the Alpha value.</param>
        /// <param name="blendColor">The equation-constant blending color.</param>
        public BlendState(bool isOpaque, BlendEquationModeEXT equationModeRgb, BlendEquationModeEXT equationModeAlpha, BlendingFactor sourceFactorRgb, BlendingFactor destFactorRgb, BlendingFactor sourceFactorAlpha, BlendingFactor destFactorAlpha, Vector4 blendColor = default)
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
        /// Sets <see cref="EquationModeRGB"/> and <see cref="EquationModeAlpha"/>.
        /// </summary>
        public void SetEquationModeRgba(BlendEquationModeEXT equationModeRgba)
        {
            EquationModeRGB = equationModeRgba;
            EquationModeAlpha = equationModeRgba;
        }

        /// <summary>
        /// Sets <see cref="SourceFactorRGB"/> and <see cref="SourceFactorAlpha"/>.
        /// </summary>
        public void SetSourceFactorRgba(BlendingFactor sourceFactorRgba)
        {
            SourceFactorRGB = sourceFactorRgba;
            SourceFactorAlpha = sourceFactorRgba;
        }

        /// <summary>
        /// Sets <see cref="DestFactorRGB"/> and <see cref="DestFactorAlpha"/>.
        /// </summary>
        public void SetDestFactorRgba(BlendingFactor destFactorRgba)
        {
            DestFactorRGB = destFactorRgba;
            DestFactorAlpha = destFactorRgba;
        }

        /// <summary>
        /// Creates a new <see cref="BlendState"/> instance with the same values as this one.
        /// </summary>
        public BlendState Clone()
        {
            return new BlendState(IsOpaque, EquationModeRGB, EquationModeAlpha, SourceFactorRGB, DestFactorRGB, SourceFactorAlpha, DestFactorAlpha, BlendColor);
        }

        public override string ToString()
        {
            // TODO: Optimize

            if (IsOpaque)
                return "Opaque";

            System.Text.StringBuilder builder = new System.Text.StringBuilder(300);

            if (EquationModeRGB == EquationModeAlpha)
            {
                builder.Append(nameof(EquationModeRGB) + "A=\"");
                builder.Append(EquationModeRGB.ToString());
            }
            else
            {
                builder.Append(nameof(EquationModeRGB) + "=\"");
                builder.Append(EquationModeRGB.ToString());
                builder.Append("\", " + nameof(EquationModeAlpha) + "=\"");
                builder.Append(EquationModeAlpha.ToString());
            }

            if (SourceFactorRGB == SourceFactorAlpha)
            {
                builder.Append("\", " + nameof(SourceFactorRGB) + "A=\"");
                builder.Append(SourceFactorRGB.ToString());
            }
            else
            {
                builder.Append("\", " + nameof(SourceFactorRGB) + "=\"");
                builder.Append(SourceFactorRGB.ToString());
                builder.Append("\", " + nameof(SourceFactorAlpha) + "=\"");
                builder.Append(SourceFactorAlpha.ToString());
            }

            if (DestFactorRGB == DestFactorAlpha)
            {
                builder.Append("\", " + nameof(DestFactorRGB) + "A=\"");
                builder.Append(DestFactorRGB.ToString());
            }
            else
            {
                builder.Append("\", " + nameof(DestFactorRGB) + "=\"");
                builder.Append(DestFactorRGB.ToString());
                builder.Append("\", " + nameof(DestFactorAlpha) + "=\"");
                builder.Append(DestFactorAlpha.ToString());
            }

            builder.Append("\", " + nameof(BlendColor) + "=");
            builder.Append(BlendColor.ToString());

            return builder.ToString();
        }

        public bool Equals(BlendState other)
        {
            return other != null
                && IsOpaque == other.IsOpaque
                && EquationModeRGB == other.EquationModeRGB
                && EquationModeAlpha == other.EquationModeAlpha
                && SourceFactorRGB == other.SourceFactorRGB
                && SourceFactorAlpha == other.SourceFactorAlpha
                && DestFactorRGB == other.DestFactorRGB
                && DestFactorAlpha == other.DestFactorAlpha
                && BlendColor == other.BlendColor;
        }

        #region Static Members

        /// <summary>Gets a <see cref="BlendState"/> where fragments are written 'as is' without any mixing.</summary>
        public static BlendState Opaque => new BlendState(true);

        /// <summary>Gets a <see cref="BlendState"/> where fragments are mixed with non-premultiplied alpha.</summary>
        public static BlendState NonPremultiplied => new BlendState(false, BlendEquationModeEXT.FuncAdd, BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        /// <summary>Gets a <see cref="BlendState"/> where fragments are mixed using the source's alpha.</summary>
        public static BlendState AlphaBlend => new BlendState(false, BlendEquationModeEXT.FuncAdd, BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

        /// <summary>Gets a <see cref="BlendState"/> where fragments are mixed by adding them together.</summary>
        public static BlendState Additive => new BlendState(false, BlendEquationModeEXT.FuncAdd, BlendEquationModeEXT.FuncAdd,
            BlendingFactor.One, BlendingFactor.One, BlendingFactor.One, BlendingFactor.One);

        /// <summary>Gets a <see cref="BlendState"/> where fragments are mixed by subscracting them together.</summary>
        public static BlendState Substractive => new BlendState(false, BlendEquationModeEXT.FuncSubtract,
            BlendEquationModeEXT.FuncSubtract, BlendingFactor.One, BlendingFactor.One, BlendingFactor.One, BlendingFactor.One);

        #endregion
    }
}
