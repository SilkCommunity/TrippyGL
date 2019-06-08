using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with only Vector3 Position
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPosition : IVertex
    {
        /// <summary>The size of each VertexPosition, measured in bytes</summary>
        public const int SizeInBytes = 3 * 4;

        /// <summary>The vertex's Position</summary>
        public Vector3 Position;

        /// <summary>
        /// Creates a VertexPosition with the specified position
        /// </summary>
        /// <param name="position">The vertex Position</param>
        public VertexPosition(Vector3 position)
        {
            this.Position = position;
        }

        public override string ToString()
        {
            return String.Concat("(", Position.X, ", ", Position.Y, ", ", Position.Z, ")");
        }

        /// <summary>
        /// Creates an array with the descriptions of all the vertex attributes present in a VertexPosition
        /// </summary>
        public VertexAttribDescription[] AttribDescriptions
        {
            get
            {
                return new VertexAttribDescription[]
                {
                    new VertexAttribDescription(ActiveAttribType.FloatVec3)
                };
            }
        }
    }
}
