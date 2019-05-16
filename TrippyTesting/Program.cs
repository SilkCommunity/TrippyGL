using System;
using OpenTK;

namespace TrippyTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            using(GameWindow w = new Game())
            {
                w.Run(60, 60);
            }
        }
    }
}
