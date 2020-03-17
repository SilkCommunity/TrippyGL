using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    // TODO: Add documentation & descriptions to the enums (and the values in the enums?)

    public enum TextureImageFormat
    {
        // These are organized in such away so the base type (float, int, uint)
        // is differentiable by dividing by 32 and the remainder indicates the amount of components
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

    public enum DepthStencilFormat
    {
        None = 0,
        Depth24Stencil8 = RenderbufferStorage.Depth24Stencil8,
        Depth32fStencil8 = RenderbufferStorage.Depth32fStencil8,
        Depth16 = RenderbufferStorage.DepthComponent16,
        Depth24 = RenderbufferStorage.DepthComponent24,
        Depth32f = RenderbufferStorage.DepthComponent32f,
        Stencil8 = RenderbufferStorage.StencilIndex8, //not a recommended format though, better to use Depth24Stencil8
    }

    public enum SaveImageFormat
    {
        Png, Jpeg, Bmp
    }

    public enum FramebufferAttachmentPoint
    {
        Color0 = FramebufferAttachment.ColorAttachment0,
        Color1 = FramebufferAttachment.ColorAttachment1,
        Color2 = FramebufferAttachment.ColorAttachment2,
        Color3 = FramebufferAttachment.ColorAttachment3,
        Color4 = FramebufferAttachment.ColorAttachment4,
        Color5 = FramebufferAttachment.ColorAttachment5,
        Color6 = FramebufferAttachment.ColorAttachment6,
        Color7 = FramebufferAttachment.ColorAttachment7,
        Color8 = FramebufferAttachment.ColorAttachment8,
        Color9 = FramebufferAttachment.ColorAttachment9,
        Color10 = FramebufferAttachment.ColorAttachment10,
        Color11 = FramebufferAttachment.ColorAttachment11,
        Color12 = FramebufferAttachment.ColorAttachment12,
        Color13 = FramebufferAttachment.ColorAttachment13,
        Color14 = FramebufferAttachment.ColorAttachment14,
        Color15 = FramebufferAttachment.ColorAttachment15,
        Depth = FramebufferAttachment.DepthAttachment,
        Stencil = FramebufferAttachment.StencilAttachment,
        DepthStencil = FramebufferAttachment.DepthStencilAttachment,
    }

    public enum RenderbufferFormat
    {
        Color4b = RenderbufferStorage.Rgba8,

        Float = RenderbufferStorage.R32f,
        Float2 = RenderbufferStorage.Rg32f,
        Float4 = RenderbufferStorage.Rgba32f,

        Int = RenderbufferStorage.R32i,
        Int2 = RenderbufferStorage.Rg32i,
        Int4 = RenderbufferStorage.Rgba32i,

        UnsignedInt = RenderbufferStorage.R32ui,
        UnsignedInt2 = RenderbufferStorage.Rg32ui,
        UnsignedInt4 = RenderbufferStorage.Rgba32ui,

        Depth16 = RenderbufferStorage.DepthComponent16,
        Depth24 = RenderbufferStorage.DepthComponent24,
        Depth32f = RenderbufferStorage.DepthComponent32f,
        Depth24Stencil8 = RenderbufferStorage.Depth24Stencil8,
        Depth32fStencil8 = RenderbufferStorage.Depth32fStencil8,
        Stencil8 = RenderbufferStorage.StencilIndex8,
    }

    public enum CubeMapFace
    {
        PositiveX = TextureTarget.TextureCubeMapPositiveX,
        NegativeX = TextureTarget.TextureCubeMapNegativeX,
        PositiveY = TextureTarget.TextureCubeMapPositiveY,
        NegativeY = TextureTarget.TextureCubeMapNegativeY,
        PositiveZ = TextureTarget.TextureCubeMapPositiveZ,
        NegativeZ = TextureTarget.TextureCubeMapNegativeZ
    }
}
