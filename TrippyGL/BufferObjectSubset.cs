using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public abstract class BufferObjectSubset
    {
        public readonly BufferObject Buffer;

        public readonly BufferTarget Target;

        public int StorageLengthInBytes { get; private protected set; }

        internal readonly int bufferTargetBindingIndex;

        internal BufferObjectSubset(BufferObject bufferObject, BufferTarget bufferTarget)
        {
            bufferTargetBindingIndex = bufferObject.GraphicsDevice.GetBindingTargetIndex(bufferTarget);
            if (bufferTargetBindingIndex < 0)
                throw new FormatException("The given bufferTarget for this BufferObjectSubset is invalid");

            this.Buffer = bufferObject;
            this.Target = bufferTarget;
        }
    }
}
