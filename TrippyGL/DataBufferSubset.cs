using System;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace TrippyGL
{
    /// <summary>
    /// An abstract class for BufferObjectSubset-s that will manage a struct type across the entire subset
    /// </summary>
    /// <typeparam name="T">The type of struct (element) this DataBufferSubset will manage</typeparam>
    public abstract class DataBufferSubset<T> : BufferObjectSubset, IDataBufferSubset where T : struct
    {
        /// <summary>The length of the buffer object's storage measured in elements</summary>
        public int StorageLength { get; private set; }

        /// <summary>The size of each element in the buffer object's storage measured in bytes</summary>
        public int ElementSize { get; }

        /// <summary>
        /// Creates a DataBufferSubset with the given BufferObject and target, offset into the buffer in bytes and storage length in elements
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        /// <param name="bufferTarget">The BufferTarget this subset will always bind to</param>
        /// <param name="storageOffsetBytes">The offset into the buffer's storage where this subset begins</param>
        /// <param name="storageLength">The length of this subset measured in elements</param>
        internal DataBufferSubset(BufferObject bufferObject, BufferTarget bufferTarget, int storageOffsetBytes, int storageLength) : base(bufferObject, bufferTarget)
        {
            ElementSize = Marshal.SizeOf<T>();
            InitializeStorage(storageOffsetBytes, storageLength * ElementSize);
            StorageLength = storageLength;
        }

        /// <summary>
        /// Creates a DataBufferSubset with the given BufferObject and target, offset into the buffer in bytes and storage length in elements
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        /// <param name="bufferTarget">The BufferTarget this subset will always bind to</param>
        /// <param name="data">An array containing the initial data to set to the subset</param>
        /// <param name="dataOffset">The offset into the data array to start reading values from</param>
        /// <param name="storageOffsetBytes">The offset into the buffer's storage where this subset begins</param>
        /// <param name="storageLength">The length of this subset measured in elements</param>
        internal DataBufferSubset(BufferObject bufferObject, BufferTarget bufferTarget, T[] data, int dataOffset, int storageOffsetBytes, int storageLength)
            : this(bufferObject, bufferTarget, storageOffsetBytes, storageLength)
        {
            SetData(data, dataOffset, 0, storageLength);
        }

        /// <summary>
        /// Creates a DataBufferSubset with the given BufferObject and target, with the subset covering the entire buffer's storage
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        /// <param name="bufferTarget">The BufferTarget this subset will always bind to</param>
        internal DataBufferSubset(BufferObject bufferObject, BufferTarget bufferTarget) : base(bufferObject, bufferTarget)
        {
            ElementSize = Marshal.SizeOf<T>();
            InitializeStorage(0, bufferObject.StorageLengthInBytes);
            StorageLength = StorageLengthInBytes / ElementSize;

            if (StorageLength * ElementSize != StorageLengthInBytes)
                throw new ArgumentException("The provided BufferObjectSubset's StorageLengthInBytes should be a multiple of this.ElementSize");
        }

        /// <summary>
        /// Creates a DataBufferSubset with the given BufferObject and target, with the subset covering the entire buffer's storage and sets initial data
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        /// <param name="bufferTarget">The BufferTarget this subset will always bind to</param>
        /// <param name="data">An array containing the initial data to set to the subset</param>
        /// <param name="dataOffset">The offset into the data array to start reading values from</param>
        internal DataBufferSubset(BufferObject bufferObject, BufferTarget bufferTarget, T[] data, int dataOffset) : this(bufferObject, bufferTarget)
        {
            SetData(data, dataOffset, 0, data.Length);
        }

        /// <summary>
        /// Creates a DataBufferSubset that occupies the same area in the same buffer as another buffer subset but has another BufferTarget
        /// </summary>
        /// <param name="copy">The BufferObjectSubset to copy the range form</param>
        /// <param name="bufferTarget">The BufferTarget this subset will always bind to</param>
        internal DataBufferSubset(BufferObjectSubset copy, BufferTarget bufferTarget) : base(copy, bufferTarget)
        {
            ElementSize = Marshal.SizeOf<T>();
            StorageLength = StorageLengthInBytes / ElementSize;
            if (StorageLength * ElementSize != StorageLengthInBytes)
                throw new ArgumentException("The provided BufferObjectSubset's StorageLengthInBytes should be a multiple of this.ElementSize");
        }

        /// <summary>
        /// Creates a DataBufferSubset that occupies the same area in the same buffer and uses the same struct type as another DataBufferSubset but has another BufferTarget
        /// </summary>
        /// <param name="copy">The DataBufferSubset to copy the range from</param>
        /// <param name="bufferTarget">The BufferTarget this subset will always bind to</param>
        internal DataBufferSubset(DataBufferSubset<T> copy, BufferTarget bufferTarget) : base(copy, bufferTarget)
        {
            ElementSize = copy.ElementSize;
            StorageLength = copy.StorageLength;
        }

        /// <summary>
        /// Sets the data of a specified part of this subset's storage
        /// </summary>
        /// <param name="data">The array containing the data to set</param>
        /// <param name="dataOffset">The offset into the data array to start reading values from</param>
        /// <param name="storageOffset">The offset into the subset's storage to start writing to</param>
        /// <param name="elementCount">The amount of elements to set</param>
        public void SetData(T[] data, int dataOffset, int storageOffset, int elementCount)
        {
            Buffer.ValidateWriteOperation();
            ValidateSetParams(data, dataOffset, storageOffset, elementCount);

            Buffer.GraphicsDevice.BindBuffer(this);
            GL.BufferSubData(BufferTarget, (IntPtr)(storageOffset * ElementSize + StorageOffsetInBytes), elementCount * ElementSize, ref data[dataOffset]);
        }

        /// <summary>
        /// Sets the data of this subset's storage
        /// </summary>
        /// <param name="data">The array containing the data to set</param>
        public void SetData(T[] data)
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
        public void GetData(T[] data, int dataOffset, int storageOffset, int elementCount)
        {
            Buffer.ValidateReadOperation();
            ValidateGetParams(data, dataOffset, storageOffset, elementCount);

            Buffer.GraphicsDevice.BindBuffer(this);
            GL.GetBufferSubData(BufferTarget, (IntPtr)(storageOffset * ElementSize + StorageOffsetInBytes), elementCount * ElementSize, ref data[dataOffset]);
        }

        /// <summary>
        /// Gets the data of this subset's storage
        /// </summary>
        /// <param name="data">The array to which the returned data will be written to</param>
        public void GetData(T[] data)
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
            InitializeStorage(storageOffsetBytes, storageLength * ElementSize);
            StorageLength = storageLength;
        }

        /// <summary>
        /// Validates the parameters for a set operation
        /// </summary>
        private protected void ValidateSetParams(T[] data, int dataOffset, int storageOffset, int elementCount)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException("data", "The data array can't be null nor empty");

            if (storageOffset < 0 || storageOffset >= StorageLength)
                throw new ArgumentOutOfRangeException("storageOffset", storageOffset, "Storage offset must be in the range [0, this.StorageLength)");

            if (dataOffset < 0 || dataOffset >= data.Length)
                throw new ArgumentOutOfRangeException("dataOffset", dataOffset, "Data offset must be in the range [0, data.Length)");

            if (data.Length - dataOffset < elementCount)
                throw new ArgumentOutOfRangeException("There isn't enough data in the array to and read elementCount elements starting from index dataOffset");

            if (elementCount > StorageLength - storageOffset)
                throw new ArgumentOutOfRangeException("The buffer's storage isn't big enough to write elementCount elements starting from storageOffset");
        }

        /// <summary>
        /// Validates the parameters for a get operation
        /// </summary>
        private protected void ValidateGetParams(T[] data, int dataOffset, int storageOffset, int elementCount)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data array can't be null nor empty", "data");

            if (storageOffset < 0 || storageOffset >= StorageLength)
                throw new ArgumentOutOfRangeException("storageOffset", storageOffset, "Storage offset must be in the range [0, StorageLength)");

            if (dataOffset < 0 || dataOffset >= data.Length)
                throw new ArgumentOutOfRangeException("dataOffset", dataOffset, "Data offset must be in the range [0, data.Length)");

            if (data.Length - dataOffset < elementCount)
                throw new ArgumentOutOfRangeException("There data array ins't big enough to write dataLength elements starting from index dataOffset");

            if (elementCount > StorageLength - storageOffset)
                throw new ArgumentOutOfRangeException("There isn't enough data in the buffer object's storage to read dataLength elements starting from index storageOffset");
        }

        public override string ToString()
        {
            return String.Concat(base.ToString(), ", StorageLength=", StorageLength.ToString(), ", ElementSize=", ElementSize.ToString());
        }

        /// <summary>
        /// Calculates the required storage length in bytes required for a DataBufferSubset with the specified storage length
        /// </summary>
        /// <param name="storageLength">The desired length for the subset measured in elements</param>
        public static int CalculateRequiredSizeInBytes(int storageLength)
        {
            return Marshal.SizeOf<T>() * storageLength;
        }

        /// <summary>
        /// Copies data from a source buffer to a destination buffer
        /// </summary>
        /// <param name="source">The buffer to copy data from</param>
        /// <param name="sourceOffset">The index of the first element to copy from the source buffer</param>
        /// <param name="dest">The buffer to write data to</param>
        /// <param name="destOffset">The index of of the first element to write on the dest buffer</param>
        /// <param name="dataLength">The amount of elements to copy</param>
        public static void CopyBuffers(DataBufferSubset<T> source, int sourceOffset, DataBufferSubset<T> dest, int destOffset, int dataLength)
        {
            GraphicsDevice g = source.Buffer.GraphicsDevice;
            if (g != dest.Buffer.GraphicsDevice)
                throw new InvalidOperationException("You can't copy data between buffers from different GraphicsDevice-s");

            if (sourceOffset < 0 || sourceOffset > source.StorageLength)
                throw new ArgumentOutOfRangeException("sourceOffset", sourceOffset, "sourceOffset must be in the range [0, source.StorageLength)");

            if (destOffset < 0 || destOffset > dest.StorageLength)
                throw new ArgumentOutOfRangeException("destOffset", destOffset, "destOffset must be in the range [0, dest.StorageLength)");

            if (sourceOffset + dataLength > source.StorageLength)
                throw new BufferCopyException("There isn't enough data in the source buffer to copy dataLength elements");

            if (destOffset + dataLength > dest.StorageLength)
                throw new BufferCopyException("There isn't enough data in the dest buffer to copy dataLength elements");

            if (source == dest)
            {
                // We're copying from and to the same buffer? Let's ensure the sections don't overlap then

                if ((destOffset + dataLength <= sourceOffset) || (destOffset > sourceOffset + dataLength))
                    throw new BufferCopyException("When copying to and from the same buffer, the ranges must not overlap");
                // This checks that the dest range is either fully to the left or fully to the right of the source range
            }

            // Everything looks fine, let's perform the copy operation!
            g.CopyReadBuffer = source.Buffer;
            g.CopyWriteBuffer = dest.Buffer;
            int elementSize = source.ElementSize;
            GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, BufferTarget.CopyWriteBuffer, (IntPtr)(source.StorageOffsetInBytes + sourceOffset * elementSize), (IntPtr)(dest.StorageOffsetInBytes + destOffset * elementSize), dataLength * elementSize);
        }

        /// <summary>
        /// Copies all the data from a source buffer to a destination buffer
        /// </summary>
        /// <param name="source">The buffer to copy data from</param>
        /// <param name="dest">The buffer to write data to</param>
        public static void CopyBuffers(DataBufferSubset<T> source, DataBufferSubset<T> dest)
        {
            CopyBuffers(source, 0, dest, 0, source.StorageLength);
        }
    }

    /// <summary>
    /// This interface is used to be able to acces methods of a DataBufferSubset without caring about it's type param
    /// </summary>
    internal interface IDataBufferSubset
    {
        int StorageLength { get; }
        int ElementSize { get; }
    }
}
