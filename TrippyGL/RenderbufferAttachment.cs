using System;

namespace TrippyGL
{

    /// <summary>
    /// Represents an attachment of a <see cref="TrippyGL.Texture"/> to a <see cref="FramebufferObject"/>.
    /// </summary>
    public readonly struct FramebufferTextureAttachment : IEquatable<FramebufferTextureAttachment>
    {
        public readonly Texture Texture;
        public readonly FramebufferAttachmentPoint AttachmentPoint;

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
            unchecked
            {
                int hashCode = Texture.GetHashCode();
                hashCode = (hashCode * 397) ^ AttachmentPoint.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(FramebufferTextureAttachment other)
        {
            return Texture == other.Texture && AttachmentPoint == other.AttachmentPoint;
        }

        public override bool Equals(object obj)
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
        public readonly RenderbufferObject Renderbuffer;
        public readonly FramebufferAttachmentPoint AttachmentPoint;

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
            unchecked
            {
                int hashCode = Renderbuffer.GetHashCode();
                hashCode = (hashCode * 397) ^ AttachmentPoint.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(FramebufferRenderbufferAttachment other)
        {
            return Renderbuffer == other.Renderbuffer && AttachmentPoint == other.AttachmentPoint;
        }

        public override bool Equals(object obj)
        {
            if (obj is FramebufferRenderbufferAttachment framebufferRenderbufferAttachment)
                return Equals(framebufferRenderbufferAttachment);
            return false;
        }
    }
}
