using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
#pragma warning disable CA2000 // Dispose objects before losing scope

namespace TrippyGL.Fonts
{
    /// <summary>
    /// Stores information for multiple fonts and provides methods for loading/saving from/to streams.
    /// </summary>
    public sealed class TrippyFontFile : IDisposable
    {
        /// <summary>The information of all the fonts in this <see cref="TrippyFontFile"/>.</summary>
        public TextureFontData[] FontDatas;

        /// <summary>The image in this <see cref="TrippyFontFile"/>.</summary>
        public Image<Rgba32> Image;

        /// <summary>
        /// Creates a <see cref="TrippyFontFile"/> instance.
        /// </summary>
        /// <param name="fontDatas">The information of the fonts.</param>
        /// <param name="image">The image containing the font's characters.</param>
        public TrippyFontFile(TextureFontData[] fontDatas, Image<Rgba32> image)
        {
            FontDatas = fontDatas;
            Image = image;
        }

        /// <summary>
        /// Throws an exception if any of the fields in this <see cref="TrippyFontFile"/> is null.
        /// </summary>
        public void ThrowIfAnyNull()
        {
            if (FontDatas == null || FontDatas.Length == 0)
                throw new InvalidOperationException(nameof(FontDatas) + " can't be null.");

            if (FontDatas.Length > ushort.MaxValue)
                throw new InvalidOperationException("Too many items in " + nameof(FontDatas));

            if (Image == null)
                throw new InvalidOperationException(nameof(Image) + " can't be null.");

            for (int i = 0; i < FontDatas.Length; i++)
                if (FontDatas[i] == null)
                    throw new InvalidOperationException("The elements in the " + nameof(FontDatas) + "can't be null.");
        }

        /// <summary>
        /// Disposes the image used by this <see cref="TrippyFontFile"/>.
        /// </summary>
        public void Dispose()
        {
            Image?.Dispose();
            Image = null;
        }

        /// <summary>
        /// Reads a sequence of bytes from the stream and returns whether it matches the
        /// preamble that all <see cref="TrippyFontFile"/> streams start with.
        /// </summary>
        internal static bool ReadPreamble(Stream stream)
        {
            return stream.ReadByte() == 175
                && stream.ReadByte() == 1
                && stream.ReadByte() == 69
                && stream.ReadByte() == 33
                && stream.ReadByte() == 222;
        }

        /// <summary>
        /// Writes the sequence of bytes with which all <see cref="TrippyFontFile"/> streams start with.
        /// </summary>
        internal static void WritePreamble(Stream stream)
        {
            stream.WriteByte(175);
            stream.WriteByte(1);
            stream.WriteByte(69);
            stream.WriteByte(33);
            stream.WriteByte(222);
        }

        /// <summary>
        /// Creates a <see cref="TrippyFontFile"/> by reading it's data from a file.
        /// </summary>
        public static TrippyFontFile FromFile(string file)
        {
            using FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            return FromStream(new BinaryReader(fileStream));
        }

        /// <summary>
        /// Creates a <see cref="TrippyFontFile"/> by reading it's data from a stream.
        /// </summary>
        public static TrippyFontFile FromStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return FromStream(new BinaryReader(stream));
        }

        /// <summary>
        /// Creates a <see cref="TrippyFontFile"/> by reading it's data from a stream.
        /// </summary>
        public static TrippyFontFile FromStream(BinaryReader streamReader)
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
                fontDatas[i] = TextureFontData.FromStream(streamReader);

            streamReader.ReadChar(); // 'p'
            streamReader.ReadChar(); // 'n'
            streamReader.ReadChar(); // 'g'

            Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(streamReader.BaseStream);
            return new TrippyFontFile(fontDatas, image);
        }

        /// <summary>
        /// Writes this <see cref="TrippyFontFile"/>'s data to a file.
        /// </summary>
        public void WriteToFile(string file)
        {
            using FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write);
            WriteToStream(new BinaryWriter(fileStream));
        }

        /// <summary>
        /// Writes this <see cref="TrippyFontFile"/>'s data to a stream.
        /// </summary>
        public void WriteToStream(Stream stream)
        {
            WriteToStream(new BinaryWriter(stream));
        }

        /// <summary>
        /// Writes this <see cref="TrippyFontFile"/>'s data to a stream.
        /// </summary>
        public void WriteToStream(BinaryWriter streamWriter)
        {
            if (streamWriter == null)
                throw new ArgumentNullException(nameof(streamWriter));

            ThrowIfAnyNull();

            WritePreamble(streamWriter.BaseStream);

            streamWriter.Write(0);
            streamWriter.Write(0);
            streamWriter.Write(0);
            streamWriter.Write(0);

            streamWriter.Write((ushort)FontDatas.Length);

            for (int i = 0; i < FontDatas.Length; i++)
                FontDatas[i].WriteToStream(streamWriter);

            streamWriter.Write('p');
            streamWriter.Write('n');
            streamWriter.Write('g');

            Image.SaveAsPng(streamWriter.BaseStream);
        }

        public override string ToString()
        {
            return FontDatas == null ? "Empty " + nameof(TrippyFontFile)
                : string.Concat(nameof(TrippyFontFile) + " with ", FontDatas.Length.ToString(), " fonts");
        }
    }
}
