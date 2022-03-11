using System;
using System.IO;
using SixLabors.Fonts;
using TrippyGL;
using TrippyGL.Fonts;
using TrippyGL.Fonts.Building;
using TrippyGL.Fonts.Extensions;

namespace TextureBatcherTest
{
    static class FontLoadHelper
    {
        public const string FontFileName = "font.tglf";

        public static void LoadOrCreateFonts(GraphicsDevice graphicsDevice, out TextureFont ComicSans48, out TextureFont ArialItalic36)
        {
            try
            {
                // Tries to load the font files from the file font.tglf. Will fail if the file is corrupted or doesn't exist.
                TextureFont[] fonts = TextureFontExtensions.FromFile(graphicsDevice, FontFileName);
                ComicSans48 = fonts[0];
                ArialItalic36 = fonts[1];
            }
            catch
            {
                Console.WriteLine(FontFileName + " file not found, creating the fonts...");

                // Type of file that can store information about multiple TextureFonts (as long as they share a single Texture2D).
                TrippyFontFile fontFile;
                try
                {
                    Font comicSans48 = SystemFonts.CreateFont("Comic Sans MS", 48);
                    Font arialItalic36 = SystemFonts.CreateFont("Arial", 36, FontStyle.Italic);
                    fontFile = FontBuilderExtensions.CreateFontFile(new Font[] { comicSans48, arialItalic36 });
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed to create font files! Are these system fonts installed? \"Comic Sans MS\" and \"Arial Italic\"");
                    Console.ResetColor();
                    Console.WriteLine(e.Message);
                    Console.ReadKey();
                    throw new FileNotFoundException("System Fonts: \"Comic Sans MS\" and \"Arial Italic\"");
                }

                // Create and load up the fonts for rendering.
                TextureFont[] fonts = fontFile.CreateFonts(graphicsDevice);
                ComicSans48 = fonts[0];
                ArialItalic36 = fonts[1];

                // Users are not expected to write code like this, but rather create their TrippyFontFile and ship the
                // resulting ".tglf" file as an asset. You can then easily load them up using TextureFontExtensions.FromFile()

                try
                {
                    fontFile.WriteToFile(FontFileName);
                    Console.WriteLine("Written created fonts to " + FontFileName + " for future reuse.");
                }
                catch
                {
                    Console.WriteLine("Failed to save fonts to " + FontFileName + ". They will be recreated rather than reused next time.");
                }
            }
        }
    }
}
