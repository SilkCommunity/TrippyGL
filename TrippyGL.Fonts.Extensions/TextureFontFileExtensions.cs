using System;
using TrippyGL.ImageSharp;

namespace TrippyGL.Fonts.Extensions
{
    public static class TextureFontFileExtensions
    {
        public static TextureFont[] CreateFonts(this TrippyFontFile font, GraphicsDevice graphicsDevice, bool generateMipmaps = false)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            if (font == null)
                throw new ArgumentNullException(nameof(font));

            font.ThrowIfAnyNull();

            Texture2D texture = new Texture2D(graphicsDevice, (uint)font.Image.Width, (uint)font.Image.Height, generateMipmaps);
            try
            {
                TextureFont[] textureFonts = new TextureFont[font.FontDatas.Length];
                for (int i = 0; i < textureFonts.Length; i++)
                    textureFonts[i] = font.FontDatas[i].CreateFont(texture);

                texture.SetData(font.Image);
                return textureFonts;
            }
            catch
            {
                texture.Dispose();
                throw;
            }
        }

        public static TextureFont CreateFont(this TrippyFontFile font, GraphicsDevice graphicsDevice, bool generateMipmaps = false)
        {
            return font.CreateFonts(graphicsDevice, generateMipmaps)[0];
        }
    }
}
