using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    /// <summary>
    /// Represents a sampler-array-type shader uniform from a <see cref="ShaderProgram"/> and allows control over that uniform.
    /// </summary>
    public class ShaderSamplerArrayUniform : ShaderUniform
    {
        /// <summary>The <see cref="Texture"/> values for the samplers in the array.</summary>
        internal Texture[] texValues;

        /// <summary>Gets the length of the uniform array.</summary>
        public int ArrayLength => texValues.Length;

        internal ShaderSamplerArrayUniform(ShaderProgram owner, int uniformLoc, string name, int size, ActiveUniformType type)
            : base(owner, uniformLoc, name, size, type)
        {
            texValues = new Texture[size];
        }

        /// <summary>
        /// Gets the applied texture value for the specified index on the uniform array.
        /// </summary>
        public Texture GetValue(int index)
        {
            return texValues[index];
        }

        public override void SetValueTexture(Texture value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (texValues[0] != value)
            {
                texValues[0] = value;
                OwnerProgram.Uniforms.isTextureListDirty = true;
            }
        }

        public override void SetValueTextureArray(Span<Texture> values, int startUniformIndex = 0)
        {
            if (startUniformIndex < 0 || startUniformIndex >= texValues.Length)
                throw new ArgumentOutOfRangeException(nameof(startUniformIndex), nameof(startUniformIndex) + " must be in the range [0, " + nameof(ArrayLength) + ")");

            if (startUniformIndex + values.Length > texValues.Length)
                throw new ArgumentOutOfRangeException("Tried to set too many textures");

            bool isDirty = false;
            for (int i = 0; i < values.Length; i++)
            {
                int uniformIndex = startUniformIndex + i;
                if (texValues[uniformIndex] != values[i])
                {
                    texValues[uniformIndex] = values[i];
                    isDirty = true;
                }
            }

            OwnerProgram.Uniforms.isTextureListDirty |= isDirty;
        }

        /// <summary>
        /// This is called by <see cref="ShaderUniformList.EnsureSamplerUniformsSet"/> after all the required sampler uniform
        /// textures have been bound to different units.<para/>
        /// This method assumes that the <see cref="TextureValue"/> texture is bound to a texture unit, so it is ready to be used.
        /// This method also assumes that the <see cref="ShaderProgram"/> is the one currently in use.
        /// </summary>
        internal void ApplyUniformValues()
        {
            Span<int> units = stackalloc int[texValues.Length];
            for (int i = 0; i < units.Length; i++)
                units[i] = texValues[i] == null ? 0 : texValues[i].lastBindUnit;
            GL.Uniform1(UniformLocation, units.Length, ref units[0]);
        }
    }
}
