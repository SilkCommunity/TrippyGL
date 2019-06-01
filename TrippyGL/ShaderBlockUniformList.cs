using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public class ShaderBlockUniformList
    {
        private ShaderBlockUniform[] uniforms;

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

        public int Count { get { return uniforms.Length; } }

        public int TotalUniformCount { get; private set; }

        public ShaderBlockUniformList(ShaderProgram program)
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

        public void EnsureAllSet()
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
