using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace TrippyGL
{
    /// <summary>
    /// A 1D OpenGL texture
    /// </summary>
    public class Texture1D : Texture
    {
        /// <summary>The width of the texture</summary>
        public int Width { get; private set; }

        /// <summary>
        /// Creates a Texture1D with the desired parameters
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="width">The width of the texture</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for this texture</param>
        /// <param name="imageFormat">The image format for this texture</param>
        public Texture1D(GraphicsDevice graphicsDevice, int width, bool generateMipmaps = false, TextureImageFormat imageFormat = TextureImageFormat.Color4b)
            : base(graphicsDevice, TextureTarget.Texture1D, imageFormat)
        {
            this.Width = width;
            ValidateTextureSize(width);

            RecreateImage(width);

            if (generateMipmaps)
            {
                IsMipmapped = true;
                GL.GenerateMipmap((GenerateMipmapTarget)this.TextureType);
                GL.TexParameter(this.TextureType, TextureParameterName.TextureMinFilter, (int)DefaultMipmapMinFilter);
            }
            else
                GL.TexParameter(this.TextureType, TextureParameterName.TextureMinFilter, (int)DefaultMinFilter);

            GL.TexParameter(this.TextureType, TextureParameterName.TextureMagFilter, (int)DefaultMagFilter);
        }

        /// <summary>
        /// Creates a Texture1D from an image from a file
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="file">The file containing the texture pixels data</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for this texture</param>
        public Texture1D(GraphicsDevice graphicsDevice, string file, bool generateMipmaps = false) : base(graphicsDevice, TextureTarget.Texture1D, TextureImageFormat.Color4b)
        {
            using (Bitmap bitmap = new Bitmap(file))
            {
                this.Width = bitmap.Width * bitmap.Height;
                ValidateTextureSize(Width);

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                graphicsDevice.BindTextureSetActive(this);
                GL.TexImage1D(this.TextureType, 0, this.PixelFormat, this.Width, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, this.PixelType, data.Scan0);
                bitmap.UnlockBits(data);
            }

            if (generateMipmaps)
            {
                IsMipmapped = true;
                GL.GenerateMipmap((GenerateMipmapTarget)this.TextureType);
                GL.TexParameter(this.TextureType, TextureParameterName.TextureMinFilter, (int)DefaultMipmapMinFilter);
            }
            else
                GL.TexParameter(this.TextureType, TextureParameterName.TextureMinFilter, (int)DefaultMinFilter);

            GL.TexParameter(this.TextureType, TextureParameterName.TextureMagFilter, (int)DefaultMagFilter);
        }

        /// <summary>
        /// Sets the data of part of the texture by copying it from the specified pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read
        /// </summary>
        /// <param name="dataPtr">The pointer for reading the data</param>
        /// <param name="x">The X coordinate of the first pixel to write</param>
        /// <param name="width">The width of the rectangle of pixels to write</param>
        /// <param name="pixelDataFormat">The format of the pixel data in dataPtr. Accepted values are: Red, Rg, Rgb, Bgr, Rgba, Bgra, DepthComponent and StencilIndex</param>
        public void SetData<T>(IntPtr data, int x, int width, OpenTK.Graphics.OpenGL4.PixelFormat pixelDataFormat)
        {
            ValidateRectOperation(x, width);

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexSubImage1D(this.TextureType, 0, x, width, pixelDataFormat, this.PixelType, data);
        }

        /// <summary>
        /// Sets the data of a specified area of the texture, copying the new data from a specified array
        /// </summary>
        /// <typeparam name="T">The type of struct to save the data as. This struct's format should match the texture pixel's format</typeparam>
        /// <param name="data">The array containing the new texture data</param>
        /// <param name="dataOffset">The index of the first element in the data array to start reading from</param>
        /// <param name="x">The X coordinate of the first pixel to write</param>
        /// <param name="width">The width of the area of pixels to write</param>
        public void SetData<T>(T[] data, int dataOffset, int x, int width) where T : struct
        {
            ValidateSetOperation(data, dataOffset, x, width);

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexSubImage1D(this.TextureType, 0, x, width, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, this.PixelType, ref data[dataOffset]);
        }

        /// <summary>
        /// Sets the data of the entire texture, copying the new data from a given array
        /// </summary>
        /// <typeparam name="T">The type of struct to save the data as. This struct's format should match the texture pixel's format</typeparam>
        /// <param name="data">The array containing the new texture data</param>
        /// <param name="dataOffset">The index of the first element in the array to start reading from</param>
        public void SetData<T>(T[] data, int dataOffset = 0) where T : struct
        {
            SetData(data, dataOffset, 0, this.Width);
        }
        
        /// <summary>
        /// Gets the data of the entire texture and copies it to a specified pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read
        /// </summary>
        /// <param name="dataPtr">The pointer for writting the data</param>
        /// <param name="pixelDataFormat">The format of the pixel data in dataPtr. Accepted values are: Red, Rg, Rgb, Bgr, Rgba, Bgra, DepthComponent and StencilIndex</param>
        public void GetData<T>(IntPtr data, OpenTK.Graphics.OpenGL4.PixelFormat pixelDataFormat)
        {
            GraphicsDevice.BindTextureSetActive(this);
            GL.GetTexImage(this.TextureType, 0, pixelDataFormat, this.PixelType, data);
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
            GL.GetTexImage(this.TextureType, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, this.PixelType, data);
        }

        /// <summary>
        /// Sets the texture coordinate wrapping modes for when a texture is sampled outside the [0, 1] range
        /// </summary>
        /// <param name="sWrapMode">The wrap mode for the S (or texture-X) coordinate</param>
        public void SetWrapMode(TextureWrapMode sWrapMode)
        {
            GraphicsDevice.BindTextureSetActive(this);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapS, (int)sWrapMode);
        }

        /// <summary>
        /// Recreates this texture's image with a new size, resizing the texture but losing the image data
        /// </summary>
        /// <param name="width">The new width for the texture</param>
        public void RecreateImage(int width)
        {
            ValidateTextureSize(width);

            this.Width = width;

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexImage1D(this.TextureType, 0, this.PixelFormat, width, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, this.PixelType, IntPtr.Zero);
        }

        private protected void ValidateTextureSize(int width)
        {
            if (width <= 0 || width > GraphicsDevice.MaxTextureSize)
                throw new ArgumentOutOfRangeException("width", width, "Texture width must be in the range (0, MAX_TEXTURE_SIZE]");
        }

        private protected void ValidateSetOperation<T>(T[] data, int dataOffset, int x, int width) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data", "Data array can't be null");

            if (dataOffset < 0 || dataOffset >= data.Length)
                throw new ArgumentOutOfRangeException("dataOffset", "dataOffset must be in the range [0, data.Length)");

            ValidateRectOperation(x, width);

            if (data.Length - dataOffset > width)
                throw new ArgumentException("Too much data was specified for the texture area to write", "data");
        }

        private protected void ValidateGetOperation<T>(T[] data, int dataOffset) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data", "Data array can't be null");

            if (dataOffset < 0 || dataOffset >= data.Length)
                throw new ArgumentOutOfRangeException("dataOffset", "dataOffset must be in the range [0, data.Length)");

            if (data.Length - dataOffset < this.Width)
                throw new ArgumentException("The provided data array isn't big enough for the specified texture area starting from dataOffset", "data");
        }

        private protected void ValidateRectOperation(int x, int width)
        {
            if (x < 0 || x >= this.Width)
                throw new ArgumentOutOfRangeException("x", x, "X must be in the range [0, this.Width)");

            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", width, "Width must be greater than 0");

            if (width > this.Width - x)
                throw new ArgumentOutOfRangeException("width", width, "Width is too large");
        }
    }
}
