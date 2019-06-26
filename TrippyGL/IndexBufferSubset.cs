using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// A BufferObjectSubset whose purpose is to store index data
    /// </summary>
    public class IndexBufferSubset : BufferObjectSubset
    {
        /// <summary>The length of the buffer object's storage measured in elements</summary>
        public int StorageLength { get; private set; }

        /// <summary>The size of each element in the buffer object's storage measured in bytes</summary>
        public readonly int ElementSize;

        /// <summary>Gets the amount of bytes each element occupies</summary>
        public readonly DrawElementsType ElementType;

        /// <summary>
        /// Creates a new IndexBufferSubset with the specified offset into the buffer, storage length and element type
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        /// <param name="storageOffsetBytes">The offset into the buffer's storage where this subset begins</param>
        /// <param name="storageLength">The length of this subset measured in elements</param>
        /// <param name="elementType">The type of elements this index buffer will use</param>
        public IndexBufferSubset(BufferObject bufferObject, int storageOffsetBytes, int storageLength, DrawElementsType elementType)
            : base(bufferObject, BufferTarget.ElementArrayBuffer)
        {
            ElementType = elementType;
            ElementSize = TrippyUtils.GetSizeInBytesOfElementType(elementType);
            ResizeSubset(storageOffsetBytes, storageLength);
        }

        /// <summary>
        /// Creates an IndexBufferSubset with the given BufferObject, with the subset covering the entire buffer's storage
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        /// <param name="elementType">The type of elements this index buffer will use</param>
        public IndexBufferSubset(BufferObject bufferObject, DrawElementsType elementType) : base(bufferObject, BufferTarget.ElementArrayBuffer)
        {
            ElementType = elementType;
            ElementSize = TrippyUtils.GetSizeInBytesOfElementType(elementType);
            InitializeStorage(0, Buffer.StorageLengthInBytes);
            StorageLength = bufferObject.StorageLengthInBytes / ElementSize;

            if (StorageLength * ElementSize != StorageLengthInBytes)
                throw new ArgumentException("The provided BufferObjectSubset's StorageLengthInBytes should be a multiple of this.ElementSize");
        }

        /// <summary>
        /// Creates a new IndexBufferSubset with the specified offset into the buffer, storage length, UnsignedInt element type and initial data
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        /// <param name="data">An array containing the initial data</param>
        /// <param name="dataOffset">The index of the first element to read from the array</param>
        /// <param name="storageOffsetBytes">The offset into the buffer's storage where this subset begins</param>
        /// <param name="storageLength">The length of this subset measured in elements</param>
        public IndexBufferSubset(BufferObject bufferObject, uint[] data, int dataOffset, int storageOffsetBytes, int storageLength)
            : this(bufferObject, storageOffsetBytes, storageLength, DrawElementsType.UnsignedInt)
        {
            SetData(data, dataOffset, 0, data.Length);
        }

        /// <summary>
        /// Creates a new IndexBufferSubset with the specified offset into the buffer, storage length, UnsignedShort element type and initial data
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        /// <param name="data">An array containing the initial data</param>
        /// <param name="dataOffset">The index of the first element to read from the array</param>
        /// <param name="storageOffsetBytes">The offset into the buffer's storage where this subset begins</param>
        /// <param name="storageLength">The length of this subset measured in elements</param>
        public IndexBufferSubset(BufferObject bufferObject, ushort[] data, int dataOffset, int storageOffsetBytes, int storageLength)
            : this(bufferObject, storageOffsetBytes, storageLength, DrawElementsType.UnsignedShort)
        {
            SetData(data, dataOffset, 0, data.Length);
        }

        /// <summary>
        /// Creates a new IndexBufferSubset with the specified offset into the buffer, storage length, UnsignedByte element type and initial data
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        /// <param name="data">An array containing the initial data</param>
        /// <param name="dataOffset">The index of the first element to read from the array</param>
        /// <param name="storageOffsetBytes">The offset into the buffer's storage where this subset begins</param>
        /// <param name="storageLength">The length of this subset measured in elements</param>
        public IndexBufferSubset(BufferObject bufferObject, byte[] data, int dataOffset, int storageOffsetBytes, int storageLength)
            : this(bufferObject, storageOffsetBytes, storageLength, DrawElementsType.UnsignedByte)
        {
            SetData(data, dataOffset, 0, data.Length);
        }

        /// <summary>
        /// Sets the data of a specified part of this subset's storage
        /// </summary>
        /// <param name="data">The array containing the data to set</param>
        /// <param name="dataOffset">The offset into the data array to start reading values from</param>
        /// <param name="storageOffset">The offset into the subset's storage to start writing to</param>
        /// <param name="elementCount">The amount of elements to set</param>
        public void SetData(uint[] data, int dataOffset, int storageOffset, int elementCount)
        {
            ValidateCorrectElementType(DrawElementsType.UnsignedInt);
            ValidateSetParams(data.Length, dataOffset, storageOffset, elementCount);

            Buffer.GraphicsDevice.BindBuffer(this);
            GL.BufferSubData(BufferTarget, (IntPtr)(storageOffset * ElementSize + StorageOffsetInBytes), elementCount * ElementSize, ref data[dataOffset]);
        }

        /// <summary>
        /// Sets the data of this subset's storage
        /// </summary>
        /// <param name="data">The array containing the data to set</param>
        public void SetData(uint[] data)
        {
            SetData(data, 0, 0, data.Length);
        }

        /// <summary>
        /// Sets the data of a specified part of this subset's storage
        /// </summary>
        /// <param name="data">The array containing the data to set</param>
        /// <param name="dataOffset">The offset into the data array to start reading values from</param>
        /// <param name="storageOffset">The offset into the subset's storage to start writing to</param>
        /// <param name="elementCount">The amount of elements to set</param>
        public void SetData(ushort[] data, int dataOffset, int storageOffset, int elementCount)
        {
            ValidateCorrectElementType(DrawElementsType.UnsignedShort);
            ValidateSetParams(data.Length, dataOffset, storageOffset, elementCount);

            Buffer.GraphicsDevice.BindBuffer(this);
            GL.BufferSubData(BufferTarget, (IntPtr)(storageOffset * ElementSize + StorageOffsetInBytes), elementCount * ElementSize, ref data[dataOffset]);
        }

        /// <summary>
        /// Sets the data of this subset's storage
        /// </summary>
        /// <param name="data">The array containing the data to set</param>
        public void SetData(ushort[] data)
        {
            SetData(data, 0, 0, data.Length);
        }

        /// <summary>
        /// Sets the data of a specified part of this subset's storage
        /// </summary>
        /// <param name="data">The array containing the data to set</param>
        /// <param name="dataOffset">The offset into the data array to start reading values from</param>
        /// <param name="storageOffset">The offset into the subset's storage to start writing to</param>
        /// <param name="elementCount">The amount of elements to set</param>
        public void SetData(byte[] data, int dataOffset, int storageOffset, int elementCount)
        {
            ValidateCorrectElementType(DrawElementsType.UnsignedByte);
            ValidateSetParams(data.Length, dataOffset, storageOffset, elementCount);

            Buffer.GraphicsDevice.BindBuffer(this);
            GL.BufferSubData(BufferTarget, (IntPtr)(storageOffset * ElementSize + StorageOffsetInBytes), elementCount * ElementSize, ref data[dataOffset]);
        }

        /// <summary>
        /// Sets the data of this subset's storage
        /// </summary>
        /// <param name="data">The array containing the data to set</param>
        public void SetData(byte[] data)
        {
            SetData(data, 0, 0, data.Length);
        }

        /// <summary>
        /// Gets the data of a specified part of this subset's storage
        /// </summary>
        /// <param name="data">The array to which the returned data will be written to</param>
        /// <param name="dataOffset">The offset into the data array to start writing values to</param>
        /// <param name="storageOffset">The offset into the subset's storage to start reading from</param>
        /// <param name="elementCount">The amount of elements to get</param>
        public void GetData(uint[] data, int dataOffset, int storageOffset, int elementCount)
        {
            ValidateCorrectElementType(DrawElementsType.UnsignedInt);
            ValidateGetParams(data.Length, dataOffset, storageOffset, elementCount);

            Buffer.GraphicsDevice.BindBuffer(this);
            GL.GetBufferSubData(BufferTarget, (IntPtr)(storageOffset * ElementSize + StorageOffsetInBytes), elementCount * ElementSize, ref data[dataOffset]);
        }

        /// <summary>
        /// Gets the data of this subset's storage
        /// </summary>
        /// <param name="data">The array to which the returned data will be written to</param>
        public void GetData(uint[] data)
        {
            GetData(data, 0, 0, data.Length);
        }

        /// <summary>
        /// Gets the data of a specified part of this subset's storage
        /// </summary>
        /// <param name="data">The array to which the returned data will be written to</param>
        /// <param name="dataOffset">The offset into the data array to start writing values to</param>
        /// <param name="storageOffset">The offset into the subset's storage to start reading from</param>
        /// <param name="elementCount">The amount of elements to get</param>
        public void GetData(ushort[] data, int dataOffset, int storageOffset, int elementCount)
        {
            ValidateCorrectElementType(DrawElementsType.UnsignedShort);
            ValidateGetParams(data.Length, dataOffset, storageOffset, elementCount);

            Buffer.GraphicsDevice.BindBuffer(this);
            GL.GetBufferSubData(BufferTarget, (IntPtr)(storageOffset * ElementSize + StorageOffsetInBytes), elementCount * ElementSize, ref data[dataOffset]);
        }

        /// <summary>
        /// Gets the data of this subset's storage
        /// </summary>
        /// <param name="data">The array to which the returned data will be written to</param>
        public void GetData(ushort[] data)
        {
            GetData(data, 0, 0, data.Length);
        }

        /// <summary>
        /// Gets the data of a specified part of this subset's storage
        /// </summary>
        /// <param name="data">The array to which the returned data will be written to</param>
        /// <param name="dataOffset">The offset into the data array to start writing values to</param>
        /// <param name="storageOffset">The offset into the subset's storage to start reading from</param>
        /// <param name="elementCount">The amount of elements to get</param>
        public void GetData(byte[] data, int dataOffset, int storageOffset, int elementCount)
        {
            ValidateCorrectElementType(DrawElementsType.UnsignedByte);
            ValidateGetParams(data.Length, dataOffset, storageOffset, elementCount);

            Buffer.GraphicsDevice.BindBuffer(this);
            GL.GetBufferSubData(BufferTarget, (IntPtr)(storageOffset * ElementSize + StorageOffsetInBytes), elementCount * ElementSize, ref data[dataOffset]);
        }

        /// <summary>
        /// Gets the data of this subset's storage
        /// </summary>
        /// <param name="data">The array to which the returned data will be written to</param>
        public void GetData(byte[] data)
        {
            GetData(data, 0, 0, data.Length);
        }

        /// <summary>
        /// Changes the subset location of this DataBufferSubset
        /// </summary>
        /// <param name="storageOffsetBytes">The offset into the buffer's storage where this subset begins</param>
        /// <param name="storageLength">The length of this subset measured in elements</param>
        public void ResizeSubset(int storageOffsetBytes, int storageLength)
        {
            if (storageOffsetBytes % ElementSize != 0)
                throw new ArgumentException("storageOffsetBytes should be a multiple of this.ElementSize", "storageOffsetBytes");
            //Else it's pretty much impossible to use the index buffer

            InitializeStorage(storageOffsetBytes, storageLength * ElementSize);
            StorageLength = storageLength;
        }

        /// <summary>
        /// Checks that the index buffer's ElementType is the specified one and throws an exception if it's not
        /// </summary>
        /// <param name="elementType">The element type to check</param>
        private void ValidateCorrectElementType(DrawElementsType elementType)
        {
            if (elementType != ElementType)
                throw new InvalidOperationException("To perform this operation the IndexBufferSubset's ElementType must be " + elementType.ToString());
        }

        /// <summary>
        /// Validates the parameters for a set operation
        /// </summary>
        private void ValidateSetParams(int dataLength, int dataOffset, int storageOffset, int elementCount)
        {
            if (dataLength == 0)
                throw new ArgumentNullException("data", "The data array can't be null nor empty");

            if (storageOffset < 0 || storageOffset >= StorageLength)
                throw new ArgumentOutOfRangeException("storageOffset", storageOffset, "Storage offset must be in the range [0, this.StorageLength)");

            if (dataOffset < 0 || dataOffset >= dataLength)
                throw new ArgumentOutOfRangeException("dataOffset", dataOffset, "Data offset must be in the range [0, data.Length)");

            if (dataLength - dataOffset < elementCount)
                throw new ArgumentOutOfRangeException("There isn't enough data in the array to and read elementCount elements starting from index dataOffset");

            if (elementCount > StorageLength - storageOffset)
                throw new ArgumentOutOfRangeException("The buffer's storage isn't big enough to write elementCount elements starting from storageOffset");
        }

        /// <summary>
        /// Validates the parameters for a get operation
        /// </summary>
        private void ValidateGetParams(int dataLength, int dataOffset, int storageOffset, int elementCount)
        {
            if (dataLength == 0)
                throw new ArgumentException("Data array can't be null nor empty", "data");

            if (storageOffset < 0 || storageOffset >= StorageLength)
                throw new ArgumentOutOfRangeException("storageOffset", storageOffset, "Storage offset must be in the range [0, StorageLength)");

            if (dataOffset < 0 || dataOffset >= dataLength)
                throw new ArgumentOutOfRangeException("dataOffset", dataOffset, "Data offset must be in the range [0, data.Length)");

            if (dataLength - dataOffset < elementCount)
                throw new ArgumentOutOfRangeException("There data array ins't big enough to write dataLength elements starting from index dataOffset");

            if (elementCount > StorageLength - storageOffset)
                throw new ArgumentOutOfRangeException("There isn't enough data in the buffer object's storage to read dataLength elements starting from index storageOffset");
        }
    }
}