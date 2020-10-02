using System;
using System.Drawing;
using System.Numerics;

namespace TrippyGL
{
    public class MonospaceTextureFont : TextureFont
    {
        public readonly float Advance;

        public MonospaceTextureFont(Texture2D texture, float size, char firstChar, char lastChar, Vector2[] renderOffsets,
            Rectangle[] sources, float advance, float lineAdvance, float ascender, float descender, float lineGap)
            : base(texture, size, firstChar, lastChar, renderOffsets, sources, lineAdvance, ascender, descender, lineGap)
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
