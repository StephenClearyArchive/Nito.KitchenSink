// <copyright file="OptionAttribute.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.OptionParsing
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Specifies that a command-line option sets this property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class OptionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionAttribute"/> class.
        /// </summary>
        /// <param name="longName">The long name of the option. The long name may not contain ':' or '=' characters.</param>
        /// <param name="argument">A value indicating whether this option takes an argument.</param>
        public OptionAttribute(string longName, OptionArgument argument = OptionArgument.Required)
        {
            this.LongName = longName;
            this.Argument = argument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionAttribute"/> class.
        /// </summary>
        /// <param name="shortName">The short name of the option, if any. The short name may not be ':' or '='.</param>
        /// <param name="argument">A value indicating whether this option takes an argument.</param>
        public OptionAttribute(char shortName, OptionArgument argument = OptionArgument.Required)
        {
            this.ShortName = shortName;
            this.Argument = argument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionAttribute"/> class.
        /// </summary>
        /// <param name="longName">The long name of the option. The long name may not contain ':' or '=' characters.</param>
        /// <param name="shortName">The short name of the option, if any. The short name may not be ':' or '='.</param>
        /// <param name="argument">A value indicating whether this option takes an argument.</param>
        public OptionAttribute(string longName, char shortName, OptionArgument argument = OptionArgument.Required)
        {
            this.LongName = longName;
            this.ShortName = shortName;
            this.Argument = argument;
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
        /// Gets or sets a value indicating whether this option takes an argument.
        /// </summary>
        public OptionArgument Argument { get; set; }
    }
}
