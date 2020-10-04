using System;
using System.Drawing;
using System.Numerics;

#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

namespace TrippyGL
{
    public abstract class TextureFont : IDisposable
    {
        public const char NewlineIndicator = '\n';

        public readonly Texture2D Texture;

        public readonly float Size;

        public readonly string Name;

        public readonly char FirstChar;
        public readonly char LastChar;
        public readonly int CharCount;

        public readonly float LineAdvance;

        public readonly float Ascender;
        public readonly float Descender;
        public readonly float LineGap;

        private readonly Vector2[] renderOffsets;

        private readonly Rectangle[] sources;

        public TextureFont(Texture2D texture, float size, char firstChar, char lastChar, Vector2[] renderOffsets,
            Rectangle[] sources, float ascender, float descender, float lineGap, string name)
        {
            if (lastChar < firstChar)
                throw new ArgumentException(nameof(firstChar) + " must be lower or equal than " + nameof(lastChar) + ".");

            CharCount = lastChar - firstChar + 1;

            Texture = texture ?? throw new ArgumentNullException(nameof(texture));

            this.renderOffsets = renderOffsets ?? throw new ArgumentNullException(nameof(renderOffsets));
            if (renderOffsets.Length != CharCount)
                throw new ArgumentException("The length of the " + nameof(renderOffsets) + " array must match the amount of characters.", nameof(renderOffsets));

            this.sources = sources ?? throw new ArgumentNullException(nameof(sources));
            if (sources.Length != CharCount)
                throw new ArgumentException("The length of the " + nameof(sources) + " array must match the amount of characters.", nameof(sources));

            FirstChar = firstChar;
            LastChar = lastChar;
            Size = size;
            Name = name;

            LineAdvance = ascender - descender + lineGap;
            Ascender = ascender;
            Descender = descender;
            LineGap = lineGap;
        }

        public bool HasCharacter(char character)
        {
            return character >= FirstChar && character <= LastChar;
        }

        public bool HasCharacters(ReadOnlySpan<char> characters)
        {
            for (int i = 0; i < characters.Length; i++)
                if (characters[i] < FirstChar && characters[i] > LastChar)
                    return false;
            return true;
        }

        public abstract float GetAdvance(char character);

        public abstract Vector2 GetKerning(char fromChar, char toChar);

        public Rectangle GetSource(char character)
        {
            return sources[character - FirstChar];
        }

        public Vector2 GetRenderOffset(char character)
        {
            return renderOffsets[character - FirstChar];
        }

        public abstract Vector2 Measure(ReadOnlySpan<char> text);

        public abstract Vector2 MeasureLine(ReadOnlySpan<char> text);

        public float MeasureHeight(ReadOnlySpan<char> text)
        {
            float height = LineAdvance - LineGap;

            for (int i = 0; i < text.Length; i++)
                if (text[i] == NewlineIndicator)
                    height += LineAdvance;

            return height;
        }

        public void Dispose()
        {
            Texture.Dispose();
        }
    }
}
