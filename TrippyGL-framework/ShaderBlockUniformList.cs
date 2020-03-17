using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// A list of <see cref="ShaderBlockUniform"/>-s belonging to a <see cref="ShaderProgram"/>.
    /// This class also does some controlling over these uniform blocks to make everything run nicely.
    /// </summary>
    public readonly struct ShaderBlockUniformList : IEquatable<ShaderBlockUniformList>
    {
        /// <summary>The <see cref="ShaderProgram"/> the uniform blocks belong to.</summary>
        public readonly ShaderProgram Program;

        /// <summary>All of the block uniforms in the <see cref="ShaderProgram"/>.</summary>
        private readonly ShaderBlockUniform[] uniforms;

        /// <summary>
        /// Gets a <see cref="ShaderBlockUniform"/> by name. If there's no such name, returns null.
        /// </summary>
        /// <param name="name">The name (declared in the shaders) of the <see cref="ShaderBlockUniform"/> to get.</param>
        public ShaderBlockUniform this[string name]
        {
            get
            {
                for (int i = 0; i < uniforms.Length; i++)
                    if (uniforms[i].Name == name)
                        return uniforms[i];
                return null;
            }
        }

        /// <summary>The amount of <see cref="ShaderBlockUniform"/> in the <see cref="ShaderProgram"/>.</summary>
        public int Count => uniforms.Length;

        /// <summary>The total amount of uniforms from all the block. If a block has two values, these count as two uniforms.</summary>
        public readonly int TotalUniformCount;

        /// <summary>
        /// Creates a <see cref="ShaderBlockUniformList"/> and queries the uniforms for a given <see cref="ShaderProgram"/>.
        /// </summary>
        internal ShaderBlockUniformList(ShaderProgram program)
        {
            GL.GetProgram(program.Handle, GetProgramParameterName.ActiveUniformBlocks, out int blockUniformCount);

            Program = program;
            TotalUniformCount = 0;

            if (blockUniformCount < 0)
                uniforms = null;
            else
            {
                uniforms = new ShaderBlockUniform[blockUniformCount];
                for (int i = 0; i < blockUniformCount; i++)
                {
                    GL.GetActiveUniformBlock(program.Handle, i, ActiveUniformBlockParameter.UniformBlockNameLength, out int nameLength);
                    GL.GetActiveUniformBlockName(program.Handle, i, nameLength, out int actualNameLength, out string name);
                    GL.GetActiveUniformBlock(program.Handle, i, ActiveUniformBlockParameter.UniformBlockBinding, out int bindingIndex);
                    GL.GetActiveUniformBlock(program.Handle, i, ActiveUniformBlockParameter.UniformBlockActiveUniforms, out int activeUniformCount);
                    TotalUniformCount += activeUniformCount;

                    uniforms[i] = new ShaderBlockUniform(program, bindingIndex, name, activeUniformCount);
                }
            }
        }

        public static bool operator ==(ShaderBlockUniformList left, ShaderBlockUniformList right) => left.Equals(right);

        public static bool operator !=(ShaderBlockUniformList left, ShaderBlockUniformList right) => !left.Equals(right);

        /// <summary>
        /// Ensures the buffer bindings for the uniform blocks are correctly set for a drawing operation.
        /// This is called by <see cref="ShaderProgram.EnsurePreDrawStates"/>.
        /// </summary>
        internal void EnsureBufferBindingsSet()
        {
            if (uniforms != null)
                for (int i = 0; i < uniforms.Length; i++)
                    uniforms[i].ApplyUniformValue();
        }

        public override string ToString()
        {
            return string.Concat(nameof(Count) + "=", Count.ToString());
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Program.GetHashCode();
                for (int i = 0; i < uniforms.Length; i++)
                    hashCode = (hashCode * 397) ^ uniforms[i].GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(ShaderBlockUniformList other)
        {
            return Program == other.Program
                && uniforms == other.uniforms;
        }

        public override bool Equals(object obj)
        {
            if (obj is ShaderBlockUniformList shaderBlockUniformList)
                return Equals(shaderBlockUniformList);
            return false;
        }
    }
}
