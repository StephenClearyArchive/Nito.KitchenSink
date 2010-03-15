// <copyright file="MultipleByteDelimiterFramer.cs" company="Nito Programs">
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
    /// Provides message framing for multiple-byte-delimiter-based protocols. Does not perform any unescaping of the message data.
    /// </summary>
    public sealed class MultipleByteDelimiterFramer : IFramer
    {
        /// <summary>
        /// The maximum size of messages allowed, or 0 if there is no maximum.
        /// </summary>
        private readonly int maxMessageSize;

        /// <summary>
        /// The beginning delimiter.
        /// </summary>
        private readonly byte[] beginDelimiter;

        /// <summary>
        /// The ending delimiter.
        /// </summary>
        private readonly byte[] endDelimiter;

        /// <summary>
        /// The trace source used for all communications messages.
        /// </summary>
        private static TraceSource trace = new TraceSource("Sockets", SourceLevels.All);

        /// <summary>
        /// The data buffer, which grows dynamically as more data arrives. This is never null.
        /// </summary>
        private List<byte> dataBuffer;

        /// <summary>
        /// How much of the begin delimiter has been seen (implicitly at the beginning of the data buffer).
        /// </summary>
        private int beginDelimiterIndex;

        /// <summary>
        /// How much of the end delimiter has been seen (implicitly at the end of the data buffer).
        /// </summary>
        private int endDelimiterIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleByteDelimiterFramer"/> class.
        /// </summary>
        /// <param name="maxMessageSize">Maximum size of messages, or 0 if message size is not restricted.</param>
        /// <param name="beginDelimiter">The begin delimiter.</param>
        /// <param name="endDelimiter">The end delimiter.</param>
        public MultipleByteDelimiterFramer(int maxMessageSize, byte[] beginDelimiter, byte[] endDelimiter)
        {
            this.maxMessageSize = maxMessageSize;
            this.beginDelimiter = beginDelimiter;
            this.endDelimiter = endDelimiter;
            this.dataBuffer = new List<byte>();
        }

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
            this.beginDelimiterIndex = 0;
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
                if (this.beginDelimiterIndex != this.beginDelimiter.Length)
                {
                    // We are searching for the begin delimiter
                    int bytesRequested = this.beginDelimiter.Length - this.beginDelimiterIndex;
                    int bytesUsed = Math.Min(bytesRequested, bytesAvailable);

                    // Test if we have a (continuing) match
                    if (this.beginDelimiter.Slice(this.beginDelimiterIndex, bytesUsed).SequenceEqual(data.Slice(i, bytesUsed)))
                    {
                        this.beginDelimiterIndex += bytesUsed;
                        i += bytesUsed;
                    }
                    else
                    {
                        trace.TraceEvent(TraceEventType.Error, 0, "Message does not begin with start delimieter " + this.beginDelimiter.PrettyDump() + " in data " + data.Slice(i, bytesAvailable).PrettyDump());
                        throw new System.Net.ProtocolViolationException("Message does not begin with a start delimiter");
                    }
                }
                else
                {
                    // The begin delimiter has been found; we are reading data while searching for the end delimiter
                    if (this.endDelimiterIndex != 0)
                    {
                        // A partial end delimiter was found at the end of a previous chunk of incoming data; see if this data finishes the end delimiter
#if DEBUG
                        Debug.Assert(i == 0, "This branch should only be taken when a new chunk of incoming data has arrived.");
#endif
                        int bytesToMatch = Math.Min(this.endDelimiter.Length - this.endDelimiterIndex, bytesAvailable);
                        if (this.endDelimiter.Slice(this.endDelimiterIndex, bytesToMatch).SequenceEqual(data.Slice(0, bytesToMatch)))
                        {
                            // A partial or complete match was found
                            if (bytesToMatch == this.endDelimiter.Length - this.endDelimiterIndex)
                            {
                                // A complete match has been found: report the message and continue processing
                                if (this.MessageArrived != null)
                                {
                                    this.MessageArrived(this.dataBuffer);
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
                            newData.AddRange(this.endDelimiter.Take(this.endDelimiterIndex));
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
                            int bytesToMatch = Math.Min(this.endDelimiter.Length, bytesAvailable - endDelimiterFoundIndex);
                            if (this.endDelimiter.Slice(0, bytesToMatch).SequenceEqual(data.Slice(i + endDelimiterFoundIndex, bytesToMatch)))
                            {
                                // A partial or complete match was found
                                if (bytesToMatch == this.endDelimiter.Length)
                                {
                                    // A complete match was found: report the message and continue processing
                                    this.CheckMaxMessageSize(this.dataBuffer.Count, endDelimiterFoundIndex);
                                    IList<byte> message;
                                    if (this.dataBuffer.Count == 0)
                                    {
                                        message = data.Slice(i, endDelimiterFoundIndex);
                                    }
                                    else
                                    {
                                        message = ListExtensions.Concat(this.dataBuffer, data.Slice(i, endDelimiterFoundIndex));
                                    }

                                    if (this.MessageArrived != null)
                                    {
                                        this.MessageArrived(message);
                                    }

                                    i += endDelimiterFoundIndex + bytesToMatch;
                                    this.dataBuffer.Clear();
                                    this.beginDelimiterIndex = 0;
#if DEBUG
                                    Debug.Assert(this.endDelimiterIndex == 0, "endDelimiterIndex should already be 0");
#endif
                                    break;
                                }
                                else
                                {
                                    // A partial match was found at the end of the available data
                                    this.AppendDataToDataBuffer(data, i, endDelimiterFoundIndex);
                                    this.endDelimiterIndex = bytesToMatch;
                                    return;
                                }
                            }
                        }

                        if (endDelimiterFoundIndex == bytesAvailable)
                        {
                            // No end delimiter match was found (not even a partial at the end of the buffer)
                            this.AppendDataToDataBuffer(data, i, bytesAvailable);
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
        /// <param name="index">The index into <paramref name="data"/> marking the start of the new data.</param>
        /// <param name="count">The number of bytes to append from <paramref name="data"/> into the data buffer.</param>
        private void AppendDataToDataBuffer(IList<byte> data, int index, int count)
        {
            this.CheckMaxMessageSize(this.dataBuffer.Count, count);

            int newCapacity = this.dataBuffer.Count + count;
            if (this.dataBuffer.Capacity < newCapacity)
            {
                this.dataBuffer.Capacity = newCapacity;
            }

            this.dataBuffer.AddRange(data.Slice(index, count));
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
