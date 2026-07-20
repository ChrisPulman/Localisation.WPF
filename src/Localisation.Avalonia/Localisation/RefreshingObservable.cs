// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

#if REACTIVE_SHIM
namespace CP.Localisation.Avalonia.Reactive;
#else
namespace CP.Localisation.Avalonia;
#endif

/// <summary>Creates an observable value that reevaluates when localization state changes.</summary>
/// <typeparam name="T">The value type.</typeparam>
/// <param name="valueFactory">Creates the current value.</param>
/// <param name="resourceKey">The resource key observed for targeted refreshes.</param>
/// <param name="observeResourceChanges">Whether resource refresh notifications are observed.</param>
internal sealed class RefreshingObservable<T>(Func<T> valueFactory, string? resourceKey, bool observeResourceChanges) : IObservable<T>
{
    private readonly bool _observeResourceChanges = observeResourceChanges;

    private readonly string? _resourceKey = resourceKey;

    private readonly Func<T> _valueFactory = valueFactory;

    /// <inheritdoc />
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);

        void Publish() => observer.OnNext(_valueFactory());

        EventHandler cultureChanged = (_, _) => Publish();

        EventHandler<ResourceRefreshEventArgs> resourceRefresh = (_, args) =>
        {
            if (!_observeResourceChanges
                || (args.Key is not null && !string.Equals(args.Key, _resourceKey, StringComparison.Ordinal)))
            {
                return;
            }

            Publish();
        };

        Publish();
        CultureManager.UICultureChanged += cultureChanged;
        ResxExtension.RefreshRequested += resourceRefresh;

        return new CallbackDisposable(() =>
        {
            CultureManager.UICultureChanged -= cultureChanged;
            ResxExtension.RefreshRequested -= resourceRefresh;
        });
    }

    private sealed class CallbackDisposable(Action callback) : IDisposable
    {
        private Action? _callback = callback;

        /// <inheritdoc />
        public void Dispose() => Interlocked.Exchange(ref _callback, null)?.Invoke();
    }
}
