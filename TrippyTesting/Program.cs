using System;
using OpenTK;

using OpenTK.Graphics.OpenGL4;

namespace TrippyTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            using (GameWindow w = new Tests.SimpleTriangleTest()) //test 1
            //using (GameWindow w = new Game()) //test 2
            //using (GameWindow w = new Game2()) //test 3
            //using (GameWindow w = new Tests.StructPaddingTest()) //test 4
            //using (GameWindow w = new TransformGame()) //test 5
            {
                w.Run(60, 60);
            }
        }
    }
}
