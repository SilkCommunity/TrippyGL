using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace TrippyGL
{
    /// <summary>
    /// A configurable 2D framebuffer you can perform offscreen rendering operations to
    /// </summary>
    public class Framebuffer2D : GraphicsResource
    {
        /// <summary>The framebuffer's handle</summary>
        public readonly int Handle;

        /// <summary>The texture to which this framebuffer renders to</summary>
        public readonly Texture2D Texture;

        /// <summary>The depth-stencil format for this framebuffer</summary>
        public readonly DepthStencilFormat DepthStencil;

        /// <summary>Whether this framebuffer stores depth data</summary>
        public readonly bool HasDepth;

        /// <summary>Whether this framebuffer stores stencil data</summary>
        public readonly bool HasStencil;

        /// <summary>A Renderbuffer's handle that is attached to this framebuffer as either depth/stencil or both. 0 if no renderbuffer was attached</summary>
        private readonly int rbo;

        /// <summary>The width of this renderbuffer's image</summary>
        public int Width { get { return Texture.Width; } }

        /// <summary>The height of this renderbuffer's image</summary>
        public int Height { get { return Texture.Height; } }

        /// <summary>The amout of samples this renderbuffer has</summary>
        public int Samples { get { return Texture.Samples; } }

        /// <summary>
        /// Creates a Framebuffer2D with the specified parameters
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="imageFormat">The image format for this framebuffer's texture</param>
        /// <param name="samples"></param>
        public Framebuffer2D(GraphicsDevice graphicsDevice, int width, int height, DepthStencilFormat depthStencilFormat, TextureImageFormat imageFormat = TextureImageFormat.Color4b, int samples = 0)
            : base(graphicsDevice)
        {
            Texture = new Texture2D(graphicsDevice, width, height, false, samples, imageFormat);
            this.DepthStencil = depthStencilFormat;
            Handle = GL.GenFramebuffer();
            graphicsDevice.ForceBindFramebuffer(Handle);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, Texture.TextureType, Texture.Handle, 0);

            if (this.DepthStencil == DepthStencilFormat.None)
                rbo = 0;
            else
            {
                rbo = GL.GenRenderbuffer();
                graphicsDevice.ForceBindRenderbuffer(rbo);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, (RenderbufferStorage)this.DepthStencil, this.Width, this.Height);

                FramebufferAttachment rboAttachmentType;
                if (TrippyUtils.IsDepthStencilFormatDepthAndStencil(this.DepthStencil))
                {
                    rboAttachmentType = FramebufferAttachment.DepthStencilAttachment;
                    this.HasDepth = true;
                    this.HasStencil = true;
                }
                else if (TrippyUtils.IsDepthStencilFormatDepthOnly(this.DepthStencil))
                {
                    rboAttachmentType = FramebufferAttachment.DepthAttachment;
                    this.HasDepth = true;
                    this.HasStencil = false;
                }
                else if (TrippyUtils.IsDepthStencilFormatStencilOnly(this.DepthStencil))
                {
                    rboAttachmentType = FramebufferAttachment.StencilAttachment;
                    this.HasDepth = false;
                    this.HasStencil = true;
                }
                else
                    throw new ArgumentException("The provided DepthStencilFormat is invalid", "depthStencilFormat");

                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, rboAttachmentType, RenderbufferTarget.Renderbuffer, rbo);
            }

            FramebufferErrorCode c = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (c != FramebufferErrorCode.FramebufferComplete)
                throw new FramebufferCreationException(c);
        }

        /// <summary>
        /// Recreates the framebuffer's image and renderbuffers to a new size
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ReacreateFramebuffer(int width, int height)
        {
            if (rbo != 0)
                ValidateRenderbufferSize(width, height);

            Texture.RecreateImage(width, height);
            if (rbo != 0)
            {
                GraphicsDevice.BindRenderbuffer(rbo);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, (RenderbufferStorage)this.DepthStencil, width, height);
            }
        }

        /// <summary>
        /// Saves this texture as an image file. You can't save multisampled textures
        /// </summary>
        /// <param name="file">The location in which to store the file</param>
        /// <param name="imageFormat">The format</param>
        public void SaveAsImage(string file, SaveImageFormat imageFormat)
        {
            if (String.IsNullOrEmpty(file))
                throw new ArgumentException("You must specify a file name", "file");

            if (Texture.ImageFormat != TextureImageFormat.Color4b)
                throw new InvalidOperationException("In order to save a framebuffer as image, it must be in Color4b format");

            ImageFormat format;

            switch (imageFormat)
            {
                case SaveImageFormat.Png:
                    format = ImageFormat.Png;
                    break;
                case SaveImageFormat.Jpeg:
                    format = ImageFormat.Jpeg;
                    break;
                case SaveImageFormat.Bmp:
                    format = ImageFormat.Bmp;
                    break;
                case SaveImageFormat.Tiff:
                    format = ImageFormat.Tiff;
                    break;
                default:
                    throw new ArgumentException("You must use a proper value from SaveImageFormat", "imageFormat");
            }

            using (Bitmap b = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                BitmapData data = b.LockBits(new Rectangle(0, 0, this.Width, this.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GraphicsDevice.BindFramebufferRead(this);
                GL.ReadPixels(0, 0, this.Width, this.Height, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                //GraphicsDevice.EnsureTextureBoundAndActive(this);
                //GL.GetTexImage(this.TextureType, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                b.UnlockBits(data);
                b.Save(file, ImageFormat.Png);
            }
        }

        protected override void Dispose(bool isManualDispose)
        {
            GL.DeleteFramebuffer(this.Handle);
            if (rbo != 0)
                GL.DeleteRenderbuffer(rbo);

            base.Dispose(isManualDispose);
        }

        private protected void ValidateRenderbufferSize(int width, int height)
        {
            if (width <= 0 || width > GraphicsDevice.MaxRenderbufferSize)
                throw new ArgumentOutOfRangeException("width", width, "width must be in the range (0, MAX_RENDERBUFFER_SIZE]");

            if (height <= 0 || height > GraphicsDevice.MaxRenderbufferSize)
                throw new ArgumentOutOfRangeException("height", height, "height must be in the range (0, MAX_RENDERBUFFER_SIZE]");
        }
    }
}
