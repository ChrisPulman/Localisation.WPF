// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Threading;

#if REACTIVE_SHIM
namespace CP.Localisation.Avalonia.Reactive;
#else
namespace CP.Localisation.Avalonia;
#endif

/// <summary>Coordinates the culture used by Avalonia localization bindings.</summary>
public static class CultureManager
{
    private static readonly Signal<RxVoid> _uiCultureChangedSignal = new();

    private static bool _synchronizeThreadCulture = true;

    private static CultureInfo _uiCulture = CultureInfo.CurrentUICulture;

    /// <summary>Raised after the active UI culture changes or targets are explicitly refreshed.</summary>
    public static event EventHandler? UICultureChanged;

    /// <summary>Gets or sets whether the current and default thread cultures follow <see cref="UICulture"/>.</summary>
    public static bool SynchronizeThreadCulture
    {
        get => _synchronizeThreadCulture;

        set
        {
            _synchronizeThreadCulture = value;
            if (!value)
            {
                return;
            }

            SynchronizeCulture(_uiCulture);
        }
    }

    /// <summary>Gets or sets the culture used to resolve localized values.</summary>
    public static CultureInfo UICulture
    {
        get => _uiCulture;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (string.Equals(_uiCulture.Name, value.Name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _uiCulture = value;
            if (_synchronizeThreadCulture)
            {
                SynchronizeCulture(value);
            }

            PublishChange();
        }
    }

    /// <summary>Gets an observable notification emitted whenever localized targets should refresh.</summary>
    public static IObservable<RxVoid> UICultureChangedObserver => _uiCultureChangedSignal;

    /// <summary>Forces all culture-aware bindings to reevaluate their values.</summary>
    public static void Refresh() => PublishChange();

    private static void PublishChange()
    {
        UICultureChanged?.Invoke(null, EventArgs.Empty);
        _uiCultureChangedSignal.OnNext(RxVoid.Default);
    }

    private static void SynchronizeCulture(CultureInfo culture)
    {
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
