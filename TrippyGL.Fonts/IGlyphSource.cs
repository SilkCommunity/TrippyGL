using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TrippyGL.Fonts
{
    /// <summary>
    /// Defines methods and properties used by <see cref="FontBuilder"/> for building
    /// <see cref="TrippyFontFile"/> instances.
    /// </summary>
    public interface IGlyphSource
    {
        /// <summary>The first character in the range the font will contain.</summary>
        public char FirstChar { get; }

        /// <summary>The last (inclusive) character in the range the font will contain.</summary>
        public char LastChar { get; }

        /// <summary>The size of the font, typically measured in pixels.</summary>
        public float Size { get; }

        /// <summary>The name of the font. Can be null, but no longer than <see cref="TextureFontData.MaxFontNameLength"/>.</summary>
        public string Name { get; }

        /// <summary>The distance between the baseline and the highest glyph's highest point. Typically positive.</summary>
        public float Ascender { get; }

        /// <summary>The distance between the baseline and the lowest glyph's lowest point. Typically negative.</summary>
        public float Descender { get; }

        /// <summary>The distance between the lowest point of a line and the highest point of the next line.</summary>
        public float LineGap { get; }

        /// <summary>
        /// Gets the advances for all glyphs. 
        /// </summary>
        /// <returns>
        /// Whether the font is spaced. If this returned false, then the font is monospace and advances
        /// will be an array of length one, whose only element is the advance for all characters.
        /// </returns>
        /// <remarks>
        /// The implementation shouldn't hold a reference to the returned array. Users of this interface
        /// are allowed to use the same array instance without having to copy the data to a new location.
        /// </remarks>
        public bool GetAdvances(out float[] advances);

        /// <summary>
        /// Tries to get kerning for all glyphs.
        /// </summary>
        /// <returns>
        /// Whether the font has kerning. If false, no kerning is used and kerningOffsets should be ignored.
        /// </returns>
        /// <remarks>
        /// The implementation shouldn't hold a reference to the returned array. Users of this interface
        /// are allowed to use the same array instance without having to copy the data to a new location.
        /// </remarks>
        public bool TryGetKerning(out Vector2[,] kerningOffsets);

        /// <summary>
        /// Gets the render offsets for all glyphs.
        /// </summary>
        /// <remarks>
        /// The implementation shouldn't hold a reference to the returned array. Users of this interface
        /// are allowed to use the same array instance without having to copy the data to a new location.
        /// </remarks>
        public Vector2[] GetRenderOffsets();

        /// <summary>
        /// Gets the size in pixels of the area needed for drawing a specific character.
        /// </summary>
        /// <remarks>
        /// A size of zero means a glyph does not need drawing (for example, a space character).
        /// </remarks>
        public System.Drawing.Point GetGlyphSize(int charCode);

        /// <summary>
        /// Draws a character into the given image, at the specified location.
        /// </summary>
        /// <remarks>
        /// This operation shouldn't touch anything outside the area defined by the given location
        /// and the size provided for the same character in <see cref="GetGlyphSize(int)"/>.
        /// </remarks>
        public void DrawGlyphToImage(int charCode, System.Drawing.Point location, Image<Rgba32> image);
    }
}
