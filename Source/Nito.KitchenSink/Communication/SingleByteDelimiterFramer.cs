// <copyright file="SingleByteDelimiterFramer.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.Communication
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Nito.KitchenSink;
    using Nito.Linq;

    /// <summary>
    /// Provides message framing for single-byte-delimiter-based protocols. Does not perform any unescaping of the message data.
    /// </summary>
    public sealed class SingleByteDelimiterFramer : IFramer
    {
        /// <summary>
        /// The maximum size of messages allowed, or 0 if there is no maximum.
        /// </summary>
        private readonly int maxMessageSize;

        /// <summary>
        /// The beginning delimiter.
        /// </summary>
        private readonly byte beginDelimiter;

        /// <summary>
        /// The ending delimiter.
        /// </summary>
        private readonly byte endDelimiter;

        /// <summary>
        /// The trace source used for all communications messages.
        /// </summary>
        private static TraceSource trace = new TraceSource("Sockets", SourceLevels.All);

        /// <summary>
        /// The data buffer, which grows dynamically as more data arrives. This is never null.
        /// </summary>
        private List<byte> dataBuffer;

        /// <summary>
        /// Whether the begin delimiter has been seen.
        /// </summary>
        private bool sawBeginDelimiter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleByteDelimiterFramer"/> class.
        /// </summary>
        /// <param name="maxMessageSize">Maximum size of messages, or 0 if message size is not restricted.</param>
        /// <param name="beginDelimiter">The begin delimiter.</param>
        /// <param name="endDelimiter">The end delimiter.</param>
        public SingleByteDelimiterFramer(int maxMessageSize, byte beginDelimiter, byte endDelimiter)
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
            this.sawBeginDelimiter = false;
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
                if (!this.sawBeginDelimiter)
                {
                    // Search for a begin delimiter
                    if (data[i] != this.beginDelimiter)
                    {
                        trace.TraceEvent(TraceEventType.Error, 0, "Message does not begin with start delimieter 0x" + this.beginDelimiter.ToString("X2", NumberFormatInfo.InvariantInfo) + " in data " + data.Slice(i, bytesAvailable).PrettyDump());
                        throw new System.Net.ProtocolViolationException("Message does not begin with a start delimiter");
                    }

                    // The begin delimiter was found, so continue processing
                    this.sawBeginDelimiter = true;
                    ++i;
                }
                else
                {
                    IList<byte> availableData = data.Slice(i, bytesAvailable);

                    // Search for an end delimiter
                    int endIndex = availableData.IndexOf(this.endDelimiter);
                    if (endIndex == -1)
                    {
                        // The end delimiter wasn't found, so place the data in the data buffer and return
                        this.AppendDataToDataBuffer(availableData);
                        return;
                    }
                    else
                    {
                        // The end delimiter was found

                        // Concatenate our existing data buffer with the rest of this message
                        this.CheckMaxMessageSize(this.dataBuffer.Count, endIndex);
                        IList<byte> message;
                        if (this.dataBuffer.Count == 0)
                        {
                            message = data.Slice(i, endIndex);
                        }
                        else
                        {                        
                            message = ListExtensions.Concat(this.dataBuffer, data.Slice(i, endIndex));
                        }

                        // Invoke the callback
                        if (this.MessageArrived != null)
                        {
                            this.MessageArrived(message);
                        }

                        // Clear the data buffer and continue processing
                        this.sawBeginDelimiter = false;
                        this.dataBuffer.Clear();
                        i += endIndex + 1;
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
