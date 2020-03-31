using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    public struct ShaderProgramBuilder
    {
        public string VertexShaderCode;
        public bool HasVertexShader => !string.IsNullOrWhiteSpace(VertexShaderCode);

        public string GeometryShaderCode;
        public bool HasGeometryShader => !string.IsNullOrWhiteSpace(GeometryShaderCode);

        public string FragmentShaderCode;
        public bool HasFragmentShader => !string.IsNullOrWhiteSpace(FragmentShaderCode);

        private SpecifiedShaderAttrib[] specifiedAttribs;
        public bool HasAttribsSpecified => specifiedAttribs != null;

        public string VertexShaderLog;
        public string GeometryShaderLog;
        public string FragmentShaderLog;
        public string ProgramLog;

        public void SpecifyVertexAttribs(ReadOnlySpan<SpecifiedShaderAttrib> attributes)
        {
            specifiedAttribs = attributes.ToArray();
            ValidateSpecifiedAttribs();
        }

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
                    specifiedAttribs[index] = new SpecifiedShaderAttrib(attribNames[index], attribs[i].AttribType, attribs[i].Size);
                    index++;
                }
            ValidateSpecifiedAttribs();
        }

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
                    specifiedAttribs[index] = new SpecifiedShaderAttrib(attribNames[index], attribs[i].AttribDescription.AttribType, attribs[i].AttribDescription.Size);
                    index++;
                }
            ValidateSpecifiedAttribs();
        }

        public void SpecifyVertexAttribs<T>(ReadOnlySpan<string> attribNames) where T : unmanaged, IVertex
        {
            T t = default;
            int attribCount = t.AttribDescriptionCount;
            Span<VertexAttribDescription> attribDescriptions = attribCount > 256 ?
                new VertexAttribDescription[attribCount] : stackalloc VertexAttribDescription[attribCount];
            t.WriteAttribDescriptions(attribDescriptions);

            SpecifyVertexAttribs(attribDescriptions, attribNames);
        }

        private void ValidateSpecifiedAttribs()
        {
            for (int i = 0; i < specifiedAttribs.Length; i++)
                if (string.IsNullOrWhiteSpace(specifiedAttribs[i].Name))
                    throw new ArgumentException("All shader attributes must have a valid name");
        }

        public ShaderProgram Create(GraphicsDevice graphicsDevice, bool getLogs = false)
        {
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

            try
            {
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
                    fsHandle = graphicsDevice.GL.CreateShader(ShaderType.FragmentShader);
                    graphicsDevice.GL.ShaderSource(fsHandle, FragmentShaderCode);
                    graphicsDevice.GL.CompileShader(fsHandle);
                    graphicsDevice.GL.GetShader(fsHandle, ShaderParameterName.CompileStatus, out compileStatus);
                    if (compileStatus == (int)GLEnum.False)
                        throw new ShaderCompilationException(graphicsDevice.GL.GetShaderInfoLog(fsHandle));
                    if (getLogs)
                        FragmentShaderLog = graphicsDevice.GL.GetShaderInfoLog(fsHandle);
                }

                uint programHandle = graphicsDevice.GL.CreateProgram();

                uint attribIndex = 0;
                for (uint i = 0; i < specifiedAttribs.Length; i++)
                {
                    graphicsDevice.GL.BindAttribLocation(programHandle, attribIndex, specifiedAttribs[i].Name);
                    attribIndex += TrippyUtils.GetVertexAttribTypeIndexCount(specifiedAttribs[i].AttribType);
                }

                graphicsDevice.GL.AttachShader(programHandle, vsHandle);
                if (gsHandle != 0)
                    graphicsDevice.GL.AttachShader(programHandle, gsHandle);
                if (fsHandle != 0)
                    graphicsDevice.GL.AttachShader(programHandle, fsHandle);

                graphicsDevice.GL.LinkProgram(programHandle);
                graphicsDevice.GL.GetProgram(programHandle, ProgramPropertyARB.LinkStatus, out int linkStatus);
                if (linkStatus == (int)GLEnum.False)
                {
                    graphicsDevice.GL.DeleteProgram(programHandle);
                    throw new ProgramLinkException(graphicsDevice.GL.GetProgramInfoLog(programHandle));
                }
                if (getLogs)
                    ProgramLog = graphicsDevice.GL.GetProgramInfoLog(programHandle);

                graphicsDevice.GL.DetachShader(programHandle, vsHandle);
                if (gsHandle != 0)
                    graphicsDevice.GL.DetachShader(programHandle, gsHandle);
                if (fsHandle != 0)
                    graphicsDevice.GL.DetachShader(programHandle, fsHandle);

                ActiveVertexAttrib[] activeAttribs = CreateActiveAttribArray(graphicsDevice, programHandle);
                if (!DoVertexAttributesMatch(activeAttribs, specifiedAttribs))
                {
                    graphicsDevice.GL.DeleteProgram(programHandle);
                    throw new InvalidOperationException("The specified vertex attributes don't match the ones declared in the shaders");
                }

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
    }

    public struct SpecifiedShaderAttrib
    {
        public AttributeType AttribType;
        public int Size;
        public string Name;

        public SpecifiedShaderAttrib(string name, AttributeType type, int size)
        {
            AttribType = type;
            Name = name;
            Size = size;
        }

        public bool Matches(in ActiveVertexAttrib activeAttrib)
        {
            return AttribType == activeAttrib.AttribType
                //&& Size == activeAttrib.Size
                && Name == activeAttrib.Name;
        }

        public override string ToString()
        {
            return string.Concat(AttribType.ToString(), " ", Name);
        }
    }
}
