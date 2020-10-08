using System.IO;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA2000 // Dispose objects before losing scope

namespace TrippyGL.Fonts.Extensions
{
    public static class TextureFontExtensions
    {
        public static TextureFont[] FromFile(GraphicsDevice graphicsDevice, string file, bool generateMipmaps = false)
        {
            using TrippyFontFile fontFile = TrippyFontFile.FromFile(file);
            return fontFile.CreateFonts(graphicsDevice, generateMipmaps);
        }

        public static TextureFont[] FromStream(GraphicsDevice graphicsDevice, Stream stream, bool generateMipmaps = false)
        {
            using TrippyFontFile fontFile = TrippyFontFile.FromStream(stream);
            return fontFile.CreateFonts(graphicsDevice, generateMipmaps);
        }

        public static TextureFont[] FromStream(GraphicsDevice graphicsDevice, BinaryReader streamReader, bool generateMipmaps = false)
        {
            using TrippyFontFile fontFile = TrippyFontFile.FromStream(streamReader);
            return fontFile.CreateFonts(graphicsDevice, generateMipmaps);
        }
    }
}
