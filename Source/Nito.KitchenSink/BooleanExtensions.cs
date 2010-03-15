// <copyright file="BooleanExtensions.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;

    /// <summary>
    /// Provides useful extension methods for the <see cref="Boolean"/> type.
    /// </summary>
    public static class BooleanExtensions
    {
        /// <summary>
        /// Converts a boolean value to an integer value. Returns 0 if the boolean value is false, or 1 if the boolean value is true.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <returns>0 if the boolean value is false, or 1 if the boolean value is true.</returns>
        public static int ToInt32(this bool value)
        {
            return value ? 1 : 0;
        }

        /// <summary>
        /// Converts a nullable boolean value to a nullable integer value. Returns 0 if the boolean value is false, or 1 if the boolean value is true, or null if there is no boolean value.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <returns>0 if the boolean value is false, or 1 if the boolean value is true, or null if there is no boolean value.</returns>
        public static int? ToInt32(this bool? value)
        {
            if (value.HasValue)
            {
                return value.Value.ToInt32();
            }

            return null;
        }
    }
}
