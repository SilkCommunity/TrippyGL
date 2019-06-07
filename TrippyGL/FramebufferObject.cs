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

        public readonly DepthStencilFormat DepthStencil;

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
        /// <param name="imageFormat">The image format for this framebuffer's texture</param>
        public FramebufferObject(GraphicsDevice graphicsDevice, int width, int height, DepthStencilFormat depthStencilFormat, int samples = 0, TextureImageFormat imageFormat = TextureImageFormat.Color4b)
            : base(graphicsDevice)
        {
            Texture = new Texture2D(graphicsDevice, width, height, false, samples, imageFormat);
            this.DepthStencil = depthStencilFormat;
            Handle = GL.GenFramebuffer();
            graphicsDevice.BindFramebuffer(Handle);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, Texture.TextureType, Texture.Handle, 0);
        }

        public void ReacreateFramebuffer()
        {

        }

        protected override void Dispose(bool isManualDispose)
        {
            GL.DeleteFramebuffer(this.Handle);
            base.Dispose(isManualDispose);
        }
    }
}
