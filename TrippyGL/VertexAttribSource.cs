using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    /// <summary>
    /// Describes the source for a vertex attribute's data. This stores a VertexAttribDescription about the attribute and a BufferObject, which is where the data will come from
    /// </summary>
    public class VertexAttribSource
    {
        /// <summary>The VertexDataBufferObject from which the vertex attributes will be read</summary>
        public readonly BufferObject DataBuffer;

        /// <summary>The description of the vertex attribute</summary>
        public readonly VertexAttribDescription AttribDescription;

        /// <summary>
        /// Creates a VertexAttribSource with a given BufferObject and the given VertexAttribDescription
        /// </summary>
        /// <param name="dataBuffer">The BufferObject where the vertex attrib data is located</param>
        /// <param name="attribDesc">The VertexAttribDescription describing the vertex attribute</param>
        public VertexAttribSource(BufferObject dataBuffer, VertexAttribDescription attribDesc)
        {
            if (dataBuffer == null)
                throw new ArgumentNullException("dataBuffer");

            if (dataBuffer.BufferTarget != BufferTarget.ArrayBuffer)
                throw new ArgumentException("The specified BufferObject must be usable as vertex attrib data. Try using a VertexDataBufferObject", "dataBuffer");

            this.DataBuffer = dataBuffer;
            this.AttribDescription = attribDesc;
        }

        /// <summary>
        /// Creates a VertexAttribSource with a given BufferObject and specifies a vertex attribute where the data format in the shader and the buffer match and don't need conversion
        /// </summary>
        /// <param name="dataBuffer">The BufferObject where the vertex attrib data is located</param>
        /// <param name="attribType">The type of attribute declared in the shader</param>
        public VertexAttribSource(BufferObject dataBuffer, ActiveAttribType attribType)
            : this(dataBuffer, new VertexAttribDescription(attribType))
        {

        }

        /// <summary>
        /// Creates a VertexAttribSource with a given BufferObject and specifies a vertex attribute where the data format in the shader and the buffer don't match and need conversion
        /// </summary>
        /// <param name="dataBuffer">The BufferObject where the vertex attrib data is located</param>
        /// <param name="attribType">The type of attribute declared in the shader</param>
        /// <param name="normalized">Whether the data needs to be normalized (uint/ushort/byte -> float between 0 and 1, or int/short/sbyte -> float between -1 and 1)</param>
        /// <param name="dataBaseType">The base type of the data found on the buffer. If normalized is true, this must be an integer type</param>
        public VertexAttribSource(BufferObject dataBuffer, ActiveAttribType attribType, bool normalized, VertexAttribPointerType dataBaseType)
            : this(dataBuffer, new VertexAttribDescription(attribType, normalized, dataBaseType))
        {

        }

        public override string ToString()
        {
            return String.Concat(AttribDescription.ToString(), " bufferHandle=", DataBuffer.Handle.ToString());
        }
    }
}
