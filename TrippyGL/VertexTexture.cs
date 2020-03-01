using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with <see cref="Vector3"/> Position and <see cref="Vector2"/> TexCoords.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexTexture : IVertex
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

        public override string ToString()
        {
            return string.Concat("(", Position.X.ToString(), ", ", Position.Y.ToString(), ", ", Position.Z.ToString(), ") (", TexCoords.X.ToString(), ", ", TexCoords.Y.ToString(), ")");
        }

        public int AttribDescriptionCount => 2;

        public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
        {
            descriptions[0] = new VertexAttribDescription(ActiveAttribType.FloatVec3);
            descriptions[1] = new VertexAttribDescription(ActiveAttribType.FloatVec2);
        }
    }
}
