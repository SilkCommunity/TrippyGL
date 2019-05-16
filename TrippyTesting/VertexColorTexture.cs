using System;
using OpenTK;
using OpenTK.Graphics;

namespace TrippyTesting
{
    struct VertexColorTexture
    {
        public const int SizeInBytes = 24;

        public Vector3 Position;
        public Color4b Color;
        public Vector2 TexCoords;

        public VertexColorTexture(Vector3 Position, Color4b Color, Vector2 TexCoords)
        {
            this.Position = Position;
            this.Color = Color;
            this.TexCoords = TexCoords;
        }
    }
}
