// <copyright file="OptionFiles.cs" company="Nito Programs">
//     Copyright (c) 2011 Nito Programs.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nito.KitchenSink.OptionParsing
{
    /// <summary>
    /// Provides methods to enable options files.
    /// </summary>
    public static class OptionFiles
    {
        /// <summary>
        /// Lexes the command-line, allowing option files.
        /// </summary>
        /// <param name="commandLine">The command line to lex. If this is <c>null</c>, the command line for the current process is used.</param>
        /// <param name="optionFilesCharacter">The character used to designate an option file. Defaults to <c>@</c>. Two option file characters are treated as a single literal (non-option file) character.</param>
        /// <param name="lexer">The lexer to use to lex the command line and option files. If this is <c>null</c>, <see cref="ConsoleCommandLineLexer"/> is used to lex the command line and option files.</param>
        /// <returns>The combined options and arguments, including those referenced in option files.</returns>
        public static IEnumerable<string> LexWithOptionFiles(string commandLine = null, char optionFilesCharacter = '@', Func<string, IEnumerable<string>> lexer = null)
        {
            commandLine = commandLine ?? Environment.CommandLine;
            lexer = lexer ?? ConsoleCommandLineLexer.Lex;
            var singleOptionFilesCharacter = new string(optionFilesCharacter, 1);
            var doubleOptionFilesCharacter = new string(optionFilesCharacter, 2);

            var source = lexer(commandLine).Skip(1);
            foreach (var str in source)
            {
                if (!str.StartsWith(singleOptionFilesCharacter))
                {
                    yield return str;
                }
                else
                {
                    if (str.StartsWith(doubleOptionFilesCharacter))
                    {
                        yield return str.Substring(1);
                    }
                    else
                    {
                        IEnumerable<string> additionalOptions;
                        try
                        {
                            additionalOptions = File.ReadAllLines(str.Substring(1)).SelectMany(x => lexer(x));
                        }
                        catch (Exception ex)
                        {
                            throw new OptionParsingException.OptionArgumentException("Could not read option file  " + str.Substring(1), ex);
                        }

                        foreach (var strFromFile in additionalOptions)
                        {
                            yield return strFromFile;
                        }
                    }
                }
            }
        }
    }
}
