using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace TrippyGL
{
    public abstract class Texture : IDisposable
    {
        private static Texture[] binds;
        private static int activeTextureUnit;
        internal static int maxTextureSize;
        internal static void Init()
        {
            binds = new Texture[TrippyLib.MaxTextureImageUnits];
            for (int i = 0; i < binds.Length; i++)
                binds[i] = null;
            activeTextureUnit = 0;
            GL.ActiveTexture(TextureUnit.Texture0);
            maxTextureSize = TrippyLib.MaxTextureSize;
        }

        /// <summary>
        /// Resets all texture bind variables. These are used by binding functions to not call glBindTexture if the texture is already bound and other operations.
        /// You might want to reset this is interoperating with another library
        /// </summary>
        public static void ResetBindStates()
        {
            activeTextureUnit = GL.GetInteger(GetPName.ActiveTexture) - (int)TextureUnit.Texture0;
            for (int i = 0; i < binds.Length; i++)
                binds[i] = null;
        }

        /// <summary>
        /// Ensures all the given textures are bound to a TextureUnit.
        /// Returns an array indicating the TextureUnit to which the texture of the same index is bound to
        /// </summary>
        /// <param name="textures">The textures to ensure are all bound</param>
        public static TextureUnit[] EnsureAllBound(Texture[] textures)
        {
            if (textures.Length > binds.Length)
                throw new NotSupportedException("You tried to bind more textures at the same time than this system supports");

            TextureUnit[] units = new TextureUnit[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                if (binds[i] != textures[i])
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + i);
                    activeTextureUnit = i;
                    textures[i].BindToCurrentTextureUnit();
                }
                units[i] = TextureUnit.Texture0 + i;
            }

            return units;
        }

        /// <summary>
        /// Ensures all the given textures are bound to a TextureUnit.
        /// Returns an array indicating the TextureUnit to which the texture of the same index is bound to
        /// </summary>
        /// <param name="textures">The textures to ensure are all bound</param>
        public static TextureUnit[] EnsureAllBound(List<Texture> textures)
        {
            if (textures.Count > binds.Length)
                throw new NotSupportedException("You tried to bind more textures at the same time than this system supports");

            TextureUnit[] units = new TextureUnit[textures.Count];
            for (int i = 0; i < textures.Count; i++)
            {
                if (binds[i] != textures[i])
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + i);
                    activeTextureUnit = i;
                    textures[i].BindToCurrentTextureUnit();
                }
                units[i] = TextureUnit.Texture0 + i;
            }

            return units;
        }

        /// <summary>The GL Texture's name</summary>
        public readonly int Handle;

        /// <summary>The type of texture, such as 1D, 2D, Multisampled 2D, or CubeMap</summary>
        public readonly TextureTarget TextureType;

        /// <summary>The format of the pixels, such as RGBA, RGB, R32f, or even different depth/stencil formats</summary>
        public readonly PixelInternalFormat PixelFormat;

        /// <summary>The data type of the components of the texture's pixels, such as UnsignedByte (typical), Float, Int, HalfFloat, etc</summary>
        public readonly PixelType PixelType;

        internal int LastBindUnit { get; private set; }

        internal Texture(TextureTarget type, PixelInternalFormat pixelInternalFormat, PixelType pixelType)
        {
            this.Handle = GL.GenTexture();
            this.TextureType = type;
            this.PixelFormat = pixelInternalFormat;
            this.PixelType = pixelType;
            BindToCurrentTextureUnit();
        }

        ~Texture()
        {
            if (TrippyLib.isLibActive)
                dispose();
        }

        /// <summary>
        /// Ensures both that the texture is bound and that the TextureUnit it is bound to is the currently the active one.
        /// Returns the TextureUnit this texture is now bound to. Calling another texture's ensure method might mean this texture is no longer bound nor currently active.
        /// </summary>
        public TextureUnit EnsureBoundAndActive()
        {
            if (binds[LastBindUnit] == this)
            {
                if (activeTextureUnit != LastBindUnit)
                    GL.ActiveTexture(TextureUnit.Texture0 + LastBindUnit);
            }
            else
                BindToCurrentTextureUnit();
            return TextureUnit.Texture0 + LastBindUnit;
        }

        /// <summary>
        /// Ensures that this texture is bound to a TextureUnit, but doesn't ensure that TextureUnit is the currently active one.
        /// Returns the TextureUnit this texture is bound to
        /// </summary>
        public TextureUnit EnsureBound()
        {
            if (binds[LastBindUnit] != this)
                BindToCurrentTextureUnit();
            return TextureUnit.Texture0 + LastBindUnit;
        }

        /// <summary>
        /// Binds the texture to the currently active texture unit.
        /// Prefer using EnsureBound() or EnsureBoundAndActive()
        /// </summary>
        public void BindToCurrentTextureUnit()
        {
            LastBindUnit = activeTextureUnit;
            binds[LastBindUnit] = this;
            GL.BindTexture(TextureType, Handle);
        }

        /// <summary>
        /// Sets this texture's minifying and magnifying filters
        /// </summary>
        /// <param name="minFilter">The texture's minifying filter</param>
        /// <param name="magFilter">The texture's magnifying filter</param>
        public void SetTextureFilters(TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            EnsureBoundAndActive();
            GL.TexParameter(TextureType, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureType, TextureParameterName.TextureMagFilter, (int)magFilter);
        }

        /// <summary>
        /// Sets the texture coordinate wrapping modes for when a texture is sampled outside the [0, 1] range
        /// </summary>
        /// <param name="sWrapMode">The wrap mode for the S (or texture-X) coordinate</param>
        public void SetWrapMode(TextureWrapMode sWrapMode)
        {
            EnsureBoundAndActive();
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapS, (int)sWrapMode);
        }

        /// <summary>
        /// Sets the texture coordinate wrapping modes for when a texture is sampled outside the [0, 1] range
        /// </summary>
        /// <param name="sWrapMode">The wrap mode for the S (or texture-X) coordinate</param>
        /// <param name="tWrapMode">The wrap mode for the T (or texture-Y) coordinate</param>
        public void SetWrapModes(TextureWrapMode sWrapMode, TextureWrapMode tWrapMode)
        {
            EnsureBoundAndActive();
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapS, (int)sWrapMode);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapT, (int)tWrapMode);
        }

        /// <summary>
        /// Sets the texture coordinate wrapping modes for when a texture is sampled outside the [0, 1] range
        /// </summary>
        /// <param name="sWrapMode">The wrap mode for the S (or texture-X) coordinate</param>
        /// <param name="tWrapMode">The wrap mode for the T (or texture-Y) coordinate</param>
        /// <param name="rWrapMode">The wrap mode for the R (or texture-Z) coordinate</param>
        public void SetWrapModes(TextureWrapMode sWrapMode, TextureWrapMode tWrapMode, TextureWrapMode rWrapMode)
        {
            EnsureBoundAndActive();
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapS, (int)sWrapMode);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapT, (int)tWrapMode);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapR, (int)rWrapMode);
        }

        /// <summary>
        /// This method disposes the texture with no checks at all
        /// </summary>
        private void dispose()
        {
            GL.DeleteTexture(Handle);
        }

        /// <summary>
        /// Disposes the texture, releasing the resources it uses.
        /// The texture can't be used after being disposed
        /// </summary>
        public void Dispose()
        {
            dispose();
            GC.SuppressFinalize(this);
        }
    }
}
