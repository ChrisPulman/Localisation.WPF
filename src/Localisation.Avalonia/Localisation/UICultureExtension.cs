// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Avalonia;

#if REACTIVE_SHIM
namespace CP.Localisation.Avalonia.Reactive;
#else
namespace CP.Localisation.Avalonia;
#endif

/// <summary>Provides a binding that tracks the current culture's IETF language tag.</summary>
public sealed class UICultureExtension
{
    private readonly RefreshingObservable<string> _observable = new(
        () => CultureManager.UICulture.IetfLanguageTag,
        resourceKey: null,
        observeResourceChanges: false);

    /// <summary>Forces all culture bindings to reevaluate their values.</summary>
    public static void UpdateAllTargets() => CultureManager.Refresh();

    /// <summary>Creates an Avalonia binding that updates after the UI culture changes.</summary>
    /// <param name="serviceProvider">The XAML service provider.</param>
    /// <returns>An observable Avalonia binding.</returns>
    public object ProvideValue(IServiceProvider? serviceProvider) => _observable.ToBinding();
}
