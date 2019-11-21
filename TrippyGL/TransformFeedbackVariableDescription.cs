using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    /// <summary>
    /// Represents a transform feedback variable for a TransformFeedbackObject.
    /// Used to tell the object what the transform feedback's output variables are and how and where they should be stored.
    /// </summary>
    public struct TransformFeedbackVariableDescription
    {
        /// <summary>The buffer subset to which this variable applies.</summary>
        public readonly BufferObjectSubset BufferSubset;

        /// <summary>The type of this variable or, in the case this is a padding indicator, an integer with the amount of components to skip.</summary>
        public readonly TransformFeedbackType Type;

        /// <summary>The total amount of components (4 bytes) used by this variable.</summary>
        public readonly int ComponentCount;

        /// <summary>Whether this descriptor doesn't represent an actual variable but just indicates padding.</summary>
        public bool IsPadding { get { return (int)Type > 0 && (int)Type < 5; } }

        /// <summary>The amount of components to pad, if this is a padding descriptor.</summary>
        public int PaddingComponentCount { get { return ComponentCount; } }

        /// <summary>
        /// Creates a TransformFeedbackVariableDescription that describes a variable of a specified type to be written to a specified buffer subset.
        /// </summary>
        /// <param name="bufferSubset">The buffer subset where the variable should be written.</param>
        /// <param name="variableType">The type of the variable.</param>
        public TransformFeedbackVariableDescription(BufferObjectSubset bufferSubset, TransformFeedbackType variableType)
        {
            if (!Enum.IsDefined(typeof(TransformFeedbackType), variableType))
                throw new FormatException("The specified variable type is invalid");

            BufferSubset = bufferSubset;
            Type = variableType;
            ComponentCount = TrippyUtils.GetTransformFeedbackTypeComponentCount(variableType);
        }

        /// <summary>
        /// Creates a TransformFeedbackVariableDescription that describes padding in a specified buffer.
        /// </summary>
        /// <param name="bufferSubset">The buffer subset where the variable should be written.</param>
        /// <param name="paddingComponents">The amount of components to skip writing to.</param>
        public TransformFeedbackVariableDescription(BufferObjectSubset bufferSubset, int paddingComponents)
        {
            if (paddingComponents < 1 || paddingComponents > 4)
                throw new ArgumentOutOfRangeException("paddingComponents", paddingComponents, "paddingComponents must be in the range [1, 4]");

            // The value of the "Type" variable is what differentiates padding descriptors from descriptors that describe actual variables.
            // If Type is in the range of [1, 4] (the range of a valid paddingComponents value), then it's a padding descriptor.
            // Still, you shouldn't read Type but rather use the public bool IsPadding { get; } and public int PaddingComponentCount { get; }

            BufferSubset = bufferSubset;
            Type = (TransformFeedbackType)paddingComponents;
            ComponentCount = paddingComponents;
        }

        public override string ToString()
        {
            if (IsPadding)
                return string.Concat("BufferSubset.BufferHandle=", BufferSubset.BufferHandle.ToString(), ", PaddingComponents=", PaddingComponentCount.ToString());
            return string.Concat("BufferSubset.BufferHandle=", BufferSubset.BufferHandle.ToString(), ", Type=", Type.ToString());
        }
    }
}
