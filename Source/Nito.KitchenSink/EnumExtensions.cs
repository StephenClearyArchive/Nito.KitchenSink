using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    /// <summary>
    /// Provides useful extensions for enumerations.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Determines if a bit flag or set of bit flags are set in an enumeration value.
        /// </summary>
        /// <typeparam name="T">The type of enumeration.</typeparam>
        /// <param name="value">The value to test for the bit flag(s).</param>
        /// <param name="flag">The flag(s) to test for.</param>
        /// <returns><c>true</c> if <paramref name="value"/> contains the bit flags defined in <paramref name="flag"/>; otherwise, <c>false</c>.</returns>
        public static bool Contains<T>(this Enum value, T flag)
        {
            try
            {
                return (Convert.ToInt64(value) & Convert.ToInt64(flag)) != 0;
            }
            catch (OverflowException)
            {
                return (Convert.ToUInt64(value) & Convert.ToUInt64(flag)) != 0;
            }
        }

        /// <summary>
        /// Combines an enumeration value with a bit flag or set of bit flags and returns the new value.
        /// </summary>
        /// <typeparam name="T">The type of enumeration.</typeparam>
        /// <param name="value">The value to combine with the bit flag(s).</param>
        /// <param name="flag">The flag(s) to add to <paramref name="value"/>.</param>
        /// <returns><paramref name="value"/> combined with <paramref name="flag"/>.</returns>
        public static T Add<T>(this Enum value, T flag)
        {
            try
            {
                return (T)Convert.ChangeType(Convert.ToInt64(value) | Convert.ToInt64(flag), Enum.GetUnderlyingType(typeof(T)));
            }
            catch (OverflowException)
            {
                return (T)Convert.ChangeType(Convert.ToUInt64(value) | Convert.ToUInt64(flag), Enum.GetUnderlyingType(typeof(T)));
            }
        }

        /// <summary>
        /// Removes a bit flag of set of bit flags from an enumeration value and returns the new value.
        /// </summary>
        /// <typeparam name="T">The type of enumeration.</typeparam>
        /// <param name="value">The value from which to remove the bit flag(s).</param>
        /// <param name="flag">The flag(s) to remove from <paramref name="value"/>.</param>
        /// <returns><paramref name="value"/> with any flags in <paramref name="flag"/> removed.</returns>
        public static T Remove<T>(this Enum value, T flag)
        {
            try
            {
                return (T)Convert.ChangeType(Convert.ToInt64(value) & ~Convert.ToInt64(flag), Enum.GetUnderlyingType(typeof(T)));
            }
            catch (OverflowException)
            {
                return (T)Convert.ChangeType(Convert.ToUInt64(value) & ~Convert.ToUInt64(flag), Enum.GetUnderlyingType(typeof(T)));
            }
        }

        /// <summary>
        /// Adds or removes a bit flag or set of bit flags in an enumeration value and returns the new value.
        /// </summary>
        /// <typeparam name="T">The type of enumeration.</typeparam>
        /// <param name="value">The value on which to operate.</param>
        /// <param name="flag">The flag(s) to add to or remove from <paramref name="value"/>.</param>
        /// <param name="add">Whether to add or remove the flags; <c>true</c> adds the flags; <c>false</c> removes them.</param>
        /// <returns>The new value.</returns>
        public static T AddOrRemove<T>(this Enum value, T flag, bool add)
        {
            if (add)
            {
                return value.Add(flag);
            }
            else
            {
                return value.Remove(flag);
            }
        }
    }
}
