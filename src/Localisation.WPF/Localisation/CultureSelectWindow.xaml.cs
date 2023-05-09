// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace CP.Localisation;

/// <summary>
/// Window that allows the user to select the culture to use at design time.
/// </summary>
public partial class CultureSelectWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CultureSelectWindow"/> class.
    /// Create a new instance of the window.
    /// </summary>
    public CultureSelectWindow()
    {
        InitializeComponent();
        var cultures = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.SpecificCultures));
        cultures.Sort(new CultureInfoComparer());
        _cultureCombo.ItemsSource = cultures;
        _cultureCombo.SelectedItem = CultureManager.UICulture;
    }

    private void CultureCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_cultureCombo.SelectedItem is CultureInfo cultureInfo)
        {
            CultureManager.UICulture = cultureInfo;
        }
    }

    /// <summary>
    /// Handle sorting Culture Info.
    /// </summary>
    private class CultureInfoComparer : Comparer<CultureInfo>
    {
        public override int Compare(CultureInfo? x, CultureInfo? y) =>
            x == null
                ? throw new ArgumentNullException(nameof(x))
                : y switch
                {
                    null => throw new ArgumentNullException(nameof(y)),
                    _ => x.DisplayName.CompareTo(y.DisplayName)
                };
    }
}
