using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Describes the source for a vertex attribute's data. This stores a <see cref="VertexAttribDescription"/>
    /// about the attribute and a buffer subset from which the attrib data will come from.
    /// </summary>
    public readonly struct VertexAttribSource : IEquatable<VertexAttribSource>
    {
        /// <summary>The buffer subset from which the vertex attributes will be read.</summary>
        public readonly DataBufferSubset BufferSubset;

        /// <summary>The description of the vertex attribute.</summary>
        public readonly VertexAttribDescription AttribDescription;

        /// <summary>Gets whether this <see cref="VertexAttribSource"/> is only used to indicate padding.</summary>
        public bool IsPadding => AttribDescription.IsPadding;

        /// <summary>
        /// Creates a <see cref="VertexAttribSource"/> with a given <see cref="BufferObjectSubset"/>
        /// and the given <see cref="VertexAttribDescription"/>.
        /// </summary>
        /// <param name="bufferSubset">The <see cref="BufferObjectSubset"/> where the vertex attrib data is located. Must be a subset usable for vertex data.</param>
        /// <param name="attribDesc">The <see cref="VertexAttribDescription"/> describing the vertex attribute.</param>
        public VertexAttribSource(DataBufferSubset bufferSubset, VertexAttribDescription attribDesc)
        {
            if (bufferSubset == null)
                throw new ArgumentNullException(nameof(bufferSubset));

            if (bufferSubset.BufferTarget != BufferTargetARB.ArrayBuffer)
                throw new ArgumentException("The specified " + nameof(BufferObjectSubset) + " must be usable as vertex attrib data. Try using a VertexDataBufferSubset", nameof(bufferSubset));

            BufferSubset = bufferSubset;
            AttribDescription = attribDesc;
        }

        /// <summary>
        /// Creates a <see cref="VertexAttribSource"/> with a given <see cref="BufferObjectSubset"/> and specifies a
        /// vertex attribute where the data format in the shader and the buffer match and don't need conversion.
        /// </summary>
        /// <param name="bufferSubset">The <see cref="BufferObjectSubset"/> where the vertex attrib data is located. Must be a subset usable for vertex data.</param>
        /// <param name="attribType">The type of attribute declared in the shader.</param>
        /// <param name="attribDivisor">The divisor that defines how reading this attribute advances on instanced rendering.</param>
        public VertexAttribSource(DataBufferSubset bufferSubset, AttributeType attribType, uint attribDivisor = 0)
            : this(bufferSubset, new VertexAttribDescription(attribType, attribDivisor))
        {

        }

        /// <summary>
        /// Creates a <see cref="VertexAttribSource"/> with a given <see cref="BufferObjectSubset"/> and specifies
        /// a vertex attribute where the data format in the shader and the buffer don't match and need conversion.
        /// </summary>
        /// <param name="bufferSubset">The <see cref="BufferObjectSubset"/> where the vertex attrib data is located. Must be a subset usable for vertex data.</param>
        /// <param name="attribType">The type of attribute declared in the shader.</param>
        /// <param name="normalized">Whether the data needs to be normalized (uint/ushort/byte -> float between 0 and 1, or int/short/sbyte -> float between -1 and 1).</param>
        /// <param name="dataBaseType">The base type of the data found on the buffer. If normalized is true, this must be an integer type.</param>
        /// <param name="attribDivisor">The divisor that defines how reading this attribute advances on instanced rendering.</param>
        public VertexAttribSource(DataBufferSubset bufferSubset, AttributeType attribType, bool normalized, VertexAttribPointerType dataBaseType, uint attribDivisor = 0)
            : this(bufferSubset, new VertexAttribDescription(attribType, normalized, dataBaseType, attribDivisor))
        {

        }

        /// <summary>
        /// Creates a <see cref="VertexAttribSource"/> that represents padding. This means that the created
        /// <see cref="VertexAttribSource"/> will not indicate a buffer to read data from or use a vertex
        /// attribute index, it will just leave an ignored space between other attributes.
        /// </summary>
        /// <param name="bufferSubset">The <see cref="BufferObjectSubset"/> where the padding will be added.</param>
        /// <param name="paddingBytes">The amount of space to leave empty, measured in bytes.</param>
        public VertexAttribSource(DataBufferSubset bufferSubset, uint paddingBytes)
        {
            if (bufferSubset == null)
                throw new ArgumentNullException(nameof(bufferSubset));

            if (bufferSubset.BufferTarget != BufferTargetARB.ArrayBuffer)
                throw new ArgumentException("The specified BufferObjectSubset must be usable as vertex attrib data. Try using a VertexDataBufferSubset", nameof(bufferSubset));

            BufferSubset = bufferSubset;
            AttribDescription = new VertexAttribDescription(paddingBytes);
        }

        public static bool operator ==(VertexAttribSource left, VertexAttribSource right) => left.Equals(right);
        public static bool operator !=(VertexAttribSource left, VertexAttribSource right) => !left.Equals(right);

        public override string ToString()
        {
            return string.Concat(AttribDescription.ToString(), ", bufferHandle=", BufferSubset?.BufferHandle.ToString());
        }

        /// <summary>
        /// Creates a <see cref="VertexAttribSource"/> that specifies padding for an
        /// amount of bytes calculated based on the baseType and size parameters.
        /// </summary>
        /// <param name="bufferSubset">The buffer subset in which the padding will be applied.</param>
        public static VertexAttribSource CreatePadding(DataBufferSubset bufferSubset, VertexAttribPointerType baseType, uint size)
        {
            return new VertexAttribSource(bufferSubset, VertexAttribDescription.CreatePadding(baseType, size));
        }

        /// <summary>
        /// Creates a <see cref="VertexAttribSource"/> that specifies padding for the amount
        /// of bytes used by a specified <see cref="AttributeType"/>.
        /// </summary>
        /// <param name="bufferSubset">The buffer subset in which the padding will be applied.</param>
        /// <remarks>
        /// Padding indicators ignore padding based on type that occurs when using compensation
        /// for struct padding (which is the default behavior in <see cref="VertexArray"/>).
        /// </remarks>
        public static VertexAttribSource CreatePadding(DataBufferSubset bufferSubset, AttributeType attribType)
        {
            return new VertexAttribSource(bufferSubset, VertexAttribDescription.CreatePadding(attribType));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BufferSubset, AttribDescription);
        }

        public override bool Equals(object obj)
        {
            if (obj is VertexAttribSource vertexAttribSource)
                return Equals(vertexAttribSource);
            return false;
        }

        public bool Equals(VertexAttribSource other)
        {
            return BufferSubset == other.BufferSubset && AttribDescription.Equals(other.AttribDescription);
        }
    }
}
