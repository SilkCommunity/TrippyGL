using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyTesting
{
    [StructLayout(LayoutKind.Sequential)]
    struct ParticleVertex
    {
        public Vector3 Position;
        public Color4 Color;

        public ParticleVertex(Vector3 position, Color4 color)
        {
            this.Position = position;
            this.Color = color;
        }

        public override string ToString()
        {
            return String.Concat("(", Position.X, ", ", Position.Y, ", ", Position.Z, ") (", Color.R, ", ", Color.G, ", ", Color.B, ", ", Color.A, ")");
        }
    }
}
