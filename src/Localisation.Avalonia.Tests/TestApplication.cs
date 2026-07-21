// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Headless;

namespace Localisation.Avalonia.Tests;

internal sealed class TestApplication : Application
{
    public override void Initialize()
    {
    }

    internal static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>().UseHeadless(new());
}
