using Silk.NET.OpenGL;
using System;

namespace TrippyGL
{
    /// <summary>
    /// Encapsulates an OpenGL program object for using shaders.
    /// Shaders define how things are processed in the graphics card,
    /// from calculating vertex positions to choosing the color of each fragment.
    /// </summary>
    public sealed class ShaderProgram : GraphicsResource
    {
        // TODO: Change this to internal!!!
        /// <summary>The handle for the OpenGL Program object.</summary>
        public readonly uint Handle;

        private uint vsHandle = uint.MaxValue;
        private uint gsHandle = uint.MaxValue;
        private uint fsHandle = uint.MaxValue;

        // These stores the data of the attributes provided via SpecifyVertexAttribs() to compare that they actually exist and match after linking
        private string[] givenAttribNames = null;
        private VertexAttribDescription[] givenAttribDescriptions = null;

        /// <summary>Gets data about the geometry shader in this <see cref="ShaderProgram"/>, if there is one.</summary>
        public GeometryShaderData GeometryShader { get; private set; }

        /// <summary>The list of uniforms in this <see cref="ShaderProgram"/>.</summary>
        public ShaderUniformList Uniforms { get; private set; }

        /// <summary>The list of block uniforms in this <see cref="ShaderProgram"/>.</summary>
        public ShaderBlockUniformList BlockUniforms { get; private set; }

        /// <summary>The vertex attributes for this <see cref="ShaderProgram"/> queried from OpenGL at program linking.</summary>
        private ActiveVertexAttrib[] activeAttribs;

        /// <summary>Gets the input attributes on this program, once it's been linked.</summary>
        public ReadOnlySpan<ActiveVertexAttrib> ActiveAttribs => activeAttribs;

        /// <summary>Whether this <see cref="ShaderProgram"/> has been linked.</summary>
        public bool IsLinked { get; private set; } = false;

        /// <summary>Whether this <see cref="ShaderProgram"/> is the one currently in use.</summary>
        public bool IsCurrentlyInUse => GraphicsDevice.ShaderProgram == this;

        /// <summary>Whether this <see cref="ShaderProgram"/> has a vertex shader attached.</summary>
        public bool HasVertexShader => vsHandle != uint.MaxValue;

        /// <summary>Whether this <see cref="ShaderProgram"/> has a geometry shader attached.</summary>
        public bool HasGeometryShader => gsHandle != uint.MaxValue;

        /// <summary>Whether this <see cref="ShaderProgram"/> has a fragment shader attached.</summary>
        public bool HasFragmentShader => fsHandle != uint.MaxValue;

        /// <summary>
        /// Creates a <see cref="ShaderProgram"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        public ShaderProgram(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            Handle = GL.CreateProgram();
        }

        /// <summary>
        /// Adds a vertex shader to this <see cref="ShaderProgram"/>.
        /// </summary>
        /// <param name="code">The GLSL code for the vertex shader.</param>
        public void AddVertexShader(string code)
        {
            if (!TryAddVertexShader(code, out string log))
                throw new ShaderCompilationException(log);
        }

        /// <summary>
        /// Tries to add a vertex shader to the ShaderProgram. Returns whether it was successful.
        /// </summary>
        /// <param name="code">The vertex shader's code.</param>
        /// <param name="code">The GLSL code for the vertex shader.</param>
        public bool TryAddVertexShader(string code, out string shaderLog)
        {
            ValidateUnlinked();

            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("You must specify shader code", "code");

            if (vsHandle != uint.MaxValue)
                throw new InvalidOperationException("This ShaderProgram already has a vertex shader");

            uint vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, code);
            GL.CompileShader(vs);
            GL.GetShader(vs, ShaderParameterName.CompileStatus, out int status);
            shaderLog = GL.GetShaderInfoLog(vs);

            if (status == (int)GLEnum.False)
                return false;

            vsHandle = vs;
            GL.AttachShader(Handle, vsHandle);
            return true;
        }

        /// <summary>
        /// Tries to add a vertex shader to the ShaderProgram. Returns whether it was successful.
        /// </summary>
        /// <param name="code">The GLSL code for the vertex shader.</param>
        public bool TryAddVertexShader(string code)
        {
            ValidateUnlinked();

            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("You must specify shader code", "code");

            if (vsHandle != uint.MaxValue)
                throw new InvalidOperationException("This ShaderProgram already has a vertex shader");

            uint vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, code);
            GL.CompileShader(vs);
            GL.GetShader(vs, ShaderParameterName.CompileStatus, out int status);

            if (status == (int)GLEnum.False)
                return false;

            vsHandle = vs;
            GL.AttachShader(Handle, vsHandle);
            return true;
        }

        /// <summary>
        /// Adds a geometry shader to this ShaderProgram.
        /// </summary>
        /// <param name="code">The GLSL code for the geometry shader.</param>
        public void AddGeometryShader(string code)
        {
            if (!TryAddGeometryShader(code, out string log))
                throw new ShaderCompilationException(log);
        }

        /// <summary>
        /// Tries to add a geometry shader to the ShaderProgram. Returns whether it was successful.
        /// </summary>
        /// <param name="code">The GLSL code for the geometry shader.</param>
        /// <param name="shaderLog">The compilation log from the shader.</param>
        public bool TryAddGeometryShader(string code, out string shaderLog)
        {
            ValidateUnlinked();

            if (!GraphicsDevice.IsGeometryShaderAvailable)
                throw new PlatformNotSupportedException("Geometry shaders aren't supported on this system");

            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("You must specify shader code", "code");

            if (gsHandle != uint.MaxValue)
                throw new InvalidOperationException("This ShaderProgram already has a geometry shader");

            uint gs = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(gs, code);
            GL.CompileShader(gs);
            GL.GetShader(gs, ShaderParameterName.CompileStatus, out int status);
            shaderLog = GL.GetShaderInfoLog(gs);

            if (status == (int)GLEnum.False)
                return false;

            gsHandle = gs;
            GL.AttachShader(Handle, gsHandle);
            return true;
        }

        /// <summary>
        /// Tries to add a geometry shader to the ShaderProgram. Returns whether it was successful.
        /// </summary>
        /// <param name="code">The GLSL code for the geometry shader.</param>
        public bool TryAddGeometryShader(string code)
        {
            ValidateUnlinked();

            if (!GraphicsDevice.IsGeometryShaderAvailable)
                throw new PlatformNotSupportedException("Geometry shaders aren't supported on this system");

            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("You must specify shader code", "code");

            if (gsHandle != uint.MaxValue)
                throw new InvalidOperationException("This ShaderProgram already has a geometry shader");

            uint gs = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(gs, code);
            GL.CompileShader(gs);
            GL.GetShader(gs, ShaderParameterName.CompileStatus, out int status);

            if (status == (int)GLEnum.False)
                return false;

            gsHandle = gs;
            GL.AttachShader(Handle, gsHandle);
            return true;
        }

        /// <summary>
        /// Adds a fragment shader to this ShaderProgram.
        /// </summary>
        /// <param name="code">The GLSL code for the fragment shader.</param>
        public void AddFragmentShader(string code)
        {
            if (!TryAddFragmentShader(code, out string log))
                throw new ShaderCompilationException(log);
        }

        /// <summary>
        /// Tries to add a fragment shader to the ShaderProgram. Returns whether it was successful.
        /// </summary>
        /// <param name="code">The GLSL code for the fragment shader.</param>
        /// <param name="shaderLog">The compilation log from the shader.</param>
        public bool TryAddFragmentShader(string code, out string shaderLog)
        {
            ValidateUnlinked();

            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("You must specify shader code", "code");

            if (fsHandle != uint.MaxValue)
                throw new InvalidOperationException("This ShaderProgram already has a fragment shader");

            uint fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, code);
            GL.CompileShader(fs);
            GL.GetShader(fs, ShaderParameterName.CompileStatus, out int status);
            shaderLog = GL.GetShaderInfoLog(fs);

            if (status == (int)GLEnum.False)
                return false;

            fsHandle = fs;
            GL.AttachShader(Handle, fsHandle);
            return true;
        }

        /// <summary>
        /// Tries to add a fragment shader to the ShaderProgram. Returns whether it was successful.
        /// </summary>
        /// <param name="code">The GLSL code for the fragment shader.</param>
        public bool TryAddFragmentShader(string code)
        {
            ValidateUnlinked();

            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("You must specify shader code", "code");

            if (fsHandle != uint.MaxValue)
                throw new InvalidOperationException("This ShaderProgram already has a fragment shader");

            uint fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, code);
            GL.CompileShader(fs);
            GL.GetShader(fs, ShaderParameterName.CompileStatus, out int status);

            if (status == (int)GLEnum.False)
                return false;

            fsHandle = fs;
            GL.AttachShader(Handle, fsHandle);
            return true;
        }

        /// <summary>
        /// Specifies the input vertex attributes for this ShaderProgram declared on the vertex shader.
        /// </summary>
        /// <param name="attribData">The input attributes's descriptions, ordered by attribute index.</param>
        /// <param name="attribNames">The input attribute's names, ordered by attribute index.</param>
        public void SpecifyVertexAttribs(ReadOnlySpan<VertexAttribDescription> attribData, ReadOnlySpan<string> attribNames)
        {
            ValidateUnlinked();

            if (givenAttribNames != null)
                throw new InvalidOperationException("Attributes have already been specified for this program");

            attribData = TrippyUtils.CopyVertexAttribDescriptionsWithoutPaddingDescriptors(attribData);

            if (attribData.Length == 0)
                throw new ArgumentException("There must be at least one attribute source", "attribData");

            if (attribData.Length != attribNames.Length)
                throw new ArgumentException("The attribData and attribNames arrays must have matching lengths");

            uint index = 0;
            for (int i = 0; i < attribNames.Length; i++)
            {
                if (string.IsNullOrEmpty(attribNames[i]))
                    throw new ArgumentException("All names in the array must have be valid");

                GL.BindAttribLocation(Handle, index, attribNames[i]);
                index += attribData[i].AttribIndicesUseCount;
            }

            // The following stored arrays are copies of the ones provided by the user.
            // This way we ensure the user can't modify these
            givenAttribDescriptions = attribData.ToArray();
            givenAttribNames = attribNames.ToArray();
        }

        /// <summary>
        /// Specifies the input vertex attributes for this ShaderProgram declared on the vertex shader.
        /// </summary>
        /// <param name="attribSources">The input attribute's descriptions, ordered by attribute index.</param>
        /// <param name="attribNames">The input attribute's names, ordered by attribute index.</param>
        public void SpecifyVertexAttribs(ReadOnlySpan<VertexAttribSource> attribSources, ReadOnlySpan<string> attribNames)
        {
            Span<VertexAttribDescription> attribData = attribSources.Length > 256 ?
                new VertexAttribDescription[attribSources.Length] : stackalloc VertexAttribDescription[attribSources.Length];

            for (int i = 0; i < attribData.Length; i++)
                attribData[i] = attribSources[i].AttribDescription;
            SpecifyVertexAttribs(attribData, attribNames);
        }

        /// <summary>
        /// Specifies the input vertex attributes for this ShaderProgram declared on the vertex shader.
        /// </summary>
        /// <typeparam name="T">The type of vertex this ShaderProgram will use as input.</typeparam>
        /// <param name="attribNames">The input attribute's names, ordered by attribute index.</param>
        public void SpecifyVertexAttribs<T>(ReadOnlySpan<string> attribNames) where T : struct, IVertex
        {
            T t = default;
            int attribCount = t.AttribDescriptionCount;
            Span<VertexAttribDescription> attribDescriptions = attribCount > 256 ?
                new VertexAttribDescription[attribCount] : stackalloc VertexAttribDescription[attribCount];
            t.WriteAttribDescriptions(attribDescriptions);

            SpecifyVertexAttribs(attribDescriptions, attribNames);
        }

        /// <summary>
        /// Links the program.
        /// Once the program has been linked, it cannot be modifyed anymore, so make sure you add all your necessary shaders and specify vertex attributes.
        /// </summary>
        public void LinkProgram()
        {
            ValidateUnlinked();

            if (vsHandle == uint.MaxValue)
                throw new InvalidOperationException("Shader program must have a vertex shader before linking!");

            if (givenAttribNames == null)
                throw new InvalidOperationException("The vertex attributes's indices have never been specified");

            GL.LinkProgram(Handle);
            GL.GetProgram(Handle, ProgramPropertyARB.LinkStatus, out int status);
            if (status == (int)GLEnum.False)
                throw new ProgramLinkException(GL.GetProgramInfoLog(Handle));
            IsLinked = true;

            GL.DetachShader(Handle, vsHandle);
            GL.DeleteShader(vsHandle);

            if (fsHandle != uint.MaxValue)
            {
                GL.DetachShader(Handle, fsHandle);
                GL.DeleteShader(fsHandle);
            }

            if (gsHandle != uint.MaxValue)
            {
                GL.DetachShader(Handle, gsHandle);
                GL.DeleteShader(gsHandle);

                GeometryShader = new GeometryShaderData(GL, Handle);
            }

            activeAttribs = CreateActiveAttribsArray(this);
            BlockUniforms = new ShaderBlockUniformList(this);
            Uniforms = ShaderUniformList.CreateForProgram(this);

            if (!DoVertexAttributesMatch(activeAttribs, givenAttribDescriptions, givenAttribNames))
                throw new InvalidOperationException("The vertex attributes specified on SpecifyVertexAttribs() don't match the shader-defined attributes either in name or type");
            givenAttribNames = null;
            givenAttribDescriptions = null;
        }

        /// <summary>
        /// Ensures this program is the one currently in use for it's <see cref="GraphicsDevice"/>.
        /// </summary>
        internal void EnsureInUse()
        {
            GraphicsDevice.ShaderProgram = this;
        }

        /// <summary>
        /// Ensures all necessary states are set for a draw command to use this program, such as making
        /// sure sampler or block uniforms are properly set.<para/>
        /// This should always be called before a draw operation and assumes this
        /// <see cref="ShaderProgram"/> is the one currently in use.
        /// </summary>
        internal void EnsurePreDrawStates()
        {
            Uniforms?.EnsureSamplerUniformsSet();
            BlockUniforms.EnsureBufferBindingsSet();
        }

        /// <summary>
        /// Ensures this program is unlinked and throw a proper exception otherwise.
        /// </summary>
        internal void ValidateUnlinked()
        {
            if (IsLinked)
                throw new InvalidOperationException("The program has already been linked");
        }

        /// <summary>
        /// Ensures this program is linked and throw a proper exception otherwise.
        /// </summary>
        internal void ValidateLinked()
        {
            if (!IsLinked)
                throw new InvalidOperationException("The program must be linked first");
        }

        protected override void Dispose(bool isManualDispose)
        {
            if (isManualDispose && GraphicsDevice.ShaderProgram == this)
                GraphicsDevice.ShaderProgram = null;

            GL.DeleteProgram(Handle);
            base.Dispose(isManualDispose);
        }

        private static ActiveVertexAttrib[] CreateActiveAttribsArray(ShaderProgram program)
        {
            // We query the total amount of attributes we'll be reading from OpenGL
            program.GL.GetProgram(program.Handle, ProgramPropertyARB.ActiveAttributes, out int attribCount);

            // We'll be storing the attributes in this list and then turning it into an array, because we can't
            // know for sure how many attributes we'll have at the end, we just know it's be <= than attribCount
            ActiveVertexAttrib[] attribList = new ActiveVertexAttrib[attribCount];
            int attribListIndex = 0;

            // We query all the ShaderProgram's attributes one by one and add them to attribList
            for (uint i = 0; i < attribCount; i++)
            {
                ActiveVertexAttrib a = new ActiveVertexAttrib(program, i);
                if (a.Location >= 0)    // Sometimes other stuff shows up, such as gl_InstanceID with location -1.
                    attribList[attribListIndex++] = a;  // We should, of course, filter these out.
            }

            ActiveVertexAttrib[] attributes;
            if (attribListIndex == attribList.Length)
                attributes = attribList;
            else
            {
                attributes = new ActiveVertexAttrib[attribListIndex];
                Array.Copy(attribList, attributes, attribListIndex);
                attributes = attribList;
            }

            // The attributes don't always appear ordered by location, so let's order them now
            Array.Sort(attributes, (x, y) => x.Location.CompareTo(y.Location));
            return attributes;
        }

        /// <summary>
        /// Checks that the names given for some vertex attributes match the names actually found for the vertex attributes.
        /// </summary>
        /// <param name="attributes">The active <see cref="ActiveVertexAttrib"/>-s found by querying them from OpenGL, sorted by location.</param>
        /// <param name="providedDesc">The <see cref="VertexAttribDescription"/>-s provided by the user of the library.</param>
        /// <param name="providedNames">The names of the <see cref="VertexAttribDescription"/>-s provided by the user of the library.</param>
        private static bool DoVertexAttributesMatch(ReadOnlySpan<ActiveVertexAttrib> attributes, ReadOnlySpan<VertexAttribDescription> providedDesc, ReadOnlySpan<string> providedNames)
        {
            // While all of the attribute names are provided by the user, that doesn't mean all of them are in here.
            // The GLSL compiler may not make an attribute ACTIVE if, for example, it is never used.
            // So, if we see a provided name doesn't match, maybe it isn't active, so let's skip that name and check the next.
            // That said, both arrays are indexed in the same way. So if all attributes are active, we'll basically just
            // check one-by-one, index-by-index that the names on attributes[i] match providedNames[i]

            int nameIndex = 0;

            if (providedNames.Length == 0)
                return attributes.Length == 0;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (nameIndex == providedNames.Length)
                    return false;

                while (providedDesc[nameIndex].AttribType != attributes[i].AttribType || attributes[i].Name != providedNames[nameIndex])
                {
                    if (++nameIndex == providedNames.Length)
                        return false;
                }
                nameIndex++;
            }

            return true;
        }

        /// <summary>
        /// Stores data about a geometry shader.
        /// </summary>
        public readonly struct GeometryShaderData
        {
            /// <summary>The PrimitiveType the geometry shader takes as input.</summary>
            public readonly PrimitiveType GeometryInputType;

            /// <summary>The PrimitiveType the geometry shader takes as output.</summary>
            public readonly PrimitiveType GeometryOutputType;

            /// <summary>The amount of invocations the geometry shader will do.</summary>
            public readonly int GeometryShaderInvocations;

            /// <summary>The maximum amount of vertices the geometry shader can output.</summary>
            public readonly int GeometryVerticesOut;

            internal GeometryShaderData(GL gl, uint programHandle)
            {
                gl.GetProgram(programHandle, ProgramPropertyARB.GeometryInputType, out int tmp);
                GeometryInputType = (PrimitiveType)tmp;

                gl.GetProgram(programHandle, ProgramPropertyARB.GeometryOutputType, out tmp);
                GeometryOutputType = (PrimitiveType)tmp;

                gl.GetProgram(programHandle, GLEnum.GeometryShaderInvocations, out tmp);
                GeometryShaderInvocations = tmp;

                gl.GetProgram(programHandle, ProgramPropertyARB.GeometryVerticesOut, out tmp);
                GeometryVerticesOut = tmp;
            }
        }
    }
}
