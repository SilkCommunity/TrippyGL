using System;
using System.Numerics;

namespace TrippyGL
{
    /// <summary>
    /// A simple, configurable <see cref="ShaderProgram"/> that provides basic functionality
    /// without having to write any GLSL code.
    /// </summary>
    public sealed class SimpleShaderProgram : ShaderProgram
    {
        /// <summary>Whether this <see cref="SimpleShaderProgram"/> uses vertex colors.</summary>
        public readonly bool VertexColorsEnabled;

        /// <summary>Whether this <see cref="SimpleShaderProgram"/> uses vertex texture coordinates.</summary>
        public readonly bool TextureEnabled;

        /// <summary>Whether this <see cref="SimpleShaderProgram"/> uses vertex normals.</summary>
        public readonly bool LightningEnabled;

        /// <summary>Whether this <see cref="SimpleShaderProgram"/> includes a World matrix in the vertex shader.</summary>
        public readonly bool HasWorldUniform;

        // These are all the uniforms for controlling this SimpleShaderProgram's parameters
        private readonly ShaderUniform worldUniform;
        private readonly ShaderUniform viewUniform;
        private readonly ShaderUniform projectionUniform;
        private readonly ShaderUniform colorUniform;
        internal readonly ShaderUniform sampUniform;
        private readonly ShaderUniform cameraPosUniform;
        private readonly ShaderUniform reflectivityUniform;
        private readonly ShaderUniform specularPowerUniform;
        private readonly ShaderUniform ambientLightColorUniform;

        // The last applied values for this SimpleShaderProgram's parameters
        private Matrix4x4 world;
        private Matrix4x4 view;
        private Matrix4x4 projection;
        private Vector4 color;
        private Texture2D texture;
        private float reflectivity;
        private float specularPower;
        private Vector3 ambientLightColor;

        // The lists of directional/positional lights, or null if there are none.
        private readonly DirectionalLight[] directionalLights;
        private readonly PositionalLight[] positionalLights;

        /// <summary>
        /// Gets or sets this <see cref="SimpleShaderProgram"/>'s World matrix.
        /// </summary>
        public Matrix4x4 World
        {
            get => world;
            set
            {
                worldUniform.SetValueMat4(value);
                world = value;
            }
        }

        /// <summary>
        /// Gets or sets this <see cref="SimpleShaderProgram"/>'s View matrix.
        /// </summary>
        public Matrix4x4 View
        {
            get => view;
            set
            {
                Vector3 cameraPos;
                if (!cameraPosUniform.IsEmpty && Matrix4x4.Invert(value, out Matrix4x4 inverse))
                {
                    Vector4 tmp = Vector4.Transform(Vector4.UnitW, inverse);
                    cameraPos = new Vector3(tmp.X, tmp.Y, tmp.Z);
                }
                else
                    cameraPos = default;
                SetView(value, cameraPos);
            }
        }

        /// <summary>
        /// Gets or sets this <see cref="SimpleShaderProgram"/>'s Projection matrix.
        /// </summary>
        public Matrix4x4 Projection
        {
            get => projection;
            set
            {
                projectionUniform.SetValueMat4(value);
                projection = value;
            }
        }

        /// <summary>
        /// Gets or sets this <see cref="SimpleShaderProgram"/>'s default color.
        /// </summary>
        /// <remarks>
        /// This color multiplies the output of the fragment shader.
        /// </remarks>
        public Vector4 Color
        {
            get => color;
            set
            {
                colorUniform.SetValueVec4(value);
                color = value;
            }
        }

        /// <summary>
        /// Gets or sets this <see cref="SimpleShaderProgram"/>'s <see cref="Texture2D"/>.
        /// </summary>
        /// <remarks>
        /// If enabled, the <see cref="SimpleShaderProgram"/> will sample this <see cref="Texture2D"/>
        /// using the coordinates in the vertices as part of the output color calculation.
        /// </remarks>
        public Texture2D Texture
        {
            get => texture;
            set
            {
                sampUniform.SetValueTexture(value);
                texture = value;
            }
        }

        /// <summary>
        /// Gets or sets this <see cref="SimpleShaderProgram"/>'s material reflectivity parameter.
        /// </summary>
        /// <remarks>This only works when lightning is enabled.</remarks>
        public float Reflectivity
        {
            get => reflectivity;
            set
            {
                reflectivityUniform.SetValueFloat(value);
                reflectivity = value;
            }
        }

        /// <summary>
        /// Gets or sets this <see cref="SimpleShaderProgram"/>'s specular light power parameter.
        /// </summary>
        /// <remarks>This only works when lightning is enabled.</remarks>
        public float SpecularPower
        {
            get => specularPower;
            set
            {
                specularPowerUniform.SetValueFloat(value);
                specularPower = value;
            }
        }

        /// <summary>
        /// Gets or sets this <see cref="SimpleShaderProgram"/>'s ambient lightning color.
        /// </summary>
        /// <remarks>This only works when lightning is enabled.</remarks>
        public Vector3 AmbientLightColor
        {
            get => ambientLightColor;
            set
            {
                ambientLightColorUniform.SetValueVec3(value);
                ambientLightColor = value;
            }
        }

        /// <summary>
        /// Gets this <see cref="SimpleShaderProgram"/>'s list of directional lights.
        /// </summary>
        public ReadOnlySpan<DirectionalLight> DirectionalLights => new ReadOnlySpan<DirectionalLight>(directionalLights);

        /// <summary>
        /// Gets this <see cref="SimpleShaderProgram"/>'s list of positional lights.
        /// </summary>
        public ReadOnlySpan<PositionalLight> PositionalLights => new ReadOnlySpan<PositionalLight>(positionalLights);

        /// <summary>
        /// Creates a <see cref="SimpleShaderProgram"/> from an already compiled GL Program Object.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="programHandle">The GL Program Object's handle.</param>
        /// <param name="activeAttribs">The active attributes, already queried from the program.</param>
        /// <param name="vertColorsEnabled">Whether this <see cref="SimpleShaderProgram"/> uses vertex colors.</param>
        /// <param name="textureEnabled">Whether this <see cref="SimpleShaderProgram"/> uses a texture.</param>
        /// <param name="directionalLightCount">The amount of directional lights on this <see cref="SimpleShaderProgram"/>.</param>
        /// <param name="positionalLightCount">The amount of positional lights on this <see cref="SimpleShaderProgram"/>.</param>
        internal SimpleShaderProgram(GraphicsDevice graphicsDevice, uint programHandle, ActiveVertexAttrib[] activeAttribs,
            bool hasVertexShader, bool hasGeometryShader, bool hasFragmentShader,
            bool vertColorsEnabled, bool textureEnabled, int directionalLightCount, int positionalLightCount)
            : base(graphicsDevice, programHandle, activeAttribs, hasVertexShader, hasGeometryShader, hasFragmentShader)
        {
            const string InvalidUniformMessage = "Uniform not found or is incorrect type: ";

            // We set these booleans based on what we've got.
            VertexColorsEnabled = vertColorsEnabled;
            TextureEnabled = textureEnabled;
            LightningEnabled = directionalLightCount != 0 || positionalLightCount != 0;

            // These uniforms are always present- so let's get them and ensure they're valid.
            worldUniform = Uniforms["World"];
            HasWorldUniform = !worldUniform.IsEmpty;
            if (HasWorldUniform && worldUniform.UniformType != UniformType.FloatMat4)
                throw new InvalidOperationException(InvalidUniformMessage + "World");
            viewUniform = Uniforms["View"];
            if (viewUniform.UniformType != UniformType.FloatMat4)
                throw new InvalidOperationException(InvalidUniformMessage + "View");
            projectionUniform = Uniforms["Projection"];
            if (projectionUniform.UniformType != UniformType.FloatMat4)
                throw new InvalidOperationException(InvalidUniformMessage + "Projection");
            colorUniform = Uniforms["Color"];
            if (colorUniform.UniformType != UniformType.FloatVec4)
                throw new InvalidOperationException(InvalidUniformMessage + "Color");

            // If texture is enabled, we get the uniform for sampling a texture.
            if (TextureEnabled)
            {
                sampUniform = Uniforms["samp"];
                if (sampUniform.UniformType != UniformType.Sampler2D)
                    throw new InvalidOperationException(InvalidUniformMessage + "samp");
            }

            // If lightning is enabled, we get the uniforms for managing the lights.
            if (LightningEnabled)
            {
                ambientLightColorUniform = Uniforms["ambientLightColor"];
                if (ambientLightColorUniform.UniformType != UniformType.FloatVec3)
                    throw new InvalidOperationException(InvalidUniformMessage + "ambientLightColor");
                cameraPosUniform = Uniforms["cameraPos"];
                if (cameraPosUniform.UniformType != UniformType.FloatVec3)
                    throw new InvalidOperationException(InvalidUniformMessage + "cameraPos");
                reflectivityUniform = Uniforms["reflectivity"];
                if (reflectivityUniform.UniformType != UniformType.Float)
                    throw new InvalidOperationException(InvalidUniformMessage + "reflectivity");
                specularPowerUniform = Uniforms["specularPower"];
                if (specularPowerUniform.UniformType != UniformType.Float)
                    throw new InvalidOperationException(InvalidUniformMessage + "specularPower");

                // We create the directional lights and store those arrays
                // This also sets the default values on the lights
                directionalLights = CreateDirectionalLights(directionalLightCount);
                positionalLights = CreatePositionalLights(positionalLightCount);
            }

            // We set the default values for the different shader parameters.
            if (HasWorldUniform)
                World = Matrix4x4.Identity;
            SetView(Matrix4x4.Identity, Vector3.Zero);
            Projection = Matrix4x4.Identity;
            Color = new Vector4(1, 1, 1, 1);

            // If lightning is enabled, we set the default values for those parameters too.
            if (LightningEnabled)
            {
                AmbientLightColor = Vector3.Zero;
                Reflectivity = 1;
                SpecularPower = 1;
            }
        }

        /// <summary>
        /// Creates an array of <see cref="DirectionalLight"/>-s for this <see cref="SimpleShaderProgram"/>
        /// by getting the <see cref="ShaderUniform"/>-s from <see cref="ShaderProgram.Uniforms"/>.
        /// </summary>
        /// <param name="directionalLightCount">The amount of directional lights on this <see cref="SimpleShaderProgram"/>.</param>
        private DirectionalLight[] CreateDirectionalLights(int directionalLightCount)
        {
            if (directionalLightCount <= 0)
                return null;

            DirectionalLight[] lights = new DirectionalLight[directionalLightCount];
            for (int i = 0; i < directionalLightCount; i++)
            {
                string itostring = i.ToString();
                ShaderUniform dirUniform = Uniforms["dLightDir" + itostring];
                ShaderUniform diffColUniform = Uniforms["dLightDiffColor" + itostring];
                ShaderUniform specColUniform = Uniforms["dLightSpecColor" + itostring];
                if (dirUniform.UniformType != UniformType.FloatVec3 || diffColUniform.UniformType != UniformType.FloatVec3
                    || specColUniform.UniformType != UniformType.FloatVec3)
                    throw new InvalidOperationException("Invalid uniforms for " + nameof(DirectionalLight) + " number " + itostring);

                lights[i] = new DirectionalLight(dirUniform, diffColUniform, specColUniform);
            }

            return lights;
        }

        /// <summary>
        /// Creates an array of <see cref="PositionalLight"/>-s for this <see cref="SimpleShaderProgram"/>
        /// by getting the <see cref="ShaderUniform"/>-s from <see cref="ShaderProgram.Uniforms"/>.
        /// </summary>
        /// <param name="positionalLightCount">The amount of positional lights on this <see cref="SimpleShaderProgram"/>.</param>
        private PositionalLight[] CreatePositionalLights(int positionalLightCount)
        {
            if (positionalLightCount <= 0)
                return null;

            PositionalLight[] lights = new PositionalLight[positionalLightCount];
            for (int i = 0; i < positionalLightCount; i++)
            {
                string itostring = i.ToString();
                ShaderUniform posUniform = Uniforms["pLightPos" + itostring];
                ShaderUniform diffColUniform = Uniforms["pLightDiffColor" + itostring];
                ShaderUniform specColUniform = Uniforms["pLightSpecColor" + itostring];
                ShaderUniform attConfigUniform = Uniforms["pAttConfig" + itostring];
                if (posUniform.UniformType != UniformType.FloatVec3 || diffColUniform.UniformType != UniformType.FloatVec3
                    || specColUniform.UniformType != UniformType.FloatVec3 || attConfigUniform.UniformType != UniformType.FloatVec3)
                    throw new InvalidOperationException("Invalid uniforms for " + nameof(PositionalLight) + " number " + itostring);

                lights[i] = new PositionalLight(posUniform, diffColUniform, specColUniform, attConfigUniform);
            }

            return lights;
        }

        /// <summary>
        /// Sets this <see cref="SimpleShaderProgram"/>'s View matrix, alongside the camera's position.
        /// </summary>
        /// <param name="view">The view matrix.</param>
        /// <param name="cameraPos">The camera's position in world space.</param>
        /// <remarks>
        /// The camera's position is needed for specular lightning calculations. If lightning is disabled,
        /// this parameter will be ignored.<para/>
        /// If the camera's position is known, setting the view matrix here is more performant than
        /// setting it in <see cref="View"/> because otherwise the camera's position needs to be
        /// calculated, and this is done by inversing the view matrix and multiplying that by (0, 0, 0, 1).
        /// </remarks>
        public void SetView(in Matrix4x4 view, in Vector3 cameraPos)
        {
            viewUniform.SetValueMat4(view);
            if (!cameraPosUniform.IsEmpty)
                cameraPosUniform.SetValueVec3(cameraPos);
            this.view = view;
        }

        /// <summary>
        /// Creates a <see cref="SimpleShaderProgram"/> to use with a specified vertex type.
        /// </summary>
        /// <typeparam name="T">The type of vertex the <see cref="SimpleShaderProgram"/> will use.</typeparam>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> the <see cref="SimpleShaderProgram"/> will use.</param>
        /// <param name="positionalLights">The amount of positional lights to include in the shader.</param>
        /// <param name="directionalLights">The amount of directional lights to include in the shader.</param>
        /// <param name="excludeWorldMatrix">Whether to exclude the World matrix from the vertex shader.</param>
        /// <remarks>
        /// Texture sampling and vertex colors will be included if the given vertex type has attributes
        /// to supply these. The first FloatVec4 attribute will be taken as color and the first FloatVec2
        /// attribute will be taken as texture coordinates.
        /// </remarks>
        public static SimpleShaderProgram Create<T>(GraphicsDevice graphicsDevice, int directionalLights = 0,
            int positionalLights = 0, bool excludeWorldMatrix = false) where T : unmanaged, IVertex
        {
            SimpleShaderProgramBuilder builder = new SimpleShaderProgramBuilder()
            {
                PositionalLights = positionalLights,
                DirectionalLights = directionalLights,
                ExcludeWorldMatrix = excludeWorldMatrix
            };
            builder.ConfigureVertexAttribs<T>();
            builder.VertexColorsEnabled = builder.ColorAttributeIndex >= 0;
            builder.TextureEnabled = builder.TexCoordsAttributeIndex >= 0;
            return builder.Create(graphicsDevice);
        }
    }
}
