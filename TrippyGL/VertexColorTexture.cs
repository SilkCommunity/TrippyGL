using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexColorTexture : IVertex
    {
        public Vector3 Position;
        public Color4b Color;
        public Vector2 TexCoords;

        public VertexColorTexture(Vector3 position, Color4b color, Vector2 texCoords)
        {
            this.Position = position;
            this.Color = color;
            this.TexCoords = texCoords;
        }

        public VertexColorTexture(Vector3 position, Vector2 texCoords)
        {
            this.Position = position;
            this.Color = new Color4b(255, 255, 255, 255);
            this.TexCoords = texCoords;
        }

        public override string ToString()
        {
            return String.Concat("(", Position.X, ", ", Position.Y, ", ", Position.Z, ") (", Color.R, ", ", Color.G, ", ", Color.B, ", ", Color.A, ") (", TexCoords.X, ", ", TexCoords.Y, ")");
        }

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
