// <copyright file="MultipleByteMultipleDelimiterFramer.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.Communication
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Nito.KitchenSink;
    using Nito.Linq;

    /// <summary>
    /// Provides message framing for multiple-byte-delimiter-based protocols that support multiple delimiters. Does not perform any unescaping of the message data.
    /// </summary>
    /// <remarks>
    /// <para>No begin delimiter may start with another begin delimiter.</para>
    /// </remarks>
    public sealed class MultipleByteMultipleDelimiterFramer
    {
        /// <summary>
        /// The maximum size of messages allowed, or 0 if there is no maximum.
        /// </summary>
        private readonly int maxMessageSize;

        /// <summary>
        /// The beginning delimiters.
        /// </summary>
        private readonly byte[][] beginDelimiters;

        /// <summary>
        /// The ending delimiters.
        /// </summary>
        private readonly byte[][] endDelimiters;

        /// <summary>
        /// The trace source used for all communications messages.
        /// </summary>
        private static TraceSource trace = new TraceSource("Sockets", SourceLevels.All);

        /// <summary>
        /// The data buffer, which grows dynamically as more data arrives. This is never null. This buffer contains the begin delimiter but not the end delimiter.
        /// </summary>
        private List<byte> dataBuffer;

        /// <summary>
        /// The index of the matching begin delimiter at the front of the data buffer, and also the index of the end delimiter that must match. This is -1 if there are no matching begin delimiters yet.
        /// </summary>
        private int delimiterIndex;

        /// <summary>
        /// How much of the end delimiter has been seen (implicitly at the end of the data buffer).
        /// </summary>
        private int endDelimiterIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleByteMultipleDelimiterFramer"/> class.
        /// </summary>
        /// <param name="maxMessageSize">Maximum size of messages, or 0 if message size is not restricted.</param>
        /// <param name="beginDelimiters">The begin delimiters.</param>
        /// <param name="endDelimiters">The end delimiters.</param>
        public MultipleByteMultipleDelimiterFramer(int maxMessageSize, IEnumerable<byte[]> beginDelimiters, IEnumerable<byte[]> endDelimiters)
        {
            this.maxMessageSize = maxMessageSize;
            this.beginDelimiters = beginDelimiters.ToArray();
            this.endDelimiters = endDelimiters.ToArray();
            this.dataBuffer = new List<byte>();
            this.delimiterIndex = -1;
            if (this.beginDelimiters.Length != this.endDelimiters.Length)
            {
                throw new ArgumentException("The sequence of begin delimiters must have the same number of elements as the sequence of end delimiters");
            }
        }

        /// <summary>
        /// Receives notification that a message has arrived.
        /// </summary>
        /// <param name="delimiterIndex">The index of the matching begin/end delimiters for this message.</param>
        /// <param name="message">The message that has arrived. This may contain an alias of the data passed to <see cref="IFramer.DataReceived"/> in this call stack, but will not contain an alias to any data previously passed to <see cref="IFramer.DataReceived"/>.</param>
        public delegate void MessageArrivedEventHandler(int delimiterIndex, IList<byte> message);

        /// <summary>
        /// Occurs when a message has arrived. Exceptions thrown from this method propogate through <see cref="DataReceived"/>, and may leave the framer instance in an invalid state.
        /// </summary>
        public event MessageArrivedEventHandler MessageArrived;

        /// <summary>
        /// Re-initializes the framer instance to a clean state. After this method returns, the framer instance is identical to a newly-constructed instance.
        /// </summary>
        public void Reset()
        {
            this.dataBuffer.Clear();
            this.delimiterIndex = -1;
            this.endDelimiterIndex = 0;
        }

        /// <summary>
        /// Notifies the framer instance that incoming data has been received from the stream. This method will invoke <see cref="MessageArrived"/> as necessary.
        /// </summary>
        /// <remarks>
        /// <para>This method may invoke <see cref="MessageArrived"/> zero or more times.</para>
        /// <para>Zero-length receives are ignored. May streams use a 0-length read to indicate the end of a stream, but the framer takes no action in this case.</para>
        /// </remarks>
        /// <param name="data">The data received from the stream. Cannot be null. May be a slice of the read buffer for the stream.</param>
        /// <exception cref="System.Net.ProtocolViolationException">If the data received is not a properly-formed message.</exception>
        public void DataReceived(IList<byte> data)
        {
            int i = 0;
            while (i != data.Count)
            {
                int bytesAvailable = data.Count - i;
                if (this.delimiterIndex == -1)
                {
                    // We are searching for the begin delimiter
                    bool waiting = false;
                    for (int index = 0; index != this.beginDelimiters.Length; ++index)
                    {
                        byte[] beginDelimiter = this.beginDelimiters[index];
                        int bytesRequested = beginDelimiter.Length - this.dataBuffer.Count;
                        if (bytesRequested > bytesAvailable)
                        {
                            // Not enough data has arrived for a complete match
                            waiting = true;
                            continue;
                        }

                        // Test if this begin delimiter already has been rejected
                        if (bytesRequested <= 0)
                        {
                            continue;
                        }

                        // Test if we have a complete match
                        IList<byte> lastOfBeginDelimiter = data.Slice(i, bytesRequested);
                        if (beginDelimiter.SequenceEqual(this.dataBuffer.Concat(lastOfBeginDelimiter)))
                        {
                            this.AppendDataToDataBuffer(lastOfBeginDelimiter);
                            i += bytesRequested;
                            this.delimiterIndex = index;
                            break;
                        }
                    }

                    if (this.delimiterIndex == -1)
                    {
                        if (waiting)
                        {
                            // The data is not long enough to match at least one of the begin delimiters, and it doesn't match any that it is long enough to match.
                            this.AppendDataToDataBuffer(data.Slice(i, bytesAvailable));
                            return;
                        }
                        else
                        {
                            trace.TraceEvent(TraceEventType.Error, 0, "Message does not begin with a defined start delimieter in data " + data.Slice(i, bytesAvailable).PrettyDump());
                            throw new System.Net.ProtocolViolationException("Message does not begin with a start delimiter");
                        }
                    }
                }
                else
                {
                    // The begin delimiter has been found; we are reading data while searching for the end delimiter
                    byte[] endDelimiter = this.endDelimiters[this.delimiterIndex];
                    if (this.endDelimiterIndex != 0)
                    {
                        // A partial end delimiter was found at the end of a previous chunk of incoming data; see if this data finishes the end delimiter
#if DEBUG
                        Debug.Assert(i == 0, "This branch should only be taken when a new chunk of incoming data has arrived.");
#endif
                        int bytesToMatch = Math.Min(endDelimiter.Length - this.endDelimiterIndex, bytesAvailable);
                        if (endDelimiter.Slice(this.endDelimiterIndex, bytesToMatch).SequenceEqual(data.Slice(0, bytesToMatch)))
                        {
                            // A partial or complete match was found
                            if (bytesToMatch == endDelimiter.Length - this.endDelimiterIndex)
                            {
                                // A complete match has been found: report the message and continue processing
                                if (this.MessageArrived != null)
                                {
                                    this.MessageArrived(this.delimiterIndex, this.dataBuffer.Skip(this.beginDelimiters[this.delimiterIndex].Length));
                                }

                                i += bytesToMatch;
                                this.Reset();
                            }
                            else
                            {
                                // The partial match has been extended into a longer partial match (at the end of the data)
                                this.endDelimiterIndex += bytesAvailable;
                                return;
                            }
                        }
                        else
                        {
                            // This was a false partial end delimiter; prepend it to the incoming data and continue searching
                            List<byte> newData = new List<byte>(data.Count + this.endDelimiterIndex);
                            newData.AddRange(endDelimiter.Take(this.endDelimiterIndex));
                            newData.AddRange(data);
                            data = newData;

                            this.endDelimiterIndex = 0;
                        }
                    }
                    else
                    {
                        // The data buffer does not have an implicit partial end delimiter on it, so we can just search the incoming data
                        // Search for a complete end delimiter, or a partial end delimiter at the end of the incoming data
                        int endDelimiterFoundIndex;
                        for (endDelimiterFoundIndex = 0; endDelimiterFoundIndex != bytesAvailable; ++endDelimiterFoundIndex)
                        {
                            int bytesToMatch = Math.Min(endDelimiter.Length, bytesAvailable - endDelimiterFoundIndex);
                            if (endDelimiter.Slice(0, bytesToMatch).SequenceEqual(data.Slice(i + endDelimiterFoundIndex, bytesToMatch)))
                            {
                                // A partial or complete match was found
                                IList<byte> match = data.Slice(i, endDelimiterFoundIndex);
                                if (bytesToMatch == endDelimiter.Length)
                                {
                                    // A complete match was found: report the message and continue processing
                                    this.CheckMaxMessageSize(this.dataBuffer.Count, endDelimiterFoundIndex);
                                    IList<byte> message = ListExtensions.Concat(this.dataBuffer, match);

                                    if (this.MessageArrived != null)
                                    {
                                        this.MessageArrived(this.delimiterIndex, message.Skip(this.beginDelimiters[this.delimiterIndex].Length));
                                    }

                                    i += endDelimiterFoundIndex + bytesToMatch;
                                    this.dataBuffer.Clear();
                                    this.delimiterIndex = -1;
#if DEBUG
                                    Debug.Assert(this.endDelimiterIndex == 0, "endDelimiterIndex should already be 0");
#endif
                                    break;
                                }
                                else
                                {
                                    // A partial match was found at the end of the available data
                                    this.AppendDataToDataBuffer(match);
                                    this.endDelimiterIndex = bytesToMatch;
                                    return;
                                }
                            }
                        }

                        if (endDelimiterFoundIndex == bytesAvailable)
                        {
                            // No end delimiter match was found (not even a partial at the end of the buffer)
                            this.AppendDataToDataBuffer(data.Slice(i, bytesAvailable));
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resizes the data buffer and appends the new range of data to it.
        /// </summary>
        /// <param name="data">The buffer containing the new data to be appended.</param>
        private void AppendDataToDataBuffer(IList<byte> data)
        {
            this.CheckMaxMessageSize(this.dataBuffer.Count, data.Count);

            int newCapacity = this.dataBuffer.Count + data.Count;
            if (this.dataBuffer.Capacity < newCapacity)
            {
                this.dataBuffer.Capacity = newCapacity;
            }

            this.dataBuffer.AddRange(data);
        }

        /// <summary>
        /// Checks for messages that exceed the maximum message size in an overflow-safe way.
        /// </summary>
        /// <param name="currentCount">The current size of the message so far.</param>
        /// <param name="additionalCount">The additional bytes that have arrived as part of this message.</param>
        private void CheckMaxMessageSize(int currentCount, int additionalCount)
        {
            if (this.maxMessageSize != 0 && additionalCount > this.maxMessageSize - currentCount)
            {
                trace.TraceEvent(TraceEventType.Error, 0, "Message size " + (currentCount + additionalCount) + " exceeds maximum data size " + this.maxMessageSize);
                throw new System.Net.ProtocolViolationException("Maximum message size exceeded");
            }
        }
    }
}
