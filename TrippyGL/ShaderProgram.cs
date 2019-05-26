using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Encapsulates an OpenGL program object for using shaders.
    /// Shaders define how things are processed in the graphics card, from calculating vertex positions to choosing the color of each fragment
    /// </summary>
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

        public GeometryShaderData GeometryShader { get; private set; }

        /// <summary>The list of uniforms in this program</summary>
        public ShaderUniformList Uniforms { get; private set; }

        /// <summary>Whether this ShaderProgram has been linked</summary>
        public bool IsLinked { get; private set; } = false;

        /// <summary>Whether this ShaderProgram is the one currently in use</summary>
        public bool IsCurrentlyInUse { get { return LastUsedProgram == this; } }

        /// <summary>Whether this ShaderProgram has a geometry shader attached</summary>
        public bool HasGeometryShader { get { return gsHandle != -1; } }

        /// <summary>Whether this ShaderProgram has a fragment shader attached</summary>
        public bool HasFragmentShader { get { return fsHandle != -1; } }

        /// <summary>
        /// Creates a ShaderProgram
        /// </summary>
        public ShaderProgram()
        {
            Handle = GL.CreateProgram();
        }

        ~ShaderProgram()
        {
            if (TrippyLib.isLibActive)
                dispose();
        }

        /// <summary>
        /// Adds a vertex shader to this ShaderProgram
        /// </summary>
        /// <param name="code">The GLSL code for the vertex shader</param>
        public void AddVertexShader(string code)
        {
            EnsureUnlinked();

            if (String.IsNullOrEmpty(code))
                throw new ArgumentException("You must specify shader code", "code");

            if (vsHandle != -1)
                throw new InvalidOperationException("This ShaderProgram already has a vertex shader");

            vsHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vsHandle, code);
            GL.CompileShader(vsHandle);
            EnsureShaderCompiledProperly(vsHandle);

            GL.AttachShader(Handle, vsHandle);
        }

        /// <summary>
        /// Adds a geometry shader to this ShaderProgram
        /// </summary>
        /// <param name="code">The GLSL code for the geometry shader</param>
        public void AddGeometryShader(string code)
        {
            EnsureUnlinked();

            if (String.IsNullOrEmpty(code))
                throw new ArgumentException("You must specify shader code", "code");

            if (gsHandle != -1)
                throw new InvalidOperationException("This ShaderProgram already has a geometry shader");

            gsHandle = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(gsHandle, code);
            GL.CompileShader(gsHandle);
            EnsureShaderCompiledProperly(gsHandle);

            GL.AttachShader(Handle, gsHandle);
        }

        /// <summary>
        /// Adds a fragment shader to this ShaderProgram
        /// </summary>
        /// <param name="code">The GLSL code for the fragment shader</param>
        public void AddFragmentShader(string code)
        {
            EnsureUnlinked();

            if (String.IsNullOrEmpty(code))
                throw new ArgumentException("You must specify shader code", "code");

            if (fsHandle != -1)
                throw new InvalidOperationException("This ShaderProgram already has a fragment shader");

            fsHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fsHandle, code);
            GL.CompileShader(fsHandle);
            EnsureShaderCompiledProperly(fsHandle);

            GL.AttachShader(Handle, fsHandle);
        }

        /// <summary>
        /// Specifies the input vertex attributes for this ShaderProgram declared on the vertex shader
        /// </summary>
        /// <param name="attribData">The input attributes's descriptions, ordered by attribute index</param>
        /// <param name="attribNames">The input attribute's names, ordered by attribute index</param>
        public void SpecifyVertexAttribs(VertexAttribDescription[] attribData, string[] attribNames)
        {
            EnsureUnlinked();

            //if (vsHandle == -1) //this order is actually not a requirement on OpenGL...
            //    throw new InvalidOperationException("You must add a vertex shader before specifying vertex attributes");

            if (areAttribsBound)
                throw new InvalidOperationException("Attributes have already been bound for this program");

            if (attribData == null)
                throw new ArgumentNullException("attribData");

            if (attribNames == null)
                throw new ArgumentNullException("attribNames");

            if (attribData.Length == 0)
                throw new ArgumentException("There must be at least one attribute source", "attribData");

            if (attribData.Length != attribNames.Length)
                throw new ArgumentException("The attribData and attribNames arrays must have matching lengths");

            int index = 0;
            for(int i=0; i<attribNames.Length; i++)
            {
                if (String.IsNullOrEmpty(attribNames[i]))
                    throw new ArgumentException("All names in the array must have be valid");

                GL.BindAttribLocation(Handle, index, attribNames[i]);
                index += attribData[i].AttribIndicesUseCount;
            }

            areAttribsBound = true;
        }

        /// <summary>
        /// Specifies the input vertex attributes for this ShaderProgram declared on the vertex shader
        /// </summary>
        /// <param name="attribSources">The input attribute's descriptions, ordered by attribute index</param>
        /// <param name="attribNames">The input attribute's names, ordered by attribute index</param>
        public void SpecifyVertexAttribs(VertexAttribSource[] attribSources, string[] attribNames)
        {
            VertexAttribDescription[] attribData = new VertexAttribDescription[attribSources.Length];
            for (int i = 0; i < attribData.Length; i++)
                attribData[i] = attribSources[i].AttribDescription;
            SpecifyVertexAttribs(attribData, attribNames);
        }

        /// <summary>
        /// Specifies the input vertex attributes for this ShaderProgram declared on the vertex shader
        /// </summary>
        /// <typeparam name="T">The type of vertex this ShaderProgram will use as input</typeparam>
        /// <param name="attribNames">The input attribute's names, ordered by attribute index</param>
        public void SpecifyVertexAttribs<T>(string[] attribNames) where T : struct, IVertex
        {
            SpecifyVertexAttribs(new T().AttribDescriptions, attribNames);
        }

        /// <summary>
        /// Links the program.
        /// Once the program has been linked, it cannot be modifyed anymore, so make sure you add all your necessary shaders and specify vertex attributes
        /// </summary>
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

            GL.DetachShader(Handle, vsHandle);
            GL.DeleteShader(vsHandle);

            if(fsHandle != -1)
            {
                GL.DetachShader(Handle, fsHandle);
                GL.DeleteShader(fsHandle);
            }

            if(gsHandle != -1)
            {
                GL.DetachShader(Handle, gsHandle);
                GL.DeleteShader(gsHandle);

                this.GeometryShader = new GeometryShaderData(this.Handle);
            }

            Uniforms = new ShaderUniformList(this);
        }

        /// <summary>
        /// Ensures this ShaderProgram is the one currently in use
        /// </summary>
        public void EnsureInUse()
        {
            if (LastUsedProgram != this)
                Use();
        }

        /// <summary>
        /// Installs this program into the rendering pipeline. Prefer using EnsureInUse() to avoid unnecessary Use()-s
        /// </summary>
        public void Use()
        {
            LastUsedProgram = this;
            GL.UseProgram(Handle);
        }

        /// <summary>
        /// Make sure this program is unlinked and throw a proper exception otherwise
        /// </summary>
        internal void EnsureUnlinked()
        {
            if (IsLinked)
                throw new InvalidOperationException("The program has already been linked");
        }

        /// <summary>
        /// Make sure this program is linked and throw a proper exception otherwise
        /// </summary>
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

        /// <summary>
        /// Checks that the given shader has compiled properly and throw an appropiate exception otherwise
        /// </summary>
        /// <param name="shader"></param>
        private static void EnsureShaderCompiledProperly(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status == (int)All.False)
                throw new ShaderCompilationException(GL.GetShaderInfoLog(shader));
        }



        public class GeometryShaderData
        {
            public readonly PrimitiveType GeometryInputType;
            public readonly PrimitiveType GeometryOutputType;
            public readonly int GeometryShaderInvocations;
            public readonly int GeometryVerticesOut;

            internal GeometryShaderData(int program)
            {
                int i;

                GL.GetProgram(program, GetProgramParameterName.GeometryInputType, out i);
                GeometryInputType = (PrimitiveType)i;

                GL.GetProgram(program, GetProgramParameterName.GeometryOutputType, out i);
                GeometryOutputType = (PrimitiveType)i;

                GL.GetProgram(program, GetProgramParameterName.GeometryShaderInvocations, out i);
                GeometryShaderInvocations = i;

                GL.GetProgram(program, GetProgramParameterName.GeometryVerticesOut, out i);
                GeometryVerticesOut = i;
            }
        }
    }
}
