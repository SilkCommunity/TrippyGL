using System;

namespace TrippyGL
{
    /// <summary>
    /// An exception thrown when a shader didn't compile properly.
    /// </summary>
    public class ShaderCompilationException : Exception
    {
        public ShaderCompilationException() : base() { }

        public ShaderCompilationException(string infoLog) : this(infoLog, null) { }

        public ShaderCompilationException(string infoLog, Exception innerException)
            : base(string.Concat("Shader didn't compile properly: ", Environment.NewLine, infoLog), innerException) { }
    }

    /// <summary>
    /// An exception thrown when a <see cref="ShaderProgram"/> didn't link properly.
    /// </summary>
    public class ProgramLinkException : Exception
    {
        public ProgramLinkException() : base() { }

        public ProgramLinkException(string infoLog) : this(infoLog, null) { }

        public ProgramLinkException(string infoLog, Exception innerException)
            : base(string.Concat("Program didn't link properly: ", Environment.NewLine, infoLog), innerException) { }
    }

    /// <summary>
    /// An exception thrown when a <see cref="FramebufferObject"/> fails to be updated.
    /// </summary>
    public class FramebufferException : Exception
    {
        public FramebufferException() : base() { }

        public FramebufferException(string message) : base(message) { }

        public FramebufferException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// An exception thrown when a blit operation is invalid.
    /// </summary>
    public class InvalidBlitException : Exception
    {
        public InvalidBlitException() : base() { }

        public InvalidBlitException(string message) : base(message) { }

        public InvalidBlitException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// An exception thrown when a <see cref="FramebufferObject"/> can't attach a resource to an attachment point.
    /// </summary>
    public class InvalidFramebufferAttachmentException : Exception
    {
        public InvalidFramebufferAttachmentException() : base() { }

        public InvalidFramebufferAttachmentException(string message) : base(message) { }

        public InvalidFramebufferAttachmentException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// An exception thrown when a buffer copy operation fails.
    /// </summary>
    public class BufferCopyException : Exception
    {
        public BufferCopyException() : base() { }

        public BufferCopyException(string message) : base(message) { }

        public BufferCopyException(string message, Exception innerException) : base(message, innerException) { }
    }
}
