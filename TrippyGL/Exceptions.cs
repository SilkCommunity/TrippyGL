using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrippyGL
{
    public class ShaderCompilationException : Exception
    {
        internal ShaderCompilationException(string infoLog) : base(String.Concat("Shader didn't compile properly: ", Environment.NewLine, infoLog)) { }
    }

    public class ProgramLinkException : Exception
    {
        internal ProgramLinkException(string infoLog) : base(String.Concat("Program didn't link properly: ", Environment.NewLine, infoLog)) { }
    }
}
