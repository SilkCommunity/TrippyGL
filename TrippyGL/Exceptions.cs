using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// An exception thrown when a shader didn't compile properly
    /// </summary>
    public class ShaderCompilationException : Exception
    {
        internal ShaderCompilationException(string infoLog) : base(String.Concat("Shader didn't compile properly: ", Environment.NewLine, infoLog)) { }
    }

    /// <summary>
    /// An exception thrown when a ShaderProgram didn't link properly
    /// </summary>
    public class ProgramLinkException : Exception
    {
        internal ProgramLinkException(string infoLog) : base(String.Concat("Program didn't link properly: ", Environment.NewLine, infoLog)) { }
    }

    /// <summary>
    /// An exception thrown when a Framebuffer fails to be created
    /// </summary>
    public class FramebufferCreationException : Exception
    {
        internal FramebufferCreationException(FramebufferErrorCode c) : base("The framebuffer couldn't be created: " + c)
        {

        }
    }

    /// <summary>
    /// An exception thrown when a blit operation between two framebuffers is invalid
    /// </summary>
    public class InvalidBlitException : Exception
    {
        internal InvalidBlitException(string message) : base(message)
        {

        }
    }
}
