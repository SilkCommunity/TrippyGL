using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    /// <summary>
    /// Describes the source for a vertex attribute's data. This stores a VertexAttribDescription about the attribute and a buffer subset from which the attrib data will come from
    /// </summary>
    public class VertexAttribSource
    {
        /// <summary>The buffer subset from which the vertex attributes will be read</summary>
        public readonly BufferObjectSubset BufferSubset;

        /// <summary>The description of the vertex attribute</summary>
        public readonly VertexAttribDescription AttribDescription;

        /// <summary>Gets whether this VertexAttribSource is only used to indicate padding</summary>
        public bool IsPadding { get { return AttribDescription.IsPadding; } }

        /// <summary>
        /// Creates a VertexAttribSource with a given BufferObjectSubset and the given VertexAttribDescription
        /// </summary>
        /// <param name="bufferSubset">The BufferObjectSubset where the vertex attrib data is located. Must be a subset usable for vertex data</param>
        /// <param name="attribDesc">The VertexAttribDescription describing the vertex attribute</param>
        public VertexAttribSource(BufferObjectSubset bufferSubset, VertexAttribDescription attribDesc)
        {
            if (bufferSubset.BufferTarget != BufferTarget.ArrayBuffer)
                throw new ArgumentException("The specified BufferObjectSubset must be usable as vertex attrib data. Try using a VertexDataBufferSubset", "bufferSubset");

            BufferSubset = bufferSubset;
            AttribDescription = attribDesc;
        }

        /// <summary>
        /// Creates a VertexAttribSource with a given BufferObjectSubset and specifies a vertex attribute where the data format in the shader and the buffer match and don't need conversion
        /// </summary>
        /// <param name="bufferSubset">The BufferObjectSubset where the vertex attrib data is located. Must be a subset usable for vertex data</param>
        /// <param name="attribType">The type of attribute declared in the shader</param>
        public VertexAttribSource(BufferObjectSubset bufferSubset, ActiveAttribType attribType, int attribDivisor = 0)
            : this(bufferSubset, new VertexAttribDescription(attribType, attribDivisor))
        {

        }

        /// <summary>
        /// Creates a VertexAttribSource with a given BufferObjectSubset and specifies a vertex attribute where the data format in the shader and the buffer don't match and need conversion
        /// </summary>
        /// <param name="bufferSubset">The BufferObjectSubset where the vertex attrib data is located. Must be a subset usable for vertex data</param>
        /// <param name="attribType">The type of attribute declared in the shader</param>
        /// <param name="normalized">Whether the data needs to be normalized (uint/ushort/byte -> float between 0 and 1, or int/short/sbyte -> float between -1 and 1)</param>
        /// <param name="dataBaseType">The base type of the data found on the buffer. If normalized is true, this must be an integer type</param>
        public VertexAttribSource(BufferObjectSubset bufferSubset, ActiveAttribType attribType, bool normalized, VertexAttribPointerType dataBaseType, int attribDivisor = 0)
            : this(bufferSubset, new VertexAttribDescription(attribType, normalized, dataBaseType, attribDivisor))
        {

        }

        /// <summary>
        /// Creates a VertexAttribSource that represents padding. This means that the created VertexAttribSource will not indicate
        /// a buffer to read data from or use a vertex attribute index, it will just leave an ignored space between other attributes.
        /// </summary>
        /// <param name="bufferSubset">The BufferObjectSubset where the vertex attrib data is located. Must be a subset usable for vertex data</param>
        /// <param name="paddingBytes">The amount of space to leave empty, measured in bytes</param>
        public VertexAttribSource(BufferObjectSubset bufferSubset, int paddingBytes)
        {
            if (bufferSubset.BufferTarget != BufferTarget.ArrayBuffer)
                throw new ArgumentException("The specified BufferObjectSubset must be usable as vertex attrib data. Try using a VertexDataBufferSubset", "bufferSubset");

            BufferSubset = bufferSubset;
            AttribDescription = new VertexAttribDescription(paddingBytes);
        }

        public override string ToString()
        {
            return String.Concat(AttribDescription.ToString(), " bufferHandle=", BufferSubset.BufferHandle.ToString());
        }

        public static VertexAttribSource CreatePadding(BufferObjectSubset bufferSubset, VertexAttribPointerType baseType, int size)
        {
            return new VertexAttribSource(bufferSubset, VertexAttribDescription.CreatePadding(baseType, size));
        }

        public static VertexAttribSource CreatePadding(BufferObjectSubset bufferSubset, ActiveAttribType attribType)
        {
            return new VertexAttribSource(bufferSubset, VertexAttribDescription.CreatePadding(attribType));
        }
    }
}
