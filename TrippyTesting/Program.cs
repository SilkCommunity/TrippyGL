using System;
using OpenTK;

using OpenTK.Graphics.OpenGL4;

namespace TrippyTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program started");

            using (GameWindow w = new Tests.TransformFeedback2())
                w.Run();

            Console.WriteLine("Program ended");
        }

        public static void OnDebugMessage(DebugSource debugSource, DebugType debugType, int messageId, DebugSeverity debugSeverity, string message)
        {
            if (messageId != 131185 && messageId != 131186)
                Console.WriteLine(String.Concat("Debug message: source=", debugSource.ToString(), " type=", debugType.ToString(), " id=", messageId.ToString(), " severity=", debugSeverity.ToString(), " message=\"", message, "\""));
        }

        static int ReadInt()
        {
            int res;
            while (!Int32.TryParse(Console.ReadLine(), out res)) ;
            return res;
        }
    }
}
