using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Stores the output transform feedback variables of a ShaderProgram and the transform feedback configuration
    /// </summary>
    public class TransformFeedbackProgramVariableList
    {
        private readonly TransformFeedbackProgramVariable[] variables;

        /// <summary>The output mode of the ShaderProgram's transform feedback</summary>
        public readonly TransformFeedbackMode TransformFeedbackMode;

        /// <summary>The amount of transform feedback variables the ShaderProgram outputs</summary>
        public int Count { get { return variables.Length; } }

        /// <summary>
        /// Gets a transform feedback variable
        /// </summary>
        /// <param name="index">The index of the variable from [0, this.Count)</param>
        public TransformFeedbackProgramVariable this[int index] { get { return variables[index]; } }

        internal TransformFeedbackProgramVariableList(ShaderProgram program)
        {
            GL.GetProgram(program.Handle, GetProgramParameterName.TransformFeedbackBufferMode, out int bufferMode);
            TransformFeedbackMode = (TransformFeedbackMode)bufferMode;

            GL.GetProgram(program.Handle, GetProgramParameterName.TransformFeedbackVaryingMaxLength, out int maxNameLength);
            GL.GetProgram(program.Handle, GetProgramParameterName.TransformFeedbackVaryings, out int varyingCount);
            List<TransformFeedbackProgramVariable> variableList = new List<TransformFeedbackProgramVariable>(varyingCount);

            int bufferIndex = 0;
            for (int i = 0; i < varyingCount; i++)
            {
                GL.GetTransformFeedbackVarying(program.Handle, i, maxNameLength, out int nameLength, out int size, out TransformFeedbackType type, out string name);
                if (name == "gl_NextBuffer")
                    bufferIndex++;
                else
                    variableList.Add(new TransformFeedbackProgramVariable(bufferIndex, size, type, name));
            }
            variables = variableList.ToArray();
        }

        internal bool DoVariablesMatch(string[] givenNames)
        {
            if (variables.Length == 0)
                return givenNames.Length == 0;  // For transform feedback we're not gonna support declaring but not using a variable
                                                // What am I supposed to do? Leave it as it is or somehow add padding? That would require re-linking!
                                                
            int variableIndex = 0;

            for (int i = 0; i < givenNames.Length; i++)
            {
                while (variables[variableIndex++].Name != givenNames[i])
                {
                    if (variableIndex >= variables.Length)
                        return false;
                }
            }
            return true;
        }
    }
}
