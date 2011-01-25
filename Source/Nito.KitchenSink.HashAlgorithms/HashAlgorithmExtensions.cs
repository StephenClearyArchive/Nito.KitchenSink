// <copyright file="HashAlgorithmExtensions.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.HashAlgorithms
{
    using System.Diagnostics.Contracts;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides OpenSSL-like extension methods for <see cref="HashAlgorithm"/> and derived classes.
    /// </summary>
    public static class HashAlgorithmExtensions
    {
        /// <summary>
        /// Updates the hash value by hashing the provided byte buffer. <see cref="HashAlgorithm.Initialize"/> should be called before invoking this method for the first time.
        /// </summary>
        /// <param name="hash">The hash algorithm, including its state. May not be <c>null</c>.</param>
        /// <param name="buffer">The input byte array to include in the hash calculation. May not be <c>null</c>.</param>
        /// <param name="offset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="count">The number of bytes in the input byte array to use as data.</param>
        public static void Update(this HashAlgorithm hash, byte[] buffer, int offset, int count)
        {
            Contract.Requires(hash != null);
            Contract.Requires(buffer != null);
            Contract.Requires(offset >= 0);
            Contract.Requires(count >= 0);
            Contract.Requires(count <= buffer.Length);
            Contract.Requires(buffer.Length - count >= offset);

            hash.TransformBlock(buffer, offset, count, null, 0);
        }

        /// <summary>
        /// Updates the hash value by hashing the provided byte buffer. <see cref="HashAlgorithm.Initialize"/> should be called before invoking this method for the first time.
        /// </summary>
        /// <param name="hash">The hash algorithm, including its state. May not be <c>null</c>.</param>
        /// <param name="buffer">The input byte array to include in the hash calculation.</param>
        public static void Update(this HashAlgorithm hash, byte[] buffer)
        {
            Contract.Requires(hash != null);
            Contract.Requires(buffer != null);

            hash.TransformBlock(buffer, 0, buffer.Length, null, 0);
        }

        /// <summary>
        /// Finishes the hash calculation and returns the calculated hash value (<see cref="HashAlgorithm.Hash"/>). This method should only be called once.
        /// </summary>
        /// <param name="hash">The hash algorithm, including its state. May not be <c>null</c>.</param>
        /// <returns>The calcualted hash value.</returns>
        public static byte[] Final(this HashAlgorithm hash)
        {
            Contract.Requires(hash != null);
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == ((hash.HashSize + 7) & -8) / 8);

            hash.TransformFinalBlock(new byte[0], 0, 0);
            var ret = hash.Hash;
            Contract.Assume(ret != null);
            Contract.Assume(ret.Length == ((hash.HashSize + 7) & -8) / 8);
            return ret;
        }
    }
}
