using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with Vector3 Position and Vector2 TexCoords
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexTexture : IVertex
    {
        /// <summary>The size of each VertexTexture, measured in bytes</summary>
        public const int SizeInBytes = (3 + 2) * 4;

        /// <summary>The vertex's Position</summary>
        public Vector3 Position;

        /// <summary>The vertex's TexCoords</summary>
        public Vector2 TexCoords;

        /// <summary>
        /// Creates a VertexTexture with the specified position and texture coordinates
        /// </summary>
        /// <param name="position">The vertex Position</param>
        /// <param name="texCoords">The vertex TexCoords</param>
        public VertexTexture(Vector3 position, Vector2 texCoords)
        {
            this.Position = position;
            this.TexCoords = texCoords;
        }

        public override string ToString()
        {
            return String.Concat("(", Position.X.ToString(), ", ", Position.Y.ToString(), ", ", Position.Z.ToString(), ") (", TexCoords.X.ToString(), ", ", TexCoords.Y.ToString(), ")");
        }

        /// <summary>
        /// Creates an array with the descriptions of all the vertex attributes present in a VertexTexture
        /// </summary>
        public VertexAttribDescription[] AttribDescriptions
        {
            get
            {
                return new VertexAttribDescription[]
                {
                    new VertexAttribDescription(ActiveAttribType.FloatVec3),
                    new VertexAttribDescription(ActiveAttribType.FloatVec2)
                };
            }
        }
    }
}
