using System;
using Silk.NET.OpenGL;

namespace TrippyTesting
{
    class Program
    {
        // WARNING
        // This "TrippyTesting" project is old AF and will be deleted pretty soon.
        // DO NOT USE THESE FOR TESTING THE LIBRARY! Use the projects inside
        // the TrippyTests folder instead.
        // Unless you're here to look at the beautiful TerrainMaker one, I'll
        // probably make a similar one for TrippyTests at some point.

        static void Main(string[] args)
        {
            new TextGame().Run();
        }

        public static void OnDebugMessage(DebugSource debugSource, DebugType debugType, int messageId, DebugSeverity debugSeverity, string message)
        {
            if (messageId != 131185 && messageId != 131186)
                Console.WriteLine(string.Concat("Debug message: source=", debugSource.ToString(), " type=", debugType.ToString(), " id=", messageId.ToString(), " severity=", debugSeverity.ToString(), " message=\"", message, "\""));
        }
    }
}
