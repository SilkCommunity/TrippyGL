using System;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace TrippyGL.ImageSharp
{
    /// <summary>
    /// Extension methods for <see cref="TextureCubemap"/>.
    /// </summary>
    public static class TextureCubeMapExtensions
    {
        /// <summary>
        /// Sets the data of one of the <see cref="TextureCubemap"/>'s faces from an <see cref="Image{Rgba32}"/>.
        /// </summary>
        /// <param name="texture">The <see cref="TextureCubemap"/> to set data for.</param>
        /// <param name="face">The face of the cubemap to set data for.</param>
        /// <param name="image">The image with the pixel data to set.</param>
        public static void SetData(this TextureCubemap texture, CubemapFace face, Image<Rgba32> image)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (texture.ImageFormat != TextureImageFormat.Color4b)
                throw new InvalidOperationException(nameof(TextureCubemap.ImageFormat) + " must be " + nameof(TextureImageFormat.Color4b) + " in order to do this");

            if (image.Width != texture.Size || image.Height != texture.Size)
                throw new InvalidOperationException("The width and height of the image must match " + nameof(TextureCubemap.Size));

            texture.SetData<Rgba32>(face, image.GetPixelSpan(), PixelFormat.Rgba);
        }

        /// <summary>
        /// Sets the data of one of the <see cref="TextureCubemap"/>'s faces from a file.
        /// </summary>
        /// <param name="texture">The <see cref="TextureCubemap"/> to set data for.</param>
        /// <param name="face">The face of the cubemap to set data for.</param>
        /// <param name="file">The file containing the image with the pixel data to set.</param>
        public static void SetData(this TextureCubemap texture, CubemapFace face, string file)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(file);
            SetData(texture, face, image);
        }
    }
}
