using System;
using OpenTK;
using TrippyGL;

using OpenTK.Graphics.OpenGL4;

namespace TrippyTesting
{
    class Program
    {
        static void Main(string[] args)
        {

            using(GameWindow w = new Game3())
            {
                w.Run(60, 60);
            }
            return;

#pragma warning disable 0162
            while (true)
            {
                Console.WriteLine("Ready. ");
                GameWindow w = null;
                do
                {
                    switch (ReadInt())
                    {
                        case 0:
                            w = new Tests.SimpleTriangleTest();
                            break;
                        case 1:
                            w = new Tests.SimpleTextureTest();
                            break;
                        case 2:
                            w = new Tests.StructPaddingTest();
                            break;
                        case 3:
                            w = new Game2();
                            break;
                        case 4:
                            w = new Game();
                            break;
                        case 5:
                            w = new TransformGame();
                            break;
                        case -1:
                            return;

                    }
                } while (w == null);

                //using (GameWindow w = new Tests.SimpleTriangleTest()) //test 0
                //using(GameWindow w = new Tests.SimpleTextureTest()) //test 1
                //using (GameWindow w = new Tests.StructPaddingTest()) //test 2
                //using (GameWindow w = new Game2()) //test 3
                //using (GameWindow w = new Game()) //test 4
                //using (GameWindow w = new TransformGame()) //test 5
                using (w)
                {
                    w.Run(60, 60);
                }
                Console.WriteLine("[TEST ENDED]\n\n\n");
            }
        }

        static int ReadInt()
        {
            int res;
            while (!Int32.TryParse(Console.ReadLine(), out res)) ;
            return res;
        }
    }
}
