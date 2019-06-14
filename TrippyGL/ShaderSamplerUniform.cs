using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Represents a sampler-type shader uniform from a shader program and allows control over that uniform
    /// </summary>
    public class ShaderSamplerUniform : ShaderUniform
    {
        /// <summary>The texture value assigned to this sampler uniform</summary>
        public Texture TextureValue { get; private set; }

        /// <summary>The last texture unit to be applied as a value for the sampler</summary>
        private int lastTextureUnitApplied = -1;

        internal ShaderSamplerUniform(ShaderProgram owner, int uniformLoc, string name, int size, ActiveUniformType type)
            : base(owner, uniformLoc, name, size, type)
        {

        }

        public override void SetValueTexture(Texture value)
        {
            if (value == null)
                throw new ArgumentNullException("texture");

            if (this.TextureValue != value)
            {
                this.TextureValue = value;
                OwnerProgram.Uniforms.isTextureListDirty = true;
            }
        }

        public override void SetValueTextureArray(Texture[] values, int startValueIndex, int startUniformIndex, int count)
        {
            if (values == null)
                throw new ArgumentNullException("textures");

            if (startValueIndex < 0 || startValueIndex >= values.Length)
                throw new ArgumentOutOfRangeException("startValueIndex", "startValueIndex must be in the range [0, Length)");

            if (startUniformIndex == 0 && count > 0)
                SetValueTexture(values[startValueIndex]);
        }

        /// <summary>
        /// Applies an integer value to the uniform. If it's already the value stored in the uniform, then it doesn't modify it.
        /// This is because to indicate to a sampler which texture it should sample from, you have to bind that texture to a texture unit
        /// and apply that value as int to the uniform
        /// </summary>
        /// <param name="unit">The value to apply to the uniform</param>
        private void ApplyUniformValue(int unit)
        {
            if (lastTextureUnitApplied != unit)
            {
                lastTextureUnitApplied = unit;
                GL.Uniform1(UniformLocation, unit);
            }
        }

        /// <summary>
        /// This is called by ShaderUniformList.EnsureSamplerUniformsSet() after all the required sampler uniform textures have been bound to different units
        /// This method supposes that all the "value" texture is bound to a texture unit, so it is ready to be used.
        /// This method also assumes that the ShaderProgram is the one currently in use.
        /// </summary>
        internal void ApplyUniformValue()
        {
            if (TextureValue != null)
                ApplyUniformValue(TextureValue.lastBindUnit);
        }
    }
}
