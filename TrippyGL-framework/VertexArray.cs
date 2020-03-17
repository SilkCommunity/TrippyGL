using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    /// <summary>
    /// Used for specifying the way vertex attributes are laid out in memory and from which
    /// <see cref="BufferObjectSubset"/> each vertex attribute comes from. Also stores an optional index buffer.
    /// </summary>
    public sealed class VertexArray : GraphicsResource
    {
        /// <summary>The handle for the GL Vertex Array Object.</summary>
        public readonly int Handle;

        /// <summary>A copy of the <see cref="VertexAttribSource"/>-s provided to the constructor. Should only be read.</summary>
        private readonly VertexAttribSource[] attribSources;

        /// <summary>A list with the sources that will feed the vertex attribute's data on draw calls.</summary>
        public ReadOnlySpan<VertexAttribSource> AttribSources => attribSources;

        public readonly IndexBufferSubset IndexBuffer;

        internal VertexArray(GraphicsDevice graphicsDevice, VertexAttribSource[] attribSources, IndexBufferSubset indexBuffer = null, bool compensateStructPadding = true, int paddingPackValue = 4)
            : base(graphicsDevice)
        {
            EnsureAttribsValid(attribSources);
            this.attribSources = attribSources;

            Handle = GL.GenVertexArray();

            IndexBuffer = indexBuffer;

            UpdateVertexAttributes(compensateStructPadding, paddingPackValue); //this also binds the vertex array
        }

        /// <summary>
        /// Creates a VertexArray with the specified attribute sources.
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use.</param>
        /// <param name="attribSources">The sources from which the data of the vertex attributes will come from.</param>
        /// <param name="indexBuffer">An index buffer to attach to the vertex array, null if none is desired.</param>
        /// <param name="compensateStructPadding">Whether to compensate for C#'s struct padding. Default is true.</param>
        /// <param name="paddingPackValue">The struct packing value for compensating for padding. C#'s default is 4.</param>
        public VertexArray(GraphicsDevice graphicsDevice, ReadOnlySpan<VertexAttribSource> attribSources, IndexBufferSubset indexBuffer = null, bool compensateStructPadding = true, int paddingPackValue = 4)
            : this(graphicsDevice, attribSources.ToArray(), indexBuffer, compensateStructPadding, paddingPackValue)
        {

        }

        /// <summary>
        /// Creates a VertexArray in which all the vertex attributes come from the same data buffer.
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use.</param>
        /// <param name="bufferSubset">The data buffer that stores all the vertex attributes.</param>
        /// <param name="attribDescriptions">The descriptions of the vertex attributes.</param>
        /// <param name="indexBuffer">An index buffer to attach to the vertex array, null if none is desired.</param>
        /// <param name="compensateStructPadding">Whether to compensate for C#'s struct padding. Default is true.</param>
        /// <param name="paddingPackValue">The struct packing value for compensating for padding. C#'s default is 4.</param>
        public VertexArray(GraphicsDevice graphicsDevice, BufferObjectSubset bufferSubset, ReadOnlySpan<VertexAttribDescription> attribDescriptions, IndexBufferSubset indexBuffer = null, bool compensateStructPadding = true, int paddingPackValue = 4)
            : this(graphicsDevice, MakeAttribList(bufferSubset, attribDescriptions), indexBuffer, compensateStructPadding, paddingPackValue)
        {

        }

        /// <summary>
        /// Updates the places where vertex data is read from for this VertexArray. Call this whenever you change a buffer subset.
        /// </summary>
        /// <param name="compensateStructPadding">Whether to automatically compensate for C#'s padding on structs.</param>
        /// <param name="paddingPackValue">The struct packing value for compensating for padding. C#'s default is 4.</param>
        public void UpdateVertexAttributes(bool compensateStructPadding = true, int paddingPackValue = 4)
        {
            // Makes all glVertexAttribPointer calls to specify the vertex attrib data on the VAO and enables the vertex attributes.
            // The parameters of glVertexAttribPointer are calculated based on the VertexAttribSource-s from AttribSources

            GraphicsDevice.VertexArray = this;

            AttribCallDesc[] calls = new AttribCallDesc[attribSources.Length];

            int attribIndex = 0;
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
                int offset = 0;
                BufferObjectSubset prevSubset = null; //setting this to null ensures the first for loop will enter the "different subset" if and initialize these variables
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
                            int packval = Math.Min(TrippyUtils.GetVertexAttribSizeInBytes(currentBaseType), paddingPackValue); // offset should be aligned by the default packing value or the size of the base type
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
                int offset = 0;
                int prevBufferHandle = -1;
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
                calls[i].CallGlVertexAttribPointer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer == null ? 0 : IndexBuffer.BufferHandle);
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
                ", ", IndexBuffer == null ? "no index buffer" : "has index buffer"
            );
        }

        private static VertexAttribSource[] MakeAttribList(BufferObjectSubset bufferSubset, ReadOnlySpan<VertexAttribDescription> attribDescriptions)
        {
            VertexAttribSource[] sources = new VertexAttribSource[attribDescriptions.Length];
            for (int i = 0; i < sources.Length; i++)
                sources[i] = new VertexAttribSource(bufferSubset, attribDescriptions[i]);
            return sources;
        }

        /// <summary>
        /// Creates a VertexArray for the specified vertex type, where all of the vertex attributes come from the same buffer.
        /// </summary>
        /// <typeparam name="T">The type of vertex to use</typeparam>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use.</param>
        /// <param name="dataBuffer">The buffer from which all attributes come from.</param>
        /// <param name="indexBuffer">An index buffer to attach to the vertex array, null if none is desired.</param>
        /// <param name="compensateStructPadding">Whether to compensate for C#'s struct padding. Default is true.</param>
        public static VertexArray CreateSingleBuffer<T>(GraphicsDevice graphicsDevice, BufferObjectSubset dataBuffer, IndexBufferSubset indexBuffer = null, bool compensateStructPadding = true) where T : struct, IVertex
        {
            T t = default;
            int attribCount = t.AttribDescriptionCount;
            Span<VertexAttribDescription> attribDescriptions = attribCount > 256 ?
                new VertexAttribDescription[attribCount] : stackalloc VertexAttribDescription[attribCount];
            t.WriteAttribDescriptions(attribDescriptions);

            return new VertexArray(graphicsDevice, dataBuffer, attribDescriptions, indexBuffer, compensateStructPadding);
        }

        private void EnsureAttribsValid(VertexAttribSource[] attribSources)
        {
            if (attribSources.Length == 0)
                throw new ArgumentException("You can't create a " + nameof(VertexArray) + " with no attributes", nameof(attribSources));

            int attribIndexCount = 0;
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
        /// This is a helper class for VertexArray.UpdateVertexAttributes().
        /// </summary>
        private struct AttribCallDesc
        {
            public VertexAttribSource source;
            public int index, offset;

            public void CallGlVertexAttribPointer()
            {
                source.BufferSubset.Buffer.GraphicsDevice.BindBuffer(source.BufferSubset);
                int offs = offset + source.BufferSubset.StorageOffsetInBytes;
                int stride = ((IDataBufferSubset)source.BufferSubset).ElementSize;
                for (int i = 0; i < source.AttribDescription.AttribIndicesUseCount; i++)
                {
                    if (!source.AttribDescription.Normalized && source.AttribDescription.AttribBaseType == VertexAttribPointerType.Double)
                        GL.VertexAttribLPointer(index + i, source.AttribDescription.Size, VertexAttribDoubleType.Double, stride, (IntPtr)offs);
                    else if (!source.AttribDescription.Normalized && TrippyUtils.IsVertexAttribIntegerType(source.AttribDescription.AttribType))
                        GL.VertexAttribIPointer(index + i, source.AttribDescription.Size, (VertexAttribIntegerType)source.AttribDescription.AttribBaseType, stride, (IntPtr)offs);
                    else
                        GL.VertexAttribPointer(index + i, source.AttribDescription.Size, source.AttribDescription.AttribBaseType, source.AttribDescription.Normalized, stride, offs);

                    GL.EnableVertexAttribArray(index + i);
                    if (source.AttribDescription.AttribDivisor != 0)
                        GL.VertexAttribDivisor(index + i, source.AttribDescription.AttribDivisor);
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
