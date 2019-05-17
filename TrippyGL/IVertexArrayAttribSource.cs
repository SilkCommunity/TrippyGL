namespace TrippyGL
{
    /// <summary>
    /// Defines a source of data for a vertex attribute. This data must be stored in an OpenGL Buffer Object bound to GL_ARRAY_BUFFER
    /// </summary>
    public interface IVertexArrayAttribSource
    {
        /// <summary>
        /// Ensure the OpenGL Buffer Object is bound to GL_ARRAY_BUFFER
        /// </summary>
        void EnsureBound();
    }
}
