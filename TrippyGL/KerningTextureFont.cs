using System;
using System.Drawing;
using System.Numerics;

namespace TrippyGL
{
    /// <summary>
    /// A <see cref="TextureFont"/> where characters can have different advance values and kerning.
    /// </summary>
    public sealed class KerningTextureFont : TextureFont
    {
        private readonly float[] advances;

        /// <summary>The kerning offsets for each character. This are in order [from, to].</summary>
        private readonly Vector2[,] kerningOffsets;

        /// <summary>The advance values for the characters in this font.</summary>
        public ReadOnlySpan<float> Advances => new ReadOnlySpan<float>(advances);

        /// <summary>
        /// Creates a <see cref="KerningTextureFont"/>.
        /// </summary>
        /// <remarks>
        /// Any array passed to this method will NOT be copied. The provided instance will be used instead.
        /// Holding on to a reference to these arrays and modifying them afterwards can have unexpected
        /// behavior.
        /// </remarks>
        public KerningTextureFont(Texture2D texture, float size, char firstChar, char lastChar, Vector2[] renderOffsets,
               Rectangle[] sources, Vector2[,] kerningOffsets, float[] advances, float ascender,
               float descender, float lineGap, string name)
               : base(texture, size, firstChar, lastChar, renderOffsets, sources, ascender, descender, lineGap, name)
        {
            this.advances = advances ?? throw new ArgumentNullException(nameof(advances));
            if (advances.Length != CharCount)
                throw new ArgumentException("The length of the " + nameof(advances) + " array must match the amount of characters.", nameof(advances));

            this.kerningOffsets = kerningOffsets ?? throw new ArgumentNullException(nameof(kerningOffsets));
            if (kerningOffsets.GetLength(0) != CharCount || kerningOffsets.GetLength(1) != CharCount)
                throw new ArgumentException("The length in both dimentions of the " + nameof(kerningOffsets) + " array must match the amount of characters.", nameof(kerningOffsets));
        }

        public override float GetAdvance(char character)
        {
            ValidateCharAvailable(character);
            return advances[character - FirstChar];
        }

        public override Vector2 GetKerning(char fromChar, char toChar)
        {
            ValidateCharAvailable(fromChar);
            ValidateCharAvailable(toChar);
            return kerningOffsets[fromChar - FirstChar, toChar - FirstChar];
        }

        public override Vector2 Measure(ReadOnlySpan<char> text)
        {
            if (text.IsEmpty)
                return default;

            int lineCount = 1;
            float maxLineWidth = 0;
            float lineWidth = 0;
            bool notFirstInLine = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == NewlineIndicator)
                {
                    if (lineWidth > maxLineWidth)
                        maxLineWidth = lineWidth;
                    lineWidth = 0;
                    lineCount++;
                    notFirstInLine = false;
                }
                else
                {
                    lineWidth += advances[c - FirstChar];
                    if (notFirstInLine)
                        lineWidth += kerningOffsets[text[i - 1] - FirstChar, c - FirstChar].X;
                    else
                        notFirstInLine = true;
                }
            }

            return new Vector2(Math.Max(lineWidth, maxLineWidth), lineCount * LineAdvance);
        }

        public override Vector2 MeasureLine(ReadOnlySpan<char> text)
        {
            if (text.IsEmpty)
                return default;

            float lineWidth = advances[text[0] - FirstChar];
            for (int i = 1; i < text.Length && text[i] != NewlineIndicator; i++)
                lineWidth += advances[text[i] - FirstChar] + kerningOffsets[text[i - 1] - FirstChar, text[i] - FirstChar].X;

            return new Vector2(lineWidth, LineAdvance);
        }

        /// <summary>
        /// Creates a <see cref="SpacedTextureFont"/> that contains the exact same information
        /// as this <see cref="KerningTextureFont"/>, except for the kerning.
        /// </summary>
        public SpacedTextureFont ToKerningless()
        {
            return new SpacedTextureFont(Texture, Size, FirstChar, LastChar, renderOffsets, sources, advances,
                Ascender, Descender, LineGap, Name);
        }
    }
}
