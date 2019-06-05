using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace TrippyGL
{
    /// <summary>
    /// Encapsulates an OpenGL texture object. This is the base class for all texture types and manages a lot of their internal workings
    /// </summary>
    public abstract class Texture : GraphicsResource
    {
        private protected const TextureMinFilter DefaultMinFilter = TextureMinFilter.Linear, DefaultMipmapMinFilter = TextureMinFilter.LinearMipmapLinear;
        private protected const TextureMagFilter DefaultMagFilter = TextureMagFilter.Linear;

        /// <summary>The GL Texture's name</summary>
        public readonly int Handle;

        /// <summary>The type of texture, such as 1D, 2D, Multisampled 2D, or CubeMap</summary>
        public readonly TextureTarget TextureType;

        /// <summary>The format of the pixels, such as RGBA, RGB, R32f, or even different depth/stencil formats</summary>
        public readonly PixelInternalFormat PixelFormat;

        /// <summary>The data type of the components of the texture's pixels, such as UnsignedByte (typical), Float, Int, HalfFloat, etc</summary>
        public readonly PixelType PixelType;

        /// <summary>Whether this texture is mipmapped</summary>
        public bool IsMipmapped { get; protected set; }

        internal int lastBindUnit;

        internal Texture(GraphicsDevice graphicsDevice, TextureTarget type, PixelInternalFormat pixelInternalFormat, PixelType pixelType) : base (graphicsDevice)
        {
            this.Handle = GL.GenTexture();
            this.TextureType = type;
            this.PixelFormat = pixelInternalFormat;
            this.PixelType = pixelType;
            this.lastBindUnit = 0;
            this.IsMipmapped = false;
        }

        /// <summary>
        /// Sets this texture's minifying and magnifying filters
        /// </summary>
        /// <param name="minFilter">The texture's minifying filter</param>
        /// <param name="magFilter">The texture's magnifying filter</param>
        public void SetTextureFilters(TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            GraphicsDevice.EnsureTextureBoundAndActive(this);
            GL.TexParameter(TextureType, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureType, TextureParameterName.TextureMagFilter, (int)magFilter);
        }

        //THIS FUNCTION SHOULD BE PASSED ON TO THE FUTURE TEXTURE3D CLASS
        /// <summary>
        /// Sets the texture coordinate wrapping modes for when a texture is sampled outside the [0, 1] range
        /// </summary>
        /// <param name="sWrapMode">The wrap mode for the S (or texture-X) coordinate</param>
        /// <param name="tWrapMode">The wrap mode for the T (or texture-Y) coordinate</param>
        /// <param name="rWrapMode">The wrap mode for the R (or texture-Z) coordinate</param>
        public void SetWrapModes(TextureWrapMode sWrapMode, TextureWrapMode tWrapMode, TextureWrapMode rWrapMode)
        {
            GraphicsDevice.EnsureTextureBoundAndActive(this);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapS, (int)sWrapMode);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapT, (int)tWrapMode);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapR, (int)rWrapMode);
        }

        protected override void Dispose(bool isManualDispose)
        {
            GL.DeleteTexture(this.Handle);
            base.Dispose(isManualDispose);
        }
    }
}
