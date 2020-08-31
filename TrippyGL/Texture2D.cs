using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// A <see cref="Texture"/> whose image has two dimensions and support for multisampling.
    /// </summary>
    public sealed class Texture2D : TextureMultisamplable
    {
        /// <summary>The width of this <see cref="Texture2D"/>.</summary>
        public uint Width { get; private set; }

        /// <summary>The height of this <see cref="Texture2D"/>.</summary>
        public uint Height { get; private set; }

        /// <summary>
        /// Creates a <see cref="Texture2D"/> with the desired parameters but no image data.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="width">The width of the <see cref="Texture2D"/>.</param>
        /// <param name="height">The height of the <see cref="Texture2D"/>.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for this <see cref="Texture2D"/>.</param>
        /// <param name="samples">The amount of samples for this <see cref="Texture2D"/>. Default is 0.</param>
        /// <param name="imageFormat">The image format for this <see cref="Texture2D"/>.</param>
        public Texture2D(GraphicsDevice graphicsDevice, uint width, uint height, bool generateMipmaps = false, uint samples = 0, TextureImageFormat imageFormat = TextureImageFormat.Color4b)
            : base(graphicsDevice, samples == 0 ? TextureTarget.Texture2D : TextureTarget.Texture2DMultisample, samples, imageFormat)
        {
            RecreateImage(width, height); //This also binds the texture

            if (generateMipmaps)
                GenerateMipmaps();

            if (Samples == 0)
            {
                GL.TexParameter(TextureType, TextureParameterName.TextureMinFilter, IsMipmapped ? (int)DefaultMipmapMinFilter : (int)DefaultMinFilter);
                GL.TexParameter(TextureType, TextureParameterName.TextureMagFilter, (int)DefaultMagFilter);
            }
        }

        /// <summary>
        /// Sets the data of an area of the <see cref="Texture2D"/>.
        /// </summary>
        /// <param name="ptr">The pointer from which the pixel data will be read.</param>
        /// <param name="rectX">The X coordinate of the first pixel to write.</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void SetDataPtr(void* ptr, int rectX, int rectY, uint rectWidth, uint rectHeight, PixelFormat pixelFormat = 0)
        {
            ValidateNotMultisampledPixelAccess();
            ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexSubImage2D(TextureType, 0, rectX, rectY, rectWidth, rectHeight, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ptr);
        }

        /// <summary>
        /// Sets the data of an area of the <see cref="Texture2D"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2D"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the new pixel data.</param>
        /// <param name="rectX">The X coordinate of the first pixel to write.</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void SetData<T>(ReadOnlySpan<T> data, int rectX, int rectY, uint rectWidth, uint rectHeight, PixelFormat pixelFormat = 0) where T : unmanaged
        {
            ValidateNotMultisampledPixelAccess();
            ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);
            if (data.Length < rectWidth * rectHeight)
                throw new ArgumentException("Not enough pixel data", nameof(data));

            GraphicsDevice.BindTextureSetActive(this);
            fixed (void* ptr = data)
                GL.TexSubImage2D(TextureType, 0, rectX, rectY, rectWidth, rectHeight, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ptr);
        }

        /// <summary>
        /// Sets the data of the entire <see cref="Texture2D"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2D"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the new pixel data.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public void SetData<T>(ReadOnlySpan<T> data, PixelFormat pixelFormat = 0) where T : unmanaged
        {
            SetData(data, 0, 0, Width, Height, pixelFormat);
        }

        /// <summary>
        /// Gets the data of the entire <see cref="Texture2D"/>.
        /// </summary>
        /// <param name="ptr">The pointer to which the pixel data will be written.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void GetDataPtr(void* ptr, PixelFormat pixelFormat = 0)
        {
            ValidateNotMultisampledPixelAccess();
            GraphicsDevice.BindTextureSetActive(this);
            GL.GetTexImage(TextureType, 0, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ptr);
        }

        /// <summary>
        /// Gets the data of the entire <see cref="Texture2D"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2D"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="Span{T}"/> in which to write the pixel data.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void GetData<T>(Span<T> data, PixelFormat pixelFormat = 0) where T : unmanaged
        {
            ValidateNotMultisampledPixelAccess();
            if (data.Length < Width * Height)
                throw new ArgumentException("Insufficient space to store the requested pixel data", nameof(data));

            GraphicsDevice.BindTextureSetActive(this);
            fixed (void* ptr = data)
                GL.GetTexImage(TextureType, 0, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ptr);
        }

        /// <summary>
        /// Sets the texture coordinate wrapping modes for when a texture is sampled outside the [0, 1] range.
        /// </summary>
        /// <param name="sWrapMode">The wrap mode for the S (or texture-X) coordinate.</param>
        /// <param name="tWrapMode">The wrap mode for the T (or texture-Y) coordinate.</param>
        public void SetWrapModes(TextureWrapMode sWrapMode, TextureWrapMode tWrapMode)
        {
            ValidateNotMultisampledWrapStates();
            GraphicsDevice.BindTextureSetActive(this);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapS, (int)sWrapMode);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapT, (int)tWrapMode);
        }

        /// <summary>
        /// Recreates this <see cref="Texture2D"/>'s image with a new size,
        /// resizing the <see cref="Texture2D"/> but losing the image data.
        /// </summary>
        /// <param name="width">The new width for the <see cref="Texture2D"/>.</param>
        /// <param name="height">The new height for the <see cref="Texture2D"/>.</param>
        public unsafe void RecreateImage(uint width, uint height)
        {
            ValidateTextureSize(width, height);

            Width = width;
            Height = height;

            GraphicsDevice.BindTextureSetActive(this);
            if (Samples == 0)
                GL.TexImage2D(TextureType, 0, (int)PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, (void*)0);
            else
                GL.TexImage2DMultisample(TextureTarget.Texture2DMultisample, Samples, PixelInternalFormat, Width, Height, true);
        }

        private void ValidateTextureSize(uint width, uint height)
        {
            if (width <= 0 || width > GraphicsDevice.MaxTextureSize)
                throw new ArgumentOutOfRangeException(nameof(width), width, nameof(height) + " must be in the range (0, " + nameof(GraphicsDevice.MaxTextureSize) + "]");

            if (height <= 0 || height > GraphicsDevice.MaxTextureSize)
                throw new ArgumentOutOfRangeException(nameof(height), height, nameof(height) + " must be in the range (0, " + nameof(GraphicsDevice.MaxTextureSize) + "]");
        }

        private void ValidateRectOperation(int rectX, int rectY, uint rectWidth, uint rectHeight)
        {
            if (rectX < 0 || rectY >= Height)
                throw new ArgumentOutOfRangeException(nameof(rectX), rectX, nameof(rectX) + " must be in the range [0, " + nameof(Width) + ")");

            if (rectY < 0 || rectY >= Height)
                throw new ArgumentOutOfRangeException(nameof(rectY), rectY, nameof(rectY) + " must be in the range [0, " + nameof(Height) + ")");

            if (rectWidth <= 0)
                throw new ArgumentOutOfRangeException(nameof(rectWidth), rectWidth, nameof(rectWidth) + " must be greater than 0");

            if (rectHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(rectHeight), rectHeight, nameof(rectHeight) + "must be greater than 0");

            if (rectWidth > Width - rectX || rectHeight > Height - rectY)
                throw new ArgumentOutOfRangeException("Specified area is outside of the texture's storage");
        }
    }
}
