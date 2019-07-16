using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// A DataBufferSubset whose purpose is to store vertex data
    /// </summary>
    /// <typeparam name="T">The type of struct the vertex will use</typeparam>
    public class VertexDataBufferSubset<T> : DataBufferSubset<T> where T : struct
    {
        /// <summary>
        /// Creates a VertexDataBufferSubset with the given BufferObject, offset into the buffer in bytes and storage length in elements
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        /// <param name="storageOffsetBytes">The offset into the buffer's storage where this subset begins</param>
        /// <param name="storageLength">The length of this subset measured in elements</param>
        public VertexDataBufferSubset(BufferObject bufferObject, int storageOffsetBytes, int storageLength)
            : base(bufferObject, BufferTarget.ArrayBuffer, storageOffsetBytes, storageLength)
        {

        }

        /// <summary>
        /// Creates a VertexDataBufferSubset with the given BufferObject, offset into the buffer in bytes and storage length in elements
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        /// <param name="data">An array containing the initial data to set to the subset</param>
        /// <param name="dataOffset">The offset into the data array to start reading values from</param>
        /// <param name="storageOffsetBytes">The offset into the buffer's storage where this subset begins</param>
        /// <param name="storageLength">The length of this subset measured in elements</param>
        public VertexDataBufferSubset(BufferObject bufferObject, T[] data, int dataOffset, int storageOffsetBytes, int storageLength)
            : base(bufferObject, BufferTarget.ArrayBuffer, data, dataOffset, storageOffsetBytes, storageLength)
        {

        }

        /// <summary>
        /// Creates a VertexDataBufferSubset with the given BufferObject, with the subset covering the entire buffer's storage
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        public VertexDataBufferSubset(BufferObject bufferObject)
            : base(bufferObject, BufferTarget.ArrayBuffer)
        {

        }

        /// <summary>
        /// Creates a VertexDataBufferSubset with the given BufferObject, with the subset covering the entire buffer's storage
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        /// <param name="data">An array containing the initial data to set to the subset</param>
        /// <param name="dataOffset">The offset into the data array to start reading values from</param>
        public VertexDataBufferSubset(BufferObject bufferObject, T[] data, int dataOffset = 0) : base(bufferObject, BufferTarget.ArrayBuffer, data, dataOffset)
        {

        }

        /// <summary>
        /// Creates a VertexDataBufferSubset that occupies the same area in the same buffer as another buffer subset
        /// </summary>
        /// <param name="copy">The BufferObjectSubset to copy the range from</param>
        public VertexDataBufferSubset(BufferObjectSubset copy) : base(copy, BufferTarget.ArrayBuffer)
        {

        }

        /// <summary>
        /// Creates a VertexDataBufferSubset that occupies the same area in the same buffer and uses the same struct type as another DataBufferSubset
        /// </summary>
        /// <param name="copy">The DataBufferSubset to copy the range from</param>
        public VertexDataBufferSubset(DataBufferSubset<T> copy) : base(copy, BufferTarget.ArrayBuffer)
        {

        }
    }
}
