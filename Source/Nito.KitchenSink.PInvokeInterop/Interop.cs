// <copyright file="Interop.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.PInvokeInterop
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Useful methods for p/Invoke interop.
    /// </summary>
    public static class Interop
    {
        /// <summary>
        /// Converts an unmanaged function pointer to a delegate.
        /// </summary>
        /// <typeparam name="T">The type of delegate to convert to.</typeparam>
        /// <param name="pointer">The pointer to convert. May not be <c>null</c>.</param>
        /// <returns>The function pointer, as a delegate.</returns>
        public static T GetDelegateForFunctionPointer<T>(IntPtr pointer) where T : class
        {
            Contract.Requires(pointer != null);
            Contract.Ensures(Contract.Result<T>() != null);
            return (T)(object)Marshal.GetDelegateForFunctionPointer(pointer, typeof(T));
        }

        /// <summary>
        /// Returns a <see cref="Win32Exception"/> with the error code of <see cref="Marshal.GetLastWin32Error"/>.
        /// </summary>
        /// <returns>A <see cref="Win32Exception"/> with the error code of <see cref="Marshal.GetLastWin32Error"/>.</returns>
        public static Win32Exception GetLastWin32Exception()
        {
            Contract.Ensures(Contract.Result<Win32Exception>() != null);
            return new Win32Exception(Marshal.GetLastWin32Error(), "Win32 Error 0x" + Marshal.GetLastWin32Error().ToString("X") + ": " + new Win32Exception().Message);
        }

        /// <summary>
        /// Returns a <see cref="Win32Exception"/> with the error code of <see cref="Marshal.GetLastWin32Error"/>.
        /// </summary>
        /// <param name="function">The name of the last function that was called. This is included in the error message.</param>
        /// <returns>A <see cref="Win32Exception"/> with the error code of <see cref="Marshal.GetLastWin32Error"/>.</returns>
        public static Win32Exception GetLastWin32Exception(string function)
        {
            Contract.Ensures(Contract.Result<Win32Exception>() != null);
            return new Win32Exception(Marshal.GetLastWin32Error(), "Win32 Error 0x" + Marshal.GetLastWin32Error().ToString("X") + " from " + function + ": " + new Win32Exception().Message);
        }

        /// <summary>
        /// Returns an error message from a message table in a specific DLL; throws an exception if the error code is not defined in the dll. This method will affect <see cref="Marshal.GetLastWin32Error"/>.
        /// </summary>
        /// <param name="dll">The DLL to search for the message.</param>
        /// <param name="code">The code of the message to find.</param>
        /// <returns>The error message, if found in the DLL; otherwise, an exception is thrown.</returns>
        public static string FormatMessageFromDll(string dll, int code)
        {
            Contract.Ensures(Contract.Result<string>() != null);
            return SafeNativeMethods.FormatMessageFromModule(dll, (uint)code);
        }

        /// <summary>
        /// Returns an error message from a message table in a specific DLL; returns null if there was some error retrieving the error message. This method will affect <see cref="Marshal.GetLastWin32Error"/>.
        /// </summary>
        /// <param name="dll">The DLL to search for the message.</param>
        /// <param name="code">The code of the message to find.</param>
        /// <returns>The error message, if found in the DLL; otherwise, <c>null</c>.</returns>
        public static string TryFormatMessageFromDll(string dll, int code)
        {
            return SafeNativeMethods.TryFormatMessageFromModule(dll, (uint)code);
        }
    }
}
