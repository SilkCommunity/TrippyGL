using System.Text;

namespace TrippyGL
{
    /// <summary>
    /// A read-only list of VertexAttribSource-s
    /// </summary>
    public class VertexAttribSourceList
    {
        /// <summary>The internal VertexAttribSource array. Marked internal for convenience, this should only be read</summary>
        internal readonly VertexAttribSource[] sources;

        /// <summary>
        /// Gets a VertexAttribSource from the list
        /// </summary>
        /// <param name="index">The list index of the VertexAttribSource</param>
        public VertexAttribSource this[int index] { get { return sources[index]; } }

        /// <summary>The amount of VertexAttribSource-s in this list</summary>
        public int Length { get { return sources.Length; } }

        /// <summary>
        /// Creates a VertexAttribSourceList by copying the VertexAttribSource from the specified array
        /// </summary>
        /// <param name="sources"></param>
        internal VertexAttribSourceList(VertexAttribSource[] sources)
        {
            this.sources = new VertexAttribSource[sources.Length];
            for (int i = 0; i < sources.Length; i++)
                this.sources[i] = new VertexAttribSource(sources[i].BufferSubset, sources[i].AttribDescription);
        }

        /// <summary>
        /// Creates a VertexAttribSourceList where all the VertexAttribSources use the same BufferObjectSubset
        /// </summary>
        /// <param name="bufferSubset"></param>
        /// <param name="attribDescriptions"></param>
        internal VertexAttribSourceList(BufferObjectSubset bufferSubset, VertexAttribDescription[] attribDescriptions)
        {
            sources = new VertexAttribSource[attribDescriptions.Length];
            for (int i = 0; i < sources.Length; i++)
                sources[i] = new VertexAttribSource(bufferSubset, attribDescriptions[i]);
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
