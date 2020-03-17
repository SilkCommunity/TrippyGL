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
        public readonly int BindingIndex;

        /// <summary>The amount of active uniforms this uniform block contains.</summary>
        public readonly int ActiveUniformCount;

        private IBufferRangeBindable uniformSource;
        private int uniformBindOffsetBytes, uniformBindLengthBytes;

        internal ShaderBlockUniform(ShaderProgram owner, int bindingIndex, string name, int activeUniformCount)
        {
            OwnerProgram = owner;
            BindingIndex = bindingIndex;
            Name = name;
            ActiveUniformCount = activeUniformCount;
        }

        /// <summary>
        /// Sets the buffer containing the values for this uniform.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as the uniform.</typeparam>
        /// <param name="buffer">The buffer from which the values will be read.</param>
        /// <param name="elementIndex">The index of the element in the buffer subset whose value should be used.</param>
        public void SetValue<T>(UniformBufferSubset<T> buffer, int elementIndex = 0) where T : struct
        {
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
            if (uniformSource != null)
                uniformSource.BindBufferRange(BindingIndex, uniformBindOffsetBytes, uniformBindLengthBytes);
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
