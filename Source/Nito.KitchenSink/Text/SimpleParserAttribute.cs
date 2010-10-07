using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.Text
{
    /// <summary>
    /// Specifies that a given parser should be used for this type or member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property)]
    public sealed class SimpleParserAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleParserAttribute"/> class.
        /// </summary>
        /// <param name="parserType">Type of the parser.</param>
        public SimpleParserAttribute(Type parserType)
        {
            if (parserType.GetInterface("Nito.KitchenSink.Text.ISimpleParser") == null)
            {
                throw new InvalidOperationException("The type passed to SimpleParserAttribute must implement ISimpleParser.");
            }

            this.ParserType = parserType;
        }

        /// <summary>
        /// Gets or sets the type of the parser.
        /// </summary>
        /// <value>The type of the parser.</value>
        public Type ParserType { get; set; }
    }
}
