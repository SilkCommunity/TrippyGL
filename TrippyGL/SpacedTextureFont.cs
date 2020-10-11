using System;
using System.Drawing;
using System.Numerics;

namespace TrippyGL
{
    /// <summary>
    /// A <see cref="TextureFont"/> where characters can have different advance values but no kerning.
    /// </summary>
    public class SpacedTextureFont : TextureFont
    {
        private readonly float[] advances;

        /// <summary>The advance values for the characters in this font.</summary>
        public ReadOnlySpan<float> Advances => new ReadOnlySpan<float>(advances);

        /// <summary>
        /// Creates a <see cref="SpacedTextureFont"/>.
        /// </summary>
        /// <remarks>
        /// Any array passed to this method will NOT be copied. The provided instance will be used instead.
        /// Holding on to a reference to these arrays and modifying them afterwards can have unexpected
        /// behavior.
        /// </remarks>
        public SpacedTextureFont(Texture2D texture, float size, char firstChar, char lastChar, Vector2[] renderOffsets,
               Rectangle[] sources, float[] advances, float ascender, float descender, float lineGap, string name)
               : base(texture, size, firstChar, lastChar, renderOffsets, sources, ascender, descender, lineGap, name)
        {
            this.advances = advances ?? throw new ArgumentNullException(nameof(advances));
            if (advances.Length != CharCount)
                throw new ArgumentException("The length of the " + nameof(advances) + " array must match the amount of characters.", nameof(advances));
        }

        public override float GetAdvance(char character)
        {
            return advances[character - FirstChar];
        }

        public override Vector2 GetKerning(char fromChar, char toChar)
        {
            return default;
        }

        public override Vector2 Measure(ReadOnlySpan<char> text)
        {
            if (text.IsEmpty)
                return default;

            int lineCount = 1;
            float maxLineWidth = 0;
            float lineWidth = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == NewlineIndicator)
                {
                    if (lineWidth > maxLineWidth)
                        maxLineWidth = lineWidth;
                    lineWidth = 0;
                    lineCount++;
                }
                else
                    lineWidth += advances[c - FirstChar];
            }

            return new Vector2(Math.Max(lineWidth, maxLineWidth), lineCount * LineAdvance);
        }

        public override Vector2 MeasureLine(ReadOnlySpan<char> text)
        {
            if (text.IsEmpty)
                return default;

            float lineWidth = advances[text[0] - FirstChar];
            for (int i = 1; i < text.Length && text[i] != NewlineIndicator; i++)
                lineWidth += advances[text[i] - FirstChar];

            return new Vector2(lineWidth, LineAdvance);
        }
    }
}
