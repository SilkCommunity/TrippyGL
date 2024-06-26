using System;

namespace TrippyGL
{
    /// <summary>
    /// A buffer-backed block of uniforms. That is, a block of uniforms that get their value from a <see cref="BufferObject"/>.
    /// </summary>
    public sealed class ShaderBlockUniform
    {
        /// <summary>The name with which this <see cref="ShaderBlockUniform"/> is declared in the shader.</summary>
        public readonly string Name;

        /// <summary>The <see cref="ShaderProgram"/> that owns this uniform.</summary>
        public readonly ShaderProgram OwnerProgram;

        /// <summary>The binding index in the uniform buffer target from which this uniform reads buffer data.</summary>
        public readonly uint BindingIndex;

        /// <summary>The amount of active uniforms this uniform block contains.</summary>
        public readonly int ActiveUniformCount;

        private UniformBufferSubset? uniformSource;
        private uint uniformBindOffsetBytes, uniformBindLengthBytes;

        internal ShaderBlockUniform(ShaderProgram owner, uint bindingIndex, string name, int activeUniformCount)
        {
            OwnerProgram = owner;
            BindingIndex = bindingIndex;
            Name = name;
            ActiveUniformCount = activeUniformCount;
        }

        /// <summary>
        /// Sets the buffer containing the values for this uniform.
        /// </summary>
        /// <param name="buffer">The buffer from which the values will be read.</param>
        /// <param name="elementIndex">The index of the element in the buffer subset whose value should be used.</param>
        public void SetValue(UniformBufferSubset buffer, uint elementIndex = 0)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (elementIndex < 0 || elementIndex > buffer.StorageLength)
                throw new ArgumentOutOfRangeException(nameof(elementIndex), nameof(elementIndex) + " must be in the range [0, " + nameof(buffer.StorageLength) + ")");

            uniformSource = buffer;
            buffer.GetOffsetAndStorageLengthForIndex(elementIndex, out uniformBindOffsetBytes, out uniformBindLengthBytes);
        }

        /// <summary>
        /// This is called by <see cref="ShaderBlockUniformList.EnsureBufferBindingsSet"/> to ensure the buffer backing
        /// this uniform block is bound to the right index and range on the uniform buffer target.
        /// </summary>
        internal void ApplyUniformValue()
        {
            uniformSource?.BindBufferRange(BindingIndex, uniformBindOffsetBytes, uniformBindLengthBytes);
        }

        public override string ToString()
        {
            return string.Concat(
                nameof(Name) + "=\"", Name, "\"",
                ", " + nameof(BindingIndex) + "=", BindingIndex.ToString(),
                ", " + nameof(ActiveUniformCount) + "=", ActiveUniformCount.ToString()
            );
        }
    }
}
