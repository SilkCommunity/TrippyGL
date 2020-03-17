using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Provides a limited but simple way to store vertex data in a single <see cref="BufferObject"/>.
    /// </summary>
    /// <typeparam name="T">The type of vertex to use. Must be a struct and implement <see cref="IVertex"/>.</typeparam>
    public readonly struct VertexBuffer<T> : IDisposable, IEquatable<VertexBuffer<T>> where T : unmanaged, IVertex
    {
        /// <summary>The <see cref="BufferObject"/> that stores all the vertex and index data.</summary>
        public readonly BufferObject Buffer;

        /// <summary>The <see cref="VertexDataBufferSubset{T}"/> that manages the element storage.</summary>
        public readonly VertexDataBufferSubset<T> DataSubset;

        /// <summary>The <see cref="TrippyGL.VertexArray"/> that defines how the vertex attributes are read.</summary>
        public readonly VertexArray VertexArray;

        /// <summary>The <see cref="IndexBufferSubset"/> that manages the index storage.</summary>
        public IndexBufferSubset IndexSubset => VertexArray.IndexBuffer;

        /// <summary>The size of a single vertex measured in bytes.</summary>
        public readonly uint ElementSize;

        /// <summary>The length of the <see cref="VertexBuffer{T}"/>'s element storage measured in vertices.</summary>
        public uint StorageLength => DataSubset.StorageLength;

        /// <summary>The length of the <see cref="VertexBuffer{T}"/>'s index storage measured in index elements.</summary>
        public uint IndexStorageLength => IndexSubset.StorageLength;

        /// <summary>Whether this <see cref="VertexBuffer{T}"/> is all null (because, for example, it hasn't been created yet).</summary>
        public bool IsEmpty => Buffer == null;

        /// <summary>
        /// Creates a <see cref="VertexBuffer{T}"/> with specified length, optional index buffer and usage hint.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="storageLength">The length for the <see cref="VertexBuffer{T}"/>'s element storage measured in vertices.</param>
        /// <param name="indexStorageLength">The length for the <see cref="VertexBuffer{T}"/>'s index storage measured in index elements, or 0 for no index storage.</param>
        /// <param name="indexElementType">The type of index element to use.</param>
        /// <param name="usageHint">Used by the graphics driver to optimize performance.</param>
        public VertexBuffer(GraphicsDevice graphicsDevice, uint storageLength, uint indexStorageLength, DrawElementsType indexElementType, BufferUsageARB usageHint)
        {
            ValidateStorageLength(storageLength);
            ElementSize = (uint)Marshal.SizeOf<T>();

            uint bufferStorageLengthBytes;
            uint elementSubsetLengthBytes = storageLength * ElementSize;

            bool hasIndexBuffer = indexStorageLength > 0;
            uint indexSubsetStartBytes;

            if (hasIndexBuffer)
            {
                uint indexElementSize = IndexBufferSubset.GetSizeInBytesOfElementType(indexElementType);
                indexSubsetStartBytes = (elementSubsetLengthBytes + indexElementSize - 1) / indexElementSize * indexElementSize;

                bufferStorageLengthBytes = indexSubsetStartBytes + indexElementSize * indexStorageLength;
            }
            else
            {
                indexSubsetStartBytes = 0;
                bufferStorageLengthBytes = elementSubsetLengthBytes;
            }

            Buffer = new BufferObject(graphicsDevice, bufferStorageLengthBytes, usageHint);
            DataSubset = new VertexDataBufferSubset<T>(Buffer, 0, storageLength);
            IndexBufferSubset indexSubset = hasIndexBuffer ? new IndexBufferSubset(Buffer, indexSubsetStartBytes, indexStorageLength, indexElementType) : null;

            VertexArray = VertexArray.CreateSingleBuffer<T>(graphicsDevice, DataSubset, indexSubset);
        }

        /// <summary>
        /// Creates a <see cref="VertexBuffer{T}"/> with specified length, optional index buffer,
        /// usage hint and initial vertex data.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="storageLength">The length for the <see cref="VertexBuffer{T}"/>'s element storage measured in vertices.</param>
        /// <param name="indexStorageLength">The length for the <see cref="VertexBuffer{T}"/>'s index storage measured in index elements, or 0 for no index storage.</param>
        /// <param name="indexElementType">The type of index element to use.</param>
        /// <param name="usageHint">Used by the graphics driver to optimize performance.</param>
        /// <param name="data">A <see cref="Span{T}"/> containing the initial vertex data.</param>
        /// <param name="dataWriteOffset">The offset into the vertex subset's storage at which to start writting the initial data.</param>
        public VertexBuffer(GraphicsDevice graphicsDevice, uint storageLength, uint indexStorageLength, DrawElementsType indexElementType, BufferUsageARB usageHint, Span<T> data, uint dataWriteOffset = 0)
            : this(graphicsDevice, storageLength, indexStorageLength, indexElementType, usageHint)
        {
            DataSubset.SetData(data, dataWriteOffset);
        }

        /// <summary>
        /// Creates a <see cref="VertexBuffer{T}"/> with specified initial vertex data and length but no index buffer.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="storageLength">The length for the <see cref="VertexBuffer{T}"/>'s element storage measured in vertices.</param>
        /// <param name="usageHint">Used by the graphics driver to optimize performance.</param>
        /// <param name="data">A <see cref="Span{T}"/> containing the initial vertex data.</param>
        /// <param name="dataWriteOffset">The offset into the vertex subset's storage at which to start writting the initial data.</param>
        public VertexBuffer(GraphicsDevice graphicsDevice, uint storageLength, BufferUsageARB usageHint, Span<T> data, uint dataWriteOffset = 0)
            : this(graphicsDevice, storageLength, 0, default, usageHint, data, dataWriteOffset)
        {

        }

        /// <summary>
        /// Creates a <see cref="VertexBuffer{T}"/> with specified initial vertex data and
        /// same length as that data <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="data">A <see cref="Span{T}"/> containing the initial vertex data.</param>
        /// <param name="usageHint">Used by the graphics driver to optimize performance.</param>
        public VertexBuffer(GraphicsDevice graphicsDevice, Span<T> data, BufferUsageARB usageHint)
            : this(graphicsDevice, (uint)data.Length, 0, default, usageHint, data)
        {

        }

        /// <summary>
        /// Creates a <see cref="VertexBuffer{T}"/> with specified length and undefined initial data.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="storageLength">The length for the <see cref="VertexBuffer{T}"/>'s element storage measured in vertices.</param>
        /// <param name="usageHint">Used by the graphics driver to optimize performance.</param>
        public VertexBuffer(GraphicsDevice graphicsDevice, uint storageLength, BufferUsageARB usageHint)
            : this(graphicsDevice, storageLength, 0, default, usageHint)
        {

        }

        /// <summary>
        /// Implicit cast to a <see cref="TrippyGL.VertexArray"/> allows a <see cref="VertexBuffer{T}"/>
        /// to be set directly onto <see cref="GraphicsDevice.VertexArray"/>.
        /// </summary>
        public static implicit operator VertexArray(VertexBuffer<T> vertexBuffer) => vertexBuffer.VertexArray;

        public static bool operator ==(VertexBuffer<T> left, VertexBuffer<T> right) => left.Equals(right);

        public static bool operator !=(VertexBuffer<T> left, VertexBuffer<T> right) => !left.Equals(right);

        /// <summary>
        /// Recreates this <see cref="VertexBuffer{T}"/>'s storage with a new size.<para/>
        /// The contents of the <see cref="VertexBuffer{T}"/>'s storage are undefined after this operation.
        /// </summary>
        /// <param name="storageLength">The new length for the <see cref="VertexBuffer{T}"/>'s element storage.</param>
        /// <param name="indexStorageLength">The new length for the <see cref="VertexBuffer{T}"/>'s index storage. 0 means either no index storage or keep previous length.</param>
        /// <param name="usageHint">Used by the graphics driver to optimize performance. 0 for same as before.</param>
        public void RecreateStorage(uint storageLength, uint indexStorageLength = 0, BufferUsageARB usageHint = default)
        {
            ValidateStorageLength(storageLength);

            IndexBufferSubset indexSubset = IndexSubset;
            bool hasIndexSubset = indexSubset != null;

            uint bufferStorageLengthBytes;
            uint elementSubsetLengthBytes = storageLength * ElementSize;

            uint indexSubsetStartBytes;

            if (hasIndexSubset)
            {
                if (indexStorageLength <= 0)
                    indexStorageLength = indexSubset.StorageLength;

                uint indexElementSize = IndexSubset.ElementSize;
                indexSubsetStartBytes = (elementSubsetLengthBytes + indexElementSize - 1) / indexElementSize * indexElementSize;

                bufferStorageLengthBytes = indexSubsetStartBytes + indexElementSize * indexStorageLength;
            }
            else
            {
                if (indexStorageLength > 0)
                    throw new InvalidOperationException("Resizing index storage only works if the VertexBuffer was created with index storage");

                indexSubsetStartBytes = 0;
                bufferStorageLengthBytes = elementSubsetLengthBytes;
            }

            Buffer.RecreateStorage(bufferStorageLengthBytes, usageHint);

            DataSubset.ResizeSubset(0, storageLength);
            indexSubset?.ResizeSubset(indexSubsetStartBytes, indexStorageLength);

        }

        /// <summary>
        /// Recreate this <see cref="VertexBuffer{T}"/>'s storage with a new size and <see cref="BufferUsageHint"/>.<para/>
        /// The contents of the <see cref="VertexBuffer{T}"/>'s storage are undefined after this operation.
        /// </summary>
        /// <param name="storageLength">The desired new length for the storage measured in elements.</param>
        /// <param name="usageHint">Used by the graphics driver to optimize performance. 0 for same as before.</param>
        public void RecreateStorage(uint storageLength, BufferUsageARB usageHint)
        {
            RecreateStorage(storageLength, 0, usageHint);
        }

        /// <summary>
        /// Disposes the <see cref="GraphicsResource"/>-s used by this <see cref="VertexBuffer{T}"/>.
        /// </summary>
        public void Dispose()
        {
            Buffer.Dispose();
            VertexArray.Dispose();
        }

        public override string ToString()
        {
            return IsEmpty ? "Empty " + nameof(VertexBuffer<T>) : string.Concat(
                "BufferHandle=", Buffer.Handle.ToString(),
                ", " + nameof(StorageLength) + "=", StorageLength.ToString(),
                ", " + nameof(IndexStorageLength) + "=", IndexStorageLength.ToString()
            );
        }

        /// <summary>
        /// Checks that the given storage length value is valid and throws an exception if it's not.
        /// </summary>
        private static void ValidateStorageLength(uint storageLength)
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
