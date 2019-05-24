using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public class ShaderProgram : IDisposable
    {
        /// <summary>Gets the last program to be used</summary>
        public static ShaderProgram LastUsedProgram { get; private set; }

        /// <summary>The handle for the OpenGL Progrma object</summary>
        public readonly int Handle;

        private int vsHandle = -1;
        private int gsHandle = -1;
        private int fsHandle = -1;
        private bool areAttribsBound = false;

        public ShaderUniformList Uniforms { get; private set; }

        public bool IsLinked { get; private set; } = false;

        public bool IsCurrentlyInUse { get { return LastUsedProgram == this; } }

        public ShaderProgram()
        {
            Handle = GL.CreateProgram();
        }

        ~ShaderProgram()
        {
            if (TrippyLib.isLibActive)
                dispose();
        }

        public void AddVertexShader(string code)
        {
            EnsureUnlinked();

            if (vsHandle != -1)
                throw new InvalidOperationException("This ShaderProgram already has a vertex shader");

            vsHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vsHandle, code);
            GL.CompileShader(vsHandle);
            EnsureShaderCompiledProperly(vsHandle);

            GL.AttachShader(Handle, vsHandle);
        }

        public void AddFragmentShader(string code)
        {
            EnsureUnlinked();

            if (fsHandle != -1)
                throw new InvalidOperationException("This ShaderProgram already has a fragment shader");

            fsHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fsHandle, code);
            GL.CompileShader(fsHandle);
            EnsureShaderCompiledProperly(fsHandle);

            GL.AttachShader(Handle, fsHandle);
        }

        public void SpecifyVertexAttribs(VertexAttribDescription[] attribData, string[] attribNamesOrdered)
        {
            EnsureUnlinked();

            if (vsHandle == -1)
                throw new InvalidOperationException("You must add a vertex shader before specifying vertex attributes");

            if (areAttribsBound)
                throw new InvalidOperationException("Attributes have already been bound for this program");

            if (attribData == null)
                throw new ArgumentNullException("attribData");

            if (attribNamesOrdered == null)
                throw new ArgumentNullException("attribNames");

            if (attribData.Length == 0)
                throw new ArgumentException("There must be at least one attribute source", "attribData");

            if (attribData.Length != attribNamesOrdered.Length)
                throw new ArgumentException("The attribData and attribNames arrays must have matching lengths");

            int index = 0;
            for(int i=0; i<attribNamesOrdered.Length; i++)
            {
                GL.BindAttribLocation(Handle, index, attribNamesOrdered[i]);
                index += attribData[i].AttribIndicesUseCount;
            }

            areAttribsBound = true;
        }

        public void SpecifyVertexAttribs(VertexAttribSource[] attribSources, string[] attribNamesOrdered)
        {
            VertexAttribDescription[] attribData = new VertexAttribDescription[attribSources.Length];
            for (int i = 0; i < attribData.Length; i++)
                attribData[i] = attribSources[i].AttribDescription;
            SpecifyVertexAttribs(attribData, attribNamesOrdered);
        }

        public void LinkProgram()
        {
            EnsureUnlinked();

            if (vsHandle == -1)
                throw new InvalidOperationException("Shader program must have a vertex shader");
            
            if (!areAttribsBound)
                throw new InvalidOperationException("The vertex attributes's indices have never been specified");

            GL.LinkProgram(Handle);
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int status);
            if (status == (int)All.False)
                throw new ProgramLinkException(GL.GetProgramInfoLog(Handle));
            IsLinked = true;

            Uniforms = new ShaderUniformList(this);
        }

        public void EnsureInUse()
        {
            if (LastUsedProgram != this)
                Use();
        }

        public void Use()
        {
            LastUsedProgram = this;
            GL.UseProgram(Handle);
            Uniforms.EnsureSamplerUniformsSet();
        }

        internal void EnsureUnlinked()
        {
            if (IsLinked)
                throw new InvalidOperationException("The program has already been linked");
        }

        internal void EnsureLinked()
        {
            if (!IsLinked)
                throw new InvalidOperationException("The program must be linked first");
        }

        /// <summary>
        /// Disposes the shader program with no checks at all
        /// </summary>
        private void dispose()
        {
            GL.DeleteProgram(Handle);
        }

        /// <summary>
        /// Disposes the resources used by this ShaderProgram.
        /// The ShaderProgram cannot be used after disposing
        /// </summary>
        public void Dispose()
        {
            dispose();
            GC.SuppressFinalize(this);
        }

        private static void EnsureShaderCompiledProperly(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status == (int)All.False)
                throw new ShaderCompilationException(GL.GetShaderInfoLog(shader));
        }
    }
}
