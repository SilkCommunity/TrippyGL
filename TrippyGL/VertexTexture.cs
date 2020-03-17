using Silk.NET.OpenGL;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with <see cref="Vector3"/> Position and <see cref="Vector2"/> TexCoords.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexTexture : IVertex, IEquatable<VertexTexture>
    {
        /// <summary>The size of a <see cref="VertexTexture"/> measured in bytes.</summary>
        public const int SizeInBytes = (3 + 2) * 4;

        /// <summary>The vertex's position.</summary>
        public Vector3 Position;

        /// <summary>The vertex's texture coordinates.</summary>
        public Vector2 TexCoords;

        /// <summary>
        /// Creates a <see cref="VertexTexture"/> with the specified position and texture coordinates.
        /// </summary>
        public VertexTexture(Vector3 position, Vector2 texCoords)
        {
            Position = position;
            TexCoords = texCoords;
        }

        public static bool operator ==(VertexTexture left, VertexTexture right) => left.Equals(right);

        public static bool operator !=(VertexTexture left, VertexTexture right) => !left.Equals(right);

        public int AttribDescriptionCount => 2;

        public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
        {
            descriptions[0] = new VertexAttribDescription(AttributeType.FloatVec3);
            descriptions[1] = new VertexAttribDescription(AttributeType.FloatVec2);
        }

        public override string ToString()
        {
            return string.Concat("(", Position.X.ToString(), ", ", Position.Y.ToString(), ", ", Position.Z.ToString(), ") (", TexCoords.X.ToString(), ", ", TexCoords.Y.ToString(), ")");
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ TexCoords.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(VertexTexture other)
        {
            return Position == other.Position
                && TexCoords == other.TexCoords;
        }

        public override bool Equals(object obj)
        {
            if (obj is VertexTexture vertexTexture)
                return Equals(vertexTexture);
            return false;
        }
    }
}
