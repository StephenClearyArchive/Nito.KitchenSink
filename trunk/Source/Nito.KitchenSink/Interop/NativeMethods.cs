using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;

namespace Nito.KitchenSink
{
    internal static partial class NativeMethods
    {
        public const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        public const uint FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        public const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

        [DllImport("Kernel32.dll", SetLastError = true), SuppressUnmanagedCodeSecurity]
        [return:MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("Kernel32.dll", SetLastError = true), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("Kernel32.dll", EntryPoint = "LoadLibraryEx", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true), SuppressUnmanagedCodeSecurity]
        private static extern SafeModuleHandle DoLoadLibraryEx(string lpFileName, IntPtr hFile, uint flags);

        private static SafeModuleHandle LoadLibraryEx(string fileName, uint flags)
        {
            SafeModuleHandle ret = DoLoadLibraryEx(fileName, IntPtr.Zero, flags);
            if (ret.IsInvalid)
            {
                throw Interop.GetLastWin32Exception();
            }

            return ret;
        }

        [DllImport("Kernel32.dll", EntryPoint = "FormatMessage", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true), SuppressUnmanagedCodeSecurity]
        private static extern uint DoFormatMessageFromModuleAllocatingBuffer(uint flags, SafeModuleHandle lpSource, uint dwMessageId, uint dwLanguageId, out SafeLocalMemoryHandle lpBuffer, uint nSize, IntPtr arguments);

        private static uint FormatMessageFromModuleAllocatingBuffer(uint flags, SafeModuleHandle dll, uint code, out SafeLocalMemoryHandle localMemory, uint minimumBufferSize)
        {
            uint ret = DoFormatMessageFromModuleAllocatingBuffer(flags, dll, code, 0, out localMemory, minimumBufferSize, IntPtr.Zero);
            if (ret == 0)
            {
                throw Interop.GetLastWin32Exception();
            }

            return ret;
        }

        public static string FormatMessageFromModule(string dllName, uint code)
        {
            using (var dll = LoadLibraryEx(dllName, LOAD_LIBRARY_AS_DATAFILE))
            {
                SafeLocalMemoryHandle localMemory;
                uint charsNotIncludingNull = FormatMessageFromModuleAllocatingBuffer(
                    FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_HMODULE | FORMAT_MESSAGE_IGNORE_INSERTS,
                    dll,
                    code,
                    out localMemory,
                    0);
                using (localMemory)
                {
                    return Marshal.PtrToStringAuto(localMemory.DangerousGetHandle(), (int)charsNotIncludingNull);
                }
            }
        }

        public static string TryFormatMessageFromModule(string dllName, uint code)
        {
            SafeModuleHandle dll = DoLoadLibraryEx(dllName, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
            if (dll.IsInvalid)
            {
                return null;
            }

            using (dll)
            {
                SafeLocalMemoryHandle localMemory;
                uint charsNotIncludingNull = DoFormatMessageFromModuleAllocatingBuffer(
                    FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_HMODULE | FORMAT_MESSAGE_IGNORE_INSERTS,
                    dll,
                    code,
                    0,
                    out localMemory,
                    0,
                    IntPtr.Zero);
                if (charsNotIncludingNull == 0)
                {
                    return null;
                }

                using (localMemory)
                {
                    return Marshal.PtrToStringAuto(localMemory.DangerousGetHandle(), (int)charsNotIncludingNull);
                }
            }
        }

        private sealed class SafeModuleHandle : SafeHandle
        {
            public SafeModuleHandle()
                : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid
            {
                get { return this.handle == IntPtr.Zero; }
            }

            protected override bool ReleaseHandle()
            {
                return NativeMethods.FreeLibrary(this.handle);
            }
        }

        private sealed class SafeLocalMemoryHandle : SafeHandle
        {
            public SafeLocalMemoryHandle()
                : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid
            {
                get { return this.handle == IntPtr.Zero; }
            }

            protected override bool ReleaseHandle()
            {
                return NativeMethods.LocalFree(this.handle) == IntPtr.Zero;
            }
        }
    }
}
