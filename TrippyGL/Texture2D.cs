using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace TrippyGL
{
    public class Texture2D : Texture
    {
        public readonly int Width, Height;
        public readonly int Multisample;

        internal Texture2D(int width, int height, int multisample, PixelInternalFormat pixelFormat = PixelInternalFormat.Rgba, PixelType pixelType = PixelType.UnsignedByte) : base(multisample == 0 ? TextureTarget.Texture2D : TextureTarget.Texture2DMultisample, pixelFormat, pixelType)
        {
            //BindToCurrentTextureUnit(); //texture is already bound by base constructor
            this.Width = width;
            this.Height = height;
            this.Multisample = multisample;

            if (this.Multisample == 0)
                GL.TexImage2D(this.TextureType, 0, this.PixelFormat, this.Width, this.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, this.PixelType, IntPtr.Zero);
            else
                GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, this.Multisample, this.PixelFormat, this.Width, this.Height, true);

            GL.TexParameter(this.TextureType, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(this.TextureType, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        internal Texture2D(string file, TextureTarget textureTarget) : base(textureTarget, PixelInternalFormat.Rgba, PixelType.UnsignedByte)
        {
            //BindToCurrentTextureUnit(); //texture is already bound by base constructor
            this.Multisample = 0;
            using (Bitmap bitmap = new Bitmap(file))
            {
                this.Width = bitmap.Width;
                this.Height = bitmap.Height;
                if (Width > maxTextureSize || Height > maxTextureSize)
                    throw new NotSupportedException("The maximum supported texture size on this system is " + maxTextureSize);

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, this.Width, this.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(this.TextureType, 0, this.PixelFormat, this.Width, this.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, this.PixelType, data.Scan0);
                bitmap.UnlockBits(data);
            }
            GL.TexParameter(this.TextureType, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(this.TextureType, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        public Texture2D(string file) : this(file, TextureTarget.Texture2D)
        {

        }

        /// <summary>
        /// Sets the data of the entire texture by copying from the specified pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read.
        /// You can't replace a multisampled texture's data
        /// </summary>
        /// <param name="dataPtr">The pointer to the data</param>
        /// <param name="pixelDataFormat">The format of the pixel data in dataPtr</param>
        public void SetData(IntPtr dataPtr, OpenTK.Graphics.OpenGL4.PixelFormat pixelDataFormat)
        {
            SetData(dataPtr, 0, 0, this.Width, this.Height, pixelDataFormat);
        }

        /// <summary>
        /// Sets the data of the entire texture by copying from the specified pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read.
        /// You can't replace a multisampled texture's data
        /// </summary>
        /// <param name="dataPtr">The pointer to the data</param>
        /// <param name="rectX">The X value of the first pixel to replace</param>
        /// <param name="rectY">The Y value of the first pixel to replace</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to replace</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to replace</param>
        /// <param name="pixelDataFormat">The format of the pixel data in dataPtr</param>
        public void SetData(IntPtr dataPtr, int rectX, int rectY, int rectWidth, int rectHeight, OpenTK.Graphics.OpenGL4.PixelFormat pixelDataFormat)
        {
            EnsureBoundAndActive();
            if (Multisample == 0)
                GL.TexSubImage2D(this.TextureType, 0, rectX, rectY, rectWidth, rectHeight, pixelDataFormat, this.PixelType, dataPtr);
            else
                throw new InvalidOperationException("You can't replace a multisampled texture's data");
        }

        public void SetData(Color4b[] data, int rectX, int rectY, int rectWidth, int rectHeight)
        {
            EnsureBoundAndActive();
            if (Multisample == 0)
                GL.TexSubImage2D(this.TextureType, 0, rectX, rectY, rectWidth, rectHeight, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, this.PixelType, data);
            else
                throw new NotSupportedException("You can't write the data of a multisampled texture");
        }

        public void SetData(Color4b[] colorData)
        {
            SetData(colorData, 0, 0, this.Width, this.Height);
        }

        private protected void ValidateSetOperation(Color4b[] data, int dataOffset, int rectX, int rectY, int rectWidth, int rectHeight)
        {
            if (data == null)
                throw new ArgumentNullException("data", "Color data array can't be null");

            if (dataOffset < 0 || dataOffset >= data.Length)
                throw new ArgumentOutOfRangeException("dataOffset", "dataOffset must be in the range [0, data.Length)");

            if (data.Length - dataOffset < (rectWidth - rectX) * (rectHeight - rectY))
                throw new ArgumentException("The provided data array isn't big enough starting from dataOffset", "data");

            ValidateSetOperation(rectX, rectY, rectWidth, rectHeight);
        }

        private protected void ValidateSetOperation(int rectX, int rectY, int rectWidth, int rectHeight)
        {
            if (rectX < 0 || rectY >= this.Height)
                throw new ArgumentOutOfRangeException("rectX", rectX, "rectX must be in the range [0, Width)");

            if (rectY < 0 || rectY >= this.Height)
                throw new ArgumentOutOfRangeException("rectY", rectY, "rectY must be in the range [0, Height)");

            if (rectWidth > this.Width - rectX)
                throw new ArgumentOutOfRangeException("rectWidth", rectWidth, "rectWidth is too large");

            if (rectHeight > this.Height - rectY)
                throw new ArgumentOutOfRangeException("rectHeight", rectHeight, "rectHeight is too large");
        }
    }
}
