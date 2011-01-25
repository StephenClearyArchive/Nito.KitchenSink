// <copyright file="GuidExtensions.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.GuidDecoding
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Known values for the <see cref="Guid"/> Variant field.
    /// </summary>
    public enum GuidVariant : int
    {
        /// <summary>
        /// Reserved for NCS backward compatibility.
        /// </summary>
        NCSBackwardCompatibility = 0,

        /// <summary>
        /// A GUID conforming to RFC 4122.
        /// </summary>
        RFC4122 = 4,

        /// <summary>
        /// Reserved for Microsoft backward compatibility.
        /// </summary>
        MicrosoftBackwardCompatibility = 6,

        /// <summary>
        /// Reserved for future definition.
        /// </summary>
        ReservedForFutureDefinition = 7,
    }

    /// <summary>
    /// Known values for the <see cref="Guid"/> Version field.
    /// </summary>
    public enum GuidVersion : int
    {
        /// <summary>
        /// Time-based (sequential) GUID.
        /// </summary>
        TimeBased = 1,

        /// <summary>
        /// DCE Security GUID with embedded POSIX UID/GID. See "DCE 1.1: Authentication and Security Services", Chapter 5 and "DCE 1.1: RPC", Appendix A.
        /// </summary>
        DCESecurity = 2,

        /// <summary>
        /// Name-based GUID using the MD5 hashing algorithm.
        /// </summary>
        NameBasedUsingMD5 = 3,

        /// <summary>
        /// Random GUID.
        /// </summary>
        Random = 4,

        /// <summary>
        /// Name-based GUID using the SHA-1 hashing algorithm.
        /// </summary>
        NameBasedUsingSHA1 = 5,
    }

    /// <summary>
    /// Known values of DCE domains.
    /// </summary>
    public enum DCEDomain
    {
        /// <summary>
        /// The principal domain. On POSIX machines, this is the POSIX UID domain.
        /// </summary>
        Person = 0,

        /// <summary>
        /// The group domain. On POSIX machines, this is the POSIX GID domain.
        /// </summary>
        Group = 1,

        /// <summary>
        /// The organization domain.
        /// </summary>
        Organization = 2,
    }

    /// <summary>
    /// Extension methods for the <see cref="Guid"/> structure.
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>
        /// Gets the 3-bit Variant field of the GUID.
        /// </summary>
        /// <param name="guid">The GUID from which to extract the field.</param>
        /// <returns>The Variant field of the GUID.</returns>
        [Pure]
        public static GuidVariant GetVariant(this Guid guid)
        {
            byte value = guid.ToByteArray()[8];
            if ((value & 0x80) == 0)
            {
                return GuidVariant.NCSBackwardCompatibility;
            }
            else if ((value & 0x40) == 0)
            {
                return GuidVariant.RFC4122;
            }
            else
            {
                return (GuidVariant)(value >> 5);
            }
        }

        /// <summary>
        /// Gets the 4-bit Version field of the GUID. This is only valid if <see cref="GetVariant"/> returns <see cref="GuidVariant.RFC4122"/>.
        /// </summary>
        /// <param name="guid">The GUID from which to extract the field.</param>
        /// <returns>The Version field of the GUID.</returns>
        [Pure]
        public static GuidVersion GetVersion(this Guid guid)
        {
            Contract.Requires(guid.GetVariant() == GuidVariant.RFC4122);
            return (GuidVersion)(guid.ToByteArray()[7] >> 4);
        }

        /// <summary>
        /// Gets the Domain field of the security GUID. This is only valid if <see cref="GetVersion"/> returns <see cref="GuidVersion.DCESecurity"/>
        /// </summary>
        /// <param name="guid">The security GUID from which to extract the field.</param>
        /// <returns>The Domain field of the security GUID.</returns>
        public static DCEDomain GetDomain(this Guid guid)
        {
            Contract.Requires(guid.GetVersion() == GuidVersion.DCESecurity);
            return (DCEDomain)(guid.ToByteArray()[9]);
        }

        /// <summary>
        /// Gets the Local Identifier field of the security GUID. The return value should be interpreted as an unsigned integer. This is only valid if <see cref="GetVersion"/> returns <see cref="GuidVersion.DCESecurity"/>
        /// </summary>
        /// <param name="guid">The security GUID from which to extract the field.</param>
        /// <returns>The Domain field of the security GUID.</returns>
        public static int GetLocalIdentifier(this Guid guid)
        {
            Contract.Requires(guid.GetVersion() == GuidVersion.DCESecurity);
            byte[] value = guid.ToByteArray();
            uint ret = value[3];
            ret <<= 8;
            ret |= value[2];
            ret <<= 8;
            ret |= value[1];
            ret <<= 8;
            ret |= value[0];
            return (int)ret;
        }

        /// <summary>
        /// Gets the 60-bit Timestamp field of the GUID. This is only valid if <see cref="GetVersion"/> returns <see cref="GuidVersion.TimeBased"/>.
        /// </summary>
        /// <param name="guid">The GUID from which to extract the field.</param>
        /// <returns>The Timestamp field of the GUID.</returns>
        public static long GetTimestamp(this Guid guid)
        {
            Contract.Requires(guid.GetVersion() == GuidVersion.TimeBased);
            byte[] value = guid.ToByteArray();
            long ret = value[7] & 0x0F;
            ret <<= 8;
            ret |= value[6];
            ret <<= 8;
            ret |= value[5];
            ret <<= 8;
            ret |= value[4];
            ret <<= 8;
            ret |= value[3];
            ret <<= 8;
            ret |= value[2];
            ret <<= 8;
            ret |= value[1];
            ret <<= 8;
            ret |= value[0];
            return ret;
        }

        /// <summary>
        /// Gets the 28-bit Timestamp field of the GUID; the lowest 32 bits of the returned value are always zero. This is only valid if <see cref="GetVersion"/> returns <see cref="GuidVersion.DCESecurity"/>.
        /// </summary>
        /// <param name="guid">The GUID from which to extract the field.</param>
        /// <returns>The Timestamp field of the GUID.</returns>
        public static long GetPartialTimestamp(this Guid guid)
        {
            Contract.Requires(guid.GetVersion() == GuidVersion.DCESecurity);
            byte[] value = guid.ToByteArray();
            long ret = value[7] & 0x0F;
            ret <<= 8;
            ret |= value[6];
            ret <<= 8;
            ret |= value[5];
            ret <<= 8;
            ret |= value[4];
            ret <<= 32;
            return ret;
        }

        /// <summary>
        /// Gets the date and time that this GUID was created, in UTC. This is only valid if <see cref="GetVersion"/> returns <see cref="GuidVersion.TimeBased"/>.
        /// </summary>
        /// <param name="guid">The GUID from which to extract the field.</param>
        /// <returns>The date and time that this GUID was created, in UTC.</returns>
        public static DateTime GetCreateTime(this Guid guid)
        {
            Contract.Requires(guid.GetVersion() == GuidVersion.TimeBased);
            return new DateTime(1582, 10, 15, 0, 0, 0, DateTimeKind.Utc).AddTicks(guid.GetTimestamp());
        }

        /// <summary>
        /// Gets the approximate date and time that this GUID was created, in UTC. This is only valid if <see cref="GetVersion"/> returns <see cref="GuidVersion.DCESecurity"/>.
        /// </summary>
        /// <param name="guid">The GUID from which to extract the field.</param>
        /// <returns>The date and time that this GUID was created, in UTC.</returns>
        public static DateTime GetPartialCreateTime(this Guid guid)
        {
            Contract.Requires(guid.GetVersion() == GuidVersion.DCESecurity);
            return new DateTime(1582, 10, 15, 0, 0, 0, DateTimeKind.Utc).AddTicks(guid.GetPartialTimestamp());
        }

        /// <summary>
        /// Gets the 14-bit Clock Sequence field of the GUID. This is only valid if <see cref="GetVersion"/> returns <see cref="GuidVersion.TimeBased"/>.
        /// </summary>
        /// <param name="guid">The GUID from which to extract the field.</param>
        /// <returns>The Clock Sequence field of the GUID.</returns>
        public static int GetClockSequence(this Guid guid)
        {
            Contract.Requires(guid.GetVersion() == GuidVersion.TimeBased);
            byte[] value = guid.ToByteArray();
            int ret = value[8] & 0x3F;
            ret <<= 8;
            ret |= value[9];
            return ret;
        }

        /// <summary>
        /// Gets the 6-bit Clock Sequence field of the GUID. The lowest 8 bits of the returned value are always 0. This is only valid if <see cref="GetVersion"/> returns <see cref="GuidVersion.DCESecurity"/>.
        /// </summary>
        /// <param name="guid">The GUID from which to extract the field.</param>
        /// <returns>The Clock Sequence field of the GUID.</returns>
        public static int GetPartialClockSequence(this Guid guid)
        {
            Contract.Requires(guid.GetVersion() == GuidVersion.DCESecurity);
            byte[] value = guid.ToByteArray();
            int ret = value[8] & 0x3F;
            ret <<= 8;
            return ret;
        }

        /// <summary>
        /// Gets the 6-byte (48-bit) Node field of the GUID. This is only valid if <see cref="GetVersion"/> returns <see cref="GuidVersion.TimeBased"/> or <see cref="GuidVersion.DCESecurity"/>.
        /// </summary>
        /// <param name="guid">The GUID from which to extract the field.</param>
        /// <returns>The Node field of the GUID.</returns>
        public static IList<byte> GetNode(this Guid guid)
        {
            Contract.Requires(guid.GetVersion() == GuidVersion.TimeBased || guid.GetVersion() == GuidVersion.DCESecurity);
            var ret = new byte[6];
            Array.Copy(guid.ToByteArray(), 10, ret, 0, 6);
            return ret;
        }

        /// <summary>
        /// Returns <c>true</c> if the Node field is a MAC address; returns <c>false</c> if the Node field is a random value. This is only valid if <see cref="GetVersion"/> returns <see cref="GuidVersion.TimeBased"/> or <see cref="GuidVersion.DCESecurity"/>.
        /// </summary>
        /// <param name="guid">The GUID to inspect.</param>
        /// <returns>Returns <c>true</c> if the Node field is a MAC address; returns <c>false</c> if the Node field is a random value.</returns>
        public static bool NodeIsMAC(this Guid guid)
        {
            Contract.Requires(guid.GetVersion() == GuidVersion.TimeBased || guid.GetVersion() == GuidVersion.DCESecurity);
            return (guid.ToByteArray()[10] & 0x80) == 0;
        }

        /// <summary>
        /// Gets what remains of the 128-bit MD5 or SHA-1 hash of the name used to create this GUID. This is only valid if <see cref="GetVersion"/> returns <see cref="GuidVersion.NameBasedUsingMD5"/> or <see cref="GuidVersion.NameBasedUsingSHA1"/>. Note that bits 60-63 and bits 70-71 will always be zero (their original values are permanently lost).
        /// </summary>
        /// <param name="guid">The GUID from which to extract the hash value.</param>
        /// <returns>The hash value from the GUID.</returns>
        public static IList<byte> GetHash(this Guid guid)
        {
            Contract.Requires(guid.GetVersion() == GuidVersion.NameBasedUsingMD5 || guid.GetVersion() == GuidVersion.NameBasedUsingSHA1);
            byte[] ret = guid.ToByteArray();
            ret[7] &= 0x0F;
            ret[8] &= 0x3F;
            return ret;
        }

        /// <summary>
        /// Gets the 122-bit random value used to create this GUID. This is only valid if <see cref="GetVersion"/> returns <see cref="GuidVersion.Random"/>. The most-significant 6 bits of the first octet in the returned array are always 0.
        /// </summary>
        /// <param name="guid">The GUID from which to extract the random value.</param>
        /// <returns>The random value of the GUID.</returns>
        public static IList<byte> GetRandom(this Guid guid)
        {
            Contract.Requires(guid.GetVersion() == GuidVersion.Random);
            byte[] ret = guid.ToByteArray();
            int octet7 = ret[7] & 0x0F;
            int octet8 = ret[8] & 0x3F;
            ret[7] = (byte)(octet7 | (octet8 << 4));
            ret[8] = ret[0];
            ret[0] = (byte)(octet8 >> 4);
            return ret;
        }
    }
}
