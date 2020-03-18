using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with <see cref="Vector3"/> Position, <see cref="Vector3"/> Normal and <see cref="Color4b"/> Color.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexNormalColor : IVertex, IEquatable<VertexNormalColor>
    {
        /// <summary>The size of a <see cref="VertexNormalColor"/> measured in bytes.</summary>
        public const int SizeInBytes = (3 + 3 + 1) * 4;

        /// <summary>The vertex's position.</summary>
        public Vector3 Position;

        /// <summary>The vertex's normal.</summary>
        public Vector3 Normal;

        /// <summary>The vertex's color.</summary>
        public Color4b Color;

        /// <summary>
        /// Creates a <see cref="VertexNormalColor"/> with the specified position, normal and color.
        /// </summary>
        public VertexNormalColor(Vector3 position, Vector3 normal, Color4b color)
        {
            Position = position;
            Normal = normal;
            Color = color;
        }

        public static bool operator ==(VertexNormalColor left, VertexNormalColor right) => left.Equals(right);

        public static bool operator !=(VertexNormalColor left, VertexNormalColor right) => !left.Equals(right);

        public int AttribDescriptionCount => 3;

        public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
        {
            descriptions[0] = new VertexAttribDescription(AttributeType.FloatVec3);
            descriptions[1] = new VertexAttribDescription(AttributeType.FloatVec3);
            descriptions[2] = new VertexAttribDescription(AttributeType.FloatVec4, true, VertexAttribPointerType.UnsignedByte);
        }

        public override string ToString()
        {
            return string.Concat("(", Position.X.ToString(), ", ", Position.Y.ToString(), ", ", Position.Z.ToString(), ") (", Normal.X.ToString(), ", ", Normal.Y.ToString(), ", ", Normal.Z.ToString(), ") (", Color.R.ToString(), ", ", Color.G.ToString(), ", ", Color.B.ToString(), ", ", Color.A.ToString(), ")");
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Normal.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(VertexNormalColor other)
        {
            return Position == other.Position
                && Normal == other.Normal
                && Color == other.Color;
        }

        public override bool Equals(object obj)
        {
            if (obj is VertexNormalColor vertexNormalColor)
                return Equals(vertexNormalColor);
            return false;
        }
    }
}
