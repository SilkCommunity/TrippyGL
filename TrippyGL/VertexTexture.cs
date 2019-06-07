using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexTexture : IVertex
    {
        public const int SizeInBytes = (3 + 2) * 4;

        public Vector3 Position;
        public Vector2 TexCoords;

        public VertexTexture(Vector3 position, Vector2 texCoords)
        {
            this.Position = position;
            this.TexCoords = texCoords;
        }

        public override string ToString()
        {
            return String.Concat("(", Position.X, ", ", Position.Y, ", ", Position.Z, ") (", TexCoords.X, ", ", TexCoords.Y, ")");
        }

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
