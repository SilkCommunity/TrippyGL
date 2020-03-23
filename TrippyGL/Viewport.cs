using System;
using System.Collections.Generic;
using System.Text;

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
            get { return X + (int)Width; }
            set { Width = (uint)(value - X); }
        }

        /// <summary>
        /// Gets or sets the value of <see cref="Y"/> + <see cref="Height"/>. Setting this will only modify the <see cref="Height"/>.
        /// </summary>
        public int Bottom
        {
            get { return Y + (int)Height; }
            set { Height = (uint)(value - Y); }
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

        public static bool operator ==(Viewport left, Viewport right) => left.Equals(right);

        public static bool operator !=(Viewport left, Viewport right) => !left.Equals(right);

        /// <summary>
        /// Returns whether another <see cref="Viewport"/> is enclosed fully inside this <see cref="Viewport"/>.
        /// </summary>
        public bool Contains(Viewport other)
        {
            return X <= other.X && Y <= other.Y && Right >= other.Right && Bottom >= other.Bottom;
        }

        /// <summary>
        /// Returns whether another <see cref="Viewport"/> intersects at least one pixel with this <see cref="Viewport"/>.
        /// </summary>
        public bool Intersects(Viewport other)
        {
            return other.X < X + Width && X < (other.X + other.Width)
                && other.Y < Y + Height && Y < other.Y + other.Height;
        }

        /// <summary>
        /// Returns a <see cref="Viewport"/> whose area is the intersection between this and another <see cref="Viewport"/>.
        /// </summary>
        public Viewport Intersection(Viewport other)
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
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Width.GetHashCode();
                hashCode = (hashCode * 397) ^ Height.GetHashCode();
                return hashCode;
            }
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
