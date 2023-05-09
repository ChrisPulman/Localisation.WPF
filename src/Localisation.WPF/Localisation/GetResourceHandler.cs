// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace CP.Localisation;

/// <summary>
/// Defines the handling method for the <see cref="ResxExtension.GetResource"/> event.
/// </summary>
/// <param name="resxName">The name of the resx file.</param>
/// <param name="key">The resource key within the file.</param>
/// <param name="culture">The culture to get the resource for.</param>
/// <returns>The resource.</returns>
public delegate object GetResourceHandler(string? resxName, string? key, CultureInfo culture);
