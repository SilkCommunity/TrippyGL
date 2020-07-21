using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Encapsulates an OpenGL texture object. This is the base class for all
    /// texture types and manages some of their internal workings.
    /// </summary>
    public abstract class Texture : GraphicsResource
    {
        private protected const TextureMinFilter DefaultMinFilter = TextureMinFilter.Nearest, DefaultMipmapMinFilter = TextureMinFilter.NearestMipmapNearest;
        private protected const TextureMagFilter DefaultMagFilter = TextureMagFilter.Nearest;

        /// <summary>The handle for the GL Texture Object.</summary>
        public readonly uint Handle;

        /// <summary>The type of this <see cref="Texture"/>, such as 1D, 2D, Multisampled 2D, Array 2D, CubeMap, etc.</summary>
        public readonly TextureTarget TextureType;

        /// <summary>The internal format of the pixels, such as RGBA, RGB, R32f, or even different depth/stencil formats.</summary>
        internal readonly InternalFormat PixelInternalFormat;

        /// <summary>The data type of the components of the <see cref="Texture"/>'s pixels.</summary>
        internal readonly PixelType PixelType;

        /// <summary>The format of the pixel data.</summary>
        internal readonly PixelFormat PixelFormat;

        /// <summary>The format for this <see cref="Texture"/>'s image.</summary>
        public readonly TextureImageFormat ImageFormat;

        /// <summary>Gets whether this <see cref="Texture"/> is mipmapped.</summary>
        public bool IsMipmapped { get; private set; }

        /// <summary>False if this <see cref="Texture"/> can be mipmapped (depends on texture type).</summary>
        private readonly bool isNotMipmappable;

        /// <summary>Gets whether this <see cref="Texture"/> can be mipmapped (depends on texture type).</summary>
        public bool IsMipmappable => !isNotMipmappable;

        /// <summary>Gets whether this <see cref="Texture"/> is currently bound to a unit.</summary>
        public bool IsBound => GraphicsDevice.IsTextureBound(this);

        /// <summary>Gets whether this <see cref="Texture"/> is currently bound to the currently active texture unit.</summary>
        public bool IsBoundAndActive => GraphicsDevice.IsTextureBound(this) && lastBindUnit == GraphicsDevice.ActiveTextureUnit;

        /// <summary>Gets the texture unit to which this <see cref="Texture"/> is currently bound, or -1 if it's not bound anywhere.</summary>
        public int CurrentlyBoundUnit => IsBound ? lastBindUnit : -1;

        /// <summary>The last texture unit to which this <see cref="Texture"/> was bound. This value is used by binding functions.</summary>
        internal int lastBindUnit;

        /// <summary>
        /// Creates a <see cref="Texture"/> with specified <see cref="TextureTarget"/> and <see cref="TextureImageFormat"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="type">The type of texture (or texture target) the texture will be.</param>
        /// <param name="imageFormat">The type of image format this texture will store.</param>
        internal Texture(GraphicsDevice graphicsDevice, TextureTarget type, TextureImageFormat imageFormat) : base(graphicsDevice)
        {
            if (!Enum.IsDefined(typeof(TextureTarget), type))
                throw new FormatException("Invalid texture target");

            if (!Enum.IsDefined(typeof(TextureImageFormat), imageFormat))
                throw new FormatException("Invalid texture image format");

            TextureType = type;
            ImageFormat = imageFormat;
            TrippyUtils.GetTextureFormatEnums(imageFormat, out PixelInternalFormat, out PixelType, out PixelFormat);
            lastBindUnit = 0;
            IsMipmapped = false;
            isNotMipmappable = !TrippyUtils.IsTextureTypeMipmappable(type);
            Handle = GL.GenTexture();
        }

        /// <summary>
        /// Sets this <see cref="Texture"/>'s minifying and magnifying filters.
        /// </summary>
        /// <param name="minFilter">The desired minifying filter for the <see cref="Texture"/>.</param>
        /// <param name="magFilter">The desired magnifying filter for the <see cref="Texture"/>.</param>
        public void SetTextureFilters(TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            GraphicsDevice.BindTextureSetActive(this);
            GL.TexParameter(TextureType, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureType, TextureParameterName.TextureMagFilter, (int)magFilter);
        }

        /// <summary>
        /// Generates mipmaps for this <see cref="Texture"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public void GenerateMipmaps()
        {
            if (isNotMipmappable)
                throw new InvalidOperationException(string.Concat("This texture type is not mipmappable! Type: ", TextureType.ToString()));

            GraphicsDevice.BindTextureSetActive(this);
            GL.GenerateMipmap(TextureType);
            IsMipmapped = true;
        }

        protected override void Dispose(bool isManualDispose)
        {
            GL.DeleteTexture(Handle);
            base.Dispose(isManualDispose);
        }

        public override string ToString()
        {
            return string.Concat(
                nameof(Handle) + "=", Handle.ToString(),
                ", " + nameof(TextureType) + "=", TextureType.ToString(),
                ", " + nameof(ImageFormat) + "=", ImageFormat.ToString(),
                ", " + nameof(IsMipmapped) + "=", IsMipmapped.ToString(),
                ", " + nameof(IsBound) + "=", IsBound.ToString(),
                ", " + nameof(IsBoundAndActive) + "=", IsBoundAndActive.ToString()
            );
        }
    }
}
