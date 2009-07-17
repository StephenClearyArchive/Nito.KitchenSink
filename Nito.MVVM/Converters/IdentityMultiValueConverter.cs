// <copyright file="IdentityMultiValueConverter.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.MVVM
{
    using System;
    using System.Collections;
    using System.Windows.Data;
    using Nito.Utility;

    /// <summary>
    /// A multi-value converter that performs no conversions.
    /// </summary>
    public sealed class IdentityMultiValueConverter : IMultiValueConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityMultiValueConverter"/> class, setting <see cref="Comparer"/> to <see cref="EqualityComparer.Default"/>.
        /// </summary>
        public IdentityMultiValueConverter()
        {
            this.Comparer = EqualityComparer.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityMultiValueConverter"/> class, setting <see cref="Comparer"/> to <paramref name="comparer"/>.
        /// </summary>
        /// <param name="comparer">The comparer to use to determine element equality.</param>
        public IdentityMultiValueConverter(IEqualityComparer comparer)
        {
            this.Comparer = comparer;
        }

        /// <summary>
        /// Gets or sets the comparer to use to determine element equality. This may not be set to null.
        /// </summary>
        public IEqualityComparer Comparer { get; set; }

        /// <summary>
        /// Gets or sets the value that is returned when there are no source elements. This is null by default.
        /// </summary>
        public object EmptyValue { get; set; }

        /// <summary>
        /// Gets or sets the value that is returned when not all source elements are equal. This is null by default.
        /// </summary>
        public object InvalidValue { get; set; }

        /// <summary>
        /// Converts from a group of source elements to a single result element.
        /// </summary>
        /// <param name="values">The group of source elements.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns><see cref="EmptyValue"/> if there are no source elements; <see cref="InvalidValue"/> if not all source elements are equal; one of the source elements if all source elements are equal.</returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 0)
            {
                return this.EmptyValue;
            }

            object ret = values[0];
            for (int i = 1; i != values.Length; ++i)
            {
                if (!this.Comparer.Equals(ret, values[i]))
                {
                    return this.InvalidValue;
                }
            }

            return ret;
        }

        /// <summary>
        /// Converts from a single result element back to a group of source elements.
        /// </summary>
        /// <param name="value">The single result element. If <paramref name="value"/> is equal to <see cref="EmptyValue"/> or <see cref="InvalidValue"/>, no special action is taken.</param>
        /// <param name="targetTypes">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>A group of source elements, each one a copy of <paramref name="value"/>.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] ret = new object[targetTypes.Length];

            for (int i = 0; i != ret.Length; ++i)
            {
                ret[i] = value;
            }

            return ret;
        }
    }
}
