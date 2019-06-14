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

            this.GraphicsDevice = graphicsDevice;
        }

        ~GraphicsResource()
        {
            if (!GraphicsDevice.IsDisposed)
                Dispose(false);
        }

        protected virtual void Dispose(bool isManualDispose)
        {

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
                GraphicsDevice.OnResourceDisposed(this);
                GraphicsDevice = null;
                GC.SuppressFinalize(this);
            }
        }
    }
}
