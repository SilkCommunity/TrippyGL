using System;
using System.Numerics;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Represents a positional light on a shader and provides functionality
    /// to change it's diffuse/specular colors and position.
    /// </summary>
    public sealed class PositionalLight
    {
        /// <summary>The uniform for setting this light's position.</summary>
        private readonly ShaderUniform positionUniform;

        /// <summary>The uniform for setting this light's diffuse color.</summary>
        private readonly ShaderUniform diffuseColorUniform;

        /// <summary>The uniform for setting this light's specular color.</summary>
        private readonly ShaderUniform specularColorUniform;

        /// <summary>The last known value of this light's position.</summary>
        private Vector3 position;

        /// <summary>The last known value of this light's diffuse color.</summary>
        private Vector3 diffuseColor;

        /// <summary>The last known value of this light's specular color.</summary>
        private Vector3 specularColor;

        /// <summary>
        /// Gets or sets this light's position.
        /// </summary>
        public Vector3 Position
        {
            get => position;
            set
            {
                positionUniform.SetValueVec3(value);
                position = value;
            }
        }

        /// <summary>
        /// Gets or sets this light's diffuse color.
        /// </summary>
        public Vector3 DiffuseColor
        {
            get => diffuseColor;
            set
            {
                diffuseColorUniform.SetValueVec3(value);
                diffuseColor = value;
            }
        }

        /// <summary>
        /// Gets or sets this light's specular color
        /// </summary>
        public Vector3 SpecularColor
        {
            get => specularColor;
            set
            {
                specularColorUniform.SetValueVec3(value);
                specularColor = value;
            }
        }

        /// <summary>
        /// Creates a <see cref="PositionalLight"/> with the specified <see cref="ShaderUniform"/>-s.
        /// </summary>
        /// <param name="positionUniform">The uniform for setting this light's position. Must be of type <see cref="UniformType.FloatVec3"/>.</param>
        /// <param name="diffuseColorUniform">The uniform for setting this light's diffuse color. Must be of type <see cref="UniformType.FloatVec3"/>.</param>
        /// <param name="specularColorUniform">The uniform for setting this light's specular color. Must be of type <see cref="UniformType.FloatVec3"/>.</param>
        public PositionalLight(ShaderUniform positionUniform, ShaderUniform diffuseColorUniform, ShaderUniform specularColorUniform)
        {
            const string WrongUniformsMessage = "The provided uniforms must be the correct type.";

            if (positionUniform.UniformType != UniformType.FloatVec3)
                throw new ArgumentException(WrongUniformsMessage, nameof(positionUniform));
            if (diffuseColorUniform.UniformType != UniformType.FloatVec3)
                throw new ArgumentException(WrongUniformsMessage, nameof(diffuseColorUniform));
            if (specularColorUniform.UniformType != UniformType.FloatVec3)
                throw new ArgumentException(WrongUniformsMessage, nameof(specularColorUniform));

            this.positionUniform = positionUniform;
            this.diffuseColorUniform = diffuseColorUniform;
            this.specularColorUniform = specularColorUniform;

            Position = new Vector3(0, 0, 0);
            DiffuseColor = new Vector3(1, 1, 1);
            SpecularColor = new Vector3(1, 1, 1);
        }
    }
}
