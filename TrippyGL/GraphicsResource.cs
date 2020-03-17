using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Encapsulates any graphics resource.
    /// These include <see cref="Texture"/>, <see cref="BufferObject"/>, <see cref="VertexArray"/>, etc.
    /// </summary>
    public abstract class GraphicsResource : IDisposable
    {
        /// <summary>The <see cref="GraphicsDevice"/> that manages this <see cref="GraphicsResource"/>.</summary>
        public GraphicsDevice GraphicsDevice { get; internal set; }

        internal GL GL => GraphicsDevice.GL;

        /// <summary>Whether this <see cref="GraphicsResource"/> has been disposed.</summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a <see cref="GraphicsResource"/>-s that uses the specified <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="TrippyGL.GraphicsDevice"/> for this <see cref="GraphicsResource"/>.</param>
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
        protected virtual void Dispose(bool isManualDispose)
        {

        }

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
