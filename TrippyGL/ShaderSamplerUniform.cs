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

        internal ShaderSamplerUniform(ShaderProgram owner, int uniformLoc, string name, ActiveUniformType type)
            : base(owner, uniformLoc, name, type)
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

    /// <summary>
    /// Represents a sampler-array-type shader uniform from a shader program and allows control over that uniform
    /// </summary>
    public class ShaderSamplerArrayUniform : ShaderUniform
    {
        /// <summary>The texture values for the samplers in the array</summary>
        internal Texture[] texValues;

        /// <summary>The length of the uniform array</summary>
        public int ArrayLength { get { return texValues.Length; } }

        internal ShaderSamplerArrayUniform(ShaderProgram owner, int uniformLoc, string name, int size, ActiveUniformType type)
            : base(owner, uniformLoc, name, type)
        {
            texValues = new Texture[size];
        }

        /// <summary>
        /// Gets the applied texture value for the specified index on the uniform array
        /// </summary>
        public Texture GetValue(int index)
        {
            return texValues[index];
        }

        public override void SetValueTexture(Texture value)
        {
            if (value == null)
                throw new ArgumentNullException("texture");

            if (texValues[0] != value)
            {
                texValues[0] = value;
                OwnerProgram.Uniforms.isTextureListDirty = true;
            }
        }

        public override void SetValueTextureArray(Texture[] values, int startValueIndex, int startUniformIndex, int count)
        {
            ValidateSetParams(values, startValueIndex, startUniformIndex, count);

            for (int i = 0; i < count; i++)
            {
                int uniformindex = startUniformIndex + i;
                int valueindex = startValueIndex + i;
                if (this.texValues[uniformindex] != values[valueindex])
                {
                    this.texValues[uniformindex] = values[valueindex];
                    OwnerProgram.Uniforms.isTextureListDirty = true;
                }
            }
        }

        /// <summary>
        /// This is called by ShaderUniformList.EnsureSamplerUniformsSet() after all the required sampler uniform textures have been bound to different units.
        /// This method supposes that all the textures in the "values" array are bound to texture units, so they are all ready to be used together.
        /// This method also assumes the uniform's ShaderProgram is the one currently in use
        /// </summary>
        internal void ApplyUniformValues()
        {
            int[] units = new int[texValues.Length];
            for (int i = 0; i < texValues.Length; i++)
                if (texValues[i] != null)
                    units[i] = texValues[i].lastBindUnit;
            GL.Uniform1(UniformLocation, texValues.Length, units);
        }

        private protected void ValidateSetParams(Texture[] values, int startValueIndex, int startUniformIndex, int count)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            if (startValueIndex < 0 || startValueIndex >= values.Length)
                throw new ArgumentOutOfRangeException("startValueIndex", "startValueIndex must be in the range [0, values.Length)");

            if (startUniformIndex < 0 || startUniformIndex >= this.texValues.Length)
                throw new ArgumentOutOfRangeException("startUniformIndex", "startUniformIndex must be in the range [0, Length)");

            if (count > values.Length - startValueIndex)
                throw new ArgumentException("The textures array isn't big enough to read count values starting from startIndex");

            if (count > values.Length - startUniformIndex)
                throw new ArgumentException("The uniform array isn't big enough to write count values starting from startUniformIndex");
        }
    }
}
