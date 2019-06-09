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
        private protected const TextureMinFilter DefaultMinFilter = TextureMinFilter.Nearest, DefaultMipmapMinFilter = TextureMinFilter.NearestMipmapNearest;
        private protected const TextureMagFilter DefaultMagFilter = TextureMagFilter.Nearest;

        /// <summary>The GL Texture's name</summary>
        public readonly int Handle;

        /// <summary>The type of texture, such as 1D, 2D, Multisampled 2D, or CubeMap</summary>
        public readonly TextureTarget TextureType;

        /// <summary>The format of the pixels, such as RGBA, RGB, R32f, or even different depth/stencil formats (though these are unused)</summary>
        internal readonly PixelInternalFormat PixelFormat;

        /// <summary>The data type of the components of the texture's pixels, such as UnsignedByte (typical), Float, Int, HalfFloat, etc</summary>
        internal readonly PixelType PixelType;

        /// <summary>The format for this texture's image</summary>
        public readonly TextureImageFormat ImageFormat;

        /// <summary>Gets whether this texture is mipmapped</summary>
        public bool IsMipmapped { get; protected set; }

        /// <summary>Gets whether this texture is currently bound to a unit</summary>
        public bool IsBound { get { return GraphicsDevice.IsTextureBound(this); } }

        /// <summary>Gets whether this texture is currently bound to the currently active texture unit</summary>
        public bool IsBoundAndActive { get { return GraphicsDevice.IsTextureBound(this) && lastBindUnit == GraphicsDevice.ActiveTextureUnit; } }

        /// <summary>Gets the texture unit to which this texture is currently bound, or -1 if it's not bound anywhere</summary>
        public int GetCurrentlyBoundUnit { get { return IsBound ? lastBindUnit : -1; } }

        /// <summary>The last texture unit to which this texture was bound. This value is used by binding functions</summary>
        internal int lastBindUnit;

        internal Texture(GraphicsDevice graphicsDevice, TextureTarget type, TextureImageFormat imageFormat) : base (graphicsDevice)
        {
            this.Handle = GL.GenTexture();
            this.TextureType = type;
            this.ImageFormat = imageFormat;
            GetTextureFormatEnums(imageFormat, out PixelFormat, out PixelType);
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

        /// <summary>
        /// Turns a value from the TextureImageFormat enum into the necessary enums to create an OpenGL texture's image storage
        /// </summary>
        /// <param name="imageFormat">The requested image format</param>
        /// <param name="pixelInternalFormat">The pixel's internal format</param>
        /// <param name="pixelType">The pixel's type</param>
        public static void GetTextureFormatEnums(TextureImageFormat imageFormat, out PixelInternalFormat pixelInternalFormat, out PixelType pixelType)
        {
            // The workings of this function are related to the numbers assigned to each enum value
            int b = (int)imageFormat / 32;

            switch (b)
            {
                case 0: //this works because the only unsigned byte type is Color4b
                    #region UnsignedByteTypes
                    pixelType = PixelType.UnsignedByte;
                    pixelInternalFormat = PixelInternalFormat.Rgba8;
                    return;
                    #endregion
                case 1:
                    #region FloatTypes
                    pixelType = PixelType.Float;
                    switch ((int)imageFormat - b * 32)
                    {
                        case 1:
                            pixelInternalFormat = PixelInternalFormat.R32f;
                            return;
                        case 2:
                            pixelInternalFormat = PixelInternalFormat.Rg32f;
                            return;
                        case 3:
                            pixelInternalFormat = PixelInternalFormat.Rgb32f;
                            return;
                        case 4:
                            pixelInternalFormat = PixelInternalFormat.Rgba32f;
                            return;
                    }
                    break;
                    #endregion
                case 2:
                    #region IntTypes
                    pixelType = PixelType.Int;
                    switch ((int)imageFormat - b * 32)
                    {
                        case 1:
                            pixelInternalFormat = PixelInternalFormat.R32i;
                            return;
                        case 2:
                            pixelInternalFormat = PixelInternalFormat.Rg32i;
                            return;
                        case 3:
                            pixelInternalFormat = PixelInternalFormat.Rgb32i;
                            return;
                        case 4:
                            pixelInternalFormat = PixelInternalFormat.Rgba32i;
                            return;
                    }
                    break;
                    #endregion
                case 3:
                    #region UnsignedIntTypes
                    pixelType = PixelType.UnsignedInt;
                    switch ((int)imageFormat - b * 32)
                    {
                        case 1:
                            pixelInternalFormat = PixelInternalFormat.R32ui;
                            return;
                        case 2:
                            pixelInternalFormat = PixelInternalFormat.Rg32ui;
                            return;
                        case 3:
                            pixelInternalFormat = PixelInternalFormat.Rgb32ui;
                            return;
                        case 4:
                            pixelInternalFormat = PixelInternalFormat.Rgba32ui;
                            return;
                    }
                    break;
                    #endregion
            }
            throw new ArgumentException("Image format is not a valid TextureImageFormat value", "imageFormat");
        }

        protected override void Dispose(bool isManualDispose)
        {
            GL.DeleteTexture(this.Handle);
            base.Dispose(isManualDispose);
        }

        public override string ToString()
        {
            return String.Concat("Handle=", Handle, ", Type=", TextureType, ", ImageFormat=", ImageFormat, "Mipmapped=", IsMipmapped, ", Bound=", IsBound, "BoundAndActive=", IsBoundAndActive);
        }
    }
}
