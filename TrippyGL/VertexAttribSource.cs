using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    public struct VertexAttribSource
    {
        /// <summary>The VertexDataBufferObject from which the vertex attributes will be read</summary>
        public readonly IVertexArrayAttribSource DataBuffer;

        /// <summary>The size of the attribute. A float or int would be 1, a vec2 would be 2, a vec3i would be 3, etc</summary>
        public readonly int Size;

        /// <summary>The size in bytes of the attribute. A float is 4, a vec2i is 8, a vec4 is 16</summary>
        public readonly int SizeInBytes;

        /// <summary>Whether the attrib data should be normalized when loaded into shaders</summary>
        public readonly bool Normalized;

        /// <summary>The type of variable this attribute corresponds to. Vectors and non-vectors would have the same AttribType as other sized same-type vectors, so a vec4 has AttribType float and a vec2i has AttribType int</summary>
        public readonly VertexAttribPointerType AttribType;

        public VertexAttribSource(IVertexArrayAttribSource dataBuffer, int size, bool normalized, VertexAttribPointerType attribType, int sizeInBytes)
        {
            if (dataBuffer == null)
                throw new ArgumentNullException("dataBuffer");

            if (size <= 0)
                throw new ArgumentOutOfRangeException("size", size, "Size must be greater than 0");

            if (sizeInBytes <= 0)
                throw new ArgumentOutOfRangeException("sizeInBytes", sizeInBytes, "sizeInBytes must be greater than 0");

            this.DataBuffer = dataBuffer;
            this.Size = size;
            this.Normalized = normalized;
            this.AttribType = attribType;
            this.SizeInBytes = sizeInBytes;
        }

        public VertexAttribSource(IVertexArrayAttribSource dataBuffer, int size, bool normalized, VertexAttribPointerType attribType)
            : this(dataBuffer, size, normalized, attribType, GetSizeInBytesOfAttribType(attribType) * size)
        {

        }

        public override string ToString()
        {
            return String.Concat(AttribType, Size, " SizeInBytes=", SizeInBytes, Normalized ? " Normalized" : " Not normalized");
        }


        internal static int GetSizeInBytesOfAttribType(VertexAttribPointerType type)
        {
            switch (type)
            {
                case VertexAttribPointerType.Byte:
                case VertexAttribPointerType.UnsignedByte:
                    return 1;

                case VertexAttribPointerType.Short:
                case VertexAttribPointerType.UnsignedShort:
                case VertexAttribPointerType.HalfFloat:
                    return 2;

                case VertexAttribPointerType.Float:
                case VertexAttribPointerType.Int:
                case VertexAttribPointerType.UnsignedInt:
                case VertexAttribPointerType.Fixed:
                    return 4;

                case VertexAttribPointerType.Double:
                    return 8;

                //case VertexAttribPointerType.Int2101010Rev:
                //case VertexAttribPointerType.UnsignedInt10F11F11FRev:
                //case VertexAttribPointerType.UnsignedInt2101010Rev:
                default:
                    throw new NotSupportedException("The specified vertex attribute format's size in bytes cannot be deciphered by the pointer type");

            }
        }
    }
}
