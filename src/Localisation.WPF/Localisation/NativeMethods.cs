// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace CP;

internal static class NativeMethods
{
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    internal static extern int Shell_NotifyIcon(int message, NOTIFYICONDATA pnid);

    [DllImport("gdi32.dll")]
    internal static extern bool DeleteObject(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal class NOTIFYICONDATA
    {
#pragma warning disable SA1401 // Fields should be private
        public int CbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA));

        public IntPtr HWnd;

        public int UID;

        public int UFlags;

        public int UCallbackMessage;

        public IntPtr HIcon;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string? SzTip;

        public int DwState;

        public int DwStateMask;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string? SzInfo;

        public int UTimeoutOrVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string? SzInfoTitle;

        public int DwInfoFlags;
#pragma warning restore SA1401 // Fields should be private
    }
}
