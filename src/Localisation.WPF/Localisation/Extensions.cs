// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace CP.Localisation;

/// <summary>
/// Extensions.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <param name="this">The @this.</param>
    /// <returns>A Value.</returns>
    public static string? ConvertToString(this Enum @this) => ResourceEnumConverter.ConvertToString(@this);

    /// <summary>
    /// Synchronizes the culture information for the current thread.
    /// </summary>
    /// <param name="this">The @this.</param>
    public static void SyncCultureInfo(this CultureInfo @this) => System.Threading.Thread.CurrentThread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture = @this;
}
