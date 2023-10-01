using System;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace TrippyGL.Fonts.Building
{
    /// <summary>
    /// Provides extension methods that allow building a <see cref="TrippyFontFile"/> instance
    /// from <see cref="SixLabors.Fonts"/> font objects or font files.
    /// </summary>
    public static class FontBuilderExtensions
    {
        /// <summary>
        /// Creates a <see cref="TrippyFontFile"/> holding information for multiple fonts.
        /// </summary>
        /// <remarks>All the fonts have the same character range.</remarks>
        public static TrippyFontFile CreateFontFile(ReadOnlySpan<Font> fonts, char firstChar = ' ', char lastChar = '~', Color? backgroundColor = null)
        {
            IGlyphSource[] glyphSources = new IGlyphSource[fonts.Length];
            for (int i = 0; i < fonts.Length; i++)
                glyphSources[i] = new FontGlyphSource(fonts[i], firstChar, lastChar);
            return FontBuilder.CreateFontFile(glyphSources, backgroundColor);
        }

        /// <summary>
        /// Creates a <see cref="TrippyFontFile"/> holding information for a single font.
        /// </summary>
        public static TrippyFontFile CreateFontFile(Font font, char firstChar = ' ', char lastChar = '~', Color? backgroundColor = null)
        {
            return FontBuilder.CreateFontFile(new FontGlyphSource(font, firstChar, lastChar), backgroundColor);
        }

        /// <summary>
        /// Creates a <see cref="TrippyFontFile"/> holding information for a single font.
        /// </summary>
        public static TrippyFontFile CreateFontFile(string fontFile, float size, char firstChar = ' ', char lastChar = '~', Color? backgroundColor = null)
        {
            using FileStream fileStream = new FileStream(fontFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            return CreateFontFile(fileStream, size, firstChar, lastChar, backgroundColor);
        }

        /// <summary>
        /// Creates a <see cref="TrippyFontFile"/> holding information for a single font.
        /// </summary>
        public static TrippyFontFile CreateFontFile(Stream fontStream, float size, char firstChar = ' ', char lastChar = '~', Color? backgroundColor = null)
        {
            return FontBuilder.CreateFontFile(new FontGlyphSource(new FontCollection().Add(fontStream).CreateFont(size), firstChar, lastChar), backgroundColor);
        }

        /// <summary>
        /// Creates a <see cref="TrippyFontFile"/> holding information for multiple fonts with the same size.
        /// </summary>
        /// <remarks>All the fonts have the same character range.</remarks>
        public static TrippyFontFile CreateFontFile(ReadOnlySpan<string> fontFiles, float size, char firstChar = ' ', char lastChar = '~', Color? backgroundColor = null)
        {
            IGlyphSource[] glyphSources = new IGlyphSource[fontFiles.Length];
            for (int i = 0; i < glyphSources.Length; i++)
                glyphSources[i] = new FontGlyphSource(new FontCollection().Add(fontFiles[i]).CreateFont(size), firstChar, lastChar);
            return FontBuilder.CreateFontFile(glyphSources, backgroundColor);
        }
    }
}
