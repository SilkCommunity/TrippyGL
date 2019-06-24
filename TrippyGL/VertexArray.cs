using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    /// <summary>
    /// Vertex arrays are used for specifying the way vertex attributes are laid out in memory and from which buffer object each data attribute comes from.
    /// </summary>
    public class VertexArray : GraphicsResource
    {
        /// <summary>The GL VertexArrayObject's name</summary>
        public readonly int Handle;

        /// <summary>A list with the sources that will feed the vertex attribute's data on draw calls</summary>
        public readonly VertexAttribSourceList AttribSources;

        /// <summary>
        /// Creates a VertexArray with the specified attribute sources
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="attribSources">The sources from which the data of the vertex attributes will come from</param>
        /// <param name="compensateStructPadding">Whether to compensate for C#'s struct padding. Default is true</param>
        /// <param name="paddingPackValue">The struct packing value for compensating for padding. C#'s default is 4</param>
        public VertexArray(GraphicsDevice graphicsDevice, VertexAttribSource[] attribSources, bool compensateStructPadding = true, int paddingPackValue = 4)
            : base(graphicsDevice)
        {
            if (attribSources == null)
                throw new ArgumentNullException("attribSources");

            if (attribSources.Length == 0)
                throw new ArgumentException("You can't create a VertexArray with no attributes", "attribSources");

            Handle = GL.GenVertexArray();
            AttribSources = new VertexAttribSourceList(attribSources);

            MakeVertexAttribPointerCalls(compensateStructPadding, paddingPackValue);
        }

        /// <summary>
        /// Creates a VertexArray in which all the vertex attributes come from the same data buffer
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="bufferSubset">The data buffer that stores all the vertex attributes</param>
        /// <param name="attribDescriptions">The descriptions of the vertex attributes</param>
        /// <param name="compensateStructPadding">Whether to compensate for C#'s struct padding. Default is true</param>
        /// <param name="paddingPackValue">The struct packing value for compensating for padding. C#'s default is 4</param>
        public VertexArray(GraphicsDevice graphicsDevice, BufferObjectSubset bufferSubset, VertexAttribDescription[] attribDescriptions, bool compensateStructPadding = true, int paddingPackValue = 4)
            : base(graphicsDevice)
        {
            if (bufferSubset == null)
                throw new ArgumentNullException("dataBuffer");

            if (attribDescriptions == null)
                throw new ArgumentNullException("attribDescriptions");

            if (attribDescriptions.Length == 0)
                throw new ArgumentException("You can't create a VertexArray with no attributes", "attribDescriptions");

            Handle = GL.GenVertexArray();

            VertexAttribSource[] s = new VertexAttribSource[attribDescriptions.Length];
            for (int i = 0; i < s.Length; i++)
                s[i] = new VertexAttribSource(bufferSubset, attribDescriptions[i]);
            AttribSources = new VertexAttribSourceList(s);

            MakeVertexAttribPointerCalls(compensateStructPadding, paddingPackValue);
        }

        /// <summary>
        /// Makes all glVertexAttribPointer calls to specify the vertex attrib data on the VAO and enables the vertex attributes.
        /// The parameters of glVertexAttribPointer are calculated based on the VertexAttribSource-s from AttribSources
        /// </summary>
        /// <param name="compensateStructPadding">Whether to automatically compensate for C#'s padding on structs</param>
        /// <param name="paddingPackValue">The struct packing value for compensating for padding. C#'s default is 4</param>
        private void MakeVertexAttribPointerCalls(bool compensateStructPadding, int paddingPackValue = 4)
        {
            GraphicsDevice.BindVertexArray(this);

            AttribCallDesc[] calls = new AttribCallDesc[AttribSources.Length];

            int attribIndex = 0;
            for (int i = 0; i < calls.Length; i++)
            {
                calls[i] = new AttribCallDesc
                {
                    source = AttribSources[i],
                    index = attribIndex
                };
                attribIndex += calls[i].source.AttribDescription.AttribIndicesUseCount;
            }

            // Sort by buffer object, so all sources that share BufferObject are grouped together
            Array.Sort(calls, (x, y) => x.source.BufferSubset.BufferHandle.CompareTo(y.source.BufferSubset.BufferHandle));


            if (compensateStructPadding)
            {
                #region CalculateOffsetsWithPadding
                int offset = 0;
                int prevBufferHandle = -1; //setting this to -1 ensures the first for loop will enter the "different struct" if and initialize these variables
                VertexAttribPointerType currentBaseType = 0;
                int baseTypeCount = 0, baseTypeByteSize = 0;

                for (int i = 0; i < calls.Length; i++)
                {
                    if (calls[i].source.BufferSubset.BufferHandle != prevBufferHandle)
                    {
                        // it's a different buffer, so let's calculate the padding values as for a new, different struct
                        offset = 0;
                        baseTypeCount = 0;
                        prevBufferHandle = calls[i].source.BufferSubset.BufferHandle;
                        currentBaseType = calls[i].source.AttribDescription.AttribBaseType;
                        baseTypeByteSize = TrippyUtils.GetVertexAttribSizeInBytes(currentBaseType);
                        calls[i].offset = 0;
                    }
                    else if (currentBaseType != calls[i].source.AttribDescription.AttribBaseType)
                    {
                        // the base type has changed, let's ensure padding is applied to offset
                        baseTypeCount = 0;
                        currentBaseType = calls[i].source.AttribDescription.AttribBaseType;
                        baseTypeByteSize = TrippyUtils.GetVertexAttribSizeInBytes(currentBaseType);
                        int p = Math.Min(baseTypeByteSize, paddingPackValue);
                        offset = (offset + p - 1) / p * p;
                    }
                    baseTypeCount += calls[i].source.AttribDescription.Size * calls[i].source.AttribDescription.AttribIndicesUseCount;

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
            {
                GraphicsDevice.BindBuffer(calls[i].source.BufferSubset);
                calls[i].CallGlVertexAttribPointer();
            }
        }

        protected override void Dispose(bool isManualDispose)
        {
            GL.DeleteVertexArray(Handle);
            base.Dispose(isManualDispose);
        }

        public override string ToString()
        {
            return String.Concat("AttribSources: {", AttribSources.ToString(), "}");
        }

        /// <summary>
        /// Creates a VertexArray for the specified vertex type, where all of the vertex attributes come from the same buffer
        /// </summary>
        /// <typeparam name="T">The type of vertex to use</typeparam>
        /// <param name="dataBuffer">The buffer from which all attributes come from</param>
        /// <param name="compensateStructPadding">Whether to compensate for C#'s struct padding. Default is true</param>
        public static VertexArray CreateSingleBuffer<T>(GraphicsDevice graphicsDevice, BufferObjectSubset dataBuffer, bool compensateStructPadding = true) where T : struct, IVertex
        {
            VertexAttribDescription[] desc = new T().AttribDescriptions;
            return new VertexArray(graphicsDevice, dataBuffer, desc, true);
        }


        private class AttribCallDesc
        {
            public VertexAttribSource source;
            public int index, offset;

            public void CallGlVertexAttribPointer()
            {
                int offs = offset + source.BufferSubset.StorageOffsetInBytes;
                for (int i = 0; i < source.AttribDescription.AttribIndicesUseCount; i++)
                {
                    int stride = ((IDataBufferSubset)source.BufferSubset).ElementSize;
                    if (!source.AttribDescription.Normalized && source.AttribDescription.AttribBaseType == VertexAttribPointerType.Double)
                        GL.VertexAttribLPointer(index + i, source.AttribDescription.Size, VertexAttribDoubleType.Double, stride, (IntPtr)offs);
                    else if (!source.AttribDescription.Normalized && TrippyUtils.IsVertexAttribIntegerType(source.AttribDescription.AttribType))
                        GL.VertexAttribIPointer(index + i, source.AttribDescription.Size, (VertexAttribIntegerType)source.AttribDescription.AttribBaseType, stride, (IntPtr)offs);
                    else
                        GL.VertexAttribPointer(index + i, source.AttribDescription.Size, source.AttribDescription.AttribBaseType, source.AttribDescription.Normalized, stride, offs);

                    GL.EnableVertexAttribArray(index + i);
                    offs += source.AttribDescription.SizeInBytes / source.AttribDescription.AttribIndicesUseCount;
                }
            }

            public override string ToString()
            {
                return String.Concat("index ", index, " offset ", offset);
            }
        }
    }
}
