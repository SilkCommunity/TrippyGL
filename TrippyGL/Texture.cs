using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public abstract class Texture : IDisposable
    {
        private static int[] binds;
        private static int activeTextureUnit;
        internal static int maxTextureSize;
        internal static void Init()
        {
            binds = new int[TrippyLib.MaxTextureImageUnits];
            for (int i = 0; i < binds.Length; i++)
                binds[i] = -1;
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
                binds[i] = -1;
        }

        /// <summary>
        /// Ensures all the given textures are bound to a TextureUnit.
        /// Returns an array indicating the TextureUnit to which the texture of the same index is bound to
        /// </summary>
        /// <param name="textures">The textures to ensure are all bound</param>
        public static TextureUnit[] EnsureAllBound(Texture[] textures)
        {
            TextureUnit[] units = new TextureUnit[textures.Length];
            if (textures.Length > binds.Length)
            {
                for (int i = 0; i < textures.Length; i++)
                {
                    if (binds[i] != textures[i].Handle)
                    {
                        GL.ActiveTexture(TextureUnit.Texture0 + i);
                        textures[i].BindToCurrentTextureUnit();
                    }
                    units[i] = TextureUnit.Texture0 + i;
                }
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

        /// <summary>The last texture unit to which this texture got bound. Used to check if it's still bound there so no unnecesary glBindTextures are called</summary>
        private int lastBindUnit;

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
            dispose();
        }

        /// <summary>
        /// Ensures both that the texture is bound and that the TextureUnit it is bound to is the currently the active one.
        /// Returns the TextureUnit this texture is now bound to. Calling another texture's ensure method might mean this texture is no longer bound nor currently active.
        /// </summary>
        public TextureUnit EnsureBoundAndActive()
        {
            if (binds[lastBindUnit] == Handle)
            {
                if (activeTextureUnit != lastBindUnit)
                    GL.ActiveTexture(TextureUnit.Texture0 + lastBindUnit);
            }
            else
                BindToCurrentTextureUnit();
            return TextureUnit.Texture0 + lastBindUnit;
        }

        /// <summary>
        /// Ensures that this texture is bound to a TextureUnit, but doesn't ensure that TextureUnit is the currently active one.
        /// Returns the TextureUnit this texture is bound to
        /// </summary>
        public TextureUnit EnsureBound()
        {
            if (binds[lastBindUnit] != Handle)
                BindToCurrentTextureUnit();
            return TextureUnit.Texture0 + lastBindUnit;
        }

        /// <summary>
        /// Binds the texture to the currently active texture unit
        /// </summary>
        protected void BindToCurrentTextureUnit()
        {
            lastBindUnit = activeTextureUnit;
            binds[lastBindUnit] = Handle;
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


        public void SetWrapMode(TextureWrapMode sWrapMode)
        {
            EnsureBoundAndActive();
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapS, (int)sWrapMode);
        }

        public void SetWrapModes(TextureWrapMode sWrapMode, TextureWrapMode tWrapMode)
        {
            EnsureBoundAndActive();
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapS, (int)sWrapMode);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapT, (int)tWrapMode);
        }

        public void SetWrapModes(TextureWrapMode sWrapMode, TextureWrapMode tWrapMode, TextureWrapMode rWrapMode)
        {
            EnsureBoundAndActive();
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapS, (int)sWrapMode);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapT, (int)tWrapMode);
            GL.TexParameter(TextureType, TextureParameterName.TextureWrapR, (int)rWrapMode);
        }

        private void dispose()
        {
            GL.DeleteTexture(Handle);
        }

        public void Dispose()
        {
            dispose();
            GC.SuppressFinalize(this);
        }
    }
}
