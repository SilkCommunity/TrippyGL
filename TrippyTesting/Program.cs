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
            /*using(GameWindow w = new Tests.FramebufferTest2())
            {
                w.Run(60, 60);
            }
            return;*/

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
                        case 9:
                            w = new Tests.FramebufferTest1();
                            break;
                        case 10:
                            w = new Tests.FramebufferTest2();
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

        public static void OnDebugMessage(DebugSource src, DebugType type, int id, DebugSeverity sev, int length, IntPtr msg, IntPtr param)
        {
            Console.WriteLine("Debug message: src=" + src + " type=" + type + " id=" + id + " sev=" + sev + " msg=" + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(msg));
        }

        static int ReadInt()
        {
            int res;
            while (!Int32.TryParse(Console.ReadLine(), out res)) ;
            return res;
        }
    }
}
