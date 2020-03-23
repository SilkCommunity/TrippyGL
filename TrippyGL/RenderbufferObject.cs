using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// A buffer optimized to be rendered to. The only way to use a <see cref="RenderbufferObject"/>
    /// is to attach it to a <see cref="FramebufferObject"/>.
    /// </summary>
    public sealed class RenderbufferObject : GraphicsResource
    {
        /// <summary>The handle for the GL Renderbuffer Object.</summary>
        public readonly uint Handle;

        /// <summary>The width of this <see cref="RenderbufferObject"/>.</summary>
        public readonly uint Width;

        /// <summary>The height of this <see cref="RenderbufferObject"/>.</summary>
        public readonly uint Height;

        /// <summary>The amount of samples this <see cref="RenderbufferObject"/> has.</summary>
        public readonly uint Samples;

        /// <summary>The format for this <see cref="RenderbufferObject"/>.</summary>
        public readonly RenderbufferFormat Format;

        /// <summary>Gets whether the format of this <see cref="RenderbufferObject"/> is depth-only.</summary>
        public bool IsDepthOnly => Format == RenderbufferFormat.Depth16 || Format == RenderbufferFormat.Depth24 || Format == RenderbufferFormat.Depth32f;

        /// <summary>Gets whether the format of this <see cref="RenderbufferObject"/> is stencil-only.</summary>
        public bool IsStencilOnly => Format == RenderbufferFormat.Stencil8;

        /// <summary>Gets whether the format of this <see cref="RenderbufferObject"/> is depth-stencil.</summary>
        public bool IsDepthStencil => Format == RenderbufferFormat.Depth24Stencil8 || Format == RenderbufferFormat.Depth32fStencil8;

        /// <summary>Gets whether the format of this <see cref="RenderbufferObject"/> is color-renderable.</summary>
        public bool IsColorRenderableFormat => !(IsDepthOnly || IsStencilOnly || IsDepthStencil);

        /// <summary>
        /// Creates a <see cref="RenderbufferObject"/> with the specified format.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="width">The width for the <see cref="RenderbufferObject"/>.</param>
        /// <param name="height">The height for the <see cref="RenderbufferObject"/>.</param>
        /// <param name="format">The format for the <see cref="RenderbufferObject"/>'s storage.</param>
        /// <param name="samples">The amount of samples the <see cref="RenderbufferObject"/> will have.</param>
        public RenderbufferObject(GraphicsDevice graphicsDevice, uint width, uint height, RenderbufferFormat format, uint samples = 0)
            : base(graphicsDevice)
        {
            if (!Enum.IsDefined(typeof(RenderbufferFormat), format))
                throw new ArgumentException("Invalid renderbuffer format", nameof(format));

            if (width <= 0 || width > graphicsDevice.MaxRenderbufferSize)
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be in the range (0, " + nameof(graphicsDevice.MaxRenderbufferSize) + "]");

            if (height <= 0 || height > graphicsDevice.MaxRenderbufferSize)
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be in the range (0, " + nameof(graphicsDevice.MaxRenderbufferSize) + "]");

            ValidateSampleCount(samples);

            Handle = GL.GenRenderbuffer();
            Format = format;
            Width = width;
            Height = height;
            Samples = samples;
            graphicsDevice.ForceBindRenderbuffer(this);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, Samples, (InternalFormat)format, Width, Height);
        }

        protected override void Dispose(bool isManualDispose)
        {
            GL.DeleteRenderbuffer(Handle);
            base.Dispose(isManualDispose);
        }

        internal void ValidateSampleCount(uint samples)
        {
            if (samples < 0 || samples > GraphicsDevice.MaxSamples)
                throw new ArgumentOutOfRangeException(nameof(samples), samples, "The sample count must be in the range [0, " + nameof(GraphicsDevice.MaxSamples) + "]");
        }
    }
}
