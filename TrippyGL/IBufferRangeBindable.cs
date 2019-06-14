namespace TrippyGL
{
    /// <summary>
    /// This interface is implemented by BufferObjects that can be bound to a ranged target (such as UniformBufferObject)
    /// </summary>
    internal interface IBufferRangeBindable
    {
        /// <summary>
        /// Ensure a specified element of the buffer is bound to the specified binding index
        /// </summary>
        /// <param name="bindingIndex">The target index to bind to</param>
        /// <param name="elementIndex">The index of the element in the buffer whose range to bind</param>
        void EnsureBoundRange(int bindingIndex, int elementIndex);
    }
}
