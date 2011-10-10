// <copyright file="ConsoleCommandLineLexer.cs" company="Nito Programs">
//     Copyright (c) 2011 Nito Programs.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.OptionParsing
{
    /// <summary>
    /// Provides the default Win32 Console command-line lexing.
    /// </summary>
    public static class ConsoleCommandLineLexer
    {
        private enum LexerState
        {
            /// <summary>
            /// The default state; no data exists in the argument character buffer.
            /// </summary>
            Default,

            /// <summary>
            /// An argument has been started.
            /// </summary>
            Argument,

            /// <summary>
            /// A quote character has been seen, and we are now parsing quoted data.
            /// </summary>
            Quoted,
        }

        /// <summary>
        /// Lexes the command line, using the same rules as CommandLineToArgvW.
        /// </summary>
        /// <param name="commandLine">The command line to parse.</param>
        /// <returns>The lexed command line.</returns>
        public static IEnumerable<string> Lex(this string commandLine)
        {
            Contract.Requires(commandLine != null);
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            LexerState state = LexerState.Default;
            int backslashes = 0;

            string result = string.Empty;
            foreach (var ch in commandLine)
            {
                if (ch == '"')
                {
                    // Handle the special rules for any backslashes preceding a double-quote.
                    result += new string('\\', backslashes / 2);
                    if ((backslashes % 2) == 0)
                    {
                        // An even number of backslashes means that this is a normal double-quote.
                        if (state == LexerState.Quoted)
                            state = LexerState.Argument;
                        else
                            state = LexerState.Quoted;
                    }
                    else
                    {
                        // An odd number of backslashes means the double-quote is escaped.
                        result += '"';
                        Contract.Assume(state != LexerState.Default);
                    }

                    backslashes = 0;
                }
                else if (ch == '\\')
                {
                    ++backslashes;
                    if (state == LexerState.Default)
                        state = LexerState.Argument;
                }
                else
                {
                    // Consume any backslashes preceding the whitespace or normal character.
                    result += new string('\\', backslashes);
                    backslashes = 0;

                    // CommandLineToArgvW only recognizes spaces and tabs as whitespace.
                    if (ch == ' ' || ch == '\t')
                    {
                        if (state == LexerState.Quoted)
                        {
                            // Quoted whitespace characters are just added to the token.
                            result += ch;
                        }
                        else if (state == LexerState.Argument)
                        {
                            // Unquoted whitespace characters separate tokens.
                            yield return result;
                            result = string.Empty;
                            state = LexerState.Default;
                        }
                    }
                }
            }

            result += new string('\\', backslashes);

            if (state != LexerState.Default)
            {
                yield return result;
            }
        }

        /// <summary>
        /// Lexes the command line for this process, using the same rules as CommandLineToArgvW. The returned command line includes the process name.
        /// </summary>
        /// <returns>The lexed command line.</returns>
        public static IEnumerable<string> Lex()
        {
            return Environment.GetCommandLineArgs();
        }

        /// <summary>
        /// Takes a list of arguments to pass to a program, and quotes them. This method does not quote or escape special shell characters (see <see cref="CommandPromptCommandLine"/>).
        /// </summary>
        /// <param name="arguments">The arguments to quote (if necessary) and concatenate into a command line.</param>
        /// <returns>The command line.</returns>
        public static string Escape(IEnumerable<string> arguments)
        {
            Contract.Requires(arguments != null);
            Contract.Ensures(Contract.Result<string>() != null);

            // Escape each argument (if necessary) and join them with spaces.
            return string.Join(" ", arguments.Select(argument =>
            {
                Contract.Assume(argument != null);

                // An argument does not need escaping if it does not have any whitespace or quote characters.
                if (!argument.Any(ch => ch == ' ' || ch == '\t' || ch == '"') && argument != string.Empty)
                {
                    return argument;
                }

                // To escape the argument, wrap it in double-quotes and escape existing double-quotes, doubling any existing escape characters but only if they precede a double-quote.
                var ret = new StringBuilder();
                ret.Append('"');
                int backslashes = 0;
                foreach (var ch in argument)
                {
                    if (ch == '\\')
                    {
                        ++backslashes;
                    }
                    else if (ch == '"')
                    {
                        ret.Append(new string('\\', 2 * backslashes + 1));
                        backslashes = 0;
                        ret.Append(ch);
                    }
                    else
                    {
                        ret.Append(new string('\\', backslashes));
                        backslashes = 0;
                        ret.Append(ch);
                    }
                }

                ret.Append(new string('\\', backslashes));
                ret.Append('"');
                return ret.ToString();
            }));
        }
    }
}
