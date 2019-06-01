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

        public ShaderBlockUniformList(ShaderProgram program)
        {
            GL.GetProgram(program.Handle, GetProgramParameterName.ActiveUniformBlocks, out int length);
            uniforms = new ShaderBlockUniform[length];
            
            for(int i=0; i<length; i++)
            {
                GL.GetActiveUniformBlock(program.Handle, i, ActiveUniformBlockParameter.UniformBlockNameLength, out int nameLength);
                GL.GetActiveUniformName(program.Handle, i, nameLength, out int actualNameLength, out string name);
                GL.GetActiveUniformBlock(program.Handle, i, ActiveUniformBlockParameter.UniformBlockBinding, out int bindingIndex);

                uniforms[i] = new ShaderBlockUniform(program, bindingIndex, name);
            }
        }

        public void EnsureAllSet()
        {
            //TODO: everything
        }

        public override string ToString()
        {
            return String.Concat("ShaderBlockUniformList with ", uniforms.Length, " uniform blocks");
        }

    }
}
