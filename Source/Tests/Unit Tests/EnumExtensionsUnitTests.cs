using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink;

namespace UnitTests
{
    [TestClass]
    public class EnumExtensionsUnitTests
    {
        [Flags]
        private enum UnsignedByteEnum : byte
        {
            None = 0,
        }

        [Flags]
        private enum SignedByteEnum : sbyte
        {
            None = 0,
        }

        [Flags]
        private enum UnsignedShortEnum : ushort
        {
            None = 0,
        }

        [Flags]
        private enum SignedShortEnum : short
        {
            None = 0,
        }

        [Flags]
        private enum UnsignedInt32Enum : uint
        {
            None = 0,
        }

        [Flags]
        private enum SignedInt32Enum : int
        {
            None = 0,
        }

        [Flags]
        private enum UnsignedInt64Enum : ulong
        {
            None = 0,
        }

        [Flags]
        private enum SignedInt64Enum : long
        {
            None = 0,
        }

        [TestMethod]
        public void UnsignedByteEnum_HasAllBitsWorking()
        {
            var combined = UnsignedByteEnum.None;

            for (int bit = 0; bit != 8; ++bit)
            {
                uint value = (uint)0x1 << bit;
                var enumValue = (UnsignedByteEnum)value;
                Assert.IsTrue(enumValue.Contains(enumValue));
                Assert.IsTrue(enumValue.Remove(enumValue) == UnsignedByteEnum.None);
                Assert.IsTrue(UnsignedByteEnum.None.Add(enumValue) == enumValue);

                combined = combined.Add(enumValue);
            }

            for (int bit = 0; bit != 8; ++bit)
            {
                uint value = (uint)0x1 << bit;
                var enumValue = (UnsignedByteEnum)value;
                Assert.IsTrue(combined.Contains(enumValue));
                Assert.IsTrue(combined.Add(enumValue) == combined);
                Assert.IsTrue(combined.Remove(enumValue) == (UnsignedByteEnum)~value);
            }
        }

        [TestMethod]
        public void SignedByteEnum_HasAllBitsWorking()
        {
            var combined = SignedByteEnum.None;

            for (int bit = 0; bit != 8; ++bit)
            {
                int value = 0x1 << bit;
                var enumValue = (SignedByteEnum)value;
                Assert.IsTrue(enumValue.Contains(enumValue));
                Assert.IsTrue(enumValue.Remove(enumValue) == SignedByteEnum.None);
                Assert.IsTrue(SignedByteEnum.None.Add(enumValue) == enumValue);

                combined = combined.Add(enumValue);
            }

            for (int bit = 0; bit != 8; ++bit)
            {
                int value = 0x1 << bit;
                var enumValue = (SignedByteEnum)value;
                Assert.IsTrue(combined.Contains(enumValue));
                Assert.IsTrue(combined.Add(enumValue) == combined);
                Assert.IsTrue(combined.Remove(enumValue) == (SignedByteEnum)~value);
            }
        }

        [TestMethod]
        public void UnsignedShortEnum_HasAllBitsWorking()
        {
            var combined = UnsignedShortEnum.None;

            for (int bit = 0; bit != 16; ++bit)
            {
                uint value = (uint)0x1 << bit;
                var enumValue = (UnsignedShortEnum)value;
                Assert.IsTrue(enumValue.Contains(enumValue));
                Assert.IsTrue(enumValue.Remove(enumValue) == UnsignedShortEnum.None);
                Assert.IsTrue(UnsignedShortEnum.None.Add(enumValue) == enumValue);

                combined = combined.Add(enumValue);
            }

            for (int bit = 0; bit != 16; ++bit)
            {
                uint value = (uint)0x1 << bit;
                var enumValue = (UnsignedShortEnum)value;
                Assert.IsTrue(combined.Contains(enumValue));
                Assert.IsTrue(combined.Add(enumValue) == combined);
                Assert.IsTrue(combined.Remove(enumValue) == (UnsignedShortEnum)~value);
            }
        }

        [TestMethod]
        public void SignedShortEnum_HasAllBitsWorking()
        {
            var combined = SignedShortEnum.None;

            for (int bit = 0; bit != 16; ++bit)
            {
                int value = 0x1 << bit;
                var enumValue = (SignedShortEnum)value;
                Assert.IsTrue(enumValue.Contains(enumValue));
                Assert.IsTrue(enumValue.Remove(enumValue) == SignedShortEnum.None);
                Assert.IsTrue(SignedShortEnum.None.Add(enumValue) == enumValue);

                combined = combined.Add(enumValue);
            }

            for (int bit = 0; bit != 16; ++bit)
            {
                int value = 0x1 << bit;
                var enumValue = (SignedShortEnum)value;
                Assert.IsTrue(combined.Contains(enumValue));
                Assert.IsTrue(combined.Add(enumValue) == combined);
                Assert.IsTrue(combined.Remove(enumValue) == (SignedShortEnum)~value);
            }
        }

        [TestMethod]
        public void UnsignedInt32Enum_HasAllBitsWorking()
        {
            var combined = UnsignedInt32Enum.None;

            for (int bit = 0; bit != 32; ++bit)
            {
                uint value = (uint)0x1 << bit;
                var enumValue = (UnsignedInt32Enum)value;
                Assert.IsTrue(enumValue.Contains(enumValue));
                Assert.IsTrue(enumValue.Remove(enumValue) == UnsignedInt32Enum.None);
                Assert.IsTrue(UnsignedInt32Enum.None.Add(enumValue) == enumValue);

                combined = combined.Add(enumValue);
            }

            for (int bit = 0; bit != 32; ++bit)
            {
                uint value = (uint)0x1 << bit;
                var enumValue = (UnsignedInt32Enum)value;
                Assert.IsTrue(combined.Contains(enumValue));
                Assert.IsTrue(combined.Add(enumValue) == combined);
                Assert.IsTrue(combined.Remove(enumValue) == (UnsignedInt32Enum)~value);
            }
        }

        [TestMethod]
        public void SignedInt32Enum_HasAllBitsWorking()
        {
            var combined = SignedInt32Enum.None;

            for (int bit = 0; bit != 32; ++bit)
            {
                int value = 0x1 << bit;
                var enumValue = (SignedInt32Enum)value;
                Assert.IsTrue(enumValue.Contains(enumValue));
                Assert.IsTrue(enumValue.Remove(enumValue) == SignedInt32Enum.None);
                Assert.IsTrue(SignedInt32Enum.None.Add(enumValue) == enumValue);

                combined = combined.Add(enumValue);
            }

            for (int bit = 0; bit != 32; ++bit)
            {
                int value = 0x1 << bit;
                var enumValue = (SignedInt32Enum)value;
                Assert.IsTrue(combined.Contains(enumValue));
                Assert.IsTrue(combined.Add(enumValue) == combined);
                Assert.IsTrue(combined.Remove(enumValue) == (SignedInt32Enum)~value);
            }
        }

        [TestMethod]
        public void UnsignedInt64Enum_HasAllBitsWorking()
        {
            var combined = UnsignedInt64Enum.None;

            for (int bit = 0; bit != 64; ++bit)
            {
                ulong value = (ulong)0x1 << bit;
                var enumValue = (UnsignedInt64Enum)value;
                Assert.IsTrue(enumValue.Contains(enumValue));
                Assert.IsTrue(enumValue.Remove(enumValue) == UnsignedInt64Enum.None);
                Assert.IsTrue(UnsignedInt64Enum.None.Add(enumValue) == enumValue);

                combined = combined.Add(enumValue);
            }

            for (int bit = 0; bit != 64; ++bit)
            {
                ulong value = (ulong)0x1 << bit;
                var enumValue = (UnsignedInt64Enum)value;
                Assert.IsTrue(combined.Contains(enumValue));
                Assert.IsTrue(combined.Add(enumValue) == combined);
                Assert.IsTrue(combined.Remove(enumValue) == (UnsignedInt64Enum)~value);
            }
        }

        [TestMethod]
        public void SignedInt64Enum_HasAllBitsWorking()
        {
            var combined = SignedInt64Enum.None;

            for (int bit = 0; bit != 64; ++bit)
            {
                long value = (long)0x1 << bit;
                var enumValue = (SignedInt64Enum)value;
                Assert.IsTrue(enumValue.Contains(enumValue));
                Assert.IsTrue(enumValue.Remove(enumValue) == SignedInt64Enum.None);
                Assert.IsTrue(SignedInt64Enum.None.Add(enumValue) == enumValue);

                combined = combined.Add(enumValue);
            }

            for (int bit = 0; bit != 64; ++bit)
            {
                long value = (long)0x1 << bit;
                var enumValue = (SignedInt64Enum)value;
                Assert.IsTrue(combined.Contains(enumValue));
                Assert.IsTrue(combined.Add(enumValue) == combined);
                Assert.IsTrue(combined.Remove(enumValue) == (SignedInt64Enum)~value);
            }
        }
    }
}
