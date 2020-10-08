using System;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace TrippyGL.Fonts.Extensions
{
    public static class FontBuilderExtensions
    {
        public static TrippyFontFile CreateFontFile(ReadOnlySpan<Font> fonts, char firstChar = ' ', char lastChar = '~', Color? backgroundColor = null)
        {
            IGlyphSource[] glyphSources = new IGlyphSource[fonts.Length];
            for (int i = 0; i < fonts.Length; i++)
                glyphSources[i] = new FontGlyphSource(fonts[i], firstChar, lastChar);
            return FontBuilder.CreateFontFile(glyphSources, backgroundColor);
        }

        public static TrippyFontFile CreateFontFile(ReadOnlySpan<IFontInstance> fonts, float size, char firstChar = ' ', char lastChar = '~', Color? backgroundColor = null)
        {
            IGlyphSource[] glyphSources = new IGlyphSource[fonts.Length];
            for (int i = 0; i < fonts.Length; i++)
                glyphSources[i] = new FontGlyphSource(fonts[i], size, firstChar, lastChar);
            return FontBuilder.CreateFontFile(glyphSources, backgroundColor);
        }

        public static TrippyFontFile CreateFontFile(Font font, char firstChar = ' ', char lastChar = '~', Color? backgroundColor = null)
        {
            return FontBuilder.CreateFontFile(new FontGlyphSource(font, firstChar, lastChar), backgroundColor);
        }

        public static TrippyFontFile CreateFontFile(IFontInstance font, float size, char firstChar = ' ', char lastChar = '~', Color? backgroundColor = null)
        {
            return FontBuilder.CreateFontFile(new FontGlyphSource(font, size, firstChar, lastChar), backgroundColor);
        }

        public static TrippyFontFile CreateFontFile(string fontFile, float size, char firstChar = ' ', char lastChar = '~', Color? backgroundColor = null)
        {
            using FileStream fileStream = new FileStream(fontFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            return CreateFontFile(fileStream, size, firstChar, lastChar, backgroundColor);
        }

        public static TrippyFontFile CreateFontFile(Stream fontStream, float size, char firstChar = ' ', char lastChar = '~', Color? backgroundColor = null)
        {
            return CreateFontFile(FontInstance.LoadFont(fontStream), size, firstChar, lastChar, backgroundColor);
        }
    }
}
