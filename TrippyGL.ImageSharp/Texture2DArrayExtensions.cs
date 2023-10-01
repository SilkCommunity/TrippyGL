using System;
using System.IO;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TrippyGL.ImageSharp
{
    /// <summary>
    /// Provides extension methods for <see cref="Texture2DArray"/>
    /// </summary>
    public static class Texture2DArrayExtensions
    {
        /// <summary>
        /// Sets the data of one of the <see cref="Texture2DArray"/>'s images from an <see cref="Image{Rgba32}"/>.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2DArray"/> to set data for.</param>
        /// <param name="depthLevel">The array layer to set the data for.</param>
        /// <param name="image">The image with the pixel data to set.</param>
        public static void SetData(this Texture2DArray texture, int depthLevel, Image<Rgba32> image)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (texture.ImageFormat != TextureImageFormat.Color4b)
                throw new InvalidOperationException(nameof(TextureCubemap.ImageFormat) + " must be " + nameof(TextureImageFormat.Color4b) + " in order to do this");

            if (image.Width != texture.Width || image.Height != texture.Height)
                throw new InvalidOperationException("The size of the image must match the size of the " + nameof(Texture2DArray));

            if (image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> pixels))
            {
                texture.SetData<Rgba32>(pixels.Span, depthLevel, PixelFormat.Rgba);
            }
            else
            {
                image.ProcessPixelRows(accessor =>
                {
                    for (int yi = 0; yi < accessor.Height; yi++)
                        texture.SetData<Rgba32>(accessor.GetRowSpan(yi), 0, yi, depthLevel, (uint)accessor.Width, 1, 1, PixelFormat.Rgba);
                });
            }
        }

        /// <summary>
        /// Sets the data of one of the <see cref="Texture2DArray"/>'s images from a stream.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2DArray"/> to set data for.</param>
        /// <param name="depthLevel">The array layer to set the data for.</param>
        /// <param name="stream">The stream from which to load an image.</param>
        public static void SetData(this Texture2DArray texture, int depthLevel, Stream stream)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(stream);
            SetData(texture, depthLevel, image);
        }

        /// <summary>
        /// Sets the data of one of the <see cref="Texture2DArray"/>'s images from a file.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2DArray"/> to set data for.</param>
        /// <param name="depthLevel">The array layer to set the data for.</param>
        /// <param name="file">The file containing the image with the pixel data to set.</param>
        public static void SetData(this Texture2DArray texture, int depthLevel, string file)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(file);
            SetData(texture, depthLevel, image);
        }
    }
}
