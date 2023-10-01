using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using SixLabors.Fonts;
using SixLabors.Fonts.Unicode;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TrippyGL.Fonts.Building
{
    /// <summary>
    /// An implementation of <see cref="IGlyphSource"/> that sources it's glyphs from
    /// a <see cref="SixLabors.Fonts"/> font.
    /// </summary>
    public sealed class FontGlyphSource : IGlyphSource
    {
        /// <summary>The DPI to use for drawing the glyphs.</summary>
        private const float DrawDpi = 96;

        /// <summary>The DPI to use for calculations.</summary>
        private const float CalcDpi = 72;

        /// <summary>The <see cref="Font"/> from which this <see cref="FontGlyphSource"/> gets glyph data.</summary>
        public readonly Font Font;

        /// <summary>The path collections that make up each character.</summary>
        private readonly IPathCollection[] glyphPaths;

        /// <summary>The colors of the paths that make up each character. Might be null.</summary>
        private readonly Color?[][]? pathColors;

        /// <summary>Configuration for how glyphs should be rendered.</summary>
        public DrawingOptions DrawingOptions;

        /// <summary>The color with which to draw glyphs when no color is present. Default is <see cref="Color.White"/>.</summary>
        public Color DefaultGlyphColor = Color.White;

        /// <summary>The sizes for all characters.</summary>
        private readonly System.Drawing.Point[] glyphSizes;

        /// <summary>The render offsets for all characters.</summary>
        private readonly Vector2[] renderOffsets;

        /// <summary>Whether to include kerning if present in the font. Default is true.</summary>
        public bool IncludeKerningIfPresent = true;

        public char FirstChar { get; }

        public char LastChar { get; }

        public float Size => Font.Size;

        public string Name { get; }

        public float Ascender => Font.Size * (DrawDpi / CalcDpi) * Font.FontMetrics.HorizontalMetrics.Ascender / Font.FontMetrics.UnitsPerEm;

        public float Descender => Font.Size * (DrawDpi / CalcDpi) * Font.FontMetrics.HorizontalMetrics.Descender / Font.FontMetrics.UnitsPerEm;

        public float LineGap => Font.Size * (DrawDpi / CalcDpi) * Font.FontMetrics.HorizontalMetrics.LineGap / Font.FontMetrics.UnitsPerEm;

        public int CharCount => LastChar - FirstChar + 1;

        /// <summary>
        /// Creates a <see cref="FontGlyphSource"/> instance.
        /// </summary>
        public FontGlyphSource(Font font, string name, char firstChar = ' ', char lastChar = '~')
        {
            if (lastChar < firstChar)
                throw new ArgumentException(nameof(LastChar) + " can't be lower than " + nameof(firstChar));

            Font = font ?? throw new ArgumentNullException(nameof(font));

            FirstChar = firstChar;
            LastChar = lastChar;
            Name = name;

            glyphPaths = CreatePaths(out pathColors, out glyphSizes, out renderOffsets);

            DrawingOptions = new DrawingOptions
            {
                ShapeOptions = { IntersectionRule = IntersectionRule.NonZero },
            };
        }

        /// <summary>
        /// Creates a <see cref="FontGlyphSource"/> instance.
        /// </summary>
        public FontGlyphSource(Font font, char firstChar = ' ', char lastChar = '~')
            : this(font, font.FontMetrics.Description.FontNameInvariantCulture, firstChar, lastChar) { }

        /// <summary>
        /// Creates the <see cref="IPathCollection"/> for all the characters, also getting their colors,
        /// glyph sizes and render offsets.
        /// </summary>
        private IPathCollection[] CreatePaths(out Color?[][]? colors, out System.Drawing.Point[] sizes, out Vector2[] offsets)
        {
            FontMetrics fontMetrics = Font.FontMetrics;
            ColorGlyphRenderer glyphRenderer = new ColorGlyphRenderer();
            TextRenderer textRenderer = new TextRenderer(glyphRenderer);
            TextOptions textOpts = new TextOptions(Font) { Dpi = DrawDpi };

            Span<char> singleCharSpan = stackalloc char[1];

            IPathCollection[] paths = new IPathCollection[CharCount];
            sizes = new System.Drawing.Point[paths.Length];
            offsets = new Vector2[paths.Length];
            colors = null;

            for (int i = 0; i < paths.Length; i++)
            {
                char c = (char)(i + FirstChar);
                glyphRenderer.Reset();
                if (!fontMetrics.TryGetGlyphMetrics(new CodePoint(c), TextAttributes.None, TextDecorations.None, LayoutMode.HorizontalTopBottom, ColorFontSupport.None, out IReadOnlyList<GlyphMetrics>? glyphs))
                    continue;
                GlyphMetrics? glyphMetrics = glyphs.FirstOrDefault();
                if (glyphMetrics == null)
                    continue;

                singleCharSpan[0] = c;
                textRenderer.RenderText(singleCharSpan, textOpts);
                IPathCollection p = glyphRenderer.Paths;
                RectangleF bounds = p.Bounds;

                float area = bounds.Width * bounds.Height;
                if (float.IsFinite(area) && area != 0 && (c > char.MaxValue || !char.IsWhiteSpace(c)))
                {
                    paths[i] = p;
                    sizes[i] = new System.Drawing.Point((int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height));
                    renderOffsets[i] = new Vector2(bounds.X, bounds.Y);
                }

                if (glyphRenderer.HasAnyPathColors())
                {
                    colors ??= new Color?[CharCount][];
                    colors[i] = glyphRenderer.PathColors;
                }
            }

            return paths;
        }

        public bool GetAdvances([NotNullWhen(true)] out float[]? advances)
        {
            FontMetrics fontMetrics = Font.FontMetrics;

            // If all glyphs have the same advance value, the "advances" array will remain null, and "adv" will
            // contain that advance value.
            // If at least two glyphs have differente advance values, that value of "adv" will be ignored, and
            // "advances" will store every glyph's advance separately.
            // If no glyph is found with a valid advance value, both will remain null.
            float[]? advancesArray = null;
            float? adv = null;

            for (int i = FirstChar; i <= LastChar; i++)
            {

                if (!fontMetrics.TryGetGlyphMetrics(new CodePoint(i), TextAttributes.None, TextDecorations.None, LayoutMode.HorizontalTopBottom, ColorFontSupport.None, out IReadOnlyList<GlyphMetrics>? glyphs))
                    continue;
                GlyphMetrics? inst = glyphs.FirstOrDefault();
                if (inst == null)
                    continue;

                float iAdv = inst.AdvanceWidth * Font.Size * (DrawDpi / CalcDpi) / inst.UnitsPerEm;

                if (advancesArray == null)
                {
                    if (adv == null)
                    {
                        adv = iAdv;
                    }
                    else if (iAdv != adv)
                    {
                        // We need to create the "advances" array and move every glyph's advance value to there.
                        // All the glyphs up to this point had the same advance value "adv", so we fill the "advances"
                        // array with that value up to the index of the current glyph.
                        advancesArray = new float[CharCount];
                        for (int c = 0; c < i - FirstChar; c++)
                            advancesArray[c] = adv.Value;
                        advancesArray[i - FirstChar] = iAdv;
                    }
                }
                else
                    advancesArray[i - FirstChar] = iAdv;
            }

            if (advancesArray == null)
            {
                advances = new float[1] { adv.GetValueOrDefault() };
                return false;
            }

            advances = advancesArray;
            return true;
        }

        public bool TryGetKerning([NotNullWhen(true)] out Vector2[,]? kerningOffsets)
        {
            FontMetrics fontMetrics = Font.FontMetrics;

            kerningOffsets = null;
            if (!IncludeKerningIfPresent)
                return false;

            for (int a = FirstChar; a <= LastChar; a++)
            {
                if (!Font.TryGetGlyphs(new CodePoint(a), out IReadOnlyList<Glyph>? aGlyphs) || aGlyphs.Count == 0)
                    continue;
                Glyph aGlyph = aGlyphs.First();

                for (int b = FirstChar; b <= LastChar; b++)
                {
                    if (!Font.TryGetGlyphs(new CodePoint(b), out IReadOnlyList<Glyph>? bGlyphs) || bGlyphs.Count == 0)
                        continue;
                    Glyph bGlyph = bGlyphs.First();

                    Font.TryGetKerningOffset(bGlyph, aGlyph, DrawDpi, out Vector2 offset);
                    if (offset.X != 0 || offset.Y != 0)
                    {
                        kerningOffsets ??= new Vector2[CharCount, CharCount];
                        kerningOffsets[a - FirstChar, b - FirstChar] = offset;
                    }
                }
            }

            return kerningOffsets != null;
        }

        public System.Drawing.Point GetGlyphSize(int charCode)
        {
            return glyphSizes[charCode - FirstChar];
        }

        public Vector2[] GetRenderOffsets()
        {
            return renderOffsets;
        }

        public void DrawGlyphToImage(int charCode, System.Drawing.Point location, Image<Rgba32> image)
        {
            int charIndex = charCode - FirstChar;
            IPathCollection paths = glyphPaths[charIndex];
            paths = paths.Translate(location.X - renderOffsets[charIndex].X, location.Y - renderOffsets[charIndex].Y);
            DrawColoredPaths(image, paths, pathColors?[charIndex]);
        }

        /// <summary>
        /// Draws a collection of paths with the given colors onto the image.
        /// </summary>
        private void DrawColoredPaths(Image<Rgba32> image, IPathCollection paths, Color?[]? pathColors)
        {
            IEnumerator<IPath> pathEnumerator = paths.GetEnumerator();

            int i = 0;
            while (pathEnumerator.MoveNext())
            {
                IPath path = pathEnumerator.Current;
                Color color = (pathColors != null && i < pathColors.Length) ? pathColors[i] ?? DefaultGlyphColor : DefaultGlyphColor;
                image.Mutate(x => x.Fill(DrawingOptions, color, path));
                i++;
            }
        }

        public override string ToString()
        {
            return string.Concat(Font.FontMetrics.Description.FontNameInvariantCulture ?? "Unnamed " + nameof(FontGlyphSource),
                " - ", CharCount.ToString(), " characters");
        }
    }
}
