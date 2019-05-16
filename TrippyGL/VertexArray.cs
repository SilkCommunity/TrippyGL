using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public class VertexArray : IDisposable
    {
        private static int lastArrayBound = -1;
        public static void ResetBindState()
        {
            lastArrayBound = -1;
        }

        /// <summary></summary>
        public readonly int Handle;
        public readonly VertexAttribSource[] AttribSources;

        public VertexArray(VertexAttribSource[] attribSources)
        {
            Handle = GL.GenVertexArray();
            this.AttribSources = attribSources;

            MakeVertexAttribPointerCalls();
        }

        ~VertexArray()
        {
            dispose();
        }

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
