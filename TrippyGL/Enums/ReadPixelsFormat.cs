namespace TrippyGL
{
    /// <summary>
    /// Specifies formats for a read pixels operation on a <see cref="FramebufferObject"/>.
    /// </summary>
    public enum ReadPixelsFormat
    {
        // Allowed values in glReadPixels:
        // GL_RED, GL_GREEN, GL_BLUE, GL_RGB, GL_BGR, GL_RGBA, and GL_BGRA, GL_DEPTH_COMPONENT, GL_STENCIL_INDEX, GL_DEPTH_STENCIL
        // Source: https://www.khronos.org/registry/OpenGL-Refpages/gl4/html/glReadPixels.xhtml

        Red = 6403,
        Green = 6404,
        Blue = 6405,
        Rgb = 6407,
        Bgr = 32992,
        Rgba = 6408,
        Bgra = 32993,
        DepthComponent = 6402,
        StencilIndex = 6401,
        DepthStencil = 34041
    }
}
