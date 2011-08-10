// <copyright file="OptionDefinition.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.OptionParsing
{
    /// <summary>
    /// Whether an option may take an argument.
    /// </summary>
    public enum OptionArgument
    {
        /// <summary>
        /// The option never takes an argument.
        /// </summary>
        None,

        /// <summary>
        /// The option requires an argument.
        /// </summary>
        Required,

        /// <summary>
        /// The option takes an argument if present. The argument may not start with '-' or '/'.
        /// </summary>
        Optional,
    }

    /// <summary>
    /// The definition of an option that may be passed to a console program.
    /// </summary>
    public sealed class OptionDefinition
    {
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

        /// <summary>
        /// Gets or sets a value indicating whether this option takes an argument.
        /// </summary>
        public OptionArgument Argument { get; set; }
    }
}
