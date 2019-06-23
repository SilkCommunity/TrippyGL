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
            for(int i=0; i<calls.Length; i++)
            {
                calls[i] = new AttribCallDesc
                {
                    source = AttribSources[i],
                    index = attribIndex
                };
                attribIndex += calls[i].source.AttribDescription.AttribIndicesUseCount;
            }

            // Sort by buffer object, so all sources that share BufferObject are grouped together
            Array.Sort(calls, (x, y) => x.source.DataBuffer.Handle.CompareTo(y.source.DataBuffer.Handle));


            if (compensateStructPadding)
            {
                #region CalculateOffsetsWithPadding
                int offset = 0;
                int prevBufferHandle = -1; //setting this to -1 ensures the first for loop will enter the "different struct" if and initialize these variables
                VertexAttribPointerType currentBaseType = 0;
                int baseTypeCount = 0, baseTypeByteSize = 0;

                for (int i = 0; i < calls.Length; i++)
                {
                    if (calls[i].source.DataBuffer.Handle != prevBufferHandle)
                    {
                        // it's a different buffer, so let's calculate the padding values as for a new, different struct
                        offset = 0;
                        baseTypeCount = 0;
                        prevBufferHandle = calls[i].source.DataBuffer.Handle;
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
                    if (prevBufferHandle != calls[i].source.DataBuffer.Handle)
                    {
                        prevBufferHandle = calls[i].source.DataBuffer.Handle;
                        offset = 0;
                    }

                    calls[i].offset = offset;
                    offset += calls[i].source.AttribDescription.SizeInBytes;
                }
                #endregion
            }

            for (int i = 0; i < calls.Length; i++)
            {
                GraphicsDevice.BindBuffer(calls[i].source.DataBuffer);
                calls[i].CallGlVertexAttribPointer();
            }
        }

        protected override void Dispose(bool isManualDispose)
        {
            GL.DeleteVertexArray(this.Handle);
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
        public static VertexArray CreateSingleBuffer<T>(GraphicsDevice graphicsDevice, BufferObject dataBuffer, bool compensateStructPadding = true) where T : struct, IVertex
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
                int offs = offset;
                //GraphicsDevice.EnsureBufferBound(source.DataBuffer);
                for (int i = 0; i < source.AttribDescription.AttribIndicesUseCount; i++)
                {
                    if (!source.AttribDescription.Normalized && source.AttribDescription.AttribBaseType == VertexAttribPointerType.Double)
                        GL.VertexAttribLPointer(index + i, source.AttribDescription.Size, VertexAttribDoubleType.Double, source.DataBuffer.ElementSize, (IntPtr)offs);
                    else if (!source.AttribDescription.Normalized && TrippyUtils.IsVertexAttribIntegerType(source.AttribDescription.AttribType))
                        GL.VertexAttribIPointer(index + i, source.AttribDescription.Size, (VertexAttribIntegerType)source.AttribDescription.AttribBaseType, source.DataBuffer.ElementSize, (IntPtr)offs);
                    else
                        GL.VertexAttribPointer(index + i, source.AttribDescription.Size, source.AttribDescription.AttribBaseType, source.AttribDescription.Normalized, source.DataBuffer.ElementSize, offs);

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
