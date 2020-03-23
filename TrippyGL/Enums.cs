using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Specifies formats a <see cref="Texture"/>'s image can have.
    /// </summary>
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

    /// <summary>
    /// Specifies depth and/or stencil formats a <see cref="FramebufferObject"/> can have.
    /// </summary>
    public enum DepthStencilFormat
    {
        None = 0,
        Depth24Stencil8 = InternalFormat.Depth24Stencil8,
        Depth32fStencil8 = InternalFormat.Depth32fStencil8,
        Depth16 = InternalFormat.DepthComponent16,
        Depth24 = InternalFormat.DepthComponent24Arb,
        Depth32f = InternalFormat.DepthComponent32f,
        Stencil8 = InternalFormat.StencilIndex8, //not a recommended format though, better to use Depth24Stencil8
    }

    /// <summary>
    /// Specifies image file formats.
    /// </summary>
    public enum SaveImageFormat
    {
        Png, Jpeg, Bmp, Gif
    }

    /// <summary>
    /// Specifies the attachment points on a <see cref="FramebufferObject"/>.
    /// </summary>
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
        Depth = GLEnum.DepthAttachment,
        Stencil = FramebufferAttachment.StencilAttachment,
        DepthStencil = GLEnum.DepthStencilAttachment,
    }

    /// <summary>
    /// Specifies formats a <see cref="RenderbufferObject"/>'s storage can have.
    /// </summary>
    public enum RenderbufferFormat
    {
        Color4b = InternalFormat.Rgba8,

        Float = InternalFormat.R32f,
        Float2 = InternalFormat.RG32f,
        Float4 = InternalFormat.Rgba32f,

        Int = InternalFormat.R32i,
        Int2 = InternalFormat.RG32i,
        Int4 = InternalFormat.Rgba32i,

        UnsignedInt = InternalFormat.R32ui,
        UnsignedInt2 = InternalFormat.RG32ui,
        UnsignedInt4 = InternalFormat.Rgba32ui,

        Depth16 = InternalFormat.DepthComponent16,
        Depth24 = InternalFormat.DepthComponent24Arb,
        Depth32f = InternalFormat.DepthComponent32f,
        Depth24Stencil8 = InternalFormat.Depth24Stencil8,
        Depth32fStencil8 = InternalFormat.Depth32fStencil8,
        Stencil8 = InternalFormat.StencilIndex8,
    }

    /// <summary>
    /// Specifies the faces of a <see cref="TextureCubemap"/>.
    /// </summary>
    public enum CubemapFace
    {
        PositiveX = TextureTarget.TextureCubeMapPositiveX,
        NegativeX = TextureTarget.TextureCubeMapNegativeX,
        PositiveY = TextureTarget.TextureCubeMapPositiveY,
        NegativeY = TextureTarget.TextureCubeMapNegativeY,
        PositiveZ = TextureTarget.TextureCubeMapPositiveZ,
        NegativeZ = TextureTarget.TextureCubeMapNegativeZ
    }

    /// <summary>
    /// Specifies formats for a read pixels operation on a <see cref="FramebufferObject"/>.
    /// </summary>
    public enum ReadPixelsFormat
    {
        // GL_RED, GL_GREEN, GL_BLUE, GL_RGB, GL_BGR, GL_RGBA, and GL_BGRA, GL_DEPTH_COMPONENT, GL_STENCIL_INDEX, GL_DEPTH_STENCIL
        Red = PixelFormat.Red,
        Green = PixelFormat.Green,
        Blue = PixelFormat.Blue,
        Rgb = PixelFormat.Rgb,
        Bgr = PixelFormat.Bgr,
        Rgba = PixelFormat.Rgba,
        Bgra = PixelFormat.Bgra,
        DepthComponent = PixelFormat.DepthComponent,
        StencilIndex = PixelFormat.StencilIndex,
        DepthStencil = PixelFormat.DepthStencil
    }
}
