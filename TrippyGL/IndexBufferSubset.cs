#pragma warning disable CA1062 // Validate arguments of public methods
using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// A <see cref="BufferObjectSubset"/> whose purpose is to store index data.
    /// </summary>
    public sealed class IndexBufferSubset : BufferObjectSubset
    {
        private const uint SizeOfUint = sizeof(uint);
        private const uint SizeOfUshort = sizeof(ushort);
        private const uint SizeOfByte = sizeof(byte);

        /// <summary>The length of the subset's storage measured in elements.</summary>
        public uint StorageLength { get; private set; }

        /// <summary>The size of each element in the subset's storage measured in bytes.</summary>
        public readonly uint ElementSize;

        /// <summary>The type of element this <see cref="IndexBufferSubset"/> stores.</summary>
        public readonly DrawElementsType ElementType;

        /// <summary>
        /// Creates a new <see cref="IndexBufferSubset"/> with the given <see cref="BufferObject"/>
        /// and specified offset into the buffer, storage length and element type.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        /// <param name="storageOffsetBytes">The offset into the <see cref="BufferObject"/>'s storage where this subset begins.</param>
        /// <param name="storageLength">The length of this subset measured in elements.</param>
        /// <param name="elementType">The type of elements this index subset will use.</param>
        public IndexBufferSubset(BufferObject bufferObject, uint storageOffsetBytes, uint storageLength, DrawElementsType elementType)
            : base(bufferObject, BufferTargetARB.ElementArrayBuffer)
        {
            ElementType = elementType;
            ElementSize = GetSizeInBytesOfElementType(elementType);
            ResizeSubset(storageOffsetBytes, storageLength);
        }

        /// <summary>
        /// Creates an <see cref="IndexBufferSubset"/> with the given <see cref="BufferObject"/>,
        /// with the subset covering the entire <see cref="BufferObject"/>'s storage.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        /// <param name="elementType">The type of elements this index subset will use.</param>
        public IndexBufferSubset(BufferObject bufferObject, DrawElementsType elementType)
            : this(bufferObject, 0, bufferObject.StorageLengthInBytes, elementType)
        {

        }

        /// <summary>
        /// Creates a new <see cref="IndexBufferSubset"/> with the specified offset into the buffer,
        ///  storage length, <see cref="DrawElementsType.UnsignedInt"/> element type and optional initial data.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        /// <param name="storageOffsetBytes">The offset into the <see cref="BufferObject"/>'s storage where this subset begins.</param>
        /// <param name="storageLength">The length of this subset measured in elements.</param>
        /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the initial data to set to the subset, or empty.</param>
        /// <param name="dataWriteOffset">The offset into the subset's storage at which to start writting the initial data.</param>
        public IndexBufferSubset(BufferObject bufferObject, uint storageOffsetBytes, uint storageLength, ReadOnlySpan<uint> data, uint dataWriteOffset = 0)
            : this(bufferObject, storageOffsetBytes, storageLength, DrawElementsType.UnsignedInt)
        {
            if (!data.IsEmpty)
                SetData(data, dataWriteOffset);
        }

        /// <summary>
        /// Creates a new <see cref="IndexBufferSubset"/> with the specified offset into the buffer,
        /// storage length, <see cref="DrawElementsType.UnsignedShort"/> element type and initial data.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        /// <param name="storageOffsetBytes">The offset into the <see cref="BufferObject"/>'s storage where this subset begins.</param>
        /// <param name="storageLength">The length of this subset measured in elements.</param>
        /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the initial data to set to the subset, or empty.</param>
        /// <param name="dataWriteOffset">The offset into the subset's storage at which to start writting the initial data.</param>
        public IndexBufferSubset(BufferObject bufferObject, uint storageOffsetBytes, uint storageLength, ReadOnlySpan<ushort> data, uint dataWriteOffset = 0)
            : this(bufferObject, storageOffsetBytes, storageLength, DrawElementsType.UnsignedShort)
        {
            if (!data.IsEmpty)
                SetData(data, dataWriteOffset);
        }

        /// <summary>
        /// Creates a new <see cref="IndexBufferSubset"/> with the specified offset into the buffer,
        /// storage length, <see cref="DrawElementsType.UnsignedByte"/> element type and initial data.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        /// <param name="storageOffsetBytes">The offset into the <see cref="BufferObject"/>'s storage where this subset begins.</param>
        /// <param name="storageLength">The length of this subset measured in elements.</param>
        /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the initial data to set to the subset.</param>
        /// <param name="dataWriteOffset">The offset into the subset's storage at which to start writting the initial data.</param>
        public IndexBufferSubset(BufferObject bufferObject, uint storageOffsetBytes, uint storageLength, ReadOnlySpan<byte> data, uint dataWriteOffset = 0)
            : this(bufferObject, storageOffsetBytes, storageLength, DrawElementsType.UnsignedByte)
        {
            if (!data.IsEmpty)
                SetData(data, dataWriteOffset);
        }

        /// <summary>
        /// Sets the data of a specified part of this subset's storage.
        /// </summary>
        /// <param name="data">The <see cref="ReadOnlySpan{T}"/> containing the data to set.</param>
        /// <param name="storageOffset">The offset into the subset's storage to start writing to.</param>
        public unsafe void SetData(ReadOnlySpan<uint> data, uint storageOffset = 0)
        {
            ValidateCorrectElementType(DrawElementsType.UnsignedInt);
            ValidateSetParams(data.Length, storageOffset);

            Buffer.GraphicsDevice.BindBufferObject(Buffer);
            fixed (void* ptr = &data[0])
                Buffer.GL.BufferSubData(GraphicsDevice.DefaultBufferTarget, (int)(storageOffset * SizeOfUint + StorageOffsetInBytes), (uint)data.Length * SizeOfUint, ptr);
        }

        /// <summary>
        /// Sets the data of a specified part of this subset's storage.
        /// </summary>
        /// <param name="data">The <see cref="ReadOnlySpan{T}"/> containing the data to set.</param>
        /// <param name="storageOffset">The offset into the subset's storage to start writing to.</param>
        public unsafe void SetData(ReadOnlySpan<ushort> data, uint storageOffset = 0)
        {
            ValidateCorrectElementType(DrawElementsType.UnsignedShort);
            ValidateSetParams(data.Length, storageOffset);

            Buffer.GraphicsDevice.BindBufferObject(Buffer);
            fixed (void* ptr = &data[0])
                Buffer.GL.BufferSubData(GraphicsDevice.DefaultBufferTarget, (int)(storageOffset * SizeOfUshort + StorageOffsetInBytes), (uint)data.Length * SizeOfUshort, ptr);
        }

        /// <summary>
        /// Sets the data of a specified part of this subset's storage.
        /// </summary>
        /// <param name="data">The <see cref="ReadOnlySpan{T}"/> containing the data to set.</param>
        /// <param name="storageOffset">The offset into the subset's storage to start writing to.</param>
        public unsafe void SetData(ReadOnlySpan<byte> data, uint storageOffset = 0)
        {
            ValidateCorrectElementType(DrawElementsType.UnsignedByte);
            ValidateSetParams(data.Length, storageOffset);

            Buffer.GraphicsDevice.BindBufferObject(Buffer);
            fixed (void* ptr = &data[0])
                Buffer.GL.BufferSubData(GraphicsDevice.DefaultBufferTarget, (int)(storageOffset * SizeOfByte + StorageOffsetInBytes), (uint)data.Length * SizeOfByte, ptr);
        }

        /// <summary>
        /// Gets the data of a specified part of this subset's storage.
        /// </summary>
        /// <param name="data">The <see cref="Span{T}"/> to which the returned data will be written to.</param>
        /// <param name="storageOffset">The offset into the subset's storage to start reading from.</param>
        public void GetData(Span<uint> data, uint storageOffset = 0)
        {
            ValidateCorrectElementType(DrawElementsType.UnsignedInt);
            ValidateGetParams(data.Length, storageOffset);

            Buffer.GraphicsDevice.BindBufferObject(Buffer);
            Buffer.GL.GetBufferSubData(GraphicsDevice.DefaultBufferTarget, (int)(storageOffset * SizeOfUint + StorageOffsetInBytes), (uint)data.Length * SizeOfUint, data);
        }

        /// <summary>
        /// Gets the data of a specified part of this subset's storage.
        /// </summary>
        /// <param name="data">The <see cref="Span{T}"/> to which the returned data will be written to.</param>
        /// <param name="storageOffset">The offset into the subset's storage to start reading from.</param>
        public void GetData(Span<ushort> data, uint storageOffset = 0)
        {
            ValidateCorrectElementType(DrawElementsType.UnsignedShort);
            ValidateGetParams(data.Length, storageOffset);

            Buffer.GraphicsDevice.BindBufferObject(Buffer);
            Buffer.GL.GetBufferSubData(GraphicsDevice.DefaultBufferTarget, (int)(storageOffset * SizeOfUshort + StorageOffsetInBytes), (uint)data.Length * SizeOfUshort, data);
        }

        /// <summary>
        /// Gets the data of a specified part of this subset's storage.
        /// </summary>
        /// <param name="data">The <see cref="Span{T}"/> to which the returned data will be written to.</param>
        /// <param name="storageOffset">The offset into the subset's storage to start reading from.</param>
        public void GetData(Span<byte> data, uint storageOffset = 0)
        {
            ValidateCorrectElementType(DrawElementsType.UnsignedByte);
            ValidateGetParams(data.Length, storageOffset);

            Buffer.GraphicsDevice.BindBufferObject(Buffer);
            Buffer.GL.GetBufferSubData(GraphicsDevice.DefaultBufferTarget, (int)(storageOffset * SizeOfByte + StorageOffsetInBytes), (uint)data.Length * SizeOfByte, data);
        }

        /// <summary>
        /// Changes the subset location of this <see cref="IndexBufferSubset"/>.
        /// </summary>
        /// <param name="storageOffsetBytes">The offset into the buffer object's storage where this subset begins.</param>
        /// <param name="storageLength">The length of this subset measured in elements.</param>
        public void ResizeSubset(uint storageOffsetBytes, uint storageLength)
        {
            if (storageOffsetBytes % ElementSize != 0)
                throw new ArgumentException(nameof(storageOffsetBytes) + " should be a multiple of " + nameof(ElementSize), nameof(storageOffsetBytes));

            InitializeStorage(storageOffsetBytes, storageLength * ElementSize);
            StorageLength = storageLength;
        }

        /// <summary>
        /// Checks that this index buffer's <see cref="ElementType"/> is the specified one and throws an exception if it's not.
        /// </summary>
        /// <param name="elementType">The element type to compare.</param>
        private void ValidateCorrectElementType(DrawElementsType elementType)
        {
            if (elementType != ElementType)
                throw new InvalidOperationException("To perform this operation the " + nameof(IndexBufferSubset) + "'s " + nameof(ElementType) + " must be " + elementType.ToString());
        }

        /// <summary>
        /// Validates the parameters for a set operation.
        /// </summary>
        private void ValidateSetParams(int dataLength, uint storageOffset)
        {
            if (storageOffset < 0 || storageOffset >= StorageLength)
                throw new ArgumentOutOfRangeException(nameof(storageOffset), storageOffset, nameof(storageOffset) + " must be in the range [0, " + nameof(StorageLength) + ")");

            if (dataLength + storageOffset > StorageLength)
                throw new BufferCopyException("Tried to write past the subset's length");
        }

        /// <summary>
        /// Validates the parameters for a get operation.
        /// </summary>
        private void ValidateGetParams(int dataLength, uint storageOffset)
        {
            if (storageOffset < 0 || storageOffset >= StorageLength)
                throw new ArgumentOutOfRangeException(nameof(storageOffset), storageOffset, nameof(storageOffset) + " must be in the range [0, " + nameof(StorageLength) + ")");

            if (dataLength + storageOffset > StorageLength)
                throw new BufferCopyException("Tried to read past the subset's length");
        }

        /// <summary>
        /// Calculates the required storage length in bytes required for a UniformBufferSubset with the specified storage length.
        /// </summary>
        /// <param name="elementType">The desired element type for the index buffer.</param>
        /// <param name="storageLength">The desired length for the subset measured in elements.</param>
        public static uint CalculateRequiredSizeInBytes(DrawElementsType elementType, uint storageLength)
        {
            return GetSizeInBytesOfElementType(elementType) * storageLength;
        }

        /// <summary>
        /// Gets the size in bytes for one element of the specified type.
        /// If the provided type isn't GL_UNSIGNED_BYTE, GL_UNSIGNED_SHORT or GL_UNSIGNED_INT, this method throws an exception.
        /// </summary>
        /// <param name="type">The type of element.</param>
        public static uint GetSizeInBytesOfElementType(DrawElementsType elementType)
        {
            return elementType switch
            {
                DrawElementsType.UnsignedByte => SizeOfByte,
                DrawElementsType.UnsignedShort => SizeOfUshort,
                DrawElementsType.UnsignedInt => SizeOfUint,
                _ => throw new ArgumentException("Invalid " + nameof(DrawElementsType) + " value"),
            };
        }
    }
}