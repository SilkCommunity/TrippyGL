using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace TrippyGL
{
    /// <summary>
    /// Provides a limited but much simpler way to store vertex data in a single <see cref="BufferObject"/>.
    /// </summary>
    /// <typeparam name="T">The type of vertex to use. Must be a struct and implement <see cref="IVertex"/>.</typeparam>
    public readonly struct VertexBuffer<T> : IDisposable, IEquatable<VertexBuffer<T>> where T : struct, IVertex
    {
        // TODO: Add support for an Index Buffer

        /// <summary>The <see cref="BufferObject"/> that stores all the vertex data.</summary>
        public readonly BufferObject Buffer;

        /// <summary>The <see cref="VertexDataBufferSubset{T}"/> that manages the storage from <see cref="Buffer"/>.</summary>
        public readonly VertexDataBufferSubset<T> DataSubset;

        /// <summary>The <see cref="VertexArray"/> that defines how the vertex attributes are read.</summary>
        public readonly VertexArray VertexArray;

        /// <summary>The size of a single vertex measured in bytes.</summary>
        public readonly int ElementSize;

        /// <summary>The length of the buffer's storage measured in vertices.</summary>
        public int StorageLength => DataSubset.StorageLength;

        /// <summary>
        /// Creates a <see cref="VertexBuffer{T}"/> with specified initial data and length.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="storageLength">The desired length for the buffer's storage measured in vertices.</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object.</param>
        /// <param name="data">A <see cref="Span{T}"/> containing the initial buffer data.</param>
        /// <param name="dataWriteOffset">The offset into the subset's storage at which to start writting the initial data.</param>
        public VertexBuffer(GraphicsDevice graphicsDevice, int storageLength, BufferUsageHint usageHint, Span<T> data, int dataWriteOffset = 0)
        {
            ValidateStorageLength(storageLength);
            ElementSize = Marshal.SizeOf<T>();

            Buffer = new BufferObject(graphicsDevice, storageLength * ElementSize, usageHint);
            DataSubset = new VertexDataBufferSubset<T>(Buffer);
            DataSubset.SetData(data, dataWriteOffset);
            VertexArray = VertexArray.CreateSingleBuffer<T>(graphicsDevice, DataSubset);
        }

        /// <summary>
        /// Creates a <see cref="VertexBuffer{T}"/> with specified initial data and same length as that data <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="data">A <see cref="Span{T}"/> containing the initial buffer data.</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object.</param>
        public VertexBuffer(GraphicsDevice graphicsDevice, Span<T> data, BufferUsageHint usageHint)
            : this(graphicsDevice, data.Length, usageHint, data)
        {

        }

        /// <summary>
        /// Creates a <see cref="VertexBuffer{T}"/> with specified length and undefined initial data.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="storageLength">The desired length for the buffer's storage measured in vertices.</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object.</param>
        public VertexBuffer(GraphicsDevice graphicsDevice, int storageLength, BufferUsageHint usageHint)
        {
            ValidateStorageLength(storageLength);
            ElementSize = Marshal.SizeOf<T>();

            Buffer = new BufferObject(graphicsDevice, storageLength * ElementSize, usageHint);
            DataSubset = new VertexDataBufferSubset<T>(Buffer);
            VertexArray = VertexArray.CreateSingleBuffer<T>(graphicsDevice, DataSubset);
        }

        public static implicit operator VertexArray(VertexBuffer<T> vertexBuffer) => vertexBuffer.VertexArray;

        public static bool operator ==(VertexBuffer<T> left, VertexBuffer<T> right) => left.Equals(right);

        public static bool operator !=(VertexBuffer<T> left, VertexBuffer<T> right) => !left.Equals(right);

        /// <summary>
        /// Recreate this <see cref="VertexBuffer{T}"/>'s storage with a new size.<para/>
        /// The contents of the new storage are undefined after this operation.
        /// </summary>
        /// <param name="storageLength">The desired new length for the storage measured in elements.</param>
        public void RecreateStorage(int storageLength)
        {
            ValidateStorageLength(storageLength);

            Buffer.RecreateStorage(storageLength * ElementSize);
            DataSubset.ResizeSubset(0, storageLength);
        }

        /// <summary>
        /// Recreate this <see cref="VertexBuffer{T}"/>'s storage with a new size and <see cref="BufferUsageHint"/>.<para/>
        /// The contents of the new storage are undefined after this operation.
        /// </summary>
        /// <param name="storageLength">The desired new length for the storage measured in elements.</param>
        /// <param name="usageHint">The new <see cref="BufferUsageHint"/> for the buffer.</param>
        public void RecreateStorage(int storageLength, BufferUsageHint usageHint)
        {
            ValidateStorageLength(storageLength);

            Buffer.RecreateStorage(storageLength * ElementSize, usageHint);
            DataSubset.ResizeSubset(0, storageLength);
        }

        /// <summary>
        /// Disposes the <see cref="GraphicsResource"/>-s used by this <see cref="VertexBuffer{T}"/>.
        /// </summary>
        public void Dispose()
        {
            Buffer.Dispose();
            VertexArray.Dispose();
        }

        /// <summary>
        /// Checks that the given storage length value is valid and throws an exception if it's not.
        /// </summary>
        private static void ValidateStorageLength(int storageLength)
        {
            if (storageLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(storageLength), storageLength, nameof(storageLength) + " must be greater than 0");
        }

        public override int GetHashCode()
        {
            return Buffer.GetHashCode();
        }

        public bool Equals(VertexBuffer<T> other)
        {
            return Buffer == other.Buffer;
        }

        public override bool Equals(object obj)
        {
            if (obj is VertexBuffer<T> vertexBuffer)
                return Equals(vertexBuffer);
            return false;
        }
    }
}
