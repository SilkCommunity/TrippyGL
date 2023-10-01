using System;
using System.IO;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TrippyGL.ImageSharp
{
    /// <summary>
    /// Provides extension methods for <see cref="Texture2D"/>.
    /// </summary>
    public static class Texture2DExtensions
    {
        /// <summary>
        /// Sets a the data of an area of a <see cref="Texture2D"/> from an <see cref="Image{Rgba32}"/>.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> whose image to set.</param>
        /// <param name="x">The x position of the first pixel to set.</param>
        /// <param name="y">The y position of the first pixel to set.</param>
        /// <param name="image">The image to set the data from. The width and height is taken from here.</param>
        public static void SetData(this Texture2D texture, int x, int y, Image<Rgba32> image)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (texture.ImageFormat != TextureImageFormat.Color4b)
                throw new ArgumentException(nameof(texture), ImageUtils.TextureFormatMustBeColor4bError);

            if (image == null)
                throw new ArgumentNullException(nameof(image));

            // ImageSharp might have the images stores contiguously in memory, or it might not.
            // If it does, then let's set the texture data all at once. If it doesn't, then we do it by rows.
            if (image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> pixels))
            {
                texture.SetData<Rgba32>(pixels.Span, x, y, (uint)image.Width, (uint)image.Height, PixelFormat.Rgba);
            }
            else
            {
                image.ProcessPixelRows(accessor =>
                {
                    for (int yi = 0; yi < accessor.Height; yi++)
                        texture.SetData<Rgba32>(accessor.GetRowSpan(yi), x, y + yi, (uint)accessor.Width, 1, PixelFormat.Rgba);
                });
            }
        }

        /// <summary>
        /// Sets a the data of an entire <see cref="Texture2D"/> from an <see cref="Image{Rgba32}"/>.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> whose image to set.</param>
        /// <param name="image">The image to set the data from.</param>
        public static void SetData(this Texture2D texture, Image<Rgba32> image)
        {
            SetData(texture, 0, 0, image);
        }

        /// <summary>
        /// Gets the data of the entire <see cref="Texture2D"/>.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> to get the image from.</param>
        /// <param name="image">The image in which to write the pixel data.</param>
        /// <param name="flip">Whether to flip the image after the pixels are read.</param>
        public static void GetData(this Texture2D texture, Image<Rgba32> image, bool flip = false)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (texture.ImageFormat != TextureImageFormat.Color4b)
                throw new ArgumentException(nameof(texture), ImageUtils.TextureFormatMustBeColor4bError);

            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (image.Width != texture.Width || image.Height != texture.Height)
                throw new ArgumentException(nameof(image), ImageUtils.ImageSizeMustMatchTextureSizeError);

            if (image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> pixels))
            {
                texture.GetData(pixels.Span, PixelFormat.Rgba);
            }
            else
            {
                // Unfortunately, OpenGL only has functions for getting the whole texture at once. Not by parts.
                Rgba32[] data = new Rgba32[texture.Width * texture.Height];
                texture.GetData<Rgba32>(data);

                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgba32> dataRowSpan = data.AsSpan((int)(texture.Width * y), (int)texture.Width);
                        Span<Rgba32> imageRowSpan = accessor.GetRowSpan(y);

                        dataRowSpan.CopyTo(imageRowSpan);
                    }
                });
            }

            if (flip)
                image.Mutate(x => x.Flip(FlipMode.Vertical));
        }

        /// <summary>
        /// Creates a <see cref="Texture2D"/> from an <see cref="Image{Rgba32}"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the resource will use.</param>
        /// <param name="image">The image to create the <see cref="Texture2D"/> with.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for the <see cref="Texture2D"/>.</param>
        public static Texture2D FromImage(GraphicsDevice graphicsDevice, Image<Rgba32> image, bool generateMipmaps = false)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            if (image == null)
                throw new ArgumentNullException(nameof(image));

            Texture2D texture = new Texture2D(graphicsDevice, (uint)image.Width, (uint)image.Height);

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
        /// Creates a <see cref="Texture2D"/> from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the resource will use.</param>
        /// <param name="stream">The stream from which to load an image.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for the <see cref="Texture2D"/>.</param>
        public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream, bool generateMipmaps = false)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(stream);
            return FromImage(graphicsDevice, image, generateMipmaps);
        }

        /// <summary>
        /// Creates a <see cref="Texture2D"/> by loading an image from a file.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the resource will use.</param>
        /// <param name="file">The file containing the image to create the <see cref="Texture2D"/> with.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for the <see cref="Texture2D"/>.</param>
        public static Texture2D FromFile(GraphicsDevice graphicsDevice, string file, bool generateMipmaps = false)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(file);
            return FromImage(graphicsDevice, image, generateMipmaps);
        }

        /// <summary>
        /// Saves this <see cref="Texture2D"/>'s image to a stream. You can't save multisampled textures.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> whose image to save.</param>
        /// <param name="stream">The stream to save the texture image to.</param>
        /// <param name="imageFormat">The format the image will be saved as.</param>
        /// <param name="flip">Whether to flip the image after the pixels are read.</param>
        public static void SaveAsImage(this Texture2D texture, Stream stream, SaveImageFormat imageFormat, bool flip = false)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (texture.Samples != 0)
                throw new NotSupportedException("You can't save multisampled textures");

            if (stream == null)
                throw new ArgumentException("You must specify a stream", nameof(stream));

            IImageFormat format = ImageUtils.GetFormatFor(imageFormat);
            using Image<Rgba32> image = new Image<Rgba32>((int)texture.Width, (int)texture.Height);
            texture.GetData(image, flip);
            image.Save(stream, format);
        }

        /// <summary>
        /// Saves this <see cref="Texture2D"/>'s image to a file. You can't save multisampled textures.
        /// If the file already exists, it will be replaced.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> whose image to save.</param>
        /// <param name="file">The name of the file where the image will be saved.</param>
        /// <param name="imageFormat">The format the image will be saved as.</param>
        /// <param name="flip">Whether to flip the image after the pixels are read.</param>
        public static void SaveAsImage(this Texture2D texture, string file, SaveImageFormat imageFormat, bool flip = false)
        {
            using FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
            SaveAsImage(texture, fileStream, imageFormat, flip);
        }
    }
}
