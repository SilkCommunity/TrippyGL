using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public class UniformBufferObject<T> : BufferObject, IBufferRangeBindable where T : struct
    {
        private protected static int[] bindingHandles;
        private protected static int[] bindingOffsets;
        private protected static int[] bindingSizes;

        private static int uniformOffsetAlignment;
        public static int UniformOffsetAlignment { get; private set; }
        public static void Init1()
        {
            uniformOffsetAlignment = TrippyLib.UniformBufferOffsetAlignment;

            bindingHandles = new int[TrippyLib.MaxUniformBufferBindings];
            bindingOffsets = new int[bindingHandles.Length];
            bindingSizes = new int[bindingHandles.Length];
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

            EnsureBound();
            GL.BufferData(this.BufferTarget, storageLengthBytes, ref initialValue, usageHint);
        }

        public UniformBufferObject(int storageLength, int dataOffset, T[] data, BufferUsageHint usageHint) : base(BufferTarget.UniformBuffer)
        {
            this.storageLength = storageLength;
            this.elementSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
            uniformOffsetAlignment = TrippyLib.UniformBufferOffsetAlignment; //the static variable changes on it's own the compiler is shit
            this.elementStride = (this.elementSize + uniformOffsetAlignment - 1) / uniformOffsetAlignment * uniformOffsetAlignment;
            this.storageLengthBytes = this.elementStride * storageLength;

            ValidateInitWithDataParams(storageLength, dataOffset, data);

            EnsureBound();
            GL.BufferData(this.BufferTarget, storageLength * elementSize, IntPtr.Zero, usageHint);
            
            //set data?
        }

        public void BindRange(int bindingIndex, int elementIndex = 0)
        {
            int offset = this.elementStride * elementIndex;
            GL.BindBufferRange(BufferRangeTarget.UniformBuffer, bindingIndex, this.Handle, (IntPtr)(this.elementStride * elementIndex), this.elementSize);

            binds[bindingIndex] = this.Handle;
            bindingHandles[bindingIndex] = this.Handle;
            bindingOffsets[bindingIndex] = offset;
            bindingSizes[bindingIndex] = this.elementSize;
        }

        public void EnsureBoundRange(int bindingIndex, int elementIndex = 0)
        {
            Init1();
            if (bindingHandles[bindingIndex] != this.Handle)
            {
                int offset = this.elementStride * elementIndex;
                if (bindingOffsets[bindingIndex] != offset || bindingSizes[bindingIndex] != this.elementSize)
                    BindRange(bindingIndex, elementIndex);
            }
        }

        public void SetValue(T value, int index = 0)
        {
            EnsureBound();
            GL.BufferSubData(this.BufferTarget, (IntPtr)(index * this.elementStride), this.elementSize, ref value);
        }
    }
}
