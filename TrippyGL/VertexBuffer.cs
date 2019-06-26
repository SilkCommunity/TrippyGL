using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace TrippyGL
{
    /// <summary>
    /// Encapsulates a simple way to store vertex data in a single buffer
    /// </summary>
    /// <typeparam name="T">The type of vertex to use. Must be a struct and implement IVertex</typeparam>
    public class VertexBuffer<T> : IDisposable where T : struct, IVertex
    {
        /// <summary>The BufferObject that stores all the vertex data</summary>
        public readonly BufferObject Buffer;

        /// <summary>The subset that manages all of Buffer's storage</summary>
        public readonly VertexDataBufferSubset<T> BufferSubset;

        /// <summary>The VertexArray that defines how the vertex attributes are read</summary>
        public readonly VertexArray VertexArray;

        /// <summary>The size of a single vertex measured in bytes</summary>
        public readonly int ElementSize;

        /// <summary>The length of the buffer's storage measured in vertices</summary>
        public int StorageLength { get; private set; }

        /// <summary>
        /// Creates a VertexBuffer with specified initial data and length
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="data">An array containing the initial buffer data</param>
        /// <param name="dataOffset">The index of the first element to read from the data array</param>
        /// <param name="storageLength">The desired length for the buffer's storage measured in vertices</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public VertexBuffer(GraphicsDevice graphicsDevice, T[] data, int dataOffset, int storageLength, BufferUsageHint usageHint)
        {
            ValidateStorageLength(storageLength);
            ElementSize = Marshal.SizeOf<T>();

            Buffer = new BufferObject(graphicsDevice, storageLength * ElementSize, usageHint);
            BufferSubset = new VertexDataBufferSubset<T>(Buffer);
            BufferSubset.SetData(data, dataOffset, 0, data.Length);
            VertexArray = VertexArray.CreateSingleBuffer<T>(graphicsDevice, BufferSubset);
        }

        /// <summary>
        /// Creates a VertexBuffer with specified initial data and length
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="data">An array containing the initial buffer data</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public VertexBuffer(GraphicsDevice graphicsDevice, T[] data, BufferUsageHint usageHint)
        {
            if (data.Length == 0)
                throw new ArgumentException("The data array must have a length greater than 0", "data");
            ElementSize = Marshal.SizeOf<T>();

            StorageLength = data.Length;
            Buffer = new BufferObject(graphicsDevice, data.Length * ElementSize, usageHint);
            BufferSubset = new VertexDataBufferSubset<T>(Buffer);
            BufferSubset.SetData(data);
            VertexArray = VertexArray.CreateSingleBuffer<T>(graphicsDevice, BufferSubset);
        }

        /// <summary>
        /// Creates a VertexBuffer with specified length and undefined initial data
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="storageLength">The desired length for the buffer's storage measured in vertices</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public VertexBuffer(GraphicsDevice graphicsDevice, int storageLength, BufferUsageHint usageHint)
        {
            ValidateStorageLength(storageLength);
            ElementSize = Marshal.SizeOf<T>();

            StorageLength = storageLength;
            Buffer = new BufferObject(graphicsDevice, storageLength * ElementSize, usageHint);
            BufferSubset = new VertexDataBufferSubset<T>(Buffer);
            VertexArray = VertexArray.CreateSingleBuffer<T>(graphicsDevice, BufferSubset);
        }

        /// <summary>
        /// Sets the data of a specified part of this VertexBuffer's storage
        /// </summary>
        /// <param name="data">The array containing the data to set</param>
        /// <param name="dataOffset">The offset into the data array to start reading values from</param>
        /// <param name="storageOffset">The offset into the subset's storage to start writing to</param>
        /// <param name="elementCount">The amount of elements to set</param>
        public void SetData(T[] data, int dataOffset, int storageOffset, int elementCount)
        {
            BufferSubset.SetData(data, dataOffset, storageOffset, elementCount);
        }

        /// <summary>
        /// Sets the data of this VertexBuffer's storage
        /// </summary>
        /// <param name="data">The array containing the data to set</param>
        public void SetData(T[] data)
        {
            BufferSubset.SetData(data);
        }

        /// <summary>
        /// Gets the data of a specified part of this VertexBuffer's storage
        /// </summary>
        /// <param name="data">The array to which the returned data will be written to</param>
        /// <param name="dataOffset">The offset into the data array to start writing values to</param>
        /// <param name="storageOffset">The offset into the subset's storage to start reading from</param>
        /// <param name="elementCount">The amount of elements to get</param>
        public void GetData(T[] data, int dataOffset, int storageOffset, int elementCount)
        {
            BufferSubset.GetData(data, dataOffset, storageOffset, elementCount);
        }

        /// <summary>
        /// Gets the data of this VertexBuffer's storage
        /// </summary>
        /// <param name="data">The array to which the returned data will be written to</param>
        public void GetData(T[] data)
        {
            BufferSubset.GetData(data);
        }
        
        /// <summary>
        /// Recreate this VertexBuffer's storage with a new size. The contents of the new storage are undefined after this operation
        /// </summary>
        /// <param name="storageLength">The desired new length for the storage</param>
        public void RecreateStorage(int storageLength)
        {
            ValidateStorageLength(storageLength);

            Buffer.RecreateStorage(storageLength * ElementSize);
            BufferSubset.ResizeSubset(0, storageLength);
            this.StorageLength = storageLength;
        }

        /// <summary>
        /// Recreate this VertexBuffer's storage with a new size. The contents of the new storage are undefined after this operation
        /// </summary>
        /// <param name="storageLength">The desired new length for the storage</param>
        /// <param name="usageHint">The new usage hint for the buffer</param>
        public void RecreateStorage(int storageLength, BufferUsageHint usageHint)
        {
            ValidateStorageLength(storageLength);

            Buffer.RecreateStorage(storageLength * ElementSize, usageHint);
            BufferSubset.ResizeSubset(0, storageLength);
            this.StorageLength = storageLength;
        }

        /// <summary>
        /// Disposes the GraphicsResource-s used by this VertexBuffer
        /// </summary>
        public void Dispose()
        {
            Buffer.Dispose();
            VertexArray.Dispose();
        }

        private static void ValidateStorageLength(int storageLength)
        {
            if (storageLength <= 0)
                throw new ArgumentOutOfRangeException("storageLength", storageLength, "Storage length must be greater than 0");
        }
    }
}
