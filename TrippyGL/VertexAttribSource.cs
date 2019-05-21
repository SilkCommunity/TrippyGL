using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    public struct VertexAttribSource
    {
        /// <summary>The VertexDataBufferObject from which the vertex attributes will be read</summary>
        public readonly BufferObject DataBuffer;

        public VertexAttribDescription AttribDescription;

        public VertexAttribSource(BufferObject dataBuffer, VertexAttribDescription attribDesc)
        {
            if (dataBuffer == null)
                throw new ArgumentNullException("dataBuffer");

            if (dataBuffer.BufferTarget != BufferTarget.ArrayBuffer)
                throw new ArgumentException("The specified BufferObject must be usable as vertex attrib data. Try using a VertexDataBufferObject", "dataBuffer");


            this.DataBuffer = dataBuffer;
            this.AttribDescription = attribDesc;
        }

        public VertexAttribSource(BufferObject dataBuffer, ActiveAttribType attribType)
            : this(dataBuffer, new VertexAttribDescription(attribType))
        {

        }

        public VertexAttribSource(BufferObject dataBuffer, ActiveAttribType attribType, bool normalized, VertexAttribPointerType dataBaseType)
            : this(dataBuffer, new VertexAttribDescription(attribType, normalized, dataBaseType))
        {

        }

        public override string ToString()
        {
            return String.Concat(AttribDescription.ToString(), " bufferHandle=", DataBuffer.Handle);
        }
    }

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

        public VertexAttribDescription(ActiveAttribType attribType)
        {
            this.AttribType = attribType;
            this.Normalized = false;
            GetAttribTypeData(attribType, out this.AttribIndicesUseCount, out this.Size, out this.AttribBaseType);
            this.SizeInBytes = GetSizeInBytesOfAttribType(this.AttribBaseType) * this.Size * this.AttribIndicesUseCount;
        }

        /// <summary>
        /// 
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
            this.Size = GetAttribTypeSize(attribType);
            this.AttribIndicesUseCount = GetAttribTypeIndexCount(attribType);
            this.SizeInBytes = GetSizeInBytesOfAttribType(dataBaseType) * this.Size * this.AttribIndicesUseCount;

            if (normalized && !IsIntegerType(dataBaseType))
                throw new ArgumentException("For normalized vertex attributes, the dataBaseType must be an integer", "dataBaseType");
        }

        public override string ToString()
        {
            return Normalized ? String.Concat("Normalized ", AttribType, " baseType ", AttribBaseType) : String.Concat("Unnormalized ", AttribType);
        }

        /// <summary>
        /// Returns whether the specified data base type is of integer format (such as byte, ushort, int, uint)
        /// </summary>
        public static bool IsIntegerType(VertexAttribPointerType dataBaseType)
        {
            return dataBaseType == VertexAttribPointerType.UnsignedByte || dataBaseType == VertexAttribPointerType.Byte
                || dataBaseType == VertexAttribPointerType.UnsignedShort || dataBaseType == VertexAttribPointerType.Short
                || dataBaseType == VertexAttribPointerType.UnsignedInt || dataBaseType == VertexAttribPointerType.Int
                || dataBaseType == VertexAttribPointerType.Int2101010Rev || dataBaseType == VertexAttribPointerType.UnsignedInt2101010Rev
                || dataBaseType == VertexAttribPointerType.UnsignedInt10F11F11FRev;
        }

        /// <summary>
        /// Gets whether the specified attrib type is an integer type (such as int, ivecX, uint or uvecX)
        /// </summary>
        public static bool IsIntegerType(ActiveAttribType attribType)
        {
            return attribType == ActiveAttribType.Int || attribType == ActiveAttribType.UnsignedInt
                || (attribType >= ActiveAttribType.IntVec2 && attribType <= ActiveAttribType.IntVec4)
                || (attribType >= ActiveAttribType.UnsignedIntVec2 && attribType <= ActiveAttribType.UnsignedIntVec4);
        }

        /// <summary>
        /// Gets whether the specified attrib type is a floating point double type (such as double, dvecX or dmatMxN)
        /// </summary>
        public static bool IsDoubleType(ActiveAttribType attribType)
        {
            return attribType == ActiveAttribType.Double
                || (attribType >= ActiveAttribType.DoubleVec2 && attribType <= ActiveAttribType.DoubleVec4)
                || (attribType >= ActiveAttribType.DoubleMat2 && attribType <= ActiveAttribType.DoubleMat4x3);
        }

        public static void GetAttribTypeData(ActiveAttribType attribType, out int indexUseCount, out int size, out VertexAttribPointerType type)
        {
            indexUseCount = GetAttribTypeIndexCount(attribType);
            size = GetAttribTypeSize(attribType);
            type = GetAttribBaseType(attribType);
        }

        /// <summary>
        /// Gets the base variable type for the specified attribute type
        /// (for example, vec4 would return float. dmat2 would return double, ivec2 returns int)
        /// </summary>
        public static VertexAttribPointerType GetAttribBaseType(ActiveAttribType attribType)
        {
            if (attribType == ActiveAttribType.Float // is it a float?
                || (attribType >= ActiveAttribType.FloatVec2 && attribType <= ActiveAttribType.FloatVec4) // or is it a float vector?
                || (attribType >= ActiveAttribType.FloatMat2 && attribType <= ActiveAttribType.FloatMat4x3)) // or is it a float matrix?
                return VertexAttribPointerType.Float;

            if (attribType == ActiveAttribType.Int
                || (attribType >= ActiveAttribType.IntVec2 && attribType <= ActiveAttribType.IntVec4))
                return VertexAttribPointerType.Int;

            if (attribType == ActiveAttribType.Double
                || (attribType >= ActiveAttribType.DoubleVec2 && attribType <= ActiveAttribType.DoubleVec4)
                || (attribType >= ActiveAttribType.DoubleMat2 && attribType <= ActiveAttribType.DoubleMat4x3))
                return VertexAttribPointerType.Double;

            if (attribType == ActiveAttribType.UnsignedInt
                || (attribType >= ActiveAttribType.UnsignedIntVec2 || attribType <= ActiveAttribType.UnsignedIntVec4))
                return VertexAttribPointerType.UnsignedInt;

            throw new ArgumentException("The provided value is not a valid enum value", "attribType");
        }

        /// <summary>
        /// Gets the attribute's size. By size, this means "vector size" (float is 1, vec2i is 2, bvec4 is 4, etc)
        /// </summary>
        public static int GetAttribTypeSize(ActiveAttribType attribType)
        {
            if ((attribType >= ActiveAttribType.Int && attribType <= ActiveAttribType.Float) || attribType == ActiveAttribType.Double)
                return 1;

            if (attribType >= ActiveAttribType.FloatVec2 && attribType <= ActiveAttribType.FloatVec4)
                return attribType - ActiveAttribType.FloatVec2 + 2;

            if (attribType >= ActiveAttribType.IntVec2 && attribType <= ActiveAttribType.IntVec4)
                return attribType - ActiveAttribType.IntVec2 + 2;

            if (attribType >= ActiveAttribType.UnsignedIntVec2 && attribType <= ActiveAttribType.UnsignedIntVec4)
                return attribType - ActiveAttribType.UnsignedIntVec2 + 2;

            if (attribType >= ActiveAttribType.DoubleVec2 && attribType <= ActiveAttribType.DoubleVec4)
                return attribType - ActiveAttribType.DoubleVec2 + 2;

            switch (attribType)
            {
                case ActiveAttribType.FloatMat2:
                case ActiveAttribType.FloatMat3x2:
                case ActiveAttribType.FloatMat4x2:
                case ActiveAttribType.DoubleMat2:
                case ActiveAttribType.DoubleMat3x2:
                case ActiveAttribType.DoubleMat4x2:
                    return 2;

                case ActiveAttribType.FloatMat3:
                case ActiveAttribType.FloatMat2x3:
                case ActiveAttribType.FloatMat4x3:
                case ActiveAttribType.DoubleMat3:
                case ActiveAttribType.DoubleMat2x3:
                case ActiveAttribType.DoubleMat4x3:
                    return 3;

                case ActiveAttribType.FloatMat4:
                case ActiveAttribType.FloatMat2x4:
                case ActiveAttribType.FloatMat3x4:
                case ActiveAttribType.DoubleMat4:
                case ActiveAttribType.DoubleMat2x4:
                case ActiveAttribType.DoubleMat3x4:
                    return 4;
            }

            throw new ArgumentException("The provided value is not a valid enum value", "attribType");
        }

        /// <summary>
        /// Gets the amount of indices the vertex attribute occupies
        /// </summary>
        public static int GetAttribTypeIndexCount(ActiveAttribType attribType)
        {
            if ((attribType >= ActiveAttribType.Int && attribType <= ActiveAttribType.Float)
                || attribType == ActiveAttribType.Double
                || (attribType >= ActiveAttribType.FloatVec2 && attribType <= ActiveAttribType.IntVec4)
                || (attribType >= ActiveAttribType.UnsignedIntVec2 && attribType <= ActiveAttribType.UnsignedIntVec4)
                || (attribType >= ActiveAttribType.DoubleVec2 && attribType <= ActiveAttribType.DoubleVec4))
                return 1;

            switch (attribType)
            {
                case ActiveAttribType.FloatMat2:
                case ActiveAttribType.FloatMat2x3:
                case ActiveAttribType.FloatMat2x4:
                case ActiveAttribType.DoubleMat2:
                case ActiveAttribType.DoubleMat2x3:
                case ActiveAttribType.DoubleMat2x4:
                    return 2;

                case ActiveAttribType.FloatMat3:
                case ActiveAttribType.FloatMat3x2:
                case ActiveAttribType.FloatMat3x4:
                case ActiveAttribType.DoubleMat3:
                case ActiveAttribType.DoubleMat3x2:
                case ActiveAttribType.DoubleMat3x4:
                    return 3;

                case ActiveAttribType.FloatMat4:
                case ActiveAttribType.FloatMat4x2:
                case ActiveAttribType.FloatMat4x3:
                case ActiveAttribType.DoubleMat4:
                case ActiveAttribType.DoubleMat4x2:
                case ActiveAttribType.DoubleMat4x3:
                    return 4;
            }

            throw new ArgumentException("The provided value is not a valid enum value", "attribType");
        }

        /// <summary>
        /// Gets the size in bytes of an attribute type
        /// </summary>
        public static int GetSizeInBytesOfAttribType(VertexAttribPointerType type)
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
                    throw new NotSupportedException("The specified vertex attribute format's size in bytes cannot be deciphered by the pointer type.");

            }
        }
    }
}
