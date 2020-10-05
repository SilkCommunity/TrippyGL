using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TrippyGL.FontBuilding
{
    public interface IGlyphSource
    {
        public char FirstChar { get; }
        
        public char LastChar { get; }

        public float Size { get; }

        public string Name { get; }

        public float Ascender { get; }

        public float Descender { get; }

        public float LineGap { get; }

        /// <summary>
        /// Gets the advances for all glyphs.
        /// </summary>
        /// <returns>
        /// Whether the font is spaced. If this returned false, then the font is monospace and advances
        /// will be an array of length one, whose only element is the advance for all characters.
        /// </returns>
        public bool GetAdvances(out float[] advances);

        /// <summary>
        /// Tries to get kerning for all glyphs.
        /// </summary>
        /// <returns>
        /// Whether the font has kerning. If false, no kerning is used and kerningOffsets should be ignored.
        /// </returns>
        public bool TryGetKerning(out Vector2[,] kerningOffsets);

        public Vector2[] GetRenderOffsets();

        public System.Drawing.Point GetGlyphSize(int charCode);

        public void DrawGlyphToImage(int charCode, System.Drawing.Point location, Image<Rgba32> image);
    }
}
