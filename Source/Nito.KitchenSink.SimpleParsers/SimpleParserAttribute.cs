// <copyright file="SimpleParserAttribute.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.SimpleParsers
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Specifies that a given parser should be used for this type or member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property)]
    public sealed class SimpleParserAttribute : Attribute
    {
        private Type parserType;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleParserAttribute"/> class.
        /// </summary>
        /// <param name="parserType">Type of the parser.</param>
        public SimpleParserAttribute(Type parserType)
        {
            Contract.Requires(parserType != null);
            Contract.Requires(parserType.GetInterface("Nito.KitchenSink.SimpleParsers.ISimpleParser") != null, "The type passed to SimpleParserAttribute must implement ISimpleParser.");

            this.parserType = parserType;
        }

        /// <summary>
        /// Gets or sets the type of the parser.
        /// </summary>
        /// <value>The type of the parser.</value>
        public Type ParserType
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                Contract.Ensures(Contract.Result<Type>().GetInterface("Nito.KitchenSink.SimpleParsers.ISimpleParser") != null);

                return this.parserType;
            }
            
            set
            {
                Contract.Requires(value != null);
                Contract.Requires(value.GetInterface("Nito.KitchenSink.SimpleParsers.ISimpleParser") != null, "The type passed to SimpleParserAttribute must implement ISimpleParser.");

                this.parserType = value;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.parserType != null);
            Contract.Invariant(this.parserType.GetInterface("Nito.KitchenSink.SimpleParsers.ISimpleParser") != null);
        }
    }
}
