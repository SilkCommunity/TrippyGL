using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// A subset of a <see cref="BufferObject"/>'s storage that allows the same buffer to be used for various purposes.
    /// </summary>
    public abstract class BufferObjectSubset
    {
        /// <summary>The <see cref="BufferObject"/> this subset belongs to.</summary>
        public readonly BufferObject Buffer;

        /// <summary>The handle of the <see cref="BufferObject"/> that owns this subset.</summary>
        public readonly uint BufferHandle;

        /// <summary>The <see cref="BufferTargetARB"/> to which this subset always binds to.</summary>
        public readonly BufferTargetARB BufferTarget;

        /// <summary>The offset into this <see cref="Buffer"/>'s storage at which this subset starts, measured in bytes.</summary>
        public uint StorageOffsetInBytes { get; private set; }

        /// <summary>The length of this subset's storage measured in bytes.</summary>
        public uint StorageLengthInBytes { get; private set; }

        /// <summary>
        /// The index of the next byte in the <see cref="BufferObject"/>'s storage after this subset
        /// (a byte that does NOT belong to this subset but is sequentially next to this subset's end).<para/>
        /// </summary>
        /// <remarks>
        /// This is equal to (<see cref="StorageOffsetInBytes"/> + <see cref="StorageLengthInBytes"/>).
        /// </remarks>
        public uint StorageEndInBytes => StorageOffsetInBytes + StorageLengthInBytes;

        /// <summary>
        /// The index in the <see cref="GraphicsDevice.bufferBindings"/> array where this
        /// <see cref="BufferObjectSubset"/>'s <see cref="BufferTargetARB"/> last bound handle is stored.
        /// </summary>
        internal readonly int bufferTargetBindingIndex;

        /// <summary>
        /// Creates a <see cref="BufferObjectSubset"/> with the given <see cref="BufferObject"/> and
        /// target, but storage offset and length are left uninitialized.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        /// <param name="bufferTarget">The <see cref="BufferTargetARB"/> this subset will always bind to.</param>
        internal BufferObjectSubset(BufferObject bufferObject, BufferTargetARB bufferTarget)
        {
            if (bufferObject == null)
                throw new ArgumentNullException(nameof(bufferObject));

            if (!Enum.IsDefined(typeof(BufferTargetARB), bufferTarget))
                throw new ArgumentException(nameof(bufferTarget) + " must be a valid " + nameof(BufferTargetARB) + " value", nameof(bufferTarget));

            bufferTargetBindingIndex = bufferObject.GraphicsDevice.GetBindingTargetIndex(bufferTarget);
            Buffer = bufferObject;
            BufferHandle = Buffer.Handle;
            BufferTarget = bufferTarget;
        }

        /// <summary>
        /// Creates a <see cref="BufferObjectSubset"/> with the given <see cref="BufferObject"/> and
        /// target, offset into the buffer and storage length.
        /// </summary>
        /// <param name="bufferObject">The <see cref="BufferObject"/> this subset will belong to.</param>
        /// <param name="bufferTarget">The <see cref="BufferTargetARB"/> this subset will always bind to.</param>
        /// <param name="storageOffsetBytes">The offset into the <see cref="BufferObject"/>'s storage where this subset begins.</param>
        /// <param name="storageLengthBytes">The length of this subset measured in bytes.</param>
        internal BufferObjectSubset(BufferObject bufferObject, BufferTargetARB bufferTarget, uint storageOffsetBytes, uint storageLengthBytes)
            : this(bufferObject, bufferTarget)
        {
            InitializeStorage(storageOffsetBytes, storageLengthBytes);
        }

        /// <summary>
        /// Sets and checks the <see cref="StorageOffsetInBytes"/> and <see cref="StorageLengthInBytes"/> values.
        /// </summary>
        /// <param name="storageOffsetBytes">The desired offset into the <see cref="BufferObject"/>'s storage for this subset measured in bytes.</param>
        /// <param name="storageLengthBytes">The desired storage length for this subset measured in bytes.</param>
        private protected void InitializeStorage(uint storageOffsetBytes, uint storageLengthBytes)
        {
            if (storageOffsetBytes < 0 || storageOffsetBytes >= Buffer.StorageLengthInBytes)
                throw new ArgumentOutOfRangeException(nameof(storageOffsetBytes), storageOffsetBytes, nameof(storageOffsetBytes) + " must be in the range [0, " + nameof(Buffer.StorageLengthInBytes) + ")");

            if (storageLengthBytes + storageOffsetBytes > Buffer.StorageLengthInBytes)
                throw new ArgumentException("The given " + nameof(BufferObject) + " isn't big enough for the specified range");

            StorageOffsetInBytes = storageOffsetBytes;
            StorageLengthInBytes = storageLengthBytes;
        }

        public override string ToString()
        {
            return string.Concat(
                nameof(BufferHandle) + "=", BufferHandle.ToString(),
                ", " + nameof(BufferTarget) + "=", BufferTarget.ToString(),
                ", " + nameof(StorageOffsetInBytes) + "=", StorageOffsetInBytes.ToString(),
                ", " + nameof(StorageLengthInBytes) + "=", StorageLengthInBytes.ToString()
            );
        }
    }
}
