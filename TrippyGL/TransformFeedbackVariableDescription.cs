using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Represents a transform feedback variable for a TransformFeedbackObject.
    /// Used to tell the object what the transform feedback's output variables are and how and where they should be stored
    /// </summary>
    public struct TransformFeedbackVariableDescription
    {
        /// <summary>The buffer subset to which this variable applies</summary>
        public readonly BufferObjectSubset BufferSubset;

        /// <summary>The type of this variable or, in the case this is a padding indicator, an integer with the amount of components to skip</summary>
        public readonly TransformFeedbackType Type;

        public readonly int ComponentCount;

        /// <summary>Whether this descriptor doesn't represent an actual variable but just indicates padding</summary>
        public bool IsPadding { get { return (int)Type > 0 && (int)Type < 5; } }

        /// <summary>The amount of components to pad, if this is a padding descriptor</summary>
        public int PaddingComponentCount { get { return (int)Type; } }

        public TransformFeedbackVariableDescription(BufferObjectSubset bufferSubset, TransformFeedbackType type)
        {
            BufferSubset = bufferSubset;
            Type = type;
            ComponentCount = TrippyUtils.GetTransformFeedbackTypeComponentCount(type);
        }

        public TransformFeedbackVariableDescription(BufferObjectSubset bufferSubset, int paddingComponents)
        {
            if (paddingComponents < 1 || paddingComponents > 4)
                throw new ArgumentOutOfRangeException("paddingComponents", paddingComponents, "paddingComponents must be in the range [1, 4]");

            BufferSubset = bufferSubset;
            Type = (TransformFeedbackType)paddingComponents;
            ComponentCount = paddingComponents;
        }

        public override string ToString()
        {
            return String.Concat("BufferSubset.BufferHandle=", BufferSubset.BufferHandle.ToString(), ", Type=", Type.ToString());
        }
    }
}
