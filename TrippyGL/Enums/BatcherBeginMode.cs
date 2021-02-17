namespace TrippyGL
{
    /// <summary>
    /// Specifies options on how a <see cref="TextureBatcher"/> handles drawing textures.
    /// </summary>
    public enum BatcherBeginMode
    {
        // IMPORTANT NOTE:
        // The values are set specifically so those that require batch items to be sorted have the
        // least significant bit set to 1. This way, in order to know whether sorting is needed we
        // can just do (beginMode & 1) == 1

        /// <summary>
        /// Textures are drawn when End() is called in order of draw call, batching together where possible.
        /// </summary>
        Deferred = 0,

        /// <summary>
        /// Textures are drawn in order of draw call but the batcher doesn't wait until End() to flush all the calls.<para/>
        /// If the same texture is drawn consecutively the Draw()-s will still be batched into a single draw call.
        /// </summary>
        OnTheFly = 2,

        /// <summary>
        /// Each texture is drawn in it's own individual draw call, immediately, during Draw().
        /// </summary>
        Immediate = 4,

        /// <summary>
        /// Textures are drawn when End() is called, but first sorted by texture. This uses the least amount of draw
        /// calls, but doesn't retain order (depth testing can be used for ordering).
        /// </summary>
        SortByTexture = 1,

        /// <summary>
        /// Textures are drawn when End() is called, but first sorted by depth in back-to-front order.
        /// This means items with higher depth get drawn before items with lower depth.<para/>
        /// Textures with the same depth aren't guaranteed to retain the order in which they were Draw()-n.
        /// </summary>
        SortBackToFront = 3,

        /// <summary>
        /// Textures are drawn when End() is called, but first sorted by depth in front-to-back order.
        /// This means items with lower depth get drawn before items with higher depth.<para/>
        /// Textures with the same depth aren't guaranteed to retain the order in which they were Draw()-n.
        /// </summary>
        SortFrontToBack = 5,
    }
}
