using System;
using OpenTK;

using OpenTK.Graphics.OpenGL4;

namespace TrippyTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            using (GameWindow w = new StructPaddingTest())
            {
                w.Run(60, 60);
            }
        }
    }
}
