// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace CP.Localisation;

/// <summary>
/// Defines a base class for markup extensions which are managed by a central <see
/// cref="MarkupExtensionManager"/>. This allows the associated markup targets to be updated via
/// the manager.
/// </summary>
/// <remarks>
/// The ManagedMarkupExtension holds a weak reference to the target object to allow it to update
/// the target. A weak reference is used to avoid a circular dependency which would prevent the
/// target being garbage collected.
/// </remarks>
public abstract class ManagedMarkupExtension : MarkupExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedMarkupExtension"/> class.
    /// Create a new instance of the markup extension.
    /// </summary>
    /// <param name="manager">The manager.</param>
    protected ManagedMarkupExtension(MarkupExtensionManager manager) => manager?.RegisterExtension(this);

    /// <summary>
    /// Gets a value indicating whether is an associated target still alive ie not garbage collected.
    /// </summary>
    public bool IsTargetAlive
    {
        get
        {
            // for normal elements the _targetObjects.Count will always be 1 for templates the
            // Count may be zero if this method is called in the middle of window elaboration
            // after the template has been instantiated but before the elements that use it have
            // been. In this case return true so that we don't unhook the extension prematurely
            if (TargetObjects.Count == 0)
            {
                return true;
            }

            // otherwise just check whether the referenced target(s) are alive
            return TargetObjects.Any(reference => reference.IsAlive);
        }
    }

    /// <summary>
    /// Gets a value indicating whether returns true if a target attached to this extension is in design mode.
    /// </summary>
    internal bool IsInDesignMode => TargetObjects.Any(reference => reference.Target is DependencyObject element && DesignerProperties.GetIsInDesignMode(element));

    /// <summary>
    /// Gets return the target objects the extension is associated with.
    /// </summary>
    /// <remarks>
    /// For normal elements their will be a single target. For templates their may be zero or
    /// more targets.
    /// </remarks>
    protected List<WeakReference> TargetObjects { get; } = [];

    /// <summary>
    /// Gets return the Target Property the extension is associated with.
    /// </summary>
    /// <remarks>Can either be a <see cref="DependencyProperty"/> or <see cref="PropertyInfo"/>.</remarks>
    protected object? TargetProperty { get; private set; }

    /// <summary>
    /// Gets return the type of the Target Property.
    /// </summary>
    protected Type? TargetPropertyType
    {
        get
        {
            if (TargetProperty is DependencyProperty)
            {
                return (TargetProperty as DependencyProperty)?.PropertyType;
            }
            else if (TargetProperty is PropertyInfo)
            {
                return (TargetProperty as PropertyInfo)?.PropertyType;
            }
            else if (TargetProperty != null)
            {
                return TargetProperty.GetType();
            }

            return null;
        }
    }

    /// <summary>
    /// Is the given object the target for the extension.
    /// </summary>
    /// <param name="target">The target to check.</param>
    /// <returns>True if the object is one of the targets for this extension.</returns>
    public bool IsTarget(object target) => TargetObjects.Any(reference => reference.IsAlive && reference.Target == target);

    /// <summary>
    /// Return the value for this instance of the Markup Extension.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The value of the element.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        RegisterTarget(serviceProvider);
        object result = this;

        // when used in a template the _targetProperty may be null - in this case return this
        if (TargetProperty != null)
        {
            result = GetValue();
        }

        return result;
    }

    /// <summary>
    /// Update the associated targets.
    /// </summary>
    public void UpdateTargets()
    {
        foreach (var reference in TargetObjects)
        {
            if (reference.IsAlive)
            {
                UpdateTarget(reference.Target!);
            }
        }
    }

    /// <summary>
    /// Return the value associated with the key from the resource manager.
    /// </summary>
    /// <returns>The value from the resources if possible otherwise the default value.</returns>
    protected abstract object GetValue();

    /// <summary>
    /// Called by <see cref="ProvideValue(IServiceProvider)"/> to register the target and object
    /// using the extension.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    protected virtual void RegisterTarget(IServiceProvider serviceProvider)
    {
        var provideValueTarget = serviceProvider?.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        var target = provideValueTarget?.TargetObject;

        // Check if the target is a SharedDp which indicates the target is a template In this
        // case we don't register the target and ProvideValue returns this allowing the extension
        // to be evaluated for each instance of the template
        if (target != null && target.GetType().FullName != "System.Windows.SharedDp")
        {
            TargetProperty = provideValueTarget!.TargetProperty;
            TargetObjects.Add(new WeakReference(target));
        }
    }

    /// <summary>
    /// Called by <see cref="UpdateTargets"/> to update each target referenced by the extension.
    /// </summary>
    /// <param name="target">The target to update.</param>
    protected virtual void UpdateTarget(object target)
    {
        if (TargetProperty is DependencyProperty)
        {
            if (target is DependencyObject dependencyObject)
            {
                dependencyObject.SetValue(TargetProperty as DependencyProperty, GetValue());
            }
        }
        else if (TargetProperty is PropertyInfo)
        {
            (TargetProperty as PropertyInfo)?.SetValue(target, GetValue(), null);
        }
    }
}
