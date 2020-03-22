using System;
using SixLabors.ImageSharp.Formats;

namespace TrippyGL.ImageSharp
{
    public static class Utils
    {
        public static IImageFormat GetFormatFor(SaveImageFormat imageFormat)
        {
            return imageFormat switch
            {
                SaveImageFormat.Png => SixLabors.ImageSharp.Formats.Png.PngFormat.Instance,
                SaveImageFormat.Jpeg => SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance,
                SaveImageFormat.Bmp => SixLabors.ImageSharp.Formats.Bmp.BmpFormat.Instance,
                SaveImageFormat.Gif => SixLabors.ImageSharp.Formats.Gif.GifFormat.Instance,
                _ => throw new ArgumentException("Invalid " + nameof(SaveImageFormat), nameof(imageFormat)),
            };
        }
    }
}
