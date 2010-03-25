// <copyright file="Interop.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Useful methods for p/Invoke interop.
    /// </summary>
    public static class Interop
    {
        /// <summary>
        /// Converts an unmanaged function pointer to a delegate.
        /// </summary>
        /// <typeparam name="T">The type of delegate to convert to.</typeparam>
        /// <param name="pointer">The pointer to convert.</param>
        /// <returns>The function pointer, as a delegate.</returns>
        public static T GetDelegateForFunctionPointer<T>(IntPtr pointer)
        {
            return (T)(object)Marshal.GetDelegateForFunctionPointer(pointer, typeof(T));
        }

        /// <summary>
        /// Returns a <see cref="Win32Exception"/> with the error code of <see cref="Marshal.GetLastWin32Error"/>.
        /// </summary>
        /// <returns>A <see cref="Win32Exception"/> with the error code of <see cref="Marshal.GetLastWin32Error"/>.</returns>
        public static Win32Exception GetLastWin32Exception()
        {
            return new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
