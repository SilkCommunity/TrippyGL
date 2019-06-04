using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    public class VertexArray : GraphicsResource
    {
        /// <summary>The GL VertexArrayObject's name</summary>
        public readonly int Handle;

        /// <summary>The sources that will feed the vertex attribute's data on draw calls</summary>
        public readonly VertexAttribSource[] AttribSources;

        /// <summary>
        /// Creates a VertexArray with the specified attribute sources
        /// </summary>
        /// <param name="attribSources">The sources from which the data of the vertex attributes will come from</param>
        /// <param name="compensateStructPadding">Whether to compensate for C#'s struct padding. Default is true</param>
        public VertexArray(GraphicsDevice graphicsDevice, VertexAttribSource[] attribSources, bool compensateStructPadding = true)
            : base(graphicsDevice)
        {
            if (attribSources == null)
                throw new ArgumentNullException("attribSources");

            if (attribSources.Length == 0)
                throw new ArgumentException("You can't create a VertexArray with no attributes", "attribSources");

            Handle = GL.GenVertexArray();
            this.AttribSources = attribSources;

            MakeVertexAttribPointerCalls(compensateStructPadding);
        }

        /// <summary>
        /// Creates a VertexArray in which all the vertex attributes come from the same data buffer
        /// </summary>
        /// <param name="dataBuffer">The data buffer that stores all the vertex attributes</param>
        /// <param name="attribDescriptions">The descriptions of the vertex attributes</param>
        /// <param name="compensateStructPadding">Whether to compensate for C#'s struct padding. Default is true</param>
        public VertexArray(GraphicsDevice graphicsDevice, BufferObject dataBuffer, VertexAttribDescription[] attribDescriptions, bool compensateStructPadding = true)
            : base(graphicsDevice)
        {
            if (dataBuffer == null)
                throw new ArgumentNullException("dataBuffer");

            if (attribDescriptions == null)
                throw new ArgumentNullException("attribDescriptions");

            if (attribDescriptions.Length == 0)
                throw new ArgumentException("You can't create a VertexArray with no attributes", "attribDescriptions");

            Handle = GL.GenVertexArray();

            AttribSources = new VertexAttribSource[attribDescriptions.Length];
            for (int i = 0; i < AttribSources.Length; i++)
                AttribSources[i] = new VertexAttribSource(dataBuffer, attribDescriptions[i]);

            MakeVertexAttribPointerCalls(compensateStructPadding);
        }

        /// <summary>
        /// Makes all glVertexAttribPointer calls to specify the vertex attrib data on the VAO and enables the vertex attributes.
        /// The parameters of glVertexAttribPointer are calculated based on the VertexAttribSource-s from AttribSources
        /// </summary>
        /// <param name="compensateStructPadding">Whether to automatically compensate for C#'s padding on structs</param>
        /// <param name="packValue">The struct packing value for compensating for padding. C#'s default is 4</param>
        private void MakeVertexAttribPointerCalls(bool compensateStructPadding, int packValue = 4)
        {
            States.EnsureVertexArrayBound(this);

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
                        baseTypeByteSize = VertexAttribDescription.GetSizeInBytesOfAttribType(currentBaseType);
                        calls[i].offset = 0;
                    }
                    else if (currentBaseType != calls[i].source.AttribDescription.AttribBaseType)
                    {
                        // the base type has changed, let's ensure padding is applied to offset
                        baseTypeCount = 0;
                        currentBaseType = calls[i].source.AttribDescription.AttribBaseType;
                        baseTypeByteSize = VertexAttribDescription.GetSizeInBytesOfAttribType(currentBaseType);
                        int p = Math.Min(baseTypeByteSize, packValue);
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
                calls[i].CallGlVertexAttribPointer();

            /*int attribIndex = 0;
            for (int i = 0; i < AttribSources.Length; i++)
            {
                VertexAttribSource vas = AttribSources[i];
                
                int offset = 0;
                for (int c = 0; c < i; c++)
                {
                    // Calculate the offset value for this attribute.
                    // To to this, we go through all previous vertex attributes and find those with the same BufferObject
                    // When we find an attribute that is before this one that shares BufferObject, we add it's size in bytes to the offset
                    // We might want to compensate for the struct padding, so if compensateStructPadding is true we fix up the value.

                    VertexAttribSource vas2 = AttribSources[c];
                    if (vas2.DataBuffer == vas.DataBuffer)
                        offset += vas2.AttribDescription.SizeInBytes;
                }

                vas.DataBuffer.EnsureBound(); // WE DON'T NEED TO CALCULATE STRIDE, JUST USE vas.DataBuffer.ElementSize
                for (int c = 0; c < vas.AttribDescription.AttribIndicesUseCount; c++)
                {
                    if (!vas.AttribDescription.Normalized && vas.AttribDescription.AttribBaseType == VertexAttribPointerType.Double)
                        GL.VertexAttribLPointer(attribIndex, vas.AttribDescription.Size, VertexAttribDoubleType.Double, vas.DataBuffer.ElementSize, (IntPtr)offset);
                    else if (!vas.AttribDescription.Normalized && VertexAttribDescription.IsIntegerType(vas.AttribDescription.AttribBaseType))
                        GL.VertexAttribIPointer(attribIndex, vas.AttribDescription.Size, (VertexAttribIntegerType)vas.AttribDescription.AttribBaseType, vas.DataBuffer.ElementSize, (IntPtr)offset);
                    else
                        GL.VertexAttribPointer(attribIndex, vas.AttribDescription.Size, vas.AttribDescription.AttribBaseType, vas.AttribDescription.Normalized, vas.DataBuffer.ElementSize, offset);

                    GL.EnableVertexAttribArray(attribIndex);
                    attribIndex++;
                    offset += vas.AttribDescription.SizeInBytes / vas.AttribDescription.AttribIndicesUseCount;
                }
            }*/
        }

        protected override void Dispose(bool isManualDispose)
        {
            GL.DeleteVertexArray(this.Handle);
            base.Dispose(isManualDispose);
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
                States.EnsureBufferBound(source.DataBuffer);
                for (int i = 0; i < source.AttribDescription.AttribIndicesUseCount; i++)
                {
                    if (!source.AttribDescription.Normalized && source.AttribDescription.AttribBaseType == VertexAttribPointerType.Double)
                        GL.VertexAttribLPointer(index + i, source.AttribDescription.Size, VertexAttribDoubleType.Double, source.DataBuffer.ElementSize, (IntPtr)offs);
                    else if (!source.AttribDescription.Normalized && VertexAttribDescription.IsIntegerType(source.AttribDescription.AttribType))
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
