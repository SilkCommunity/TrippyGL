using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// A <see cref="DataBufferSubset{T}"/> whose purpose is to store vertex data.
    /// </summary>
    /// <typeparam name="T">The type of struct (element) this <see cref="VertexDataBufferSubset{T}"/> will manage.</typeparam>
    public sealed class VertexDataBufferSubset<T> : DataBufferSubset<T> where T : unmanaged
    {
        /// <summary>
        /// Creates a <see cref="VertexDataBufferSubset{T}"/> with the given <see cref="BufferObject"/>
        /// and target, offset into the buffer in bytes, storage length in elements and optional initial data.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        /// <param name="storageOffsetBytes">The offset into the <see cref="BufferObject"/>'s storage where this subset begins.</param>
        /// <param name="storageLength">The length of this subset measured in elements.</param>
        /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the initial data to set to the subset, or empty.</param>
        /// <param name="dataWriteOffset">The offset into the subset's storage at which to start writting the initial data.</param>
        public VertexDataBufferSubset(BufferObject bufferObject, uint storageOffsetBytes, uint storageLength, ReadOnlySpan<T> data = default, uint dataWriteOffset = 0)
            : base(bufferObject, BufferTargetARB.ArrayBuffer, storageOffsetBytes, storageLength, data, dataWriteOffset)
        {

        }

        /// <summary>
        /// Creates a <see cref="VertexDataBufferSubset{T}"/> with the given <see cref="BufferObject"/>
        /// and target, with the subset covering the entire buffer's storage and optional initial data.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the initial data to set to the subset, or empty.</param>
        /// <param name="dataWriteOffset">The offset into the subset's storage at which to start writting the initial data.</param>
        public VertexDataBufferSubset(BufferObject bufferObject, ReadOnlySpan<T> data = default, uint dataWriteOffset = 0)
            : base(bufferObject, BufferTargetARB.ArrayBuffer, data, dataWriteOffset)
        {

        }
    }
}
