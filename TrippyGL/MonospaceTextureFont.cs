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
        /// <remarks>
        /// Any array passed to this method will NOT be copied. The provided instance will be used instead.
        /// Holding on to a reference to these arrays and modifying them afterwards can have unexpected
        /// behavior.
        /// </remarks>
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

            int lineCount = 1;
            int maxCharsPerLine = 0;
            int charsInLine = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == NewlineIndicator)
                {
                    if (charsInLine > maxCharsPerLine)
                        maxCharsPerLine = charsInLine;
                    lineCount++;
                    charsInLine = 0;
                }
                else
                    charsInLine++;
            }

            if (charsInLine > maxCharsPerLine)
                maxCharsPerLine = charsInLine;

            return new Vector2(Advance * maxCharsPerLine, lineCount * LineAdvance);
        }

        public override Vector2 MeasureLine(ReadOnlySpan<char> text)
        {
            return new Vector2(text.Length * Advance, LineAdvance);
        }
    }
}
