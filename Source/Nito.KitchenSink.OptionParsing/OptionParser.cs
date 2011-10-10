// <copyright file="OptionParser.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.OptionParsing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Nito.KitchenSink.SimpleParsers;

    /// <summary>
    /// A command-line option parser. Takes option definitions and a sequence of strings, and emits a sequence of <see cref="Option"/> instances.
    /// </summary>
    public static class OptionParser
    {
        /// <summary>
        /// Parses the command line into a series of options.
        /// </summary>
        /// <param name="commandLine">The command line to parse. If <c>null</c>, the process' command line is lexed by <see cref="NitoCommandLineLexer"/>.</param>
        /// <param name="definitions">The sequence of option definitions to use when parsing the command line. May not be <c>null</c>.</param>
        /// <param name="stringComparer">The string comparison to use when parsing options. If <c>null</c>, then the string comparer for the current culture is used.</param>
        /// <returns>The sequence of options represented by the command line.</returns>
        public static IEnumerable<Option> Parse(IEnumerable<string> commandLine, IEnumerable<OptionDefinition> definitions, StringComparer stringComparer = null)
        {
            Contract.Requires(definitions != null);
            Contract.Ensures(Contract.Result<IEnumerable<Option>>() != null);

            return new Parser(commandLine, definitions, stringComparer);
        }

        /// <summary>
        /// Parses the command line into a default-constructed arguments object. Option definitions are determined by the attributes on the properties of the arguments object. This method will call <see cref="IOptionArguments.Validate"/> on the returned value before returning.
        /// </summary>
        /// <typeparam name="T">The type of arguments object to initialize.</typeparam>
        /// <param name="commandLine">The command line to parse. If <c>null</c>, the process' command line is lexed by <see cref="NitoCommandLineLexer"/>.</param>
        /// <param name="parserCollection">A parser collection to use for parsing, or <c>null</c> to use the default parsers.</param>
        /// <param name="stringComparer">The string comparison to use when parsing options. If <c>null</c>, then the string comparer for the current culture is used.</param>
        /// <returns>The arguments object.</returns>
        public static T Parse<T>(IEnumerable<string> commandLine = null, SimpleParserCollection parserCollection = null, StringComparer stringComparer = null) where T : class, IOptionArguments, new()
        {
            T ret = new T();
            ret.Parse(commandLine, parserCollection, stringComparer);
            return ret;
        }

        private sealed class Parser : IEnumerable<Option>
        {
            private static readonly char[] ArgumentDelimiters = new[] { ':', '=' };

            /// <summary>
            /// The option definitions.
            /// </summary>
            private readonly OptionDefinition[] definitions;

            /// <summary>
            /// The string comparer to use when parsing options.
            /// </summary>
            private readonly StringComparer stringComparer;

            /// <summary>
            /// The command line to parse.
            /// </summary>
            private readonly IEnumerable<string> commandLine;

            /// <summary>
            /// The last option read, or <c>null</c> if there is no option context. If this is not <c>null</c>, then it is an option that can take an argument.
            /// </summary>
            private OptionDefinition lastOption;

            /// <summary>
            /// Whether or not the option parsing is done. If this is <c>true</c>, then all remaining command line items are positional parameters.
            /// </summary>
            private bool done;

            /// <summary>
            /// Initializes a new instance of the <see cref="Parser"/> class.
            /// </summary>
            /// <param name="commandLine">The command line to parse. If <c>null</c>, the process' command line is lexed by <see cref="NitoCommandLineLexer"/>.</param>
            /// <param name="definitions">The option definitions. May not be <c>null</c>.</param>
            /// <param name="stringComparer">The string comparer to use when parsing options. If <c>null</c>, then the string comparer for the current culture is used.</param>
            public Parser(IEnumerable<string> commandLine, IEnumerable<OptionDefinition> definitions, StringComparer stringComparer)
            {
                Contract.Requires(definitions != null);

                this.definitions = definitions.ToArray();
                this.stringComparer = stringComparer ?? StringComparer.CurrentCulture;
                this.commandLine = commandLine ?? NitoCommandLineLexer.Lex();
            }

            [ContractInvariantMethod]
            private void ObjectInvariant()
            {
                Contract.Invariant(this.definitions != null);
                Contract.Invariant(this.stringComparer != null);
                Contract.Invariant(this.commandLine != null);

                // If the option parsing is done, then the last option must be null.
                Contract.Invariant(!this.done || this.lastOption == null);

                // If the last option is not null, then it must be an option that can take an argument.
                Contract.Invariant(this.lastOption == null || this.lastOption.Argument != OptionArgument.None);
            }

            /// <summary>
            /// Parses the command line into options.
            /// </summary>
            /// <returns>The sequence of options specified on the command line.</returns>
            public IEnumerator<Option> GetEnumerator()
            {
                foreach (var value in this.commandLine)
                {
                    // If the option parsing is done, then all remaining command line elements are positional arguments.
                    if (this.done)
                    {
                        yield return new Option { Argument = value };
                        continue;
                    }

                    if (this.lastOption != null && this.lastOption.Argument == OptionArgument.Required)
                    {
                        // Argument for a preceding option
                        yield return new Option { Definition = this.lastOption, Argument = value };
                        this.lastOption = null;
                        continue;
                    }

                    // Either there is no last option, or the last option's argument is optional.

                    if (value == "--")
                    {
                        // End-of-options marker

                        if (this.lastOption != null)
                        {
                            // The last option was an option that takes an optional argument, without an argument.
                            yield return new Option { Definition = this.lastOption };
                            this.lastOption = null;
                        }

                        // All future parameters are positional arguments.
                        this.done = true;
                        continue;
                    }

                    if (value.StartsWith("--"))
                    {
                        // Long option

                        if (this.lastOption != null)
                        {
                            // The last option was an option that takes an optional argument, without an argument.
                            yield return new Option { Definition = this.lastOption };
                        }

                        string option = null;
                        string argument = null;
                        int argumentIndex = value.IndexOfAny(ArgumentDelimiters, 2);
                        if (argumentIndex == -1)
                        {
                            // No argument delimiters were found; the command line element is an option.
                            option = value.Substring(2);
                        }
                        else
                        {
                            // An argument delimiter was found; split the command line element into an option and its argument.
                            option = value.Substring(2, argumentIndex - 2);
                            argument = value.Substring(argumentIndex + 1);
                        }

                        // Find the option in the option definitions.
                        this.lastOption = this.definitions.FirstOrDefault(x => this.stringComparer.Equals(x.LongName, option));
                        if (this.lastOption == null)
                        {
                            throw new OptionParsingException.UnknownOptionException("Unknown option  " + option + "  in parameter  " + value);
                        }

                        if (argument != null)
                        {
                            // There is an argument with this long option, so it is complete.
                            if (this.lastOption.Argument == OptionArgument.None)
                            {
                                throw new OptionParsingException.OptionArgumentException("Option  " + option + "  cannot take an argument in parameter  " + value);
                            }

                            yield return new Option { Definition = this.lastOption, Argument = argument };
                            this.lastOption = null;
                            continue;
                        }

                        if (this.lastOption.Argument == OptionArgument.None)
                        {
                            // This long option does not take an argument, so it is complete.
                            yield return new Option { Definition = this.lastOption };
                            this.lastOption = null;
                            continue;
                        }

                        // This long option does take an argument.
                        continue;
                    }

                    if (value.StartsWith("-"))
                    {
                        // Short option or short option run

                        if (this.lastOption != null)
                        {
                            // The last option was an option that takes an optional argument, without an argument.
                            yield return new Option { Definition = this.lastOption };
                        }

                        if (value.Length < 2)
                        {
                            throw new OptionParsingException.InvalidParameterException("Invalid parameter  " + value);
                        }

                        string option = value[1].ToString();
                        this.lastOption = this.definitions.FirstOrDefault(x => this.stringComparer.Equals(x.ShortNameAsString, option));
                        if (this.lastOption == null)
                        {
                            throw new OptionParsingException.UnknownOptionException("Unknown option  " + option + "  in parameter  " + value);
                        }

                        // The first short option may either have an argument or start a short option run
                        int argumentIndex = value.IndexOfAny(ArgumentDelimiters, 2);
                        if (argumentIndex == 2)
                        {
                            // The first short option has an argument.
                            if (this.lastOption.Argument == OptionArgument.None)
                            {
                                throw new OptionParsingException.OptionArgumentException("Option  " + option + "  cannot take an argument in parameter  " + value);
                            }

                            yield return new Option { Definition = this.lastOption, Argument = value.Substring(3) };
                            this.lastOption = null;
                        }
                        else if (argumentIndex != -1)
                        {
                            // There is an argument delimiter somewhere else in the parameter string.
                            throw new OptionParsingException.InvalidParameterException("Invalid parameter  " + value);
                        }
                        else if (value.Length == 2)
                        {
                            // The first short option is the only one.
                            if (this.lastOption.Argument == OptionArgument.None)
                            {
                                yield return new Option { Definition = this.lastOption };
                                this.lastOption = null;
                            }
                        }
                        else
                        {
                            // This is a short option run; they must not take arguments.
                            for (int i = 1; i != value.Length; ++i)
                            {
                                option = value[i].ToString();
                                this.lastOption = this.definitions.FirstOrDefault(x => this.stringComparer.Equals(x.ShortNameAsString, option));
                                if (this.lastOption == null)
                                {
                                    throw new OptionParsingException.UnknownOptionException("Unknown option  " + option + "  in parameter  " + value);
                                }

                                if (this.lastOption.Argument == OptionArgument.Required)
                                {
                                    throw new OptionParsingException.OptionArgumentException("Option  " + option + "  cannot be in a short option run (because it takes an argument) in parameter  " + value);
                                }

                                yield return new Option { Definition = this.lastOption };
                                this.lastOption = null;
                            }
                        }

                        continue;
                    }

                    if (value.StartsWith("/"))
                    {
                        // Short or long option

                        if (this.lastOption != null)
                        {
                            // The last option was an option that takes an optional argument, without an argument.
                            yield return new Option { Definition = this.lastOption };
                        }

                        if (value.Length < 2)
                        {
                            throw new OptionParsingException.InvalidParameterException("Invalid parameter  " + value);
                        }

                        string option = null;
                        string argument = null;
                        int argumentIndex = value.IndexOfAny(ArgumentDelimiters, 2);
                        if (argumentIndex == -1)
                        {
                            option = value.Substring(1);
                        }
                        else
                        {
                            option = value.Substring(1, argumentIndex - 1);
                            argument = value.Substring(argumentIndex + 1);
                        }

                        this.lastOption = this.definitions.FirstOrDefault(x => this.stringComparer.Equals(x.LongName, option) || this.stringComparer.Equals(x.ShortNameAsString, option));
                        if (this.lastOption == null)
                        {
                            throw new OptionParsingException.UnknownOptionException("Unknown option  " + option + "  in parameter  " + value);
                        }

                        if (argument != null)
                        {
                            // There is an argument with this option, so it is complete.
                            if (this.lastOption.Argument == OptionArgument.None)
                            {
                                throw new OptionParsingException.OptionArgumentException("Option  " + option + "  cannot take an argument in parameter  " + value);
                            }

                            yield return new Option { Definition = this.lastOption, Argument = argument };
                            this.lastOption = null;
                            continue;
                        }

                        if (this.lastOption.Argument == OptionArgument.None)
                        {
                            // This option does not take an argument, so it is complete.
                            yield return new Option { Definition = this.lastOption };
                            this.lastOption = null;
                            continue;
                        }

                        // This option does take an argument.
                        continue;
                    }

                    if (this.lastOption != null)
                    {
                        // The last option was an option that takes an optional argument, with an argument.
                        yield return new Option { Definition = this.lastOption, Argument = value };
                        this.lastOption = null;
                        continue;
                    }

                    // The first positional argument
                    this.done = true;
                    yield return new Option { Argument = value };
                    continue;
                }

                if (this.lastOption != null)
                {
                    if (this.lastOption.Argument == OptionArgument.Required)
                    {
                        throw new OptionParsingException.OptionArgumentException("Missing argument for option  " + this.lastOption.Name);
                    }

                    yield return new Option { Definition = this.lastOption };
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
