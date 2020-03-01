using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with only a <see cref="Vector3"/> Position.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPosition : IVertex
    {
        /// <summary>The size of a <see cref="VertexPosition"/> measured in bytes.</summary>
        public const int SizeInBytes = 3 * 4;

        /// <summary>The vertex's position.</summary>
        public Vector3 Position;

        /// <summary>
        /// Creates a <see cref="VertexPosition"/> with the specified position.
        /// </summary>
        /// <param name="position">The vertex position.</param>
        public VertexPosition(Vector3 position)
        {
            Position = position;
        }

        public override string ToString()
        {
            return string.Concat("(", Position.X.ToString(), ", ", Position.Y.ToString(), ", ", Position.Z.ToString(), ")");
        }

        public int AttribDescriptionCount => 1;

        public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
        {
            descriptions[0] = new VertexAttribDescription(ActiveAttribType.FloatVec3);
        }
    }
}
