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

            /// <summary>
            /// The quote has just been closed, but the argument is still being parsed.
            /// </summary>
            EndQuotedArgument,
        }

        /// <summary>
        /// A string buffer combined with a backslash count.
        /// </summary>
        private sealed class Buffer
        {
            private string result;
            private int backslashes;

            public Buffer()
            {
                this.result = string.Empty;
                this.backslashes = 0;
            }

            /// <summary>
            /// Adds any outstanding backslashes to the result, and resets the backslash count.
            /// </summary>
            private void Normalize()
            {
                this.result += new string('\\', this.backslashes);
                this.backslashes = 0;
            }

            /// <summary>
            /// Appends a character to the buffer. If the character is a double-quote, it is treated like an ordinary character. The character may not be a backslash.
            /// </summary>
            /// <param name="ch">The character. May not be a backslash.</param>
            public void AppendNormalChar(char ch)
            {
                Contract.Requires(ch != '\\');

                this.Normalize();
                this.result += ch;
            }

            /// <summary>
            /// Appends a backslash to the buffer.
            /// </summary>
            public void AppendBackslash()
            {
                ++this.backslashes;
            }

            /// <summary>
            /// Processes a double-quote, which may add it to the buffer. Returns <c>true</c> if there were an even number of backslashes.
            /// </summary>
            /// <returns><c>true</c> if there were an even number of backslashes.</returns>
            public bool AppendQuote()
            {
                this.result += new string('\\', this.backslashes / 2);
                var ret = ((this.backslashes % 2) == 0);
                this.backslashes = 0;
                if (!ret)
                {
                    // An odd number of backslashes means the double-quote is escaped.
                    this.result += '"';
                }

                return ret;
            }

            /// <summary>
            /// Appends a regular character or backslash to the buffer.
            /// </summary>
            /// <param name="ch">The character to append. May not be a double quote.</param>
            public void AppendChar(char ch)
            {
                Contract.Requires(ch != '"');

                if (ch == '\\')
                    this.AppendBackslash();
                else
                    this.AppendNormalChar(ch);
            }

            /// <summary>
            /// Consumes the buffer so far, resetting the buffer and backslash count.
            /// </summary>
            /// <returns>The buffer.</returns>
            public string Consume()
            {
                this.Normalize();
                var ret = this.result;
                this.result = string.Empty;
                return ret;
            }
        }

        /// <summary>
        /// Lexes the command line, using the same rules as <see cref="Environment.GetCommandLineArgs"/>.
        /// </summary>
        /// <param name="commandLine">The command line to parse.</param>
        /// <returns>The lexed command line.</returns>
        public static IEnumerable<string> Lex(this string commandLine)
        {
            Contract.Requires(commandLine != null);
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            // The MSDN information for <see cref="Environment.GetCommandLineArgs"/> is incomplete.
            // This blog post fills in the gaps: http://www.hardtoc.com/archives/162 (webcite: http://www.webcitation.org/62LHTVelJ )

            LexerState state = LexerState.Default;

            Buffer buffer = new Buffer();
            foreach (var ch in commandLine)
            {
                switch (state)
                {
                    case LexerState.Default:
                        if (ch == '"')
                        {
                            // Enter the quoted state, without placing anything in the buffer.
                            state = LexerState.Quoted;
                            break;
                        }

                        // Whitespace is ignored.
                        if (ch == ' ' || ch == '\t')
                        {
                            break;
                        }

                        buffer.AppendChar(ch);
                        state = LexerState.Argument;
                        break;

                    case LexerState.Argument:
                        // We have an argument started, though it may be just an empty string for now.

                        if (ch == '"')
                        {
                            // Handle the special rules for any backslashes preceding a double-quote.
                            if (buffer.AppendQuote())
                            {
                                // An even number of backslashes means that this is a normal double-quote.
                                state = LexerState.Quoted;
                            }

                            break;
                        }

                        if (ch == ' ' || ch == '\t')
                        {
                            // Whitespace ends this argument, so publish it and restart in the default state.
                            yield return buffer.Consume();
                            state = LexerState.Default;
                            break;
                        }

                        // Count backslashes; put other characters directly into the buffer.
                        buffer.AppendChar(ch);
                        break;

                    case LexerState.Quoted:
                        // We are within quotes, but may already have characters in the argument buffer.

                        if (ch == '"')
                        {
                            // Handle the special rules for any backslashes preceding a double-quote.
                            if (buffer.AppendQuote())
                            {
                                // An even number of backslashes means that this is a normal double-quote.
                                state = LexerState.EndQuotedArgument;
                            }

                            break;
                        }

                        // Any non-quote character (including whitespace) is appended to the argument buffer.
                        buffer.AppendChar(ch);
                        break;

                    case LexerState.EndQuotedArgument:
                        // This is a special state that is treated like Argument or Quoted depending on whether the next character is a quote. It's not possible to stay in this state.

                        if (ch == '"')
                        {
                            // We just read a double double-quote within a quoted context, so we add the quote to the buffer and re-enter the quoted state.
                            buffer.AppendNormalChar(ch);
                            state = LexerState.Quoted;
                        }
                        else if (ch == ' ' || ch == '\t')
                        {
                            // In this case, the double-quote we just read did in fact end the quotation, so we publish the argument and restart in the default state.
                            yield return buffer.Consume();
                            state = LexerState.Default;
                        }
                        else
                        {
                            // If the double-quote is followed by a non-quote, non-whitespace character, then it's considered a continuation of the argument (leaving the quoted state).
                            buffer.AppendChar(ch);
                            state = LexerState.Argument;
                        }

                        break;
                }
            }

            // If we end in the middle of an argument (or even a quotation), then we just publish what we have.
            if (state != LexerState.Default)
            {
                yield return buffer.Consume();
            }
        }

        /// <summary>
        /// Lexes the command line for this process, using the same rules as <see cref="Environment.GetCommandLineArgs"/>. The returned command line includes the process name.
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
