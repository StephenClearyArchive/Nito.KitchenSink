// <copyright file="OptionArguments.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.OptionParsing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using Nito.KitchenSink.SimpleParsers;

    /// <summary>
    /// An arguments class, which uses option attributes on its properties.
    /// </summary>
    public interface IOptionArguments
    {
        /// <summary>
        /// Validates the arguments by throwing <see cref="OptionParsingException"/> errors as necessary.
        /// </summary>
        void Validate();
    }

    /// <summary>
    /// Provides extension methods for <see cref="IOptionArguments"/> implementations (arguments objects).
    /// </summary>
    public static class OptionArgumentsExtensions
    {
        /// <summary>
        /// Gets a friendly name for a type (using angle brackets with generic parameters instead of backticks, and replacing <c>Nullable&lt;T&gt;</c> with <c>T?</c>).
        /// </summary>
        /// <param name="type">The type whose name is returned.</param>
        /// <returns>The friendly name for the type.</returns>
        private static string FriendlyTypeName(Type type)
        {
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<string>() != null);

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return FriendlyTypeName(underlyingType) + "?";
            }
            
            if (type.IsGenericType)
            {
                var backtickIndex = type.Name.IndexOf('`');
                Contract.Assume(backtickIndex > 0);
                return type.Name.Substring(0, backtickIndex) + "<" + string.Join(", ", type.GetGenericArguments().Select(FriendlyTypeName)) + ">";
            }

            return type.Name;
        }

        /// <summary>
        /// Sets a property on an arguments object, translating exceptions.
        /// </summary>
        /// <param name="property">The property to set.</param>
        /// <param name="argumentsObject">The arguments object.</param>
        /// <param name="value">The value for the property.</param>
        private static void SetOptionProperty(this PropertyInfo property, object argumentsObject, object value)
        {
            Contract.Requires(property != null);
            Contract.Requires(argumentsObject != null);

            try
            {
                property.SetValue(argumentsObject, value, null);
            }
            catch (TargetInvocationException ex)
            {
                throw new OptionParsingException.OptionArgumentException(ex.InnerException.Message, ex.InnerException);
            }
        }

        /// <summary>
        /// Parses the command line into an arguments object. Option definitions are determined by the attributes on the properties of <paramref name="argumentsObject"/>. This method will call <see cref="IOptionArguments.Validate"/>.
        /// </summary>
        /// <typeparam name="T">The type of arguments object to initialize.</typeparam>
        /// <param name="argumentsObject">The arguments object that is initialized. May not be <c>null</c>.</param>
        /// <param name="commandLine">The command line to parse. If <c>null</c>, the process' command line is lexed according to standard .NET rules.</param>
        /// <param name="parserCollection">A parser collection to use for parsing, or <c>null</c> to use the default parsers.</param>
        /// <param name="stringComparer">The string comparison to use when parsing options. If <c>null</c>, then the string comparer for the current culture is used.</param>
        public static void Parse<T>(this T argumentsObject, IEnumerable<string> commandLine = null, SimpleParserCollection parserCollection = null, StringComparer stringComparer = null) where T : class, IOptionArguments
        {
            Contract.Requires(argumentsObject != null);

            if (parserCollection == null)
            {
                parserCollection = new SimpleParserCollection();
            }

            if (stringComparer == null)
            {
                stringComparer = StringComparer.CurrentCulture;
            }

            // Generate option definitions from OptionAttributes on the arguments object.
            var options = new Dictionary<OptionDefinition, Action<string>>();
            var positionalArguments = new List<Action<string>>();
            Action<string> remainingPositionalArguments = null;
            var argumentsObjectType = argumentsObject.GetType();
            foreach (var property in argumentsObjectType.GetProperties())
            {
                var localProperty = property;

                // If the property specifies a [SimpleParser], then create a parser for that property.
                var parserOverrideAttribute = property.GetCustomAttributes(typeof(SimpleParserAttribute), true).OfType<SimpleParserAttribute>().FirstOrDefault();
                var parserOverride = ((parserOverrideAttribute == null) ? null : Activator.CreateInstance(parserOverrideAttribute.ParserType)) as ISimpleParser;

                foreach (var attribute in property.GetCustomAttributes(true))
                {
                    // Handle [Option] attributes.
                    var optionAttribute = attribute as OptionAttribute;
                    if (optionAttribute != null)
                    {
                        var optionDefinition = new OptionDefinition { LongName = optionAttribute.LongName, ShortName = optionAttribute.ShortName, Argument = optionAttribute.Argument };
                        if (optionDefinition.Argument == OptionArgument.None)
                        {
                            // If the option takes no arguments, it must be applied to a boolean property.
                            if (localProperty.PropertyType != typeof(bool))
                            {
                                throw new InvalidOperationException("An OptionAttribute with no Argument may only be applied to a boolean property.");
                            }

                            // If the option is specified, set the property to true.
                            options.Add(optionDefinition, _ => localProperty.SetOptionProperty(argumentsObject, true));
                        }
                        else
                        {
                            // If the option takes an argument, then attempt to parse it to the correct type.
                            options.Add(optionDefinition, parameter =>
                            {
                                if (parameter == null)
                                    return;

                                var value = parserOverride != null ? parserOverride.TryParse(parameter) : parserCollection.TryParse(parameter, localProperty.PropertyType);
                                if (value == null)
                                {
                                    throw new OptionParsingException.OptionArgumentException("Could not parse  " + parameter + "  as " + FriendlyTypeName(Nullable.GetUnderlyingType(localProperty.PropertyType) ?? localProperty.PropertyType));
                                }

                                localProperty.SetOptionProperty(argumentsObject, value);
                            });
                        }
                    }

                    // Handle [PositionalArgument] attributes.
                    var positionalArgumentAttribute = attribute as PositionalArgumentAttribute;
                    if (positionalArgumentAttribute != null)
                    {
                        if (positionalArguments.Count <= positionalArgumentAttribute.Index)
                        {
                            positionalArguments.AddRange(new Action<string>[positionalArgumentAttribute.Index - positionalArguments.Count + 1]);
                            Contract.Assume(positionalArguments.Count > positionalArgumentAttribute.Index);
                        }

                        if (positionalArguments[positionalArgumentAttribute.Index] != null)
                        {
                            throw new InvalidOperationException("More than one property has a PositionalArgumentAttribute.Index of " + positionalArgumentAttribute.Index + ".");
                        }

                        // If the positional argument is specified, then attempt to parse it to the correct type.
                        positionalArguments[positionalArgumentAttribute.Index] = parameter =>
                        {
                            Contract.Assume(parameter != null);
                            var value = parserOverride != null ? parserOverride.TryParse(parameter) : parserCollection.TryParse(parameter, localProperty.PropertyType);
                            if (value == null)
                            {
                                throw new OptionParsingException.OptionArgumentException("Could not parse  " + parameter + "  as " + FriendlyTypeName(Nullable.GetUnderlyingType(localProperty.PropertyType) ?? localProperty.PropertyType));
                            }

                            localProperty.SetOptionProperty(argumentsObject, value);
                        };
                    }

                    // Handle [PositionalArguments] attributes.
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

                        // As the remaining positional arguments are specified, then attempt to parse it to the correct type and add it to the collection.
                        remainingPositionalArguments = parameter =>
                        {
                            Contract.Assume(parameter != null);
                            var value = parserOverride != null ? parserOverride.TryParse(parameter) : parserCollection.TryParse(parameter, addMethod.GetParameters()[0].ParameterType);
                            if (value == null)
                            {
                                throw new OptionParsingException.OptionArgumentException("Could not parse  " + parameter + "  as " + FriendlyTypeName(Nullable.GetUnderlyingType(addMethod.GetParameters()[0].ParameterType) ?? addMethod.GetParameters()[0].ParameterType));
                            }

                            addMethod.Invoke(localProperty.GetValue(argumentsObject, null), new[] { value });
                        };
                    }
                }
            }

            // Handle [OptionPresent] attributes.
            var optionDefinitions = options.Keys;
            foreach (var property in argumentsObjectType.GetProperties())
            {
                var localProperty = property;

                var optionPresentAttribute = property.GetCustomAttributes(typeof(OptionPresentAttribute), true).OfType<OptionPresentAttribute>().FirstOrDefault();
                if (optionPresentAttribute == null)
                    continue;

                // This attribute must be applied to a boolean property.
                if (localProperty.PropertyType != typeof(bool))
                {
                    throw new InvalidOperationException("An OptionPresentAttribute may only be applied to a boolean property.");
                }

                OptionDefinition optionDefinition = null;
                if (optionPresentAttribute.LongName != null)
                {
                    optionDefinition = optionDefinitions.FirstOrDefault(x => stringComparer.Equals(x.LongName, optionPresentAttribute.LongName));
                }
                else
                {
                    optionDefinition = optionDefinitions.FirstOrDefault(x => stringComparer.Equals(x.ShortNameAsString, optionPresentAttribute.ShortNameAsString));
                }

                if (optionDefinition == null)
                {
                    throw new InvalidOperationException("OptionPresentAttribute does not refer to an existing OptionAttribute for option " + optionPresentAttribute.Name);
                }

                // If the option is specified, set the property to true.
                options[optionDefinition] = (Action<string>)Delegate.Combine(options[optionDefinition], (Action<string>)(_ => localProperty.SetOptionProperty(argumentsObject, true)));
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
            var parser = OptionParser.Parse(commandLine, optionDefinitions, stringComparer);
            int positionalArgumentIndex = 0;
            foreach (var option in parser)
            {
                Contract.Assume(option != null);
                if (option.Definition == null)
                {
                    if (positionalArgumentIndex < positionalArguments.Count)
                    {
                        var action = positionalArguments[positionalArgumentIndex];
                        Contract.Assume(action != null);
                        action(option.Argument);
                    }
                    else
                    {
                        remainingPositionalArguments(option.Argument);
                    }

                    ++positionalArgumentIndex;
                }
                else
                {
                    var action = options[option.Definition];
                    Contract.Assume(action != null);
                    action(option.Argument);
                }
            }

            // Have the arguments object perform its own validation.
            argumentsObject.Validate();
        }
    }
}
