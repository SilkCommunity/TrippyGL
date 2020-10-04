using System;
using System.IO;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TrippyGL.ImageSharp;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA2000 // Dispose objects before losing scope

namespace TrippyGL.ImageSharp
{
    public static class TextureFontExtensions
    {
        public static TextureFontData CreateFontData(this TextureFont textureFont)
        {
            if (textureFont == null)
                throw new ArgumentNullException(nameof(textureFont));

            TextureFontData fontData = new TextureFontData()
            {
                Size = textureFont.Size,
                Name = textureFont.Name,
                FirstChar = textureFont.FirstChar,
                LastChar = textureFont.LastChar,
                Ascender = textureFont.Ascender,
                Descender = textureFont.Descender,
                LineGap = textureFont.LineGap
            };

            int charCount = fontData.CharCount;
            fontData.Advances = new float[textureFont is MonospaceTextureFont ? 1 : charCount];
            for (int i = 0; i < fontData.Advances.Length; i++)
                fontData.Advances[i] = textureFont.GetAdvance((char)(i + fontData.FirstChar));

            fontData.RenderOffsets = new Vector2[charCount];
            for (int i = 0; i < fontData.RenderOffsets.Length; i++)
                fontData.RenderOffsets[i] = textureFont.GetRenderOffset((char)(i + fontData.FirstChar));

            fontData.SourceRectangles = new System.Drawing.Rectangle[charCount];
            for (int i = 0; i < fontData.SourceRectangles.Length; i++)
                fontData.SourceRectangles[i] = textureFont.GetSource((char)(i + fontData.FirstChar));

            if (textureFont is KerningTextureFont)
            {
                fontData.KerningOffsets = new Vector2[charCount, charCount];
                for (int i = 0; i < fontData.KerningOffsets.GetLength(0); i++)
                    for (int c = 0; c < fontData.KerningOffsets.GetLength(1); c++)
                        fontData.KerningOffsets[i, c] = textureFont.GetKerning((char)(i + fontData.FirstChar), (char)(c + fontData.FirstChar));
            }

            return fontData;
        }

        public static TextureFont[] CreateFonts(GraphicsDevice graphicsDevice, TextureFontData[] fontDatas, Image<Rgba32> image, bool generateMipmaps = false)
        {
            Texture2D texture = Texture2DExtensions.FromImage(graphicsDevice, image, generateMipmaps);

            try
            {
                TextureFont[] fonts = new TextureFont[fontDatas.Length];

                for (int i = 0; i < fonts.Length; i++)
                    fonts[i] = fontDatas[i].CreateFont(texture);
                return fonts;
            }
            catch
            {
                texture.Dispose();
                throw;
            }
        }

        public static TextureFont[] FromFileMultiple(GraphicsDevice graphicsDevice, string file, bool generateMipmaps = false)
        {
            using FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            return FromStreamMultiple(graphicsDevice, new BinaryReader(fileStream), generateMipmaps);
        }

        public static TextureFont[] FromStreamMultiple(GraphicsDevice graphicsDevice, Stream stream, bool generateMipmaps = false)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return FromStreamMultiple(graphicsDevice, new BinaryReader(stream), generateMipmaps);
        }

        public static TextureFont[] FromStreamMultiple(GraphicsDevice graphicsDevice, BinaryReader streamReader, bool generateMipmaps = false)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            if (streamReader == null)
                throw new ArgumentNullException(nameof(streamReader));

            TextureFontData[] fontDatas = TextureFontData.FromStream(streamReader, out Image<Rgba32> image);
            using (image)
            {
                return CreateFonts(graphicsDevice, fontDatas, image, generateMipmaps);
            }
        }
    }
}
