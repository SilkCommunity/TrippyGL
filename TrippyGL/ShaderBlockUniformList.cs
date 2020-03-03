using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// A list of <see cref="ShaderBlockUniform"/>-s belonging to a <see cref="ShaderProgram"/>.
    /// This class also does some controlling over these uniform blocks to make everything run nicely.
    /// </summary>
    public sealed class ShaderBlockUniformList
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

        private ShaderBlockUniformList(ShaderProgram program, int blockUniformCount)
        {
            Program = program;
            uniforms = new ShaderBlockUniform[blockUniformCount];
            TotalUniformCount = 0;

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

        /// <summary>
        /// Ensures the buffer bindings for the blocks is correct.
        /// This is called by <see cref="ShaderProgram.EnsurePreDrawStates"/>
        /// </summary>
        internal void EnsureAllSet()
        {
            for (int i = 0; i < uniforms.Length; i++)
                uniforms[i].ApplyUniformValue();
        }

        public override string ToString()
        {
            return string.Concat(nameof(Count) + "=", Count.ToString());
        }

        /// <summary>
        /// Creates a <see cref="ShaderBlockUniformList"/> and queries the uniforms for a given <see cref="ShaderProgram"/>.
        /// </summary>
        internal static ShaderBlockUniformList CreateForProgram(ShaderProgram program)
        {
            GL.GetProgram(program.Handle, GetProgramParameterName.ActiveUniformBlocks, out int blockUniformCount);
            return blockUniformCount == 0 ? null : new ShaderBlockUniformList(program, blockUniformCount);
        }
    }
}
