using System;

#pragma warning disable CA1062 // Validate arguments of public methods

namespace TrippyGL.Utils
{
    /// <summary>
    /// Provides some general help math-related functions and values.
    /// </summary>
    public static class TrippyMath
    {
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
