using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.Options
{
    using System.Collections;
    using System.Diagnostics.Contracts;

    using Text;

    /// <summary>
    /// A command-line option parser. Takes option definitions and a sequence of strings, and emits a sequence of <see cref="Option"/> instances.
    /// </summary>
    public sealed class OptionParser
    {
        /// <summary>
        /// The option that was parsed by the <see cref="OptionParser"/>.
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

        /// <summary>
        /// Gets or sets the sequence of option definitions to use when parsing the command line.
        /// </summary>
        public IEnumerable<OptionDefinition> Definitions { get; set; }

        /// <summary>
        /// The string comparison to use when searching option definitions.
        /// </summary>
        public StringComparer StringComparer { get; set; }

        /// <summary>
        /// Parses the command line into a series of options.
        /// </summary>
        /// <param name="commandLine">The command line to parse.</param>
        /// <returns>The sequence of options represented by the command line.</returns>
        public IEnumerable<Option> Parse(IEnumerable<string> commandLine)
        {
            Contract.Requires(this.Definitions != null, "OptionParser.Definitions must be set before calling Parse.");

            return new Parser(this.Definitions, this.StringComparer, commandLine);
        }

        /// <summary>
        /// Parses the command line into a parameter object. The <see cref="Definitions"/> property is ignored; option definitions are determined by the attributes on the properties of <paramref name="parameterObject"/>.
        /// </summary>
        /// <typeparam name="T">The type of parameter object to initialize.</typeparam>
        /// <param name="commandLine">The command line to parse.</param>
        /// <param name="parameterObject">The parameter object that is initialized.</param>
        /// <param name="parserCollection">A parser collection to use for parsing, or <c>null</c> to use the default parsers.</param>
        public void Parse<T>(IEnumerable<string> commandLine, T parameterObject, SimpleParserCollection parserCollection = null) where T : class
        {
            if (parserCollection == null)
            {
                parserCollection = new SimpleParserCollection();
            }

            // Generate option definitions from OptionAttributes on the parameter object.
            var options = new Dictionary<OptionDefinition, Action<string>>();
            var positionalArguments = new List<Action<string>>();
            Action<string> remainingPositionalArguments = null;
            var parameterObjectType = parameterObject.GetType();
            foreach (var property in parameterObjectType.GetProperties())
            {
                var localProperty = property;
                foreach (var attribute in property.GetCustomAttributes(true))
                {
                    var optionAttribute = attribute as OptionAttribute;
                    if (optionAttribute != null)
                    {
                        var optionDefinition = new OptionDefinition { LongName = optionAttribute.LongName, ShortName = optionAttribute.ShortName, Argument = optionAttribute.Argument };
                        if (optionDefinition.Argument == OptionArgument.None)
                        {
                            if (localProperty.PropertyType != typeof(bool))
                            {
                                throw new InvalidOperationException("An OptionAttribute with no Argument may only be applied to a boolean property.");
                            }

                            options.Add(optionDefinition, _ => localProperty.SetValue(parameterObject, true, null));
                        }
                        else
                        {
                            // TODO: allow conversion overrides
                            options.Add(optionDefinition, parameter =>
                            {
                                var value = parserCollection.TryParse(parameter, localProperty.PropertyType);
                                if (value == null)
                                {
                                    throw new OptionParsingException.OptionArgumentException("Could not parse " + parameter + " as " + localProperty.PropertyType.Name);
                                }

                                localProperty.SetValue(parameterObject, value, null);
                            });
                        }
                    }

                    var positionalArgumentAttribute = attribute as PositionalArgumentAttribute;
                    if (positionalArgumentAttribute != null)
                    {
                        if (positionalArguments.Count <= positionalArgumentAttribute.Index)
                        {
                            positionalArguments.AddRange(EnumerableEx.Repeat((Action<string>)null, positionalArgumentAttribute.Index - positionalArguments.Count + 1));
                        }

                        if (positionalArguments[positionalArgumentAttribute.Index] != null)
                        {
                            throw new InvalidOperationException("More than one property has a PositionalArgumentAttribute.Index of " + positionalArgumentAttribute.Index + ".");
                        }

                        // TODO: allow conversion overrides
                        positionalArguments[positionalArgumentAttribute.Index] = parameter =>
                        {
                            var value = parserCollection.TryParse(parameter, localProperty.PropertyType);
                            if (value == null)
                            {
                                throw new OptionParsingException.OptionArgumentException("Could not parse " + parameter + " as " + localProperty.PropertyType.Name);
                            }

                            localProperty.SetValue(parameterObject, value, null);
                        };
                    }

                    var positionalArgumentsAttribute = attribute as PositionalArgumentsAttribute;
                    if (positionalArgumentsAttribute != null)
                    {
                        if (remainingPositionalArguments != null)
                        {
                            throw new InvalidOperationException("More than one property has a PositionalArgumentsAttribute.");
                        }

                        var addMethods = localProperty.PropertyType.GetMethods().Where(x => x.Name == "Add" && x.GetParameters().Length == 1);
                        if (!addMethods.Any())
                        {
                            throw new InvalidOperationException("Property with PositionalArgumentsAttribute does not implement an Add method taking exactly one parameter.");
                        }

                        if (addMethods.Count() != 1)
                        {
                            throw new InvalidOperationException("Property with PositionalArgumentsAttribute has more than one Add method taking exactly one parameter.");
                        }

                        var addMethod = addMethods.First();

                        // TODO: allow conversion overrides
                        remainingPositionalArguments = parameter =>
                        {
                            var value = parserCollection.TryParse(parameter, addMethod.GetParameters()[0].ParameterType);
                            if (value == null)
                            {
                                throw new OptionParsingException.OptionArgumentException("Could not parse " + parameter + " as " + addMethod.GetParameters()[0].ParameterType.Name);
                            }

                            addMethod.Invoke(localProperty.GetValue(parameterObject, null), new[] { value });
                        };
                    }
                }
            }

            // Verify options.
            for (int i = 0; i != positionalArguments.Count; ++i)
            {
                if (positionalArguments[i] == null)
                {
                    throw new InvalidOperationException("No property has a PositionalArgumentAttribute with Index of " + i + ".");
                }
            }

            if (remainingPositionalArguments == null)
            {
                throw new InvalidOperationException("No property has a PositionalArgumentsAttribute.");
            }

            // Parse the command line, filling in the property values.
            var parser = new Parser(options.Keys, this.StringComparer, commandLine);
            int positionalArgumentIndex = 0;
            foreach (var option in parser)
            {
                if (option.Definition == null)
                {
                    if (positionalArgumentIndex < positionalArguments.Count)
                    {
                        positionalArguments[positionalArgumentIndex](option.Argument);
                    }
                    else
                    {
                        remainingPositionalArguments(option.Argument);
                    }

                    ++positionalArgumentIndex;
                }
                else
                {
                    options[option.Definition](option.Argument);
                }
            }
        }

        private static readonly char[] ArgumentDelimiters = new[] { ':', '=' };

        private sealed class Parser : IEnumerable<Option>
        {
            private readonly OptionDefinition[] definitions;
            private readonly StringComparer stringComparer;
            private readonly IEnumerable<string> commandLine;
            private OptionDefinition lastOption;
            private bool done;

            public Parser(IEnumerable<OptionDefinition> definitions, StringComparer stringComparer, IEnumerable<string> commandLine)
            {
                this.definitions = definitions.ToArray();
                this.stringComparer = stringComparer ?? StringComparer.CurrentCulture;
                this.commandLine = commandLine;
            }

            public IEnumerator<Option> GetEnumerator()
            {
                foreach (var value in this.commandLine)
                {
                    // If lastOption is not null, then it must be an option that can take an argument.
                    Contract.Assert(this.lastOption == null || this.lastOption.Argument != OptionArgument.None);

                    if (this.done)
                    {
                        Contract.Assert(this.lastOption == null);
                        yield return new Option { Argument = value };
                        continue;
                    }

                    if (this.lastOption == null || this.lastOption.Argument == OptionArgument.Optional)
                    {
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
                                option = value.Substring(2);
                            }
                            else
                            {
                                option = value.Substring(2, argumentIndex - 2);
                                argument = value.Substring(argumentIndex + 1);
                            }

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
                            this.lastOption = this.definitions.FirstOrDefault(x => this.stringComparer.Equals(x.ShortName.ToString(), option));
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
                                    this.lastOption = this.definitions.FirstOrDefault(x => this.stringComparer.Equals(x.ShortName.ToString(), option));
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

                            this.lastOption = this.definitions.FirstOrDefault(x => this.stringComparer.Equals(x.LongName, option) || this.stringComparer.Equals(x.ShortName.ToString(), option));
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

                    // Argument for a preceding option
                    yield return new Option { Definition = this.lastOption, Argument = value };
                    this.lastOption = null;
                    continue;
                }

                if (this.lastOption != null)
                {
                    if (this.lastOption.Argument == OptionArgument.Required)
                    {
                        throw new OptionParsingException.OptionArgumentException("Missing argument for option --" + this.lastOption.LongName);
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
