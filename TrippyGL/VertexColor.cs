using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with <see cref="Vector3"/> Position and <see cref="Color4b"/> Color.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexColor : IVertex, IEquatable<VertexColor>
    {
        /// <summary>The size of a <see cref="VertexColor"/> measured in bytes.</summary>
        public const int SizeInBytes = (3 + 1) * 4;

        /// <summary>The vertex's position.</summary>
        public Vector3 Position;

        /// <summary>The vertex's color.</summary>
        public Color4b Color;

        /// <summary>
        /// Creates a <see cref="VertexColor"/> with the specified position and color.
        /// </summary>
        public VertexColor(Vector3 position, Color4b color)
        {
            Position = position;
            Color = color;
        }

        /// <summary>
        /// Creates a <see cref="VertexColor"/> with the specified position and white color.
        /// </summary>
        public VertexColor(Vector3 position)
        {
            Position = position;
            Color = new Color4b(255, 255, 255, 255);
        }

        public static bool operator ==(VertexColor left, VertexColor right) => left.Equals(right);
        public static bool operator !=(VertexColor left, VertexColor right) => !left.Equals(right);

        public int AttribDescriptionCount => 2;

        public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
        {
            descriptions[0] = new VertexAttribDescription(AttributeType.FloatVec3);
            descriptions[1] = new VertexAttribDescription(AttributeType.FloatVec4, true, AttributeBaseType.UnsignedByte);
        }

        public override string ToString()
        {
            return string.Concat("(", Position.X.ToString(), ", ", Position.Y.ToString(), ", ", Position.Z.ToString(), "), (", Color.R.ToString(), ", ", Color.G.ToString(), ", ", Color.B.ToString(), ", ", Color.A.ToString(), ")");
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Color);
        }

        public bool Equals(VertexColor other)
        {
            return Position == other.Position
                && Color == other.Color;
        }

        public override bool Equals(object obj)
        {
            if (obj is VertexColor vertexColor)
                return Equals(vertexColor);
            return false;
        }
    }
}
