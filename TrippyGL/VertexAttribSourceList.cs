using System;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// A read-only list of VertexAttribSource-s
    /// </summary>
    public class VertexAttribSourceList
    {
        /// <summary>The internal VertexAttribSource array</summary>
        internal readonly VertexAttribSource[] sources; //marked internal for convenience, this should only be read

        /// <summary>
        /// Gets a VertexAttribSource from the list
        /// </summary>
        /// <param name="index">The list index of the VertexAttribSource</param>
        public VertexAttribSource this[int index] { get { return sources[index]; } }

        /// <summary>The amount of VertexAttribSource-s in this list</summary>
        public int Length { get { return sources.Length; } }

        internal VertexAttribSourceList(VertexAttribSource[] sources)
        {
            this.sources = sources;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(sources.Length * 72);
            int max = sources.Length - 1;
            for (int i = 0; i < max; i++)
            {
                builder.Append('{');
                builder.Append(sources[i].ToString());
                builder.AppendLine("}, ");
            }
            builder.Append('{');
            builder.Append(sources[max].ToString());
            builder.Append('}');
            return builder.ToString();
        }
    }
}
