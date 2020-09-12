#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
#pragma warning disable CA1063 // Implement IDisposable Correctly

using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Base class for any graphics resource.
    /// These include <see cref="Texture"/>, <see cref="BufferObject"/>, <see cref="VertexArray"/>, etc.
    /// </summary>
    public abstract class GraphicsResource : IDisposable
    {
        /// <summary>The <see cref="TrippyGL.GraphicsDevice"/> that manages this <see cref="GraphicsResource"/>.</summary>
        public GraphicsDevice GraphicsDevice { get; internal set; }

        /// <summary>Gets this <see cref="GraphicsResource"/>'s GL object.</summary>
        internal GL GL => GraphicsDevice.GL;

        /// <summary>Whether this <see cref="GraphicsResource"/> has been disposed.</summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a <see cref="GraphicsResource"/> that uses the specified <see cref="TrippyGL.GraphicsDevice"/>.
        /// </summary>
        internal GraphicsResource(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            graphicsDevice.OnResourceAdded(this);
        }

        ~GraphicsResource()
        {
            if (GraphicsDevice != null && !GraphicsDevice.IsDisposed)
                Dispose(false);
        }

        /// <summary>
        /// Disposes this <see cref="GraphicsResource"/>, deleting and releasing the resources it uses.
        /// Resources override this method to implement their disposing code.
        /// </summary>
        /// <param name="isManualDispose">Whether the call to this function happened because of a call to <see cref="Dispose()"/> or by the destructor.</param>
        protected abstract void Dispose(bool isManualDispose);

        /// <summary>
        /// Disposes this <see cref="GraphicsResource"/> without notifying <see cref="GraphicsDevice"/>.
        /// This function is only called by <see cref="GraphicsDevice"/>.
        /// </summary>
        internal void DisposeByGraphicsDevice()
        {
            if (!IsDisposed)
            {
                Dispose(true);
                IsDisposed = true;
                GC.SuppressFinalize(this);
                GraphicsDevice = null;
            }
        }

        /// <summary>
        /// Disposes this <see cref="GraphicsResource"/>. It cannot be used after it's been disposed.
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                Dispose(true);
                IsDisposed = true;
                GC.SuppressFinalize(this);
                GraphicsDevice.OnResourceRemoved(this);
                GraphicsDevice = null;
            }
        }
    }
}
