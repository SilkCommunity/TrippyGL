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
            return String.Concat(AttribDescription.ToString(), " bufferHandle=", DataBuffer.Handle);
        }
    }

    /// <summary>
    /// Describes a vertex attribute. This is, both how it is declared in the shader and how it is stored in a buffer object.
    /// </summary>
    public struct VertexAttribDescription
    {
        /// <summary>The size of the attribute. A float or int would be 1, a vec2 would be 2, a vec3i would be 3, etc</summary>
        public readonly int Size;

        /// <summary></summary>
        public readonly VertexAttribPointerType AttribBaseType;

        /// <summary>The size in bytes of the attribute. A float is 4, a ivec2 is 8, a vec4 is 16, a double is 8, a mat3 is 36, etc</summary>
        public readonly int SizeInBytes;

        /// <summary>Whether the attrib data should be normalized when loaded into shaders</summary>
        public readonly bool Normalized;

        /// <summary>The amount of attribute indices this specific attribute occupies. Usually 1, but float matrices for example use one for each row</summary>
        public readonly int AttribIndicesUseCount;

        /// <summary>The type of the attribute declared in the shader</summary>
        public readonly ActiveAttribType AttribType;

        /// <summary>
        /// Creates a VertexAttribDescription where the format of the data declared in the shader is the same as present in the buffer and no conversion needs to be done
        /// </summary>
        /// <param name="attribType">The type of attribute declared in the shader</param>
        public VertexAttribDescription(ActiveAttribType attribType)
        {
            this.AttribType = attribType;
            this.Normalized = false;
            TrippyUtils.GetVertexAttribTypeData(attribType, out this.AttribIndicesUseCount, out this.Size, out this.AttribBaseType);
            this.SizeInBytes = TrippyUtils.GetVertexAttribSizeInBytes(this.AttribBaseType) * this.Size * this.AttribIndicesUseCount;
        }

        /// <summary>
        /// Creates a VertexAttribDescription where the data format declared in the shader isn't the same as the format the data will be read as
        /// </summary>
        /// <param name="attribType">The type of the attribute declared in the shader</param>
        /// <param name="normalized">Whether the vertex data should be normalized before being loaded into the shader</param>
        /// <param name="dataBaseType">The base type in which the data will be read from the buffer</param>
        public VertexAttribDescription(ActiveAttribType attribType, bool normalized, VertexAttribPointerType dataBaseType)
        {
            this.AttribType = attribType;
            this.Normalized = normalized;
            this.AttribBaseType = dataBaseType;
            this.AttribType = attribType;
            this.Size = TrippyUtils.GetVertexAttribTypeSize(attribType);
            this.AttribIndicesUseCount = TrippyUtils.GetVertexAttribTypeIndexCount(attribType);
            this.SizeInBytes = TrippyUtils.GetVertexAttribSizeInBytes(dataBaseType) * this.Size * this.AttribIndicesUseCount;

            if (normalized && !TrippyUtils.IsVertexAttribIntegerType(dataBaseType))
                throw new ArgumentException("For normalized vertex attributes, the dataBaseType must be an integer", "dataBaseType");
        }

        public override string ToString()
        {
            return Normalized ? String.Concat("Normalized ", AttribType, " baseType ", AttribBaseType) : String.Concat("Unnormalized ", AttribType);
        }
    }
}
