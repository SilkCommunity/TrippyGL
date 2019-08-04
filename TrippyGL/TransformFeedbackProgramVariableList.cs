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
        /// <summary>The array that stores the TransformFeedbackProgramVariable-s</summary>
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

        /// <summary>
        /// Creates a TransformFeedbackProgramVariableList where the variables are queried from the given ShaderProgram
        /// </summary>
        /// <param name="program">The ShaderProgram to query the variables from</param>
        internal TransformFeedbackProgramVariableList(ShaderProgram program)
        {
            // We query a bunch of info from the ShaderProgram's transform feedback configuration
            GL.GetProgram(program.Handle, GetProgramParameterName.TransformFeedbackBufferMode, out int bufferMode);
            TransformFeedbackMode = (TransformFeedbackMode)bufferMode;

            GL.GetProgram(program.Handle, GetProgramParameterName.TransformFeedbackVaryingMaxLength, out int maxNameLength);
            GL.GetProgram(program.Handle, GetProgramParameterName.TransformFeedbackVaryings, out int varyingCount);

            // We'll use a list and turn it into an array afterwards, any gl_NextBuffer variables shouldn't be added
            // so the final variable count might be less than varyingCount
            List<TransformFeedbackProgramVariable> variableList = new List<TransformFeedbackProgramVariable>(varyingCount);

            // The buffer index into which each variable goes
            int bufferIndex = 0;
            for (int i = 0; i < varyingCount; i++)
            {
                // Query the information from the varying variable
                GL.GetTransformFeedbackVarying(program.Handle, i, maxNameLength, out int nameLength, out int size, out TransformFeedbackType type, out string name);

                if (name == "gl_NextBuffer") // If it's gl_NextBuffer, we'll just increment bufferIndex
                    bufferIndex++; // So the next variable will have stored that it's going to the following index
                else // Else, we add the variable into the list. It might be named "gl_SkipComponents#" though
                    variableList.Add(new TransformFeedbackProgramVariable(bufferIndex, size, type, name));
            }

            variables = variableList.ToArray();
        }

        /// <summary>
        /// Checks that the variables in this list match the ones from the provided names
        /// </summary>
        /// <param name="givenNames"></param>
        internal bool DoVariablesMatch(string[] givenNames)
        {
            if (variables.Length == 0)
                return givenNames.Length == 0;  // For transform feedback we're not gonna support declaring but not using a variable
                                                // What am I supposed to do? Leave it as it is or somehow add padding? That would require re-linking!
                                                
            int variableIndex = 0; // The index we'll read from the "variables" array.
            // We'll increment this after each variable we check, whether the check was successfull or not.

            for (int i = 0; i < givenNames.Length; i++)
            {
                while (variables[variableIndex++].Name != givenNames[i])
                {
                    if (variableIndex >= variables.Length)
                        return false;
                }
            }

            return true; // We got to the end? There were no mismatches!
        }
    }
}
