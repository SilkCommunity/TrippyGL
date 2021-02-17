namespace TrippyGL
{
    /// <summary>
    /// Specifies formats a <see cref="RenderbufferObject"/>'s storage can have.
    /// </summary>
    public enum RenderbufferFormat
    {
        Color4b = 32856,

        Float = 33326,
        Float2 = 33328,
        Float4 = 34836,

        Int = 33333,
        Int2 = 33339,
        Int4 = 36226,

        UnsignedInt = 33334,
        UnsignedInt2 = 33340,
        UnsignedInt4 = 36208,

        Depth16 = 33189,
        Depth24 = 33190,
        Depth32f = 36012,
        Depth24Stencil8 = 35056,
        Depth32fStencil8 = 36013,
        Stencil8 = 36168,
    }
}
