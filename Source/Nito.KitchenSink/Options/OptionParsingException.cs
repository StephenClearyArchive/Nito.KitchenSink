using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.Options
{
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
        /// An option argument error was encountered during command-line option parsing.
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
