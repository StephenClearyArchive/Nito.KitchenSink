// <copyright file="Option.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.OptionParsing
{
    /// <summary>
    /// An option that was parsed by the <see cref="OptionParser"/>.
    /// </summary>
    public sealed class Option
    {
        /// <summary>
        /// Gets or sets the option definition that matched this option. May be <c>null</c> if this is a positional argument.
        /// </summary>
        public OptionDefinition Definition { get; set; }

        /// <summary>
        /// Gets or sets the argument passed to this option. May be <c>null</c> if there is no argument.
        /// </summary>
        public string Argument { get; set; }
    }
}
