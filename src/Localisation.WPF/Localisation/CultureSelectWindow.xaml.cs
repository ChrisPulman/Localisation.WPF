// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

#if REACTIVE_SHIM
namespace CP.Localisation.Reactive;
#else
namespace CP.Localisation;
#endif

/// <summary>Window that allows the user to select the culture to use at design time.</summary>
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public partial class CultureSelectWindow : Window
{
    /// <summary>Initializes a new instance of the <see cref="CultureSelectWindow"/> class. Create a new instance of the window.</summary>
    public CultureSelectWindow()
    {
        InitializeComponent();
        var cultures = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.SpecificCultures));
        cultures.Sort(new CultureInfoComparer());
        _cultureCombo.ItemsSource = cultures;
        _cultureCombo.SelectedItem = CultureManager.UICulture;
    }

    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString() ?? GetType().Name;

    private void CultureCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_cultureCombo.SelectedItem is not CultureInfo cultureInfo)
        {
            return;
        }

        CultureManager.UICulture = cultureInfo;
    }

    /// <summary>Handle sorting Culture Info.</summary>
    private sealed class CultureInfoComparer : Comparer<CultureInfo>
    {
        public override int Compare(CultureInfo? x, CultureInfo? y) =>
            x is null
                ? throw new ArgumentNullException(nameof(x))
                : y switch
                {
                    null => throw new ArgumentNullException(nameof(y)),
                    _ => x.DisplayName.CompareTo(y.DisplayName)
                };
    }
}
