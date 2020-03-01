using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace TrippyGL
{
    /// <summary>
    /// A <see cref="Texture"/> whose image has two dimensions and support for multisampling.
    /// </summary>
    public class Texture2D : Texture, IMultisamplableTexture
    {
        /// <summary>The width of this <see cref="Texture2D"/>.</summary>
        public int Width { get; private set; }

        /// <summary>The height of this <see cref="Texture2D"/>.</summary>
        public int Height { get; private set; }

        /// <summary>The amount of samples this <see cref="Texture2D"/> has.</summary>
        public int Samples { get; private set; }

        /// <summary>
        /// Creates a <see cref="Texture2D"/> with the desired parameters but no image data.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="width">The width of the <see cref="Texture2D"/>.</param>
        /// <param name="height">The height of the <see cref="Texture2D"/>.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for this <see cref="Texture2D"/>.</param>
        /// <param name="samples">The amount of samples for this <see cref="Texture2D"/>. Default is 0.</param>
        /// <param name="imageFormat">The image format for this <see cref="Texture2D"/>.</param>
        public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool generateMipmaps = false, int samples = 0, TextureImageFormat imageFormat = TextureImageFormat.Color4b)
            : base(graphicsDevice, samples == 0 ? TextureTarget.Texture2D : TextureTarget.Texture2DMultisample, imageFormat)
        {
            ValidateSampleCount(samples);
            Samples = samples;

            RecreateImage(width, height); //This also binds the texture

            if (generateMipmaps)
                GenerateMipmaps();

            if (Samples == 0)
            {
                GL.TexParameter(TextureType, TextureParameterName.TextureMinFilter, IsMipmapped ? (int)DefaultMipmapMinFilter : (int)DefaultMinFilter);
                GL.TexParameter(TextureType, TextureParameterName.TextureMagFilter, (int)DefaultMagFilter);
            }
        }

        internal Texture2D(GraphicsDevice graphicsDevice, string file, bool generateMipmaps, TextureTarget textureTarget)
            : base(graphicsDevice, textureTarget, TextureImageFormat.Color4b)
        {
            Samples = 0;
            using (Bitmap bitmap = new Bitmap(file))
            {
                Width = bitmap.Width;
                Height = bitmap.Height;
                ValidateTextureSize(Width, Height);

                BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                graphicsDevice.BindTextureSetActive(this);
                GL.TexImage2D(TextureType, 0, PixelInternalFormat, Width, Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType, data.Scan0);
                bitmap.UnlockBits(data);
            }

            if (generateMipmaps)
                GenerateMipmaps();

            if (Samples == 0)
            {
                GL.TexParameter(TextureType, TextureParameterName.TextureMinFilter, IsMipmapped ? (int)DefaultMipmapMinFilter : (int)DefaultMinFilter);
                GL.TexParameter(TextureType, TextureParameterName.TextureMagFilter, (int)DefaultMagFilter);
            }
        }

        /// <summary>
        /// Creates a <see cref="Texture2D"/> from an image from a file.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="file">The file containing the texture pixels data.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for this <see cref="Texture2D"/>.</param>
        public Texture2D(GraphicsDevice graphicsDevice, string file, bool generateMipmaps = false)
            : this(graphicsDevice, file, generateMipmaps, TextureTarget.Texture2D)
        {

        }

        /// <summary>
        /// Sets the data of a specified area of the <see cref="Texture2D"/>, copying it from the specified pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read.
        /// </summary>
        /// <param name="dataPtr">The pointer for reading the pixel data.</param>
        /// <param name="rectX">The X coordinate of the first pixel to write.</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this texture's default.</param>
        public void SetData(IntPtr dataPtr, int rectX, int rectY, int rectWidth, int rectHeight, OpenTK.Graphics.OpenGL4.PixelFormat pixelFormat = 0)
        {
            ValidateSetOperation(rectX, rectY, rectWidth, rectHeight);

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexSubImage2D(TextureType, 0, rectX, rectY, rectWidth, rectHeight, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, dataPtr);
        }

        /// <summary>
        /// Sets the data of a specified area of the <see cref="Texture2D"/>, copying the new data from a specified <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2D"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="Span{T}"/> containing the new pixel data.</param>
        /// <param name="rectX">The X coordinate of the first pixel to write.</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this <see cref="Texture2D"/>'s default.</param>
        public void SetData<T>(Span<T> data, int rectX, int rectY, int rectWidth, int rectHeight, OpenTK.Graphics.OpenGL4.PixelFormat pixelFormat = 0) where T : struct
        {
            ValidateSetOperation(data.Length, rectX, rectY, rectWidth, rectHeight);

            GraphicsDevice.BindTextureSetActive(this);
            GL.TexSubImage2D(TextureType, 0, rectX, rectY, rectWidth, rectHeight, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ref data[0]);
        }

        /// <summary>
        /// Sets the data of the entire <see cref="Texture2D"/>, copying the new data from a given <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2D"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="Span{T}"/> containing the new pixel data.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this <see cref="Texture2D"/>'s default.</param>
        public void SetData<T>(Span<T> data, OpenTK.Graphics.OpenGL4.PixelFormat pixelFormat = 0) where T : struct
        {
            SetData(data, 0, 0, Width, Height, pixelFormat);
        }

        /// <summary>
        /// Gets the data of the entire <see cref="Texture2D"/> and copies it to a specified pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read.
        /// </summary>
        /// <param name="dataPtr">The pointer for writting the pixel data.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this <see cref="Texture2D"/>'s default.</param>
        public void GetData(IntPtr dataPtr, OpenTK.Graphics.OpenGL4.PixelFormat pixelFormat = 0)
        {
            ValidateGetOperation();
            GraphicsDevice.BindTextureSetActive(this);
            GL.GetTexImage(TextureType, 0, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, dataPtr);
        }

        /// <summary>
        /// Gets the data of the entire <see cref="Texture2D"/>, copying the texture data to a specified <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2D"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="Span{T}"/> in which to write the pixel data.</param>
        /// <param name="dataOffset">The index of the first element in the data array to start writing from.</param>
        /// <param name="pixelFormat">The pixel format the data will be read as. 0 for this <see cref="Texture2D"/>'s default.</param>
        public void GetData<T>(Span<T> data, OpenTK.Graphics.OpenGL4.PixelFormat pixelFormat = 0) where T : struct
        {
            ValidateGetOperation(data.Length);

            GraphicsDevice.BindTextureSetActive(this);
            GL.GetTexImage(TextureType, 0, pixelFormat == 0 ? PixelFormat : pixelFormat, PixelType, ref data[0]);
        }

        /// <summary>
        /// Saves this <see cref="Texture2D"/> as an image file. You can't save multisampled textures.
        /// </summary>
        /// <param name="file">The location in which to store the file.</param>
        /// <param name="imageFormat">The format.</param>
        public void SaveAsImage(string file, SaveImageFormat imageFormat)
        {
            if (Samples != 0)
                throw new NotSupportedException("You can't save multisampled textures");

            if (string.IsNullOrEmpty(file))
                throw new ArgumentException("You must specify a file name", nameof(file));

            if (ImageFormat != TextureImageFormat.Color4b)
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
                    throw new ArgumentException("You must specify a proper value from " + nameof(SaveImageFormat), nameof(imageFormat));
            }

            using (Bitmap b = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                BitmapData data = b.LockBits(new System.Drawing.Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GraphicsDevice.BindTextureSetActive(this);
                GL.GetTexImage(TextureType, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                b.UnlockBits(data);
                b.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        /// <summary>
        /// Sets the texture coordinate wrapping modes for when a texture is sampled outside the [0, 1] range.
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
        /// Recreates this <see cref="Texture2D"/>'s image with a new size,
        /// resizing the <see cref="Texture2D"/> but losing the image data.
        /// </summary>
        /// <param name="width">The new width for the <see cref="Texture2D"/>.</param>
        /// <param name="height">The new height for the <see cref="Texture2D"/>.</param>
        public void RecreateImage(int width, int height)
        {
            ValidateTextureSize(width, height);

            Width = width;
            Height = height;

            GraphicsDevice.BindTextureSetActive(this);
            if (Samples == 0)
                GL.TexImage2D(TextureType, 0, PixelInternalFormat, Width, Height, 0, PixelFormat, PixelType, IntPtr.Zero);
            else
                GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples, PixelInternalFormat, Width, Height, true);
        }

        private protected void ValidateTextureSize(int width, int height)
        {
            if (width <= 0 || width > GraphicsDevice.MaxTextureSize)
                throw new ArgumentOutOfRangeException(nameof(width), width, nameof(height) + " must be in the range (0, " + nameof(GraphicsDevice.MaxTextureSize) + "]");

            if (height <= 0 || height > GraphicsDevice.MaxTextureSize)
                throw new ArgumentOutOfRangeException(nameof(height), height, nameof(height) + " must be in the range (0, " + nameof(GraphicsDevice.MaxTextureSize) + "]");
        }

        private protected void ValidateSetOperation(int dataLength, int rectX, int rectY, int rectWidth, int rectHeight)
        {
            ValidateSetOperation(rectX, rectY, rectWidth, rectHeight);
            if (dataLength < rectWidth * rectHeight)
                throw new ArgumentException("The data Span doesn't have enough data to write the requested texture area", "data");
        }

        private protected void ValidateSetOperation(int rectX, int rectY, int rectWidth, int rectHeight)
        {
            if (Samples != 0)
                throw new InvalidOperationException("You can't write the pixels of a multisampled texture");

            ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);
        }

        private protected void ValidateGetOperation(int dataLength)
        {
            ValidateGetOperation();
            if (dataLength < Width * Height)
                throw new ArgumentException("The data Span isn't large enough to fit the requested texture area", "data");
        }

        private protected void ValidateGetOperation()
        {
            if (Samples != 0)
                throw new InvalidOperationException("You can't read the pixels of a multisampled texture");
        }

        internal void ValidateRectOperation(int rectX, int rectY, int rectWidth, int rectHeight)
        {
            if (rectX < 0 || rectY >= Height)
                throw new ArgumentOutOfRangeException(nameof(rectX), rectX, nameof(rectX) + " must be in the range [0, " + nameof(Width) + ")");

            if (rectY < 0 || rectY >= Height)
                throw new ArgumentOutOfRangeException(nameof(rectY), rectY, nameof(rectY) + " must be in the range [0, " + nameof(Height) + ")");

            if (rectWidth <= 0 || rectHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(rectWidth) + " and " + nameof(rectHeight) + " must be greater than 0");

            if (rectWidth > Width - rectX)
                throw new ArgumentOutOfRangeException(nameof(rectWidth), rectWidth, nameof(rectWidth) + " is too large");

            if (rectHeight > Height - rectY)
                throw new ArgumentOutOfRangeException(nameof(rectHeight), rectHeight, nameof(rectHeight) + " is too large");
        }

        internal void ValidateSampleCount(int samples)
        {
            if (samples < 0 || samples > GraphicsDevice.MaxSamples)
                throw new ArgumentOutOfRangeException(nameof(samples), samples, nameof(samples) + " must be in the range [0, " + nameof(GraphicsDevice.MaxSamples) + "]");
        }
    }
}
