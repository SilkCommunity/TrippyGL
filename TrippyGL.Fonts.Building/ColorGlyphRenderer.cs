using System.Collections.Generic;
using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;

namespace TrippyGL.Fonts.Extensions
{
    /// <summary>
    /// Rendering surface that Fonts can use to generate Shapes.
    /// </summary>
    internal class ColorGlyphRenderer : IColorGlyphRenderer
    {
        protected readonly PathBuilder builder = new PathBuilder();

        private readonly List<IPath> paths = new List<IPath>();
        private readonly List<Color?> colors = new List<Color?>();

        private Vector2 currentPoint = default;
        private Color? currentColor = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorGlyphRenderer"/> class.
        /// </summary>
        public ColorGlyphRenderer()
        {
            builder = new PathBuilder();
        }

        /// <summary>
        /// Get the colors for each path, where null means use user provided brush.
        /// </summary>
        public Color?[] PathColors => colors.ToArray();

        /// <summary>
        /// Gets the paths that have been rendered by this.
        /// </summary>
        public IPathCollection Paths => new PathCollection(paths);

        void IGlyphRenderer.EndText() { }

        void IGlyphRenderer.BeginText(FontRectangle rect) => BeginText(rect);

        protected virtual void BeginText(FontRectangle rect) { }

        /// <summary>
        /// Begins the glyph.
        /// </summary>
        /// <param name="location">The offset that the glyph will be rendered at.</param>
        /// <param name="size">The size.</param>
        bool IGlyphRenderer.BeginGlyph(FontRectangle rect, GlyphRendererParameters cachKey)
        {
            currentColor = null;
            builder.Clear();
            return BeginGlyph(rect, cachKey);
        }

        protected virtual bool BeginGlyph(FontRectangle rect, GlyphRendererParameters cachKey)
        {
            BeginGlyph(rect);
            return true;
        }

        protected virtual void BeginGlyph(FontRectangle rect) { }

        /// <summary>
        /// Begins the figure.
        /// </summary>
        void IGlyphRenderer.BeginFigure()
        {
            builder.StartFigure();
        }

        /// <summary>
        /// Draws a cubic bezier from the current point  to the <paramref name="point"/>
        /// </summary>
        /// <param name="secondControlPoint">The second control point.</param>
        /// <param name="thirdControlPoint">The third control point.</param>
        /// <param name="point">The point.</param>
        void IGlyphRenderer.CubicBezierTo(Vector2 secondControlPoint, Vector2 thirdControlPoint, Vector2 point)
        {
            builder.AddBezier(currentPoint, secondControlPoint, thirdControlPoint, point);
            currentPoint = point;
        }

        /// <summary>
        /// Ends the glyph.
        /// </summary>
        void IGlyphRenderer.EndGlyph()
        {
            paths.Add(builder.Build());
            colors.Add(currentColor);
        }

        void IColorGlyphRenderer.SetColor(GlyphColor color)
        {
            currentColor = new Color(new Rgba32(color.Red, color.Green, color.Blue, color.Alpha));
        }

        /// <summary>
        /// Ends the figure.
        /// </summary>
        void IGlyphRenderer.EndFigure()
        {
            builder.CloseFigure();
        }

        /// <summary>
        /// Draws a line from the current point  to the <paramref name="point"/>.
        /// </summary>
        /// <param name="point">The point.</param>
        void IGlyphRenderer.LineTo(Vector2 point)
        {
            builder.AddLine(currentPoint, point);
            currentPoint = point;
        }

        /// <summary>
        /// Moves to current point to the supplied vector.
        /// </summary>
        /// <param name="point">The point.</param>
        void IGlyphRenderer.MoveTo(Vector2 point)
        {
            builder.StartFigure();
            currentPoint = point;
        }

        /// <summary>
        /// Draws a quadratics bezier from the current point  to the <paramref name="point"/>
        /// </summary>
        /// <param name="secondControlPoint">The second control point.</param>
        /// <param name="point">The point.</param>
        void IGlyphRenderer.QuadraticBezierTo(Vector2 secondControlPoint, Vector2 endPoint)
        {
            Vector2 startPointVector = currentPoint;
            Vector2 controlPointVector = secondControlPoint;
            Vector2 endPointVector = endPoint;

            Vector2 c1 = ((controlPointVector - startPointVector) * 2 / 3) + startPointVector;
            Vector2 c2 = ((controlPointVector - endPointVector) * 2 / 3) + endPointVector;

            builder.AddBezier(startPointVector, c1, c2, endPoint);
            currentPoint = endPoint;
        }

        public bool HasAnyPathColors()
        {
            for (int i = 0; i < colors.Count; i++)
                if (colors[i].HasValue)
                    return true;
            return false;
        }

        public void Reset(float x, float y)
        {
            builder.Reset();
            builder.SetOrigin(new PointF(x, y));
            paths.Clear();
            colors.Clear();
            currentPoint = default;
            currentColor = null;
        }

        public void Reset()
        {
            Reset(0, 0);
        }
    }
}
