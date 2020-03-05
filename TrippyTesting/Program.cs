using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program started");

            using (GameWindow w = new Game4())
                w.Run();

            Console.WriteLine("Program ended");
        }

        public static void OnDebugMessage(DebugSource debugSource, DebugType debugType, int messageId, DebugSeverity debugSeverity, string message)
        {
            if (messageId != 131185 && messageId != 131186)
                Console.WriteLine(string.Concat("Debug message: source=", debugSource.ToString(), " type=", debugType.ToString(), " id=", messageId.ToString(), " severity=", debugSeverity.ToString(), " message=\"", message, "\""));
        }

        static int ReadInt()
        {
            int res;
            while (!int.TryParse(Console.ReadLine(), out res)) ;
            return res;
        }
    }
}
