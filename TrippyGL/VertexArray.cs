using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Used for specifying the way vertex attributes are laid out in memory and from which
    /// <see cref="BufferObjectSubset"/> each vertex attribute comes from. Also stores an optional index buffer.
    /// </summary>
    public sealed class VertexArray : GraphicsResource
    {
        /// <summary>The handle for the GL Vertex Array Object.</summary>
        public readonly uint Handle;

        /// <summary>A copy of the <see cref="VertexAttribSource"/>-s provided to the constructor. Should only be read.</summary>
        private readonly VertexAttribSource[] attribSources;

        /// <summary>A list with the sources that will feed the vertex attribute's data on draw calls.</summary>
        public ReadOnlySpan<VertexAttribSource> AttribSources => attribSources;

        public readonly IndexBufferSubset IndexBuffer;

        /// <summary>
        /// Creates a <see cref="VertexArray"/>
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="attribSources">
        /// The <see cref="VertexAttribSource"/> array that will be stored by this <see cref="VertexArray"/>.
        /// The library user must NOT have a reference to this array!
        /// </param>
        /// <param name="indexBuffer">An index buffer to attach to the vertex array, null if none is desired.</param>
        /// <param name="compensateStructPadding">Whether to compensate for struct padding. Default is true.</param>
        /// <param name="paddingPackValue">The struct packing value for compensating for padding. Default is 4.</param>
        internal VertexArray(GraphicsDevice graphicsDevice, VertexAttribSource[] attribSources, IndexBufferSubset indexBuffer = null, bool compensateStructPadding = true, uint paddingPackValue = 4)
            : base(graphicsDevice)
        {
            EnsureAttribsValid(attribSources);
            this.attribSources = attribSources;

            Handle = GL.GenVertexArray();

            IndexBuffer = indexBuffer;

            UpdateVertexAttributes(compensateStructPadding, paddingPackValue); //this also binds the vertex array
        }

        /// <summary>
        /// Creates a <see cref="VertexArray"/> with the specified attribute sources.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="attribSources">The sources from which the data of the vertex attributes will come from.</param>
        /// <param name="indexBuffer">An index buffer to attach to the vertex array, null if none is desired.</param>
        /// <param name="compensateStructPadding">Whether to compensate for struct padding. Default is true.</param>
        /// <param name="paddingPackValue">The struct packing value for compensating for padding. Default is 4.</param>
        public VertexArray(GraphicsDevice graphicsDevice, ReadOnlySpan<VertexAttribSource> attribSources, IndexBufferSubset indexBuffer = null, bool compensateStructPadding = true, uint paddingPackValue = 4)
            : this(graphicsDevice, attribSources.ToArray(), indexBuffer, compensateStructPadding, paddingPackValue)
        {

        }

        /// <summary>
        /// Creates a <see cref="VertexArray"/> in which all the vertex attributes come interleaved from the same data buffer.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="bufferSubset">The data buffer that stores all the vertex attributes.</param>
        /// <param name="attribDescriptions">The descriptions of the vertex attributes.</param>
        /// <param name="indexBuffer">An index buffer to attach to the vertex array, null if none is desired.</param>
        /// <param name="compensateStructPadding">Whether to compensate for struct padding. Default is true.</param>
        /// <param name="paddingPackValue">The struct packing value for compensating for padding. Default is 4.</param>
        public VertexArray(GraphicsDevice graphicsDevice, DataBufferSubset bufferSubset, ReadOnlySpan<VertexAttribDescription> attribDescriptions, IndexBufferSubset indexBuffer = null, bool compensateStructPadding = true, uint paddingPackValue = 4)
            : this(graphicsDevice, MakeAttribList(bufferSubset, attribDescriptions), indexBuffer, compensateStructPadding, paddingPackValue)
        {

        }

        /// <summary>
        /// Updates the places where vertex data is read from for this <see cref="VertexArray"/>.
        /// Call this whenever you modify a buffer subset used by this <see cref="VertexArray"/>.
        /// </summary>
        /// <param name="compensateStructPadding">Whether to compensate for struct padding. Default is true.</param>
        /// <param name="paddingPackValue">The struct packing value for compensating for padding. Default is 4.</param>
        public void UpdateVertexAttributes(bool compensateStructPadding = true, uint paddingPackValue = 4)
        {
            // Makes all glVertexAttribPointer calls to specify the vertex attrib data on the VAO and enables the vertex attributes.
            // The parameters of glVertexAttribPointer are calculated based on the VertexAttribSource-s from AttribSources

            GraphicsDevice.VertexArray = this;

            AttribCallDesc[] calls = new AttribCallDesc[attribSources.Length];

            uint attribIndex = 0;
            for (int i = 0; i < calls.Length; i++)
            {
                calls[i] = new AttribCallDesc
                {
                    source = attribSources[i],
                    index = attribIndex
                };
                attribIndex += calls[i].source.AttribDescription.AttribIndicesUseCount;
            }

            // Sort by buffer object, so all sources that share BufferObject are grouped together.
            // This facilitates calculating the offset values, since we only need to work with one offset at a time
            // rather than save the offset of each buffer simultaneously
            Array.Sort(calls, (x, y) => x.source.BufferSubset.BufferHandle.CompareTo(y.source.BufferSubset.BufferHandle));
            // Note that the calls array is now sorted by both attrib index and buffer handle

            if (compensateStructPadding)
            {
                #region CalculateOffsetsWithPadding
                uint offset = 0;
                BufferObjectSubset prevSubset = null; // Setting this to null ensures the first for loop will enter the "different subset" if and initialize these variables
                VertexAttribPointerType currentBaseType = 0;

                for (int i = 0; i < calls.Length; i++)
                {
                    if (calls[i].source.BufferSubset != prevSubset)
                    {
                        // it's a different buffer subset, so let's calculate the padding values as for a new, different struct
                        offset = 0;
                        prevSubset = calls[i].source.BufferSubset;
                        currentBaseType = calls[i].source.AttribDescription.AttribBaseType;
                        calls[i].offset = 0;
                    }
                    else if (currentBaseType != calls[i].source.AttribDescription.AttribBaseType)
                    {
                        // the base type has changed, let's ensure padding is applied to offset
                        currentBaseType = calls[i].source.AttribDescription.AttribBaseType;
                        if (!calls[i].source.IsPadding)
                        { // We add the manual padding, unless it is padding added specifically by the user
                            uint packval = Math.Min(TrippyUtils.GetVertexAttribSizeInBytes(currentBaseType), paddingPackValue); // offset should be aligned by the default packing value or the size of the base type
                            offset = (offset + packval - 1) / packval * packval; // Make offset be greater or equal to offset and divisible by packval
                        }
                    }

                    calls[i].offset = offset;
                    offset += calls[i].source.AttribDescription.SizeInBytes;
                }
                #endregion
            }
            else
            {
                #region CalculateOffsetsWithoutPadding
                uint offset = 0;
                uint prevBufferHandle = 0;
                for (int i = 0; i < calls.Length; i++)
                {
                    if (prevBufferHandle != calls[i].source.BufferSubset.BufferHandle)
                    {
                        prevBufferHandle = calls[i].source.BufferSubset.BufferHandle;
                        offset = 0;
                    }

                    calls[i].offset = offset;
                    offset += calls[i].source.AttribDescription.SizeInBytes;
                }
                #endregion
            }

            for (int i = 0; i < calls.Length; i++)
                calls[i].CallGlVertexAttribPointer(GL);

            GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, IndexBuffer == null ? 0 : IndexBuffer.BufferHandle);
        }

        protected override void Dispose(bool isManualDispose)
        {
            if (isManualDispose && GraphicsDevice.VertexArray == this)
                GraphicsDevice.VertexArray = null;

            GL.DeleteVertexArray(Handle);
            base.Dispose(isManualDispose);
        }

        public override string ToString()
        {
            return string.Concat(
                nameof(Handle) + "=", Handle.ToString(),
                ", ", attribSources.Length.ToString(), " " + nameof(AttribSources),
                ", " + nameof(IndexBuffer) + "=", IndexBuffer == null ? "null" : IndexBuffer.ToString()
            );
        }

        private static VertexAttribSource[] MakeAttribList(DataBufferSubset bufferSubset, ReadOnlySpan<VertexAttribDescription> attribDescriptions)
        {
            VertexAttribSource[] sources = new VertexAttribSource[attribDescriptions.Length];
            for (int i = 0; i < sources.Length; i++)
                sources[i] = new VertexAttribSource(bufferSubset, attribDescriptions[i]);
            return sources;
        }

        /// <summary>
        /// Creates a <see cref="VertexArray"/> for the specified vertex type, where all of the vertex attributes come interleaved from the same buffer subset.
        /// </summary>
        /// <typeparam name="T">The type of vertex to use.</typeparam>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="dataBuffer">The buffer from which all attributes come from.</param>
        /// <param name="indexBuffer">An index buffer to attach to the vertex array, null if none is desired.</param>
        /// <param name="compensateStructPadding">Whether to compensate for struct padding. Default is true.</param>
        /// <param name="paddingPackValue">The struct packing value for compensating for padding. Default is 4.</param>
        public static VertexArray CreateSingleBuffer<T>(GraphicsDevice graphicsDevice, DataBufferSubset dataBuffer, IndexBufferSubset indexBuffer = null, bool compensateStructPadding = true, uint paddingPackValue = 4) where T : unmanaged, IVertex
        {
            T t = default;
            int attribCount = t.AttribDescriptionCount;
            Span<VertexAttribDescription> attribDescriptions = attribCount > 256 ?
                new VertexAttribDescription[attribCount] : stackalloc VertexAttribDescription[attribCount];
            t.WriteAttribDescriptions(attribDescriptions);

            return new VertexArray(graphicsDevice, dataBuffer, attribDescriptions, indexBuffer, compensateStructPadding, paddingPackValue);
        }

        private void EnsureAttribsValid(VertexAttribSource[] attribSources)
        {
            if (attribSources.Length == 0)
                throw new ArgumentException("You can't create a " + nameof(VertexArray) + " with no attributes", nameof(attribSources));

            uint attribIndexCount = 0;
            for (int i = 0; i < attribSources.Length; i++)
            {
                attribIndexCount += attribSources[i].AttribDescription.AttribIndicesUseCount;

                if (attribSources[i].AttribDescription.AttribDivisor != 0)
                {
                    if (!GraphicsDevice.IsVertexAttribDivisorAvailable)
                        throw new PlatformNotSupportedException("Vertex attribute divisors are notsupported on this system");
                }
            }

            if (!GraphicsDevice.IsDoublePrecisionVertexAttribsAvailable)
            {
                for (int i = 0; i < attribSources.Length; i++)
                    if (TrippyUtils.IsVertexAttribDoubleType(attribSources[i].AttribDescription.AttribType))
                        throw new PlatformNotSupportedException("Double precition vertex attributes are not supported on this system");
            }

            if (attribIndexCount > GraphicsDevice.MaxVertexAttribs)
                throw new PlatformNotSupportedException("The current system doesn't support this many vertex attributes");
        }

        /// <summary>
        /// Manages the calls for a single vertex attribute.
        /// This is a helper struct used in <see cref="VertexArray.UpdateVertexAttributes(bool, int)"/>
        /// </summary>
        private struct AttribCallDesc
        {
            public VertexAttribSource source;
            public uint index, offset;

            public unsafe void CallGlVertexAttribPointer(GL gl)
            {
                source.BufferSubset.Buffer.GraphicsDevice.BindBuffer(source.BufferSubset);
                uint offs = offset + source.BufferSubset.StorageOffsetInBytes;
                uint stride = source.BufferSubset.ElementSize;
                for (uint i = 0; i < source.AttribDescription.AttribIndicesUseCount; i++)
                {
                    if (!source.AttribDescription.Normalized && source.AttribDescription.AttribBaseType == VertexAttribPointerType.Double)
                        gl.VertexAttribLPointer(index + i, source.AttribDescription.Size, VertexAttribPointerType.Double, stride, (void*)offs);
                    else if (!source.AttribDescription.Normalized && TrippyUtils.IsVertexAttribIntegerType(source.AttribDescription.AttribType))
                        gl.VertexAttribIPointer(index + i, source.AttribDescription.Size, source.AttribDescription.AttribBaseType, stride, (void*)offs);
                    else
                        gl.VertexAttribPointer(index + i, source.AttribDescription.Size, source.AttribDescription.AttribBaseType, source.AttribDescription.Normalized, stride, (void*)offs);

                    gl.EnableVertexAttribArray(index + i);
                    if (source.AttribDescription.AttribDivisor != 0)
                        gl.VertexAttribDivisor(index + i, source.AttribDescription.AttribDivisor);
                    offs += source.AttribDescription.SizeInBytes / source.AttribDescription.AttribIndicesUseCount;
                }
            }

            public override string ToString()
            {
                return string.Concat("index ", index, " offset ", offset);
            }
        }
    }
}
