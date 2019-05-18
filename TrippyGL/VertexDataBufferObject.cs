using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    /// <summary>
    /// VertexDataBufferObjects are used to manage a buffer storage on the GPU.
    /// This buffer storage's purpose is to feed vertex attribute data, so it is always bound to GL_ARRAY_BUFFER
    /// </summary>
    /// <typeparam name="T">The type of struct the buffer will manage. This type is used to help you upload data in a format that isn't byte array</typeparam>
    public class VertexDataBufferObject<T> : BufferObject where T : struct
    {
        /// <summary>The length of the buffer object's storage, measured in elements</summary>
        private readonly int storageLength;

        /// <summary>The size of each element, measured in bytes</summary>
        private readonly int elementSize;

        /// <summary>The length of the buffer object's storage, measured in elements</summary>
        public override int StorageLength { get { return storageLength; } }

        /// <summary>The length of the buffer object's storage, measured in bytes</summary>
        public override int StorageLengthInBytes { get { return storageLength * elementSize; } }

        /// <summary>The size of each element in the buffer object's storage measured in bytes</summary>
        public override int ElementSize { get { return elementSize; } }

        /// <summary>
        /// Creates a VertexDataBufferObject with the specified storage length and initializes the storage data by copying it from a specified index of a given array.
        /// The entire storage must be filled with data, it cannot be only partly written by this constructor
        /// </summary>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="dataOffset">The first element of the given data array to start reading from</param>
        /// <param name="data">An array containing the data to be uploaded to the buffer's storage. Can't be null</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public VertexDataBufferObject(int storageLength, int dataOffset, T[] data, BufferUsageHint usageHint) : base(BufferTarget.ArrayBuffer)
        {
            this.storageLength = storageLength;
            this.elementSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();

            InitializeStorage(this.storageLength, this.elementSize, dataOffset, data, usageHint);
        }

        /// <summary>
        /// Creates a VertexDataBufferObject with the specified storage length and initializes the storage data by copying it from a given array.
        /// The entire storage must be filled with data, it cannot be only partly written by this constructor
        /// </summary>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="data">An array containing the data to be uploaded to the buffer's storage. Can't be null</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public VertexDataBufferObject(int storageLength, T[] data, BufferUsageHint usageHint)
            : this(storageLength, 0, data, usageHint)
        {

        }

        /// <summary>
        /// Creates a VertexDataBufferObject with the specified storage length. The storage is created but the data has no specified initial value
        /// </summary>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public VertexDataBufferObject(int storageLength, BufferUsageHint usageHint) : base(BufferTarget.ArrayBuffer)
        {
            this.elementSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
            this.storageLength = storageLength;

            InitializeStorage(storageLength * elementSize, usageHint);
        }

        /// <summary>
        /// Writes part or all of the buffer object's storage
        /// </summary>
        /// <param name="storageOffset">The first element index in the buffer's storage to modify</param>
        /// <param name="dataOffset">The index of the first element from the specified data array to start reading from</param>
        /// <param name="dataLength">The amount of elements to copy from the data array to the buffer</param>
        /// <param name="data">The array containing the data to upload</param>
        public void SetData(int storageOffset, int dataOffset, int dataLength, T[] data)
        {
            ValidateSetParams(storageOffset, dataOffset, dataLength, storageLength, data);

            EnsureBound();
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(storageOffset * ElementSize), dataLength * ElementSize, ref data[dataOffset]);
        }

        public override string ToString()
        {
            return String.Concat("Handle=", Handle, ". StorageLength=", StorageLength, ", ElementSize=", ElementSize, " StructType=", typeof(T), " IsCurrentlyBound=", IsCurrentlyBound);
        }
    }
}
