using System;
using System.IO;
using System.Numerics;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA2000 // Dispose objects before losing scope

namespace TrippyGL.Fonts
{
    /// <summary>
    /// Contains information for a single font inside a <see cref="TrippyFontFile"/>.
    /// </summary>
    public sealed class TextureFontData
    {
        /// <summary>The maximum amount of characters a font name can have.</summary>
        public const ushort MaxFontNameLength = 256;

        /// <summary>The size of the font, typically measured in pixels.</summary>
        public float Size;

        /// <summary>The lowest character available in this <see cref="TextureFontData"/>.</summary>
        public char FirstChar;

        /// <summary>The highest character available in this <see cref="TextureFontData"/>.</summary>
        public char LastChar;

        /// <summary>The amount of characters in this <see cref="TextureFontData"/>.</summary>
        public int CharCount => LastChar - FirstChar + 1;

        /// <summary>The distance between the baseline and the highest glyph's highest point. Typically positive.</summary>
        public float Ascender;

        /// <summary>The distance between the baseline and the lowest glyph's lowest point. Typically negative.</summary>
        public float Descender;

        /// <summary>The distance between the lowest point of a line and the highest point of the next line.</summary>
        public float LineGap;

        /// <summary>This <see cref="TextureFontData"/>'s name.</summary>
        public string Name;

        /// <summary>The advance values for the characters in this font.</summary>
        public float[] Advances;

        /// <summary>The kerning offsets for each character. This are in order [from, to].</summary>
        public Vector2[,] KerningOffsets;

        /// <summary>Offsets that should be directly applied to the characters when drawing them.</summary>
        public Vector2[] RenderOffsets;

        /// <summary>The areas in the font's image/texture where each character is located.</summary>
        public System.Drawing.Rectangle[] SourceRectangles;

        /// <summary>
        /// Writes the data of this <see cref="TextureFontData"/> into a stream.
        /// </summary>
        public void WriteToStream(BinaryWriter streamWriter)
        {
            if (streamWriter == null)
                throw new ArgumentNullException(nameof(streamWriter));

            if (LastChar < FirstChar)
                throw new ArgumentException(nameof(LastChar) + " can't be lower than " + nameof(FirstChar) + ".");

            if (!float.IsFinite(Size) || float.IsNegative(Size))
                throw new ArgumentException(nameof(Size) + " must be a finite positive value.");

            if (!float.IsFinite(Ascender) || !float.IsFinite(Descender) || !float.IsFinite(LineGap))
                throw new ArgumentException(nameof(Ascender) + ", " + nameof(Descender) + " and " + nameof(LineGap) + " must be finite values.");

            if (Name != null && Name.Length > MaxFontNameLength)
                throw new Exception(nameof(Name) + " must be at most " + MaxFontNameLength + " characters long.");

            if (Advances == null)
                throw new ArgumentNullException(nameof(Advances));

            if (RenderOffsets == null)
                throw new ArgumentNullException(nameof(RenderOffsets));

            if (SourceRectangles == null)
                throw new ArgumentNullException(nameof(SourceRectangles));

            int charCount = CharCount;
            if (Advances.Length != 1 && Advances.Length != charCount)
                throw new ArgumentException("The length of the " + nameof(Advances) + " array must be either 1 (for monospace) or equal to " + nameof(CharCount));

            if (RenderOffsets.Length != charCount)
                throw new ArgumentException("The length of the " + nameof(RenderOffsets) + " array must be equal to " + nameof(CharCount));

            if (SourceRectangles.Length != charCount)
                throw new ArgumentException("The length of the " + nameof(SourceRectangles) + " array must be equal to " + nameof(CharCount));

            if (KerningOffsets != null && (KerningOffsets.GetLength(0) != charCount || KerningOffsets.GetLength(1) != charCount))
                throw new ArgumentException("The length on both dimentions of the " + nameof(KerningOffsets) + " array must be equal to " + nameof(CharCount));

            FontTypeByte typeByte = KerningOffsets == null ? (Advances.Length == 1 ? FontTypeByte.Monospace : FontTypeByte.Spaced) : FontTypeByte.SpacedWithKerning;

            streamWriter.Write(0);
            streamWriter.Write(0);
            streamWriter.Write(0);
            streamWriter.Write(0);

            streamWriter.Write(Size);
            streamWriter.Write((int)FirstChar);
            streamWriter.Write((int)LastChar);
            streamWriter.Write(Ascender);
            streamWriter.Write(Descender);
            streamWriter.Write(LineGap);
            streamWriter.Write((byte)typeByte);

            if (string.IsNullOrEmpty(Name))
                streamWriter.Write((ushort)0);
            else
            {
                streamWriter.Write((ushort)Name.Length);
                for (int i = 0; i < Name.Length; i++)
                    streamWriter.Write(Name[i]);
            }

            if (typeByte == FontTypeByte.Monospace)
                streamWriter.Write(Advances[0]);
            else
            {
                if (Advances.Length == 1)
                {
                    for (int i = 0; i < Advances.Length; i++)
                        streamWriter.Write(Advances[0]);
                }
                else
                {
                    for (int i = 0; i < Advances.Length; i++)
                        streamWriter.Write(Advances[i]);
                }

                if (typeByte == FontTypeByte.SpacedWithKerning)
                {
                    for (int i = 0; i < KerningOffsets.GetLength(0); i++)
                        for (int c = 0; c < KerningOffsets.GetLength(1); c++)
                        {
                            streamWriter.Write(KerningOffsets[i, c].X);
                            streamWriter.Write(KerningOffsets[i, c].Y);
                        }
                }
            }

            for (int i = 0; i < SourceRectangles.Length; i++)
            {
                streamWriter.Write((short)SourceRectangles[i].X);
                streamWriter.Write((short)SourceRectangles[i].Y);
                streamWriter.Write((short)SourceRectangles[i].Width);
                streamWriter.Write((short)SourceRectangles[i].Height);
            }

            for (int i = 0; i < RenderOffsets.Length; i++)
            {
                streamWriter.Write(RenderOffsets[i].X);
                streamWriter.Write(RenderOffsets[i].Y);
            }
        }

        /// <summary>
        /// Creates a <see cref="TextureFontData"/> by reading it's data from a stream.
        /// </summary>
        public static TextureFontData FromStream(BinaryReader streamReader)
        {
            if (streamReader == null)
                throw new ArgumentNullException(nameof(streamReader));

            streamReader.ReadInt32();
            streamReader.ReadInt32();
            streamReader.ReadInt32();
            streamReader.ReadInt32();

            float size = streamReader.ReadSingle();
            if (!float.IsFinite(size) || float.IsNegative(size))
                throw new FontLoadingException("Invalid Size: " + size + ".");

            int firstChar = streamReader.ReadInt32();
            int lastChar = streamReader.ReadInt32();
            if (lastChar < firstChar)
                throw new FontLoadingException("LastChar is lower than FirstChar.");
            int charCount = lastChar - firstChar + 1;

            float ascender = streamReader.ReadSingle();
            float descender = streamReader.ReadSingle();
            float lineGap = streamReader.ReadSingle();
            if (!float.IsFinite(ascender) || !float.IsFinite(descender) || !float.IsFinite(lineGap))
                throw new FontLoadingException("Ascender, Descender and LineGap must be finite values.");

            FontTypeByte typeByte = (FontTypeByte)streamReader.ReadByte();
            if (typeByte > FontTypeByte.SpacedWithKerning)
                throw new FontLoadingException("Invalid " + nameof(FontTypeByte));

            ushort nameLength = streamReader.ReadUInt16();
            string name = nameLength == 0 ? null : string.Create(nameLength, streamReader, (chars, sr) =>
            {
                for (int i = 0; i < chars.Length; i++)
                    chars[i] = sr.ReadChar();
            });

            float[] advances = typeByte == FontTypeByte.Monospace ? new float[1] : new float[charCount];
            Vector2[,] kerningOffsets = null;

            if (typeByte == FontTypeByte.Monospace)
            {
                float a = streamReader.ReadSingle();
                if (!float.IsFinite(a))
                    throw new FontLoadingException("Advance values must be finite.");
                advances[0] = a;
            }
            else
            {
                for (int i = 0; i < advances.Length; i++)
                {
                    float a = streamReader.ReadSingle();
                    if (!float.IsFinite(a))
                        throw new FontLoadingException("Advance values must be finite.");
                    advances[i] = a;
                }

                if (typeByte == FontTypeByte.SpacedWithKerning)
                {
                    kerningOffsets = new Vector2[charCount, charCount];
                    for (int i = 0; i < kerningOffsets.GetLength(0); i++)
                        for (int c = 0; c < kerningOffsets.GetLength(1); c++)
                        {
                            float x = streamReader.ReadSingle();
                            float y = streamReader.ReadSingle();
                            if (!float.IsFinite(x) || !float.IsFinite(y))
                                throw new FontLoadingException("Kerning offsets must be finite.");

                            kerningOffsets[i, c] = new Vector2(x, y);
                        }
                }
            }

            System.Drawing.Rectangle[] sources = new System.Drawing.Rectangle[charCount];
            for (int i = 0; i < sources.Length; i++)
            {
                short x = streamReader.ReadInt16();
                short y = streamReader.ReadInt16();
                short wid = streamReader.ReadInt16();
                sources[i] = new System.Drawing.Rectangle(x, y, wid, streamReader.ReadInt16());
            }

            Vector2[] renderOffsets = new Vector2[charCount];
            for (int i = 0; i < renderOffsets.Length; i++)
            {
                float x = streamReader.ReadSingle();
                float y = streamReader.ReadSingle();
                if (!float.IsFinite(x) || !float.IsFinite(y))
                    throw new FontLoadingException("Render offsets must be finite.");

                renderOffsets[i] = new Vector2(x, y);
            }

            return new TextureFontData()
            {
                Size = size,
                Ascender = ascender,
                Descender = descender,
                LineGap = lineGap,
                FirstChar = (char)firstChar,
                LastChar = (char)lastChar,
                Name = name,
                Advances = advances,
                KerningOffsets = kerningOffsets,
                SourceRectangles = sources,
                RenderOffsets = renderOffsets,
            };
        }

        /// <summary>
        /// Used to identify a specific byte when saving/loading <see cref="TextureFontData"/> to/from
        /// a stream that defines whether the font is monospaced, spaced, or spaced with kerning.
        /// </summary>
        internal enum FontTypeByte : byte
        {
            Monospace = 0,
            Spaced = 1,
            SpacedWithKerning = 2
        }

        public override string ToString()
        {
            return string.Concat(Name ?? "Unnamed " + nameof(TextureFontData), " - ", CharCount.ToString(), " characters");
        }
    }
}
