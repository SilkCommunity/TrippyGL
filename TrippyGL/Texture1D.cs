using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// A <see cref="Texture"/> whose image has only one dimension.
    /// </summary>
    public sealed class Texture1D : Texture
    {
        /// <summary>The size of the <see cref="Texture1D"/>.</summary>
        public uint Width { get; private set; }

        /// <summary>
        /// Creates a <see cref="Texture1D"/> with the desired parameters.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="width">The size of the <see cref="Texture1D"/>.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for this <see cref="Texture1D"/>.</param>
        /// <param name="imageFormat">The image format for this <see cref="Texture1D"/>.</param>
        public Texture1D(GraphicsDevice graphicsDevice, uint width, bool generateMipmaps = false, TextureImageFormat imageFormat = TextureImageFormat.Color4b)
            : base(graphicsDevice, TextureTarget.Texture1D, imageFormat)
        {
            RecreateImage(width);

            if (generateMipmaps)
                GenerateMipmaps();

            GL.TexParameter(TextureType, TextureParameterName.TextureMinFilter, IsMipmapped ? (int)DefaultMipmapMinFilter : (int)DefaultMinFilter);
            GL.TexParameter(TextureType, TextureParameterName.TextureMagFilter, (int)DefaultMagFilter);
        }

        /// <summary>
        /// Sets the data of an area of this <see cref="Texture1D"/>.
        /// </summary>
        /// <param name="ptr">The pointer from which the pixel data will be read.</param>
        /// <param name="xOffset">The X coordinate of the first pixel to write.</param>
        /// <param name="width">The amount of pixels to write.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void SetDataPtr(void* ptr, int xOffset, uint width, PixelFormat pixelFormat = 0)
        {
            ValidateRectOperation(xOffset, width);

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexSubImage1D(TextureType, 0, xOffset, width, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ptr);
        }

        /// <summary>
        /// Sets the data of an area of the <see cref="Texture1D"/>. The amount of pixels written
        /// is the length of the given <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture1D"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the pixel data.</param>
        /// <param name="xOffset">The X coordinate of the first pixel to write.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void SetData<T>(ReadOnlySpan<T> data, int xOffset = 0, PixelFormat pixelFormat = 0) where T : unmanaged
        {
            fixed (void* ptr = data)
                SetDataPtr(ptr, xOffset, (uint)data.Length, pixelFormat);
        }

        /// <summary>
        /// Gets the data of the entire <see cref="Texture1D"/>.
        /// </summary>
        /// <param name="ptr">The pointer to which the pixel data will be written.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void GetDataPtr(void* ptr, PixelFormat pixelFormat = 0)
        {
            GraphicsDevice.BindTextureSetActive(this);
            GL.GetTexImage(TextureType, 0, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ptr);
        }

        /// <summary>
        /// Gets the data of the entire <see cref="Texture1D"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture1D"/>'s pixels.</typeparam>
        /// <param name="data">The <see cref="Span{T}"/> in which to write the pixel data.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void GetData<T>(Span<T> data, PixelFormat pixelFormat = 0) where T : unmanaged
        {
            if (data.Length < Width)
                throw new ArgumentException("Insufficient space to store the requested pixel data", nameof(data));

            fixed (void* ptr = data)
                GetDataPtr(ptr, pixelFormat);
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
        public unsafe void RecreateImage(uint width)
        {
            ValidateTextureSize(width);

            Width = width;
            GraphicsDevice.BindTextureSetActive(this);
            GL.TexImage1D(TextureType, 0, (int)PixelInternalFormat, width, 0, PixelFormat.Rgba, PixelType, (void*)0);
        }

        private void ValidateTextureSize(uint width)
        {
            if (width <= 0 || width > GraphicsDevice.MaxTextureSize)
                throw new ArgumentOutOfRangeException(nameof(width), width, nameof(width) + " must be in the range (0, " + nameof(GraphicsDevice.MaxTextureSize) + "]");
        }

        private void ValidateRectOperation(int xOffset, uint width)
        {
            if (xOffset < 0 || xOffset >= Width)
                throw new ArgumentOutOfRangeException(nameof(xOffset), xOffset, nameof(xOffset) + " must be in the range [0, " + nameof(Width) + ")");

            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), width, nameof(width) + " must be greater than 0");

            if (xOffset + width > Width)
                throw new ArgumentOutOfRangeException("Specified area is outside of the texture's storage");
        }
    }
}
