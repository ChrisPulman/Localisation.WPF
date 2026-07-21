// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.Localisation.Avalonia.Reactive;
#else
namespace CP.Localisation.Avalonia;
#endif

/// <summary>Identifies a resource key that should be refreshed.</summary>
/// <param name="key">The resource key, or <see langword="null"/> for every key.</param>
internal sealed class ResourceRefreshEventArgs(string? key) : EventArgs
{
    /// <summary>Gets the resource key, or <see langword="null"/> for every key.</summary>
    internal string? Key { get; } = key;
}
