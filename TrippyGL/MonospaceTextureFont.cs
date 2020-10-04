using System;
using System.Drawing;
using System.Numerics;

namespace TrippyGL
{
    /// <summary>
    /// A <see cref="TextureFont"/> where all the characters have the same advance value.
    /// </summary>
    public class MonospaceTextureFont : TextureFont
    {
        /// <summary>The advance value for any character in this font.</summary>
        public readonly float Advance;

        /// <summary>
        /// Creates a <see cref="MonospaceTextureFont"/>.
        /// </summary>
        public MonospaceTextureFont(Texture2D texture, float size, char firstChar, char lastChar, Vector2[] renderOffsets,
            Rectangle[] sources, float advance, float ascender, float descender, float lineGap, string name)
            : base(texture, size, firstChar, lastChar, renderOffsets, sources, ascender, descender, lineGap, name)
        {
            Advance = advance;
        }

        public override float GetAdvance(char character)
        {
            return Advance;
        }

        public override Vector2 GetKerning(char charFrom, char charTo)
        {
            return default;
        }

        public override Vector2 Measure(ReadOnlySpan<char> text)
        {
            if (text.IsEmpty)
                return default;

            Vector2 size = new Vector2(0, LineAdvance - LineGap);

            float lineWidth;
            int charsInLine = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == NewlineIndicator)
                {
                    lineWidth = charsInLine * Advance;
                    if (lineWidth > size.X)
                        size.X = lineWidth;
                    size.Y += LineAdvance;
                    charsInLine = 0;
                }
                else
                    charsInLine++;
            }

            lineWidth = charsInLine * Advance;
            if (lineWidth > size.X)
                size.X = lineWidth;

            return size;
        }

        public override Vector2 MeasureLine(ReadOnlySpan<char> text)
        {
            return new Vector2(text.Length * Advance, LineAdvance - LineGap);
        }
    }
}
