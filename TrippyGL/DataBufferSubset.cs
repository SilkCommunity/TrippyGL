using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace TrippyGL
{
    /// <summary>
    /// An abstract class for <see cref="BufferObjectSubset"/>-s that will manage a struct type across the entire subset.
    /// </summary>
    /// <typeparam name="T">The type of struct (element) type this <see cref="DataBufferSubset{T}"/> will manage.</typeparam>
    public abstract class DataBufferSubset<T> : BufferObjectSubset, IDataBufferSubset where T : struct
    {
        /// <summary>The length of the buffer object's storage measured in elements.</summary>
        public int StorageLength { get; private set; }

        /// <summary>The size of each element in the buffer object's storage measured in bytes.</summary>
        public int ElementSize { get; }

        /// <summary>
        /// Creates a <see cref="DataBufferSubset{T}"/> with the given <see cref="BufferObject"/>
        /// and target, offset into the buffer in bytes and storage length in elements.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        /// <param name="bufferTarget">The <see cref="BufferTarget"/> this subset will always bind to.</param>
        /// <param name="storageOffsetBytes">The offset into the buffer's storage where this subset begins.</param>
        /// <param name="storageLength">The length of this subset measured in elements.</param>
        internal DataBufferSubset(BufferObject bufferObject, BufferTarget bufferTarget, int storageOffsetBytes, int storageLength) : base(bufferObject, bufferTarget)
        {
            ElementSize = Marshal.SizeOf<T>();
            InitializeStorage(storageOffsetBytes, storageLength * ElementSize);
            StorageLength = storageLength;
        }

        /// <summary>
        /// Creates a <see cref="DataBufferSubset{T}"/> with the given <see cref="BufferObject"/>
        /// and target, offset into the buffer in bytes and storage length in elements.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        /// <param name="bufferTarget">The <see cref="BufferTarget"/> this subset will always bind to.</param>
        /// <param name="storageOffsetBytes">The offset into the buffer's storage where this subset begins.</param>
        /// <param name="storageLength">The length of this subset measured in elements.</param>
        /// <param name="data">A <see cref="Span{T}"/> containing the initial data to set to the subset.</param>
        /// <param name="dataWriteOffset">The offset into the subset's storage at which to start writting the initial data.</param>
        internal DataBufferSubset(BufferObject bufferObject, BufferTarget bufferTarget, int storageOffsetBytes, int storageLength, Span<T> data, int dataWriteOffset = 0)
            : this(bufferObject, bufferTarget, storageOffsetBytes, storageLength)
        {
            SetData(data, dataWriteOffset);
        }

        /// <summary>
        /// Creates a <see cref="DataBufferSubset{T}"/> with the given <see cref="BufferObject"/>
        /// and target, with the subset covering the entire buffer's storage.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        /// <param name="bufferTarget">The <see cref="BufferTarget"/> this subset will always bind to.</param>
        internal DataBufferSubset(BufferObject bufferObject, BufferTarget bufferTarget) : base(bufferObject, bufferTarget)
        {
            ElementSize = Marshal.SizeOf<T>();
            InitializeStorage(0, bufferObject.StorageLengthInBytes);
            StorageLength = StorageLengthInBytes / ElementSize;

            if (StorageLength * ElementSize != StorageLengthInBytes)
                throw new ArgumentException("The provided " + nameof(BufferObject) + "'s "
                    + nameof(BufferObject.StorageLengthInBytes) + " should be a multiple of " + nameof(ElementSize));
        }

        /// <summary>
        /// Creates a <see cref="DataBufferSubset{T}"/> with the given <see cref="BufferObject"/>
        /// and target, with the subset covering the entire buffer's storage and sets initial data.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        /// <param name="bufferTarget">The <see cref="BufferTarget"/> this subset will always bind to.</param>
        /// <param name="data">A <see cref="Span{T}"/> containing the initial data to set to the subset.</param>
        internal DataBufferSubset(BufferObject bufferObject, BufferTarget bufferTarget, Span<T> data) : this(bufferObject, bufferTarget)
        {
            SetData(data);
        }

        /// <summary>
        /// Creates a <see cref="DataBufferSubset{T}"/> that occupies the same area in the same buffer as
        /// another buffer subset but has another <see cref="BufferTarget"/>.
        /// </summary>
        /// <param name="subsetToCopy">The <see cref="BufferObjectSubset"/> to copy the range form.</param>
        /// <param name="bufferTarget">The <see cref="BufferTarget"/> this subset will always bind to.</param>
        internal DataBufferSubset(BufferObjectSubset subsetToCopy, BufferTarget bufferTarget) : base(subsetToCopy, bufferTarget)
        {
            ElementSize = Marshal.SizeOf<T>();
            StorageLength = StorageLengthInBytes / ElementSize;

            if (StorageLength * ElementSize != StorageLengthInBytes)
                throw new ArgumentException("The provided " + nameof(BufferObjectSubset) + "'s "
                    + nameof(StorageLengthInBytes) + " should be a multiple of " + nameof(ElementSize));
        }

        /// <summary>
        /// Creates a <see cref="DataBufferSubset{T}"/> that occupies the same area in the same buffer and uses the
        /// same struct type as another <see cref="DataBufferSubset{T}"/> but has different <see cref="BufferTarget"/>.
        /// </summary>
        /// <param name="copy">The <see cref="DataBufferSubset{T}"/> to copy the range from.</param>
        /// <param name="bufferTarget">The <see cref="BufferTarget"/> this subset will always bind to.</param>
        internal DataBufferSubset(DataBufferSubset<T> copy, BufferTarget bufferTarget) : base(copy, bufferTarget)
        {
            ElementSize = copy.ElementSize;
            StorageLength = copy.StorageLength;
        }

        // TODO: SetData() should really use ReadOnlySpans<T>... Problem is, we have to pass the span by ref data[0]
        // Once this is fixed, remember to also change the constructors! And in VertexBuffer's constructors!
        // and in VertexDataBufferSubset's constructors!

        /// <summary>
        /// Sets the data of a specified part of this subset's storage.
        /// The amount of elements written is the length of the given <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="data">The <see cref="Span{T}"/> containing the data to set.</param>
        /// <param name="storageOffset">The offset into the subset's storage to start writing to, measured in elements.</param>
        public void SetData(Span<T> data, int storageOffset = 0)
        {
            Buffer.ValidateWriteOperation();
            if (storageOffset < 0 || storageOffset >= StorageLength)
                throw new ArgumentOutOfRangeException(nameof(storageOffset), storageOffset, nameof(storageOffset) + " must be in the range [0, " + nameof(StorageLength) + ")");

            if (data.Length + storageOffset > StorageLength)
                throw new ArgumentOutOfRangeException("Tried to write past the subset's length");

            Buffer.GraphicsDevice.BindBuffer(this);
            GL.BufferSubData(BufferTarget, (IntPtr)(storageOffset * ElementSize + StorageOffsetInBytes), data.Length * ElementSize, ref data[0]);
        }

        /// <summary>
        /// Gets the data of a specified part of this subset's storage.
        /// The amount of elements read is the length of the given <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="data">The <see cref="Span{T}"/> to which the returned data will be written to.</param>
        /// <param name="storageOffset">The offset into the subset's storage to start reading from, measured in elements.</param>
        public void GetData(Span<T> data, int storageOffset = 0)
        {
            Buffer.ValidateReadOperation();
            if (storageOffset < 0 || storageOffset >= StorageLength)
                throw new ArgumentOutOfRangeException(nameof(storageOffset), storageOffset, nameof(storageOffset) + " must be in the range [0, " + nameof(StorageLength) + ")");

            if (data.Length + storageOffset > StorageLength)
                throw new ArgumentOutOfRangeException("Tried to read past the subset's length");

            Buffer.GraphicsDevice.BindBuffer(this);
            GL.GetBufferSubData(BufferTarget, (IntPtr)(storageOffset * ElementSize + StorageOffsetInBytes), data.Length * ElementSize, ref data[0]);
        }

        /// <summary>
        /// Changes the subset location of this DataBufferSubset.
        /// </summary>
        /// <param name="storageOffsetBytes">The offset into the buffer's storage where this subset begins.</param>
        /// <param name="storageLength">The length of this subset measured in elements.</param>
        public void ResizeSubset(int storageOffsetBytes, int storageLength)
        {
            InitializeStorage(storageOffsetBytes, storageLength * ElementSize);
            StorageLength = storageLength;
        }

        public override string ToString()
        {
            return string.Concat(base.ToString(), ", StorageLength=", StorageLength.ToString(), ", ElementSize=", ElementSize.ToString());
        }

        /// <summary>
        /// Calculates the required storage length in bytes required for a
        /// <see cref="DataBufferSubset{T}"/> with the specified storage length.
        /// </summary>
        /// <param name="storageLength">The desired length for the subset measured in elements.</param>
        public static int CalculateRequiredSizeInBytes(int storageLength)
        {
            return Marshal.SizeOf<T>() * storageLength;
        }

        /// <summary>
        /// Copies data from a source buffer to a destination buffer.
        /// </summary>
        /// <param name="source">The buffer to copy data from.</param>
        /// <param name="sourceOffset">The index of the first element to copy from the source buffer.</param>
        /// <param name="dest">The buffer to write data to.</param>
        /// <param name="destOffset">The index of of the first element to write on the dest buffer.</param>
        /// <param name="dataLength">The amount of elements to copy.</param>
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
        /// Copies all the data from a source buffer to a destination buffer.
        /// </summary>
        /// <param name="source">The buffer to copy data from.</param>
        /// <param name="dest">The buffer to write data to.</param>
        public static void CopyBuffers(DataBufferSubset<T> source, DataBufferSubset<T> dest)
        {
            CopyBuffers(source, 0, dest, 0, source.StorageLength);
        }
    }

    /// <summary>
    /// This interface is used to be able to access properties and methods of a
    /// <see cref="DataBufferSubset{T}"/> without caring about it's type param.
    /// </summary>
    internal interface IDataBufferSubset
    {
        int StorageLength { get; }
        int ElementSize { get; }
    }
}
