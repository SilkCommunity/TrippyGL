using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    /// <summary>
    /// Owns storage on the GPU's memory that can be used for various purposes though <see cref="BufferObjectSubset"/>-s.
    /// </summary>
    public sealed class BufferObject : GraphicsResource
    {
        /// <summary>The handle for the GL Buffer Object.</summary>
        public readonly int Handle;

        /// <summary>The usage hint for this <see cref="BufferObject"/>.</summary>
        public BufferUsageHint UsageHint { get; private set; }

        /// <summary>The length of this buffer object's storage, measured in bytes.</summary>
        public int StorageLengthInBytes { get; private set; }

        /// <summary>
        /// Creates a <see cref="BufferObject"/> with a specified length and usage hint.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="sizeInBytes">The desired size of the <see cref="BufferObject"/>'s storage measured in bytes.</param>
        /// <param name="usageHint">The usage hint is used by the graphics driver to optimize performance depending on the use that will be given to the buffer object.</param>
        public BufferObject(GraphicsDevice graphicsDevice, int sizeInBytes, BufferUsageHint usageHint) : base(graphicsDevice)
        {
            Handle = GL.GenBuffer();
            RecreateStorage(sizeInBytes, usageHint);
        }

        // TODO: If something like a transform feedback operation is active, you can't read/write/reallocate storage for ANY PART OF THE BUFFER
        // so we need to throw some fucking exceptions (buffer locking?)

        /// <summary>
        /// Recreate this <see cref="BufferObject"/>'s storage with a new size and usage hint.
        /// The contents of the new storage are undefined after this operation.
        /// </summary>
        /// <param name="sizeInBytes">The new size for the <see cref="BufferObject"/> measured in bytes.</param>
        /// <param name="usageHint">The new usage hint for the <see cref="BufferObject"/>.</param>
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
        /// Recreate this <see cref="BufferObject"/>'s storage with a new size but same usage hint as before.
        /// The contents of the new storage are undefined after this operation.
        /// </summary>
        /// <param name="sizeInBytes">The new size of the <see cref="BufferObject"/> measured in bytes.</param>
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
            return string.Concat(
                nameof(Handle) + "=", Handle.ToString(),
                ", " + nameof(StorageLengthInBytes) + "=", StorageLengthInBytes.ToString(),
                ", " + nameof(UsageHint) + "=", UsageHint.ToString()
            );
        }

        /// <summary>
        /// This should always be called just before a read operation from this <see cref="BufferObject"/>.
        /// If the operation can't occur for any reason, an exception is thrown.
        /// </summary>
        internal void ValidateReadOperation()
        {

        }

        /// <summary>
        /// This should always be called just before a write operation from this <see cref="BufferObject"/>.
        /// If the operation can't occur for any reason, an exception is thrown.
        /// </summary>
        internal void ValidateWriteOperation()
        {

        }

        /// <summary>
        /// Checks that the buffer size in bytes parameter is valid and throws an exception if it's not.
        /// </summary>
        private static void ValidateBufferSize(int sizeInBytes)
        {
            if (sizeInBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(sizeInBytes), sizeInBytes, nameof(sizeInBytes) + " must be greater than 0");
        }

        /// <summary>
        /// Checks that the usage hint parameter is valid and throws an exception if it's not.
        /// </summary>
        private static void ValidateBufferUsage(BufferUsageHint usageHint)
        {
            if (!Enum.IsDefined(typeof(BufferUsageHint), usageHint))
                throw new FormatException(nameof(usageHint) + " is not a valid " + nameof(BufferUsageHint) + " value");
        }
    }
}
