using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with <see cref="Vector3"/> Position, <see cref="Vector3"/> Normal and <see cref="Vector2"/> TexCoords.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexNormalTexture : IVertex
    {
        /// <summary>The size of a <see cref="VertexNormalTexture"/> measured in bytes.</summary>
        public const int SizeInBytes = (3 + 3 + 2) * 4;

        /// <summary>The vertex's position.</summary>
        public Vector3 Position;

        /// <summary>The vertex's normal.</summary>
        public Vector3 Normal;

        /// <summary>The vertex's texture coordinates.</summary>
        public Vector2 TexCoords;

        /// <summary>
        /// Creates a <see cref="VertexNormalTexture"/> with the specified position, normal and texture coordinates.
        /// </summary>
        public VertexNormalTexture(Vector3 position, Vector3 normal, Vector2 texCoords)
        {
            Position = position;
            Normal = normal;
            TexCoords = texCoords;
        }

        public override string ToString()
        {
            return string.Concat("(", Position.X.ToString(), ", ", Position.Y.ToString(), ", ", Position.Z.ToString(), ") (", Normal.X.ToString(), ", ", Normal.Y.ToString(), ", ", Normal.Z.ToString(), ") (", TexCoords.X.ToString(), ", ", TexCoords.Y.ToString(), ")");
        }

        public int AttribDescriptionCount => 3;

        public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
        {
            descriptions[0] = new VertexAttribDescription(ActiveAttribType.FloatVec3);
            descriptions[1] = new VertexAttribDescription(ActiveAttribType.FloatVec3);
            descriptions[2] = new VertexAttribDescription(ActiveAttribType.FloatVec2);
        }
    }
}
