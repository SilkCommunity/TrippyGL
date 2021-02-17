using System;
using System.Drawing;

namespace TrippyGL
{
    /// <summary>
    /// A rectangle used to define drawing areas.
    /// </summary>
    public struct Viewport : IEquatable<Viewport>
    {
        /// <summary>The X coordinate of the first pixel inside this <see cref="Viewport"/>.</summary>
        public int X;

        /// <summary>The Y coordinate of the first pixel inside this <see cref="Viewport"/>.</summary>
        public int Y;

        /// <summary>The width of this <see cref="Viewport"/> measured in pixels.</summary>
        public uint Width;

        /// <summary>The height of this <see cref="Viewport"/> measured in pixels.</summary>
        public uint Height;

        /// <summary>
        /// Gets or sets the value of <see cref="X"/> + <see cref="Width"/>. Setting this will only modify the <see cref="Width"/>.
        /// </summary>
        public int Right
        {
            get => X + (int)Width;
            set => Width = (uint)(value - X);
        }

        /// <summary>
        /// Gets or sets the value of <see cref="Y"/> + <see cref="Height"/>. Setting this will only modify the <see cref="Height"/>.
        /// </summary>
        public int Bottom
        {
            get => Y + (int)Height;
            set => Height = (uint)(value - Y);
        }

        /// <summary>
        /// Creates a <see cref="Viewport"/> with the specified parameters.
        /// </summary>
        public Viewport(int x, int y, uint width, uint height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Creates a <see cref="Viewport"/> from a <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="rectangle"></param>
        public Viewport(in Rectangle rectangle)
        {
            X = rectangle.X;
            Y = rectangle.Y;
            Width = (uint)rectangle.Width;
            Height = (uint)rectangle.Height;
        }

        /// <summary>
        /// Creates a <see cref="Viewport"/> with the specified position and size.
        /// </summary>
        public Viewport(Point position, Size size)
        {
            X = position.X;
            Y = position.Y;
            Width = (uint)size.Width;
            Height = (uint)size.Height;
        }

        public static bool operator ==(Viewport left, Viewport right) => left.Equals(right);
        public static bool operator !=(Viewport left, Viewport right) => !left.Equals(right);

        public static implicit operator Rectangle(Viewport viewport) => new Rectangle(viewport.X, viewport.Y, (int)viewport.Width, (int)viewport.Height);
        public static implicit operator Viewport(Rectangle rectangle) => new Viewport(rectangle);

        /// <summary>
        /// Returns whether another <see cref="Viewport"/> is enclosed fully inside this <see cref="Viewport"/>.
        /// </summary>
        public bool Contains(in Viewport other)
        {
            return X <= other.X && Y <= other.Y && Right >= other.Right && Bottom >= other.Bottom;
        }

        /// <summary>
        /// Returns whether another <see cref="Viewport"/> intersects at least one pixel with this <see cref="Viewport"/>.
        /// </summary>
        public bool Intersects(in Viewport other)
        {
            return other.X < X + Width && X < (other.X + other.Width)
                && other.Y < Y + Height && Y < other.Y + other.Height;
        }

        /// <summary>
        /// Returns a <see cref="Viewport"/> whose area is the intersection between this and another <see cref="Viewport"/>.
        /// </summary>
        public Viewport Intersection(in Viewport other)
        {
            int x1 = Math.Max(X, other.X);
            int x2 = Math.Min(Right, other.Right);
            int y1 = Math.Max(Y, other.Y);
            int y2 = Math.Min(Bottom, other.Bottom);

            if (x2 >= x1 && y2 >= y1)
                return new Viewport(x1, y1, (uint)(x2 - x1), (uint)(y2 - y1));
            return default;
        }

        public override string ToString()
        {
            return string.Concat(
                nameof(X) + "=", X.ToString(),
                ", " + nameof(Y) + "=", Y.ToString(),
                ", " + nameof(Width) + "=" + Width.ToString(),
                ", " + nameof(Height) + "=", Height.ToString()
            );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height);
        }

        public bool Equals(Viewport other)
        {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            if (obj is Viewport viewport)
                return Equals(viewport);
            return false;
        }
    }
}
