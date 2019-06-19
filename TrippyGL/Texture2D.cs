using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace TrippyGL
{
    /// <summary>
    /// A 2D OpenGL texture
    /// </summary>
    public class Texture2D : Texture, IMultisamplableTexture
    {
        /// <summary>The width of this texture</summary>
        public int Width { get; private set; }

        /// <summary>The height of this texture</summary>
        public int Height { get; private set; }

        /// <summary>The amount of samples this texture has. Most common value is 0</summary>
        public int Samples { get; private set; }

        /// <summary>
        /// Creates a Texture2D with the desired parameters but no image data
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="width">The width of the texture</param>
        /// <param name="height">The height of the texture</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for this texture</param>
        /// <param name="samples">The amount of samples for this texture. Default is 0</param>
        /// <param name="imageFormat">The image format for this texture</param>
        public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool generateMipmaps = false, int samples = 0, TextureImageFormat imageFormat = TextureImageFormat.Color4b) : base(graphicsDevice, samples == 0 ? TextureTarget.Texture2D : TextureTarget.Texture2DMultisample, imageFormat)
        {
            ValidateSampleCount(samples);
            this.Samples = samples;

            RecreateImage(width, height); //This also binds the texture

            if (generateMipmaps)
                GenerateMipmaps();

            if (this.Samples == 0)
            {
                GL.TexParameter(this.TextureType, TextureParameterName.TextureMinFilter, IsMipmapped ? (int)DefaultMipmapMinFilter : (int)DefaultMinFilter);
                GL.TexParameter(this.TextureType, TextureParameterName.TextureMagFilter, (int)DefaultMagFilter);
            }
        }

        internal Texture2D(GraphicsDevice graphicsDevice, string file, bool generateMipmaps, TextureTarget textureTarget) : base(graphicsDevice, textureTarget, TextureImageFormat.Color4b)
        {
            this.Samples = 0;
            using (Bitmap bitmap = new Bitmap(file))
            {
                this.Width = bitmap.Width;
                this.Height = bitmap.Height;
                ValidateTextureSize(this.Width, this.Height);

                BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, this.Width, this.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                graphicsDevice.BindTextureSetActive(this);
                GL.TexImage2D(this.TextureType, 0, this.PixelFormat, this.Width, this.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, this.PixelType, data.Scan0);
                bitmap.UnlockBits(data);
            }

            if (generateMipmaps)
                GenerateMipmaps();

            if (this.Samples == 0)
            {
                GL.TexParameter(this.TextureType, TextureParameterName.TextureMinFilter, IsMipmapped ? (int)DefaultMipmapMinFilter : (int)DefaultMinFilter);
                GL.TexParameter(this.TextureType, TextureParameterName.TextureMagFilter, (int)DefaultMagFilter);
            }
        }

        /// <summary>
        /// Creates a Texture2D from an image from a file
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="file">The file containing the texture pixels data</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for this texture</param>
        public Texture2D(GraphicsDevice graphicsDevice, string file, bool generateMipmaps = false) : this(graphicsDevice, file, generateMipmaps, TextureTarget.Texture2D)
        {

        }

        /// <summary>
        /// Sets the data of a specified area of the texture, copying it from the specified pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read
        /// </summary>
        /// <param name="dataPtr">The pointer for reading the data</param>
        /// <param name="rectX">The X coordinate of the first pixel to write</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write</param>
        /// <param name="pixelDataFormat">The format of the pixel data in dataPtr. Accepted values are: Red, Rg, Rgb, Bgr, Rgba, Bgra, DepthComponent and StencilIndex</param>
        public void SetData(IntPtr dataPtr, int rectX, int rectY, int rectWidth, int rectHeight)
        {
            ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexSubImage2D(this.TextureType, 0, rectX, rectY, rectWidth, rectHeight, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, this.PixelType, dataPtr);
        }

        /// <summary>
        /// Sets the data of a specified area of the texture, copying the new data from a specified array
        /// </summary>
        /// <typeparam name="T">The type of struct to save the data as. This struct's format should match the texture pixel's format</typeparam>
        /// <param name="data">The array containing the new texture data</param>
        /// <param name="dataOffset">The index of the first element in the data array to start reading from</param>
        /// <param name="rectX">The X coordinate of the first pixel to write</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write</param>
        public void SetData<T>(T[] data, int dataOffset, int rectX, int rectY, int rectWidth, int rectHeight) where T : struct
        {
            ValidateSetOperation(data, dataOffset, rectX, rectY, rectWidth, rectHeight);

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexSubImage2D(this.TextureType, 0, rectX, rectY, rectWidth, rectHeight, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, this.PixelType, ref data[dataOffset]);
        }

        /// <summary>
        /// Sets the data of the entire texture, copying the new data from a given array
        /// </summary>
        /// <typeparam name="T">The type of struct to save the data as. This struct's format should match the texture pixel's format</typeparam>
        /// <param name="data">The array containing the new texture data</param>
        /// <param name="dataOffset">The index of the first element in the array to start reading from</param>
        public void SetData<T>(T[] data, int dataOffset = 0) where T : struct
        {
            SetData(data, dataOffset, 0, 0, this.Width, this.Height);
        }

        /// <summary>
        /// Gets the data of the entire texture and copies it to a specified pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read
        /// </summary>
        /// <param name="dataPtr">The pointer for writting the data</param>
        /// <param name="pixelDataFormat">The format of the pixel data in dataPtr. Accepted values are: Red, Rg, Rgb, Bgr, Rgba, Bgra, DepthComponent and StencilIndex</param>
        public void GetData(IntPtr dataPtr)
        {
            GraphicsDevice.BindTextureSetActive(this);
            GL.GetTexImage(this.TextureType, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, this.PixelType, dataPtr);
        }

        /// <summary>
        /// Gets the data of the entire texture, copying the texture data to a specified array
        /// </summary>
        /// <typeparam name="T">The type of struct to save the data as. This struct's format should match the texture pixel's format</typeparam>
        /// <param name="data">The array in which to write the texture data</param>
        /// <param name="dataOffset">The index of the first element in the data array to start writing from</param>
        public void GetData<T>(T[] data, int dataOffset = 0) where T : struct
        {
            ValidateGetOperation(data, dataOffset);
            
            GraphicsDevice.BindTextureSetActive(this);
            GL.GetTexImage(this.TextureType, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, this.PixelType, ref data[dataOffset]);
        }

        /// <summary>
        /// Saves this texture as an image file. You can't save multisampled textures
        /// </summary>
        /// <param name="file">The location in which to store the file</param>
        /// <param name="imageFormat">The format</param>
        public void SaveAsImage(string file, SaveImageFormat imageFormat)
        {
            if (Samples != 0)
                throw new NotSupportedException("You can't save multisampled textures");

            if (String.IsNullOrEmpty(file))
                throw new ArgumentException("You must specify a file name", "file");

            if (this.ImageFormat != TextureImageFormat.Color4b)
                throw new InvalidOperationException("In order to save a texture as image, it must be in Color4b format");

            ImageFormat format;

            switch (imageFormat)
            {
                case SaveImageFormat.Png:
                    format = System.Drawing.Imaging.ImageFormat.Png;
                    break;
                case SaveImageFormat.Jpeg:
                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;
                case SaveImageFormat.Bmp:
                    format = System.Drawing.Imaging.ImageFormat.Bmp;
                    break;
                case SaveImageFormat.Tiff:
                    format = System.Drawing.Imaging.ImageFormat.Tiff;
                    break;
                default:
                    throw new ArgumentException("You must use a proper value from SaveImageFormat", "imageFormat");
            }

            using (Bitmap b = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                BitmapData data = b.LockBits(new System.Drawing.Rectangle(0, 0, this.Width, this.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GraphicsDevice.BindTextureSetActive(this);
                GL.GetTexImage(this.TextureType, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                b.UnlockBits(data);
                b.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        /// <summary>
        /// Sets the texture coordinate wrapping modes for when a texture is sampled outside the [0, 1] range
        /// </summary>
        /// <param name="sWrapMode">The wrap mode for the S (or texture-X) coordinate</param>
        /// <param name="tWrapMode">The wrap mode for the T (or texture-Y) coordinate</param>
        public void SetWrapModes(TextureWrapMode sWrapMode, TextureWrapMode tWrapMode)
        {
            if (this.Samples != 0)
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
        public void RecreateImage(int width, int height)
        {
            ValidateTextureSize(width, height);

            this.Width = width;
            this.Height = height;

            GraphicsDevice.BindTextureSetActive(this);
            if (this.Samples == 0)
                GL.TexImage2D(this.TextureType, 0, this.PixelFormat, this.Width, this.Height, 0, TrippyUtils.IsImageFormatDepthType(this.ImageFormat) ? OpenTK.Graphics.OpenGL4.PixelFormat.DepthComponent : OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, this.PixelType, IntPtr.Zero);
            else
                GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, this.Samples, this.PixelFormat, this.Width, this.Height, true);

        }


        private protected void ValidateTextureSize(int width, int height)
        {
            if (width <= 0 || width > GraphicsDevice.MaxTextureSize)
                throw new ArgumentOutOfRangeException("width", width, "Texture width must be in the range (0, MAX_TEXTURE_SIZE]");

            if (height <= 0 || height > GraphicsDevice.MaxTextureSize)
                throw new ArgumentOutOfRangeException("height", height, "Texture height must be in the range (0, MAX_TEXTURE_SIZE]");
        }

        private protected void ValidateSetOperation<T>(T[] data, int dataOffset, int rectX, int rectY, int rectWidth, int rectHeight) where T : struct
        {
            if (this.Samples != 0)
                throw new InvalidOperationException("You can't write the data of a multisampled texture");

            //if (data == null) //it's gonna throw null reference anyway
            //    throw new ArgumentNullException("data", "Data array can't be null");

            if (dataOffset < 0 || dataOffset >= data.Length)
                throw new ArgumentOutOfRangeException("dataOffset", "dataOffset must be in the range [0, data.Length)");

            ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);

            if (data.Length - dataOffset < rectWidth * rectHeight)
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

            if (data.Length - dataOffset < this.Width * this.Height)
                throw new ArgumentException("The provided data array isn't big enough for the texture starting from dataOffset", "data");
        }

        internal void ValidateRectOperation(int rectX, int rectY, int rectWidth, int rectHeight)
        {
            if (rectX < 0 || rectY >= this.Height)
                throw new ArgumentOutOfRangeException("rectX", rectX, "rectX must be in the range [0, this.Width)");

            if (rectY < 0 || rectY >= this.Height)
                throw new ArgumentOutOfRangeException("rectY", rectY, "rectY must be in the range [0, this.Height)");

            if (rectWidth <= 0 || rectHeight <= 0)
                throw new ArgumentOutOfRangeException("rectWidth and rectHeight must be greater than 0");

            if (rectWidth > this.Width - rectX)
                throw new ArgumentOutOfRangeException("rectWidth", rectWidth, "rectWidth is too large");

            if (rectHeight > this.Height - rectY)
                throw new ArgumentOutOfRangeException("rectHeight", rectHeight, "rectHeight is too large");
        }

        internal void ValidateSampleCount(int samples)
        {
            if (samples < 0 || samples > GraphicsDevice.MaxSamples)
                throw new ArgumentOutOfRangeException("samples", samples, "The sample count must be in the range [0, MAX_SAMPLES]");
        }
    }
}
