using System;
using TrippyGL.ImageSharp;

namespace TrippyGL.Fonts.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="TrippyFontFile"/>.
    /// </summary>
    public static class TrippyFontFileExtensions
    {
        /// <summary>
        /// Creates an array of <see cref="TextureFont"/>-s from the <see cref="TrippyFontFile"/>.
        /// </summary>
        /// <param name="font">The <see cref="TrippyFontFile"/> to create fonts from.</param>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the <see cref="TextureFont"/>-s will use.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for the texture's font.</param>
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

        /// <summary>
        /// Creates a single <see cref="TextureFont"/> from the <see cref="TrippyFontFile"/>.
        /// </summary>
        /// <param name="font">The <see cref="TrippyFontFile"/> to create fonts from.</param>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the <see cref="TextureFont"/> will use.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for the texture's font.</param>
        public static TextureFont CreateFont(this TrippyFontFile font, GraphicsDevice graphicsDevice, bool generateMipmaps = false)
        {
            if (font.FontDatas.Length > 1)
                throw new FontLoadingException("Called " + nameof(CreateFont) + "() on a " + nameof(TrippyFontFile) + " with multiple fonts! Try " + nameof(CreateFonts) + "() instead.");

            return font.CreateFonts(graphicsDevice, generateMipmaps)[0];
        }
    }
}
