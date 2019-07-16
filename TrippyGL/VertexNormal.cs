using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with a Vector3 Position and Vector3 Normal
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct VertexNormal : IVertex
    {
        /// <summary>The size of a VertexNormal measured in bytes</summary>
        public const int SizeInBytes = (3 + 3) * 4;

        /// <summary>The vertex's position</summary>
        public Vector3 Position;

        /// <summary>The vertex's normal</summary>
        public Vector3 Normal;

        /// <summary>
        /// Creates a VertexNormal with the specified position and normal
        /// </summary>
        /// <param name="position">The vertex position</param>
        /// <param name="normal">The vertex normal</param>
        public VertexNormal(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }

        public override string ToString()
        {
            return String.Concat("(", Position.X.ToString(), ", ", Position.Y.ToString(), ", ", Position.Z.ToString(), ") (", Normal.X.ToString(), ", ", Normal.Y.ToString(), ", ", Normal.Z.ToString(), ")");
        }

        /// <summary>
        /// Creates an array with the descriptions of all the vertex attributes present in a VertexNormal
        /// </summary>
        public VertexAttribDescription[] AttribDescriptions
        {
            get
            {
                return new VertexAttribDescription[]
                {
                    new VertexAttribDescription(ActiveAttribType.FloatVec3),
                    new VertexAttribDescription(ActiveAttribType.FloatVec3)
                };
            }
        }
    }
}
