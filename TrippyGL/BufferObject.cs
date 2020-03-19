using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Owns storage on the GPU's memory that can be used for various purposes though <see cref="BufferObjectSubset"/>-s.
    /// </summary>
    public sealed class BufferObject : GraphicsResource
    {
        /// <summary>The handle for the GL Buffer Object.</summary>
        public readonly uint Handle;

        /// <summary>The usage hint applied for this <see cref="BufferObject"/>.</summary>
        public BufferUsageARB UsageHint { get; private set; }

        /// <summary>The length of this <see cref="BufferObject"/>'s storage, measured in bytes.</summary>
        public uint StorageLengthInBytes { get; private set; }

        /// <summary>
        /// Creates a <see cref="BufferObject"/> with a specified length and usage hint.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="sizeInBytes">The desired size of the <see cref="BufferObject"/>'s storage measured in bytes.</param>
        /// <param name="usageHint">Used by the graphics driver to optimize performance.</param>
        public BufferObject(GraphicsDevice graphicsDevice, uint sizeInBytes, BufferUsageARB usageHint) : base(graphicsDevice)
        {
            if (usageHint == 0)
                throw new ArgumentException(nameof(usageHint) + " is not a valid " + nameof(BufferUsageARB) + " value");

            Handle = GL.GenBuffer();
            RecreateStorage(sizeInBytes, usageHint);
        }

        /// <summary>
        /// Recreate this <see cref="BufferObject"/>'s storage with a new size and usage hint.
        /// The contents of the new storage are undefined after this operation.
        /// </summary>
        /// <param name="sizeInBytes">The new size for the <see cref="BufferObject"/> measured in bytes.</param>
        /// <param name="usageHint">The new usage hint for the <see cref="BufferObject"/>, or 0 to keep the previous hint.</param>
        public unsafe void RecreateStorage(uint sizeInBytes, BufferUsageARB usageHint = 0)
        {
            // We check that the parameters are valid, then store them
            ValidateBufferSize(sizeInBytes);

            if (usageHint != 0)
            {
                ValidateBufferUsage(usageHint);
                UsageHint = usageHint;
            }

            StorageLengthInBytes = sizeInBytes;

            // We then bind this buffer and specify it's storage to OpenGL
            GraphicsDevice.BindBufferObject(this);
            GL.BufferData(GraphicsDevice.DefaultBufferTarget, sizeInBytes, (void*)0, UsageHint);
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
        /// Checks that the buffer size in bytes parameter is valid and throws an exception if it's not.
        /// </summary>
        private static void ValidateBufferSize(uint sizeInBytes)
        {
            if (sizeInBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(sizeInBytes), sizeInBytes, nameof(sizeInBytes) + " must be greater than 0");
        }

        /// <summary>
        /// Checks that the usage hint parameter is valid and throws an exception if it's not.
        /// </summary>
        private static void ValidateBufferUsage(BufferUsageARB usageHint)
        {
            if (!Enum.IsDefined(typeof(BufferUsageARB), usageHint))
                throw new ArgumentException(nameof(usageHint) + " is not a valid " + nameof(BufferUsageARB) + " value", nameof(usageHint));
        }
    }
}
