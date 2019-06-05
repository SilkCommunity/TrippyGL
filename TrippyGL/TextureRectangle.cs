using System;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public class TextureRectangle : Texture2D
    {
        public TextureRectangle(GraphicsDevice graphicsDevice, string file, bool generateMipmaps = false) : base(graphicsDevice, file, generateMipmaps, TextureTarget.TextureRectangle)
        {
            
        }
    }
}
