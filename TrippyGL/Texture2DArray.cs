using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    /// <summary>
    /// An OpenGL array of 2D textures
    /// </summary>
    public class Texture2DArray : Texture, IMultisamplableTexture
    {
        /// <summary>The width of this texture</summary>
        public int Width { get; private set; }

        /// <summary>The height of this texture</summary>
        public int Height { get; private set; }

        /// <summary>The number of array layers of this texture</summary>
        public int Depth { get; private set; }

        /// <summary>The amount of samples this texture has. Most common value is 0</summary>
        public int Samples { get; private set; }

        public Texture2DArray(GraphicsDevice graphicsDevice, int width, int height, int depth, int samples = 0, TextureImageFormat imageFormat = TextureImageFormat.Color4b)
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
        /// Sets the data of a specified area of the texture, copying the new data from a given pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read
        /// </summary>
        /// <param name="dataPtr">The pointer for reading the data</param>
        /// <param name="rectX">The X coordinate of the first pixel to write</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write</param>
        /// <param name="rectZ">The Z coordinate of the first pixel to write</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write</param>
        /// <param name="rectDepth">The depth of the rectangle of pixels to write</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default</param>
        public void SetData(IntPtr dataPtr, int rectX, int rectY, int rectZ, int rectWidth, int rectHeight, int rectDepth, OpenTK.Graphics.OpenGL4.PixelFormat pixelFormat = 0)
        {
            ValidateRectOperation(rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth);

            GraphicsDevice.BindTexture(this);
            GL.TexSubImage3D(TextureType, 0, rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, dataPtr);
        }

        /// <summary>
        /// Sets the data of a specified area of the texture, copying the new data from a specified array
        /// </summary>
        /// <typeparam name="T">The type of struct to save the data as. This struct's format should match the texture pixel's format</typeparam>
        /// <param name="data">The array containing the new texture data</param>
        /// <param name="dataOffset">The index of the first element in the data array to start reading from</param>
        /// <param name="rectX">The X coordinate of the first pixel to write</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write</param>
        /// <param name="rectZ">The Z coordinate of the first pixel to write</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write</param>
        /// <param name="rectDepth">The depth of the rectangle of pixels to write</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default</param>
        public void SetData<T>(T[] data, int dataOffset, int rectX, int rectY, int rectZ, int rectWidth, int rectHeight, int rectDepth, OpenTK.Graphics.OpenGL4.PixelFormat pixelFormat = 0) where T : struct
        {
            ValidateSetOperation(data, dataOffset, rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth);

            GraphicsDevice.BindTexture(this);
            GL.TexSubImage3D(TextureType, 0, rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ref data[dataOffset]);
        }

        /// <summary>
        /// Sets the data of an entire array layer of the texture
        /// </summary>
        /// <typeparam name="T">The type of struct to save the data as. This struct's format should match the texture pixel's format</typeparam>
        /// <param name="data">The array containing the new texture data</param>
        /// <param name="dataOffset">The index of the first element in the data array to start reading from</param>
        /// <param name="depthLevel">The array layer to set the data for</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default</param>
        public void SetData<T>(T[] data, int dataOffset, int depthLevel, OpenTK.Graphics.OpenGL4.PixelFormat pixelFormat = 0) where T : struct
        {
            SetData(data, dataOffset, 0, 0, depthLevel, Width, Height, 1, pixelFormat);
        }

        /// <summary>
        /// Sets the texture coordinate wrapping modes for when a texture is sampled outside the [0, 1] range
        /// </summary>
        /// <param name="sWrapMode">The wrap mode for the S (or texture-X) coordinate</param>
        /// <param name="tWrapMode">The wrap mode for the T (or texture-Y) coordinate</param>
        public void SetWrapModes(TextureWrapMode sWrapMode, TextureWrapMode tWrapMode)
        {
            if (Samples != 0)
                throw new InvalidOperationException("You can't change a multisampled texture's sampler states");

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapS, (int)sWrapMode);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapT, (int)tWrapMode);
        }

        /// <summary>
        /// Recreates this texture's image with a new size, resizing the texture but losing the image data
        /// </summary>
        /// <param name="width">The new width for the texture</param>
        /// <param name="height">The new height for the texture</param>
        /// <param name="depth">The new depth for the texture</param>
        public void RecreateImage(int width, int height, int depth)
        {
            ValidateTextureSize(width, height, depth);

            Width = width;
            Height = height;
            Depth = depth;

            GraphicsDevice.BindTextureSetActive(this);
            if (Samples == 0)
                GL.TexImage3D(TextureType, 0, PixelInternalFormat, width, height, depth, 0, PixelFormat, PixelType, IntPtr.Zero);
            else
                GL.TexImage3DMultisample((TextureTargetMultisample)TextureType, Samples, PixelInternalFormat, width, height, depth, true);
        }


        private protected void ValidateTextureSize(int width, int height, int depth)
        {
            if (width <= 0 || width > GraphicsDevice.MaxTextureSize)
                throw new ArgumentOutOfRangeException("width", width, "Texture width must be in the range (0, MAX_TEXTURE_SIZE]");

            if (height <= 0 || height > GraphicsDevice.MaxTextureSize)
                throw new ArgumentOutOfRangeException("height", height, "Texture height must be in the range (0, MAX_TEXTURE_SIZE]");

            if (depth <= 0 || depth > GraphicsDevice.MaxArrayTextureLayers)
                throw new ArgumentOutOfRangeException("depth", depth, "Texture depth must be in the range (0, MAX_ARRAY_TEXTURE_LAYERS)");
        }

        private protected void ValidateRectOperation(int rectX, int rectY, int rectZ, int rectWidth, int rectHeight, int rectDepth)
        {
            if (rectX < 0 || rectY >= Height)
                throw new ArgumentOutOfRangeException("rectX", rectX, "rectX must be in the range [0, this.Width)");

            if (rectY < 0 || rectY >= Height)
                throw new ArgumentOutOfRangeException("rectY", rectY, "rectY must be in the range [0, this.Height)");

            if (rectZ < 0 || rectZ >= Depth)
                throw new ArgumentOutOfRangeException("rectZ", rectZ, "rectZ must be in the range [0, this.Depth)");

            if (rectWidth <= 0 || rectHeight <= 0 || rectDepth <= 0)
                throw new ArgumentOutOfRangeException("rectWidth, rectHeight and rectDepth must be greater than 0");

            if (rectWidth > Width - rectX)
                throw new ArgumentOutOfRangeException("rectWidth", rectWidth, "rectWidth is too large");

            if (rectHeight > Height - rectY)
                throw new ArgumentOutOfRangeException("rectHeight", rectHeight, "rectHeight is too large");

            if (rectDepth > Depth - rectZ)
                throw new ArgumentOutOfRangeException("rectDepth", rectDepth, "rectDepth is too large");
        }

        private protected void ValidateSetOperation<T>(T[] data, int dataOffset, int rectX, int rectY, int rectZ, int rectWidth, int rectHeight, int rectDepth) where T : struct
        {
            if (Samples != 0)
                throw new InvalidOperationException("You can't write the data of a multisampled texture");

            //if (data == null) //it's gonna throw null reference anyway
            //    throw new ArgumentNullException("data", "Data array can't be null");

            if (dataOffset < 0 || dataOffset >= data.Length)
                throw new ArgumentOutOfRangeException("dataOffset", "dataOffset must be in the range [0, data.Length)");

            ValidateRectOperation(rectX, rectY, rectZ, rectWidth, rectHeight, rectDepth);

            if (data.Length - dataOffset < rectWidth * rectHeight * rectDepth)
                throw new ArgumentException("The data array isn't big enough to read the specified amount of data", "data");
        }

        private protected void ValidateGetOperation<T>(T[] data, int dataOffset) where T : struct
        {
            if (Samples != 0)
                throw new InvalidOperationException("You can't read the data of a multisampled texture");

            //if (data == null) //it's gonna throw null reference anyway
            //    throw new ArgumentNullException("data", "Data array can't be null");

            if (dataOffset < 0 || dataOffset >= data.Length)
                throw new ArgumentOutOfRangeException("dataOffset", "dataOffset must be in the range [0, data.Length)");

            if (data.Length - dataOffset < Width * Height * Depth)
                throw new ArgumentException("The provided data array isn't big enough for the texture starting from dataOffset", "data");
        }
    }
}
