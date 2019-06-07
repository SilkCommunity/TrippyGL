using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public enum TextureImageFormat
    {
        // These are organized in such away so the base type (float, int, uint)
        // is differentiable by dividing by 32 and the remainder indicates the amount of components
        // (amount of components: Color4b has 4, Vector2 has 2, Vector3i has 3, etc)
        // This is done in Texture.GetTextureFormatEnums()

        Color4b = 5,

        Float = 33,
        Vector2 = 34,
        Vector3 = 35,
        Vector4 = 36,

        Int = 65,
        Vector2i = 66,
        Vector3i = 67,
        Vector4i = 68,

        UnsignedInt = 97,
        UVector2i = 98,
        UVector3i = 99,
        UVector4i = 100,
    }

    public enum DepthStencilFormat
    {
        Depth24Stencil8 = RenderbufferStorage.Depth24Stencil8,
        Depth32fStencil8 = RenderbufferStorage.Depth32fStencil8,
        Depth16 = RenderbufferStorage.DepthComponent16,
        Depth24 = RenderbufferStorage.DepthComponent24,
        Depth32 = RenderbufferStorage.DepthComponent32,
        Depth32f = RenderbufferStorage.DepthComponent32f,
        Stencil8 = RenderbufferStorage.StencilIndex8,
        Stencil16 = RenderbufferStorage.StencilIndex16
    }

    public enum SaveImageFormat
    {
        Png, Jpeg, Tiff, Bmp
    }
}
