using System;
using Silk.NET.OpenGL;
using TrippyGL.Utils;

namespace TrippyGL
{
    /// <summary>
    /// Used to construct <see cref="ShaderProgram"/> instances with the desired parameters.
    /// </summary>
    public struct ShaderProgramBuilder : IEquatable<ShaderProgramBuilder>
    {
        /// <summary>The vertex shader code for the <see cref="ShaderProgram"/>.</summary>
        public string VertexShaderCode;
        /// <summary>Whether <see cref="VertexShaderCode"/> is not null or white spaces.</summary>
        public bool HasVertexShader => !string.IsNullOrWhiteSpace(VertexShaderCode);

        /// <summary>The geometry shader code for the <see cref="ShaderProgram"/>.</summary>
        public string GeometryShaderCode;
        /// <summary>Whether <see cref="GeometryShaderCode"/> is not null or white spaces.</summary>
        public bool HasGeometryShader => !string.IsNullOrWhiteSpace(GeometryShaderCode);

        /// <summary>The fragment shader code for the <see cref="ShaderProgram"/>.</summary>
        public string FragmentShaderCode;
        /// <summary>Whether <see cref="FragmentShaderCode"/> is not null or white spaces.</summary>
        public bool HasFragmentShader => !string.IsNullOrWhiteSpace(FragmentShaderCode);

        /// <summary>The specified attributes ordered by attribute index.</summary>
        private SpecifiedShaderAttrib[] specifiedAttribs;
        /// <summary>Whether vertex attributes have been specified to this <see cref="ShaderProgramBuilder"/>.</summary>
        public bool HasAttribsSpecified => specifiedAttribs != null;

        /// <summary>The vertex shader log of the last <see cref="ShaderProgram"/> this builder created.</summary>
        public string VertexShaderLog;
        /// <summary>The geometry shader log of the last <see cref="ShaderProgram"/> this builder created.</summary>
        public string GeometryShaderLog;
        /// <summary>The fragment shader log of the last <see cref="ShaderProgram"/> this builder created.</summary>
        public string FragmentShaderLog;
        /// <summary>The program log of the last <see cref="ShaderProgram"/> this builder created.</summary>
        public string ProgramLog;

        public static bool operator ==(ShaderProgramBuilder left, ShaderProgramBuilder right) => left.Equals(right);
        public static bool operator !=(ShaderProgramBuilder left, ShaderProgramBuilder right) => !left.Equals(right);

        /// <summary>
        /// Specifies the vertex attributes for the <see cref="ShaderProgram"/>.
        /// </summary>
        /// <param name="attributes">The vertex attributes in order of index.</param>
        public void SpecifyVertexAttribs(ReadOnlySpan<SpecifiedShaderAttrib> attributes)
        {
            specifiedAttribs = attributes.ToArray();
        }

        /// <summary>
        /// Specifies the vertex attributes for the <see cref="ShaderProgram"/>
        /// </summary>
        /// <param name="attribs">The vertex attributes in order of index.</param>
        /// <param name="attribNames">The names of the attributes orderd by attribute index.</param>
        public void SpecifyVertexAttribs(ReadOnlySpan<VertexAttribDescription> attribs, ReadOnlySpan<string> attribNames)
        {
            int length = 0;
            for (int i = 0; i < attribs.Length; i++)
                if (!attribs[i].IsPadding)
                    length++;

            specifiedAttribs = new SpecifiedShaderAttrib[length];
            int index = 0;
            for (int i = 0; i < attribs.Length; i++)
                if (!attribs[i].IsPadding)
                {
                    specifiedAttribs[index] = new SpecifiedShaderAttrib(index < attribNames.Length ? attribNames[index] : null, attribs[i].AttribType);
                    index++;
                }
        }

        /// <summary>
        /// Specifies the vertex attributes for the <see cref="ShaderProgram"/>
        /// </summary>
        /// <param name="attribs">The vertex attributes in order of index.</param>
        /// <param name="attribNames">The names of the attributes orderd by attribute index.</param>
        public void SpecifyVertexAttribs(ReadOnlySpan<VertexAttribSource> attribs, ReadOnlySpan<string> attribNames)
        {
            int length = 0;
            for (int i = 0; i < attribs.Length; i++)
                if (!attribs[i].IsPadding)
                    length++;

            specifiedAttribs = new SpecifiedShaderAttrib[length];
            int index = 0;
            for (int i = 0; i < attribs.Length; i++)
                if (!attribs[i].IsPadding)
                {
                    specifiedAttribs[index] = new SpecifiedShaderAttrib(attribNames[index], attribs[i].AttribDescription.AttribType);
                    index++;
                }
        }

        /// <summary>
        /// Specifies the vertex attributes for the <see cref="ShaderProgram"/>.
        /// </summary>
        /// <typeparam name="T">The type of vertex to use.</typeparam>
        /// <param name="attribNames">The names of the attributes orderd by attribute index.</param>
        public void SpecifyVertexAttribs<T>(ReadOnlySpan<string> attribNames) where T : unmanaged, IVertex
        {
            T t = default;
            int attribCount = t.AttribDescriptionCount;
            Span<VertexAttribDescription> attribDescriptions = attribCount > 32 ?
                new VertexAttribDescription[attribCount] : stackalloc VertexAttribDescription[attribCount];
            t.WriteAttribDescriptions(attribDescriptions);

            SpecifyVertexAttribs(attribDescriptions, attribNames);
        }

        /// <summary>
        /// Compiles shaders using the code from the different XShaderCode fields and creates a
        /// GL Program Object by linking them together. Then, queries the active attributes and
        /// ensures they match the provided ones in <see cref="specifiedAttribs"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the <see cref="ShaderProgram"/> will use.</param>
        /// <param name="activeAttribs">The active attributes found by querying the linked program.</param>
        /// <param name="getLogs">Whether to get compilation and linking logs from the shaders and program.</param>
        /// <returns>The handle of the newly created GL Program Object.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="ShaderCompilationException"/>
        /// <exception cref="ProgramLinkException"/>
        internal uint CreateInternal(GraphicsDevice graphicsDevice, out ActiveVertexAttrib[] activeAttribs,
            out bool hasVs, out bool hasGs, out bool hasFs, bool getLogs = false)
        {
            VertexShaderLog = null;
            FragmentShaderLog = null;
            GeometryShaderLog = null;
            ProgramLog = null;

            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            if (!HasVertexShader)
                throw new InvalidOperationException("A vertex shader must be specified");

            if (!HasAttribsSpecified)
                throw new InvalidOperationException("Vertex attributes must be specified");

            // glCreateShader returns non-zero values so we can set these as default no problem
            uint vsHandle = 0;
            uint gsHandle = 0;
            uint fsHandle = 0;

            uint programHandle = 0;
            bool success = false;

            // We encapsulate the logic in a try catch so whether there is an
            // exception or not, we glDeleteShader all the shader handles
            try
            {
                // We create the vertex shader, compile the code and check it's status
                vsHandle = graphicsDevice.GL.CreateShader(ShaderType.VertexShader);
                hasVs = true;

                graphicsDevice.GL.ShaderSource(vsHandle, VertexShaderCode);
                graphicsDevice.GL.CompileShader(vsHandle);
                graphicsDevice.GL.GetShader(vsHandle, ShaderParameterName.CompileStatus, out int compileStatus);
                if (getLogs || compileStatus == (int)GLEnum.False)
                {
                    VertexShaderLog = graphicsDevice.GL.GetShaderInfoLog(vsHandle);
                    if (compileStatus == (int)GLEnum.False)
                        throw new ShaderCompilationException(ShaderType.VertexShader, VertexShaderLog);
                }

                if (HasGeometryShader)
                {
                    // If we have code for it, we create the geometry shader, compile the code and check it's status
                    gsHandle = graphicsDevice.GL.CreateShader(ShaderType.GeometryShader);
                    hasGs = true;
                    graphicsDevice.GL.ShaderSource(gsHandle, GeometryShaderCode);
                    graphicsDevice.GL.CompileShader(gsHandle);
                    graphicsDevice.GL.GetShader(gsHandle, ShaderParameterName.CompileStatus, out compileStatus);
                    if (getLogs || compileStatus == (int)GLEnum.False)
                    {
                        GeometryShaderLog = graphicsDevice.GL.GetShaderInfoLog(gsHandle);
                        if (compileStatus == (int)GLEnum.False)
                            throw new ShaderCompilationException(ShaderType.GeometryShader, GeometryShaderLog);
                    }
                }
                else
                    hasGs = false;

                if (HasFragmentShader)
                {
                    // If we have code for it, we create the fragment shader, compile the code and check it's status
                    fsHandle = graphicsDevice.GL.CreateShader(ShaderType.FragmentShader);
                    hasFs = true;
                    graphicsDevice.GL.ShaderSource(fsHandle, FragmentShaderCode);
                    graphicsDevice.GL.CompileShader(fsHandle);
                    graphicsDevice.GL.GetShader(fsHandle, ShaderParameterName.CompileStatus, out compileStatus);
                    if (getLogs || compileStatus == (int)GLEnum.False)
                    {
                        FragmentShaderLog = graphicsDevice.GL.GetShaderInfoLog(fsHandle);
                        if (compileStatus == (int)GLEnum.False)
                            throw new ShaderCompilationException(ShaderType.FragmentShader, FragmentShaderLog);
                    }
                }
                else
                    hasFs = false;

                // We create the gl program object
                programHandle = graphicsDevice.GL.CreateProgram();

                // We loop through all the attributes declared for the shader and bind them all to the correct location
                uint attribIndex = 0;
                for (uint i = 0; i < specifiedAttribs.Length; i++)
                {
                    // Some attributes use more than 1 location, those ones we bind only once.
                    // A null or empty name means we skip that attrib because the shader doesn't use it.
                    // We still, though, have to advance attribIndex.
                    if (!string.IsNullOrWhiteSpace(specifiedAttribs[i].Name))
                        graphicsDevice.GL.BindAttribLocation(programHandle, attribIndex, specifiedAttribs[i].Name);

                    attribIndex += TrippyUtils.GetVertexAttribTypeIndexCount(specifiedAttribs[i].AttribType);
                }

                // We attach all the shaders we have to the program
                graphicsDevice.GL.AttachShader(programHandle, vsHandle);
                if (gsHandle != 0)
                    graphicsDevice.GL.AttachShader(programHandle, gsHandle);
                if (fsHandle != 0)
                    graphicsDevice.GL.AttachShader(programHandle, fsHandle);

                // We link the program and check it's status
                graphicsDevice.GL.LinkProgram(programHandle);
                graphicsDevice.GL.GetProgram(programHandle, ProgramPropertyARB.LinkStatus, out int linkStatus);
                if (getLogs || linkStatus == (int)GLEnum.False)
                {
                    ProgramLog = graphicsDevice.GL.GetProgramInfoLog(programHandle);
                    if (linkStatus == (int)GLEnum.False)
                        throw new ProgramLinkException(graphicsDevice.GL.GetProgramInfoLog(programHandle));
                }

                // We detach (and later delete) the shaders. These aren't actually detached
                // nor deleted until the program using them is done with them
                graphicsDevice.GL.DetachShader(programHandle, vsHandle);
                if (gsHandle != 0)
                    graphicsDevice.GL.DetachShader(programHandle, gsHandle);
                if (fsHandle != 0)
                    graphicsDevice.GL.DetachShader(programHandle, fsHandle);

                // We query the vertex attributes that were actually found on the compiled shader program
                activeAttribs = CreateActiveAttribArray(graphicsDevice, programHandle);

                // We ensure the queried vertex attributes match the user-provided ones
                if (!DoVertexAttributesMatch(activeAttribs, specifiedAttribs))
                    throw new InvalidOperationException("The specified vertex attributes don't match the ones declared in the shaders");

                // Done!
                success = true;
                return programHandle;
            }
            catch
            {
                // If something went wrong, we're not returning a ShaderProgram, but we might have
                // created the GL Program Object depending on what failed, so let's delete that.
                graphicsDevice.GL.DeleteProgram(programHandle);
                throw; // We re-throw the exception.
            }
            finally
            {
                // glDeleteShader calls get ignored if the shader handle is 0
                graphicsDevice.GL.DeleteShader(vsHandle);
                graphicsDevice.GL.DeleteShader(gsHandle);
                graphicsDevice.GL.DeleteShader(fsHandle);
                graphicsDevice.OnShaderCompiled(this, success);
            }
        }

        /// <summary>
        /// Creates a <see cref="ShaderProgram"/> using the current values on this <see cref="ShaderProgramBuilder"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the <see cref="ShaderProgram"/> will use.</param>
        /// <param name="getLogs">Whether to get compilation and linking logs from the shaders and program.</param>
        public ShaderProgram Create(GraphicsDevice graphicsDevice, bool getLogs = false)
        {
            uint programHandle = CreateInternal(graphicsDevice, out ActiveVertexAttrib[] activeAttribs,
                out bool hasVs, out bool hasGs, out bool hasFs, getLogs);
            return new ShaderProgram(graphicsDevice, programHandle, activeAttribs, hasVs, hasGs, hasFs);
        }

        /// <summary>
        /// Queries vertex attribute data from a compiled shader program and returns an
        /// array with all the resulting <see cref="ActiveVertexAttrib"/>-s.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> to use for gl calls.</param>
        /// <param name="programHandle">The gl handle of the shader program to query attribs for.</param>
        private static ActiveVertexAttrib[] CreateActiveAttribArray(GraphicsDevice graphicsDevice, uint programHandle)
        {
            // We query the total amount of attributes we'll be reading from OpenGL
            graphicsDevice.GL.GetProgram(programHandle, ProgramPropertyARB.ActiveAttributes, out int attribCount);

            // We'll be storing the attributes in this list and then turning it into an array, because we can't
            // know for sure how many attributes we'll have at the end, we just know it's be <= than attribCount
            ActiveVertexAttrib[] attribList = new ActiveVertexAttrib[attribCount];
            int attribListIndex = 0;

            // We query all the ShaderProgram's attributes one by one and add them to attribList
            for (uint i = 0; i < attribCount; i++)
            {
                ActiveVertexAttrib a = new ActiveVertexAttrib(graphicsDevice, programHandle, i);
                if (a.Location >= 0)    // Sometimes other stuff shows up, such as gl_InstanceID with location -1.
                    attribList[attribListIndex++] = a;  // We should, of course, filter these out.
            }

            // If, for any reason, we didn't write the whole attribList array, we trim it down before saving it in attributes
            ActiveVertexAttrib[] attributes;
            if (attribListIndex == attribList.Length)
                attributes = attribList;
            else
            {
                attributes = new ActiveVertexAttrib[attribListIndex];
                Array.Copy(attribList, attributes, attribListIndex);
                attributes = attribList;
            }

            // The attributes don't always appear ordered by location, so let's sort them now
            Array.Sort(attributes, (x, y) => x.Location.CompareTo(y.Location));
            return attributes;
        }

        /// <summary>
        /// Checks that the names given for some vertex attributes match the names actually found for the vertex attributes.
        /// </summary>
        /// <param name="activeAttribs">The active <see cref="ActiveVertexAttrib"/>-s found by querying them from OpenGL, sorted by location.</param>
        /// <param name="specifiedAttribs">The attributes that were provided by the user of the library.</param>
        private static bool DoVertexAttributesMatch(ReadOnlySpan<ActiveVertexAttrib> activeAttribs, ReadOnlySpan<SpecifiedShaderAttrib> specifiedAttribs)
        {
            // While all of the attribute names are provided by the user, that doesn't mean all of them are in here.
            // The GLSL compiler may not make an attribute ACTIVE if, for example, it is never used.
            // So, if we see a provided name doesn't match, maybe it isn't active, so let's skip that name and check the next.
            // That said, both arrays are indexed in the same way. So if all attributes are active, we'll basically just
            // check one-by-one, index-by-index that the names on attributes[i] match providedAttribs[i]

            int nameIndex = 0;

            if (specifiedAttribs.Length == 0)
                return activeAttribs.Length == 0;

            for (int i = 0; i < activeAttribs.Length; i++)
            {
                if (nameIndex == specifiedAttribs.Length)
                    return false;

                while (!specifiedAttribs[nameIndex].Matches(activeAttribs[i]))
                {
                    if (++nameIndex == specifiedAttribs.Length)
                        return false;
                }
                nameIndex++;
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = VertexShaderCode == null ? 0 : VertexShaderCode.GetHashCode(StringComparison.InvariantCulture);
                if (GeometryShaderCode != null)
                    hashCode = (hashCode * 397) ^ GeometryShaderCode.GetHashCode(StringComparison.InvariantCulture);
                if (FragmentShaderCode != null)
                    hashCode = (hashCode * 397) ^ FragmentShaderCode.GetHashCode(StringComparison.InvariantCulture);
                return hashCode;
            }
        }

        public bool Equals(ShaderProgramBuilder other)
        {
            return ReferenceEquals(VertexShaderCode, other.VertexShaderCode)
                && ReferenceEquals(GeometryShaderCode, other.GeometryShaderCode)
                && ReferenceEquals(FragmentShaderCode, other.FragmentShaderCode)
                && specifiedAttribs == other.specifiedAttribs
                && ReferenceEquals(VertexShaderLog, other.VertexShaderLog)
                && ReferenceEquals(GeometryShaderLog, other.GeometryShaderLog)
                && ReferenceEquals(FragmentShaderLog, other.FragmentShaderLog)
                && ReferenceEquals(ProgramLog, other.ProgramLog);
        }

        public override bool Equals(object obj)
        {
            if (obj is ShaderProgramBuilder shaderProgramBuilder)
                return Equals(shaderProgramBuilder);
            return false;
        }
    }

}
