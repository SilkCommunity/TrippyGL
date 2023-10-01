using System;
using System.IO;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace TrippyGL.ImageSharp
{
    /// <summary>
    /// Provides extension methods for <see cref="Texture1D"/>.
    /// </summary>
    public static class Texture1DExtensions
    {
        /// <summary>
        /// Sets the data of an area of the <see cref="Texture1D"/> from an <see cref="Image{Rgba32}"/>.
        /// </summary>
        /// <param name="texture">The <see cref="Texture1D"/> whose image to set.</param>
        /// <param name="x">The x position of the first pixel to set.</param>
        /// <param name="image">The image to set the data from. The width is taken from here.</param>
        public static void SetData(this Texture1D texture, int x, Image<Rgba32> image)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (texture.ImageFormat != TextureImageFormat.Color4b)
                throw new ArgumentException(nameof(texture), ImageUtils.TextureFormatMustBeColor4bError);

            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> pixels))
            {
                texture.SetData<Rgba32>(pixels.Span, 0, PixelFormat.Rgba);
            }
            else
            {
                image.ProcessPixelRows(accessor =>
                {
                    for (int yi = 0; yi < accessor.Height; yi++)
                        texture.SetData<Rgba32>(accessor.GetRowSpan(yi), x, PixelFormat.Rgba);
                });
            }
        }

        /// <summary>
        /// Sets the data of the entire <see cref="Texture1D"/> from an <see cref="Image{Rgba32}"/>.
        /// </summary>
        /// <param name="texture">The <see cref="Texture1D"/> whose image to set.</param>
        /// <param name="image">The image to set the data from. The width is taken from here.</param>
        public static void SetData(this Texture1D texture, Image<Rgba32> image)
        {
            SetData(texture, 0, image);
        }

        /// <summary>
        /// Gets the data of the entire <see cref="Texture1D"/>.
        /// </summary>
        /// <param name="texture">The <see cref="Texture1D"/> to get the image from.</param>
        /// <param name="image">The image in which to write the pixel data.</param>
        public static void GetData(this Texture1D texture, Image<Rgba32> image)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (texture.ImageFormat != TextureImageFormat.Color4b)
                throw new ArgumentException(nameof(texture), ImageUtils.TextureFormatMustBeColor4bError);

            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (image.Width * image.Height != texture.Width)
                throw new ArgumentException(nameof(image), ImageUtils.ImageSizeMustMatchTextureSizeError);

            if (image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> pixels))
            {
                texture.GetData(pixels.Span, PixelFormat.Rgba);
            }
            else
            {
                // Similar to the copy from Texture2D, to allow for a Texture1D to be dumped into a 2D image.
                Rgba32[] data = new Rgba32[texture.Width];
                texture.GetData<Rgba32>(data);

                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgba32> dataRowSpan = data.AsSpan(image.Width * y, image.Width);
                        Span<Rgba32> imageRowSpan = accessor.GetRowSpan(y);

                        dataRowSpan.CopyTo(imageRowSpan);
                    }
                });
            }
        }

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

            Texture1D texture = new Texture1D(graphicsDevice, (uint)(image.Width * image.Height));
            try
            {
                texture.SetData(image);

                if (generateMipmaps)
                    texture.GenerateMipmaps();

                return texture;
            }
            catch
            {
                texture.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a <see cref="Texture1D"/> from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the resource will use.</param>
        /// <param name="stream">The stream from which to load an image.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for the <see cref="Texture1D"/>.</param>
        public static Texture1D FromStream(GraphicsDevice graphicsDevice, Stream stream, bool generateMipmaps = false)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(stream);
            return FromImage(graphicsDevice, image, generateMipmaps);
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
        /// Saves this <see cref="Texture1D"/>'s image to a stream.
        /// </summary>
        /// <param name="texture">The <see cref="Texture1D"/> whose image to save.</param>
        /// <param name="stream">The stream to save the texture image to.</param>
        /// <param name="imageFormat">The format the image will be saved as.</param>
        public static void SaveAsImage(this Texture1D texture, Stream stream, SaveImageFormat imageFormat)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (stream == null)
                throw new ArgumentException(nameof(stream));

            IImageFormat format = ImageUtils.GetFormatFor(imageFormat);
            using Image<Rgba32> image = new Image<Rgba32>((int)texture.Width, 1);
            texture.GetData(image);
            image.Save(stream, format);
        }

        /// <summary>
        /// Saves this <see cref="Texture1D"/>'s image to a file.
        /// If the file already exists, it will be replaced.
        /// </summary>
        /// <param name="texture">The <see cref="Texture1D"/> whose image to save.</param>
        /// <param name="file">The name of the file where the image will be saved.</param>
        /// <param name="imageFormat">The format the image will be saved as.</param>
        public static void SaveAsImage(this Texture1D texture, string file, SaveImageFormat imageFormat)
        {
            using FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
            SaveAsImage(texture, fileStream, imageFormat);
        }
    }
}
