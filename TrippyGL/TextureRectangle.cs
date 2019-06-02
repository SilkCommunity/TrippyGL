﻿using System;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public class TextureRectangle : Texture2D
    {
        public TextureRectangle(string file) : base(file, TextureTarget.TextureRectangle)
        {
            
        }
    }
}