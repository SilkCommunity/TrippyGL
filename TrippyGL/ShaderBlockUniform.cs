using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public class ShaderBlockUniform
    {
        public readonly string Name;

        public readonly ShaderProgram OwnerProgram;

        public readonly int BindingIndex;

        private IBufferRangeBindable uniformSource;
        private int elementIndex;

        internal ShaderBlockUniform(ShaderProgram owner, int bindingIndex, string name)
        {
            this.OwnerProgram = owner;
            this.BindingIndex = bindingIndex;
            this.Name = name;
        }

        public void SetValue<T>(UniformBufferObject<T> value, int elementIndex = 0) where T : struct
        {
            //if (value == null)
            //    throw new ArgumentNullException("value");

            if (elementIndex < 0 || elementIndex > value.StorageLength)
                throw new ArgumentOutOfRangeException("valueIndex", "Value index must be in the range [0, value.StorageLength)");

            uniformSource = value;
            this.elementIndex = elementIndex;

            if (OwnerProgram.IsCurrentlyInUse)
                uniformSource.EnsureBoundRange(BindingIndex, elementIndex);

        }

        public override string ToString()
        {
            return String.Concat("Name=", Name);
        }
    }
}
