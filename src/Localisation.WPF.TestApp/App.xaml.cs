// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows;
using CP.Localisation;

namespace Localisation.WPF.TestApp;

/// <summary>Interaction logic for App.xaml.</summary>
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public partial class App : Application
{
    /// <summary>Initializes a new instance of the <see cref="App"/> class.</summary>
    public App()
    {
        CultureManager.UICulture = new("en-US");
        CultureManager.UICulture.SyncCultureInfo();
    }

    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString() ?? GetType().Name;
}
