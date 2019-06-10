using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// IndexBufferObjects can be used to define the order in which a draw call's vertex are read. Vertex can therefore be laid out in any
    /// order inside the attrib source storage and ordered by an IndexBufferObject. This also allow you to repeat vertex multiple times.
    /// IndexBufferObjects are always bound to GL_ELEMENT_ARRAY_BUFFER
    /// </summary>
    public class IndexBufferObject : BufferObject
    {
        private DrawElementsType elementType;
        private int elementSizeInBytes;
        private int storageLengthInBytes;

        /// <summary>
        /// The type of element this IndexBufferObject stores.
        /// Changing this will change the meaning of the current data in the buffer's storage and the length of the storage when measured in elements
        /// </summary>
        public DrawElementsType ElementType
        {
            get { return this.elementType; }
            set
            {
                this.elementType = value;
                this.elementSizeInBytes = TrippyUtils.GetSizeInBytesOfType(value);
            }
        }

        /// <summary>Gets the length of the storage measured in elements</summary>
        public override int StorageLength { get { return storageLengthInBytes / elementSizeInBytes; } }

        /// <summary>Gets the length of the storage measured in bytes</summary>
        public override int StorageLengthInBytes { get { return storageLengthInBytes; } }

        /// <summary>Gets the amount of bytes each element occupies</summary>
        public override int ElementSize { get { return elementSizeInBytes; } }

        /// <summary>
        /// Creates an IndexBufferObject with the specified storage length. The storage is created but the data has no specified initial value
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="elementType">The type of elements the index buffer will use</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public IndexBufferObject(GraphicsDevice graphicsDevice, int storageLength, DrawElementsType elementType, BufferUsageHint usageHint)
            : base(graphicsDevice, BufferTarget.ElementArrayBuffer)
        {
            this.elementType = elementType;
            this.elementSizeInBytes = TrippyUtils.GetSizeInBytesOfType(elementType);
            this.storageLengthInBytes = storageLength * this.elementSizeInBytes;

            InitializeStorage(storageLengthInBytes, usageHint);
        }

        /// <summary>
        /// Creates an IndexBufferObject with the specified storage length and type UnsignedInt. Then initializes the storage data by copying it from a specified index of a given array.
        /// The entire storage must be filled with data, it cannot be only partly written by this constructor
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="dataOffset">The index of the first uint in the data array to start reading from</param>
        /// <param name="data">An array containing the data to be uploaded to the buffer's storage. Can't be null</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public IndexBufferObject(GraphicsDevice graphicsDevice, int storageLength, int dataOffset, uint[] data, BufferUsageHint usageHint)
            : base(graphicsDevice, BufferTarget.ElementArrayBuffer)
        {
            this.elementType = DrawElementsType.UnsignedInt;
            elementSizeInBytes = 4;
            storageLengthInBytes = storageLength * 4;

            InitializeStorage(storageLength, 4, dataOffset, data, usageHint);
        }

        /// <summary>
        /// Creates an IndexBufferObject with the specified storage length and type UnsignedShort. Then initializes the storage data by copying it from a specified index of a given array.
        /// The entire storage must be filled with data, it cannot be only partly written by this constructor
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="dataOffset">The index of the first ushort in the data array to start reading from</param>
        /// <param name="data">An array containing the data to be uploaded to the buffer's storage. Can't be null</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public IndexBufferObject(GraphicsDevice graphicsDevice, int storageLength, int dataOffset, ushort[] data, BufferUsageHint usageHint)
            : base(graphicsDevice, BufferTarget.ElementArrayBuffer)
        {
            this.elementType = DrawElementsType.UnsignedShort;
            elementSizeInBytes = 2;
            storageLengthInBytes = storageLength * 2;

            InitializeStorage(storageLength, 2, dataOffset, data, usageHint);
        }

        /// <summary>
        /// Creates an IndexBufferObject with the specified storage length and type UnsignedByte. Then initializes the storage data by copying it from a specified index of a given array.
        /// The entire storage must be filled with data, it cannot be only partly written by this constructor
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="storageLength">The length (measured in bytes) of the buffer storage</param>
        /// <param name="dataOffset">The index of the first byte in the data array to start reading from</param>
        /// <param name="data">An array containing the data to be uploaded to the buffer's storage. Can't be null</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public IndexBufferObject(GraphicsDevice graphicsDevice, int storageLength, int dataOffset, byte[] data, BufferUsageHint usageHint)
            : base(graphicsDevice, BufferTarget.ElementArrayBuffer)
        {
            this.elementType = DrawElementsType.UnsignedByte;
            elementSizeInBytes = 1;
            storageLengthInBytes = storageLength;

            InitializeStorage(storageLength, 1, dataOffset, data, usageHint);
        }

        /// <summary>
        /// Sets the data for this IndexBufferObject as UnsignedInt type. The data will be uploaded no matter this object's ElementType,
        /// but that doesn't mean the data will be correct, so you should call the appropiate SetData for your element type
        /// </summary>
        /// <param name="storageOffset">The first UnsignedInt element's index in the buffer to write</param>
        /// <param name="dataOffset">The index of the first uint from the data array to start reading from</param>
        /// <param name="dataLength">The amount of uint-s to copy from the data array to the buffer</param>
        /// <param name="data">The array containing the data to upload</param>
        public void SetData(int storageOffset, int dataOffset, int dataLength, uint[] data)
        {
            ValidateSetParams(storageOffset, dataOffset, dataLength, storageLengthInBytes / 4, data);

            GraphicsDevice.BindBuffer(this);
            GL.BufferSubData(this.BufferTarget, (IntPtr)(storageOffset * 4), dataLength * 4, ref data[dataOffset]);
        }

        /// <summary>
        /// Sets the data for this IndexBufferObject as UnsignedShort type. The data will be uploaded no matter this object's ElementType,
        /// but that doesn't mean the data will be correct, so you should call the appropiate SetData for your element type
        /// </summary>
        /// <param name="storageOffset">The first UnsignedShort element's index in the buffer to write</param>
        /// <param name="dataOffset">The index of the first ushort from the data array to start reading from</param>
        /// <param name="dataLength">The amount of ushort-s to copy from the data array to the buffer</param>
        /// <param name="data">The array containing the data to upload</param>
        public void SetData(int storageOffset, int dataOffset, int dataLength, ushort[] data)
        {
            ValidateSetParams(storageOffset, dataOffset, dataLength, storageLengthInBytes / 2, data);

            GraphicsDevice.BindBuffer(this);
            GL.BufferSubData(this.BufferTarget, (IntPtr)(storageOffset * 2), dataLength * 2, ref data[dataOffset]);
        }

        /// <summary>
        /// Sets the data for this IndexBufferObject as UnsignedByte type. The data will be uploaded no matter this object's ElementType,
        /// but that doesn't mean the data will be correct, so you should call the appropiate SetData for your element type
        /// </summary>
        /// <param name="storageOffset">The first UnsignedByte's index in the buffer to write</param>
        /// <param name="dataOffset">The index of the first byte from the data array to start reading from</param>
        /// <param name="dataLength">The amount of bytes to copy from the data array to the buffer</param>
        /// <param name="data">The array containing the data to upload</param>
        public void SetData(int storageOffset, int dataOffset, int dataLength, byte[] data)
        {
            ValidateSetParams(storageOffset, dataOffset, dataLength, storageLengthInBytes, data);

            GraphicsDevice.BindBuffer(this);
            GL.BufferSubData(this.BufferTarget, (IntPtr)storageOffset, dataLength, ref data[dataOffset]);
        }

        /// <summary>
        /// Gets the data stored in this buffer's storage as UnsignedInt type. The data will be read as uint no matter this object's Element Type,
        /// but that doesn't mean the data will be correct, so you should call the appropiate GetData for your element type
        /// </summary>
        /// <param name="storageOffset">The first UnsignedInt's index in the buffer to read</param>
        /// <param name="dataOffset">The index of the first uint to read from the buffer's storage</param>
        /// <param name="dataLength">The amount of ushorts to copy from the buffer storage into the data array</param>
        /// <param name="data">The array in which to write the recieved storage data</param>
        public void GetData(int storageOffset, int dataOffset, int dataLength, uint[] data)
        {
            ValidateGetParams(storageOffset, dataOffset, dataLength, storageLengthInBytes / 4, data);

            GraphicsDevice.BindBuffer(this);
            GL.GetBufferSubData(this.BufferTarget, (IntPtr)(storageOffset * 4), dataLength * 4, ref data[dataOffset]);
        }

        /// <summary>
        /// Gets the data stored in this buffer's storage as UnsignedShort type. The data will be read as ushort no matter this object's Element Type,
        /// but that doesn't mean the data will be correct, so you should call the appropiate GetData for your element type
        /// </summary>
        /// <param name="storageOffset">The first UnsignedShort's index in the buffer to read</param>
        /// <param name="dataOffset">The index of the first ushort to read from the buffer's storage</param>
        /// <param name="dataLength">The amount of ushorts to copy from the buffer storage into the data array</param>
        /// <param name="data">The array in which to write the recieved storage data</param>
        public void GetData(int storageOffset, int dataOffset, int dataLength, ushort[] data)
        {
            ValidateGetParams(storageOffset, dataOffset, dataLength, storageLengthInBytes / 2, data);

            GraphicsDevice.BindBuffer(this);
            GL.GetBufferSubData(this.BufferTarget, (IntPtr)(storageOffset * 2), dataLength * 2, ref data[dataOffset]);
        }

        /// <summary>
        /// Gets the data stored in this buffer's storage as UnsignedByte type. The data will be read as uint no matter this object's Element Type,
        /// but that doesn't mean the data will be correct, so you should call the appropiate GetData for your element type
        /// </summary>
        /// <param name="storageOffset">The first UnsignedByte's index in the buffer to read</param>
        /// <param name="dataOffset">The index of the first byte to read from the buffer's storage</param>
        /// <param name="dataLength">The amount of bytes to copy from the buffer storage into the data array</param>
        /// <param name="data">The array in which to write the recieved storage data</param>
        public void GetData(int storageOffset, int dataOffset, int dataLength, byte[] data)
        {
            ValidateGetParams(storageOffset, dataOffset, dataLength, storageLengthInBytes, data);

            GraphicsDevice.BindBuffer(this);
            GL.GetBufferSubData(this.BufferTarget, (IntPtr)storageOffset, dataLength, ref data[dataOffset]);
        }

        public override string ToString()
        {
            return String.Concat("Handle=", Handle.ToString(), "; StorageLength=", StorageLength.ToString(), "; ElementType=", elementType.ToString(), "; ElementSizeInBytes=", elementSizeInBytes.ToString(), "; IsCurrentlyBound=", IsCurrentlyBound.ToString());
        }

    }
}