using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    public static class EncodingObservables
    {
        /// <summary>
        /// Takes a "chunked" sequence of characters and converts it to a "chunked" sequence of bytes using the specified encoding.
        /// </summary>
        /// <param name="source">The "chunked" sequence of characters.</param>
        /// <param name="encoding">The encoding used to translate the sequence of characters to a sequence of bytes.</param>
        /// <returns>The "chunked" sequence of bytes.</returns>
        public static IObservable<byte[]> Encode(this IObservable<char[]> source, Encoding encoding)
        {
            return Observable.CreateWithDisposable<byte[]>(observer =>
            {
                var encoder = encoding.GetEncoder();

                return source.Subscribe(
                    data =>
                    {
                        try
                        {
                            var ret = new byte[encoder.GetByteCount(data, 0, data.Length, false)];
                            encoder.GetBytes(data, 0, data.Length, ret, 0, false);
                            if (ret.Length != 0)
                            {
                                observer.OnNext(ret);
                            }
                        }
                        catch (EncoderFallbackException ex)
                        {
                            observer.OnError(ex);
                        }
                    },
                    observer.OnError,
                    () =>
                    {
                        try
                        {
                            var ret = new byte[encoder.GetByteCount(new char[0], 0, 0, true)];
                            encoder.GetBytes(new char[0], 0, 0, ret, 0, true);
                            if (ret.Length != 0)
                            {
                                observer.OnNext(ret);
                            }

                            observer.OnCompleted();
                        }
                        catch (EncoderFallbackException ex)
                        {
                            observer.OnError(ex);
                        }
                    });
            });
        }

        /// <summary>
        /// Takes a "chunked" sequence of bytes and converts it to a "chunked" sequence of characters using the specified encoding.
        /// </summary>
        /// <param name="source">The "chunked" sequence of bytes.</param>
        /// <param name="encoding">The encoding used to translate the sequence of bytes to a sequence of characters.</param>
        /// <returns>The "chunked" sequence of characters.</returns>
        public static IObservable<char[]> Decode(this IObservable<byte[]> source, Encoding encoding)
        {
            return Observable.CreateWithDisposable<char[]>(observer =>
            {
                var decoder = encoding.GetDecoder();

                return source.Subscribe(
                    data =>
                    {
                        try
                        {
                            var ret = new char[decoder.GetCharCount(data, 0, data.Length, false)];
                            decoder.GetChars(data, 0, data.Length, ret, 0, false);
                            if (ret.Length != 0)
                            {
                                observer.OnNext(ret);
                            }
                        }
                        catch (EncoderFallbackException ex)
                        {
                            observer.OnError(ex);
                        }
                    },
                    observer.OnError,
                    () =>
                    {
                        try
                        {
                            var ret = new char[decoder.GetCharCount(new byte[0], 0, 0, true)];
                            decoder.GetChars(new byte[0], 0, 0, ret, 0, true);
                            if (ret.Length != 0)
                            {
                                observer.OnNext(ret);
                            }

                            observer.OnCompleted();
                        }
                        catch (EncoderFallbackException ex)
                        {
                            observer.OnError(ex);
                        }
                    });
            });
        }
    }
}
