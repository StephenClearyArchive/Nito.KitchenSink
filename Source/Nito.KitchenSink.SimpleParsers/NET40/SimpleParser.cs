// <copyright file="SimpleParser.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.SimpleParsers
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A simple parser for a specific type.
    /// </summary>
    [ContractClass(typeof(SimpleParserContracts))]
    public interface ISimpleParser
    {
        /// <summary>
        /// The type of the result of the <see cref="TryParse"/> method. This cannot be the <see cref="System.String"/> type or a nullable value type.
        /// </summary>
        Type ResultType { get; }

        /// <summary>
        /// Attempts to parse a string value into a value of type <see cref="ResultType"/>.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <returns>The resulting parsed value, or <c>null</c> if parsing was not possible.</returns>
        object TryParse(string value);
    }

    [ContractClassFor(typeof(ISimpleParser))]
    internal abstract class SimpleParserContracts : ISimpleParser
    {
        public Type ResultType
        {
            [SuppressMessage("Microsoft.Contracts", "CC1036", Justification = "Nullable.GetUnderlyingType is pure.")]
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                Contract.Ensures(Contract.Result<Type>() != typeof(string));
                Contract.Ensures(Nullable.GetUnderlyingType(Contract.Result<Type>()) == null);
                return null;
            }
        }

        public object TryParse(string value)
        {
            Contract.Requires(value != null);
            Contract.Ensures(Contract.Result<object>() == null || Contract.Result<object>().GetType() == this.ResultType);
            return null;
        }
    }
}
