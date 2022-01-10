namespace TrippyGL
{
    /// <summary>
    /// Specifies values that hint on how a <see cref="BufferObject"/> will be used.
    /// </summary>
    /// <remarks>
    /// Explanations and descriptions for these can be found in the khronos OpenGL wiki:
    /// https://www.khronos.org/opengl/wiki/Buffer_Object#Buffer_Object_Usage
    /// </remarks>
    public enum BufferUsage
    {
        /// <summary>
        /// Optimizes the buffer for it's data to be set every use, or almost every use, and used for drawing.
        /// </summary>
        StreamDraw = 35040,

        /// <summary>
        /// Optimizes the buffer for it's data to be set every use, or almost every use, and used for reading back.
        /// </summary>
        StreamRead = 35041,

        /// <summary>
        /// Optimizes the buffer for it's data to be set every use, or almost every use, and used for copying within GPU memory.
        /// </summary>
        StreamCopy = 35042,

        /// <summary>
        /// Optimizes the buffer for it's data to be set once and used for drawing.
        /// </summary>
        StaticDraw = 35044,

        /// <summary>
        /// Optimizes the buffer for it's data to be set once and used for reading back.
        /// </summary>
        StaticRead = 35045,

        /// <summary>
        /// Optimizes the buffer for it's data to be set once and used for copying within GPU memory.
        /// </summary>
        StaticCopy = 35046,

        /// <summary>
        /// Optimizes the buffer for it's data to be set often and used for drawing.
        /// </summary>
        DynamicDraw = 35048,

        /// <summary>
        /// Optimizes the buffer for it's data to be set often and used for reading back.
        /// </summary>
        DynamicRead = 35049,

        /// <summary>
        /// Optimizes the buffer for it's data to be set often and used for copying within GPU memory.
        /// </summary>
        DynamicCopy = 35050
    }
}
