using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
#pragma warning disable CA2000 // Dispose objects before losing scope

namespace TrippyGL.ImageSharp
{
    public class TrippyFontFile : IDisposable
    {
        public TextureFontData[] FontDatas;

        public Image<Rgba32> Image;

        public TrippyFontFile(TextureFontData[] fontDatas, Image<Rgba32> image)
        {
            FontDatas = fontDatas;
            Image = image;
        }

        private void ValidateFields()
        {
            if (FontDatas == null || FontDatas.Length == 0)
                throw new InvalidOperationException(nameof(FontDatas) + " can't be null.");

            if (FontDatas.Length > ushort.MaxValue)
                throw new InvalidOperationException("Too many items in " + nameof(FontDatas));

            if (Image == null)
                throw new InvalidOperationException(nameof(Image) + " can't be null.");
        }

        public TextureFont[] CreateFonts(GraphicsDevice graphicsDevice, bool generateMipmaps = false)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            ValidateFields();

            Texture2D texture = new Texture2D(graphicsDevice, (uint)Image.Width, (uint)Image.Height, generateMipmaps);
            try
            {
                TextureFont[] textureFonts = new TextureFont[FontDatas.Length];
                for (int i = 0; i < textureFonts.Length; i++)
                    textureFonts[i] = FontDatas[i].CreateFont(texture);

                texture.SetData(Image);
                return textureFonts;
            }
            catch
            {
                texture.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            Image?.Dispose();
            Image = null;
        }

        internal static bool ReadPreamble(Stream stream)
        {
            return stream.ReadByte() == 175
                && stream.ReadByte() == 1
                && stream.ReadByte() == 69
                && stream.ReadByte() == 33
                && stream.ReadByte() == 222;
        }

        internal static void WritePreamble(Stream stream)
        {
            stream.WriteByte(175);
            stream.WriteByte(1);
            stream.WriteByte(69);
            stream.WriteByte(33);
            stream.WriteByte(222);
        }

        public static TrippyFontFile FromFile(string file)
        {
            using FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            return FromStream(new BinaryReader(fileStream));
        }

        public static TrippyFontFile FromStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return FromStream(new BinaryReader(stream));
        }

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

        public void WriteToFile(string file)
        {
            using FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write);
            WriteToStream(new BinaryWriter(fileStream));
        }

        public void WriteToStream(Stream stream)
        {
            WriteToStream(new BinaryWriter(stream));
        }

        public void WriteToStream(BinaryWriter streamWriter)
        {
            if (streamWriter == null)
                throw new ArgumentNullException(nameof(streamWriter));

            ValidateFields();

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
    }
}
