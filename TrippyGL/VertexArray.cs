using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    public class VertexArray : IDisposable
    {
        /// <summary>Gets the last VertexArray to get bound</summary>
        public static VertexArray LastArrayBound { get; private set; } = null;

        /// <summary>
        /// Resets the last bound vertex array object variable. This variable is used on EnsureBound() to not call glBindVertexArray if the array is already bound.
        /// You might want to call this if interoperating with another library
        /// </summary>
        public static void ResetBindState()
        {
            LastArrayBound = null;
        }

        /// <summary>
        /// Unbinds the current VertexArray by binding to vertex array object 0
        /// </summary>
        public static void BindEmpty()
        {
            GL.BindVertexArray(0);
            LastArrayBound = null;
        }


        /// <summary>The GL VertexArrayObject's name</summary>
        public readonly int Handle;

        /// <summary>The sources that will feed the vertex attribute's data on draw calls</summary>
        public readonly VertexAttribSource[] AttribSources;

        /// <summary>
        /// Creates a VertexArray with the specified attribute sources
        /// </summary>
        /// <param name="attribSources"></param>
        public VertexArray(params VertexAttribSource[] attribSources)
        {
            if (attribSources == null)
                throw new ArgumentNullException("attribSources");

            if (attribSources.Length == 0)
                throw new ArgumentException("You can't create a VertexArray with no attributes", "attribSources");

            Handle = GL.GenVertexArray();
            this.AttribSources = attribSources;

            MakeVertexAttribPointerCalls();
        }

        /// <summary>
        /// Creates a VertexArray in which all the vertex attributes come from the same data buffer
        /// </summary>
        /// <param name="dataBuffer">The data buffer that stores all the vertex attributes</param>
        /// <param name="attribDescriptions">The descriptions of the vertex attributes</param>
        public VertexArray(BufferObject dataBuffer, VertexAttribDescription[] attribDescriptions)
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

            MakeVertexAttribPointerCalls();
        }

        ~VertexArray()
        {
            if (TrippyLib.isLibActive)
                dispose();
        }

        /// <summary>
        /// Makes all glVertexAttribPointer calls to specify the vertex attrib data on the VAO and enables the vertex attributes.
        /// The parameters of glVertexAttribPointer are calculated based on the VertexAttribSource-s from AttribSources
        /// </summary>
        private void MakeVertexAttribPointerCalls()
        {
            // TODO: Compensate for C#'s struct padding?
            EnsureBound();

            int attribIndex = 0;
            for (int i = 0; i < AttribSources.Length; i++)
            {
                VertexAttribSource vas = AttribSources[i];
                
                int offset = 0;
                for (int c = 0; c < i; c++)
                {
                    // Calculates the stride and offset values for this attribute.
                    // These are calculated by going through all other vertex attributes and finding the sources that share the same BufferObject
                    // as this VertexAttribSource. If they have the same BufferObject, then add that attribute's size in bytes to stride. If that
                    // attribute is found before this one, then it's size in bytes is also added to the offset.

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
            }
        }

        /// <summary>
        /// Ensure that this vertex array is the currently bound one, and binds it otherwise
        /// </summary>
        public void EnsureBound()
        {
            if (LastArrayBound != this)
            {
                Bind();
            }
        }

        /// <summary>
        /// Binds this VertexArray. Prefer using EnsureBound() to avoid unnecessary binds
        /// </summary>
        public void Bind()
        {
            GL.BindVertexArray(Handle);
            LastArrayBound = this;
        }

        /// <summary>
        /// This method disposes the Vertex Array with no checks at all
        /// </summary>
        private void dispose()
        {
            GL.DeleteVertexArray(Handle);
        }

        /// <summary>
        /// Disposes this Vertex Array, deleting and releasing the resources it uses.
        /// The Vertex Array cannot be used after it's been disposed
        /// </summary>
        public void Dispose()
        {
            dispose();
            GC.SuppressFinalize(this);
        }

    }
}
