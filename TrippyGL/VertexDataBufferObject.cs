using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// VertexDataBufferObjects are used to manage a buffer storage on the GPU. This buffer storage's purpose is to feed vertex attribute data
    /// </summary>
    /// <typeparam name="T">The type of struct the buffer will manage. This type is used to help you upload data in a format that isn't byte array</typeparam>
    public class VertexDataBufferObject<T> : IDisposable, IVertexArrayAttribSource where T : struct
    {
        /// <summary>The last buffer object's handle to be bound to GL_ARRAY_TARGET</summary>
        private static int lastBoundBuffer = -1;

        /// <summary>
        /// Resets the last bound buffer object variable. This variable is used on EnsureBound() to not call glBindBuffer if the buffer is already bound.
        /// You might want to call this if interoperating with another library
        /// </summary>
        public static void ResetBindState()
        {
            lastBoundBuffer = -1;
        }



        /// <summary>The GL Buffer Object's name</summary>
        public readonly int Handle;

        /// <summary>The size of each element in the buffer object's storage measured in bytes</summary>
        public readonly int StructSize;

        /// <summary>The amount of elements in the buffer object's storage</summary>
        public readonly int StorageLength;

        /// <summary>The length of the buffer object's storage measured in bytes</summary>
        public int StorageLengthInBytes { get { return StorageLength * StructSize; } }

        public bool IsCurrentlyBound { get { return lastBoundBuffer == Handle; } }
        
        /// <summary>
        /// Creates a VertexDataBufferObject with the specified storage length and initializes the storage data by copying it from a specified index of a given array.
        /// The entire storage must be filled with data, it cannot be only partly written by this constructor
        /// </summary>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="dataOffset">The first element of the given data array to start reading from</param>
        /// <param name="data">An array containing the data to be uploaded to the buffer's storage. Can't be null</param>
        /// <param name="bufferHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public VertexDataBufferObject(int storageLength, int dataOffset, T[] data, BufferUsageHint bufferHint)
        {
            if (data == null)
                throw new ArgumentNullException("data", "Data array can't be null. Try using another constructor if you don't want to set an initial value to the buffer storage");

            if (storageLength <= 0)
                throw new ArgumentOutOfRangeException("storageLength", storageLength, "Storage length must be a positive number");

            if (dataOffset < 0)
                throw new ArgumentOutOfRangeException("dataOffset", dataOffset, "Data offset must be equal or greater than 0");

            if (data.Length - dataOffset < storageLength)
                throw new ArgumentOutOfRangeException("There isn't enough data in the data array starting from dataOffset to fill the entire buffer storage");

            this.Handle = GL.GenBuffer();
            this.StructSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
            this.StorageLength = storageLength;

            EnsureBound();
            GL.BufferData(BufferTarget.ArrayBuffer, storageLength * StructSize, ref data[dataOffset], bufferHint);
        }

        /// <summary>
        /// Creates a VertexDataBufferObject with the specified storage length and initializes the storage data by copying it from a given array.
        /// The entire storage must be filled with data, it cannot be only partially written by this constructor
        /// </summary>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="data">An array containing the data to be uploaded to the buffer's storage. If null, the storage is created but no data is uploaded. If longer than necessary, only enough data to fill the storage will be read</param>
        /// <param name="bufferHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public VertexDataBufferObject(int storageLength, T[] data, BufferUsageHint bufferHint)
        {
            if (storageLength <= 0)
                throw new ArgumentOutOfRangeException("storageLength", storageLength, "Storage length must be a positive number");

            if (data != null && data.Length < storageLength)
                throw new ArgumentException("There isn't enough data in the array to fill the entire buffer", "data");

            this.Handle = GL.GenBuffer();
            this.StructSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
            this.StorageLength = storageLength;

            EnsureBound();
            GL.BufferData(BufferTarget.ArrayBuffer, storageLength * StructSize, data, bufferHint);
        }

        /// <summary>
        /// Creates a VertexDataBufferObject with the specified storage length. The storage is created but the data has no specified initial value
        /// </summary>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="bufferHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public VertexDataBufferObject(int storageLength, BufferUsageHint bufferHint)
        {
            if (storageLength <= 0)
                throw new ArgumentOutOfRangeException("storageLength", storageLength, "Storage length must be a positive number");

            this.Handle = GL.GenBuffer();
            this.StructSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
            this.StorageLength = storageLength;

            EnsureBound();
            GL.BufferData(BufferTarget.ArrayBuffer, storageLength * StructSize, IntPtr.Zero, bufferHint);
        }

        ~VertexDataBufferObject()
        {
            dispose();
        }

        /// <summary>
        /// Ensures this buffer object is currently bound to OpenGL. VertexDataBufferObjects always bind to GL_ARRAY_TARGET.
        /// </summary>
        public void EnsureBound()
        {
            if (lastBoundBuffer != Handle)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
                lastBoundBuffer = -1;
            }
        }

        /// <summary>
        /// Writes part or all of the buffer object's storage
        /// </summary>
        /// <param name="storageOffset">The first element index in the buffer's storage to modify</param>
        /// <param name="dataOffset">The first element from the specified data array to start reading from</param>
        /// <param name="dataLength">The amount of elements to read from the data array</param>
        /// <param name="data">The array containing the data to upload</param>
        public void SetData(int storageOffset, int dataOffset, int dataLength, T[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data", "The data array can't be null");

            if (storageOffset < 0 || storageOffset >= StorageLength)
                throw new ArgumentOutOfRangeException("storageOffset", storageOffset, "Storage offset must be in the range [0, StorageLength)");

            if (dataLength < 0)
                throw new ArgumentOutOfRangeException("dataLength", dataLength, "Data length must be equal or greater than 0");

            if (data.Length - dataOffset < dataLength)
                throw new ArgumentOutOfRangeException("There isn't enough data in the array to start from index dataOffset and read dataLength elements");

            if (dataLength > StorageLength - storageOffset)
                throw new ArgumentOutOfRangeException("The buffer's storage isn't big enough to write that amount of data starting at bufferOffset");

            EnsureBound();
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(storageOffset * StructSize), dataLength * StructSize, ref data[dataOffset]);
        }

        /// <summary>
        /// Writes part or all of the buffer object's storage. As much data as possible will be copied from the data array to the buffer object's storage.
        /// This means, if the given data array is longer, only enough to fill the buffer's storage will be read and vice versa
        /// </summary>
        /// <param name="data">The array containing the data to upload</param>
        public void SetData(T[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data", "The data array can't be null");

            EnsureBound();
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, Math.Min(data.Length, StorageLength) * StructSize, data);
        }

        public override string ToString()
        {
            return String.Concat("Handle=", Handle, ". StorageLength=", StorageLength, ", StructSize=", StructSize, " StructType=", typeof(T), " CurrentlyBound=", IsCurrentlyBound);
        }

        private void dispose()
        {
            GL.DeleteBuffer(Handle);
        }

        /// <summary>
        /// Disposes the VertexDataBufferObject, deleting and releasing the memory used by the storage buffer.
        /// The object cannot be used anymore after it has been disposed
        /// </summary>
        public void Dispose()
        {
            dispose();
            GC.SuppressFinalize(this);
        }
    }
}
