using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// A buffer optimized to be rendered to. The only way to use a Renderbuffer is to attach it to a Framebuffer
    /// </summary>
    public class RenderbufferObject : GraphicsResource
    {
        /// <summary>The renderbuffer's handle</summary>
        public readonly int Handle;

        /// <summary>The width of the renderbuffer</summary>
        public readonly int Width;

        /// <summary>The height of the renderbuffer</summary>
        public readonly int Height;

        /// <summary>The amount of samples the renderbuffer has</summary>
        public readonly int Samples;

        /// <summary>The format for the renderbuffer</summary>
        public readonly RenderbufferFormat Format;

        /// <summary>Whether the format of this renderbuffer is depth-only</summary>
        public bool IsDepthOnly { get { return Format == RenderbufferFormat.Depth16 || Format == RenderbufferFormat.Depth24 || Format == RenderbufferFormat.Depth32f; } }

        /// <summary>Whether the format of this renderbuffer is stencil-only</summary>
        public bool IsStencilOnly { get { return Format == RenderbufferFormat.Stencil8; } }

        /// <summary>Whether the format of this renderbuffer is depth-stencil</summary>
        public bool IsDepthStencil { get { return Format == RenderbufferFormat.Depth24Stencil8 || Format == RenderbufferFormat.Depth32fStencil8; } }

        /// <summary>Whether the format of this renderbuffer is color-renderable</summary>
        public bool IsColorRenderableFormat { get { return !(IsDepthOnly || IsStencilOnly || IsDepthStencil); } }

        /// <summary>
        /// Creates a Renderbuffer with the specified format
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice this resource will use</param>
        /// <param name="width">The width of the renderbuffer</param>
        /// <param name="height">The height of the renderbuffer</param>
        /// <param name="format">The format for the renderbuffer's storage</param>
        /// <param name="samples">The amount of samples this renderbuffer will have</param>
        public RenderbufferObject(GraphicsDevice graphicsDevice, int width, int height, RenderbufferFormat format, int samples = 0) : base(graphicsDevice)
        {
            if (!Enum.IsDefined(typeof(RenderbufferFormat), format))
                throw new ArgumentException("Invalid renderbuffer format");

            if (width <= 0 || width > graphicsDevice.MaxRenderbufferSize)
                throw new ArgumentOutOfRangeException("width", width, "Width must be in the range (0, MAX_RENDERBUFFER_SIZE]");

            if (height <= 0 || height > graphicsDevice.MaxRenderbufferSize)
                throw new ArgumentOutOfRangeException("height", height, "Height must be in the range (0, MAX_RENDERBUFFER_SIZE]");

            ValidateSampleCount(samples);

            Handle = GL.GenRenderbuffer();
            Format = format;
            Width = width;
            Height = height;
            Samples = samples;
            graphicsDevice.ForceBindRenderbuffer(this);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, Samples, (RenderbufferStorage)format, Width, Height);
        }

        protected override void Dispose(bool isManualDispose)
        {
            GL.DeleteRenderbuffer(Handle);
            base.Dispose(isManualDispose);
        }

        internal void ValidateSampleCount(int samples)
        {
            if (samples < 0 || samples > GraphicsDevice.MaxSamples)
                throw new ArgumentOutOfRangeException("samples", samples, "The sample count must be in the range [0, MAX_SAMPLES]");
        }
    }
}
