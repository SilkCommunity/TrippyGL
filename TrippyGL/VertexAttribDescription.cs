using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    /// <summary>
    /// Describes a vertex attribute. This is, both how it is declared in the shader and how it will be read from a buffer.
    /// </summary>
    public readonly struct VertexAttribDescription : IEquatable<VertexAttribDescription>
    {
        /// <summary>The size of the attribute. A float or int would be 1, a vec2 would be 2, a vec3i would be 3, etc.</summary>
        public readonly int Size;

        /// <summary>The base type of the attribute.</summary>
        public readonly VertexAttribPointerType AttribBaseType;

        /// <summary>The size in bytes of the attribute. A float is 4, a ivec2 is 8, a vec4 is 16, a double is 8, a mat3 is 36, etc.</summary>
        public readonly int SizeInBytes;

        /// <summary>Whether the attrib data should be normalized when loaded into shaders.</summary>
        public readonly bool Normalized;

        /// <summary>The amount of attribute indices this specific attribute occupies. Usually 1, but float matrices for example use one for each row.</summary>
        public readonly int AttribIndicesUseCount;

        /// <summary>The type of the attribute declared in the shader.</summary>
        public readonly ActiveAttribType AttribType;

        /// <summary>Defines the rate at which this attribute advances when rendering. If 0, it advances once per vertex. Otherwise, it advances once every AttribDivisor instance/s.</summary>
        public readonly int AttribDivisor;

        /// <summary>Gets whether this <see cref="VertexAttribDescription"/> is only used to indicate padding.</summary>
        public bool IsPadding => AttribIndicesUseCount == 0;

        /// <summary>
        /// Creates a <see cref="VertexAttribDescription"/> where the format of the data declared
        /// in the shader is the same as present in the buffer and no conversion needs to be done.
        /// </summary>
        /// <param name="attribType">The type of attribute declared in the shader.</param>
        /// <param name="attribDivisor">The divisor that defines how reading this attribute advances on instanced rendering.</param>
        public VertexAttribDescription(ActiveAttribType attribType, int attribDivisor = 0)
        {
            CheckAttribDivisor(attribDivisor);

            TrippyUtils.GetVertexAttribTypeData(attribType, out AttribIndicesUseCount, out Size, out AttribBaseType);
            SizeInBytes = TrippyUtils.GetVertexAttribSizeInBytes(AttribBaseType) * Size * AttribIndicesUseCount;
            AttribType = attribType;
            Normalized = false;
            AttribDivisor = attribDivisor;
        }

        /// <summary>
        /// Creates a <see cref="VertexAttribDescription"/> where the format of the data declared
        /// in the shader isn't the same as the format the data will be read as.
        /// </summary>
        /// <param name="attribType">The type of the attribute declared in the shader.</param>
        /// <param name="normalized">Whether the vertex data should be normalized before being loaded into the shader.</param>
        /// <param name="dataBaseType">The base type in which the data will be read from the buffer.</param>
        /// <param name="attribDivisor">The divisor that defines how reading this attribute advances on instanced rendering.</param>
        public VertexAttribDescription(ActiveAttribType attribType, bool normalized, VertexAttribPointerType dataBaseType, int attribDivisor = 0)
        {
            CheckAttribDivisor(attribDivisor);

            if (normalized)
            {
                if (!TrippyUtils.IsVertexAttribIntegerType(dataBaseType))
                    throw new ArgumentException("For normalized vertex attributes, the dataBaseType must be an integer", nameof(dataBaseType));

                if (!(TrippyUtils.IsVertexAttribFloatType(attribType) || TrippyUtils.IsVertexAttribDoubleType(attribType)))
                    throw new ArgumentException("For normalized vertex attributes, the attribType must be a float or a double", nameof(attribType));
            }

            Normalized = normalized;
            AttribDivisor = attribDivisor;
            AttribBaseType = dataBaseType;
            AttribType = attribType;
            Size = TrippyUtils.GetVertexAttribTypeSize(attribType);
            AttribIndicesUseCount = TrippyUtils.GetVertexAttribTypeIndexCount(attribType);
            SizeInBytes = TrippyUtils.GetVertexAttribSizeInBytes(dataBaseType) * Size * AttribIndicesUseCount;
        }

        /// <summary>
        /// Creates a <see cref="VertexAttribDescription"/> that represents no real attributes and is
        /// used to indicate padding (unused, ignored buffer memory in between other vertex attribs).
        /// </summary>
        /// <param name="paddingBytes">The amount of padding in bytes.</param>
        public VertexAttribDescription(int paddingBytes)
        {
            if (paddingBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(paddingBytes), paddingBytes, nameof(paddingBytes) + " must be greater than 0");

            Size = 0;
            AttribBaseType = 0;
            SizeInBytes = paddingBytes; // The only non-zero field when a VertexAttribDescription is used for padding, stores the padding in bytes
            Normalized = false;
            AttribIndicesUseCount = 0; // We'll use this value to be the one that decides whether this is padding. If it uses 0 indices, it's padding.
            AttribType = ActiveAttribType.None;
            AttribDivisor = 0;
        }

        public static bool operator ==(VertexAttribDescription left, VertexAttribDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VertexAttribDescription left, VertexAttribDescription right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            if (IsPadding)
                return string.Concat("Padding=", SizeInBytes.ToString(), " bytes");

            return string.Concat(
                Normalized ? "Normalized " : "Unnormalized ", AttribType.ToString(),
                ", " + nameof(AttribBaseType) + "=", AttribBaseType.ToString(),
                ", " + nameof(AttribIndicesUseCount) + "=", AttribIndicesUseCount.ToString(),
                ", " + nameof(AttribDivisor) + "=", AttribDivisor.ToString()
            );
        }

        public static VertexAttribDescription CreatePadding(VertexAttribPointerType baseType, int size)
        {
            return new VertexAttribDescription(TrippyUtils.GetVertexAttribSizeInBytes(baseType) * size);
        }

        public static VertexAttribDescription CreatePadding(ActiveAttribType attribType)
        {
            return CreatePadding(TrippyUtils.GetVertexAttribBaseType(attribType), TrippyUtils.GetVertexAttribTypeSize(attribType));
        }

        private static void CheckAttribDivisor(int attribDivisor)
        {
            if (attribDivisor < 0)
                throw new ArgumentOutOfRangeException(nameof(attribDivisor), attribDivisor, nameof(attribDivisor) + " must be greater than 0");
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = AttribType.GetHashCode();
                hashCode = (hashCode * 397) ^ AttribBaseType.GetHashCode();
                hashCode = (hashCode * 397) ^ Normalized.GetHashCode();
                hashCode = (hashCode * 397) ^ AttribDivisor.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(VertexAttribDescription other)
        {
            return AttribType == other.AttribType
                && AttribBaseType == other.AttribBaseType
                && Normalized == other.Normalized
                && AttribDivisor == other.AttribDivisor;
        }

        public override bool Equals(object obj)
        {
            if (obj is VertexAttribDescription vertexAttribDescription)
                return Equals(vertexAttribDescription);
            return false;
        }
    }
}
