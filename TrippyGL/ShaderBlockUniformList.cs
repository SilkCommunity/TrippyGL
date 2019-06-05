using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// A list of ShaderUniformBlock variables.
    /// This class also does some controlling over these uniforms to make everything run nicely
    /// </summary>
    public class ShaderBlockUniformList
    {
        /// <summary>All of the block uniforms in the ShaderProgram</summary>
        private ShaderBlockUniform[] uniforms;

        /// <summary>
        /// Gets a ShaderBlockUniform by name. If there's no such name, returns null
        /// </summary>
        /// <param name="name">The name (declared in the shaders) of the uniform block to get</param>
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

        /// <summary>The amount of block uniforms in the shader program</summary>
        public int Count { get { return uniforms.Length; } }

        /// <summary>The total amount of uniforms from all the block. If a block has two values, these are two uniforms</summary>
        public int TotalUniformCount { get; private set; }

        internal ShaderBlockUniformList(ShaderProgram program)
        {
            GL.GetProgram(program.Handle, GetProgramParameterName.ActiveUniformBlocks, out int length);
            uniforms = new ShaderBlockUniform[length];
            TotalUniformCount = 0;
            
            for(int i=0; i<length; i++)
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
        /// This is called by ShaderProgram.EnsurePreDrawStates()
        /// </summary>
        internal void EnsureAllSet()
        {
            for (int i = 0; i < uniforms.Length; i++)
                uniforms[i].EnsureValueApplied();
        }

        public override string ToString()
        {
            return String.Concat("ShaderBlockUniformList with ", uniforms.Length, " uniform blocks");
        }

    }
}
