using System;
using System.IO;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace TrippyGL
{
    /// <summary>
    /// Extension methods for <see cref="Texture1D"/>.
    /// </summary>
    public static class Texture1DExtensions
    {
        /// <summary>
        /// Creates a <see cref="Texture1D"/> from an <see cref="Image{Rgba32}"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the resource will use.</param>
        /// <param name="image">The image to create the <see cref="Texture1D"/> with.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for the <see cref="Texture1D"/>.</param>
        public static Texture1D FromImage(GraphicsDevice graphicsDevice, Image<Rgba32> image, bool generateMipmaps = false)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (!image.TryGetSinglePixelSpan(out Span<Rgba32> pixels))
                throw new InvalidDataException(ImageUtils.ImageNotContiguousError);
            Texture1D texture = new Texture1D(graphicsDevice, (uint)(image.Width * image.Height));
            texture.SetData<Rgba32>(pixels, 0, PixelFormat.Rgba);

            if (generateMipmaps)
                texture.GenerateMipmaps();

            return texture;
        }

        /// <summary>
        /// Creates a <see cref="Texture1D"/> by loading an image from a file.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the resource will use.</param>
        /// <param name="file">The file containing the image to create the <see cref="Texture1D"/> with.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for the <see cref="Texture1D"/>.</param>
        public static Texture1D FromFile(GraphicsDevice graphicsDevice, string file, bool generateMipmaps = false)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(file);
            return FromImage(graphicsDevice, image, generateMipmaps);
        }

        /// <summary>
        /// Saves this <see cref="Texture1D"/>'s image as an image file.
        /// If the file already exists, it will be replaced.
        /// </summary>
        /// <param name="texture">The <see cref="Texture1D"/> whose image to save.</param>
        /// <param name="file">The name of the file where the image will be saved.</param>
        /// <param name="imageFormat">The format the image will be saved as.</param>
        public static void SaveAsImage(this Texture1D texture, string file, SaveImageFormat imageFormat)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (string.IsNullOrEmpty(file))
                throw new ArgumentException("You must specify a file name", nameof(file));

            if (texture.ImageFormat != TextureImageFormat.Color4b)
                throw new InvalidOperationException("In order to save a texture as image, it must be in " + nameof(Color4b) + " format");

            IImageFormat format = ImageUtils.GetFormatFor(imageFormat);
            using Image<Rgba32> image = new Image<Rgba32>((int)texture.Width, 1);
            if (!image.TryGetSinglePixelSpan(out Span<Rgba32> pixels))
                throw new InvalidDataException(ImageUtils.ImageNotContiguousError);
            texture.GetData(pixels, PixelFormat.Rgba);

            using FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
            image.Save(fileStream, format);
        }
    }
}
