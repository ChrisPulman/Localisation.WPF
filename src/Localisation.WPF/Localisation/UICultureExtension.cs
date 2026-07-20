// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows.Markup;

#if REACTIVE_SHIM
namespace CP.Localisation.Reactive;
#else
namespace CP.Localisation;
#endif

/// <summary>Dynamically sets a markup element's language to the current <see cref="CultureManager.UICulture"/> value.</summary>
/// <remarks>
/// The culture used for displaying data bound items is based on the Language property. This
/// extension allows you to dynamically change the language based on the current <see cref="CultureManager.UICulture"/>.
/// </remarks>
[MarkupExtensionReturnType(typeof(XmlLanguage))]
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class UICultureExtension : ManagedMarkupExtension
{
    private const int CleanupInterval = 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="UICultureExtension"/> class.
    /// Creates an instance of the extension to set the language property for an element to the
    /// current <see cref="CultureManager.UICulture"/> property value.
    /// </summary>
    public UICultureExtension()
        : base(MarkupManager)
    {
    }

    /// <summary>Gets return the MarkupManager for this extension.</summary>
    public static MarkupExtensionManager MarkupManager { get; } = new(CleanupInterval);

    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString() ?? GetType().Name;

    /// <summary>Use the Markup Manager to update all targets.</summary>
    public static void UpdateAllTargets() => MarkupManager.UpdateAllTargets();

    /// <summary>Return the <see cref="XmlLanguage"/> to use for the associated Markup element.</summary>
    /// <returns>
    /// The <see cref="XmlLanguage"/> corresponding to the current <see
    /// cref="CultureManager.UICulture"/> property value.
    /// </returns>
    protected override object GetValue() => XmlLanguage.GetLanguage(CultureManager.UICulture!.IetfLanguageTag);
}
