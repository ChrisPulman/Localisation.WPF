// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

#if REACTIVE_SHIM
namespace CP.Localisation.Reactive;
#else
namespace CP.Localisation;
#endif

/// <summary>Provides localisation-related extension methods.</summary>
public static class Extensions
{
    /// <summary>Provides culture helpers.</summary>
    /// <param name="culture">The culture applied to the current thread.</param>
    extension(CultureInfo culture)
    {
        /// <summary>Synchronizes the current thread culture and UI culture.</summary>
        public void SyncCultureInfo()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
        }
    }

    /// <summary>Provides localised enumeration display helpers.</summary>
    /// <param name="value">The enumeration value to localise.</param>
    extension(Enum value)
    {
        /// <summary>Converts an enumeration value to its localised display string.</summary>
        /// <returns>The localised display string, when one can be resolved.</returns>
        public string? ConvertToString() => ResourceEnumConverter.ConvertToString(value);
    }
}
