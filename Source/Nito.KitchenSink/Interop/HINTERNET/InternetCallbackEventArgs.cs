// <copyright file="InternetCallbackEventArgs.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.WinInet
{
    using System;
    using System.Net;

    /// <summary>
    /// Arguments for <see cref="InternetHandle.InternetCallback"/> delegates.
    /// </summary>
    public class InternetCallbackEventArgs
    {
        /// <summary>
        /// The defined notification types. For future compatibility, notifications with unknown types should be ignored.
        /// </summary>
        public enum StatusCode : int
        {
            /// <summary>
            /// Looking up the IP address of the name. This is a <see cref="String"/> argument type, containing the name of the server.
            /// </summary>
            ResolvingName = 10,

            /// <summary>
            /// Successfully found the IP address of the name. This is a <see cref="String"/> argument type, containing the name of the server.
            /// </summary>
            NameResolved = 11,

            /// <summary>
            /// Connecting to the server endpoint. This is a <see cref="Socket"/> argument type, containing the endpoint of the server.
            /// </summary>
            ConnectingToServer = 20,

            /// <summary>
            /// Successfully connected to the server endpoint. This is a <see cref="Socket"/> argument type, containing the endpoint of the server.
            /// </summary>
            ConnectedToServer = 21,

            /// <summary>
            /// Sending the information request to the server.
            /// </summary>
            SendingRequest = 30,

            /// <summary>
            /// Successfully sent the information request to the server. This is a <see cref="Number"/> argument type, containing the number of bytes sent.
            /// </summary>
            RequestSent = 31,

            /// <summary>
            /// Waiting for the server to respond to a request.
            /// </summary>
            ReceivingResponse = 40,

            /// <summary>
            /// Successfully received a response from the server. This is a <see cref="Number"/> argument type, containing the number of bytes received.
            /// </summary>
            ResponseReceived = 41,

            /// <summary>
            /// Closing the connection to the server.
            /// </summary>
            ClosingConnection = 50,

            /// <summary>
            /// Successfully closed the conection to the server.
            /// </summary>
            ConnectionClosed = 51,

            /// <summary>
            /// The <c>InternetConnect</c> function has completed creating a new handle. This is an <see cref="AsyncResult"/> argument type, containing the result of the operation.
            /// </summary>
            HandleCreated = 60,

            /// <summary>
            /// The handle value has been terminated.
            /// </summary>
            HandleClosing = 70,

            /// <summary>
            /// A proxy has been detected.
            /// </summary>
            DetectingProxy = 80,

            /// <summary>
            /// An asynchronous operation has completed. This is an <see cref="AsyncResult"/> argument type, containing the result of the operation.
            /// </summary>
            RequestComplete = 100,

            /// <summary>
            /// An HTTP request is about to automatically redirect the request. This is a <see cref="String"/> argument type, containing the new URL.
            /// </summary>
            Redirect = 110,

            /// <summary>
            /// Received an intermediate (100 level) status code message from the server.
            /// </summary>
            IntermediateResponse = 120,

            /// <summary>
            /// Moved between a secure (HTTPS) and nonsecure (HTTP) site. This is a <see cref="Number"/> argument type, containing additional flags.
            /// </summary>
            StateChange = 200,

            /// <summary>
            /// Indicates the number of cookies sent or suppressed when a request is sent. This is a <see cref="Number"/> argument type, containing the number of cookies sent or suppressed.
            /// </summary>
            CookieSent = 320,

            /// <summary>
            /// Indicates the number of cookies accepted, rejected, downgraded, or leashed. This is a <see cref="Number"/> argument type, containing the number of cookies received.
            /// </summary>
            CookieReceived = 321,

            /// <summary>
            /// The response has a P3P header in it.
            /// </summary>
            P3PHeader = 325,

            /// <summary>
            /// Retrieving content from the cache. This is a <see cref="CookieHistory"/> argument type, containing information about past cookie events for this URL.
            /// </summary>
            CookieHistory = 327,
        }

        /// <summary>
        /// Gets or sets the type of notification that this callback represents.
        /// </summary>
        public StatusCode Code { get; set; }

        /// <summary>
        /// Gets or sets the raw data of the notification.
        /// </summary>
        public byte[] RawData { get; set; }

        /// <summary>
        /// A notification including socket endpoint information.
        /// </summary>
        public sealed class Socket : InternetCallbackEventArgs
        {
            /// <summary>
            /// Gets or sets the socket endpoint information.
            /// </summary>
            public EndPoint EndPoint { get; set; }
        }

        /// <summary>
        /// A notification including cookie history information.
        /// </summary>
        public sealed class CookieHistory : InternetCallbackEventArgs
        {
            /// <summary>
            /// Gets or sets a value indicating whether cookies were accepted.
            /// </summary>
            /// <value></value>
            public bool Accepted { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether cookies were leashed.
            /// </summary>
            /// <value></value>
            public bool Leashed { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether cookies were downgraded.
            /// </summary>
            /// <value></value>
            public bool Downgraded { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether cookies were rejected.
            /// </summary>
            /// <value></value>
            public bool Rejected { get; set; }
        }

        /// <summary>
        /// A notification including numerical information.
        /// </summary>
        public sealed class Number : InternetCallbackEventArgs
        {
            /// <summary>
            /// Gets or sets the numerical information; this is actually a UInt32.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// A notification including the result of an asynchronous operation.
        /// </summary>
        public sealed class AsyncResult : InternetCallbackEventArgs
        {
            /// <summary>
            /// Gets or sets the result of the asynchronous operation.
            /// </summary>
            public IntPtr Result { get; set; }

            /// <summary>
            /// Gets or sets the error code; this is actually a UInt32.
            /// </summary>
            public int Error { get; set; }
        }

        /// <summary>
        /// A notification including string information.
        /// </summary>
        public sealed class String : InternetCallbackEventArgs
        {
            /// <summary>
            /// Gets or sets the string information.
            /// </summary>
            public string Value { get; set; }
        }
    }
}
