using System;
using SixLabors.ImageSharp.Formats;

namespace TrippyGL.ImageSharp
{
    /// <summary>
    /// Contains helper image-related methods used across the library.
    /// </summary>
    public static class ImageUtils
    {
        internal const string ImageNotContiguousError = "To load/save an image, it's backing memory must be contiguous. Consider using smaller image sizes or changing your ImageSharp memory allocation settings to allow larger buffers.";

        internal const string ImageSizeMustMatchTextureSizeError = "The size of the image must match the size of the texture";

        internal const string TextureFormatMustBeColor4bError = "The texture's format must be Color4b (RGBA)";

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
