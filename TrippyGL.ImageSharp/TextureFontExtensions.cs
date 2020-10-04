using System;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

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

            fontData.TextureImage = new Image<Rgba32>((int)textureFont.Texture.Width, (int)textureFont.Texture.Height);
            textureFont.Texture.GetData(fontData.TextureImage);

            return fontData;
        }

        public static void SaveToFile(this TextureFont textureFont, string file)
        {
            CreateFontData(textureFont).SaveToFile(file);
        }
    }
}
