using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace TrippyGL
{
    public class Texture1D : Texture
    {
        public readonly int Width;
        
        public Texture1D(string file) : base(TextureTarget.Texture1D, PixelInternalFormat.Rgba, PixelType.UnsignedByte)
        {
            //texture is already bound by base constructor
            using (Bitmap bitmap = new Bitmap(file))
            {
                this.Width = bitmap.Width;
                ValidateTextureSize(Width);

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, this.Width, 1), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage1D(this.TextureType, 0, this.PixelFormat, this.Width, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, this.PixelType, data.Scan0);
                bitmap.UnlockBits(data);
            }

            GL.TexParameter(this.TextureType, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(this.TextureType, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        private protected void ValidateTextureSize(int width)
        {
            if (width <= 0 || width > maxTextureSize)
                throw new ArgumentOutOfRangeException("width", width, "Texture width must be in the range (0, MAX_TEXTURE_SIZE]");
        }
    }
}
