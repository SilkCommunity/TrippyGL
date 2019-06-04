using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace TrippyGL
{
    public class FramebufferObject : GraphicsResource
    {

        public readonly int Handle;

        public readonly Texture2D Texture;

        public int Width { get { return Texture.Width; } }
        public int Height { get { return Texture.Height; } }
        public int Samples { get { return Texture.Samples; } }

        public FramebufferObject(GraphicsDevice graphicsDevice, int width, int height, int samples, PixelInternalFormat pixelFormat = PixelInternalFormat.Rgba, PixelType pixelType = PixelType.UnsignedByte)
            : base(graphicsDevice)
        {
            Texture = new Texture2D(graphicsDevice, width, height, samples, pixelFormat, pixelType);
            Handle = GL.GenFramebuffer();
        }

        public FramebufferObject(GraphicsDevice graphicsDevice, int width, int height)
            : this(graphicsDevice, width, height, 0, PixelInternalFormat.Rgba, PixelType.UnsignedByte)
        {
            
        }

        protected override void Dispose(bool isManualDispose)
        {
            GL.DeleteFramebuffer(this.Handle);
            base.Dispose(isManualDispose);
        }
    }
}
