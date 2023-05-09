// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Windows;
using CP.Localisation;

namespace Localisation.WPF.TestApp
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            CultureManager.UICulture = new CultureInfo("en-US");
            CultureManager.UICulture.SyncCultureInfo();
        }
    }
}
