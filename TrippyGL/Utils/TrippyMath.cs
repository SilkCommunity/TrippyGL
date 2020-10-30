using System;

#pragma warning disable CA1062 // Validate arguments of public methods

namespace TrippyGL.Utils
{
    /// <summary>
    /// Provides some general help math-related functions and values.
    /// </summary>
    public static class TrippyMath
    {
        /// <summary>The value of PI divided by 2.</summary>
        public const float PiOver2 = 1.5707963267948966192313216916398f;

        /// <summary>The value of PI divided by 4.</summary>
        public const float PiOver4 = 0.78539816339744830961566084581988f;

        /// <summary>The value of PI multiplied by 3 and divided by 2.</summary>
        public const float ThreePiOver2 = 4.7123889803846898576939650749193f;

        /// <summary>The value of PI multiplied by 2.</summary>
        public const float TwoPI = 6.283185307179586476925286766559f;

        /// <summary>
        /// Interpolates linearly between two values.
        /// </summary>
        /// <param name="min">The initial value in the interpolation.</param>
        /// <param name="max">The final value in the interpolation.</param>
        /// <param name="amount">The amount of interpolation, measured between 0 and 1.</param>
        public static float Lerp(float min, float max, float amount)
        {
            return min + (max - min) * amount;
        }

        /// <summary>
        /// Interpolates linearly between two values.
        /// </summary>
        /// <param name="min">The initial value in the interpolation.</param>
        /// <param name="max">The final value in the interpolation.</param>
        /// <param name="amount">The amount of interpolation, measured between 0 and 1.</param>
        /// <remarks>
        /// In comparison to <see cref="Lerp(float, float, float)"/>, this function is more
        /// precise when working with big values.
        /// </remarks>
        public static float LerpPrecise(float min, float max, float amount)
        {
            return (1 - amount) * min + max * amount;
        }

        /// <summary>
        /// Interpolates between two values using a cubic equation.
        /// </summary>
        /// <param name="min">The initial value in the interpolation.</param>
        /// <param name="max">The final value in the interpolation.</param>
        /// <param name="amount">The amount of interpolation, measured between 0 and 1.</param>
        public static float SmoothStep(float min, float max, float amount)
        {
            // Lerp using the polynomial: 3xx - 2xxx
            return Lerp(min, max, (3 - 2 * amount) * amount * amount);
        }

        /// <summary>
        /// Interpolates between two values using a cubic equation.
        /// </summary>
        /// <param name="min">The initial value in the interpolation.</param>
        /// <param name="max">The final value in the interpolation.</param>
        /// <param name="amount">The amount of interpolation, measured between 0 and 1.</param>
        /// <remarks>
        /// In comparison to <see cref="SmoothStep(float, float, float)"/>, this function is more
        /// precise when working with big values.
        /// </remarks>
        public static float SmoothStepPrecise(float min, float max, float amount)
        {
            return LerpPrecise(min, max, (3 - 2 * amount) * amount * amount);
        }

        /// <summary>
        /// Interpolates between two values using a 5th-degree equation.
        /// </summary>
        /// <param name="min">The initial value in the interpolation.</param>
        /// <param name="max">The final value in the interpolation.</param>
        /// <param name="amount">The amount of interpolation, measured between 0 and 1.</param>
        public static float SmootherStep(float min, float max, float amount)
        {
            // Lerp using the polynomial: 6(x^5) - 15(x^4) + 10(x^3)
            return Lerp(min, max, (6 * amount * amount - 15 * amount + 10) * amount * amount * amount);
        }

        /// <summary>
        /// Interpolates between two values using a 5th-degree equation.
        /// </summary>
        /// <param name="min">The initial value in the interpolation.</param>
        /// <param name="max">The final value in the interpolation.</param>
        /// <param name="amount">The amount of interpolation, measured between 0 and 1.</param>
        /// <remarks>
        /// In comparison to <see cref="SmootherStep(float, float, float)"/>, this function is more
        /// precise when working with big values.
        /// </remarks>
        public static float SmootherStepPrecise(float min, float max, float amount)
        {
            // Lerp using the polynomial: 6(x^5) - 15(x^4) + 10(x^3)
            return LerpPrecise(min, max, (6 * amount * amount - 15 * amount + 10) * amount * amount * amount);
        }

        /// <summary>
        /// Calculates the size to use for an array that needs resizing, where the new size
        /// will be a power of two times the previous capacity.
        /// </summary>
        /// <param name="currentCapacity">The current length of the array.</param>
        /// <param name="requiredCapacity">The minimum required length for the array.</param>
        /// <remarks>
        /// This is calculated with the following equation:<para/>
        /// <code>
        /// newCapacity = currentCapacity * pow(2, ceiling(log2(requiredCapacity/currentCapacity)));
        /// </code>
        /// </remarks>
        public static int GetNextCapacity(int currentCapacity, int requiredCapacity)
        {
            // Finds the smallest number that is greater than requiredCapacity and satisfies this equation:
            // " newCapacity = oldCapacity * 2^X " where X is an integer

            const double log2 = 0.69314718055994530941723212145818;
            int power = (int)Math.Ceiling(Math.Log(requiredCapacity / (double)currentCapacity) / log2);
            return currentCapacity * IntegerPow(2, power);
        }

        /// <summary>
        /// Calculates an integer value, raised to an integer exponent. Only works with positive values.
        /// </summary>
        public static int IntegerPow(int value, int exponent)
        {
            int r = 1;
            while (exponent > 0)
            {
                if ((exponent & 1) == 1)
                    r *= value;
                exponent >>= 1;
                value *= value;
            }
            return r;
        }

        /// <summary>
        /// Returns a random floating-point number in the range [0.0, max) (or (max, 0.0] if negative).
        /// </summary>
        /// <param name="random">The <see cref="Random"/> object to use for generating random numbers.</param>
        /// <param name="max">The exclusive maximum value of the random number to be generated.</param>
        public static float NextFloat(this Random random, float max)
        {
            return (float)random.NextDouble() * max;
        }

        /// <summary>
        /// Returns a random floating-point number in the range [min, max) (or (max, min] if min>max)
        /// </summary>
        /// <param name="random">The <see cref="Random"/> object to use for generating random numbers.</param>
        /// <param name="min">The inclusive minimum value of the random number to be generated.</param>
        /// <param name="max">The exclusive maximum value of the random number to be generated.</param>
        public static float NextFloat(this Random random, float min, float max)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }
    }
}
