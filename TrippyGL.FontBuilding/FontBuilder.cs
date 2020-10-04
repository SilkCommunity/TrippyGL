using System;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TrippyGL.ImageSharp;
using TrippyGL.Utils.Rectpack;

namespace TrippyGL.FontBuilding
{
    public static class FontBuilder
    {
        public static TextureFontData[] CreateFontDatas(ReadOnlySpan<IGlyphSource> glyphSources, out Image<Rgba32> image, Color backgroundColor)
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

            image = new Image<Rgba32>((int)bounds.Width, (int)bounds.Height);
            image.Mutate(x => x.Fill(backgroundColor));

            System.Drawing.Rectangle[][] sourceRectangles = new System.Drawing.Rectangle[glyphSources.Length][];
            for (int i = 0; i < sourceRectangles.Length; i++)
                sourceRectangles[i] = new System.Drawing.Rectangle[glyphSources[i].LastChar - glyphSources[i].FirstChar + 1];

            for (int i = 0; i < packingRects.Length; i++)
            {
                PackingRectangle rect = packingRects[i];

                int glyphSourceIndex = 0;
                for (; idsStart[glyphSourceIndex] > rect.Id; glyphSourceIndex++) ;

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

            return fontDatas;
        }

        public static TextureFontData CreateFontData(IGlyphSource glyphSources, out Image<Rgba32> image, Color backgroundColor)
        {
            return CreateFontDatas(new IGlyphSource[] { glyphSources }, out image, backgroundColor)[0];
        }
    }
}
