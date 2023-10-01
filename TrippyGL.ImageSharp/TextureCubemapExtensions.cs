using System;
using System.IO;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TrippyGL.ImageSharp
{
    /// <summary>
    /// Provides extension methods for <see cref="TextureCubemap"/>.
    /// </summary>
    public static class TextureCubemapExtensions
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
                throw new InvalidOperationException("The width and height of the image must be " + nameof(TextureCubemap.Size));

            if (image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> pixels))
            {
                texture.SetData<Rgba32>(face, pixels.Span, PixelFormat.Rgba);
            }
            else
            {
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                        texture.SetData<Rgba32>(face, accessor.GetRowSpan(y), 0, y, (uint)accessor.Width, 1);
                });
            }
        }

        /// <summary>
        /// Sets the data of one of the <see cref="TextureCubemap"/>'s faces from a stream.
        /// </summary>
        /// <param name="texture">The <see cref="TextureCubemap"/> to set data for.</param>
        /// <param name="face">The face of the cubemap to set data for.</param>
        /// <param name="stream">The stream from which to load an image.</param>
        public static void SetData(this TextureCubemap texture, CubemapFace face, Stream stream)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(stream);
            SetData(texture, face, stream);
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

        /// <summary>
        /// Creates a <see cref="TextureCubemap"/> with the given files as texture image data.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the resource will use.</param>
        /// <param name="imgNegX">An image file with the cubemap's negative X face.</param>
        /// <param name="imgPosX">An image file with the cubemap's positive X face.</param>
        /// <param name="imgNegY">An image file with the cubemap's negative Y face.</param>
        /// <param name="imgPosY">An image file with the cubemap's positive Y face.</param>
        /// <param name="imgNegZ">An image file with the cubemap's negative Z face.</param>
        /// <param name="imgPosZ">An image file with the cubemap's positive Z face.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for the <see cref="TextureCubemap"/>.</param>
        public static TextureCubemap FromFiles(GraphicsDevice graphicsDevice, string imgNegX, string imgPosX, string imgNegY, string imgPosY, string imgNegZ, string imgPosZ, bool generateMipmaps = false)
        {
            using Image<Rgba32> negx = Image.Load<Rgba32>(imgNegX);

            if (negx.Width != negx.Height)
                throw new InvalidOperationException("The width and height of all the images must be the same for a cubemap.");

            TextureCubemap cubemap = new TextureCubemap(graphicsDevice, (uint)negx.Width);

            try
            {
                cubemap.SetData(CubemapFace.NegativeX, negx);
                cubemap.SetData(CubemapFace.PositiveX, imgPosX);
                cubemap.SetData(CubemapFace.NegativeY, imgNegY);
                cubemap.SetData(CubemapFace.PositiveY, imgPosY);
                cubemap.SetData(CubemapFace.NegativeZ, imgNegZ);
                cubemap.SetData(CubemapFace.PositiveZ, imgPosZ);

                if (generateMipmaps)
                    cubemap.GenerateMipmaps();

                return cubemap;
            }
            catch
            {
                cubemap.Dispose();
                throw;
            }
        }
    }
}
