using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.Mathematics
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Provides numerical functions.
    /// </summary>
    public static class Numeric
    {
        /// <summary>
        /// Removes the fractional part of a number, rounding it to the nearest integer, moving away from zero.
        /// </summary>
        /// <param name="value">The value to truncate.</param>
        /// <returns>The truncated value.</returns>
        public static decimal TruncateAwayFromZero(decimal value)
        {
            if (value < 0)
            {
                return Math.Floor(value);
            }
            else
            {
                return Math.Ceiling(value);
            }
        }

        /// <summary>
        /// Removes the fractional part of a number, rounding it to the nearest integer, moving away from zero.
        /// </summary>
        /// <param name="value">The value to truncate.</param>
        /// <returns>The truncated value.</returns>
        public static double TruncateAwayFromZero(double value)
        {
            if (value < 0)
            {
                return Math.Floor(value);
            }
            else
            {
                return Math.Ceiling(value);
            }
        }

        /// <summary>
        /// Determines a root of a function by using the bisection method.
        /// </summary>
        /// <param name="low">The initial low-end guess. This value must have a positive fitness function result.</param>
        /// <param name="high">The initial high-end guess. This value must have a negative fitness function result.</param>
        /// <param name="accuracy">The required accuracy, if an exact solution isn't found. This may be zero to determine the root to the maximum accuracy supported by the <see cref="Decimal"/> type. This parameter may not be less than zero.</param>
        /// <param name="function">The fitness function. Given a guess, this function must return a negative number ("move lower") if the guess is too large; a positive number ("move higher") if the guess is too small; and zero if the guess is just right. It is acceptable to </param>
        /// <returns>Returns <c>true</c> if the root of the function was found; in this case, <paramref name="low"/> and <paramref name="high"/> are both equal to the root. Returns <c>false</c> if the root was not found; in this case, <paramref name="low"/> and <paramref name="high"/> bracket the root, and the difference between them is less than <c>2*<paramref name="accuracy"/></c></returns>
        /// <exception cref="InvalidOperationException">The fitness function <paramref name="function"/> does not converge within the range [<paramref name="low"/>, <paramref name="high"/>].</exception>
        public static bool Bisect(ref decimal low, ref decimal high, decimal accuracy, Func<decimal, int> function)
        {
            Contract.Requires<ArgumentOutOfRangeException>(accuracy >= 0, "Accuracy cannot be negative.");

            checked
            {
                accuracy *= 2;
                while (Math.Abs(high - low) >= accuracy)
                {
                    Contract.Assume(function(low) > 0);
                    Contract.Assume(function(high) < 0);

                    decimal midpoint = (low + high) / 2;

                    Contract.Assume(low <= midpoint);
                    Contract.Assume(midpoint <= high);

                    var result = function(midpoint);
                    if (result < 0)
                    {
                        if (high == midpoint)
                        {
                            // We've reached the built-in accuracy limit of decimal.
                            return false;
                        }

                        high = midpoint;
                    }
                    else if (result > 0)
                    {
                        if (low == midpoint)
                        {
                            // We've reached the built-in accuracy limit of decimal.
                            return false;
                        }

                        low = midpoint;
                    }
                    else
                    {
                        low = high = midpoint;
                        return true;
                    }
                }

                // We've trimmed the bracket to within +/- the requested accuracy.
                return false;
            }
        }
    }
}
