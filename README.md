# Localisation.WPF
Localisation library for WPF - use with ResxManager

![Nuget](https://img.shields.io/nuget/dt/Localisation.WPF?color=pink&style=plastic)
[![NuGet](https://img.shields.io/nuget/v/Localisation.WPF.svg?style=plastic)](https://www.nuget.org/packages/Localisation.WPF)

## Usage

### 1. Create a new WPF project

### 2. Add a reference to the Localisation.WPF library

### 3. Add the ResxManager Visual Studio extension

### 4. Add a new resource file to the project

### 5. Add some resources to the resource file

### 6. Add the following to the project App.xaml.cs file to set the UI culture

```csharp
using CP.Localisation;
using System.Globalization;

    public App()
    {
        CultureManager.UICulture = new CultureInfo("en-US");
        CultureManager.UICulture.SyncCultureInfo();
    }
```

### 7. Add the following to each xaml file to set the binding culture
```xml
    Language="{UICulture}"
    ResxExtension.DefaultResxName="YOUR.NAMESPACE.Properties.Resources"
```

### 8. Add the following Resx binding to each xaml element to set the binding to the current language value
```xml
    <TextBlock Text="{Resx YOUR_RESOURCE_NAME}" />
```
