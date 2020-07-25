using Silk.NET.OpenGL;
using System;

namespace TrippyTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            new Tests.SimpleShader().Run();
        }

        public static void OnDebugMessage(DebugSource debugSource, DebugType debugType, int messageId, DebugSeverity debugSeverity, string message)
        {
            if (messageId != 131185 && messageId != 131186)
                Console.WriteLine(string.Concat("Debug message: source=", debugSource.ToString(), " type=", debugType.ToString(), " id=", messageId.ToString(), " severity=", debugSeverity.ToString(), " message=\"", message, "\""));
        }
    }
}
