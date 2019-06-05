using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    /// <summary>
    /// VertexDataBufferObjects are used to manage a buffer storage on the GPU.
    /// This buffer storage's purpose is to feed vertex attribute data, so it is always bound to GL_ARRAY_BUFFER
    /// </summary>
    /// <typeparam name="T">The type of struct the buffer will manage. This type is used to help you upload data in a format that isn't byte array</typeparam>
    public class VertexDataBufferObject<T> : DataBufferObject<T> where T : struct
    {
        /// <summary>
        /// Creates a VertexDataBufferObject with the specified storage length and initializes the storage data by copying it from a specified index of a given array.
        /// The entire storage must be filled with data, it cannot be only partly written by this constructor
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="dataOffset">The first element of the given data array to start reading from</param>
        /// <param name="data">An array containing the data to be uploaded to the buffer's storage. Can't be null</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public VertexDataBufferObject(GraphicsDevice graphicsDevice, int storageLength, int dataOffset, T[] data, BufferUsageHint usageHint)
            : base(graphicsDevice, BufferTarget.ArrayBuffer, storageLength, dataOffset, data, usageHint)
        {

        }

        /// <summary>
        /// Creates a VertexDataBufferObject with the specified storage length and initializes the storage data by copying it from a given array.
        /// The entire storage must be filled with data, it cannot be only partly written by this constructor
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="data">An array containing the data to be uploaded to the buffer's storage. Can't be null</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public VertexDataBufferObject(GraphicsDevice graphicsDevice, int storageLength, T[] data, BufferUsageHint usageHint)
            : base(graphicsDevice, BufferTarget.ArrayBuffer, storageLength, 0, data, usageHint)
        {

        }

        /// <summary>
        /// Creates a VertexDataBufferObject with the specified storage data. The data array not only gives the initial data, but the size of the storage
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="data">The data with which to initialize the buffer's storage. This also marks the storage's length</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public VertexDataBufferObject(GraphicsDevice graphicsDevice, T[] data, BufferUsageHint usageHint)
            : this(graphicsDevice, data.Length, 0, data, usageHint)
        {

        }

        /// <summary>
        /// Creates a VertexDataBufferObject with the specified storage length. The storage is created but the data has no specified initial value
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="storageLength">The length (measured in elements) of the buffer storage</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public VertexDataBufferObject(GraphicsDevice graphicsDevice, int storageLength, BufferUsageHint usageHint)
            : base(graphicsDevice, BufferTarget.ArrayBuffer, storageLength, usageHint)
        {

        }
    }
}
