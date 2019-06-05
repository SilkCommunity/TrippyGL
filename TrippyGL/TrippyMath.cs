using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Provides some general help math-related functions and values
    /// </summary>
    public static class TrippyMath
    {
        public static int IntegerPow(int a, int b)
        {
            int r = 1;
            while (b > 0)
            {
                if ((b & 1) == 1)
                    r *= a;
                b >>= 1;
                a *= a;
            }
            return r;
        }

        public static long IntegerLongPow(long a, long b)
        {
            long r = 1;
            while (b > 0)
            {
                if ((b & 1) == 1)
                    r *= a;
                b >>= 1;
                a *= a;
            }
            return r;
        }
    }
}
