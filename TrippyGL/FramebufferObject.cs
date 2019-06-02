using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace TrippyGL
{
    public class FramebufferObject : Texture2D
    {
        public FramebufferObject(int width, int height, int samples, PixelInternalFormat pixelFormat, PixelType pixelType)
            : base(width, height, samples, pixelFormat, pixelType)
        {

        }

        public FramebufferObject(int width, int height) : this(width, height, 0, PixelInternalFormat.Rgba, PixelType.UnsignedByte)
        {

        }
    }
}
