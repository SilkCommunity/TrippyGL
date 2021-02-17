using System;
using System.Numerics;

namespace TrippyGL
{
    /// <summary>
    /// Represents a directional light on a shader and provides functionality
    /// to change it's diffuse/specular colors and direction.
    /// </summary>
    public sealed class DirectionalLight
    {
        internal static string IncorrectUniformMessage = "The provided uniforms must be the correct type.";

        /// <summary>The uniform for setting this light's direction.</summary>
        private readonly ShaderUniform directionUniform;

        /// <summary>The uniform for setting this light's diffuse color.</summary>
        private readonly ShaderUniform diffuseColorUniform;

        /// <summary>The uniform for setting this light's specular color.</summary>
        private readonly ShaderUniform specularColorUniform;

        /// <summary>The last known value of this light's direction.</summary>
        private Vector3 direction;

        /// <summary>The last known value of this light's diffuse color.</summary>
        private Vector3 diffuseColor;

        /// <summary>The last known value of this light's specular color.</summary>
        private Vector3 specularColor;

        /// <summary>
        /// Gets or sets this light's direction.
        /// </summary>
        public Vector3 Direction
        {
            get => direction;
            set
            {
                direction = Vector3.Normalize(value);
                directionUniform.SetValueVec3(direction);
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
        /// Creates a <see cref="DirectionalLight"/> with the specified <see cref="ShaderUniform"/>-s.
        /// </summary>
        /// <param name="directionUniform">The uniform for setting this light's direction. Must be of type <see cref="UniformType.FloatVec3"/>.</param>
        /// <param name="diffuseColorUniform">The uniform for setting this light's diffuse color. Must be of type <see cref="UniformType.FloatVec3"/>.</param>
        /// <param name="specularColorUniform">The uniform for setting this light's specular color. Must be of type <see cref="UniformType.FloatVec3"/>.</param>
        public DirectionalLight(ShaderUniform directionUniform, ShaderUniform diffuseColorUniform, ShaderUniform specularColorUniform)
        {
            if (directionUniform.UniformType != UniformType.FloatVec3)
                throw new ArgumentException(IncorrectUniformMessage, nameof(directionUniform));
            if (diffuseColorUniform.UniformType != UniformType.FloatVec3)
                throw new ArgumentException(IncorrectUniformMessage, nameof(diffuseColorUniform));
            if (specularColorUniform.UniformType != UniformType.FloatVec3)
                throw new ArgumentException(IncorrectUniformMessage, nameof(specularColorUniform));

            this.directionUniform = directionUniform;
            this.diffuseColorUniform = diffuseColorUniform;
            this.specularColorUniform = specularColorUniform;

            Direction = new Vector3(0, -1, 0);
            DiffuseColor = new Vector3(1, 1, 1);
            SpecularColor = new Vector3(1, 1, 1);
        }
    }
}
