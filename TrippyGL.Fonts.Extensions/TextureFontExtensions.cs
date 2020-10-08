using System.IO;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA2000 // Dispose objects before losing scope

namespace TrippyGL.Fonts.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="TextureFont"/>.
    /// </summary>
    public static class TextureFontExtensions
    {
        /// <summary>
        /// Creates an array of <see cref="TextureFont"/>-s by loading them from a trippy font file.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the <see cref="TextureFont"/>-s will use.</param>
        /// <param name="file">The file on disk where the trippy font file is located.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for the texture's font.</param>
        public static TextureFont[] FromFile(GraphicsDevice graphicsDevice, string file, bool generateMipmaps = false)
        {
            using TrippyFontFile fontFile = TrippyFontFile.FromFile(file);
            return fontFile.CreateFonts(graphicsDevice, generateMipmaps);
        }

        /// <summary>
        /// Creates an array of <see cref="TextureFont"/>-s by loading them from a trippy font file.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the <see cref="TextureFont"/>-s will use.</param>
        /// <param name="stream">The stream where the trippy font file is located.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for the texture's font.</param>
        public static TextureFont[] FromStream(GraphicsDevice graphicsDevice, Stream stream, bool generateMipmaps = false)
        {
            using TrippyFontFile fontFile = TrippyFontFile.FromStream(stream);
            return fontFile.CreateFonts(graphicsDevice, generateMipmaps);
        }

        /// <summary>
        /// Creates an array of <see cref="TextureFont"/>-s by loading them from a trippy font file.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the <see cref="TextureFont"/>-s will use.</param>
        /// <param name="streamReader">The stream where the trippy font file is located.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for the texture's font.</param>
        public static TextureFont[] FromStream(GraphicsDevice graphicsDevice, BinaryReader streamReader, bool generateMipmaps = false)
        {
            using TrippyFontFile fontFile = TrippyFontFile.FromStream(streamReader);
            return fontFile.CreateFonts(graphicsDevice, generateMipmaps);
        }
    }
}
