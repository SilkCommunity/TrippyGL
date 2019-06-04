using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrippyGL
{
    public abstract class GraphicsResource : IDisposable
    {
        public GraphicsDevice GraphicsDevice { get; internal set; }

        public bool IsDisposed { get; private set; }

        public GraphicsResource(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");

            this.GraphicsDevice = graphicsDevice;
        }

        ~GraphicsResource()
        {
            if (TrippyLib.isLibActive)
                Dispose(false);
        }

        protected virtual void Dispose(bool isManualDispose)
        {
            IsDisposed = true;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
