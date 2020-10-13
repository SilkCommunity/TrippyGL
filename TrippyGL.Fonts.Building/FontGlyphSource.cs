using System;
using System.Collections.Generic;
using System.Numerics;
using SixLabors.Fonts;
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

        /// <summary>The <see cref="IFontInstance"/> from which this <see cref="FontGlyphSource"/> gets glyph data.</summary>
        public readonly IFontInstance FontInstance;

        /// <summary>The path collections that make up each character.</summary>
        private readonly IPathCollection[] glyphPaths;

        /// <summary>The colors of the paths that make up each character. Might be null.</summary>
        private readonly Color?[][] pathColors;

        /// <summary>Configuration for how glyphs should be rendered.</summary>
        public ShapeGraphicsOptions ShapeGraphicsOptions;

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

        public float Size { get; }

        public string Name { get; }

        public float Ascender => Size * (DrawDpi / CalcDpi) * FontInstance.Ascender / FontInstance.EmSize;

        public float Descender => Size * (DrawDpi / CalcDpi) * FontInstance.Descender / FontInstance.EmSize;

        public float LineGap => Size * (DrawDpi / CalcDpi) * FontInstance.LineGap / FontInstance.EmSize;

        public int CharCount => LastChar - FirstChar + 1;

        /// <summary>
        /// Creates a <see cref="FontGlyphSource"/> instance.
        /// </summary>
        public FontGlyphSource(IFontInstance fontInstance, float size, string name, char firstChar = ' ', char lastChar = '~')
        {
            if (!float.IsFinite(size) || float.IsNegative(size))
                throw new ArgumentOutOfRangeException(nameof(size), size, nameof(size) + " must be finite and positive.");

            if (lastChar < firstChar)
                throw new ArgumentException(nameof(LastChar) + " can't be lower than " + nameof(firstChar));

            FontInstance = fontInstance ?? throw new ArgumentNullException(nameof(fontInstance));

            FirstChar = firstChar;
            LastChar = lastChar;
            Size = size;
            Name = name;

            glyphPaths = CreatePaths(out pathColors, out glyphSizes, out renderOffsets);

            ShapeGraphicsOptions = new ShapeGraphicsOptions
            {
                ShapeOptions = { IntersectionRule = IntersectionRule.Nonzero },
            };
        }

        /// <summary>
        /// Creates a <see cref="FontGlyphSource"/> instance.
        /// </summary>
        public FontGlyphSource(IFontInstance fontInstance, float size, char firstChar = ' ', char lastChar = '~')
            : this(fontInstance, size, fontInstance.Description.FontNameInvariantCulture, firstChar, lastChar) { }

        /// <summary>
        /// Creates a <see cref="FontGlyphSource"/> instance.
        /// </summary>
        public FontGlyphSource(Font font, char firstChar = ' ', char lastChar = '~')
            : this(font.Instance, font.Size, font.Name, firstChar, lastChar) { }

        /// <summary>
        /// Creates the <see cref="IPathCollection"/> for all the characters, also getting their colors,
        /// glyph sizes and render offsets.
        /// </summary>
        private IPathCollection[] CreatePaths(out Color?[][] colors, out System.Drawing.Point[] sizes, out Vector2[] offsets)
        {
            float glyphRenderY = Size / CalcDpi * FontInstance.Ascender / FontInstance.EmSize;
            ColorGlyphRenderer glyphRenderer = new ColorGlyphRenderer();

            IPathCollection[] paths = new IPathCollection[CharCount];
            sizes = new System.Drawing.Point[paths.Length];
            offsets = new Vector2[paths.Length];
            colors = null;

            for (int i = 0; i < paths.Length; i++)
            {
                char c = (char)(i + FirstChar);
                glyphRenderer.Reset();
                GlyphInstance glyphInstance = FontInstance.GetGlyph(c);
                glyphInstance.RenderTo(glyphRenderer, Size, new Vector2(0, glyphRenderY), new Vector2(DrawDpi, DrawDpi), 0);
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
                    if (colors == null)
                        colors = new Color?[CharCount][];

                    colors[i] = glyphRenderer.PathColors;
                }
            }

            return paths;
        }

        public bool GetAdvances(out float[] advances)
        {
            advances = null;

            GlyphInstance firstInstance = FontInstance.GetGlyph(FirstChar);
            float adv = firstInstance.AdvanceWidth * Size * (DrawDpi / CalcDpi) / firstInstance.SizeOfEm;

            for (int i = FirstChar + 1; i <= LastChar; i++)
            {
                GlyphInstance inst = FontInstance.GetGlyph(i);
                float iAdv = inst.AdvanceWidth * Size * (DrawDpi / CalcDpi) / inst.SizeOfEm;

                if (advances == null)
                {
                    if (iAdv != adv)
                    {
                        advances = new float[CharCount];
                        for (int c = 0; c < i - FirstChar; c++)
                            advances[c] = adv;
                        advances[i - FirstChar] = iAdv;
                    }
                }
                else
                    advances[i - FirstChar] = iAdv;
            }

            if (advances == null)
            {
                advances = new float[1] { adv };
                return false;
            }

            return true;
        }

        public bool TryGetKerning(out Vector2[,] kerningOffsets)
        {
            kerningOffsets = null;
            if (!IncludeKerningIfPresent)
                return false;

            for (int a = FirstChar; a <= LastChar; a++)
            {
                GlyphInstance aInstance = FontInstance.GetGlyph(a);
                for (int b = FirstChar; b <= LastChar; b++)
                {
                    Vector2 offset = FontInstance.GetOffset(FontInstance.GetGlyph(b), aInstance);
                    if (offset.X != 0 || offset.Y != 0)
                    {
                        if (kerningOffsets == null)
                            kerningOffsets = new Vector2[CharCount, CharCount];
                        kerningOffsets[a - FirstChar, b - FirstChar] = offset * Size * (DrawDpi / CalcDpi) / FontInstance.EmSize;
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
        private void DrawColoredPaths(Image<Rgba32> image, IPathCollection paths, Color?[] pathColors)
        {
            IEnumerator<IPath> pathEnumerator = paths.GetEnumerator();

            int i = 0;
            while (pathEnumerator.MoveNext())
            {
                IPath path = pathEnumerator.Current;
                Color color = (pathColors != null && i < pathColors.Length && pathColors[i].HasValue) ? pathColors[i].Value : DefaultGlyphColor;
                image.Mutate(x => x.Fill(ShapeGraphicsOptions, color, path));
                i++;
            }
        }

        public override string ToString()
        {
            return string.Concat(FontInstance?.Description?.FontNameInvariantCulture ?? "Unnamed " + nameof(FontGlyphSource),
                " - ", CharCount.ToString(), " characters");
        }
    }
}
