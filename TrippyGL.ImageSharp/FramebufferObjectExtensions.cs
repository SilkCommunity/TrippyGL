using System;
using System.IO;
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
        /// Reads pixels from this <see cref="FramebufferObject"/>.
        /// </summary>
        /// <param name="framebuffer">The <see cref="FramebufferObject"/> to read pixels from.</param>
        /// <param name="x">The x position of the first pixel to read.</param>
        /// <param name="y">The y position of the first pixel to read.</param>
        /// <param name="image">The image in which to write the pixel data.</param>
        public static void ReadPixels(this FramebufferObject framebuffer, int x, int y, Image<Rgba32> image)
        {
            if (framebuffer == null)
                throw new ArgumentNullException(nameof(framebuffer));

            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (!image.TryGetSinglePixelSpan(out Span<Rgba32> pixels))
                throw new InvalidDataException(ImageUtils.ImageNotContiguousError);

            framebuffer.ReadPixels(pixels, x, y, (uint)image.Width, (uint)image.Height);
        }

        /// <summary>
        /// Reads pixels from this <see cref="FramebufferObject"/>.
        /// </summary>
        /// <param name="framebuffer">The <see cref="FramebufferObject"/> to read pixels from.</param>
        /// <param name="image">The image in which to write the pixel data.</param>
        public static void ReadPixels(this FramebufferObject framebuffer, Image<Rgba32> image)
        {
            ReadPixels(framebuffer, 0, 0, image);
        }

        /// <summary>
        /// Saves this <see cref="FramebufferObject"/>'s image to a stream. You can't save multisampled framebuffers.
        /// </summary>
        /// <param name="framebuffer">The <see cref="FramebufferObject"/> whose image to save.</param>
        /// <param name="stream">The stream to save the framebuffer image to.</param>
        /// <param name="imageFormat">The format the image will be saved as.</param>
        public static void SaveAsImage(this FramebufferObject framebuffer, Stream stream, SaveImageFormat imageFormat)
        {
            if (framebuffer == null)
                throw new ArgumentNullException(nameof(framebuffer));

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            IImageFormat format = ImageUtils.GetFormatFor(imageFormat);
            using Image<Rgba32> image = new Image<Rgba32>((int)framebuffer.Width, (int)framebuffer.Height);
            framebuffer.ReadPixels(image);
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            image.Save(stream, format);
        }

        /// <summary>
        /// Saves this <see cref="FramebufferObject"/>'s image to a file. You can't save multisampled framebuffers.<para/>
        /// If the file already exists, it will be replaced.
        /// </summary>
        /// <param name="framebuffer">The <see cref="FramebufferObject"/> whose image to save.</param>
        /// <param name="file">The name of the file where the image will be saved.</param>
        /// <param name="imageFormat">The format the image will be saved as.</param>
        public static void SaveAsImage(this FramebufferObject framebuffer, string file, SaveImageFormat imageFormat)
        {
            using FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
            SaveAsImage(framebuffer, fileStream, imageFormat);
        }
    }
}
