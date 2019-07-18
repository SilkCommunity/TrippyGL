using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with Vector3 Position, Vector3 Normal and Vector2 TexCoords
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct VertexNormalTexture
    {
        /// <summary>The size of a VertexNormalTexture measured in bytes</summary>
        public const int SizeInBytes = (3 + 3 + 2) * 4;

        /// <summary>The vertex's position</summary>
        public Vector3 Position;

        /// <summary>The vertex's normal</summary>
        public Vector3 Normal;

        /// <summary>The vertex's texture coordinates</summary>
        public Vector2 TexCoords;

        /// <summary>
        /// Creates a VertexNormalTexture with the specified position, normal and texture coordinates
        /// </summary>
        /// <param name="position">The vertex position</param>
        /// <param name="normal">The vertex normal</param>
        /// <param name="texCoords">The vertex texture coordinates</param>
        public VertexNormalTexture(Vector3 position, Vector3 normal, Vector2 texCoords)
        {
            this.Position = position;
            this.Normal = normal;
            this.TexCoords = texCoords;
        }

        public override string ToString()
        {
            return String.Concat("(", Position.X.ToString(), ", ", Position.Y.ToString(), ", ", Position.Z.ToString(), ") (", Normal.X.ToString(), ", ", Normal.Y.ToString(), ", ", Normal.Z.ToString(), ") (", TexCoords.X.ToString(), ", ", TexCoords.Y.ToString(), ")");
        }

        /// <summary>
        /// Creates an array with the descriptions of all the vertex attributes present in a VertexNormalTexture
        /// </summary>
        public VertexAttribDescription[] AttribDescriptions
        {
            get
            {
                return new VertexAttribDescription[]
                {
                    new VertexAttribDescription(ActiveAttribType.FloatVec3),
                    new VertexAttribDescription(ActiveAttribType.FloatVec3),
                    new VertexAttribDescription(ActiveAttribType.FloatVec2)
                };
            }
        }
    }
}
