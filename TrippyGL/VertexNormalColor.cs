using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with <see cref="Vector3"/> Position, <see cref="Vector3"/> Normal and <see cref="Color4b"/> Color.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexNormalColor : IVertex
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

        public override string ToString()
        {
            return string.Concat("(", Position.X.ToString(), ", ", Position.Y.ToString(), ", ", Position.Z.ToString(), ") (", Normal.X.ToString(), ", ", Normal.Y.ToString(), ", ", Normal.Z.ToString(), ") (", Color.R.ToString(), ", ", Color.G.ToString(), ", ", Color.B.ToString(), ", ", Color.A.ToString(), ")");
        }

        public int AttribDescriptionCount => 3;

        public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
        {
            descriptions[0] = new VertexAttribDescription(ActiveAttribType.FloatVec3);
            descriptions[1] = new VertexAttribDescription(ActiveAttribType.FloatVec3);
            descriptions[2] = new VertexAttribDescription(ActiveAttribType.FloatVec4, true, VertexAttribPointerType.UnsignedByte);
        }

        /// <summary>
        /// Creates an array with the descriptions of all the vertex attributes present in a <see cref="VertexNormalColor"/>.
        /// </summary>
        public VertexAttribDescription[] AttribDescriptions
        {
            get
            {
                return new VertexAttribDescription[]
                {
                };
            }
        }
    }
}
