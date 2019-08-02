using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Represents an active output transform feedback variable declared on a shader
    /// </summary>
    public struct TransformFeedbackProgramVariable
    {
        /// <summary>The buffer binding index to which this variable will be saved</summary>
        public readonly int BufferIndex;

        /// <summary>The size of the variable. Used for array length</summary>
        public readonly int Size;

        /// <summary>The type of this variable</summary>
        public readonly TransformFeedbackType Type;

        /// <summary>The name of the variable</summary>
        public readonly string Name;

        internal TransformFeedbackProgramVariable(int bufferIndex, int size, TransformFeedbackType type, string name)
        {
            BufferIndex = bufferIndex;
            Size = size;
            Type = type;
            Name = name;
        }

        public override string ToString()
        {
            return String.Concat("BufferIndex=", BufferIndex.ToString(), ", Size=", Size.ToString(), ", Type=", Type.ToString(), ", Name=\"", Name, "\"");
        }
    }
}
