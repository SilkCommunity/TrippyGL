using System;
using System.IO;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TrippyGL.ImageSharp;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA2000 // Dispose objects before losing scope

namespace TrippyGL.ImageSharp
{
    public sealed class TextureFontData
    {
        public const ushort MaxFontNameLength = 128;

        public float Size;

        public char FirstChar;
        public char LastChar;
        public int CharCount => LastChar - FirstChar + 1;

        public float Ascender;
        public float Descender;
        public float LineGap;
        public float LineAdvance => Ascender - Descender + LineGap;

        public string Name;

        public float[] Advances;

        public Vector2[,] KerningOffsets;

        public Vector2[] RenderOffsets;

        public System.Drawing.Rectangle[] SourceRectangles;

        public TextureFont CreateFont(Texture2D texture)
        {
            if (KerningOffsets != null)
            {
                if (Advances.Length == 1 && Advances.Length != CharCount)
                {
                    float adv = Advances[0];
                    Advances = new float[CharCount];
                    for (int i = 0; i < Advances.Length; i++)
                        Advances[i] = adv;
                }

                return new KerningTextureFont(texture, Size, FirstChar, LastChar, RenderOffsets,
                    SourceRectangles, KerningOffsets, Advances, Ascender, Descender, LineGap, Name);
            }

            if (Advances.Length == 1)
            {
                return new MonospaceTextureFont(texture, Size, FirstChar, LastChar, RenderOffsets,
                    SourceRectangles, Advances[0], Ascender, Descender, LineGap, Name);
            }

            return new SpacedTextureFont(texture, Size, FirstChar, LastChar, RenderOffsets,
                SourceRectangles, Advances, Ascender, Descender, LineGap, Name);
        }

        public void WriteToStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            WriteToStream(new BinaryWriter(stream));
        }

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

            char firstChar = streamReader.ReadChar();
            char lastChar = streamReader.ReadChar();
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
                FirstChar = firstChar,
                LastChar = lastChar,
                Name = name,
                Advances = advances,
                KerningOffsets = kerningOffsets,
                SourceRectangles = sources,
                RenderOffsets = renderOffsets,
            };
        }

        public static void WritePreamble(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            stream.WriteByte(175);
            stream.WriteByte(1);
            stream.WriteByte(69);
            stream.WriteByte(33);
            stream.WriteByte(222);
        }

        public static bool ReadPreamble(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return stream.ReadByte() == 175
                && stream.ReadByte() == 1
                && stream.ReadByte() == 69
                && stream.ReadByte() == 33
                && stream.ReadByte() == 222;
        }

        public static TextureFontData[] FromFile(string file, out Image<Rgba32> image)
        {
            using FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            return FromStream(new BinaryReader(fileStream), out image);
        }

        public static TextureFontData[] FromStream(Stream stream, out Image<Rgba32> image)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return FromStream(new BinaryReader(stream), out image);
        }

        public static TextureFontData[] FromStream(BinaryReader streamReader, out Image<Rgba32> image)
        {
            if (streamReader == null)
                throw new ArgumentNullException(nameof(streamReader));

            if (!ReadPreamble(streamReader.BaseStream))
                throw new FontLoadingException("Wrong preamble. Ensure you're loading the correct data.");

            streamReader.ReadInt32();
            streamReader.ReadInt32();
            streamReader.ReadInt32();
            streamReader.ReadInt32();

            ushort fontCount = streamReader.ReadUInt16();

            TextureFontData[] fontDatas = new TextureFontData[fontCount];
            for (int i = 0; i < fontDatas.Length; i++)
                fontDatas[i] = FromStream(streamReader);

            char pChar = streamReader.ReadChar();
            char nChar = streamReader.ReadChar();
            char gChar = streamReader.ReadChar();

            image = Image.Load<Rgba32>(streamReader.BaseStream, out SixLabors.ImageSharp.Formats.IImageFormat format);
            return fontDatas;
        }

        public static void WriteToStream(BinaryWriter streamWriter, TextureFontData[] fontDatas, Image<Rgba32> image)
        {
            if (streamWriter == null)
                throw new ArgumentNullException(nameof(streamWriter));

            if (fontDatas == null || fontDatas.Length == 0)
                throw new ArgumentNullException(nameof(fontDatas));

            if (image == null)
                throw new ArgumentNullException(nameof(image));

            WritePreamble(streamWriter.BaseStream);

            streamWriter.Write(0);
            streamWriter.Write(0);
            streamWriter.Write(0);
            streamWriter.Write(0);

            streamWriter.Write((ushort)fontDatas.Length);

            for (int i = 0; i < fontDatas.Length; i++)
                fontDatas[i].WriteToStream(streamWriter);

            streamWriter.Write('p');
            streamWriter.Write('n');
            streamWriter.Write('g');

            image.SaveAsPng(streamWriter.BaseStream);
        }

        internal enum FontTypeByte : byte
        {
            Monospace = 0,
            Spaced = 1,
            SpacedWithKerning = 2
        }
    }
}
