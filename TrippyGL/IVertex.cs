using System;

namespace TrippyGL
{
    /// <summary>
    /// This interface is used by different classes that need vertex specification (such as
    /// <see cref="VertexBuffer{T}"/>, <see cref="VertexArray"/>) to be able to handle vertex specification
    /// work in a more convenient way.<para/>
    /// All vertex structs that properly implement this interface can be easily used with these classes.
    /// </summary>
    public interface IVertex
    {
        /// <summary>The amount of attribute descriptions this vertex type has.</summary>
        int AttribDescriptionCount { get; }

        /// <summary>
        /// Gets the vertex attrib descriptors by writting them into a <see cref="Span{T}"/>.
        /// The <see cref="Span{T}"/> must have a length of <see cref="AttribDescriptionCount"/>.
        /// </summary>
        void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions);
    }
}
