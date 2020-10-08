using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TrippyGL.Fonts.Rectpack;

namespace TrippyGL.Fonts
{
    public static class FontBuilder
    {
        public static TrippyFontFile CreateFontFile(ReadOnlySpan<IGlyphSource> glyphSources, Color? backgroundColor = null)
        {
            int charCount = 0;
            for (int i = 0; i < glyphSources.Length; i++)
                charCount += glyphSources[i].LastChar - glyphSources[i].FirstChar + 1;

            PackingRectangle[] packingRects = new PackingRectangle[charCount];

            Span<int> idsStart = glyphSources.Length <= 96 ? stackalloc int[glyphSources.Length] : new int[glyphSources.Length];

            int packingRectCount = 0;
            for (int i = 0; i < glyphSources.Length; i++)
            {
                char firstChar = glyphSources[i].FirstChar;
                char lastChar = glyphSources[i].LastChar;

                int idStart = i == 0 ? 0 : idsStart[i - 1] + lastChar - firstChar + 1;
                idsStart[i] = idStart;

                for (int c = firstChar; c <= lastChar; c++)
                {
                    System.Drawing.Point size = glyphSources[i].GetGlyphSize(c);
                    if (size.X > 0 && size.Y > 0)
                    {
                        int id = idStart + c - firstChar;
                        packingRects[packingRectCount++] = new PackingRectangle(0, 0, (uint)size.X + 2, (uint)size.Y + 2, id);
                    }
                }
            }

            if (packingRects.Length != packingRectCount)
                Array.Resize(ref packingRects, packingRectCount);

            RectanglePacker.Pack(packingRects, out PackingRectangle bounds);

            Image<Rgba32> image = new Image<Rgba32>((int)bounds.Width, (int)bounds.Height);
            try
            {
                Color bgColor = backgroundColor ?? Color.Transparent;
                image.Mutate(x => x.BackgroundColor(bgColor));

                System.Drawing.Rectangle[][] sourceRectangles = new System.Drawing.Rectangle[glyphSources.Length][];
                for (int i = 0; i < sourceRectangles.Length; i++)
                    sourceRectangles[i] = new System.Drawing.Rectangle[glyphSources[i].LastChar - glyphSources[i].FirstChar + 1];

                for (int i = 0; i < packingRects.Length; i++)
                {
                    PackingRectangle rect = packingRects[i];

                    int glyphSourceIndex = 0;
                    while (glyphSourceIndex + 1 < idsStart.Length && idsStart[glyphSourceIndex + 1] < rect.Id)
                        glyphSourceIndex++;

                    int charIndex = rect.Id - idsStart[glyphSourceIndex];
                    int charCode = charIndex + glyphSources[glyphSourceIndex].FirstChar;

                    sourceRectangles[glyphSourceIndex][charIndex] = new System.Drawing.Rectangle((int)rect.X + 1, (int)rect.Y + 1, (int)rect.Width - 2, (int)rect.Height - 2);
                    glyphSources[glyphSourceIndex].DrawGlyphToImage(charCode, new System.Drawing.Point((int)rect.X + 1, (int)rect.Y + 1), image);
                }

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
                        Name = glyphSources[i].Name,
                        RenderOffsets = glyphSources[i].GetRenderOffsets(),
                        SourceRectangles = sourceRectangles[i]
                    };

                    glyphSources[i].GetAdvances(out fontDatas[i].Advances);

                    if (!glyphSources[i].TryGetKerning(out fontDatas[i].KerningOffsets))
                        fontDatas[i].KerningOffsets = null;
                }

                return new TrippyFontFile(fontDatas, image);
            }
            catch
            {
                image.Dispose();
                throw;
            }
        }

        public static TrippyFontFile CreateFontFile(IGlyphSource glyphSources, Color? backgroundColor = null)
        {
            return CreateFontFile(new IGlyphSource[] { glyphSources }, backgroundColor);
        }
    }
}
