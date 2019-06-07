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
            /*TextureImageFormat[] arr = new TextureImageFormat[]
            {
                TextureImageFormat.Color4b,
                TextureImageFormat.Float,
                TextureImageFormat.Int,
                TextureImageFormat.UnsignedInt,
                TextureImageFormat.UVector2i,
                TextureImageFormat.UVector3i,
                TextureImageFormat.UVector4i,
                TextureImageFormat.Vector2,
                TextureImageFormat.Vector2i,
                TextureImageFormat.Vector3,
                TextureImageFormat.Vector3i,
                TextureImageFormat.Vector4,
                TextureImageFormat.Vector4i,
            };

            for(int i=0; i<arr.Length; i++)
            {
                PixelType type;
                PixelInternalFormat format;
                Texture.GetTextureFormatEnums(arr[i], out format, out type);
                Console.WriteLine(arr[i] + ": " + format + "; " + type);
            }
            Console.WriteLine("END");
            Console.ReadLine();*/

            using(GameWindow w = new Game3())
            {
                w.Run(60, 60);
            }
            return;

#pragma warning disable 0162 // disable "unreachable code" warning
            while (true)
            {
                GC.Collect();
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
                        case 6:
                            w = new Tests.Test3DBatcher();
                            break;
                        case 7:
                            w = new Game3();
                            break;
                        case 8:
                            w = new MultithreadTest1();
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
