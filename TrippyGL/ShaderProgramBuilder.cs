using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Used to create <see cref="ShaderProgram"/> instances.
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
            ValidateSpecifiedAttribs();
        }

        /// <summary>
        /// Specifies the vertex attributes for the <see cref="ShaderProgram"/>
        /// </summary>
        /// <param name="attribs">The vertex attributes in order of index.</param>
        /// <param name="attribNames">The names of the attributes in the same order as previously.</param>
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
                    specifiedAttribs[index] = new SpecifiedShaderAttrib(attribNames[index], attribs[i].AttribType);
                    index++;
                }
            ValidateSpecifiedAttribs();
        }

        /// <summary>
        /// Specifies the vertex attributes for the <see cref="ShaderProgram"/>
        /// </summary>
        /// <param name="attribs">The vertex attributes in order of index.</param>
        /// <param name="attribNames">The names of the attributes in the same order as previously.</param>
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
            ValidateSpecifiedAttribs();
        }

        /// <summary>
        /// Specifies the vertex attributes for the <see cref="ShaderProgram"/>.
        /// </summary>
        /// <typeparam name="T">The type of vertex to use.</typeparam>
        /// <param name="attribNames">The names of the attributes in order of index.</param>
        public void SpecifyVertexAttribs<T>(ReadOnlySpan<string> attribNames) where T : unmanaged, IVertex
        {
            T t = default;
            int attribCount = t.AttribDescriptionCount;
            Span<VertexAttribDescription> attribDescriptions = attribCount > 256 ?
                new VertexAttribDescription[attribCount] : stackalloc VertexAttribDescription[attribCount];
            t.WriteAttribDescriptions(attribDescriptions);

            SpecifyVertexAttribs(attribDescriptions, attribNames);
        }

        /// <summary>
        /// Checks whether <see cref="specifiedAttribs"/> has valid attributes and throws an exception otherwise.
        /// </summary>
        private void ValidateSpecifiedAttribs()
        {
            for (int i = 0; i < specifiedAttribs.Length; i++)
                if (string.IsNullOrWhiteSpace(specifiedAttribs[i].Name))
                    throw new ArgumentException("All shader attributes must have a valid name");
        }

        /// <summary>
        /// Creates a <see cref="ShaderProgram"/> using the current values this <see cref="ShaderProgramBuilder"/> has.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the <see cref="ShaderProgram"/> will use.</param>
        /// <param name="getLogs">Whether to get compilation and linking logs for the shaders and program.</param>
        public ShaderProgram Create(GraphicsDevice graphicsDevice, bool getLogs = false)
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

            // We encapsulate the logic in a try catch so whether there is an
            // exception or not, we glDeleteShader all the shader handles
            try
            {
                // We create the vertex shader, compile the code and check it's status
                vsHandle = graphicsDevice.GL.CreateShader(ShaderType.VertexShader);
                graphicsDevice.GL.ShaderSource(vsHandle, VertexShaderCode);
                graphicsDevice.GL.CompileShader(vsHandle);
                graphicsDevice.GL.GetShader(vsHandle, ShaderParameterName.CompileStatus, out int compileStatus);
                if (compileStatus == (int)GLEnum.False)
                    throw new ShaderCompilationException(graphicsDevice.GL.GetShaderInfoLog(vsHandle));
                if (getLogs)
                    VertexShaderLog = graphicsDevice.GL.GetShaderInfoLog(vsHandle);

                if (HasGeometryShader)
                {
                    // If we have code for it, we create the geometry shader, compile the code and check it's status
                    gsHandle = graphicsDevice.GL.CreateShader(ShaderType.GeometryShader);
                    graphicsDevice.GL.ShaderSource(gsHandle, GeometryShaderCode);
                    graphicsDevice.GL.CompileShader(gsHandle);
                    graphicsDevice.GL.GetShader(gsHandle, ShaderParameterName.CompileStatus, out compileStatus);
                    if (compileStatus == (int)GLEnum.False)
                        throw new ShaderCompilationException(graphicsDevice.GL.GetShaderInfoLog(gsHandle));
                    if (getLogs)
                        GeometryShaderLog = graphicsDevice.GL.GetShaderInfoLog(gsHandle);
                }

                if (HasFragmentShader)
                {
                    // If we have code for it, we create the fragment shader, compile the code and check it's status
                    fsHandle = graphicsDevice.GL.CreateShader(ShaderType.FragmentShader);
                    graphicsDevice.GL.ShaderSource(fsHandle, FragmentShaderCode);
                    graphicsDevice.GL.CompileShader(fsHandle);
                    graphicsDevice.GL.GetShader(fsHandle, ShaderParameterName.CompileStatus, out compileStatus);
                    if (compileStatus == (int)GLEnum.False)
                        throw new ShaderCompilationException(graphicsDevice.GL.GetShaderInfoLog(fsHandle));
                    if (getLogs)
                        FragmentShaderLog = graphicsDevice.GL.GetShaderInfoLog(fsHandle);
                }

                // We create the gl program object
                uint programHandle = graphicsDevice.GL.CreateProgram();

                // We loop through all the attributes declared for the shader and bind them all to the correct location
                uint attribIndex = 0;
                for (uint i = 0; i < specifiedAttribs.Length; i++)
                {
                    // Some attributes use more than 1 location, those ones we bind only once
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
                if (linkStatus == (int)GLEnum.False)
                {
                    graphicsDevice.GL.DeleteProgram(programHandle);
                    throw new ProgramLinkException(graphicsDevice.GL.GetProgramInfoLog(programHandle));
                }
                if (getLogs)
                    ProgramLog = graphicsDevice.GL.GetProgramInfoLog(programHandle);

                // We detach (and later delete) the shaders. These aren't actually detached
                // nor deleted until the program using them is done with them
                graphicsDevice.GL.DetachShader(programHandle, vsHandle);
                if (gsHandle != 0)
                    graphicsDevice.GL.DetachShader(programHandle, gsHandle);
                if (fsHandle != 0)
                    graphicsDevice.GL.DetachShader(programHandle, fsHandle);

                // We query the vertex attributes that were actually found on the compiled shader program
                ActiveVertexAttrib[] activeAttribs = CreateActiveAttribArray(graphicsDevice, programHandle);

                // We ensure the queried vertex attributes match the user-provided ones
                if (!DoVertexAttributesMatch(activeAttribs, specifiedAttribs))
                {
                    graphicsDevice.GL.DeleteProgram(programHandle);
                    throw new InvalidOperationException("The specified vertex attributes don't match the ones declared in the shaders");
                }

                // Success!
                return new ShaderProgram(graphicsDevice, programHandle, activeAttribs);
            }
            finally
            {
                // glDeleteShader calls get ignored if the shader handle is 0
                graphicsDevice.GL.DeleteShader(vsHandle);
                graphicsDevice.GL.DeleteShader(gsHandle);
                graphicsDevice.GL.DeleteShader(fsHandle);
            }
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

        public bool Equals(ShaderProgramBuilder shaderProgramBuilder)
        {
            return ReferenceEquals(VertexShaderCode, shaderProgramBuilder.VertexShaderCode)
                && ReferenceEquals(GeometryShaderCode, shaderProgramBuilder.GeometryShaderCode)
                && ReferenceEquals(FragmentShaderCode, shaderProgramBuilder.FragmentShaderCode)
                && specifiedAttribs == shaderProgramBuilder.specifiedAttribs
                && ReferenceEquals(VertexShaderLog, shaderProgramBuilder.VertexShaderLog)
                && ReferenceEquals(GeometryShaderLog, shaderProgramBuilder.GeometryShaderLog)
                && ReferenceEquals(FragmentShaderLog, shaderProgramBuilder.FragmentShaderLog)
                && ReferenceEquals(ProgramLog, shaderProgramBuilder.ProgramLog);
        }

        public override bool Equals(object obj)
        {
            if (obj is ShaderProgramBuilder shaderProgramBuilder)
                return Equals(shaderProgramBuilder);
            return false;
        }
    }

}
