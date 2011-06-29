// <copyright file="OptionParsingException.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.OptionParsing
{
    using System;

    /// <summary>
    /// An error was encountered during command-line option parsing.
    /// </summary>
    public class OptionParsingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionParsingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public OptionParsingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionParsingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The exception that is the root cause of the error.</param>
        public OptionParsingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// An option argument error was encountered during command-line option parsing. This includes argument parsing errors.
        /// </summary>
        public class OptionArgumentException : OptionParsingException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OptionParsingException.UnknownOptionException"/> class.
            /// </summary>
            /// <param name="message">The message.</param>
            public OptionArgumentException(string message)
                : base(message)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="OptionArgumentException"/> class.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="innerException">The exception that is the root cause of the error.</param>
            public OptionArgumentException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }

        /// <summary>
        /// An unknown option was encountered during command-line option parsing.
        /// </summary>
        public class UnknownOptionException : OptionParsingException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OptionParsingException.UnknownOptionException"/> class.
            /// </summary>
            /// <param name="message">The message.</param>
            public UnknownOptionException(string message)
                : base(message)
            {
            }
        }

        /// <summary>
        /// An invalid option or argument was encountered during command-line option parsing.
        /// </summary>
        public class InvalidParameterException : OptionParsingException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OptionParsingException.InvalidParameterException"/> class.
            /// </summary>
            /// <param name="message">The message.</param>
            public InvalidParameterException(string message)
                : base(message)
            {
            }
        }
    }
}
