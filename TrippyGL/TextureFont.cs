using System;
using System.Drawing;
using System.Numerics;

#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

namespace TrippyGL
{
    /// <summary>
    /// Represent a range of drawable text characters, where all the characters are stored
    /// inside a <see cref="Texture2D"/>. Also provides methods for measuring text.
    /// </summary>
    public abstract class TextureFont : IDisposable
    {
        /// <summary>The character <see cref="TextureFont"/>-s use to indicate a new line.</summary>
        public const char NewlineIndicator = '\n';

        /// <summary>The <see cref="Texture2D"/> containing this <see cref="TextureFont"/>'s characters.</summary>
        public readonly Texture2D Texture;

        /// <summary>This <see cref="TextureFont"/>'s size, typically measured in pixels.</summary>
        public readonly float Size;

        /// <summary>This <see cref="TextureFont"/>'s name.</summary>
        public readonly string Name;

        /// <summary>The lowest character available in this <see cref="TextureFont"/>.</summary>
        public readonly char FirstChar;

        /// <summary>The highest character available in this <see cref="TextureFont"/>.</summary>
        public readonly char LastChar;

        /// <summary>The amount of characters this <see cref="TextureFont"/> contains.</summary>
        /// <remarks>This is equal to LastChar - FirstChar + 1.</remarks>
        public readonly int CharCount;

        /// <summary>The baseline-to-baseline distance to advance when drawing a new line with this <see cref="TextureFont"/>.</summary>
        public readonly float LineAdvance;

        /// <summary>The distance between the baseline and the highest glyph's highest point. Typically positive.</summary>
        public readonly float Ascender;

        /// <summary>The distance between the baseline and the lowest glyph's lowest point. Typically negative.</summary>
        public readonly float Descender;

        /// <summary>The distance to leave in between two lines of text when drawing with this <see cref="TextureFont"/>.</summary>
        public readonly float LineGap;

        /// <summary>Offsets that should be directly applied to the characters when drawing them.</summary>
        private readonly Vector2[] renderOffsets;

        /// <summary>The areas in <see cref="Texture"/> where each character is located.</summary>
        private readonly Rectangle[] sources;

        /// <summary>
        /// Creates a <see cref="TextureFont"/>.
        /// </summary>
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

        /// <summary>
        /// Returns whether this <see cref="TextureFont"/> can draw a specified character.
        /// </summary>
        public bool HasCharacter(char character)
        {
            return character >= FirstChar && character <= LastChar;
        }

        /// <summary>
        /// Returns whether this <see cref="TextureFont"/> can draw all the characters in the specified string.
        /// </summary>
        public bool HasCharacters(ReadOnlySpan<char> characters)
        {
            for (int i = 0; i < characters.Length; i++)
                if (characters[i] < FirstChar && characters[i] > LastChar)
                    return false;
            return true;
        }

        /// <summary>
        /// Gets the distance to advance by after drawing a character.
        /// </summary>
        public abstract float GetAdvance(char character);

        /// <summary>
        /// Gets an offset that should be applied between two characters when drawing.
        /// </summary>
        public abstract Vector2 GetKerning(char fromChar, char toChar);

        /// <summary>
        /// Gets the area in the <see cref="Texture"/> where a specified character is found.
        /// </summary>
        public Rectangle GetSource(char character)
        {
            return sources[character - FirstChar];
        }

        /// <summary>
        /// Gets the offset that should be applied directly to a character while drawing.
        /// </summary>
        public Vector2 GetRenderOffset(char character)
        {
            return renderOffsets[character - FirstChar];
        }

        /// <summary>
        /// Measures the size in pixels of a string of text.
        /// </summary>
        public abstract Vector2 Measure(ReadOnlySpan<char> text);

        /// <summary>
        /// Measures the size in pixels of a single line of text.
        /// </summary>
        /// <remarks>
        /// For monospaced fonts, this is a O(1) operation that doesn't validate characters.<para/>
        /// For non-monospaced fonts, invalid characters (including newline) will throw an exception.
        /// </remarks>
        public abstract Vector2 MeasureLine(ReadOnlySpan<char> text);

        /// <summary>
        /// Measures the height of the given text.
        /// </summary>
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
