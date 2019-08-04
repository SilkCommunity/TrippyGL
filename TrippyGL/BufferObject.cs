using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// An buffer object owns storage on the GPU's memory that can be used for various purposes
    /// </summary>
    public class BufferObject : GraphicsResource
    {
        /// <summary>The GL Buffer Object's name</summary>
        public readonly int Handle;

        /// <summary>The usage hint for this BufferObject</summary>
        public BufferUsageHint UsageHint { get; private set; }

        /// <summary>The length of this buffer object's storage, measured in bytes</summary>
        public int StorageLengthInBytes { get; private set; }

        /// <summary>
        /// Creates a BufferObject with a specified length and usage hint
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="sizeInBytes">The desired size of the buffer object's storage measured in bytes</param>
        /// <param name="usageHint">The buffer hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object</param>
        public BufferObject(GraphicsDevice graphicsDevice, int sizeInBytes, BufferUsageHint usageHint) : base(graphicsDevice)
        {
            Handle = GL.GenBuffer();
            RecreateStorage(sizeInBytes, usageHint);
        }
        
        // TODO: If something like a transform feedback operation is active, you can't read/write/reallocate storage for ANY PART OF THE BUFFER
        // so we need to throw some fucking exceptions

        /// <summary>
        /// Recreate this buffer object's storage with a new size and usage hint. The contents of the new storage are undefined after the operation
        /// </summary>
        /// <param name="sizeInBytes">The new size of the buffer object measured in bytes</param>
        /// <param name="usageHint">The new usage hint for the buffer object</param>
        public void RecreateStorage(int sizeInBytes, BufferUsageHint usageHint)
        {
            // We check that the parameters are valid
            ValidateBufferSize(sizeInBytes);
            ValidateBufferUsage(usageHint);

            // We save these values on this object
            UsageHint = usageHint;
            StorageLengthInBytes = sizeInBytes;

            // We then bind this buffer and specify it's storage to OpenGL
            GraphicsDevice.BindBufferObject(this);
            GL.BufferData(GraphicsDevice.DefaultBufferTarget, sizeInBytes, IntPtr.Zero, usageHint);
        }

        /// <summary>
        /// Recreate this buffer object's storage with a new size and same usage hint. The contents of the new storage are undefined after the operation
        /// </summary>
        /// <param name="sizeInBytes">The new size of the buffer object measured in bytes</param>
        public void RecreateStorage(int sizeInBytes)
        {
            // We check that sizeInBytes is a valid value
            ValidateBufferSize(sizeInBytes);

            // And save it
            StorageLengthInBytes = sizeInBytes;

            // We then bind this buffer and specify it's storage to OpenGL
            GraphicsDevice.BindBufferObject(this);
            GL.BufferData(GraphicsDevice.DefaultBufferTarget, sizeInBytes, IntPtr.Zero, UsageHint);
        }

        protected override void Dispose(bool isManualDispose)
        {
            GL.DeleteBuffer(Handle);
            base.Dispose(isManualDispose);
        }

        public override string ToString()
        {
            return String.Concat("Handle=", Handle.ToString(), ", StorageLengthInBytes=", StorageLengthInBytes.ToString(), ", UsageHint=", UsageHint.ToString());
        }

        /// <summary>
        /// This should always be called just before a read operation from this BufferObject.
        /// If the operation can't occur for any reason, an exception is thrown
        /// </summary>
        internal void ValidateReadOperation()
        {

        }

        /// <summary>
        /// This should always be called just before a write operation from this BufferObject.
        /// If the operation can't occur for any reason, an exception is thrown
        /// </summary>
        internal void ValidateWriteOperation()
        {

        }

        private static void ValidateBufferSize(int sizeInBytes)
        {
            if (sizeInBytes <= 0)
                throw new ArgumentOutOfRangeException("sizeInBytes", sizeInBytes, "sizeInBytes must be greater than 0");
        }

        private static void ValidateBufferUsage(BufferUsageHint usageHint)
        {
            if (!Enum.IsDefined(typeof(BufferUsageHint), usageHint))
                throw new FormatException("usageHint is not a valid BufferUsageHint value");
        }
    }
}
