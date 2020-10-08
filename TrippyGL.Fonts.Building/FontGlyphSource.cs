using System;
using System.Collections.Generic;
using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TrippyGL.Fonts.Extensions
{
    public class FontGlyphSource : IGlyphSource
    {
        private const float DrawDpi = 96;
        private const float CalcDpi = 72;

        public readonly IFontInstance FontInstance;

        private readonly IPathCollection[] glyphPaths;
        private readonly Color?[][] pathColors;

        public ShapeGraphicsOptions ShapeGraphicsOptions;

        private readonly System.Drawing.Point[] glyphSizes;

        private readonly Vector2[] renderOffsets;

        public bool IncludeKerningIfPresent = true;

        public char FirstChar { get; }

        public char LastChar { get; }

        public float Size { get; }

        public string Name { get; }

        public float Ascender => Size * (DrawDpi / CalcDpi) * FontInstance.Ascender / FontInstance.EmSize;

        public float Descender => Size * (DrawDpi / CalcDpi) * FontInstance.Descender / FontInstance.EmSize;

        public float LineGap => Size * (DrawDpi / CalcDpi) * FontInstance.LineGap / FontInstance.EmSize;

        public int CharCount => LastChar - FirstChar + 1;

        public FontGlyphSource(IFontInstance fontInstance, float size, string name, char firstChar = ' ', char lastChar = '~')
        {
            if (!float.IsFinite(size) || float.IsNegative(size))
                throw new ArgumentOutOfRangeException(nameof(size), size, "Size must be finite and positive.");

            if (lastChar < firstChar)
                throw new ArgumentException("LastChar can't be lower than FirstChar");

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

        public FontGlyphSource(IFontInstance fontInstance, float size, char firstChar = ' ', char lastChar = '~')
            : this(fontInstance, size, fontInstance.Description.FontNameInvariantCulture, firstChar, lastChar) { }

        public FontGlyphSource(Font font, char firstChar = ' ', char lastChar = '~')
            : this(font.Instance, font.Size, font.Name, firstChar, lastChar) { }

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

        private void DrawColoredPaths(Image<Rgba32> image, IPathCollection paths, Color?[] pathColors)
        {
            IEnumerator<IPath> pathEnumerator = paths.GetEnumerator();

            IPath path;
            Color color;

            int i = 0;
            while (pathEnumerator.MoveNext())
            {
                path = pathEnumerator.Current;
                color = (pathColors != null && i < pathColors.Length && pathColors[i].HasValue) ? pathColors[i].Value : Color.White;
                image.Mutate(x => x.Fill(ShapeGraphicsOptions, color, path));
            }
        }
    }
}
