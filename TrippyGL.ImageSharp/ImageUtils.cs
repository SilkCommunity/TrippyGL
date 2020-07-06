using System;
using SixLabors.ImageSharp.Formats;

namespace TrippyGL
{
    /// <summary>
    /// Contains helper image-related methods used across the library.
    /// </summary>
    public static class ImageUtils
    {
        internal const string ImageNotContiguousError = "To load/save an image, it's backing memory must be contiguous. Consider using smaller image sizes or changing your ImageSharp memory allocation settings to allow larger buffers.";

        /// <summary>
        /// Gets an appropiate <see cref="IImageFormat"/> for the given <see cref="SaveImageFormat"/>.
        /// </summary>
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
