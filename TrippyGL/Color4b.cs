using System;
using System.Numerics;
using System.Runtime.InteropServices;

#pragma warning disable CA1062 // Validate arguments of public methods

namespace TrippyGL
{
    /// <summary>
    /// A color with 4 unsigned byte components (R, G, B, A).
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Color4b : IEquatable<Color4b>
    {
        /// <summary>
        /// The red component of this <see cref="Color4b"/>.
        /// </summary>
        public byte R;

        /// <summary>
        /// The green component of this <see cref="Color4b"/>.
        /// </summary>
        public byte G;

        /// <summary>
        /// The blue component of this <see cref="Color4b"/>.
        /// </summary>
        public byte B;

        /// <summary>
        /// The alpha component of this <see cref="Color4b"/>.
        /// </summary>
        public byte A;

        /// <summary>Gets or sets this <see cref="Color4b"/> represented as a single <see cref="uint"/> value.</summary>
        /// <remarks>Red is stored in the least significant 8 bits, and Alpha in the most significant 8 bits.</remarks>
        public uint PackedValue
        {
            get => ((uint)A << 24) | ((uint)B << 16) | ((uint)G << 8) | R;
            set
            {
                R = (byte)(value & 255);
                G = (byte)((value >> 8) & 255);
                B = (byte)((value >> 16) & 255);
                A = (byte)((value >> 24) & 255);
            }
        }

        /// <summary>
        /// Constructs a <see cref="Color4b"/> structure from the specified byte values.
        /// </summary>
        public Color4b(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Constructs a <see cref="Color4b"/> from the specified float values,
        /// represented on a normalized range (from 0.0 to 1.0).
        /// </summary>
        public Color4b(float r, float g, float b, float a = 1)
        {
            R = (byte)(r * 255 + 0.5f);
            G = (byte)(g * 255 + 0.5f);
            B = (byte)(b * 255 + 0.5f);
            A = (byte)(a * 255 + 0.5f);
        }

        /// <summary>
        /// Constructs a <see cref="Color4b"/> from the specified float values in
        /// a <see cref="Vector4"/> represented on a normalized range (from 0.0 to 1.0).
        /// </summary>
        public Color4b(Vector4 rgba)
        {
            R = (byte)(rgba.X * 255 + 0.5f);
            G = (byte)(rgba.Y * 255 + 0.5f);
            B = (byte)(rgba.Z * 255 + 0.5f);
            A = (byte)(rgba.W * 255 + 0.5f);
        }

        /// <summary>
        /// Constructs a <see cref="Color4b"/> from the specified float values in
        /// a <see cref="Vector3"/> represented in a normalized range (from 0.0 to 1.0)
        /// and full alpha.
        /// </summary>
        public Color4b(Vector3 rgb)
        {
            R = (byte)(rgb.X * 255 + 0.5f);
            G = (byte)(rgb.Y * 255 + 0.5f);
            B = (byte)(rgb.Z * 255 + 0.5f);
            A = 255;
        }

        /// <summary>
        /// Constructs a <see cref="Color4b"/> from a packed value.
        /// </summary>
        public Color4b(uint packedValue)
        {
            R = (byte)(packedValue & 255);
            G = (byte)((packedValue >> 8) & 255);
            B = (byte)((packedValue >> 16) & 255);
            A = (byte)((packedValue >> 24) & 255);
        }

        /// <summary>
        /// Converts this <see cref="Color4b"/> into ARGB format.
        /// </summary>
        /// <remarks>
        /// Blue is stored in the 8 least significant bits, and Alpha in the 8 most significant bits.
        /// </remarks>
        public int ToArgb()
        {
            uint value = ((uint)A << 24) | ((uint)R << 16) | ((uint)G << 8) | B;
            return unchecked((int)value);
        }

        /// <summary>
        /// Converts this <see cref="Color4b"/> into a <see cref="Vector4"/> by normalizing
        /// the (R, G, B, A) components into a range [0-1].
        /// </summary>
        public Vector4 ToVector4()
        {
            return new Vector4(R / 255f, G / 255f, B / 255f, A / 255f);
        }

        public static bool operator ==(Color4b left, Color4b right) => left.Equals(right);
        public static bool operator !=(Color4b left, Color4b right) => !left.Equals(right);

        public static implicit operator Vector4(Color4b color) => color.ToVector4();
        public static implicit operator Color4b(Vector4 vector) => new Color4b(vector);

        /// <summary>
        /// Multiplies the RGB components of a <see cref="Color4b"/> by a scale, rounding to the nearest value.
        /// </summary>
        /// <param name="color">The <see cref="Color4b"/> to multiply.</param>
        /// <param name="scale">The scale to multiply by.</param>
        /// <seealso cref="MultiplyIncludeAlpha(Color4b, float)"/>
        public static Color4b Multiply(Color4b color, float scale)
        {
            return new Color4b((byte)(color.R * scale + 0.5f), (byte)(color.G * scale + 0.5f), (byte)(color.B * scale + 0.5f), color.A);
        }

        /// <summary>
        /// Multiplies the RGBA components of a <see cref="Color4b"/> by a scale, rounding to the nearest value.
        /// </summary>
        /// <param name="color">The <see cref="Color4b"/> to multiply.</param>
        /// <param name="scale">The scale to multiply by.</param>
        public static Color4b MultiplyIncludeAlpha(Color4b color, float scale)
        {
            return new Color4b((byte)(color.R * scale + 0.5f), (byte)(color.G * scale + 0.5f), (byte)(color.B * scale + 0.5f), (byte)(color.A * scale + 0.5f));
        }

        public override bool Equals(object obj)
        {
            if (obj is Color4b color4b)
                return Equals(color4b);
            return false;
        }

        public override int GetHashCode()
        {
            return ToArgb();
        }

        public override string ToString()
        {
            return string.Concat("(" + R.ToString() + ", " + G.ToString() + ", " + B.ToString() + ", " + A.ToString() + ")");
        }

        public bool Equals(Color4b other)
        {
            return
                R == other.R &&
                G == other.G &&
                B == other.B &&
                A == other.A;
        }

        /// <summary>
        /// Creates a <see cref="Color4b"/> with RGB values calculated from the given HSV values.
        /// </summary>
        public static Color4b FromHSV(float hue, float saturation, float value)
        {
            // Based on https://www.rapidtables.com/convert/color/hsv-to-rgb.html

            float c = value * saturation;
            float x = c * (1 - Math.Abs(hue * 6 % 2 - 1));
            float m = value - c;

            float r, g, b;
            if (hue < 0.0f)
            {
                r = 0;
                g = 0;
                b = 0;
            }
            else if (hue < 60.0f / 360.0f)
            {
                r = c;
                g = x;
                b = 0;
            }
            else if (hue < 120.0f / 360.0f)
            {
                r = x;
                g = c;
                b = 0;
            }
            else if (hue < 180.0f / 360.0f)
            {
                r = 0;
                g = c;
                b = x;
            }
            else if (hue < 240.0f / 360.0f)
            {
                r = 0;
                g = x;
                b = c;
            }
            else if (hue < 300.0f / 360.0f)
            {
                r = x;
                g = 0;
                b = c;
            }
            else if (hue <= 1f)
            {
                r = c;
                g = 0;
                b = x;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }

            return new Color4b(r + m, g + m, b + m);
        }

        /// <summary>
        /// Creates a <see cref="Color4b"/> with RGB values calculated from the given HSV values.
        /// </summary>
        public static Color4b FromHSV(in Vector3 values) => FromHSV(values.X, values.Y, values.Z);

        /// <summary>
        /// Constructs a completely randomized <see cref="Color4b"/>.
        /// </summary>
        /// <param name="random">The <see cref="System.Random"/> to use for randomizing.</param>
        public static Color4b Random(Random random)
        {
            // A single random value isn't enough, it's only 31 bits...
            // So we use one for RGB and an extra one for alpha.
            uint val = (uint)random.Next();
            return new Color4b((byte)(val & 255), (byte)((val >> 8) & 255), (byte)((val >> 16) & 255), (byte)(random.Next() & 255));
        }

        /// <summary>
        /// Constructs a randomized <see cref="Color4b"/> with an alpha value of 255.
        /// </summary>
        /// <param name="random">The <see cref="System.Random"/> to use for randomizing.</param>
        public static Color4b RandomFullAlpha(Random random)
        {
            uint val = (uint)random.Next();
            Color4b color = new Color4b((byte)(val & 255), (byte)((val >> 8) & 255), (byte)((val >> 16) & 255));
            return color;
        }

        #region Colors
        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 255, 255, 0).
        /// </summary>
        public static Color4b Transparent => new Color4b(255, 255, 255, 0);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (240, 248, 255, 255).
        /// </summary>
        public static Color4b AliceBlue => new Color4b(240, 248, 255, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (250, 235, 215, 255).
        /// </summary>
        public static Color4b AntiqueWhite => new Color4b(250, 235, 215, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 255, 255, 255).
        /// </summary>
        public static Color4b Aqua => new Color4b(0, 255, 255, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (127, 255, 212, 255).
        /// </summary>
        public static Color4b Aquamarine => new Color4b(127, 255, 212, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (240, 255, 255, 255).
        /// </summary>
        public static Color4b Azure => new Color4b(240, 255, 255, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (245, 245, 220, 255).
        /// </summary>
        public static Color4b Beige => new Color4b(245, 245, 220, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 228, 196, 255).
        /// </summary>
        public static Color4b Bisque => new Color4b(255, 228, 196, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 0, 0, 255).
        /// </summary>
        public static Color4b Black => new Color4b(0, 0, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 235, 205, 255).
        /// </summary>
        public static Color4b BlanchedAlmond => new Color4b(255, 235, 205, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 0, 255, 255).
        /// </summary>
        public static Color4b Blue => new Color4b(0, 0, 255, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (138, 43, 226, 255).
        /// </summary>
        public static Color4b BlueViolet => new Color4b(138, 43, 226, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (165, 42, 42, 255).
        /// </summary>
        public static Color4b Brown => new Color4b(165, 42, 42, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (222, 184, 135, 255).
        /// </summary>
        public static Color4b BurlyWood => new Color4b(222, 184, 135, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (95, 158, 160, 255).
        /// </summary>
        public static Color4b CadetBlue => new Color4b(95, 158, 160, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (127, 255, 0, 255).
        /// </summary>
        public static Color4b Chartreuse => new Color4b(127, 255, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (210, 105, 30, 255).
        /// </summary>
        public static Color4b Chocolate => new Color4b(210, 105, 30, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 127, 80, 255).
        /// </summary>
        public static Color4b Coral => new Color4b(255, 127, 80, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (100, 149, 237, 255).
        /// </summary>
        public static Color4b CornflowerBlue => new Color4b(100, 149, 237, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 248, 220, 255).
        /// </summary>
        public static Color4b Cornsilk => new Color4b(255, 248, 220, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (220, 20, 60, 255).
        /// </summary>
        public static Color4b Crimson => new Color4b(220, 20, 60, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 255, 255, 255).
        /// </summary>
        public static Color4b Cyan => new Color4b(0, 255, 255, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 0, 139, 255).
        /// </summary>
        public static Color4b DarkBlue => new Color4b(0, 0, 139, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 139, 139, 255).
        /// </summary>
        public static Color4b DarkCyan => new Color4b(0, 139, 139, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (184, 134, 11, 255).
        /// </summary>
        public static Color4b DarkGoldenrod => new Color4b(184, 134, 11, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (169, 169, 169, 255).
        /// </summary>
        public static Color4b DarkGray => new Color4b(169, 169, 169, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 100, 0, 255).
        /// </summary>
        public static Color4b DarkGreen => new Color4b(0, 100, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (189, 183, 107, 255).
        /// </summary>
        public static Color4b DarkKhaki => new Color4b(189, 183, 107, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (139, 0, 139, 255).
        /// </summary>
        public static Color4b DarkMagenta => new Color4b(139, 0, 139, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (85, 107, 47, 255).
        /// </summary>
        public static Color4b DarkOliveGreen => new Color4b(85, 107, 47, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 140, 0, 255).
        /// </summary>
        public static Color4b DarkOrange => new Color4b(255, 140, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (153, 50, 204, 255).
        /// </summary>
        public static Color4b DarkOrchid => new Color4b(153, 50, 204, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (139, 0, 0, 255).
        /// </summary>
        public static Color4b DarkRed => new Color4b(139, 0, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (233, 150, 122, 255).
        /// </summary>
        public static Color4b DarkSalmon => new Color4b(233, 150, 122, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (143, 188, 139, 255).
        /// </summary>
        public static Color4b DarkSeaGreen => new Color4b(143, 188, 139, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (72, 61, 139, 255).
        /// </summary>
        public static Color4b DarkSlateBlue => new Color4b(72, 61, 139, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (47, 79, 79, 255).
        /// </summary>
        public static Color4b DarkSlateGray => new Color4b(47, 79, 79, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 206, 209, 255).
        /// </summary>
        public static Color4b DarkTurquoise => new Color4b(0, 206, 209, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (148, 0, 211, 255).
        /// </summary>
        public static Color4b DarkViolet => new Color4b(148, 0, 211, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 20, 147, 255).
        /// </summary>
        public static Color4b DeepPink => new Color4b(255, 20, 147, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 191, 255, 255).
        /// </summary>
        public static Color4b DeepSkyBlue => new Color4b(0, 191, 255, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (105, 105, 105, 255).
        /// </summary>
        public static Color4b DimGray => new Color4b(105, 105, 105, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (30, 144, 255, 255).
        /// </summary>
        public static Color4b DodgerBlue => new Color4b(30, 144, 255, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (178, 34, 34, 255).
        /// </summary>
        public static Color4b Firebrick => new Color4b(178, 34, 34, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 250, 240, 255).
        /// </summary>
        public static Color4b FloralWhite => new Color4b(255, 250, 240, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (34, 139, 34, 255).
        /// </summary>
        public static Color4b ForestGreen => new Color4b(34, 139, 34, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 0, 255, 255).
        /// </summary>
        public static Color4b Fuchsia => new Color4b(255, 0, 255, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (220, 220, 220, 255).
        /// </summary>
        public static Color4b Gainsboro => new Color4b(220, 220, 220, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (248, 248, 255, 255).
        /// </summary>
        public static Color4b GhostWhite => new Color4b(248, 248, 255, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 215, 0, 255).
        /// </summary>
        public static Color4b Gold => new Color4b(255, 215, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (218, 165, 32, 255).
        /// </summary>
        public static Color4b Goldenrod => new Color4b(218, 165, 32, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (128, 128, 128, 255).
        /// </summary>
        public static Color4b Gray => new Color4b(128, 128, 128, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 128, 0, 255).
        /// </summary>
        public static Color4b Green => new Color4b(0, 128, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (173, 255, 47, 255).
        /// </summary>
        public static Color4b GreenYellow => new Color4b(173, 255, 47, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (240, 255, 240, 255).
        /// </summary>
        public static Color4b Honeydew => new Color4b(240, 255, 240, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 105, 180, 255).
        /// </summary>
        public static Color4b HotPink => new Color4b(255, 105, 180, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (205, 92, 92, 255).
        /// </summary>
        public static Color4b IndianRed => new Color4b(205, 92, 92, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (75, 0, 130, 255).
        /// </summary>
        public static Color4b Indigo => new Color4b(75, 0, 130, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 255, 240, 255).
        /// </summary>
        public static Color4b Ivory => new Color4b(255, 255, 240, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (240, 230, 140, 255).
        /// </summary>
        public static Color4b Khaki => new Color4b(240, 230, 140, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (230, 230, 250, 255).
        /// </summary>
        public static Color4b Lavender => new Color4b(230, 230, 250, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 240, 245, 255).
        /// </summary>
        public static Color4b LavenderBlush => new Color4b(255, 240, 245, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (124, 252, 0, 255).
        /// </summary>
        public static Color4b LawnGreen => new Color4b(124, 252, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 250, 205, 255).
        /// </summary>
        public static Color4b LemonChiffon => new Color4b(255, 250, 205, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (173, 216, 230, 255).
        /// </summary>
        public static Color4b LightBlue => new Color4b(173, 216, 230, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (240, 128, 128, 255).
        /// </summary>
        public static Color4b LightCoral => new Color4b(240, 128, 128, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (224, 255, 255, 255).
        /// </summary>
        public static Color4b LightCyan => new Color4b(224, 255, 255, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (250, 250, 210, 255).
        /// </summary>
        public static Color4b LightGoldenrodYellow => new Color4b(250, 250, 210, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (144, 238, 144, 255).
        /// </summary>
        public static Color4b LightGreen => new Color4b(144, 238, 144, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (211, 211, 211, 255).
        /// </summary>
        public static Color4b LightGray => new Color4b(211, 211, 211, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 182, 193, 255).
        /// </summary>
        public static Color4b LightPink => new Color4b(255, 182, 193, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 160, 122, 255).
        /// </summary>
        public static Color4b LightSalmon => new Color4b(255, 160, 122, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (32, 178, 170, 255).
        /// </summary>
        public static Color4b LightSeaGreen => new Color4b(32, 178, 170, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (135, 206, 250, 255).
        /// </summary>
        public static Color4b LightSkyBlue => new Color4b(135, 206, 250, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (119, 136, 153, 255).
        /// </summary>
        public static Color4b LightSlateGray => new Color4b(119, 136, 153, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (176, 196, 222, 255).
        /// </summary>
        public static Color4b LightSteelBlue => new Color4b(176, 196, 222, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 255, 224, 255).
        /// </summary>
        public static Color4b LightYellow => new Color4b(255, 255, 224, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 255, 0, 255).
        /// </summary>
        public static Color4b Lime => new Color4b(0, 255, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (50, 205, 50, 255).
        /// </summary>
        public static Color4b LimeGreen => new Color4b(50, 205, 50, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (250, 240, 230, 255).
        /// </summary>
        public static Color4b Linen => new Color4b(250, 240, 230, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 0, 255, 255).
        /// </summary>
        public static Color4b Magenta => new Color4b(255, 0, 255, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (128, 0, 0, 255).
        /// </summary>
        public static Color4b Maroon => new Color4b(128, 0, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (102, 205, 170, 255).
        /// </summary>
        public static Color4b MediumAquamarine => new Color4b(102, 205, 170, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 0, 205, 255).
        /// </summary>
        public static Color4b MediumBlue => new Color4b(0, 0, 205, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (186, 85, 211, 255).
        /// </summary>
        public static Color4b MediumOrchid => new Color4b(186, 85, 211, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (147, 112, 219, 255).
        /// </summary>
        public static Color4b MediumPurple => new Color4b(147, 112, 219, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (60, 179, 113, 255).
        /// </summary>
        public static Color4b MediumSeaGreen => new Color4b(60, 179, 113, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (123, 104, 238, 255).
        /// </summary>
        public static Color4b MediumSlateBlue => new Color4b(123, 104, 238, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 250, 154, 255).
        /// </summary>
        public static Color4b MediumSpringGreen => new Color4b(0, 250, 154, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (72, 209, 204, 255).
        /// </summary>
        public static Color4b MediumTurquoise => new Color4b(72, 209, 204, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (199, 21, 133, 255).
        /// </summary>
        public static Color4b MediumVioletRed => new Color4b(199, 21, 133, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (25, 25, 112, 255).
        /// </summary>
        public static Color4b MidnightBlue => new Color4b(25, 25, 112, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (245, 255, 250, 255).
        /// </summary>
        public static Color4b MintCream => new Color4b(245, 255, 250, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 228, 225, 255).
        /// </summary>
        public static Color4b MistyRose => new Color4b(255, 228, 225, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 228, 181, 255).
        /// </summary>
        public static Color4b Moccasin => new Color4b(255, 228, 181, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 222, 173, 255).
        /// </summary>
        public static Color4b NavajoWhite => new Color4b(255, 222, 173, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 0, 128, 255).
        /// </summary>
        public static Color4b Navy => new Color4b(0, 0, 128, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (253, 245, 230, 255).
        /// </summary>
        public static Color4b OldLace => new Color4b(253, 245, 230, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (128, 128, 0, 255).
        /// </summary>
        public static Color4b Olive => new Color4b(128, 128, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (107, 142, 35, 255).
        /// </summary>
        public static Color4b OliveDrab => new Color4b(107, 142, 35, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 165, 0, 255).
        /// </summary>
        public static Color4b Orange => new Color4b(255, 165, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 69, 0, 255).
        /// </summary>
        public static Color4b OrangeRed => new Color4b(255, 69, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (218, 112, 214, 255).
        /// </summary>
        public static Color4b Orchid => new Color4b(218, 112, 214, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (238, 232, 170, 255).
        /// </summary>
        public static Color4b PaleGoldenrod => new Color4b(238, 232, 170, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (152, 251, 152, 255).
        /// </summary>
        public static Color4b PaleGreen => new Color4b(152, 251, 152, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (175, 238, 238, 255).
        /// </summary>
        public static Color4b PaleTurquoise => new Color4b(175, 238, 238, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (219, 112, 147, 255).
        /// </summary>
        public static Color4b PaleVioletRed => new Color4b(219, 112, 147, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 239, 213, 255).
        /// </summary>
        public static Color4b PapayaWhip => new Color4b(255, 239, 213, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 218, 185, 255).
        /// </summary>
        public static Color4b PeachPuff => new Color4b(255, 218, 185, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (205, 133, 63, 255).
        /// </summary>
        public static Color4b Peru => new Color4b(205, 133, 63, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 192, 203, 255).
        /// </summary>
        public static Color4b Pink => new Color4b(255, 192, 203, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (221, 160, 221, 255).
        /// </summary>
        public static Color4b Plum => new Color4b(221, 160, 221, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (176, 224, 230, 255).
        /// </summary>
        public static Color4b PowderBlue => new Color4b(176, 224, 230, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (128, 0, 128, 255).
        /// </summary>
        public static Color4b Purple => new Color4b(128, 0, 128, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 0, 0, 255).
        /// </summary>
        public static Color4b Red => new Color4b(255, 0, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (188, 143, 143, 255).
        /// </summary>
        public static Color4b RosyBrown => new Color4b(188, 143, 143, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (65, 105, 225, 255).
        /// </summary>
        public static Color4b RoyalBlue => new Color4b(65, 105, 225, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (139, 69, 19, 255).
        /// </summary>
        public static Color4b SaddleBrown => new Color4b(139, 69, 19, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (250, 128, 114, 255).
        /// </summary>
        public static Color4b Salmon => new Color4b(250, 128, 114, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (244, 164, 96, 255).
        /// </summary>
        public static Color4b SandyBrown => new Color4b(244, 164, 96, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (46, 139, 87, 255).
        /// </summary>
        public static Color4b SeaGreen => new Color4b(46, 139, 87, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 245, 238, 255).
        /// </summary>
        public static Color4b SeaShell => new Color4b(255, 245, 238, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (160, 82, 45, 255).
        /// </summary>
        public static Color4b Sienna => new Color4b(160, 82, 45, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (192, 192, 192, 255).
        /// </summary>
        public static Color4b Silver => new Color4b(192, 192, 192, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (135, 206, 235, 255).
        /// </summary>
        public static Color4b SkyBlue => new Color4b(135, 206, 235, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (106, 90, 205, 255).
        /// </summary>
        public static Color4b SlateBlue => new Color4b(106, 90, 205, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (112, 128, 144, 255).
        /// </summary>
        public static Color4b SlateGray => new Color4b(112, 128, 144, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 250, 250, 255).
        /// </summary>
        public static Color4b Snow => new Color4b(255, 250, 250, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 255, 127, 255).
        /// </summary>
        public static Color4b SpringGreen => new Color4b(0, 255, 127, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (70, 130, 180, 255).
        /// </summary>
        public static Color4b SteelBlue => new Color4b(70, 130, 180, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (210, 180, 140, 255).
        /// </summary>
        public static Color4b Tan => new Color4b(210, 180, 140, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (0, 128, 128, 255).
        /// </summary>
        public static Color4b Teal => new Color4b(0, 128, 128, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (216, 191, 216, 255).
        /// </summary>
        public static Color4b Thistle => new Color4b(216, 191, 216, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 99, 71, 255).
        /// </summary>
        public static Color4b Tomato => new Color4b(255, 99, 71, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (64, 224, 208, 255).
        /// </summary>
        public static Color4b Turquoise => new Color4b(64, 224, 208, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (238, 130, 238, 255).
        /// </summary>
        public static Color4b Violet => new Color4b(238, 130, 238, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (245, 222, 179, 255).
        /// </summary>
        public static Color4b Wheat => new Color4b(245, 222, 179, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 255, 255, 255).
        /// </summary>
        public static Color4b White => new Color4b(255, 255, 255, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (245, 245, 245, 255).
        /// </summary>
        public static Color4b WhiteSmoke => new Color4b(245, 245, 245, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (255, 255, 0, 255).
        /// </summary>
        public static Color4b Yellow => new Color4b(255, 255, 0, 255);

        /// <summary>
        /// Gets a <see cref="Color4b"/> with values (154, 205, 50, 255).
        /// </summary>
        public static Color4b YellowGreen => new Color4b(154, 205, 50, 255);
        #endregion
    }
}
