using System;
using System.Text;

namespace TrippyGL
{
    /// <summary>
    /// A read-only list of <see cref="VertexAttribSource"/>-s.
    /// </summary>
    public sealed class VertexAttribSourceList
    {
        /// <summary>The internal <see cref="VertexAttribSource"/> array. Marked internal for convenience, this should only be read.</summary>
        internal readonly VertexAttribSource[] sources;

        /// <summary>
        /// Gets a <see cref="VertexAttribSource"/> from the list by index.
        /// </summary>
        /// <param name="index">The list index of the <see cref="VertexAttribSource"/>.</param>
        public VertexAttribSource this[int index] => sources[index];

        /// <summary>The amount of <see cref="VertexAttribSource"/>-s in this list.</summary>
        public int Length => sources.Length;

        /// <summary>
        /// Creates a <see cref="VertexAttribSourceList"/> by copying the <see cref="VertexAttribSource"/>-s from
        /// the specified <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <param name="sources"></param>
        internal VertexAttribSourceList(ReadOnlySpan<VertexAttribSource> sources)
        {
            this.sources = new VertexAttribSource[sources.Length];
            for (int i = 0; i < sources.Length; i++)
                this.sources[i] = sources[i];
        }

        /// <summary>
        /// Creates a <see cref="VertexAttribSourceList"/> where all the <see cref="VertexAttribSource"/>-s
        /// use the same <see cref="BufferObjectSubset"/>.
        /// </summary>
        /// <param name="bufferSubset"></param>
        /// <param name="attribDescriptions"></param>
        internal VertexAttribSourceList(BufferObjectSubset bufferSubset, ReadOnlySpan<VertexAttribDescription> attribDescriptions)
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
