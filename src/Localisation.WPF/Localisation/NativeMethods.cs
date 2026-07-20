// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.Localisation.Reactive;
#else
namespace CP.Localisation;
#endif

/// <summary>Provides the native operations used by the localisation library.</summary>
#if NET7_0_OR_GREATER
internal static partial class NativeMethods
#else
internal static class NativeMethods
#endif
{
#if NET7_0_OR_GREATER
    /// <summary>Updates an icon in the Windows notification area.</summary>
    /// <param name="message">The notification-area operation to perform.</param>
    /// <param name="notifyIconData">The notification icon data.</param>
    /// <returns>A nonzero value when the operation succeeds.</returns>
    [LibraryImport("shell32.dll", EntryPoint = "Shell_NotifyIconW")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ShellNotifyIcon(int message, in NotifyIconData notifyIconData);

    /// <summary>Deletes a native graphics object.</summary>
    /// <param name="objectHandle">The handle of the graphics object to delete.</param>
    /// <returns>A value indicating whether the object was deleted.</returns>
    [LibraryImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeleteObject(IntPtr objectHandle);
#else
    /// <summary>Updates an icon in the Windows notification area.</summary>
    /// <param name="message">The notification-area operation to perform.</param>
    /// <param name="notifyIconData">The notification icon data.</param>
    /// <returns>A nonzero value when the operation succeeds.</returns>
    [DllImport("shell32.dll", EntryPoint = "Shell_NotifyIcon", CharSet = CharSet.Auto)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ShellNotifyIcon(int message, in NotifyIconData notifyIconData);

    /// <summary>Deletes a native graphics object.</summary>
    /// <param name="objectHandle">The handle of the graphics object to delete.</param>
    /// <returns>A value indicating whether the object was deleted.</returns>
    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DeleteObject(IntPtr objectHandle);
#endif

    /// <summary>Contains the native data used to remove a notification icon.</summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal readonly struct NotifyIconData
    {
        [MarshalAs(UnmanagedType.I4)]
        private readonly int _size;

        [MarshalAs(UnmanagedType.SysInt)]
        private readonly IntPtr _windowHandle;

        [MarshalAs(UnmanagedType.I4)]
        private readonly int _identifier;

        [MarshalAs(UnmanagedType.I4)]
        private readonly int _flags;

        [MarshalAs(UnmanagedType.I4)]
        private readonly int _callbackMessage;

        [MarshalAs(UnmanagedType.SysInt)]
        private readonly IntPtr _iconHandle;

        private readonly TipBuffer _tip;

        [MarshalAs(UnmanagedType.I4)]
        private readonly int _state;

        [MarshalAs(UnmanagedType.I4)]
        private readonly int _stateMask;

        private readonly InformationBuffer _information;

        [MarshalAs(UnmanagedType.I4)]
        private readonly int _timeoutOrVersion;

        private readonly InformationTitleBuffer _informationTitle;

        [MarshalAs(UnmanagedType.I4)]
        private readonly int _informationFlags;

        /// <summary>Initializes a new instance of the <see cref="NotifyIconData"/> class.</summary>
        /// <param name="windowHandle">The native window handle associated with the icon.</param>
        /// <param name="identifier">The notification icon identifier.</param>
        /// <param name="flags">The fields in this instance that contain valid data.</param>
        /// <param name="callbackMessage">The callback message associated with the icon.</param>
        internal NotifyIconData(IntPtr windowHandle, int identifier, int flags, int callbackMessage)
        {
            _size = Marshal.SizeOf<NotifyIconData>();
            _windowHandle = windowHandle;
            _identifier = identifier;
            _flags = flags;
            _callbackMessage = callbackMessage;
            _iconHandle = IntPtr.Zero;
            _tip = default;
            _state = 0;
            _stateMask = 0;
            _information = default;
            _timeoutOrVersion = 0;
            _informationTitle = default;
            _informationFlags = 0;

            _ = _tip.Reserved;
            _ = _information.Reserved;
            _ = _informationTitle.Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Size = 256)]
        private readonly struct TipBuffer
        {
            internal byte Reserved { get; }
        }

        [StructLayout(LayoutKind.Sequential, Size = 512)]
        private readonly struct InformationBuffer
        {
            internal byte Reserved { get; }
        }

        [StructLayout(LayoutKind.Sequential, Size = 128)]
        private readonly struct InformationTitleBuffer
        {
            internal byte Reserved { get; }
        }
    }
}
