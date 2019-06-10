using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrippyGL
{
    /// <summary>
    /// A read-only list of VertexAttribSource-s
    /// </summary>
    public class VertexAttribSourceList
    {
        /// <summary>The VertexAttribSource-s</summary>
        internal readonly VertexAttribSource[] sources; //marked internal for convenience, this should only be read.

        public VertexAttribSource this[int index] { get { return sources[index]; } }

        public int Length { get { return sources.Length; } }

        public VertexAttribSourceList(VertexAttribSource[] sources)
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
