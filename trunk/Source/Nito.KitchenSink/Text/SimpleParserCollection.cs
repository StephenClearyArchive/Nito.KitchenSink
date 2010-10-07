using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.Text
{
    using System.Collections.Concurrent;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// A simple parser for a specific type.
    /// </summary>
    public interface ISimpleParser
    {
        /// <summary>
        /// The type of the result of the <see cref="TryParse"/> method.
        /// </summary>
        Type ResultType { get; }

        /// <summary>
        /// Attempts to parse a string value into a value of type <see cref="ResultType"/>.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <returns>The resulting parsed value, or <c>null</c> if parsing was not possible.</returns>
        object TryParse(string value);
    }

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
                this.Add(DefaultBoolParser.Instance);
                this.Add(DefaultUInt8Parser.Instance);
                this.Add(DefaultInt8Parser.Instance);
                this.Add(DefaultUInt16Parser.Instance);
                this.Add(DefaultInt16Parser.Instance);
                this.Add(DefaultUInt32Parser.Instance);
                this.Add(DefaultInt32Parser.Instance);
                this.Add(DefaultUInt64Parser.Instance);
                this.Add(DefaultInt64Parser.Instance);
                this.Add(DefaultGuidParser.Instance);
                this.Add(DefaultTimeSpanParser.Instance);
                this.Add(DefaultDateTimeParser.Instance);
                this.Add(DefaultDateTimeOffsetParser.Instance);
            }
        }

        /// <summary>
        /// Adds the specified parser to the parser collection. If there is another parser matching the new parser's result type, the existing parser is removed.
        /// </summary>
        /// <param name="parser">The parser to add.</param>
        public void Add(ISimpleParser parser)
        {
            Contract.Requires(parser.ResultType != typeof(string));

            this.parsers.AddOrUpdate(parser.ResultType, parser, (_, __) => parser);
        }

        /// <summary>
        /// Removes the parser for the specified result type from the parser collection. If there is no parser defined for that type, then this method is a noop.
        /// </summary>
        /// <param name="type">The type whose parser is to be removed.</param>
        public void Remove(Type type)
        {
            ISimpleParser junk;
            this.parsers.TryRemove(type, out junk);
        }

        /// <summary>
        /// Removes any parsers matching the specified parser's result type from the parser collection. If there is no parser defined for that type, then this method is a noop.
        /// </summary>
        /// <param name="parser">The parser specifying the result type of the parser to remove.</param>
        public void Remove(ISimpleParser parser)
        {
            this.Remove(parser.ResultType);
        }

        /// <summary>
        /// Finds the parser for a specified result type; returns <c>null</c> if no parser is found.
        /// </summary>
        /// <param name="type">The result type.</param>
        /// <returns>The parser for the specified result type, or <c>null</c> if no parser is found.</returns>
        public ISimpleParser Find(Type type)
        {
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
        /// <param name="value">The string to parse.</param>
        /// <param name="resultType">The type of the result.</param>
        /// <param name="useDefaultEnumParsing">Whether to attempt to parse enums if <paramref name="resultType"/> is an enumeration and no parser was found.</param>
        /// <returns>The parsed value, or <c>null</c> if the string could not be parsed.</returns>
        public object TryParse(string value, Type resultType, bool useDefaultEnumParsing = true)
        {
            if (resultType == typeof(string))
            {
                return value;
            }

            var parser = this.Find(resultType);
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

        private sealed class DefaultBoolParser : ISimpleParser
        {
            public static readonly DefaultBoolParser Instance = new DefaultBoolParser();

            public Type ResultType
            {
                get { return typeof(bool); }
            }

            public object TryParse(string value)
            {
                bool ret;
                if (bool.TryParse(value, out ret))
                {
                    return ret;
                }

                return null;
            }
        }

        private sealed class DefaultUInt8Parser : ISimpleParser
        {
            public static readonly DefaultUInt8Parser Instance = new DefaultUInt8Parser();

            public Type ResultType
            {
                get { return typeof(byte); }
            }

            public object TryParse(string value)
            {
                byte ret;
                if (byte.TryParse(value, out ret))
                {
                    return ret;
                }

                return null;
            }
        }

        private sealed class DefaultInt8Parser : ISimpleParser
        {
            public static readonly DefaultInt8Parser Instance = new DefaultInt8Parser();

            public Type ResultType
            {
                get { return typeof(sbyte); }
            }

            public object TryParse(string value)
            {
                sbyte ret;
                if (sbyte.TryParse(value, out ret))
                {
                    return ret;
                }

                return null;
            }
        }

        private sealed class DefaultUInt16Parser : ISimpleParser
        {
            public static readonly DefaultUInt16Parser Instance = new DefaultUInt16Parser();

            public Type ResultType
            {
                get { return typeof(ushort); }
            }

            public object TryParse(string value)
            {
                ushort ret;
                if (ushort.TryParse(value, out ret))
                {
                    return ret;
                }

                return null;
            }
        }

        private sealed class DefaultInt16Parser : ISimpleParser
        {
            public static readonly DefaultInt16Parser Instance = new DefaultInt16Parser();

            public Type ResultType
            {
                get { return typeof(short); }
            }

            public object TryParse(string value)
            {
                short ret;
                if (short.TryParse(value, out ret))
                {
                    return ret;
                }

                return null;
            }
        }

        private sealed class DefaultUInt32Parser : ISimpleParser
        {
            public static readonly DefaultUInt32Parser Instance = new DefaultUInt32Parser();

            public Type ResultType
            {
                get { return typeof(uint); }
            }

            public object TryParse(string value)
            {
                uint ret;
                if (uint.TryParse(value, out ret))
                {
                    return ret;
                }

                return null;
            }
        }

        private sealed class DefaultInt32Parser : ISimpleParser
        {
            public static readonly DefaultInt32Parser Instance = new DefaultInt32Parser();

            public Type ResultType
            {
                get { return typeof(int); }
            }

            public object TryParse(string value)
            {
                int ret;
                if (int.TryParse(value, out ret))
                {
                    return ret;
                }

                return null;
            }
        }

        private sealed class DefaultUInt64Parser : ISimpleParser
        {
            public static readonly DefaultUInt64Parser Instance = new DefaultUInt64Parser();

            public Type ResultType
            {
                get { return typeof(ulong); }
            }

            public object TryParse(string value)
            {
                ulong ret;
                if (ulong.TryParse(value, out ret))
                {
                    return ret;
                }

                return null;
            }
        }

        private sealed class DefaultInt64Parser : ISimpleParser
        {
            public static readonly DefaultInt64Parser Instance = new DefaultInt64Parser();

            public Type ResultType
            {
                get { return typeof(long); }
            }

            public object TryParse(string value)
            {
                long ret;
                if (long.TryParse(value, out ret))
                {
                    return ret;
                }

                return null;
            }
        }

        private sealed class DefaultGuidParser : ISimpleParser
        {
            public static readonly DefaultGuidParser Instance = new DefaultGuidParser();

            public Type ResultType
            {
                get { return typeof(Guid); }
            }

            public object TryParse(string value)
            {
                Guid ret;
                if (Guid.TryParse(value, out ret))
                {
                    return ret;
                }

                return null;
            }
        }

        private sealed class DefaultTimeSpanParser : ISimpleParser
        {
            public static readonly DefaultTimeSpanParser Instance = new DefaultTimeSpanParser();

            public Type ResultType
            {
                get { return typeof(TimeSpan); }
            }

            public object TryParse(string value)
            {
                TimeSpan ret;
                if (TimeSpan.TryParse(value, out ret))
                {
                    return ret;
                }

                return null;
            }
        }

        private sealed class DefaultDateTimeParser : ISimpleParser
        {
            public static readonly DefaultDateTimeParser Instance = new DefaultDateTimeParser();

            public Type ResultType
            {
                get { return typeof(DateTime); }
            }

            public object TryParse(string value)
            {
                DateTime ret;
                if (DateTime.TryParse(value, out ret))
                {
                    return ret;
                }

                return null;
            }
        }

        private sealed class DefaultDateTimeOffsetParser : ISimpleParser
        {
            public static readonly DefaultDateTimeOffsetParser Instance = new DefaultDateTimeOffsetParser();

            public Type ResultType
            {
                get { return typeof(DateTimeOffset); }
            }

            public object TryParse(string value)
            {
                DateTimeOffset ret;
                if (DateTimeOffset.TryParse(value, out ret))
                {
                    return ret;
                }

                return null;
            }
        }
    }
}
