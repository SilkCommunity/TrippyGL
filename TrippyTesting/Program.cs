using System;
using OpenTK;

using OpenTK.Graphics.OpenGL4;

namespace TrippyTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            //xd();
            //Console.ReadLine();

            using (GameWindow w = new StructPaddingTest())
            {
                w.Run(60, 60);
            }
        }

        static unsafe void xd()
        {
            WeirdAssVertex w = new WeirdAssVertex();
            byte* addr = (byte*)&w;
            Console.WriteLine("Size:      {0}", sizeof(WeirdAssVertex));
            Console.WriteLine("x  Offset: {0}", (byte*)&w.x - addr);
            Console.WriteLine("y  Offset: {0}", (byte*)&w.y - addr);
            Console.WriteLine("z  Offset: {0}", (byte*)&w.z - addr);
            Console.WriteLine("x2 Offset: {0}", (byte*)&w.x2 - addr);
            Console.WriteLine("y2 Offset: {0}", (byte*)&w.y2 - addr);
            Console.WriteLine("z2 Offset: {0}", (byte*)&w.z2 - addr);
            Console.WriteLine("w  Offset: {0}", (byte*)&w.w - addr);
            Console.WriteLine("cx Offset: {0}", (byte*)&w.cx - addr);
            Console.WriteLine("cy Offset: {0}", (byte*)&w.cy - addr);
        }
    }
}
