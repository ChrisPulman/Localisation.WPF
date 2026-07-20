// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

#if REACTIVE_SHIM
namespace CP.Localisation.Reactive;
#else
namespace CP.Localisation;
#endif

/// <summary>Contains the data used to resolve a localised resource dynamically.</summary>
public sealed class GetResourceEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="GetResourceEventArgs"/> class.</summary>
    /// <param name="resxName">The fully qualified resource name.</param>
    /// <param name="key">The resource key.</param>
    /// <param name="culture">The culture used to resolve the resource.</param>
    public GetResourceEventArgs(string? resxName, string? key, CultureInfo culture)
    {
        ResxName = resxName;
        Key = key;
        Culture = culture ?? throw new ArgumentNullException(nameof(culture));
    }

    /// <summary>Gets the culture used to resolve the resource.</summary>
    public CultureInfo Culture { get; }

    /// <summary>Gets the resource key.</summary>
    public string? Key { get; }

    /// <summary>Gets the fully qualified resource name.</summary>
    public string? ResxName { get; }

    /// <summary>Gets or sets the dynamically resolved resource value.</summary>
    public object? Resource { get; set; }
}
