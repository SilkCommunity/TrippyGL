using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with <see cref="Vector3"/> Position, <see cref="Vector3"/> Normal,
    /// <see cref="Color4b"/> Color and <see cref="Vector2"/> TexCoords.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexNormalColorTexture : IVertex, IEquatable<VertexNormalColorTexture>
    {
        /// <summary>The size of a <see cref="VertexNormalColorTexture"/> measured in bytes.</summary>
        public const int SizeInBytes = (3 + 3 + 1 + 2) * 4;

        /// <summary>The vertex's position.</summary>
        public Vector3 Position;

        /// <summary>The vertex's normal.</summary>
        public Vector3 Normal;

        /// <summary>The vertex's color.</summary>
        public Color4b Color;

        /// <summary>The vertex's texture coordinates.</summary>
        public Vector2 TexCoords;

        /// <summary>
        /// Creates a <see cref="VertexNormalColorTexture"/> with the specified position, normal, color and texture coordinates.
        /// </summary>
        public VertexNormalColorTexture(Vector3 position, Vector3 normal, Color4b color, Vector2 texCoords)
        {
            Position = position;
            Normal = normal;
            Color = color;
            TexCoords = texCoords;
        }

        public static bool operator ==(VertexNormalColorTexture left, VertexNormalColorTexture right) => left.Equals(right);
        public static bool operator !=(VertexNormalColorTexture left, VertexNormalColorTexture right) => !left.Equals(right);

        public int AttribDescriptionCount => 4;

        public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
        {
            descriptions[0] = new VertexAttribDescription(AttributeType.FloatVec3);
            descriptions[1] = new VertexAttribDescription(AttributeType.FloatVec3);
            descriptions[2] = new VertexAttribDescription(AttributeType.FloatVec4, true, AttributeBaseType.UnsignedByte);
            descriptions[3] = new VertexAttribDescription(AttributeType.FloatVec2);
        }

        public override string ToString()
        {
            return string.Concat("(", Position.X.ToString(), ", ", Position.Y.ToString(), ", ", Position.Z.ToString(), ") (", Normal.X.ToString(), ", ", Normal.Y.ToString(), ", ", Normal.Z.ToString(), ") (", Color.R.ToString(), ", ", Color.G.ToString(), ", ", Color.B.ToString(), ", ", Color.A.ToString(), ") (", TexCoords.X.ToString(), ", ", TexCoords.Y.ToString(), ")");
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Normal, Color, TexCoords);
        }

        public bool Equals(VertexNormalColorTexture other)
        {
            return Position == other.Position
                && Normal == other.Normal
                && Color == other.Color
                && TexCoords == other.TexCoords;
        }

        public override bool Equals(object obj)
        {
            if (obj is VertexNormalColorTexture vertexNormalColorTexture)
                return Equals(vertexNormalColorTexture);
            return false;
        }
    }
}
