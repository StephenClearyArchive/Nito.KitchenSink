using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Nito.KitchenSink
{
    [CLSCompliant(false)]
    public class InternetCallbackEventArgs
    {
        public StatusCode Code { get; set; }

        public byte[] RawData { get; set; }

        public sealed class Socket : InternetCallbackEventArgs
        {
            public EndPoint EndPoint { get; set; }
        }

        public sealed class CookieHistory : InternetCallbackEventArgs
        {
            public bool Accepted { get; set; }

            public bool Leashed { get; set; }

            public bool Downgraded { get; set; }

            public bool Rejected { get; set; }
        }

        public sealed class Number : InternetCallbackEventArgs
        {
            public uint Value { get; set; }
        }

        public sealed class AsyncResult : InternetCallbackEventArgs
        {
            public IntPtr Result { get; set; }

            public uint Error { get; set; }
        }

        public sealed class String : InternetCallbackEventArgs
        {
            public string Value { get; set; }
        }

        public enum StatusCode : uint
        {
            ResolvingName = 10,
            NameResolved = 11,
            ConnectingToServer = 20,
            ConnectedToServer = 21,
            SendingRequest = 30,
            RequestSent = 31,
            ReceivingResponse = 40,
            ResponseReceived = 41,
            ClosingConnection = 50,
            ConnectionClosed = 51,
            HandleCreated = 60,
            HandleClosing = 70,
            DetectingProxy = 80,
            RequestComplete = 100,
            Redirect = 110,
            IntermediateResponse = 120,
            StateChange = 200,
            CookieSent = 320,
            CookieReceived = 321,
            P3PHeader = 325,
            CookieHistory = 327,
        }
    }
}
