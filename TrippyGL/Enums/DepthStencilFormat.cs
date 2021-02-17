namespace TrippyGL
{
    /// <summary>
    /// Specifies depth and/or stencil formats a <see cref="FramebufferObject"/> can have.
    /// </summary>
    public enum DepthStencilFormat
    {
        None = 0,
        Depth24Stencil8 = 35056,
        Depth32fStencil8 = 36013,
        Depth16 = 33189,
        Depth24 = 33190,
        Depth32f = 36012,
        Stencil8 = 36168 //not a recommended format though, better to use Depth24Stencil8
    }
}
