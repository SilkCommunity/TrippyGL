using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TrippyGL.Fonts.Rectpack;

namespace TrippyGL.Fonts
{
    /// <summary>
    /// Contains static methods that allow building a <see cref="TrippyFontFile"/> instance from one
    /// or multiple <see cref="IGlyphSource"/>-s.
    /// </summary>
    public static class FontBuilder
    {
        /// <summary>
        /// Creates a <see cref="TrippyFontFile"/> holding information for multiple fonts.
        /// </summary>
        /// <param name="glyphSources">The <see cref="IGlyphSource"/>-s for getting the information of each font.</param>
        /// <param name="backgroundColor">The background color of the generated image. Null for transparent.</param>
        public static TrippyFontFile CreateFontFile(ReadOnlySpan<IGlyphSource> glyphSources, Color? backgroundColor = null)
        {
            // We create all the TextureFontData-s and query their basic information from the glyph sources.
            TextureFontData[] fontDatas = new TextureFontData[glyphSources.Length];
            for (int i = 0; i < fontDatas.Length; i++)
            {
                fontDatas[i] = new TextureFontData()
                {
                    Size = glyphSources[i].Size,
                    FirstChar = glyphSources[i].FirstChar,
                    LastChar = glyphSources[i].LastChar,
                    Ascender = glyphSources[i].Ascender,
                    Descender = glyphSources[i].Descender,
                    LineGap = glyphSources[i].LineGap,
                    Name = glyphSources[i].Name
                };
            }

            // We count the total amount of characters in all glyph sources combined.
            int charCount = 0;
            for (int i = 0; i < glyphSources.Length; i++)
                charCount += fontDatas[i].CharCount;

            // We need to find a way to pack all the characters into a single texture.
            // For this we use the included Rectpack library in TrippyGL.Fonts.Rectpack.
            PackingRectangle[] packingRects = new PackingRectangle[charCount];

            // The way we identify the PackingRectangles is with their rect.Id property.
            // Since we have multiple fonts, we can't set these to the characters they represent
            // because there can be collisions. So the way we assign IDs will be basically like this:
            // The IDs for the characters of the first font start at 0 and go up to font.CharCount
            // inclusive. These are, of course, in order of character.
            // The second font then gets the range that starts right after where the first font's
            // range ends and gets enough range for all it's characters, etc etc.

            // So we need to know where the range of IDs for each font starts and end. We'll store it here:
            Span<int> idsStart = glyphSources.Length <= 96 ? stackalloc int[glyphSources.Length] : new int[glyphSources.Length];

            int packingRectCount = 0;
            for (int i = 0; i < fontDatas.Length; i++)
            {
                // We calculate the starting ID for the current glyph source.
                int idStart = i == 0 ? 0 : idsStart[i - 1] + fontDatas[i - 1].CharCount;
                idsStart[i] = idStart;

                // We go through all the characters in the current glyph source.
                for (int c = fontDatas[i].FirstChar; c <= fontDatas[i].LastChar; c++)
                {
                    // We get the size. If it is positive, we add a PackingRectangle to represent it.
                    System.Drawing.Point size = glyphSources[i].GetGlyphSize(c);
                    if (size.X > 0 && size.Y > 0)
                    {
                        // We add 2 to the width and height of the rectangle so chars have an empty border.
                        int id = idStart + c - fontDatas[i].FirstChar;
                        packingRects[packingRectCount++] = new PackingRectangle(0, 0, (uint)size.X + 2, (uint)size.Y + 2, id);
                    }
                }
            }

            // We trim extra elements off the packingRects array.
            if (packingRects.Length != packingRectCount)
                Array.Resize(ref packingRects, packingRectCount);

            // We use RectanglePacker to find a bin for all the rectangles.
            RectanglePacker.Pack(packingRects, out PackingRectangle bounds);

            Image<Rgba32> image = new Image<Rgba32>((int)bounds.Width, (int)bounds.Height);

            try
            {
                // First we clear the image to the specified background color, or transparent.
                Color bgColor = backgroundColor ?? Color.Transparent;
                image.Mutate(x => x.BackgroundColor(bgColor));

                // We create the source rectangles arrays for all the fontDatas.
                for (int i = 0; i < fontDatas.Length; i++)
                    fontDatas[i].SourceRectangles = new System.Drawing.Rectangle[fontDatas[i].CharCount];

                // We go through all the packing rectangles.
                for (int i = 0; i < packingRects.Length; i++)
                {
                    PackingRectangle rect = packingRects[i];

                    // We find which glyph source this rectangle belongs to.
                    int glyphSourceIndex = 0;
                    while (glyphSourceIndex + 1 < idsStart.Length && idsStart[glyphSourceIndex + 1] < rect.Id)
                        glyphSourceIndex++;

                    // We find which character this rectangle represents.
                    int charIndex = rect.Id - idsStart[glyphSourceIndex];
                    int charCode = charIndex + fontDatas[glyphSourceIndex].FirstChar;

                    // We draw the glyph onto the image at this rectangle's location.
                    glyphSources[glyphSourceIndex].DrawGlyphToImage(charCode, new System.Drawing.Point((int)rect.X + 1, (int)rect.Y + 1), image);

                    // We set the glyph's source to match this rectangle.
                    fontDatas[glyphSourceIndex].SourceRectangles![charIndex] = new System.Drawing.Rectangle((int)rect.X + 1, (int)rect.Y + 1, (int)rect.Width - 2, (int)rect.Height - 2);
                }

                // We go through all the fontDatas and set the remaining information.
                for (int i = 0; i < fontDatas.Length; i++)
                {
                    fontDatas[i].RenderOffsets = glyphSources[i].GetRenderOffsets();

                    glyphSources[i].GetAdvances(out fontDatas[i].Advances);

                    if (!glyphSources[i].TryGetKerning(out fontDatas[i].KerningOffsets))
                        fontDatas[i].KerningOffsets = null;
                }

                // Done!
                return new TrippyFontFile(fontDatas, image);
            }
            catch
            {
                // If anything failed, we dispose the image and re-throw the exception.
                image.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a <see cref="TrippyFontFile"/> holding information for a single font.
        /// </summary>
        /// <param name="glyphSources">The <see cref="IGlyphSource"/> for getting the information of the font.</param>
        /// <param name="backgroundColor">The background color of the generated image. Null for transparent.</param>
        public static TrippyFontFile CreateFontFile(IGlyphSource glyphSources, Color? backgroundColor = null)
        {
            return CreateFontFile(new IGlyphSource[] { glyphSources }, backgroundColor);
        }
    }
}
