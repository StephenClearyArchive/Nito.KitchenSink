// <copyright file="SafeNativeMethods.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.PInvokeInterop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Native methods that are safe for any caller.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal static partial class SafeNativeMethods
    {
        /// <summary>
        /// Instructs <c>FormatMessage</c> to allocate the message buffer.
        /// </summary>
        public const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;

        /// <summary>
        /// Informs <c>FormatMessage</c> to search the given <c>HMODULE</c> for the message definition.
        /// </summary>
        public const uint FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;

        /// <summary>
        /// Instructs <c>FormatMessage</c> to ignore insertion parameters.
        /// </summary>
        public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;

        /// <summary>
        /// Instructs <c>LoadLibraryEx</c> to load the DLL but not execute it.
        /// </summary>
        public const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

        /// <summary>
        /// Formats the message from the given DLL.
        /// </summary>
        /// <param name="dllName">The name of the DLL to search for the message definition.</param>
        /// <param name="code">The code identifying the message to look up.</param>
        /// <returns>The message definition.</returns>
        public static string FormatMessageFromModule(string dllName, uint code)
        {
            Contract.Ensures(Contract.Result<string>() != null);
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
                    var ret = Marshal.PtrToStringAuto(localMemory.DangerousGetHandle(), (int)charsNotIncludingNull);
                    Contract.Assume(ret != null);
                    return ret;
                }
            }
        }

        /// <summary>
        /// Tries to format the message from the given DLL. Returns <c>null</c> if the DLL could not be loaded or does not contain a definition for the message.
        /// </summary>
        /// <param name="dllName">The name of the DLL to search for the message definition.</param>
        /// <param name="code">The code identifying the message to look up.</param>
        /// <returns>The message definition.</returns>
        public static string TryFormatMessageFromModule(string dllName, uint code)
        {
            SafeModuleHandle dll = DoLoadLibraryEx(dllName, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
            Contract.Assume(dll != null);
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

                Contract.Assume(localMemory != null);
                using (localMemory)
                {
                    return Marshal.PtrToStringAuto(localMemory.DangerousGetHandle(), (int)charsNotIncludingNull);
                }
            }
        }

        /// <summary>
        /// Loads the specified DLL library.
        /// </summary>
        /// <param name="fileName">Name of the DLL to load.</param>
        /// <param name="flags">Flags that affect the loading of the library.</param>
        /// <returns>A handle to the loaded DLL.</returns>
        private static SafeModuleHandle LoadLibraryEx(string fileName, uint flags)
        {
            Contract.Ensures(Contract.Result<SafeModuleHandle>() != null);
            Contract.Ensures(!Contract.Result<SafeModuleHandle>().IsInvalid);

            SafeModuleHandle ret = DoLoadLibraryEx(fileName, IntPtr.Zero, flags);
            Contract.Assume(ret != null);
            if (ret.IsInvalid)
            {
                throw Interop.GetLastWin32Exception();
            }

            return ret;
        }

        /// <summary>
        /// Formats the message from the given DLL, having the system allocate the message buffer.
        /// </summary>
        /// <param name="flags">The formatting message flags. This must include <see cref="FORMAT_MESSAGE_ALLOCATE_BUFFER"/>, <see cref="FORMAT_MESSAGE_FROM_HMODULE"/>, and <see cref="FORMAT_MESSAGE_IGNORE_INSERTS"/>.</param>
        /// <param name="dll">The DLL to search for the message definition. May not be <c>null</c>.</param>
        /// <param name="code">The code identifying the message to look up.</param>
        /// <param name="localMemory">On return, contains a handle to a local memory buffer allocated by the system.</param>
        /// <param name="minimumBufferSize">Minimum size of the buffer local memory buffer to allocate.</param>
        /// <returns>The number of valid characters in the local memory buffer.</returns>
        private static uint FormatMessageFromModuleAllocatingBuffer(uint flags, SafeModuleHandle dll, uint code, out SafeLocalMemoryHandle localMemory, uint minimumBufferSize)
        {
            Contract.Requires(dll != null);
            Contract.Ensures(Contract.ValueAtReturn<SafeLocalMemoryHandle>(out localMemory) != null);
            Contract.Ensures(Contract.Result<uint>() != 0);

            uint ret = DoFormatMessageFromModuleAllocatingBuffer(flags, dll, code, 0, out localMemory, minimumBufferSize, IntPtr.Zero);
            if (ret == 0)
            {
                throw Interop.GetLastWin32Exception();
            }

            Contract.Assume(localMemory != null);
            return ret;
        }

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("Kernel32.dll", EntryPoint = "LoadLibraryEx", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern SafeModuleHandle DoLoadLibraryEx(string lpFileName, IntPtr hFile, uint flags);

        [DllImport("Kernel32.dll", EntryPoint = "FormatMessage", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern uint DoFormatMessageFromModuleAllocatingBuffer(uint flags, SafeModuleHandle lpSource, uint dwMessageId, uint dwLanguageId, out SafeLocalMemoryHandle lpBuffer, uint nSize, IntPtr arguments);

        /// <summary>
        /// A handle to a DLL (HMODULE).
        /// </summary>
        private sealed class SafeModuleHandle : SafeHandle
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SafeModuleHandle"/> class.
            /// </summary>
            public SafeModuleHandle()
                : base(IntPtr.Zero, true)
            {
            }

            /// <summary>
            /// Gets a value indicating whether the handle value is invalid.
            /// </summary>
            public override bool IsInvalid
            {
                get { return this.handle == IntPtr.Zero; }
            }

            /// <summary>
            /// Frees the handle.
            /// </summary>
            /// <returns>true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false. In this case, it generates a releaseHandleFailed MDA Managed Debugging Assistant.</returns>
            protected override bool ReleaseHandle()
            {
                return SafeNativeMethods.FreeLibrary(this.handle);
            }
        }

        /// <summary>
        /// A handle to local memory (PVOID) that must be freed by <c>LocalFree</c>.
        /// </summary>
        private sealed class SafeLocalMemoryHandle : SafeHandle
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SafeLocalMemoryHandle"/> class.
            /// </summary>
            public SafeLocalMemoryHandle()
                : base(IntPtr.Zero, true)
            {
            }

            /// <summary>
            /// Gets a value indicating whether the handle value is invalid.
            /// </summary>
            public override bool IsInvalid
            {
                get { return this.handle == IntPtr.Zero; }
            }

            /// <summary>
            /// Frees the handle.
            /// </summary>
            /// <returns>true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false. In this case, it generates a releaseHandleFailed MDA Managed Debugging Assistant.</returns>
            protected override bool ReleaseHandle()
            {
                return SafeNativeMethods.LocalFree(this.handle) == IntPtr.Zero;
            }
        }
    }
}
