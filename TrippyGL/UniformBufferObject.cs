using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public class UniformBufferObject<T> : BufferObject, IBufferRangeBindable where T : struct
    {
        private static int uniformOffsetAlignment;
        public static int UniformOffsetAlignment { get; private set; }
        public static void Init1()
        {
            uniformOffsetAlignment = TrippyLib.UniformBufferOffsetAlignment;
        }

        private int elementSize;
        private int elementStride;
        private int storageLength;
        private int storageLengthBytes;

        public override int ElementSize { get { return elementSize; } }

        public int ElementStride { get { return elementStride; } }

        public override int StorageLength { get { return storageLength; } }

        public override int StorageLengthInBytes { get { return storageLengthBytes; } }

        public UniformBufferObject(T initialValue, BufferUsageHint usageHint) : base(BufferTarget.UniformBuffer)
        {
            this.storageLength = 1;
            this.elementSize = System.Runtime.InteropServices.Marshal.SizeOf<T>(initialValue);
            this.storageLengthBytes = this.elementSize;
            uniformOffsetAlignment = TrippyLib.UniformBufferOffsetAlignment; //the static variable changes on it's own the compiler is shit
            this.elementStride = (this.elementSize + uniformOffsetAlignment - 1) / uniformOffsetAlignment * uniformOffsetAlignment;

            ValidateInitWithoutDataParams(storageLengthBytes);

            States.EnsureBufferBound(this);
            GL.BufferData(this.BufferTarget, storageLengthBytes, ref initialValue, usageHint);
        }

        public UniformBufferObject(int storageLength, int dataOffset, T[] data, BufferUsageHint usageHint) : base(BufferTarget.UniformBuffer)
        {
            this.storageLength = storageLength;
            this.elementSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
            uniformOffsetAlignment = TrippyLib.UniformBufferOffsetAlignment; //the static variable changes on it's own the compiler is shit
            this.elementStride = (this.elementSize + uniformOffsetAlignment - 1) / uniformOffsetAlignment * uniformOffsetAlignment;
            this.storageLengthBytes = (storageLength - 1) * this.elementStride + this.elementSize;

            ValidateInitWithDataParams(storageLength, dataOffset, data);

            States.EnsureBufferBound(this);
            GL.BufferData(this.BufferTarget, storageLengthBytes, IntPtr.Zero, usageHint);

            for (int i = 0; i < storageLength; i++)
                SetValue(data[i], i);
        }

        public void SetValue(T value, int index = 0)
        {
            States.EnsureBufferBound(this);
            GL.BufferSubData(this.BufferTarget, (IntPtr)(index * this.elementStride), this.elementSize, ref value);
        }

        public void EnsureBoundRange(int bindingIndex, int elementIndex = 0)
        {
            States.EnsureBufferBoundRange(this, bindingIndex, elementIndex * this.ElementStride, this.elementSize);
        }
    }
}
