using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPosition : IVertex
    {
        public const int SizeInBytes = 3 * 4;

        public Vector3 Position;

        public override string ToString()
        {
            return String.Concat("(", Position.X, ", ", Position.Y, ", ", Position.Z, ")");
        }
        public VertexPosition(Vector3 position)
        {
            this.Position = position;
        }

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
