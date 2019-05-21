using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    class ShaderProgram : IDisposable
    {
        private static int lastUsedProgram;

        /// <summary>The handle for the OpenGL Progrma object</summary>
        public readonly int Handle;

        private int vsHandle = -1;
        private int gsHandle = -1;
        private int fsHandle = -1;

        private bool isLinked = false;
        private bool areAttribsBound = false;

        public bool IsLinked { get { return isLinked; } }

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
            GL.AttachShader(Handle, fsHandle);
        }

        public void SpecifyVertexAttribs(VertexAttribSource[] attribSources, string[] attribNames)
        {
            if (attribSources == null)
                throw new ArgumentNullException("attribSources");

            if (attribNames == null)
                throw new ArgumentNullException("attribNames");

            if (attribSources.Length == 0)
                throw new ArgumentException("There must be at least one attribute source", "attribSources");

            if (attribSources.Length != attribNames.Length)
                throw new ArgumentException("The attribSources and attribNames arrays must have matching lengths");

            for (int i = 0; i < attribSources.Length; i++)
            {
                GL.GetActiveAttrib(Handle, i, out int size, out ActiveAttribType type);
                GL.BindAttribLocation(Handle, i, attribNames[i]);
            }
        }

        public void LinkProgram()
        {
            EnsureUnlinked();

            if (vsHandle == -1)
                throw new InvalidOperationException("Shader program must have a vertex shader");
            
            if (!areAttribsBound)
                throw new InvalidOperationException("The vertex attributes's indices have never been specified");

        }

        public void EnsureBound()
        {
            if (lastUsedProgram != Handle)
                Bind();
        }

        public void Bind()
        {
            lastUsedProgram = Handle;
            GL.UseProgram(Handle);
        }

        internal void EnsureUnlinked()
        {
            if (isLinked)
                throw new InvalidOperationException("The program has already been linked");
        }

        internal void EnsureLinked()
        {
            if (!isLinked)
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

        internal void jaja(ActiveAttribType type)
        {
            type = ActiveAttribType.None;

            type = ActiveAttribType.Float;
            type = ActiveAttribType.FloatVec2;
            type = ActiveAttribType.FloatVec3;
            type = ActiveAttribType.FloatVec4;

            type = ActiveAttribType.Double;
            type = ActiveAttribType.DoubleVec2;
            type = ActiveAttribType.DoubleVec3;
            type = ActiveAttribType.DoubleVec4;

            type = ActiveAttribType.Int;
            type = ActiveAttribType.IntVec2;
            type = ActiveAttribType.IntVec3;
            type = ActiveAttribType.IntVec4;

            type = ActiveAttribType.UnsignedInt;
            type = ActiveAttribType.UnsignedIntVec2;
            type = ActiveAttribType.UnsignedIntVec3;
            type = ActiveAttribType.UnsignedIntVec4;

            type = ActiveAttribType.FloatMat2;
            type = ActiveAttribType.FloatMat2x3;
            type = ActiveAttribType.FloatMat2x4;
            type = ActiveAttribType.FloatMat3;
            type = ActiveAttribType.FloatMat3x2;
            type = ActiveAttribType.FloatMat3x4;
            type = ActiveAttribType.FloatMat4;
            type = ActiveAttribType.FloatMat4x2;
            type = ActiveAttribType.FloatMat4x3;

            type = ActiveAttribType.DoubleMat2;
            type = ActiveAttribType.DoubleMat2x3;
            type = ActiveAttribType.DoubleMat2x4;
            type = ActiveAttribType.DoubleMat3;
            type = ActiveAttribType.DoubleMat3x2;
            type = ActiveAttribType.DoubleMat3x4;
            type = ActiveAttribType.DoubleMat4;
            type = ActiveAttribType.DoubleMat4x2;
            type = ActiveAttribType.DoubleMat4x3;
           
        }
    }
}
