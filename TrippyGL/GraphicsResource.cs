using System;

namespace TrippyGL
{
    /// <summary>
    /// Encapsulates any graphics resource. These include Textures, BufferObjects, VertexArrays, etc
    /// </summary>
    public abstract class GraphicsResource : IDisposable
    {
        /// <summary>The graphics device that manages this graphics resource</summary>
        public GraphicsDevice GraphicsDevice { get; internal set; }

        /// <summary>Whether this graphics resource has been disposed</summary>
        public bool IsDisposed { get; private set; }

        internal GraphicsResource(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;
            graphicsDevice.OnResourceAdded(this);
        }

        ~GraphicsResource()
        {
            if (GraphicsDevice != null && !GraphicsDevice.IsDisposed)
                Dispose(false);
        }

        protected virtual void Dispose(bool isManualDispose)
        {

        }

        /// <summary>
        /// Disposes the GraphicsResource without notifying the GraphicsDevice.
        /// This function is only called by the GraphicsDevice
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
        /// Disposes this graphics resource. It cannot be used after it's been disposed
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
