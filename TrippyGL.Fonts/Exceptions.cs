using System;

namespace TrippyGL.Fonts
{
    public class FontLoadingException : Exception
    {
        public FontLoadingException() : base() { }

        public FontLoadingException(string message) : base(message) { }

        public FontLoadingException(string message, Exception innerException) : base(message, innerException) { }
    }
}
