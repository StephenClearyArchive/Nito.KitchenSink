// <copyright file="OptionPresentAttribute.cs" company="Nito Programs">
//     Copyright (c) 2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.OptionParsing
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Specifies that the presence of a command-line option sets this property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class OptionPresentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionPresentAttribute"/> class.
        /// </summary>
        /// <param name="longName">The long name of the option. The long name may not contain ':' or '=' characters.</param>
        public OptionPresentAttribute(string longName)
        {
            this.LongName = longName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionPresentAttribute"/> class.
        /// </summary>
        /// <param name="shortName">The short name of the option, if any. The short name may not be ':' or '='.</param>
        public OptionPresentAttribute(char shortName)
        {
            this.ShortName = shortName;
        }

        /// <summary>
        /// Gets or sets the long name of the option. The long name may not contain ':' or '=' characters.
        /// </summary>
        public string LongName { get; set; }

        /// <summary>
        /// Gets or sets the short name of the option, if any. The short name may not be ':' or '='.
        /// </summary>
        public char? ShortName { get; set; }

        /// <summary>
        /// Gets the short name of the option as a string.
        /// </summary>
        public string ShortNameAsString
        {
            get
            {
                if (this.ShortName == null)
                {
                    return null;
                }

                return this.ShortName.ToString();
            }
        }

        /// <summary>
        /// Gets the long name, if any; otherwise, gets the short name, if any.
        /// </summary>
        public string Name
        {
            get
            {
                return this.LongName ?? this.ShortNameAsString;
            }
        }
    }
}
