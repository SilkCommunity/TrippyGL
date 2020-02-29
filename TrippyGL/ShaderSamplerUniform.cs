using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    /// <summary>
    /// Represents a sampler-type shader uniform from a <see cref="ShaderProgram"/> and allows control over that uniform.
    /// </summary>
    public class ShaderSamplerUniform : ShaderUniform
    {
        /// <summary>The <see cref="Texture"/> value assigned to this sampler uniform.</summary>
        public Texture TextureValue { get; private set; }

        /// <summary>The last texture unit to be applied as a value for the sampler.</summary>
        private int lastTextureUnitApplied = -1;

        internal ShaderSamplerUniform(ShaderProgram owner, int uniformLoc, string name, int size, ActiveUniformType type)
            : base(owner, uniformLoc, name, size, type)
        {

        }

        public override void SetValueTexture(Texture value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (TextureValue != value)
            {
                TextureValue = value;
                OwnerProgram.Uniforms.isTextureListDirty = true;
            }
        }

        public override void SetValueTextureArray(Span<Texture> values, int startUniformIndex = 0)
        {
            if (startUniformIndex == 0 && values.Length == 1)
                SetValueTexture(values[0]);
            else
                throw new InvalidOperationException(string.Concat("Tried to set multiple textures on a ", UniformType.ToString(), " uniform"));
        }

        /// <summary>
        /// Applies an integer value to the uniform. If it's already the value stored in the uniform, then it doesn't modify it.
        /// This is because to indicate to a sampler which texture it should sample from, you have to bind that texture to a texture unit
        /// and apply that value as int to the uniform.
        /// </summary>
        /// <param name="unit">The value to apply to the uniform.</param>
        private void ApplyUniformValue(int unit)
        {
            if (lastTextureUnitApplied != unit)
            {
                lastTextureUnitApplied = unit;
                GL.Uniform1(UniformLocation, unit);
            }
        }

        /// <summary>
        /// This is called by <see cref="ShaderUniformList.EnsureSamplerUniformsSet"/> after all the required sampler uniform
        /// textures have been bound to different units.<para/>
        /// This method assumes that the <see cref="TextureValue"/> texture is bound to a texture unit, so it is ready to be used.
        /// This method also assumes that the <see cref="ShaderProgram"/> is the one currently in use.
        /// </summary>
        internal void ApplyUniformValue()
        {
            if (TextureValue != null)
                ApplyUniformValue(TextureValue.lastBindUnit);
        }
    }
}
