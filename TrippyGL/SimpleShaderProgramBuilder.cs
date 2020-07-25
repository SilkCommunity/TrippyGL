using System;
using System.Text;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    public struct SimpleShaderProgramBuilder
    {
        private static WeakReference<StringBuilder> stringBuilderReference;

        /// <summary>The version-profile string for GLSL to use. If null, "330 core" will be used.</summary>
        public string GLSLVersionString;

        public int PositionAttributeIndex;
        public int NormalAttributeIndex;
        public int ColorAttributeIndex;
        public int TexCoordsAttributeIndex;

        public bool VertexColorsEnabled;
        public bool TextureEnabled;

        public int DirectionalLights;

        public bool LightningEnabled => DirectionalLights > 0;

        public string VertexShaderLog;
        public string FragmentShaderLog;
        public string ProgramLog;

        /// <summary>
        /// Automatically sets the configuration of attribute indices.
        /// </summary>
        /// <remarks>
        /// This function loops through a vertex attributes descriptions. The first attribute found
        /// whose <see cref="VertexAttribDescription.AttribType"/> is <see cref="AttributeType.FloatVec3"/>
        /// will be used for Position, and the second will be used for Normal.
        /// The first ocurrance of a <see cref="AttributeType.FloatVec4"/> will be used for Color, and 
        /// the first ocurrance of a <see cref="AttributeType.FloatVec2"/> will be used for TexCoords.
        /// Any attribute left unassigned is set to -1.
        /// </remarks>
        /// <typeparam name="T">The type of vertex the <see cref="SimpleShaderProgram"/> will use.</typeparam>
        public void SpecifyVertexAttribs<T>() where T : unmanaged, IVertex
        {
            T tmp = default;
            int attribDescCount = tmp.AttribDescriptionCount;
            Span<VertexAttribDescription> attribDescriptions = attribDescCount > 32 ?
                new VertexAttribDescription[attribDescCount] : stackalloc VertexAttribDescription[attribDescCount];
            tmp.WriteAttribDescriptions(attribDescriptions);

            PositionAttributeIndex = FindAttributeType(attribDescriptions, AttributeType.FloatVec3);
            if (PositionAttributeIndex == -1)
                throw new InvalidOperationException("Invalid vertex format. Must have at least a FloatVec3 for position.");

            NormalAttributeIndex = FindAttributeType(attribDescriptions, AttributeType.FloatVec3, PositionAttributeIndex + 1);
            ColorAttributeIndex = FindAttributeType(attribDescriptions, AttributeType.FloatVec4);
            TexCoordsAttributeIndex = FindAttributeType(attribDescriptions, AttributeType.FloatVec2);

            // Returns the index of the first VertexAttribDescription in the span with the requested
            // AttributeType, starting at startIndex. If no ocurrance is found, return -1
            static int FindAttributeType(Span<VertexAttribDescription> attribDescriptions, AttributeType attribType, int startIndex = 0)
            {
                while (startIndex < attribDescriptions.Length)
                {
                    if (attribDescriptions[startIndex].AttribType == attribType)
                        return startIndex;
                    startIndex++;
                }

                return -1;
            }
        }

        public SimpleShaderProgram Create(GraphicsDevice graphicsDevice, bool getLogs = false)
        {
            const string DifferentVertexAttribIndicesError = "All specified vertex attribute indices must be different.";

            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            if (PositionAttributeIndex < 0)
                throw new InvalidOperationException("A vertex attribute index for Position must always be specified.");

            if (LightningEnabled)
            {
                if (NormalAttributeIndex < 0)
                    throw new InvalidOperationException("Using lightning requires a vertex Normal attribute.");
                if (NormalAttributeIndex == PositionAttributeIndex)
                    throw new InvalidOperationException(DifferentVertexAttribIndicesError);
            }

            if (VertexColorsEnabled)
            {
                if (ColorAttributeIndex < 0)
                    throw new InvalidOperationException("Using vertex colors requires a vertex Color attribute.");
                if (ColorAttributeIndex == PositionAttributeIndex || ColorAttributeIndex == NormalAttributeIndex)
                    throw new InvalidOperationException(DifferentVertexAttribIndicesError);
            }

            if (TextureEnabled)
            {
                if (TexCoordsAttributeIndex < 0)
                    throw new InvalidOperationException("Using textures requires a vertex TexCoords attribute.");
                if (TexCoordsAttributeIndex == PositionAttributeIndex || TexCoordsAttributeIndex == NormalAttributeIndex
                    || TexCoordsAttributeIndex == ColorAttributeIndex)
                    throw new InvalidOperationException(DifferentVertexAttribIndicesError);
            }

            StringBuilder builder = GetStringBuilder();

            try
            {
                bool useLightning = LightningEnabled;

                builder.Append("#version ");
                builder.Append(GLSLVersionString ?? "330 core\n\n");

                builder.Append("uniform mat4 World, View, Projection;\n");

                builder.Append("\nin vec3 vPosition;\n");
                if (useLightning) builder.Append("in vec3 vNormal;\n");
                if (VertexColorsEnabled) builder.Append("in vec4 vColor;\n");
                if (TextureEnabled) builder.Append("in vec2 vTexCoords;\n");
                builder.Append('\n');
                if (useLightning) builder.Append("out vec3 fPosition;\nout vec3 fNormal;\n");
                if (VertexColorsEnabled) builder.Append("out vec4 fColor;\n");
                if (TextureEnabled) builder.Append("out vec2 fTexCoords;\n");

                builder.Append("\nvoid main() {\n");
                builder.Append("vec4 worldPos = World * vec4(vPosition, 1.0);\n");
                builder.Append("gl_Position = Projection * View * worldPos;\n\n");
                if (useLightning)
                {
                    builder.Append("fPosition = worldPos.xyz;\n");
                    builder.Append("fNormal = (World * vec4(vNormal, 0.0)).xyz;\n");
                }
                if (VertexColorsEnabled) builder.Append("fColor = vColor;\n");
                if (TextureEnabled) builder.Append("fTexCoords = vTexCoords;\n");

                builder.Append('}');

                string vertexShader = builder.ToString();
                builder.Clear();





                builder.Append("#version ");
                builder.Append(GLSLVersionString ?? "330 core\n\n");

                builder.Append("uniform vec4 color;\n\n");

                if (TextureEnabled) builder.Append("uniform sampler2D samp;\n\n");

                if (useLightning)
                {
                    builder.Append("uniform vec3 cameraPos;\n");
                    builder.Append("uniform vec4 ambientLightColor;\n");
                    builder.Append("uniform float reflectivity;\n");
                    for (int i = 0; i < DirectionalLights; i++)
                    {
                        string itostring = i.ToString();
                        builder.Append("uniform vec4 lightColor");
                        builder.Append(itostring);
                        builder.Append(";\n");
                        builder.Append("uniform vec4 lightDir");
                        builder.Append(itostring);
                        builder.Append(";\n");
                    }
                }

                if (useLightning) builder.Append("in vec3 fPosition;\nin vec3 fNormal;\n");
                if (VertexColorsEnabled) builder.Append("in vec4 fColor;\n");
                if (TextureEnabled) builder.Append("in vec2 fTexCoords;\n");
                builder.Append("\nout vec4 FragColor;\n");

                if (DirectionalLights > 0)
                {
                    builder.Append("\nvec4 calcDirectionalLight(in vec3 norm, in vec3 ldir, in vec4 lcolor, in vec3 toCamVec, in float specPow) {\n");
                    builder.Append("float brightness = max(0.0, dot(norm, -ldir));\n");
                    builder.Append("vec3 reflectedDir = reflect(ldir, norm);\n");
                    builder.Append("float specFactor = max(0.0, dot(reflectedDir, toCamVec));\n");
                    builder.Append("float dampedFactor = pow(specFactor, specPow);\n");
                    builder.Append("return (brightness + (dampedFactor * reflectivity)) * lcolor;\n}\n");
                }

                builder.Append("\nvoid main() {\n");
                if (useLightning)
                {
                    builder.Append("vec3 unitNormal = normalize(fNormal);\n");
                    builder.Append("vec3 unitToCameraVec = normalize(cameraPos - fPosition);\n\n");
                    builder.Append("vec4 light = ambientLightColor;\n");

                    for (int i = 0; i < DirectionalLights; i++)
                    {
                        string itostring = i.ToString();
                        builder.Append("light += calcDirectionalLight(unitNormal, lightDir");
                        builder.Append(itostring);
                        builder.Append(".xyz, lightColor");
                        builder.Append(itostring);
                        builder.Append(", unitToCameraVec, lightDir");
                        builder.Append(itostring);
                        builder.Append(".w);\n");
                    }
                }

                builder.Append("vec4 finalColor = color;\n");
                if (VertexColorsEnabled) builder.Append("finalColor *= fColor;\n");
                if (TextureEnabled) builder.Append("finalColor *= texture(samp, fTexCoords);\n");

                if (useLightning) builder.Append("finalColor *= light;\n");

                builder.Append("FragColor = finalColor;\n}");

                string fragmentShader = builder.ToString();

                ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
                programBuilder.VertexShaderCode = vertexShader;
                programBuilder.FragmentShaderCode = fragmentShader;

                int maxAttribs = Math.Max(PositionAttributeIndex, Math.Max(NormalAttributeIndex, Math.Max(ColorAttributeIndex, TexCoordsAttributeIndex)));
                SpecifiedShaderAttrib[] attribs = new SpecifiedShaderAttrib[maxAttribs + 1];
                attribs[PositionAttributeIndex] = new SpecifiedShaderAttrib("vPosition", AttributeType.FloatVec3);
                if (useLightning) attribs[NormalAttributeIndex] = new SpecifiedShaderAttrib("vNormal", AttributeType.FloatVec3);
                if (VertexColorsEnabled) attribs[ColorAttributeIndex] = new SpecifiedShaderAttrib("vColor", AttributeType.FloatVec4);
                if (TextureEnabled) attribs[TexCoordsAttributeIndex] = new SpecifiedShaderAttrib("vTexCoords", AttributeType.FloatVec2);
                programBuilder.SpecifyVertexAttribs(attribs);

                uint programHandle = programBuilder.CreateInternal(graphicsDevice, out ActiveVertexAttrib[] activeAttribs, getLogs);
                VertexShaderLog = programBuilder.VertexShaderLog;
                FragmentShaderLog = programBuilder.FragmentShaderLog;
                ProgramLog = programBuilder.ProgramLog;
                return new SimpleShaderProgram(graphicsDevice, programHandle, activeAttribs, VertexColorsEnabled, DirectionalLights);
            }
            finally
            {
                builder.Clear();
                ReturnStringBuilder(builder);
            }
        }

        private static StringBuilder GetStringBuilder()
        {
            if (stringBuilderReference != null && stringBuilderReference.TryGetTarget(out StringBuilder builder))
            {
                stringBuilderReference.SetTarget(null);
                builder.Clear();
                return builder;
            }

            return new StringBuilder(1024);

        }

        private static void ReturnStringBuilder(StringBuilder stringBuilder)
        {
            if (stringBuilderReference == null)
                stringBuilderReference = new WeakReference<StringBuilder>(stringBuilder);
            else
            {
                if (!stringBuilderReference.TryGetTarget(out StringBuilder oldBuilder) || oldBuilder == null || oldBuilder.Length < stringBuilder.Length)
                    stringBuilderReference.SetTarget(stringBuilder);
            }
        }
    }
}
