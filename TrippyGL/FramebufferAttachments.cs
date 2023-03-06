using System;

namespace TrippyGL
{
    /// <summary>
    /// Represents an attachment of a <see cref="TrippyGL.Texture"/> to a <see cref="FramebufferObject"/>.
    /// </summary>
    public readonly struct FramebufferTextureAttachment : IEquatable<FramebufferTextureAttachment>
    {
        /// <summary>The <see cref="Texture"/> in this framebuffer attachment.</summary>
        public readonly Texture Texture;

        /// <summary>The attachment point to which this attachment is attached in a <see cref="FramebufferObject"/>.</summary>
        public readonly FramebufferAttachmentPoint AttachmentPoint;

        /// <summary>
        /// Creates a <see cref="FramebufferTextureAttachment"/>.
        /// </summary>
        /// <param name="texture">The <see cref="Texture"/> to attach in this attachment.</param>
        /// <param name="attachmentPoint">The attachment point to which this attachment attaches to.</param>
        public FramebufferTextureAttachment(Texture texture, FramebufferAttachmentPoint attachmentPoint)
        {
            Texture = texture;
            AttachmentPoint = attachmentPoint;
        }

        public static bool operator ==(FramebufferTextureAttachment left, FramebufferTextureAttachment right) => left.Equals(right);
        public static bool operator !=(FramebufferTextureAttachment left, FramebufferTextureAttachment right) => !left.Equals(right);

        public override string ToString()
        {
            return string.Concat("Texture" + nameof(AttachmentPoint) + "=", AttachmentPoint.ToString());
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Texture, AttachmentPoint);
        }

        public bool Equals(FramebufferTextureAttachment other)
        {
            return Texture == other.Texture && AttachmentPoint == other.AttachmentPoint;
        }

        public override bool Equals(object? obj)
        {
            if (obj is FramebufferTextureAttachment framebufferTextureAttachment)
                return Equals(framebufferTextureAttachment);
            return false;
        }
    }

    /// <summary>
    /// Represents an attachment of a <see cref="RenderbufferObject"/> to a <see cref="FramebufferObject"/>.
    /// </summary>
    public readonly struct FramebufferRenderbufferAttachment : IEquatable<FramebufferRenderbufferAttachment>
    {
        /// <summary>The <see cref="RenderbufferObject"/> in this framebuffer attachment.</summary>
        public readonly RenderbufferObject Renderbuffer;

        /// <summary>The attachment point to which this attachment is attached in a <see cref="FramebufferObject"/>.</summary>
        public readonly FramebufferAttachmentPoint AttachmentPoint;

        /// <summary>
        /// Creates a <see cref="FramebufferRenderbufferAttachment"/>.
        /// </summary>
        /// <param name="renderbuffer">The <see cref="RenderbufferObject"/> to attach in this attachment.</param>
        /// <param name="attachmentPoint">The attachment point to which this attachment attaches to.</param>
        public FramebufferRenderbufferAttachment(RenderbufferObject renderbuffer, FramebufferAttachmentPoint attachmentPoint)
        {
            Renderbuffer = renderbuffer;
            AttachmentPoint = attachmentPoint;
        }

        public static bool operator ==(FramebufferRenderbufferAttachment left, FramebufferRenderbufferAttachment right) => left.Equals(right);
        public static bool operator !=(FramebufferRenderbufferAttachment left, FramebufferRenderbufferAttachment right) => !left.Equals(right);

        public override string ToString()
        {
            return string.Concat("Renderbuffer" + nameof(AttachmentPoint) + "=", AttachmentPoint.ToString());
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Renderbuffer, AttachmentPoint);
        }

        public bool Equals(FramebufferRenderbufferAttachment other)
        {
            return Renderbuffer == other.Renderbuffer && AttachmentPoint == other.AttachmentPoint;
        }

        public override bool Equals(object? obj)
        {
            if (obj is FramebufferRenderbufferAttachment framebufferRenderbufferAttachment)
                return Equals(framebufferRenderbufferAttachment);
            return false;
        }
    }
}
