namespace TrippyGL
{
    /// <summary>
    /// Specifies the buffer binding points at which a <see cref="BufferObject"/> can be bound.
    /// </summary>
    public enum BufferTarget
    {
        ParameterBuffer = 33006,
        ArrayBuffer = 34962,
        ElementArrayBuffer = 34963,
        PixelPackBuffer = 35051,
        PixelUnpackBuffer = 35052,
        UniformBuffer = 35345,
        TextureBuffer = 35882,
        TransformFeedbackBuffer = 35982,
        CopyReadBuffer = 36662,
        CopyWriteBuffer = 36663,
        DrawIndirectBuffer = 36671,
        ShaderStorageBuffer = 37074,
        DispatchIndirectBuffer = 37102,
        QueryBuffer = 37266,
        AtomicCounterBuffer = 37568
    }
}
