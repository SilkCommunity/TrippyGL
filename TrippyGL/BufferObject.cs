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
        
        /// <summary>
        /// Recreate this buffer object's storage with a new size and usage hint. The contents of the new storage are undefined after the operation
        /// </summary>
        /// <param name="sizeInBytes">The new size of the buffer object measured in bytes</param>
        /// <param name="usageHint">The new usage hint for the buffer object</param>
        public void RecreateStorage(int sizeInBytes, BufferUsageHint usageHint)
        {
            ValidateBufferSize(sizeInBytes);
            ValidateBufferUsage(usageHint);

            UsageHint = usageHint;
            StorageLengthInBytes = sizeInBytes;
            GraphicsDevice.BindBufferObject(this);
            GL.BufferData(GraphicsDevice.DefaultBufferTarget, sizeInBytes, IntPtr.Zero, usageHint);
        }

        /// <summary>
        /// Recreate this buffer object's storage with a new size and same usage hint. The contents of the new storage are undefined after the operation
        /// </summary>
        /// <param name="sizeInBytes">The new size of the buffer object measured in bytes</param>
        public void RecreateStorage(int sizeInBytes)
        {
            ValidateBufferSize(sizeInBytes);

            StorageLengthInBytes = sizeInBytes;
            GraphicsDevice.BindBufferObject(this);
            GL.BufferData(GraphicsDevice.DefaultBufferTarget, sizeInBytes, IntPtr.Zero, UsageHint);
        }

        /// <summary>
        /// Disposes this buffer object, deleting and releasing the resources it uses.
        /// The buffer object cannot be used after it's been disposed
        /// </summary>
        protected override void Dispose(bool isManualDispose)
        {
            GL.DeleteBuffer(Handle);
            base.Dispose(isManualDispose);
        }

        public override string ToString()
        {
            return String.Concat("Handle=", Handle.ToString(), ", StorageLengthInBytes=", StorageLengthInBytes.ToString(), ", UsageHint=", UsageHint.ToString());
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
