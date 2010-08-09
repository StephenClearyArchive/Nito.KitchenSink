using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.CharacterStreams.DelimitedText
{
    using System.Collections;
    using System.Diagnostics;

    /// <summary>
    /// A class that encapulates state logic for lexing an observable sequence of characters to an observable sequence of tokens.
    /// </summary>
    public sealed class ObservableLexer : IEnumerable<IObservable<object>>
    {
        /// <summary>
        /// The trace source to which messages are written during lexing.
        /// </summary>
        private readonly static TraceSource Tracer = new TraceSource("Nito.KitchenSink.CharacterStreams.DelimitedText.ObservableLexer", SourceLevels.Information);

        /// <summary>
        /// The character used as a field separator.
        /// </summary>
        private readonly char fieldSeparator;

        /// <summary>
        /// The observer of this sequence.
        /// </summary>
        private readonly IObserver<Token> observer;

        /// <summary>
        /// The source of characters to lex.
        /// </summary>
        private readonly IObservable<char> source;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableLexer"/> class with the specified observer.
        /// </summary>
        /// <param name="observer">The observer of this sequence.</param>
        /// <param name="source">The source of characters to lex.</param>
        public ObservableLexer(IObserver<Token> observer, IObservable<char> source, char fieldSeparator = ',')
        {
            this.observer = observer;
            this.source = source;
            this.fieldSeparator = fieldSeparator;
        }

        /// <summary>
        /// Lexes the source stream, when used with <see cref="Observable.Iterate"/>.
        /// </summary>
        /// <returns>A sequence of asynchronous blocking observables.</returns>
        public IEnumerator<IObservable<object>> GetEnumerator()
        {
            // Receive the first character.
            var ret = source.Take(1).Start();
            yield return ret;

            while (true)
            {
                // Stop producing tokens as soon as there are no more characters.
                if (ret.Count == 0)
                {
                    Information("Lexer: no more tokens.");
                    yield break;
                }

                char ch = ret[0];
                if (ch == this.fieldSeparator)
                {
                    // Publish a FieldSeparator token.
                    Information("Lexer: field separator.");
                    this.observer.OnNext(new Tokens.FieldSeparator());
                }
                else if (ch == '\r')
                {
                    // Publish an EndOfRecord token.
                    Information("Lexer: end of record.");
                    this.observer.OnNext(new Tokens.EndOfRecord());

                    // Consume a '\n' character if it immediately follows the '\r'.
                    ret = source.Take(1).Start();
                    yield return ret;

                    if (ret.Count == 0 || ret[0] != '\n')
                    {
                        // Do not consume the character if it is not '\n'.
                        continue;
                    }
                }
                else if (ch == '\n')
                {
                    // Publish an EndOfRecord token.
                    Information("Lexer: end of record.");
                    this.observer.OnNext(new Tokens.EndOfRecord());
                }
                else
                {
                    // At this point, the token must be FieldData, so read in the field data, one character at a time.
                    var sb = new StringBuilder();

                    if (ch != '\"')
                    {
                        // The field data is not escaped.
                        
                        sb.Append(ch);
                        while (true)
                        {
                            // Read the next character.
                            ret = source.Take(1).Start();
                            yield return ret;

                            if (ret.Count == 0)
                            {
                                // We reached the end of the source sequence, so publish the last FieldData token.
                                Information("Lexer: unescaped field data (at end of input).");
                                this.observer.OnNext(new Tokens.FieldData { Data = sb.ToString() });

                                Information("Lexer: no more tokens.");
                                yield break;
                            }

                            ch = ret[0];
                            if (ch == this.fieldSeparator || ch == '\r' || ch == '\n')
                            {
                                // Unescaped field data can be terminated by a FieldSeparator or EndOfRecord.
                                // Publish the FieldData token.
                                Information("Lexer: unescaped field data.");
                                this.observer.OnNext(new Tokens.FieldData { Data = sb.ToString() });
                                break;
                            }

                            sb.Append(ch);
                        }

                        // Do not consume the character for the FieldSeparator or EndOfRecord tokens.
                        continue;
                    }
                    else
                    {
                        // The field data is escaped.

                        while (true)
                        {
                            // Read the next character.
                            ret = source.Take(1).Start();
                            yield return ret;

                            if (ret.Count == 0)
                            {
                                // We reached the end of the source sequence, so publish the last FieldData token (with a warning, since the ending double quote was never found).
                                Information("Lexer: escaped field data.");
                                this.observer.OnNext(new Tokens.FieldData { Data = sb.ToString() });
                            }

                            ch = ret[0];
                            if (ch == '\"')
                            {
                                // A '\"' character within escaped field data may be the end of the escaped field data or it may be the first character of a 2DQUOTE escape sequence.
                                
                                // Read the next character.
                                ret = source.Take(1).Start();
                                yield return ret;

                                if (ret.Count == 0 || ret[0] != '\"')
                                {
                                    // The '\"' character was the end of the escaped field data.
                                    Information("Lexer: escaped field data.");
                                    this.observer.OnNext(new Tokens.FieldData { Data = sb.ToString() });
                                    break;
                                }
                            }

                            sb.Append(ch);
                        }

                        // Do not consume the last character (or EOF) read.
                        continue;
                    }
                }

                // Receive the next character.
                ret = source.Take(1).Start();
                yield return ret;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Traces the specified informational message.
        /// </summary>
        /// <param name="message">The informational message to trace.</param>
        private static void Information(string message)
        {
            Tracer.TraceEvent(TraceEventType.Information, 0, message);
        }

        /// <summary>
        /// Traces the specified warning message.
        /// </summary>
        /// <param name="message">The warning message to trace.</param>
        private static void Warning(string message)
        {
            Tracer.TraceEvent(TraceEventType.Warning, 0, message);
        }
    }
}
