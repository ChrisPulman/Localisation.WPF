// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows.Data;

namespace CP.Localisation;

/// <summary>
/// Defines a type converter for enum values that converts enum values to and from string
/// representations using resources.
/// </summary>
/// <remarks>
/// This class makes Localisation of display values for enums in a project easy. Simply derive a
/// class from this class and pass the ResourceManagerin the constructor.
/// <code lang="C#" escaped="true">
/// class LocalizedEnumConverter : ResourceEnumConverter
/// {
/// public LocalizedEnumConverter(Type type)
/// : base(type, Properties.Resources.ResourceManager)
/// {
/// }
/// }
/// </code>
/// <code lang="Visual Basic" escaped="true">
/// Public Class LocalizedEnumConverter
///
/// Inherits ResourceEnumConverter
/// Public Sub New(ByVal sType as Type)
/// MyBase.New(sType, My.Resources.ResourceManager)
/// End Sub
/// End Class
/// </code>
/// Then define the enum values in the resource editor. The names of the resources are simply the
/// enum value prefixed by the enum type name with an underscore separator eg MyEnum_MyValue. You
/// can then use the TypeConverter attribute to make the LocalizedEnumConverter the default
/// TypeConverter for the enums in your project.
/// </remarks>
public class ResourceEnumConverter : EnumConverter, IValueConverter
{
    private readonly Array? _flagValues;

    private readonly bool _isFlagEnum;

    private readonly Dictionary<CultureInfo, LookupTable> _lookupTables = new();

    private readonly ResourceManager _resourceManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceEnumConverter"/> class.
    /// Create a new instance of the converter using translations from the given resource manager.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="resourceManager">The Resource Manager.</param>
    public ResourceEnumConverter(Type type, ResourceManager resourceManager)
        : base(type)
    {
        _resourceManager = resourceManager;
        var flagAttributes = type?.GetCustomAttributes(typeof(FlagsAttribute), true);
        _isFlagEnum = flagAttributes?.Length > 0;
        if (_isFlagEnum)
        {
            _flagValues = Enum.GetValues(type!);
        }
    }

    /// <summary>
    /// Convert the given enum value to string using the registered type converter.
    /// </summary>
    /// <param name="value">The enum value to convert to string.</param>
    /// <returns>The localized string value for the enum.</returns>
    public static string? ConvertToString(Enum value)
    {
        var converter = TypeDescriptor.GetConverter(value?.GetType()!);
        return converter.ConvertToString(value);
    }

    /// <summary>
    /// Return a list of the enum values and their associated display text for the given enum type.
    /// </summary>
    /// <param name="enumType">The enum type to get the values for.</param>
    /// <param name="culture">The culture to get the text for.</param>
    /// <returns>
    /// A list of KeyValuePairs where the key is the enum value and the value is the text to display.
    /// </returns>
    /// <remarks>
    /// This method can be used to provide localized binding to enums in ASP.NET applications.
    /// Unlike windows forms the standard ASP.NET controls do not use TypeConverters to convert
    /// from enum values to the displayed text. You can bind an ASP.NET control to the list
    /// returned by this method by setting the DataValueField to "Key" and theDataTextField to "Value".
    /// </remarks>
    public static List<KeyValuePair<Enum, string?>> GetValues(Type enumType, CultureInfo culture)
    {
        var result = new List<KeyValuePair<Enum, string?>>();
        var converter = TypeDescriptor.GetConverter(enumType);
        foreach (Enum value in Enum.GetValues(enumType))
        {
            var pair = new KeyValuePair<Enum, string?>(value!, converter.ConvertToString(null, culture, value));
            result.Add(pair);
        }

        return result;
    }

    /// <summary>
    /// Return a list of the enum values and their associated display text for the given enum
    /// type in the current UI Culture.
    /// </summary>
    /// <param name="enumType">The enum type to get the values for.</param>
    /// <returns>
    /// A list of KeyValuePairs where the key is the enum value and the value is the text to display.
    /// </returns>
    /// <remarks>
    /// This method can be used to provide localized binding to enums in ASP.NET applications.
    /// Unlike windows forms the standard ASP.NET controls do not use TypeConverters to convert
    /// from enum values to the displayed text. You can bind an ASP.NET control to the list
    /// returned by this method by setting the DataValueField to "Key" and theDataTextField to "Value".
    /// </remarks>
    public static List<KeyValuePair<Enum, string?>> GetValues(Type enumType) => GetValues(enumType, CultureInfo.CurrentUICulture);

    /// <summary>
    /// Handle XAML Conversion from this type to other types.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">not used.</param>
    /// <param name="culture">The culture to convert.</param>
    /// <returns>The converted value.</returns>
    object? IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) => ConvertTo(null, culture, value, targetType);

    /// <summary>
    /// Handle XAML Conversion from other types back to this type.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">not used.</param>
    /// <param name="culture">The culture to convert.</param>
    /// <returns>The converted value.</returns>
    object? IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => ConvertFrom(null, culture, value);

    /// <summary>
    /// Convert string values to enum values.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="value">The value.</param>
    /// <returns>A Value.</returns>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        culture ??= CultureInfo.CurrentCulture;

        return value is string @string
            ? _isFlagEnum ? GetFlagValue(culture, @string) : GetValue(culture, @string) ?? base.ConvertFrom(context, culture, value)
            : base.ConvertFrom(context, culture, value);
    }

    /// <summary>
    /// Convert the enum value to a string.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="value">The value.</param>
    /// <param name="destinationType">The destination Type.</param>
    /// <returns>A Value.</returns>
    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        culture ??= CultureInfo.CurrentCulture;

        return (value == null
            ? null
            : destinationType == typeof(string) || destinationType == typeof(object)
            ? _isFlagEnum ? GetFlagValueText(culture, value) : GetValueText(culture, value)
            : base.ConvertTo(context, culture, value, destinationType))!;
    }

    /// <summary>
    /// Return the name of the resource to use.
    /// </summary>
    /// <param name="value">The value to get.</param>
    /// <returns>The name of the resource to use.</returns>
    protected virtual string GetResourceName(object value)
    {
        var type = value?.GetType();
        return $"{type?.Name}_{value}";
    }

    private static bool IsSingleBitValue(ulong value) => value switch
    {
        0 => false,
        1 => true,
        _ => (value & (value - 1)) == 0,
    };

    private object? GetFlagValue(CultureInfo culture, string text)
    {
        var lookupTable = GetLookupTable(culture);
        var textValues = text.Split(',');
        ulong result = 0;
        foreach (var textValue in textValues)
        {
            var trimmedTextValue = textValue.Trim();
            if (!lookupTable.TryGetValue(trimmedTextValue, out var value))
            {
                return null;
            }

            result |= Convert.ToUInt32(value);
        }

        return Enum.ToObject(EnumType, result);
    }

    private string? GetFlagValueText(CultureInfo culture, object value)
    {
        // if there is a standard value then use it
        if (Enum.IsDefined(value.GetType(), value))
        {
            return GetValueText(culture, value);
        }

        // otherwise find the combination of flag bit values that makes up the value
        ulong lValue = Convert.ToUInt32(value);
        string? result = null;
        foreach (var flagValue in _flagValues!)
        {
            ulong lFlagValue = Convert.ToUInt32(flagValue);
            if (IsSingleBitValue(lFlagValue) && (lFlagValue & lValue) == lFlagValue)
            {
                var valueText = GetValueText(culture, flagValue!);
                result = result == null ? valueText : $"{result}, {valueText}";
            }
        }

        return result;
    }

    private LookupTable GetLookupTable(CultureInfo culture)
    {
        culture ??= CultureInfo.CurrentCulture;

        if (!_lookupTables.TryGetValue(culture, out var result))
        {
            result = new LookupTable();
            var sv = GetStandardValues();
            if (sv != null)
            {
                foreach (var value in sv)
                {
                    var text = GetValueText(culture, value!);
                    if (text != null)
                    {
                        result.Add(text, value!);
                    }
                }
            }

            _lookupTables.Add(culture, result);
        }

        return result;
    }

    /// <summary>
    /// Return the Enum value for a simple (non-flagged enum).
    /// </summary>
    /// <param name="culture">The culture to convert using.</param>
    /// <param name="text">The text to convert.</param>
    /// <returns>The enum value.</returns>
    private object? GetValue(CultureInfo culture, string text)
    {
        var lookupTable = GetLookupTable(culture);
        lookupTable.TryGetValue(text, out var result);
        return result;
    }

    /// <summary>
    /// Return the text to display for a simple value in the given culture.
    /// </summary>
    /// <param name="culture">The culture to get the text for.</param>
    /// <param name="value">The enum value to get the text for.</param>
    /// <returns>The localized text.</returns>
    private string GetValueText(CultureInfo culture, object value)
    {
        var resourceName = GetResourceName(value);
        return _resourceManager.GetString(resourceName, culture) ?? resourceName;
    }

    private class LookupTable : Dictionary<string, object>
    {
    }
}
