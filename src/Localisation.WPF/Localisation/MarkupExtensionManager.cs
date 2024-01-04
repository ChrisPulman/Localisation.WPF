// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace CP.Localisation;

/// <summary>
/// Defines a class for managing <see cref="ManagedMarkupExtension"/> objects.
/// </summary>
/// <remarks>
/// This class provides a single point for updating all markup targets that use the given Markup
/// Extension managed by this class.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="MarkupExtensionManager"/> class.
/// Create a new instance of the manager.
/// </remarks>
/// <param name="cleanupInterval">
/// The interval at which to cleanup and remove extensions associated with garbage collected
/// targets. This specifies the number of new Markup Extensions that are created before a
/// cleanup is triggered.
/// </param>
public class MarkupExtensionManager(int cleanupInterval)
{
    /// <summary>
    /// The number of extensions added since the last cleanup.
    /// </summary>
    private int _cleanupCount;

    /// <summary>
    /// Gets return a list of the currently active extensions.
    /// </summary>
    public List<ManagedMarkupExtension> ActiveExtensions { get; private set; } = [];

    /// <summary>
    /// Cleanup references to extensions for targets which have been garbage collected.
    /// </summary>
    /// <remarks>
    /// This method is called periodically as new <see cref="ManagedMarkupExtension"/> objects
    /// are registered to release <see cref="ManagedMarkupExtension"/> objects which are no
    /// longer required (because their target has been garbage collected). This method does not
    /// need to be called externally, however it can be useful to call it prior to calling
    /// GC.Collect to verify that objects are being garbage collected correctly.
    /// </remarks>
    public void CleanupInactiveExtensions()
    {
        var newExtensions = new List<ManagedMarkupExtension>(ActiveExtensions.Count);
        foreach (var ext in ActiveExtensions)
        {
            if (ext.IsTargetAlive)
            {
                newExtensions.Add(ext);
            }
        }

        ActiveExtensions = newExtensions;
    }

    /// <summary>
    /// Force the update of all active targets that use the markup extension.
    /// </summary>
    public virtual void UpdateAllTargets()
    {
        // copy the list of active targets to avoid possible errors if the list is changed while enumerating
        foreach (var extension in new List<ManagedMarkupExtension>(ActiveExtensions))
        {
            extension.UpdateTargets();
        }
    }

    /// <summary>
    /// Register a new extension and remove extensions which reference target objects that have
    /// been garbage collected.
    /// </summary>
    /// <param name="extension">The extension to register.</param>
    internal void RegisterExtension(ManagedMarkupExtension extension)
    {
        // Cleanup extensions for target objects which have been garbage collected for
        // performance only do this periodically
        if (_cleanupCount > cleanupInterval)
        {
            CleanupInactiveExtensions();
            _cleanupCount = 0;
        }

        ActiveExtensions.Add(extension);
        _cleanupCount++;
    }
}
