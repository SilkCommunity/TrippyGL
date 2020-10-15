using System;
using System.Collections.Generic;
using Silk.NET.OpenGL;
using TrippyGL.Utils;

namespace TrippyGL
{
    /// <summary>
    /// A configurable framebuffer that can be used to perform offscreen drawing operations.
    /// </summary>
    public sealed class FramebufferObject : GraphicsResource
    {
        /// <summary>The handle for the GL Framebuffer Object.</summary>
        public readonly uint Handle;

        /// <summary>The width of this <see cref="FramebufferObject"/>'s image.</summary>
        public uint Width { get; private set; }

        /// <summary>The height of this <see cref="FramebufferObject"/>'s image.</summary>
        public uint Height { get; private set; }

        /// <summary>The amount of samples this <see cref="FramebufferObject"/> has.</summary>
        public uint Samples { get; private set; }

        internal readonly List<FramebufferTextureAttachment> textureAttachments;
        internal readonly List<FramebufferRenderbufferAttachment> renderbufferAttachments;

        /// <summary>The amount of <see cref="Texture"/> attachments this <see cref="FramebufferObject"/> has.</summary>
        public int TextureAttachmentCount => textureAttachments.Count;

        /// <summary>The amount of <see cref="RenderbufferObject"/> attachments this <see cref="FramebufferObject"/> has.</summary>
        public int RenderbufferAttachmentCount => renderbufferAttachments.Count;

        /// <summary>
        /// Creates a <see cref="FramebufferObject"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        public FramebufferObject(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            Samples = 0;
            Width = 0;
            Height = 0;
            textureAttachments = new List<FramebufferTextureAttachment>();
            renderbufferAttachments = new List<FramebufferRenderbufferAttachment>();
            Handle = GL.GenFramebuffer();
        }

        /// <summary>
        /// Attaches a <see cref="Texture"/> to this <see cref="FramebufferObject"/> in a specified attachment point.
        /// </summary>
        /// <param name="texture">The <see cref="Texture"/> to attach.</param>
        /// <param name="attachmentPoint">The attachment point to attach the <see cref="Texture"/> to.</param>
        public void Attach(Texture texture, FramebufferAttachmentPoint attachmentPoint)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            ValidateAttachmentTypeExists(attachmentPoint);
            ValidateAttachmentTypeNotUsed(attachmentPoint);

            if (attachmentPoint == FramebufferAttachmentPoint.Depth && !TrippyUtils.IsImageFormatDepthOnly(texture.ImageFormat))
                throw new InvalidFramebufferAttachmentException("When attaching a texture to a depth attachment point, the texture's format must be depth-only");

            if (attachmentPoint == FramebufferAttachmentPoint.DepthStencil && !TrippyUtils.IsImageFormatDepthStencil(texture.ImageFormat))
                throw new InvalidFramebufferAttachmentException("When attaching a texture to a depth-stencil attachment point, the texture's format must be depth-stencil");

            if (attachmentPoint == FramebufferAttachmentPoint.Stencil && !TrippyUtils.IsImageFormatStencilOnly(texture.ImageFormat))
                throw new InvalidFramebufferAttachmentException("When attaching a texture to a stencil attachment point, the texture's format must be stencil-only");

            if (TrippyUtils.IsFramebufferAttachmentPointColor(attachmentPoint) && !TrippyUtils.IsImageFormatColorRenderable(texture.ImageFormat))
                throw new InvalidFramebufferAttachmentException("When attaching a texture to a color attachment point, the texture's format must be color-renderable");

            GraphicsDevice.Framebuffer = this;
            if (texture is Texture1D)
            {
                GL.FramebufferTexture1D(FramebufferTarget.Framebuffer, (FramebufferAttachment)attachmentPoint, texture.TextureType, texture.Handle, 0);
            }
            else if (texture is Texture2D)
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, (FramebufferAttachment)attachmentPoint, texture.TextureType, texture.Handle, 0);
            }
            else
                throw new InvalidFramebufferAttachmentException("This texture type cannot be attached to a framebuffer");
            textureAttachments.Add(new FramebufferTextureAttachment(texture, attachmentPoint));
        }

        /// <summary>
        /// Attaches a <see cref="RenderbufferObject"/> to this <see cref="FramebufferObject"/> in a specified attachment point.
        /// </summary>
        /// <param name="renderbuffer">The <see cref="RenderbufferObject"/> to attach.</param>
        /// <param name="attachmentPoint">The attachment point to attach the <see cref="RenderbufferObject"/> to.</param>
        public void Attach(RenderbufferObject renderbuffer, FramebufferAttachmentPoint attachmentPoint)
        {
            if (renderbuffer == null)
                throw new ArgumentNullException(nameof(renderbuffer));

            ValidateAttachmentTypeExists(attachmentPoint);
            ValidateAttachmentTypeNotUsed(attachmentPoint);

            if (attachmentPoint == FramebufferAttachmentPoint.Depth && !renderbuffer.IsDepthOnly)
                throw new InvalidFramebufferAttachmentException("When attaching a renderbuffer to a depth attachment point, the renderbuffer's format must be depth-only");

            if (attachmentPoint == FramebufferAttachmentPoint.DepthStencil && !renderbuffer.IsDepthStencil)
                throw new InvalidFramebufferAttachmentException("When attaching a renderbuffer to a depth-stencil attachment point, the renderbuffer's format must be depth-stencil");

            if (attachmentPoint == FramebufferAttachmentPoint.Stencil && !renderbuffer.IsStencilOnly)
                throw new InvalidFramebufferAttachmentException("When attaching a renderbuffer to a stencil attachment point, the renderbuffer's format must be stencil-only");

            if (TrippyUtils.IsFramebufferAttachmentPointColor(attachmentPoint) && !renderbuffer.IsColorRenderableFormat)
                throw new InvalidFramebufferAttachmentException("When attaching a renderbuffer to a color attachment point, the renderbuffer's format must be color-renderable");

            GraphicsDevice.Framebuffer = this;
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, (FramebufferAttachment)attachmentPoint, RenderbufferTarget.Renderbuffer, renderbuffer.Handle);
            renderbufferAttachments.Add(new FramebufferRenderbufferAttachment(renderbuffer, attachmentPoint));
        }

        /// <summary>
        /// Detaches whatever is in an attachment point.
        /// Throws an exception if there is no such attachment.
        /// </summary>
        /// <param name="attachmentPoint">The attachment point to clear.</param>
        public void Detach(FramebufferAttachmentPoint attachmentPoint)
        {
            if (!TryDetachTexture(attachmentPoint, out _))
                if (!TryDetachRenderbuffer(attachmentPoint, out _))
                    throw new InvalidOperationException("The specified attachment point is empty");
        }

        /// <summary>
        /// Tries to detach a <see cref="Texture"/> attached to the specified attachment point.
        /// </summary>
        /// <param name="point">The attachment point to check.</param>
        /// <param name="attachment">The detached <see cref="Texture"/> attachment, if the method returned true.</param>
        /// <returns>Returns whether the operation succeeded.</returns>
        public bool TryDetachTexture(FramebufferAttachmentPoint point, out FramebufferTextureAttachment attachment)
        {
            for (int i = 0; i < textureAttachments.Count; i++)
                if (textureAttachments[i].AttachmentPoint == point)
                {
                    GraphicsDevice.Framebuffer = this;
                    GL.FramebufferTexture(FramebufferTarget.Framebuffer, (FramebufferAttachment)point, 0, 0);
                    attachment = textureAttachments[i];
                    textureAttachments.RemoveAt(i);
                    return true;
                }

            attachment = default;
            return false;
        }

        /// <summary>
        /// Tries to detach a <see cref="RenderbufferObject"/> attached to the specified point.
        /// </summary>
        /// <param name="point">The attachment point to check.</param>
        /// <param name="attachment">The detached <see cref="RenderbufferObject"/> attachment, if the method returned true.</param>
        /// <returns>Returns whether the operation succeded.</returns>
        public bool TryDetachRenderbuffer(FramebufferAttachmentPoint point, out FramebufferRenderbufferAttachment attachment)
        {
            for (int i = 0; i < renderbufferAttachments.Count; i++)
                if (renderbufferAttachments[i].AttachmentPoint == point)
                {
                    GraphicsDevice.Framebuffer = this;
                    GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, (FramebufferAttachment)point, RenderbufferTarget.Renderbuffer, 0);
                    attachment = renderbufferAttachments[i];
                    renderbufferAttachments.RemoveAt(i);
                    return true;
                }

            attachment = default;
            return false;
        }

        /// <summary>
        /// Returns whether the specified attachment point is in use.
        /// </summary>
        /// <param name="attachmentType">The attachment point to check.</param>
        public bool HasAttachment(FramebufferAttachmentPoint attachmentType)
        {
            for (int i = 0; i < textureAttachments.Count; i++)
                if (textureAttachments[i].AttachmentPoint == attachmentType)
                    return true;

            for (int i = 0; i < renderbufferAttachments.Count; i++)
                if (renderbufferAttachments[i].AttachmentPoint == attachmentType)
                    return true;

            return false;
        }

        /// <summary>
        /// Gets the status of the <see cref="FramebufferObject"/>. 
        /// </summary>
        public FramebufferStatus GetStatus()
        {
            GraphicsDevice.Framebuffer = this;
            return (FramebufferStatus)GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        }

        /// <summary>
        /// Updates the <see cref="FramebufferObject"/>'s parameters and checks that the framebuffer is valid.
        /// This should always be called after being done attaching or detaching resources.
        /// </summary>
        public void UpdateFramebufferData()
        {
            uint width = uint.MaxValue;
            uint height = uint.MaxValue;
            uint samples = uint.MaxValue;

            for (int i = 0; i < textureAttachments.Count; i++)
            {
                Texture tex = textureAttachments[i].Texture;
                if (tex is Texture1D tex1d)
                    ValidateSize(tex1d.Width, 1);
                else if (tex is Texture2D tex2d)
                    ValidateSize(tex2d.Width, tex2d.Height);
                else
                    throw new FramebufferException("The texture format cannot be attached: " + tex.TextureType);

                ValidateSamples(tex is TextureMultisamplable ms ? ms.Samples : 0);
            }

            for (int i = 0; i < renderbufferAttachments.Count; i++)
            {
                RenderbufferObject rend = renderbufferAttachments[i].Renderbuffer;
                ValidateSize(rend.Width, rend.Height);
                ValidateSamples(rend.Samples);
            }

            Width = width;
            Height = height;
            Samples = samples;

            void ValidateSize(uint w, uint h)
            {
                if (width == uint.MaxValue)
                    width = w;
                else if (width != w)
                    throw new FramebufferException("All the attachments must be the same size");

                if (height == uint.MaxValue)
                    height = h;
                else if (height != h)
                    throw new FramebufferException("All the attachments must be the same size");
            }

            void ValidateSamples(uint s)
            {
                if (samples == uint.MaxValue)
                    samples = s;
                else if (samples != s)
                    throw new FramebufferException("All the attachments must have the same amount of samples");
            }

            FramebufferStatus c = GetStatus();
            if (c != FramebufferStatus.FramebufferComplete)
                throw new FramebufferException("The " + nameof(FramebufferObject) + " is not complete: " + c);
        }

        /// <summary>
        /// Gets a <see cref="Texture"/> attachment from this <see cref="FramebufferObject"/>.
        /// </summary>
        /// <param name="attachmentPoint">The point to look for a texture attachment at.</param>
        /// <param name="attachment">The attachment found.</param>
        /// <returns>Whether a texture attachment was found at the specified attachment point.</returns>
        public bool TryGetTextureAttachment(FramebufferAttachmentPoint attachmentPoint, out FramebufferTextureAttachment attachment)
        {
            for (int i = 0; i < textureAttachments.Count; i++)
                if (textureAttachments[i].AttachmentPoint == attachmentPoint)
                {
                    attachment = textureAttachments[i];
                    return true;
                }

            attachment = default;
            return false;
        }

        /// <summary>
        /// Gets a <see cref="RenderbufferObject"/> attachment from this <see cref="FramebufferObject"/>.
        /// </summary>
        /// <param name="attachmentPoint">The point to look for a renderbuffer attachment at.</param>
        /// <param name="attachment">The attachment found.</param>
        /// <returns>Whether a renderbuffer attachment was found at the specified attachment point.</returns>
        public bool TryGetRenderbufferAttachment(FramebufferAttachmentPoint attachmentPoint, out FramebufferRenderbufferAttachment attachment)
        {
            for (int i = 0; i < renderbufferAttachments.Count; i++)
                if (renderbufferAttachments[i].AttachmentPoint == attachmentPoint)
                {
                    attachment = renderbufferAttachments[i];
                    return true;
                }

            attachment = default;
            return false;
        }

        /// <summary>
        /// Reads pixels from this <see cref="FramebufferObject"/>.
        /// </summary>
        /// <param name="ptr">The pointer to which the pixel data will be written.</param>
        /// <param name="x">The X coordinate of the first pixel to read.</param>
        /// <param name="y">The (invertex) Y coordinate of the first pixel to read.</param>
        /// <param name="width">The width of the rectangle of pixels to read.</param>
        /// <param name="height">The height of the rectangle of pixels to read.</param>
        /// <param name="pixelFormat">The format the pixel data will be read as.</param>
        /// <param name="pixelType">The format the pixel data is stored as.</param>
        public unsafe void ReadPixelsPtr(void* ptr, int x, int y, uint width, uint height, ReadPixelsFormat pixelFormat = ReadPixelsFormat.Rgba, PixelType pixelType = PixelType.UnsignedByte)
        {
            if (x < 0)
                throw new ArgumentException(nameof(x) + " must be greater than or equal to 0", nameof(x));

            if (y < 0)
                throw new ArgumentException(nameof(y) + " must be greater than or equal to 0", nameof(y));

            if (width <= 0 || width > Width)
                throw new ArgumentOutOfRangeException(nameof(width), width, nameof(width) + " must be in the range (0, " + nameof(Width) + "]");

            if (height <= 0 || height > Height)
                throw new ArgumentOutOfRangeException(nameof(height), height, nameof(height) + " must be in the range (0, " + nameof(Height) + "]");

            if (x + width > Width || y + height > Height)
                throw new ArgumentException("Tried to read outside the " + nameof(FramebufferObject) + "'s image");

            if (pixelFormat == ReadPixelsFormat.DepthStencil && pixelType != (PixelType)GLEnum.UnsignedInt248)
                throw new ArgumentException("When reading depth-stencil, " + nameof(pixelType) + " must be UnsignedInt248", nameof(pixelType));

            GraphicsDevice.ReadFramebuffer = this;
            GL.ReadPixels(x, y, width, height, (PixelFormat)pixelFormat, pixelType, ptr);
        }

        /// <summary>
        /// Reads pixels from this <see cref="FramebufferObject"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as a pixel to read.</typeparam>
        /// <param name="data">The span to which the data will be written.</param>
        /// <param name="x">The X coordinate of the first pixel to read.</param>
        /// <param name="y">The (invertex) Y coordinate of the first pixel to read.</param>
        /// <param name="width">The width of the rectangle of pixels to read.</param>
        /// <param name="height">The height of the rectangle of pixels to read.</param>
        /// <param name="pixelFormat">The format the pixel data will be read as.</param>
        /// <param name="pixelType">The format the pixel data is stored as.</param>
        public unsafe void ReadPixels<T>(Span<T> data, int x, int y, uint width, uint height, ReadPixelsFormat pixelFormat = ReadPixelsFormat.Rgba, PixelType pixelType = PixelType.UnsignedByte) where T : unmanaged
        {
            if (data.Length < (width - x) * (height - y))
                throw new ArgumentException(nameof(data) + " must be able to hold the requested pixel data", nameof(data));

            fixed (void* ptr = data)
                ReadPixelsPtr(ptr, x, y, width, height, pixelFormat, pixelType);
        }

        public override string ToString()
        {
            return string.Concat(
                nameof(Handle) + "=", Handle.ToString(),
                ", Width=", Width.ToString(),
                ", Height=", Height.ToString(),
                ", Samples=", Samples.ToString(),
                ", " + nameof(TextureAttachmentCount) + "=", TextureAttachmentCount.ToString(),
                ", " + nameof(RenderbufferAttachmentCount) + "=", RenderbufferAttachmentCount.ToString()
            );
        }

        protected override void Dispose(bool isManualDispose)
        {
            if (isManualDispose)
            {
                bool isDraw = GraphicsDevice.DrawFramebuffer == this;
                bool isRead = GraphicsDevice.ReadFramebuffer == this;

                if (isDraw && isRead)
                    GraphicsDevice.Framebuffer = null;
                else if (isDraw)
                    GraphicsDevice.DrawFramebuffer = null;
                else if (isRead)
                    GraphicsDevice.ReadFramebuffer = null;
            }

            GL.DeleteFramebuffer(Handle);
        }

        /// <summary>
        /// Disposes all of the attachments.
        /// </summary>
        public void DisposeAttachments()
        {
            GraphicsDevice.Framebuffer = this;
            for (int i = 0; i < textureAttachments.Count; i++)
            {
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, (FramebufferAttachment)textureAttachments[i].AttachmentPoint, 0, 0);
                textureAttachments[i].Texture.Dispose();
            }
            textureAttachments.Clear();
            for (int i = 0; i < renderbufferAttachments.Count; i++)
            {
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, (FramebufferAttachment)renderbufferAttachments[i].AttachmentPoint, RenderbufferTarget.Renderbuffer, 0);
                renderbufferAttachments[i].Renderbuffer.Dispose();
            }
            renderbufferAttachments.Clear();
        }

        private static void ValidateAttachmentTypeExists(FramebufferAttachmentPoint attachment)
        {
            if (!Enum.IsDefined(typeof(FramebufferAttachmentPoint), attachment))
                throw new FormatException("Invalid attachment point");
        }

        private void ValidateAttachmentTypeNotUsed(FramebufferAttachmentPoint attachment)
        {
            if (HasAttachment(attachment))
                throw new InvalidOperationException("The " + nameof(FramebufferObject) + " already has this type of attachment");
        }
    }
}