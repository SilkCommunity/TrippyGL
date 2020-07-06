using System;
using System.IO;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TrippyGL
{
    /// <summary>
    /// Extension methods for <see cref="FramebufferObject"/>.
    /// </summary>
    public static class FramebufferObjectExtensions
    {
        /// <summary>
        /// Saves this <see cref="FramebufferObject"/>'s pixels as an image file. You can't save multisampled framebuffers.<para/>
        /// If the file already exists, it will be replaced.
        /// </summary>
        /// <param name="framebuffer">The <see cref="FramebufferObject"/> whose image to save.</param>
        /// <param name="file">The name of the file where the image will be saved.</param>
        /// <param name="imageFormat">The format the image will be saved as.</param>
        public static void SaveAsImage(this FramebufferObject framebuffer, string file, SaveImageFormat imageFormat)
        {
            if (framebuffer == null)
                throw new ArgumentNullException(nameof(framebuffer));

            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException(nameof(file));

            IImageFormat format = ImageUtils.GetFormatFor(imageFormat);
            using Image<Rgba32> image = new Image<Rgba32>((int)framebuffer.Width, (int)framebuffer.Height);
            if (!image.TryGetSinglePixelSpan(out Span<Rgba32> pixels))
                throw new InvalidDataException(ImageUtils.ImageNotContiguousError);
            framebuffer.ReadPixels(pixels, 0, 0, framebuffer.Width, framebuffer.Height, ReadPixelsFormat.Rgba, PixelType.UnsignedByte);
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            using FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
            image.Save(fileStream, format);
        }
    }
}
