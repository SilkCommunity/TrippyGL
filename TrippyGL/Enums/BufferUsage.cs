﻿namespace TrippyGL
{
    /// <summary>
    /// Specifies values that hint on how a <see cref="BufferObject"/> will be used.
    /// </summary>
    public enum BufferUsage
    {
        StreamDraw = 35040,
        StreamRead = 35041,
        StreamCopy = 35042,
        StaticDraw = 35044,
        StaticRead = 35045,
        StaticCopy = 35046,
        DynamicDraw = 35048,
        DynamicRead = 35049,
        DynamicCopy = 35050
    }
}
