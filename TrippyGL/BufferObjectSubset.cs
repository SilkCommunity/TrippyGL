using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// A subset of a buffer object's storage that allows the same buffer to be used for various purposes
    /// </summary>
    public abstract class BufferObjectSubset
    {
        /// <summary>The BufferObject this subset belongs to</summary>
        public readonly BufferObject Buffer;

        /// <summary>The handle of the BufferObject that owns this subset</summary>
        public readonly int BufferHandle;

        /// <summary>The BufferTarget to which this subset always binds to</summary>
        public readonly BufferTarget BufferTarget;

        /// <summary>The offset into the buffer's storage at which this subset starts, measured in bytes</summary>
        public int StorageOffsetInBytes { get; private set; }

        /// <summary>The length of this subset's storage measured in bytes</summary>
        public int StorageLengthInBytes { get; private set; }

        /// <summary>
        /// The index of the next byte in the BufferObject's storage after this subset
        /// (a byte that does NOT belong to this subset but is sequentially next to this subset's end).
        /// This is calculated as (StorageOffsetInBytes + StorageLengthInBytes)
        /// </summary>
        public int StorageNextInBytes { get { return StorageOffsetInBytes + StorageLengthInBytes; } }

        /// <summary>The index on the GraphicsDevice.bufferBindings array where this BufferObjectSubset's BufferTarget last bound handle is stored</summary>
        internal readonly int bufferTargetBindingIndex;

        /// <summary>
        /// Creates a BufferObjectSubset with the given BufferObject and target, offset into the buffer and storage length
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        /// <param name="bufferTarget">The BufferTarget this subset will always bind to</param>
        /// <param name="storageOffsetBytes">The offset into the buffer's storage where this subset begins</param>
        /// <param name="storageLengthBytes">The length of this subset measured in bytes</param>
        internal BufferObjectSubset(BufferObject bufferObject, BufferTarget bufferTarget, int storageOffsetBytes, int storageLengthBytes) : this(bufferObject, bufferTarget)
        {
            InitializeStorage(storageOffsetBytes, storageLengthBytes);
        }

        /// <summary>
        /// Creates a BufferObjectSubset with the given BufferObject and target, but storage offset and length are left uninitialized
        /// </summary>
        /// <param name="bufferObject">The BufferObject this subset will belong to</param>
        /// <param name="bufferTarget">The BufferTarget this subset will always bind to</param>
        internal BufferObjectSubset(BufferObject bufferObject, BufferTarget bufferTarget)
        {
            bufferTargetBindingIndex = bufferObject.GraphicsDevice.GetBindingTargetIndex(bufferTarget);
            Buffer = bufferObject;
            BufferHandle = Buffer.Handle;
            BufferTarget = bufferTarget;
        }

        /// <summary>
        /// Creates a BufferObjectSubset that occupies the same area in the same buffer as another buffer subset but has another BufferTarget
        /// </summary>
        /// <param name="copy">The BufferObjectSubset to copy</param>
        /// <param name="bufferTarget">The BufferTarget this subset will always bind to</param>
        internal BufferObjectSubset(BufferObjectSubset copy, BufferTarget bufferTarget) : this(copy.Buffer, bufferTarget, copy.StorageOffsetInBytes, copy.StorageLengthInBytes)
        {

        }

        /// <summary>
        /// Sets and checks the StorageOffsetInBytes and StorageLengthInBytes values
        /// </summary>
        /// <param name="storageOffsetBytes">The desired value for StorageOffsetInBytes</param>
        /// <param name="storageLengthBytes">The desired value for StorageLengthInBytes</param>
        private protected void InitializeStorage(int storageOffsetBytes, int storageLengthBytes)
        {
            if (storageOffsetBytes < 0 || storageOffsetBytes >= Buffer.StorageLengthInBytes)
                throw new ArgumentOutOfRangeException("storageOffsetBytes", storageOffsetBytes, "Storage offset must be in the range [0, bufferObject.StorageLengthInBytes)");

            if (storageLengthBytes + storageOffsetBytes > Buffer.StorageLengthInBytes)
                throw new ArgumentException("The given BufferObject isn't big enough for the specified range");

            StorageOffsetInBytes = storageOffsetBytes;
            StorageLengthInBytes = storageLengthBytes;
        }

        public override string ToString()
        {
            return String.Concat("BufferHandle=", BufferHandle.ToString(), ", BufferTarget=", BufferTarget.ToString(), ", StorageOffsetInBytes=", StorageOffsetInBytes.ToString(), ", StorageLengthInBytes=", StorageLengthInBytes.ToString());
        }
    }
}
