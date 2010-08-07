using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.CharacterStreams
{
    /// <summary>
    /// A string token, representing some string data.
    /// </summary>
    public class StringToken : Token
    {
        /// <summary>
        /// The data for this token.
        /// </summary>
        public string Data { get; set; }
    }
}
