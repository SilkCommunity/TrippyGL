using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
