using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace TrippyGL
{
    /// <summary>
    /// A <see cref="Texture"/> whose image has only one dimension.
    /// </summary>
    public sealed class Texture1D : Texture
    {
        /// <summary>The size of the <see cref="Texture1D"/>.</summary>
        public int Width { get; private set; }

        /// <summary>
        /// Creates a <see cref="Texture1D"/> with the desired parameters.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="width">The size of the <see cref="Texture1D"/>.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for this <see cref="Texture1D"/>.</param>
        /// <param name="imageFormat">The image format for this <see cref="Texture1D"/>.</param>
        public Texture1D(GraphicsDevice graphicsDevice, int width, bool generateMipmaps = false, TextureImageFormat imageFormat = TextureImageFormat.Color4b)
            : base(graphicsDevice, TextureTarget.Texture1D, imageFormat)
        {
            RecreateImage(width);

            if (generateMipmaps)
                GenerateMipmaps();

            GL.TexParameter(TextureType, TextureParameterName.TextureMinFilter, IsMipmapped ? (int)DefaultMipmapMinFilter : (int)DefaultMinFilter);
            GL.TexParameter(TextureType, TextureParameterName.TextureMagFilter, (int)DefaultMagFilter);
        }

        /// <summary>
        /// Creates a <see cref="Texture1D"/> from an image from a file.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="file">The file containing the texture pixels data.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for this <see cref="Texture1D"/>.</param>
        public Texture1D(GraphicsDevice graphicsDevice, string file, bool generateMipmaps = false)
            : base(graphicsDevice, TextureTarget.Texture1D, TextureImageFormat.Color4b)
        {
            using (Image<Rgba32> image = Image.Load<Rgba32>(file))
            {
                Width = image.Width * image.Height;
                ValidateTextureSize(Width);

                graphicsDevice.BindTextureSetActive(this);
                GL.TexImage1D(TextureType, 0, PixelInternalFormat, Width, 0, PixelFormat.Rgba, PixelType, ref image.GetPixelSpan()[0]);
            }

            if (generateMipmaps)
                GenerateMipmaps();

            GL.TexParameter(TextureType, TextureParameterName.TextureMinFilter, IsMipmapped ? (int)DefaultMipmapMinFilter : (int)DefaultMinFilter);
            GL.TexParameter(TextureType, TextureParameterName.TextureMagFilter, (int)DefaultMagFilter);
        }

        /// <summary>
        /// Sets the data of part of the <see cref="Texture1D"/> by copying it from the specified pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read.
        /// </summary>
        /// <param name="xOffset">The X coordinate of the first pixel to write.</param>
        /// <param name="width">The amount of pixels to write.</param>
        public void SetData(IntPtr data, int xOffset, int width)
        {
            ValidateRectOperation(xOffset, width);

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexSubImage1D(TextureType, 0, xOffset, width, PixelFormat.Rgba, PixelType, data);
        }

        /// <summary>
        /// Sets the data of a specified area of the <see cref="Texture1D"/>. The amount of pixels written
        /// is the length of the given <see cref="Span{T}"/>
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture1D"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="Span{T}"/> containing the texture data.</param>
        /// <param name="xOffset">The X coordinate of the first pixel to write.</param>
        public void SetData<T>(Span<T> data, int xOffset = 0) where T : struct
        {
            ValidateRectOperation(xOffset, data.Length);

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexSubImage1D(TextureType, 0, xOffset, data.Length, PixelFormat.Rgba, PixelType, ref data[0]);
        }

        /// <summary>
        /// Gets the data of the entire <see cref="Texture1D"/> and copies it to a specified pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read.
        /// </summary>
        /// <param name="data">The pointer for writting the data.</param>
        public void GetData(IntPtr data)
        {
            GraphicsDevice.BindTextureSetActive(this);
            GL.GetTexImage(TextureType, 0, PixelFormat.Rgba, PixelType, data);
        }

        /// <summary>
        /// Gets the data of the entire <see cref="Texture"/>, copying the texture data to a specified array.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture1D"/>'s pixels.</typeparam>
        /// <param name="data">The <see cref="Span{T}"/> in which to write the texture data.</param>
        public void GetData<T>(Span<T> data) where T : struct
        {
            if (data.Length < Width)
                throw new ArgumentException(nameof(data) + " must be large enough as to hold " + nameof(Width) + " pixels", nameof(data));

            GraphicsDevice.BindTextureSetActive(this);
            GL.GetTexImage(TextureType, 0, PixelFormat.Rgba, PixelType, ref data[0]);
        }

        /// <summary>
        /// Sets the texture coordinate wrapping modes for when a texture is sampled outside the [0, 1] range.
        /// </summary>
        /// <param name="sWrapMode">The wrap mode for the S (or texture-X) coordinate.</param>
        public void SetWrapMode(TextureWrapMode sWrapMode)
        {
            GraphicsDevice.BindTextureSetActive(this);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapS, (int)sWrapMode);
        }

        /// <summary>
        /// Recreates this <see cref="Texture1D"/>'s image with a new size,
        /// resizing the <see cref="Texture1D"/> but losing the image data.
        /// </summary>
        /// <param name="width">The new size for the <see cref="Texture1D"/>.</param>
        public void RecreateImage(int width)
        {
            ValidateTextureSize(width);

            Width = width;
            GraphicsDevice.BindTextureSetActive(this);
            GL.TexImage1D(TextureType, 0, PixelInternalFormat, width, 0, PixelFormat.Bgra, PixelType, IntPtr.Zero);
        }

        // TODO: SaveAsImage()

        private protected void ValidateTextureSize(int width)
        {
            if (width <= 0 || width > GraphicsDevice.MaxTextureSize)
                throw new ArgumentOutOfRangeException(nameof(width), width, nameof(width) + " must be in the range (0, " + nameof(GraphicsDevice.MaxTextureSize) + "]");
        }

        private protected void ValidateRectOperation(int xOffset, int width)
        {
            if (xOffset < 0 || xOffset >= Width)
                throw new ArgumentOutOfRangeException(nameof(xOffset), xOffset, nameof(xOffset) + " must be in the range [0, " + nameof(Width) + ")");

            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), width, nameof(width) + " must be greater than 0");

            if (xOffset + width > Width)
                throw new ArgumentOutOfRangeException(nameof(width), width, nameof(width) + " is too large");
        }
    }
}
