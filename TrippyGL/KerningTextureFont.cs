using System;
using System.Drawing;
using System.Numerics;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrippyGL
{
    /// <summary>
    /// A <see cref="TextureFont"/> where characters can have different advance values and kerning.
    /// </summary>
    public class KerningTextureFont : TextureFont
    {
        private readonly float[] advances;

        private readonly Vector2[,] kerningOffsets;

        /// <summary>The advance values for the characters in this font.</summary>
        public ReadOnlySpan<float> Advances => new ReadOnlySpan<float>(advances);

        /// <summary>
        /// Creates a <see cref="KerningTextureFont"/>.
        /// </summary>
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
            return advances[character - FirstChar];
        }

        public override Vector2 GetKerning(char fromChar, char toChar)
        {
            return kerningOffsets[fromChar - FirstChar, toChar - FirstChar];
        }

        public override Vector2 Measure(ReadOnlySpan<char> text)
        {
            if (text.IsEmpty)
                return default;

            Vector2 size = new Vector2(0, LineAdvance);

            float lineWidth = 0;
            bool notFirstInLine = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == NewlineIndicator)
                {
                    if (lineWidth > size.X)
                        size.X = lineWidth;
                    lineWidth = 0;
                    size.Y += LineAdvance;
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

            if (lineWidth > size.X)
                size.X = lineWidth;

            return size;
        }

        public override Vector2 MeasureLine(ReadOnlySpan<char> text)
        {
            if (text.IsEmpty)
                return default;

            float lineWidth = advances[text[0] - FirstChar];
            for (int i = 1; i < text.Length && text[i] != NewlineIndicator; i++)
                lineWidth += advances[text[i] - FirstChar] + kerningOffsets[text[i - 1] - FirstChar, text[i] - FirstChar].X;

            return new Vector2(lineWidth, LineAdvance - LineGap);
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
