// <copyright file="CommandPromptCommandLine.cs" company="Nito Programs">
//     Copyright (c) 2011 Nito Programs.
// </copyright>

using System.Diagnostics.Contracts;
using System.Text;

namespace Nito.KitchenSink.OptionParsing
{
    /// <summary>
    /// Provides command-prompt-compatible escaping for command lines.
    /// </summary>
    public static class CommandPromptCommandLine
    {
        /// <summary>
        /// Takes a command line to pass to a program, and escapes any unquoted characters that are special to the command prompt shell: <c>&amp; | ( ) &lt; &gt; ^</c>
        /// </summary>
        /// <param name="commandLine">The command line to pass to the program.</param>
        /// <returns>The command line to send to the command prompt shell.</returns>
        public static string Escape(string commandLine)
        {
            Contract.Requires(commandLine != null);
            Contract.Ensures(Contract.Result<string>() != null);

            var ret = new StringBuilder();
            bool inQuote = false;
            foreach (var ch in commandLine)
            {
                if (ch == '"')
                {
                    // Quote characters begin or end a quoted context, and are always passed through.
                    inQuote = !inQuote;
                    ret.Append(ch);
                }
                else if (inQuote)
                {
                    // If we're in a quoted context, no special characters need to be escaped.
                    ret.Append(ch);
                }
                else if (ch == '&' || ch == '|' || ch == '(' || ch == ')' || ch == '<' || ch == '>' || ch == '^')
                {
                    // Special characters outside a quoted context need to be escaped.
                    ret.Append('^');
                    ret.Append(ch);
                }
                else
                {
                    // Regular characters outside a quoted context do not need to be escaped.
                    ret.Append(ch);
                }
            }

            return ret.ToString();
        }
    }
}
