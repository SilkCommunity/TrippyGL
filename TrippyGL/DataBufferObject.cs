using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    /// <summary>
    /// DataBufferObjects are used to manage a buffer storage on the GPU.
    /// This abstract class contains shared code amoung different BufferObjects that always use the same struct for data format
    /// </summary>
    /// <typeparam name="T">The type of struct the buffer will manage. This type is used to help you upload data in a format that isn't byte array</typeparam>
    public class DataBufferObject<T> : BufferObject where T : struct
    {
        /// <summary>The length of the buffer object's storage, measured in elements</summary>
        private int storageLength;

        /// <summary>The size of each element, measured in bytes</summary>
        private int elementSize;

        /// <summary>The length of the buffer object's storage, measured in elements</summary>
        public override int StorageLength { get { return storageLength; } }

        /// <summary>The length of the buffer object's storage, measured in bytes</summary>
        public override int StorageLengthInBytes { get { return storageLength * elementSize; } }

        /// <summary>The size of each element in the buffer object's storage measured in bytes</summary>
        public override int ElementSize { get { return elementSize; } }

        /// <summary>The usage hint with which this buffer was created</summary>
        public BufferUsageHint UsageHint { get; private set; }

        /// <summary>
        /// Creates a DataBufferObject with the specified storage length and initializes the storage data by copying it from a specified index of a given array.
        /// The entire storage must be filled with data, it cannot be only partly written by this constructor
        /// </summary>
        /// <param name="storageLength">The length of the buffer storage measured in elements</param>
        /// <param name="dataOffset">The first element of the given data array to start reading from</param>
        /// <param name="data">An array containing the data to be uploaded to the buffer's storage. Can't be null</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        internal DataBufferObject(BufferTarget bufferTarget, int storageLength, int dataOffset, T[] data, BufferUsageHint usageHint) : base(BufferTarget.ArrayBuffer)
        {
            this.storageLength = storageLength;
            this.elementSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
            this.UsageHint = usageHint;

            InitializeStorage(this.storageLength, this.elementSize, dataOffset, data, usageHint);
        }

        /// <summary>
        /// Creates a VertexDataBufferObject with the specified storage length and initializes the storage data by copying it from a given array.
        /// The entire storage must be filled with data, it cannot be only partly written by this constructor
        /// </summary>
        /// <param name="storageLength">The length of the buffer storage measured in elements</param>
        /// <param name="data">An array containing the data to be uploaded to the buffer's storage. Can't be null</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        internal DataBufferObject(BufferTarget bufferTarget, int storageLength, T[] data, BufferUsageHint usageHint)
            : this(bufferTarget, storageLength, 0, data, usageHint)
        {

        }

        /// <summary>
        /// Creates a VertexDataBufferObject with the specified storage length. The storage is created but the data has no specified initial value
        /// </summary>
        /// <param name="storageLength">The length of the buffer storage measured in elements</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        internal DataBufferObject(BufferTarget bufferTarget, int storageLength, BufferUsageHint usageHint) : base(bufferTarget)
        {
            this.elementSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
            this.storageLength = storageLength;
            this.UsageHint = usageHint;

            InitializeStorage(storageLength * elementSize, usageHint);
        }

        /// <summary>
        /// Writes part or all of the buffer object's storage
        /// </summary>
        /// <param name="storageOffset">The index of the first element in the buffer's storage to write</param>
        /// <param name="dataOffset">The index of the first element from the specified data array to start reading from</param>
        /// <param name="dataLength">The amount of elements to copy from the data array to the buffer</param>
        /// <param name="data">The array containing the data to upload</param>
        public void SetData(int storageOffset, int dataOffset, int dataLength, T[] data)
        {
            ValidateSetParams(storageOffset, dataOffset, dataLength, storageLength, data);

            EnsureBound();
            GL.BufferSubData(this.BufferTarget, (IntPtr)(storageOffset * this.elementSize), dataLength * this.elementSize, ref data[dataOffset]);
        }

        /// <summary>
        /// Gets part or all of the data stored in this buffer object's storage
        /// </summary>
        /// <param name="storageOffset">The index of the first element in the buffer's storage to read</param>
        /// <param name="dataOffset">The index of the first element from the specified data array to writting to</param>
        /// <param name="dataLength">The amount of elements to copy from the buffer's storage to the data array</param>
        /// <param name="data">The array in which to write the recieved data</param>
        public void GetData(int storageOffset, int dataOffset, int dataLength, T[] data)
        {
            ValidateGetParams(storageOffset, dataOffset, dataLength, storageLength, data);

            EnsureBound();
            GL.GetBufferSubData(this.BufferTarget, (IntPtr)(storageOffset * this.elementSize), dataLength * this.elementSize, ref data[dataOffset]);
        }

        /// <summary>
        /// Recreates the storage for this buffer object with a new specified length, initial data and usage hint.
        /// The specified amount of data must be enough to fill the entire storage
        /// </summary>
        /// <param name="storageLength">The new length for the buffer's storage</param>
        /// <param name="dataOffset">The index of the first element to read from the data array</param>
        /// <param name="data">The array containing the data to upload</param>
        /// <param name="usageHint">The new usage hint for this buffer</param>
        public void RecreateStorage(int storageLength, int dataOffset, T[] data, BufferUsageHint usageHint)
        {
            InitializeStorage(storageLength, this.elementSize, dataOffset, data, usageHint);
            this.storageLength = storageLength;
            this.UsageHint = usageHint;
        }

        /// <summary>
        /// Recreates the storage for this buffer object with a new specified length and initial data.
        /// The specified amount of data must be enough to fill the entire storage
        /// </summary>
        /// <param name="storageLength">The new length for the buffer's storage</param>
        /// <param name="dataOffset">The index of the first element to read from the data array</param>
        /// <param name="data">The array containing the data to upload</param>
        public void RecreateStorage(int storageLength, int dataOffset, T[] data)
        {
            InitializeStorage(storageLength, this.elementSize, dataOffset, data, this.UsageHint);
            this.storageLength = storageLength;
        }

        /// <summary>
        /// Recreates the storage for this buffer oject with a new specified length and usage hint.
        /// All the context in the previous storage is lost after this operation and the new storage's data is undefined
        /// </summary>
        /// <param name="storageLength">The new length for the buffer's storage</param>
        /// <param name="usageHint">The new usage hint for this buffer</param>
        public void RecreateStorage(int storageLength, BufferUsageHint usageHint)
        {
            InitializeStorage(storageLength * this.elementSize, usageHint);
            this.storageLength = storageLength;
            this.UsageHint = usageHint;
        }

        /// <summary>
        /// Recreates the storage for this buffer object with a new specified length.
        /// All the content in the previous storage is lost after this operation and the new storage's data is undefined
        /// </summary>
        /// <param name="storageLength">The new length for the buffer's storage</param>
        public void RecreateStorage(int storageLength)
        {
            InitializeStorage(storageLength * this.elementSize, this.UsageHint);
            this.storageLength = storageLength;
        }

        public override string ToString()
        {
            return String.Concat("Handle=", Handle, ". StorageLength=", StorageLength, ", ElementSize=", ElementSize, " StructType=", typeof(T), " IsCurrentlyBound=", IsCurrentlyBound);
        }
    }
}
