using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// IndexBufferObjects can be used to define the order in which a draw call's vertex are read. Vertex can therefore be laid out in any
    /// order inside the attrib source storage and ordered by an IndexBufferObject. This also allow you to repeat vertex multiple times.
    /// IndexBufferObjects are always bound to GL_ELEMENT_ARRAY_BUFFER
    /// </summary>
    public class IndexBufferObject : IDisposable
    {
        /// <summary>The last buffer object's name to be bound to GL_ELEMENT_ARRAY</summary>
        private static int lastBoundBuffer = -1;

        /// <summary>
        /// Resets the last bound buffer object variable. This variable is used on EnsureBound() to not call glBindBuffer if the buffer is already bound.
        /// You might want to call this if interoperating with another library
        /// </summary>
        public static void ResetBindState()
        {
            lastBoundBuffer = -1;
        }

        /// <summary>
        /// Unbinds any IndexBufferObject by binding to buffer 0
        /// </summary>
        public static void BindEmpty()
        {
            lastBoundBuffer = 0;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }



        /// <summary>The GL Buffer Object's name</summary>
        public readonly int Handle;

        private DrawElementsType elementType;
        private int elementSizeInBytes;
        private int storageLengthInBytes;

        /// <summary>
        /// The type of element this IndexBufferObject stores.
        /// Changing this will change the meaning of the current data in the buffer's storage and the length of the storage measured in elements
        /// </summary>
        public DrawElementsType ElementType
        {
            get { return this.elementType; }
            set
            {
                this.elementType = value;
                this.elementSizeInBytes = GetSizeInBytesOfType(value);
            }
        }

        /// <summary>Gets the length of the storage measured in elements</summary>
        public int StorageLength { get { return storageLengthInBytes / elementSizeInBytes; } }

        /// <summary>Gets the length of the storage measured in bytes</summary>
        public int StorageLengthInBytes { get { return storageLengthInBytes; } }

        /// <summary>Gets the amount of bytes each element occupies</summary>
        public int ElementSizeInBytes { get { return elementSizeInBytes; } }

        /// <summary>Gets whether this IndexBufferObject is the currently bound one</summary>
        public bool IsCurrentlyBound { get { return lastBoundBuffer == Handle; } }

        /// <summary>
        /// Creates an IndexBufferObject with the specified storage length. The storage is created but the data has no specified initial value
        /// </summary>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="elementType">The type of elements the index buffer will use</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public IndexBufferObject(int storageLength, DrawElementsType elementType, BufferUsageHint usageHint)
        {
            if (storageLength <= 0)
                throw new ArgumentOutOfRangeException("storageLength", storageLength, "Storage length must be greater than 0");

            Handle = GL.GenBuffer();
            this.elementType = elementType;
            elementSizeInBytes = GetSizeInBytesOfType(elementType);
            storageLengthInBytes = storageLength * elementSizeInBytes;
            
            EnsureBound();
            GL.BufferData(BufferTarget.ElementArrayBuffer, storageLengthInBytes, IntPtr.Zero, usageHint);
        }

        /// <summary>
        /// Creates an IndexBufferObject with the specified storage length and type UnsignedInt. Then initializes the storage data by copying it from a specified index of a given array.
        /// The entire storage must be filled with data, it cannot be only partly written by this constructor
        /// </summary>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="dataOffset">The index of the first uint in the data array to start reading from</param>
        /// <param name="data">An array containing the data to be uploaded to the buffer's storage. Can't be null</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public IndexBufferObject(int storageLength, int dataOffset, uint[] data, BufferUsageHint usageHint)
        {
            if (data == null)
                throw new ArgumentNullException("data", "Data array can't be null. Try using another constructor if you don't want to set an initial value to the buffer storage");

            if (storageLength <= 0)
                throw new ArgumentOutOfRangeException("storageLength", storageLength, "Storage length must be greater than 0");

            if (dataOffset < 0 || dataOffset >= data.Length)
                throw new ArgumentOutOfRangeException("dataOffset", dataOffset, "Data offset must be in the range [0, data.Length)");

            if (data.Length - dataOffset < storageLength)
                throw new ArgumentException("There isn't enough data in the array to start from index dataOffset and read storageLength elements");

            Handle = GL.GenBuffer();
            this.elementType = DrawElementsType.UnsignedInt;
            elementSizeInBytes = 4;
            storageLengthInBytes = storageLength * 4;

            EnsureBound();
            GL.BufferData(BufferTarget.ElementArrayBuffer, storageLengthInBytes, ref data[dataOffset], usageHint);
        }

        /// <summary>
        /// Creates an IndexBufferObject with the specified storage length and type UnsignedShort. Then initializes the storage data by copying it from a specified index of a given array.
        /// The entire storage must be filled with data, it cannot be only partly written by this constructor
        /// </summary>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="dataOffset">The index of the first ushort in the data array to start reading from</param>
        /// <param name="data">An array containing the data to be uploaded to the buffer's storage. Can't be null</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public IndexBufferObject(int storageLength, int dataOffset, ushort[] data, BufferUsageHint usageHint)
        {
            if (data == null)
                throw new ArgumentNullException("data", "Data array can't be null. Try using another constructor if you don't want to set an initial value to the buffer storage");

            if (storageLength <= 0)
                throw new ArgumentOutOfRangeException("storageLength", storageLength, "Storage length must be greater than 0");

            if (dataOffset < 0 || dataOffset >= data.Length)
                throw new ArgumentOutOfRangeException("dataOffset", dataOffset, "Data offset must be in the range [0, data.Length)");

            if (data.Length - dataOffset < storageLength)
                throw new ArgumentException("There isn't enough data in the array to start from index dataOffset and read storageLength elements");

            Handle = GL.GenBuffer();
            this.elementType = DrawElementsType.UnsignedShort;
            elementSizeInBytes = 2;
            storageLengthInBytes = storageLength * 2;

            EnsureBound();
            GL.BufferData(BufferTarget.ElementArrayBuffer, storageLengthInBytes, ref data[dataOffset], usageHint);
        }

        /// <summary>
        /// Creates an IndexBufferObject with the specified storage length and type UnsignedByte. Then initializes the storage data by copying it from a specified index of a given array.
        /// The entire storage must be filled with data, it cannot be only partly written by this constructor
        /// </summary>
        /// <param name="storageLength">The length (measured in bytes) of the buffer storage</param>
        /// <param name="dataOffset">The index of the first byte in the data array to start reading from</param>
        /// <param name="data">An array containing the data to be uploaded to the buffer's storage. Can't be null</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public IndexBufferObject(int storageLength, int dataOffset, byte[] data, BufferUsageHint usageHint)
        {
            if (data == null)
                throw new ArgumentNullException("data", "Data array can't be null. Try using another constructor if you don't want to set an initial value to the buffer storage");

            if (storageLength <= 0)
                throw new ArgumentOutOfRangeException("storageLength", storageLength, "Storage length must be greater than 0");

            if (dataOffset < 0 || dataOffset >= data.Length)
                throw new ArgumentOutOfRangeException("dataOffset", dataOffset, "Data offset must be in the range [0, data.Length)");

            if (data.Length - dataOffset < storageLength)
                throw new ArgumentException("There isn't enough data in the array to start from index dataOffset and read storageLength elements");

            Handle = GL.GenBuffer();
            this.elementType = DrawElementsType.UnsignedByte;
            elementSizeInBytes = 1;
            storageLengthInBytes = storageLength;

            EnsureBound();
            GL.BufferData(BufferTarget.ElementArrayBuffer, storageLengthInBytes, ref data[dataOffset], usageHint);
        }
        
        ~IndexBufferObject()
        {
            if (TrippyLib.isLibActive)
                dispose();
        }

        /// <summary>
        /// Ensures this buffer object is currently bound to OpenGL. IndexBufferObjects always bind to GL_ELEMENT_ARRAY_BUFFER.
        /// </summary>
        public void EnsureBound()
        {
            if(lastBoundBuffer != Handle)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
                lastBoundBuffer = Handle;
            }
        }

        /// <summary>
        /// Sets the data for this IndexBufferObject as UnsignedInt type. The data will be uploaded no matter this object's ElementType,
        /// but that doesn't mean the data will be correct, so you should call the appropiate SetData for your element type.
        /// </summary>
        /// <param name="storageOffset">The first UnsignedInt element's index in the buffer to write</param>
        /// <param name="dataOffset">The index of the first uint from the data array to start reading from</param>
        /// <param name="dataLength">The amount of uint-s to copy from the data array to the buffer</param>
        /// <param name="data">The array containing the data to upload</param>
        public void SetData(int storageOffset, int dataOffset, int dataLength, uint[] data)
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
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)(storageOffset * 4), dataLength * 4, ref data[dataOffset]);
        }

        /// <summary>
        /// Sets the data for this IndexBufferObject as UnsignedShort type. The data will be uploaded no matter this object's ElementType,
        /// but that doesn't mean the data will be correct, so you should call the appropiate SetData for your element type.
        /// </summary>
        /// <param name="storageOffset">The first UnsignedShort element's index in the buffer to write</param>
        /// <param name="dataOffset">The index of the first ushort from the data array to start reading from</param>
        /// <param name="dataLength">The amount of ushort-s to copy from the data array to the buffer</param>
        /// <param name="data">The array containing the data to upload</param>
        public void SetData(int storageOffset, int dataOffset, int dataLength, ushort[] data)
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
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)(storageOffset * 2), dataLength * 2, ref data[dataOffset]);
        }

        /// <summary>
        /// Sets the data for this IndexBufferObject as UnsignedByte type. The data will be uploaded no matter this object's ElementType,
        /// but that doesn't mean the data will be correct, so you should call the appropiate SetData for your element type.
        /// </summary>
        /// <param name="storageOffset">The first UnsignedByte's index in the buffer to write</param>
        /// <param name="dataOffset">The index of the first byte from the data array to start reading from</param>
        /// <param name="dataLength">The amount of bytes to copy from the data array to the buffer</param>
        /// <param name="data">The array containing the data to upload</param>
        public void SetData(int storageOffset, int dataOffset, int dataLength, byte[] data)
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
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)storageOffset, dataLength, ref data[dataOffset]);
        }

        private void dispose()
        {
            GL.DeleteBuffer(Handle);
        }

        /// <summary>
        /// Disposes the IndexBufferObject, deleting and releasing the memory used by the storage buffer.
        /// The object cannot be used anymore after it has been disposed
        /// </summary>
        public void Dispose()
        {
            dispose();
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return String.Concat("Handle=", Handle, "; StorageLength=", StorageLength, "; ElementType=", elementType, "; ElementSizeInBytes=", elementSizeInBytes, "; IsCurrentlyBound=", IsCurrentlyBound);
        }

        internal int GetSizeInBytesOfType(DrawElementsType type)
        {
            switch (type)
            {
                case DrawElementsType.UnsignedByte:
                    return 1;
                case DrawElementsType.UnsignedShort:
                    return 2;
                case DrawElementsType.UnsignedInt:
                    return 4;
            }

            throw new ArgumentException("That's not a valid DrawElementsType value");
        }
    }
}