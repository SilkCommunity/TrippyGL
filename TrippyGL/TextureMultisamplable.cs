using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// An abstract <see cref="Texture"/> that supports multisampling.
    /// </summary>
    public class TextureMultisamplable : Texture
    {
        /// <summary>The amount of samples this <see cref="Texture"/> has.</summary>
        public uint Samples { get; private set; }

        internal TextureMultisamplable(GraphicsDevice graphicsDevice, TextureTarget type, uint samples, TextureImageFormat imageFormat)
            : base(graphicsDevice, type, imageFormat)
        {
            ValidateSampleCount(samples);
            Samples = samples;
        }

        private void ValidateSampleCount(uint samples)
        {
            if (samples < 0 || samples > GraphicsDevice.MaxSamples)
                throw new ArgumentOutOfRangeException(nameof(samples), samples, nameof(samples) + " must be in the range [0, " + nameof(GraphicsDevice.MaxSamples) + "]");
        }

        protected void ValidateNotMultisampledPixelAccess()
        {
            if (Samples != 0)
                throw new InvalidOperationException("You can't read/write the pixels of a multisampled texture");
        }

        protected void ValidateNotMultisampledWrapStates()
        {
            if (Samples != 0)
                throw new InvalidOperationException("You can't change a multisampled texture's sampler states");
        }
    }
}
