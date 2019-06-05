using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public class UniformBufferObject<T> : BufferObject, IBufferRangeBindable where T : struct
    {
        private int elementSize;
        private int elementStride;
        private int storageLength;
        private int storageLengthBytes;

        /// <summary>The size of each element in the buffer object's storage measured in bytes</summary>
        public override int ElementSize { get { return elementSize; } }

        /// <summary>The offset in bytes between the start of a value and the start of the next in the buffer's storage</summary>
        public int ElementStride { get { return elementStride; } }

        /// <summary>The amount of values this UniformBufferObject's storage can hold (as uniforms)</summary>
        public override int StorageLength { get { return storageLength; } }

        /// <summary>The length of the buffer object's storage, measured in bytes</summary>
        public override int StorageLengthInBytes { get { return storageLengthBytes; } }

        /// <summary>
        /// Creates a UniformBufferObject with a storage length of 1, able to hold one uniform
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="initialValue">The initial value of the uniform</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public UniformBufferObject(GraphicsDevice graphicsDevice, T initialValue, BufferUsageHint usageHint)
            : base(graphicsDevice, BufferTarget.UniformBuffer)
        {
            this.storageLength = 1;
            this.elementSize = System.Runtime.InteropServices.Marshal.SizeOf<T>(initialValue);
            this.storageLengthBytes = this.elementSize;
            int uniformOffsetAlignment = graphicsDevice.UniformBufferOffsetAlignment;
            this.elementStride = (this.elementSize + uniformOffsetAlignment - 1) / uniformOffsetAlignment * uniformOffsetAlignment;

            GraphicsDevice.EnsureBufferBound(this);
            GL.BufferData(this.BufferTarget, storageLengthBytes, ref initialValue, usageHint);
        }

        /// <summary>
        /// Creates a UniformBufferObject with the specified storage length, but the storage is not initialized to any value
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="storageLength">The amount of elements the buffer will be able to hold</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public UniformBufferObject(GraphicsDevice graphicsDevice, int storageLength, BufferUsageHint usageHint)
            : base(graphicsDevice, BufferTarget.UniformBuffer)
        {
            this.storageLength = storageLength;
            this.elementSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
            this.storageLengthBytes = this.elementSize;
            int uniformOffsetAlignment = graphicsDevice.UniformBufferOffsetAlignment;
            this.elementStride = (this.elementSize + uniformOffsetAlignment - 1) / uniformOffsetAlignment * uniformOffsetAlignment;

            GraphicsDevice.EnsureBufferBound(this);
            GL.BufferData(this.BufferTarget, storageLengthBytes, IntPtr.Zero, usageHint);
        }

        /// <summary>
        /// Creates a UniformBufferObject with the specified storage length and initial values given from a specified index from an array
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="storageLength">The amount of elements the buffer will be able to hold</param>
        /// <param name="dataOffset">The offset into the array where the first element is found</param>
        /// <param name="data">The array where the initial values are found</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public UniformBufferObject(GraphicsDevice graphicsDevice, int storageLength, int dataOffset, T[] data, BufferUsageHint usageHint)
            : base(graphicsDevice, BufferTarget.UniformBuffer)
        {
            this.storageLength = storageLength;
            this.elementSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
            int uniformOffsetAlignment = graphicsDevice.UniformBufferOffsetAlignment;
            this.elementStride = (this.elementSize + uniformOffsetAlignment - 1) / uniformOffsetAlignment * uniformOffsetAlignment;
            this.storageLengthBytes = (storageLength - 1) * this.elementStride + this.elementSize;

            ValidateInitWithDataParams(storageLength, dataOffset, data);

            GraphicsDevice.EnsureBufferBound(this);
            GL.BufferData(this.BufferTarget, storageLengthBytes, IntPtr.Zero, usageHint);

            for (int i = 0; i < storageLength; i++)
                SetValue(data[i], i);
        }

        /// <summary>
        /// Creates a UniformBufferObject with the specified storage length and initial values given from an array
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="storageLength">The amount of elements the buffer will be able to hold</param>
        /// <param name="data">The array where the initial values are found</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public UniformBufferObject(GraphicsDevice graphicsDevice, int storageLenth, T[] data, BufferUsageHint usageHint)
            : this(graphicsDevice, storageLenth, 0, data, usageHint)
        {

        }

        /// <summary>
        /// Sets one of the uniform values stored in this buffer
        /// </summary>
        /// <param name="value">The value to store on the uniform buffer</param>
        /// <param name="index">The index in which to store the uniform value</param>
        public void SetValue(T value, int index = 0)
        {
            if (index < 0 || index > storageLength)
                throw new ArgumentOutOfRangeException("index", index, "Index must be in the range [0, StorageLength)");

            GraphicsDevice.EnsureBufferBound(this);
            GL.BufferSubData(this.BufferTarget, (IntPtr)(index * this.elementStride), this.elementSize, ref value);
        }

        /// <summary>
        /// Ensures this buffer is bound to GL_UNIFORM_BUFFER at the specified bindingIndex.
        /// This binds a range in the buffer's storage enough to read only one element
        /// </summary>
        /// <param name="bindingIndex">The binding index in GL_UNIFORM_BUFFER to ensure is bound</param>
        /// <param name="elementIndex">The uniform element index from this buffer to bind</param>
        public void EnsureBoundRange(int bindingIndex, int elementIndex = 0)
        {
            // We do this because offset must be a multiple of GL_UNIFORM_BUFFER_OFFSET_ALIGNMENT, so that's also why
            // you can't just simply set this buffer object's value as an array. It would have to be formatted to fit

            GraphicsDevice.EnsureBufferBoundRange(this, bindingIndex, elementIndex * this.ElementStride, this.elementSize);
        }
    }
}
