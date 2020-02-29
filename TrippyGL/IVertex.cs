namespace TrippyGL
{
    /// <summary>
    /// This interface is used by different classes that need vertex specification (<see cref="VertexArray"/>, <see cref="ShaderProgram"/>)
    /// to be able to handle vertex specification work in a more convenient way.<para/>
    /// All vertex structs that properly implement this interface can be easily used with these classes.
    /// </summary>
    public interface IVertex
    {
        /// <summary>
        /// An array with the descriptions of all the vertex attributes present in this vertex.
        /// </summary>
        VertexAttribDescription[] AttribDescriptions { get; }
    }
}
