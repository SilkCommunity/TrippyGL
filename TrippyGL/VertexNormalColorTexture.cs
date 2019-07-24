using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with Vector3 Position, Vector3 Normal, Color4b Color and Vector2 TexCoords
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexNormalColorTexture
    {
        /// <summary>The size of a VertexNormalColorTexture measured in bytes</summary>
        public const int SizeInBytes = (3 + 3 + 1 + 2) * 4;

        /// <summary>The vertex's position</summary>
        public Vector3 Position;

        /// <summary>The vertex's normal</summary>
        public Vector3 Normal;

        /// <summary>The vertex's color</summary>
        public Color4b Color;

        /// <summary>The vertex's texture coordinates</summary>
        public Vector2 TexCoords;

        /// <summary>
        /// Creates a VertexNormalColorTexture with the specified position, normal, color and texture coordinates
        /// </summary>
        /// <param name="position">The vertex position</param>
        /// <param name="normal">The vertex normal</param>
        /// <param name="color">The vertex color</param>
        /// <param name="texCoords">The vertex texture coordinates</param>
        public VertexNormalColorTexture(Vector3 position, Vector3 normal, Color4b color, Vector2 texCoords)
        {
            Position = position;
            Normal = normal;
            Color = color;
            TexCoords = texCoords;
        }

        public override string ToString()
        {
            return String.Concat("(", Position.X.ToString(), ", ", Position.Y.ToString(), ", ", Position.Z.ToString(), ") (", Normal.X.ToString(), ", ", Normal.Y.ToString(), ", ", Normal.Z.ToString(), ") (", Color.R.ToString(), ", ", Color.G.ToString(), ", ", Color.B.ToString(), ", ", Color.A.ToString(), ") (", TexCoords.X.ToString(), ", ", TexCoords.Y.ToString(), ")");
        }

        /// <summary>
        /// Creates an array with the descriptions of all the vertex attributes present in a VertexNormalColorTexture
        /// </summary>
        public VertexAttribDescription[] AttribDescriptions
        {
            get
            {
                return new VertexAttribDescription[]
                {
                    new VertexAttribDescription(ActiveAttribType.FloatVec3),
                    new VertexAttribDescription(ActiveAttribType.FloatVec3),
                    new VertexAttribDescription(ActiveAttribType.FloatVec4, true, VertexAttribPointerType.UnsignedByte),
                    new VertexAttribDescription(ActiveAttribType.FloatVec2)
                };
            }
        }
    }
}
