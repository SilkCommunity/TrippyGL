using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
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
            : base(owner, uniformLoc, name, size, type)
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
