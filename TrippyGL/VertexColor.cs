using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexColor : IVertex
    {
        public Vector3 Position;
        public Color4b Color;

        public VertexColor(Vector3 position, Color4b color)
        {
            this.Position = position;
            this.Color = color;
        }

        public VertexColor(Vector3 position)
        {
            this.Position = position;
            this.Color = new Color4b(255, 255, 255, 255);
        }

        public override string ToString()
        {
            return String.Concat("(", Position.X, ", ", Position.Y, ", ", Position.Z, "), (", Color.R, ", ", Color.G, ", ", Color.B, ", ", Color.A, ")");
        }

        public VertexAttribDescription[] AttribDescriptions
        {
            get
            {
                return new VertexAttribDescription[]
                {
                    new VertexAttribDescription(ActiveAttribType.FloatVec3),
                    new VertexAttribDescription(ActiveAttribType.FloatVec4, true, VertexAttribPointerType.UnsignedByte)
                };
            }
        }
    }
}
