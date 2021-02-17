using System;

namespace TrippyGL
{
    /// <summary>
    /// Specifies the buffers that can be targeted by a clear operation.
    /// </summary>
    [Flags]
    public enum ClearBuffers : uint
    {
        Depth = 256,
        Stencil = 1024,
        Color = 16384,
    }
}
