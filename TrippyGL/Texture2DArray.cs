using Silk.NET.OpenGL;
using System;

namespace TrippyGL
{
    /// <summary>
    /// A <see cref="Texture"/> containing an array of two-dimensional images and support for multisampling
    /// </summary>
    public sealed class Texture2DArray : Texture, IMultisamplableTexture
    {
        /// <summary>The width of this <see cref="Texture2DArray"/>.</summary>
        public uint Width { get; private set; }

        /// <summary>The height of this <see cref="Texture2DArray"/>.</summary>
        public uint Height { get; private set; }

        /// <summary>The amount of images or array length of this <see cref="Texture2DArray"/>.</summary>
        public uint Depth { get; private set; }

        /// <summary>The amount of samples this <see cref="Texture2DArray"/> has.</summary>
        public uint Samples { get; private set; }

        public Texture2DArray(GraphicsDevice graphicsDevice, uint width, uint height, uint depth, uint samples = 0, TextureImageFormat imageFormat = TextureImageFormat.Color4b)
            : base(graphicsDevice, samples == 0 ? TextureTarget.Texture2DArray : TextureTarget.Texture2DMultisampleArray, imageFormat)
        {
            Samples = samples;
            RecreateImage(width, height, depth); //this also binds the texture

            if (samples == 0)
            {
                GL.TexParameter(TextureType, TextureParameterName.TextureMinFilter, (int)DefaultMinFilter);
                GL.TexParameter(TextureType, TextureParameterName.TextureMagFilter, (int)DefaultMagFilter);
            }
        }

        /// <summary>
        /// Sets the data of a specified area of the <see cref="Texture2DArray"/>, copying the new data from a given pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read.
        /// </summary>
        /// <param name="dataPtr">The pointer for reading the pixel data.</param>
        /// <param name="rectX">The X coordinate of the first pixel to write.</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
        /// <param name="rectZ">The Z coordinate of the first pixel to write.</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
        /// <param name="rectDepth">The depth of the rectangle of pixels to write.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this <see cref="Texture2DArray"/>'s default.</param>
        public unsafe void SetData(void* dataPtr, int rectX, int rectY, int rectZ, uint rectWidth, uint rectHeight, uint rectDepth, PixelFormat pixelFormat = 0)
        {
            ValidateSetOperation(rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth);

            GraphicsDevice.BindTexture(this);
            GL.TexSubImage3D(TextureType, 0, rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, dataPtr);
        }

        /// <summary>
        /// Sets the data of a specified area of the <see cref="Texture2DArray"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2DArray"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the new pixel data.</param>
        /// <param name="rectX">The X coordinate of the first pixel to write.</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
        /// <param name="rectZ">The Z coordinate of the first pixel to write.</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
        /// <param name="rectDepth">The depth of the rectangle of pixels to write.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this <see cref="Texture2DArray"/>'s default.</param>
        public unsafe void SetData<T>(ReadOnlySpan<T> data, int rectX, int rectY, int rectZ, uint rectWidth, uint rectHeight, uint rectDepth, PixelFormat pixelFormat = 0) where T : unmanaged
        {
            ValidateSetOperation(data.Length, rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth);

            GraphicsDevice.BindTexture(this);
            fixed (void* ptr = &data[0])
                GL.TexSubImage3D(TextureType, 0, rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ptr);
        }

        /// <summary>
        /// Sets the data of an entire array layer of the <see cref="Texture2DArray"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2DArray"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the new pixel data.</param>
        /// <param name="depthLevel">The array layer to set the data for.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this <see cref="Texture2DArray"/>'s default.</param>
        public void SetData<T>(ReadOnlySpan<T> data, int depthLevel, PixelFormat pixelFormat = 0) where T : unmanaged
        {
            SetData(data, 0, 0, depthLevel, Width, Height, 1, pixelFormat);
        }

        /// <summary>
        /// Sets the coordinate wrapping modes for when the <see cref="Texture2DArray"/> is sampled outside the [0, 1] range.
        /// </summary>
        /// <param name="sWrapMode">The wrap mode for the S (or texture-X) coordinate.</param>
        /// <param name="tWrapMode">The wrap mode for the T (or texture-Y) coordinate.</param>
        public void SetWrapModes(TextureWrapMode sWrapMode, TextureWrapMode tWrapMode)
        {
            if (Samples != 0)
                throw new InvalidOperationException("You can't change a multisampled texture's sampler states");

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapS, (int)sWrapMode);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapT, (int)tWrapMode);
        }

        /// <summary>
        /// Recreates this <see cref="Texture2DArray"/>'s images with a new size,
        /// resizing the <see cref="Texture2DArray"/> but losing the image data.
        /// </summary>
        /// <param name="width">The new width for the <see cref="Texture2DArray"/>.</param>
        /// <param name="height">The new height for the <see cref="Texture2DArray"/>.</param>
        /// <param name="depth">The new depth for the <see cref="Texture2DArray"/>.</param>
        public unsafe void RecreateImage(uint width, uint height, uint depth)
        {
            ValidateTextureSize(width, height, depth);

            Width = width;
            Height = height;
            Depth = depth;

            GraphicsDevice.BindTextureSetActive(this);
            if (Samples == 0)
                GL.TexImage3D(TextureType, 0, (int)PixelInternalFormat, width, height, depth, 0, PixelFormat, PixelType, (void*)0);
            else
                GL.TexImage3DMultisample(TextureType, Samples, PixelInternalFormat, width, height, depth, true);
        }

        private void ValidateTextureSize(uint width, uint height, uint depth)
        {
            if (width <= 0 || width > GraphicsDevice.MaxTextureSize)
                throw new ArgumentOutOfRangeException(nameof(width), width, nameof(width) + " must be in the range (0, " + nameof(GraphicsDevice.MaxTextureSize) + "]");

            if (height <= 0 || height > GraphicsDevice.MaxTextureSize)
                throw new ArgumentOutOfRangeException(nameof(height), height, nameof(height) + " must be in the range (0, " + nameof(GraphicsDevice.MaxTextureSize) + "]");

            if (depth <= 0 || depth > GraphicsDevice.MaxArrayTextureLayers)
                throw new ArgumentOutOfRangeException(nameof(depth), depth, nameof(depth) + " must be in the range (0, " + nameof(GraphicsDevice.MaxArrayTextureLayers) + ")");
        }

        private void ValidateRectOperation(int rectX, int rectY, int rectZ, uint rectWidth, uint rectHeight, uint rectDepth)
        {
            if (rectX < 0 || rectY >= Height)
                throw new ArgumentOutOfRangeException(nameof(rectX), rectX, nameof(rectX) + " must be in the range [0, " + nameof(Width) + ")");

            if (rectY < 0 || rectY >= Height)
                throw new ArgumentOutOfRangeException(nameof(rectY), rectY, nameof(rectY) + " must be in the range [0, " + nameof(Height) + ")");

            if (rectZ < 0 || rectZ >= Depth)
                throw new ArgumentOutOfRangeException(nameof(rectZ), rectZ, nameof(rectZ) + " must be in the range [0, " + nameof(Depth) + ")");

            if (rectWidth <= 0 || rectHeight <= 0 || rectDepth <= 0)
                throw new ArgumentOutOfRangeException(null, "rectWidth, rectHeight and rectDepth must be greater than 0");

            if (rectWidth > Width - rectX)
                throw new ArgumentOutOfRangeException(nameof(rectWidth), rectWidth, nameof(rectWidth) + " is too large");

            if (rectHeight > Height - rectY)
                throw new ArgumentOutOfRangeException(nameof(rectHeight), rectHeight, nameof(rectHeight) + " is too large");

            if (rectDepth > Depth - rectZ)
                throw new ArgumentOutOfRangeException(nameof(rectDepth), rectDepth, nameof(rectDepth) + " is too large");
        }

        private void ValidateSetOperation(int dataLength, int rectX, int rectY, int rectZ, uint rectWidth, uint rectHeight, uint rectDepth)
        {
            ValidateSetOperation(rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth);

            if (dataLength < rectWidth * rectHeight * rectDepth)
                throw new ArgumentException("The data array isn't big enough to read the specified amount of data", "data");
        }

        private void ValidateSetOperation(int rectX, int rectY, int rectZ, uint rectWidth, uint rectHeight, uint rectDepth)
        {
            if (Samples != 0)
                throw new InvalidOperationException("You can't write the pixels of a multisampled texture");

            ValidateRectOperation(rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth);
        }

        private void ValidateGetOperation(int dataLength)
        {
            ValidateGetOperation();

            if (dataLength < Width * Height * Depth)
                throw new ArgumentException("The data Span isn't large enough to fit the requested texture area", "data");
        }

        private void ValidateGetOperation()
        {
            if (Samples != 0)
                throw new InvalidOperationException("You can't read the pixels of a multisampled texture");
        }
    }
}
