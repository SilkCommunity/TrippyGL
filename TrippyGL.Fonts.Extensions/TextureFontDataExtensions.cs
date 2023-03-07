namespace TrippyGL.Fonts.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="TextureFontData"/>.
    /// </summary>
    public static class TextureFontDataExtensions
    {
        /// <summary>
        /// Creates a <see cref="TextureFont"/> from the <see cref="TextureFontData"/> and a texture.
        /// </summary>
        public static TextureFont CreateFont(this TextureFontData fontData, Texture2D texture)
        {
            bool isMonospace = true;
            for (int i = 1; i < fontData.Advances!.Length && isMonospace; i++)
                isMonospace = fontData.Advances[i] != fontData.Advances[0];

            if (fontData.KerningOffsets != null)
            {
                float[] advances;

                if (isMonospace && fontData.Advances.Length != fontData.CharCount)
                {
                    advances = new float[fontData.CharCount];
                    for (int i = 0; i < advances.Length; i++)
                        advances[i] = fontData.Advances[0];
                }
                else
                    advances = fontData.Advances;

                return new KerningTextureFont(texture, fontData.Size, fontData.FirstChar, fontData.LastChar,
                    fontData.RenderOffsets!, fontData.SourceRectangles!, fontData.KerningOffsets, advances,
                    fontData.Ascender, fontData.Descender, fontData.LineGap, fontData.Name!);
            }

            if (isMonospace)
            {
                return new MonospaceTextureFont(texture, fontData.Size, fontData.FirstChar, fontData.LastChar,
                    fontData.RenderOffsets!, fontData.SourceRectangles!, fontData.Advances[0], fontData.Ascender,
                    fontData.Descender, fontData.LineGap, fontData.Name!);
            }

            return new SpacedTextureFont(texture, fontData.Size, fontData.FirstChar, fontData.LastChar,
                fontData.RenderOffsets!, fontData.SourceRectangles!, fontData.Advances, fontData.Ascender,
                fontData.Descender, fontData.LineGap, fontData.Name!);
        }
    }
}
