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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="samples"></param>
        /// <param name="pixelFormat"></param>
        /// <param name="pixelType"></param>
        public FramebufferObject(GraphicsDevice graphicsDevice, int width, int height, int samples = 0, PixelInternalFormat pixelFormat = PixelInternalFormat.Rgba, PixelType pixelType = PixelType.UnsignedByte)
            : base(graphicsDevice)
        {
            Texture = new Texture2D(graphicsDevice, width, height, false, samples, pixelFormat, pixelType);
            Handle = GL.GenFramebuffer();
        }

        protected override void Dispose(bool isManualDispose)
        {
            GL.DeleteFramebuffer(this.Handle);
            base.Dispose(isManualDispose);
        }
    }
}
