using System;

namespace TrippyGL
{
    public struct Rectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public Rectangle(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public override string ToString()
        {
            return String.Concat("{X=", X.ToString(), ", Y=", Y.ToString(), ", Width=" + Width.ToString(), ", Height=", Height.ToString(), "}");
        }
    }
}
