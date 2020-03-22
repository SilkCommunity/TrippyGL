using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// A <see cref="BufferObjectSubset"/> whose purpose is to store uniform block values for shaders to read from.
    /// </summary>
    public abstract class UniformBufferSubset : BufferObjectSubset
    {
        /// <summary>The size of a single uniform block value, measured in bytes.</summary>
        public readonly uint ElementSize;

        /// <summary>The offset between the start of a uniform value and the start of the next.</summary>
        public readonly uint ElementStride;

        /// <summary>The amount of values this subset is able to store.</summary>
        public uint StorageLength { get; private set; }

        internal UniformBufferSubset(uint elementSize, BufferObject bufferObject, uint storageOffsetBytes, uint storageLength)
            : base(bufferObject, BufferTargetARB.UniformBuffer)
        {
            ElementSize = elementSize;
            uint uniformOffsetAlignment = (uint)bufferObject.GraphicsDevice.UniformBufferOffsetAlignment;
            ElementStride = (ElementSize + uniformOffsetAlignment - 1) / uniformOffsetAlignment * uniformOffsetAlignment;
            ResizeSubset(storageOffsetBytes, storageLength);
        }

        internal UniformBufferSubset(uint elementSize, BufferObject bufferObject)
            : base(bufferObject, BufferTargetARB.UniformBuffer)
        {
            ElementSize = elementSize;
            uint uniformOffsetAlignment = (uint)bufferObject.GraphicsDevice.UniformBufferOffsetAlignment;
            ElementStride = (ElementSize + uniformOffsetAlignment - 1) / uniformOffsetAlignment * uniformOffsetAlignment;

            uint storageLength = bufferObject.StorageLengthInBytes / ElementStride;
            if (storageLength == 0)
            {
                storageLength = bufferObject.StorageLengthInBytes / ElementSize;
                if (storageLength == 0)
                    throw new ArgumentException(nameof(bufferObject) + " must have enough capacity for at least one uniform", nameof(bufferObject));
            }

            ResizeSubset(0, storageLength);
        }

        /// <summary>
        /// Changes the subset location of this <see cref="UniformBufferSubset{T}"/>.
        /// </summary>
        /// <param name="storageOffsetBytes">The offset into the <see cref="BufferObject"/>'s storage where this subset begins. Must be a multiple of <see cref="GraphicsDevice.UniformBufferOffsetAlignment"/>.</param>
        /// <param name="storageLength">The length of this subset measured in elements. The final length in bytes may vary between machines.</param>
        public void ResizeSubset(uint storageOffsetBytes, uint storageLength)
        {
            if (storageOffsetBytes % Buffer.GraphicsDevice.UniformBufferOffsetAlignment != 0)
                throw new ArgumentException(nameof(storageOffsetBytes) + " must be a multiple of " + nameof(GraphicsDevice.UniformBufferOffsetAlignment), nameof(storageOffsetBytes));

            InitializeStorage(storageOffsetBytes, (storageLength - 1) * ElementStride + ElementSize);
            StorageLength = storageLength;
        }

        /// <summary>
        /// Binds a specified part of this subset to be used as uniform values.
        /// </summary>
        /// <param name="bindingIndex">The binding index this subset will bind to.</param>
        /// <param name="storageOffsetBytes">The offset into this subset's storage where the bind begins.</param>
        /// <param name="storageLengthBytes">The amount of bytes available to be read from the binded buffer.</param>
        internal void BindBufferRange(uint bindingIndex, uint storageOffsetBytes, uint storageLengthBytes)
        {
            Buffer.GraphicsDevice.BindBufferRange(this, bindingIndex, storageOffsetBytes + StorageOffsetInBytes, storageLengthBytes);
        }

        /// <summary>
        /// Gets the offset and length variables to use on <see cref="BindBufferRange(uint, uint, uint)"/>
        /// to bind a specific index of this buffer.
        /// </summary>
        /// <param name="elementIndex">The index of the element in this buffer to.</param>
        /// <param name="storageOffsetBytes">The offset into this subset's storage where the bind should start measured in bytes.</param>
        /// <param name="storageLengthBytes">The length of the ranged bind measured in bytes.</param>
        internal void GetOffsetAndStorageLengthForIndex(uint elementIndex, out uint storageOffsetBytes, out uint storageLengthBytes)
        {
            storageOffsetBytes = elementIndex * ElementStride;
            storageLengthBytes = ElementSize;
        }

        /// <summary>
        /// Calculates the required storage length in bytes required for a UniformBufferSubset with the specified storage length.
        /// </summary>
        /// <typeparam name="U">The struct type to use for the uniform block. This must match the uniform block's format.</typeparam>
        /// <param name="graphicsDevice">The GraphicsDevice the BufferObject will use.</param>
        /// <param name="storageLength">The amount of structs the <see cref="UniformBufferSubset{T}"/> will store.</param>
        public static uint CalculateRequiredSizeInBytes<T>(GraphicsDevice graphicsDevice, uint storageLength) where T : unmanaged
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            uint elementSize = (uint)Marshal.SizeOf<T>();
            uint uniformOffsetAlignment = (uint)graphicsDevice.UniformBufferOffsetAlignment;
            uint elementStride = (elementSize + uniformOffsetAlignment - 1) / uniformOffsetAlignment * uniformOffsetAlignment;
            return (storageLength - 1) * elementStride + elementSize;
        }
    }

    /// <summary>
    /// A <see cref="BufferObjectSubset"/> whose purpose is to store uniform block values for shaders to read from.
    /// </summary>
    /// <typeparam name="T">The type of sturct the uniform block will use. This must match the uniform block's format</typeparam>
    public sealed class UniformBufferSubset<T> : UniformBufferSubset where T : unmanaged
    {
        /// <summary>
        /// Creates a <see cref="UniformBufferSubset{T}"/> with the given <see cref="BufferObject"/>,
        /// offset into the buffer in bytes and storage length in elements.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        /// <param name="storageOffsetBytes">The offset into the <see cref="BufferObject"/>'s storage where this subset begins. Must be a multiple of <see cref="GraphicsDevice.UniformBufferOffsetAlignment"/>.</param>
        /// <param name="storageLength">The amount of elements this <see cref="UniformBufferSubset{T}"/> will be able to store.</param>
        public UniformBufferSubset(BufferObject bufferObject, uint storageOffsetBytes, uint storageLength)
            : base((uint)Marshal.SizeOf<T>(), bufferObject, storageOffsetBytes, storageLength)
        {

        }

        /// <summary>
        /// Creates a <see cref="UniformBufferSubset{T}"/> with the given <see cref="UniformBufferSubset{T}"/>,
        /// with the subset covering the entire <see cref="BufferObject"/>'s storage.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        public UniformBufferSubset(BufferObject bufferObject)
            : base((uint)Marshal.SizeOf<T>(), bufferObject)
        {

        }

        /// <summary>
        /// Sets one of the uniform values stored in this <see cref="UniformBufferSubset{T}"/>.
        /// </summary>
        /// <param name="value">The value to store on the uniform buffer.</param>
        /// <param name="index">The index in which to store the uniform value.</param>
        public unsafe void SetValue(in T value, int index = 0)
        {
            if (index < 0 || index > StorageLength)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be in the range [0, " + nameof(StorageLength) + ")");

            Buffer.GraphicsDevice.BindBuffer(this);
            fixed (void* ptr = &value)
                Buffer.GL.BufferSubData(BufferTarget, (int)(index * ElementStride + StorageOffsetInBytes), ElementSize, ptr);
        }

        /// <summary>
        /// Gets a value from the buffer's storage.
        /// </summary>
        /// <param name="value">The value read from the buffer.</param>
        /// <param name="index">The array index of the value to read.</param>
        public unsafe void GetValue(out T value, int index = 0)
        {
            if (index < 0 || index > StorageLength)
                throw new ArgumentOutOfRangeException(nameof(index), index, nameof(index) + " must be in the range [0, " + nameof(StorageLength) + ")");

            Buffer.GraphicsDevice.BindBuffer(this);
            value = default;
            fixed (void* ptr = &value)
                Buffer.GL.GetBufferSubData(BufferTarget, (int)(index * ElementStride + StorageOffsetInBytes), ElementSize, ptr);
        }

        /// <summary>
        /// Gets a value from the subset's storage.
        /// </summary>
        /// <param name="index">The array index of the value to read.</param>
        public T GetValue(int index = 0)
        {
            GetValue(out T value, index);
            return value;
        }
    }
}
