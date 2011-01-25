// <copyright file="SimpleParserCollection.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.SimpleParsers
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.Contracts;
    using System.Numerics;
    using Nito.KitchenSink.Dynamic;

    /// <summary>
    /// A collection of simple parsers. Each result type only has one parser defined for it in a given collection. This class is threadsafe.
    /// </summary>
    public sealed class SimpleParserCollection
    {
        private readonly ConcurrentDictionary<Type, ISimpleParser> parsers = new ConcurrentDictionary<Type, ISimpleParser>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleParserCollection"/> class.
        /// </summary>
        /// <param name="includeDefaultParsers">Whether to include the default parsers in the parser collection.</param>
        public SimpleParserCollection(bool includeDefaultParsers = true)
        {
            if (includeDefaultParsers)
            {
                this.Add(DefaultSimpleParser<bool>.Instance);
                this.Add(DefaultSimpleParser<byte>.Instance);
                this.Add(DefaultSimpleParser<sbyte>.Instance);
                this.Add(DefaultSimpleParser<ushort>.Instance);
                this.Add(DefaultSimpleParser<short>.Instance);
                this.Add(DefaultSimpleParser<uint>.Instance);
                this.Add(DefaultSimpleParser<int>.Instance);
                this.Add(DefaultSimpleParser<ulong>.Instance);
                this.Add(DefaultSimpleParser<long>.Instance);
                this.Add(DefaultSimpleParser<Guid>.Instance);
                this.Add(DefaultSimpleParser<TimeSpan>.Instance);
                this.Add(DefaultSimpleParser<DateTime>.Instance);
                this.Add(DefaultSimpleParser<DateTimeOffset>.Instance);
                this.Add(DefaultSimpleParser<float>.Instance);
                this.Add(DefaultSimpleParser<double>.Instance);
                this.Add(DefaultSimpleParser<decimal>.Instance);
                this.Add(DefaultSimpleParser<BigInteger>.Instance);
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.parsers != null);
        }

        /// <summary>
        /// Adds the specified parser to the parser collection. If there is another parser matching the new parser's result type, the existing parser is removed.
        /// </summary>
        /// <param name="parser">The parser to add. May not be <c>null</c>.</param>
        public void Add(ISimpleParser parser)
        {
            Contract.Requires(parser != null);

            this.parsers.AddOrUpdate(parser.ResultType, parser, (_, __) => parser);
        }

        /// <summary>
        /// Removes the parser for the specified result type from the parser collection. If there is no parser defined for that type, then this method is a noop.
        /// </summary>
        /// <param name="type">The type whose parser is to be removed. May not be <c>null</c>.</param>
        public void Remove(Type type)
        {
            Contract.Requires(type != null);

            ISimpleParser junk;
            this.parsers.TryRemove(type, out junk);
        }

        /// <summary>
        /// Removes any parsers matching the specified parser's result type from the parser collection. If there is no parser defined for that type, then this method is a noop.
        /// </summary>
        /// <param name="parser">The parser specifying the result type of the parser to remove. May not be <c>null</c>.</param>
        public void Remove(ISimpleParser parser)
        {
            Contract.Requires(parser != null);

            this.Remove(parser.ResultType);
        }

        /// <summary>
        /// Finds the parser for a specified result type; returns <c>null</c> if no parser is found.
        /// </summary>
        /// <param name="type">The result type. May not be <c>null</c>.</param>
        /// <returns>The parser for the specified result type, or <c>null</c> if no parser is found.</returns>
        public ISimpleParser Find(Type type)
        {
            Contract.Requires(type != null);

            ISimpleParser ret;
            if (this.parsers.TryGetValue(type, out ret))
            {
                return ret;
            }

            return null;
        }

        /// <summary>
        /// Attempts to parse the specified string with an expected result type.
        /// </summary>
        /// <param name="value">The string to parse. May not be <c>null</c>.</param>
        /// <param name="resultType">The type of the result. This may be a nullable type. May not be <c>null</c>.</param>
        /// <param name="useDefaultEnumParsing">Whether to attempt to parse enums if <paramref name="resultType"/> is an enumeration and no parser was found.</param>
        /// <returns>The parsed value, or <c>null</c> if the string could not be parsed.</returns>
        public object TryParse(string value, Type resultType, bool useDefaultEnumParsing = true)
        {
            Contract.Requires(value != null);
            Contract.Requires(resultType != null);

            if (resultType == typeof(string))
            {
                return value;
            }

            var type = Nullable.GetUnderlyingType(resultType) ?? resultType;
            var parser = this.Find(type);
            if (parser != null)
            {
                return parser.TryParse(value);
            }

            if (useDefaultEnumParsing && resultType.IsEnum)
            {
                try
                {
                    return Enum.Parse(resultType, value);
                }
                catch (ArgumentException)
                {
                }
                catch (OverflowException)
                {
                }
            }

            return null;
        }
    }
}
