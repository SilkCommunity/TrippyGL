using System;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public class TextureRectangle : Texture2D
    {

        //THIS IS A BAD IDEA. REDO THIS BUT IT NOT BEING A TEXTURE2D SUBCLASS.
        public TextureRectangle(GraphicsDevice graphicsDevice, string file, bool generateMipmaps = false) : base(graphicsDevice, file, generateMipmaps, TextureTarget.TextureRectangle)
        {
            
        }
    }
}
