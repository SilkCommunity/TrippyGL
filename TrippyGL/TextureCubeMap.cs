using System;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace TrippyGL
{
    public sealed class TextureCubemap : Texture
    {
        /// <summary>The size of a face from this cubemap.</summary>
        public uint Size { get; private set; }

        public unsafe TextureCubemap(GraphicsDevice graphicsDevice, uint size, TextureImageFormat imageFormat = TextureImageFormat.Color4b) : base(graphicsDevice, TextureTarget.TextureCubeMap, imageFormat)
        {
            ValidateTextureSize(size);
            Size = size;
            GraphicsDevice.BindTextureSetActive(this);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureType, TextureParameterName.TextureMinFilter, (int)DefaultMinFilter);
            GL.TexParameter(TextureType, TextureParameterName.TextureMagFilter, (int)DefaultMagFilter);

            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, (int)PixelInternalFormat, size, size, 0, PixelFormat, PixelType, (void*)0);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, (int)PixelInternalFormat, size, size, 0, PixelFormat, PixelType, (void*)0);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, (int)PixelInternalFormat, size, size, 0, PixelFormat, PixelType, (void*)0);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, (int)PixelInternalFormat, size, size, 0, PixelFormat, PixelType, (void*)0);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, (int)PixelInternalFormat, size, size, 0, PixelFormat, PixelType, (void*)0);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, (int)PixelInternalFormat, size, size, 0, PixelFormat, PixelType, (void*)0);
        }

        /// <summary>
        /// Sets the data of a specified area of the texture, copying it from the specified pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read.
        /// </summary>
        /// <param name="dataPtr">The pointer for reading the data.</param>
        /// <param name="rectX">The X coordinate of the first pixel to write.</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void SetData(CubeMapFace face, void* dataPtr, int rectX, int rectY, uint rectWidth, uint rectHeight, PixelFormat pixelFormat = 0)
        {
            ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexSubImage2D((TextureTarget)face, 0, rectX, rectY, rectWidth, rectHeight, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, dataPtr);
        }

        /// <summary>
        /// Sets the data of a specified area of the texture, copying the new data from a specified <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of struct to save the data as. This struct's format should match the texture pixel's format.</typeparam>
        /// <param name="data">The <see cref="ReadOnlySpan{T}"/> containing the new pixel data.</param>
        /// <param name="rectX">The X coordinate of the first pixel to write.</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void SetData<T>(CubeMapFace face, ReadOnlySpan<T> data, int rectX, int rectY, uint rectWidth, uint rectHeight, PixelFormat pixelFormat = 0) where T : unmanaged
        {
            ValidateSetOperation(data.Length, rectX, rectY, rectWidth, rectHeight);

            GraphicsDevice.BindTextureSetActive(this);
            fixed (void* ptr = &data[0])
                GL.TexSubImage2D((TextureTarget)face, 0, rectX, rectY, rectWidth, rectHeight, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ptr);
        }

        /// <summary>
        /// Sets the data of the entire texture, copying the new data from a specified <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of struct to save the data as. This struct's format should match the texture pixel's format.</typeparam>
        /// <param name="data">The <see cref="ReadOnlySpan{T}"/> containing the new pixel data.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public void SetData<T>(CubeMapFace face, ReadOnlySpan<T> data, PixelFormat pixelFormat = 0) where T : unmanaged
        {
            SetData(face, data, 0, 0, Size, Size, pixelFormat);
        }

        public void SetData(CubeMapFace face, string file)
        {
            if (ImageFormat != TextureImageFormat.Color4b)
                throw new InvalidOperationException("To set a cubemap's face from a file, the cubemap's format must be " + nameof(TextureImageFormat.Color4b));

            using Image<Rgba32> image = Image.Load<Rgba32>(file);
            if (image.Width != Size || image.Height != Size)
                throw new InvalidOperationException("The size of the image must match the size of the cubemap faces");

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexSubImage2D((TextureTarget)face, 0, 0, 0, Size, Size, PixelFormat.Rgba, PixelType, ref image.GetPixelSpan()[0]);
        }

        /// <summary>
        /// Gets the data of the entire texture and copies it to a specified pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read.
        /// </summary>
        /// <param name="dataPtr">The pointer for writting the data.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void GetData(CubeMapFace face, void* dataPtr, PixelFormat pixelFormat = 0)
        {
            GraphicsDevice.BindTextureSetActive(this);
            GL.GetTexImage((TextureTarget)face, 0, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, dataPtr);
        }

        /// <summary>
        /// Gets the data of the entire texture, copying the texture data to a specified array.
        /// </summary>
        /// <typeparam name="T">The type of struct to save the data as. This struct's format should match the texture pixel's format.</typeparam>
        /// <param name="data">The array in which to write the texture data.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public unsafe void GetData<T>(CubeMapFace face, Span<T> data, PixelFormat pixelFormat = 0) where T : unmanaged
        {
            ValidateGetOperation(data.Length);

            GraphicsDevice.BindTextureSetActive(this);

            fixed (void* ptr = &data[0])
                GL.GetTexImage((TextureTarget)face, 0, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ptr);
        }


        /// <summary>
        /// Sets the texture coordinate wrapping modes for when a texture is sampled outside the [0, 1] range.
        /// </summary>
        /// <param name="sWrapMode">The wrap mode for the S (or texture-X) coordinate.</param>
        /// <param name="tWrapMode">The wrap mode for the T (or texture-Y) coordinate.</param>
        /// <param name="rWrapMode">The wrap mode for the R (or texture-Z) coordinate.</param>
        public void SetWrapModes(TextureWrapMode sWrapMode, TextureWrapMode tWrapMode, TextureWrapMode rWrapMode)
        {
            GraphicsDevice.BindTextureSetActive(this);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapS, (int)sWrapMode);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapT, (int)tWrapMode);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapR, (int)rWrapMode);
        }


        private void ValidateTextureSize(uint size)
        {
            if (size <= 0 || size > GraphicsDevice.MaxCubeMapTextureSize)
                throw new ArgumentOutOfRangeException("size", size, "Cubemap size must be in the range (0, MAX_TEXTURE_CUBEMAP_SIZE]");
        }

        private void ValidateSetOperation(int dataLength, int rectX, int rectY, uint rectWidth, uint rectHeight)
        {
            ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);

            if (dataLength < rectWidth * rectHeight)
                throw new ArgumentException("The data array isn't big enough to read the specified amount of data", "data");
        }

        private void ValidateGetOperation(int dataLength)
        {
            if (dataLength < Size * Size)
                throw new ArgumentException("The provided data array isn't big enough for the texture starting from dataOffset", "data");
        }

        private void ValidateRectOperation(int rectX, int rectY, uint rectWidth, uint rectHeight)
        {
            if (rectX < 0 || rectY >= Size)
                throw new ArgumentOutOfRangeException(nameof(rectX), rectX, nameof(rectX) + " must be in the range [0, this.Size)");

            if (rectY < 0 || rectY >= Size)
                throw new ArgumentOutOfRangeException(nameof(rectY), rectY, nameof(rectY) + " must be in the range [0, this.Size)");

            if (rectWidth <= 0 || rectHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(rectWidth) + " and " + nameof(rectHeight) + " must be greater than 0");

            if (rectWidth > Size - rectX)
                throw new ArgumentOutOfRangeException(nameof(rectWidth), rectWidth, nameof(rectWidth) + " is too large");

            if (rectHeight > Size - rectY)
                throw new ArgumentOutOfRangeException(nameof(rectHeight), rectHeight, nameof(rectHeight) + " is too large");
        }
    }
}
