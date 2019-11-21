using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with Vector3 Position, Color4b Color and Vector2 TexCoords.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexColorTexture : IVertex
    {
        /// <summary>The size of a VertexColorTexture measured in bytes.</summary>
        public const int SizeInBytes = (3 + 1 + 2) * 4;

        /// <summary>The vertex's position.</summary>
        public Vector3 Position;

        /// <summary>The vertex's color.</summary>
        public Color4b Color;

        /// <summary>The vertex's texture coordinates.</summary>
        public Vector2 TexCoords;

        /// <summary>
        /// Creates a VertexColorTexture with the specified position, color and texture coordinates.
        /// </summary>
        /// <param name="position">The vertex position.</param>
        /// <param name="color">The vertex color.</param>
        /// <param name="texCoords">The vertex texture coordinates.</param>
        public VertexColorTexture(Vector3 position, Color4b color, Vector2 texCoords)
        {
            Position = position;
            Color = color;
            TexCoords = texCoords;
        }

        /// <summary>
        /// Creates a VertexColorTexture with the specified position and texture coordinates, and white color.
        /// </summary>
        /// <param name="position">The vertex position.</param>
        /// <param name="texCoords">The vertex texture coordinates.</param>
        public VertexColorTexture(Vector3 position, Vector2 texCoords)
        {
            Position = position;
            Color = new Color4b(255, 255, 255, 255);
            TexCoords = texCoords;
        }

        public override string ToString()
        {
            return string.Concat("(", Position.X.ToString(), ", ", Position.Y.ToString(), ", ", Position.Z.ToString(), ") (", Color.R.ToString(), ", ", Color.G.ToString(), ", ", Color.B.ToString(), ", ", Color.A.ToString(), ") (", TexCoords.X.ToString(), ", ", TexCoords.Y.ToString(), ")");
        }

        /// <summary>
        /// Creates an array with the descriptions of all the vertex attributes present in a VertexColorTexture.
        /// </summary>
        public VertexAttribDescription[] AttribDescriptions
        {
            get
            {
                return new VertexAttribDescription[]
                {
                    new VertexAttribDescription(ActiveAttribType.FloatVec3),
                    new VertexAttribDescription(ActiveAttribType.FloatVec4, true, VertexAttribPointerType.UnsignedByte),
                    new VertexAttribDescription(ActiveAttribType.FloatVec2)
                };
            }
        }
    }
}
