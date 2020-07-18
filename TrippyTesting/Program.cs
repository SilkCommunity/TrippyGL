using Silk.NET.OpenGL;
using System;
using TrippyGL;

namespace TrippyTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            new Tests.ObjFiles().Run();
        }

        public static void OnDebugMessage(DebugSource debugSource, DebugType debugType, int messageId, DebugSeverity debugSeverity, string message)
        {
            if (messageId != 131185 && messageId != 131186)
                Console.WriteLine(string.Concat("Debug message: source=", debugSource.ToString(), " type=", debugType.ToString(), " id=", messageId.ToString(), " severity=", debugSeverity.ToString(), " message=\"", message, "\""));
        }
    }
}
