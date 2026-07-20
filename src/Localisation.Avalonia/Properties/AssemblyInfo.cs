// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Avalonia.Metadata;

[assembly: InternalsVisibleTo("Localisation.Avalonia.Tests")]

#if REACTIVE_SHIM
[assembly: Guid("4ae3c9bd-a4b6-4c3f-8797-6c6359b93248")]
[assembly: XmlnsDefinition("https://github.com/ChrisPulman/Localisation.Avalonia.Reactive", "CP.Localisation.Avalonia.Reactive")]
[assembly: XmlnsPrefix("https://github.com/ChrisPulman/Localisation.Avalonia.Reactive", "localisationReactive")]
#else
[assembly: Guid("09426e4c-9e0f-487e-b901-8ed54701c038")]
[assembly: XmlnsDefinition("https://github.com/ChrisPulman/Localisation.Avalonia", "CP.Localisation.Avalonia")]
[assembly: XmlnsPrefix("https://github.com/ChrisPulman/Localisation.Avalonia", "localisation")]
#endif
