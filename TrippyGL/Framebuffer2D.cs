using System;
using TrippyGL.Utils;

#pragma warning disable CA1001 // Types that own disposable fields should be disposable

namespace TrippyGL
{
    /// <summary>
    /// A helper type that provides an easy to use 2D <see cref="FramebufferObject"/>.
    /// </summary>
    public readonly struct Framebuffer2D : IDisposable, IEquatable<Framebuffer2D>
    {
        /// <summary>Whether this <see cref="Framebuffer2D"/> has null values.</summary>
        public bool IsEmpty => Framebuffer == null;

        /// <summary>This <see cref="Framebuffer2D"/>'s backing <see cref="FramebufferObject"/>.</summary>
        public readonly FramebufferObject Framebuffer;

        /// <summary>The <see cref="Texture2D"/> onto which this <see cref="Framebuffer2D"/> renders.</summary>
        public readonly Texture2D Texture;

        /// <summary>The width of this <see cref="Framebuffer2D"/>'s image.</summary>
        public uint Width => Framebuffer.Width;

        /// <summary>The height of this <see cref="Framebuffer2D"/>'s image.</summary>
        public uint Height => Framebuffer.Height;

        /// <summary>The amount of samples this <see cref="Framebuffer2D"/> has.</summary>
        public uint Samples => Framebuffer.Samples;

        /// <summary>
        /// Creates a <see cref="Framebuffer2D"/> with the given width, height, and other optional parameters.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this <see cref="Framebuffer2D"/> will use.</param>
        /// <param name="width">The width of the <see cref="Framebuffer2D"/>'s image.</param>
        /// <param name="height">The height of the <see cref="Framebuffer2D"/>'s image.</param>
        /// <param name="depthStencilFormat">The depth-stencil format for an optional renderbuffer attachment.</param>
        /// <param name="samples">The amount of samples for the <see cref="Framebuffer2D"/>'s image.</param>
        /// <param name="imageFormat">The format of the <see cref="Framebuffer2D"/>'s image.</param>
        /// <param name="useDepthStencilTexture">Whether to use a texture for the depth-stencil buffer instead of a renderbuffer.</param>
        public Framebuffer2D(GraphicsDevice graphicsDevice, uint width, uint height,
            DepthStencilFormat depthStencilFormat, uint samples = 0,
            TextureImageFormat imageFormat = TextureImageFormat.Color4b, bool useDepthStencilTexture = false)
        {
            Framebuffer = new FramebufferObject(graphicsDevice);
            Texture = new Texture2D(graphicsDevice, width, height, false, samples, imageFormat);

            if (depthStencilFormat != DepthStencilFormat.None)
            {
                if (useDepthStencilTexture)
                {
                    TextureImageFormat dsFormat = TrippyUtils.DepthStencilFormatToTextureFormat(depthStencilFormat);
                    Texture2D dsTexture = new Texture2D(graphicsDevice, width, height, false, samples, dsFormat);
                    Framebuffer.Attach(dsTexture, TrippyUtils.GetCorrespondingTextureFramebufferAttachmentPoint(dsFormat));
                }
                else
                {
                    RenderbufferObject rbo = new RenderbufferObject(graphicsDevice, width, height, (RenderbufferFormat)depthStencilFormat, samples);
                    Framebuffer.Attach(rbo, TrippyUtils.GetCorrespondingRenderbufferFramebufferAttachmentPoint(rbo.Format));
                }
            }

            Framebuffer.Attach(Texture, FramebufferAttachmentPoint.Color0);
            Framebuffer.UpdateFramebufferData();
        }

        public static implicit operator FramebufferObject(Framebuffer2D framebuffer) => framebuffer.Framebuffer;
        public static implicit operator Texture2D(Framebuffer2D framebuffer) => framebuffer.Texture;

        public static bool operator ==(Framebuffer2D left, Framebuffer2D right) => left.Equals(right);
        public static bool operator !=(Framebuffer2D left, Framebuffer2D right) => !left.Equals(right);

        /// <summary>
        /// Resizes this <see cref="Framebuffer2D"/>.
        /// </summary>
        /// <param name="width">The new width for the <see cref="Framebuffer2D"/>.</param>
        /// <param name="height">The new height for the <see cref="Framebuffer2D"/>.</param>
        /// <remarks>This resizes all the framebuffer attachments.</remarks>
        public void Resize(uint width, uint height)
        {
            if (width == Width && height == Height)
                return;

            // We go through all the texture attachments and resize them.
            for (int i = 0; i < Framebuffer.textureAttachments.Count; i++)
            {
                if (!(Framebuffer.textureAttachments[i].Texture is Texture2D tex2d))
                    throw new InvalidOperationException("This framebuffer contains non-Texture2D texture attachments, a Resize2D operation is invalid");
                tex2d.RecreateImage(width, height);
            }

            // We go through all the renderbuffer attachments and resize them.
            for (int i = 0; i < Framebuffer.renderbufferAttachments.Count; i++)
            {
                // We can't resize renderbuffers, so we dispose them and create new ones.
                FramebufferRenderbufferAttachment att = Framebuffer.renderbufferAttachments[i];
                Framebuffer.TryDetachRenderbuffer(att.AttachmentPoint, out att);
                att.Renderbuffer.Dispose();
                Framebuffer.Attach(new RenderbufferObject(Framebuffer.GraphicsDevice, width, height, att.Renderbuffer.Format, att.Renderbuffer.Samples), att.AttachmentPoint);
            }

            Framebuffer.UpdateFramebufferData();
        }

        public void Dispose()
        {
            Framebuffer.DisposeAttachments();
            Framebuffer.Dispose();
        }

        public override string ToString()
        {
            return IsEmpty ? "Empty " + nameof(Framebuffer2D) : string.Concat(
                nameof(Framebuffer2D) + ": Width=", Width.ToString(),
                ", Height=", Height.ToString(),
                ", Samples=", Samples.ToString()
            );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Framebuffer);
        }

        public bool Equals(Framebuffer2D other)
        {
            return Framebuffer == other.Framebuffer;
        }

        public override bool Equals(object obj)
        {
            if (obj is Framebuffer2D framebuffer)
                return Equals(framebuffer);
            return false;
        }
    }
}
