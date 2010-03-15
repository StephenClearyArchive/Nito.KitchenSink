// <copyright file="IFramer.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.Communication
{
    using System.Collections.Generic;

    /// <summary>
    /// Receives notification that a message has arrived.
    /// </summary>
    /// <param name="message">The message that has arrived. This may contain an alias of the data passed to <see cref="IFramer.DataReceived"/> in this call stack, but will not contain an alias to any data previously passed to <see cref="IFramer.DataReceived"/>.</param>
    public delegate void MessageArrivedEventHandler(IList<byte> message);

    /// <summary>
    /// A message framer. Converts a stream of bytes into a stream of messages, represented as byte arrays.
    /// </summary>
    public interface IFramer
    {
        /// <summary>
        /// Occurs when a message has arrived. Exceptions thrown from this method propogate through <see cref="DataReceived"/>, and may leave the framer instance in an invalid state.
        /// </summary>
        event MessageArrivedEventHandler MessageArrived;

        /// <summary>
        /// Notifies the framer instance that incoming data has been received from the stream. This method will invoke <see cref="MessageArrived"/> as necessary.
        /// </summary>
        /// <remarks>
        /// <para>This method may invoke <see cref="MessageArrived"/> zero or more times.</para>
        /// <para>Zero-length receives are ignored. May streams use a 0-length read to indicate the end of a stream, but the framer takes no action in this case.</para>
        /// </remarks>
        /// <param name="data">The data received from the stream. Cannot be null. May be a slice of the read buffer for the stream.</param>
        /// <exception cref="System.Net.ProtocolViolationException">If the data received is not a properly-formed message.</exception>
        void DataReceived(IList<byte> data);

        /// <summary>
        /// Re-initializes the framer instance to a clean state. After this method returns, the framer instance is identical to a newly-constructed instance.
        /// </summary>
        void Reset();
    }
}
