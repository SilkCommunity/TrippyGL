namespace TrippyGL
{
    /// <summary>
    /// Specifies formats a <see cref="Texture"/>'s image can have.
    /// </summary>
    public enum TextureImageFormat
    {
        // These are organized in such away so the base type (float, int, uint)
        // is distinguisable by dividing by 32 and the remainder indicates the amount of components
        // (amount of components: Color4b has 4, Vector2 has 2, Vector3i has 3, etc)
        // This is done in TrippyUtils.GetTextureFormatEnums()

        Color4b = 5,

        Float = 33,
        Float2 = 34,
        Float3 = 35,
        Float4 = 36,
        Depth16 = 37,
        Depth24 = 38,
        Depth32f = 39,

        Int = 65,
        Int2 = 66,
        Int3 = 67,
        Int4 = 68,

        UnsignedInt = 97,
        UnsignedInt2 = 98,
        UnsignedInt3 = 99,
        UnsignedInt4 = 100,

        Depth24Stencil8 = 129,
    }
}
