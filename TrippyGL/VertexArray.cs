using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    public class VertexArray : IDisposable
    {
        /// <summary>The last vertex array object's name to be bound. This variable is used on EnsureBound() so to not bind the same vao consecutively</summary>
        private static int lastArrayBound = -1;

        /// <summary>
        /// Resets the last bound vertex array object variable. This variable is used on EnsureBound() to not call glBindVertexArray if the array is already bound.
        /// You might want to call this if interoperating with another library
        /// </summary>
        public static void ResetBindState()
        {
            lastArrayBound = GL.GetInteger(GetPName.VertexArrayBinding);
        }

        /// <summary>
        /// Unbinds the current VertexArray by binding to vertex array object 0
        /// </summary>
        public static void BindEmpty()
        {
            GL.BindVertexArray(0);
            lastArrayBound = 0;
        }


        /// <summary>The GL VertexArrayObject's name</summary>
        public readonly int Handle;

        /// <summary>The sources that will feed the vertex attribute's data on draw calls</summary>
        public readonly VertexAttribSource[] AttribSources;

        /// <summary>
        /// Creates a VertexArray with the specified attribute sources
        /// </summary>
        /// <param name="attribSources"></param>
        public VertexArray(VertexAttribSource[] attribSources)
        {
            if (attribSources == null)
                throw new ArgumentNullException("attribSources");

            Handle = GL.GenVertexArray();
            this.AttribSources = attribSources;

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
            EnsureBound();

            for (int i = 0; i < AttribSources.Length; i++)
            {
                VertexAttribSource vas = AttribSources[i];
                int stride = 0, offset = 0;
                for (int c = 0; c < AttribSources.Length; c++)
                {
                    // Calculates the stride and offset values for this attribute.
                    // These are calculated by going through all other vertex attributes and finding the sources that share the same BufferObject
                    // as this VertexAttribSource. If they have the same BufferObject, then add that attribute's size in bytes to stride. If that
                    // attribute is found before this one, then it's size in bytes is also added to the offset.

                    VertexAttribSource vas2 = AttribSources[c];
                    if (vas2.DataBuffer == vas.DataBuffer)
                    {
                        int sib = vas2.SizeInBytes;
                        stride += sib;
                        if (c < i)
                            offset += sib;
                    }
                }

                vas.DataBuffer.EnsureBound();
                GL.VertexAttribPointer(i, vas.Size, vas.AttribType, vas.Normalized, stride, offset);
                GL.EnableVertexAttribArray(i);
            }
        }

        /// <summary>
        /// Ensure that this vertex array is the currently bound one
        /// </summary>
        public void EnsureBound()
        {
            if (lastArrayBound != Handle)
            {
                GL.BindVertexArray(Handle);
                lastArrayBound = Handle;
            }
        }

        private void dispose()
        {
            GL.DeleteVertexArray(Handle);
        }

        public void Dispose()
        {
            dispose();
            GC.SuppressFinalize(this);
        }

    }
}
