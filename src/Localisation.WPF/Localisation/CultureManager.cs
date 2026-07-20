// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

#if REACTIVE_SHIM
namespace CP.Localisation.Reactive;
#else
namespace CP.Localisation;
#endif

/// <summary>Provides the ability to change the UICulture for WPF Windows and controls dynamically.</summary>
/// <remarks>
/// XAML elements that use the <see cref="ResxExtension"/> are automatically updated when the
/// <see cref="UICulture"/> property is changed.
/// </remarks>
public static class CultureManager
{
    private const int CultureMenuInsertionOffset = 2;

    private const int DeleteNotifyIconMessage = 2;

    private const int NotifyIconCallbackMessage = 2048;

    private const int NotifyIconIdentifier = 1;

    private const int NotifyIconMessageFlag = 1;

    /// <summary>Publishes notifications after the current UI culture changes.</summary>
    private static readonly Signal<RxVoid> _uiCultureChangedSignal = new();

    /// <summary>Stores the culture selection window while it is open.</summary>
    private static CultureSelectWindow? _cultureSelectWindow;

    /// <summary>Stores the notification-area icon used by the designer.</summary>
    private static NotifyIcon? _notifyIcon;

    /// <summary>Stores the native window handle associated with the notification icon.</summary>
    private static IntPtr _notifyIconHandle;

    /// <summary>Controls whether the current culture follows the selected UI culture.</summary>
    private static bool _synchronizeThreadCulture = true;

    /// <summary>Stores the selected UI culture.</summary>
    private static CultureInfo _uiCulture = Thread.CurrentThread.CurrentUICulture;

    /// <summary>Raised when the <see cref="UICulture"/> is changed</summary>
    /// <remarks>
    /// Since this event is static if the client object does not detach from the event a
    /// reference will be maintained to the client object preventing it from being garbage
    /// collected - thus causing a potential memory leak.
    /// </remarks>
    public static event EventHandler? UICultureChanged;

    /// <summary>Gets or sets whether the current thread culture changes to match the <see cref="UICulture"/>.</summary>
    public static bool SynchronizeThreadCulture
    {
        get => _synchronizeThreadCulture;

        set
        {
            _synchronizeThreadCulture = value;
            if (!value)
            {
                return;
            }

            SetThreadCulture(UICulture);

            // Ensure new threads also inherit the current culture
            CultureInfo.DefaultThreadCurrentCulture = Thread.CurrentThread.CurrentCulture;
        }
    }

    /// <summary>
    /// Gets or sets the UICulture for the WPF application and raises the <see cref="UICultureChanged"/>
    /// event causing any XAML elements using the <see cref="ResxExtension"/> to automatically update.
    /// </summary>
    public static CultureInfo UICulture
    {
        get => _uiCulture;

        set
        {
            if (value is null)
            {
                return;
            }

            // Compare by culture name to avoid redundant updates when equivalent instances are provided
            if (string.Equals(_uiCulture?.Name, value.Name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            ApplyUICulture(value);
        }
    }

    /// <summary>Gets the UI culture changed observer.</summary>
    /// <value>
    /// The UI culture changed observer.
    /// </value>
    public static IObservable<RxVoid> UICultureChangedObserver => _uiCultureChangedSignal;

    /// <summary>Show the UICultureSelector to allow selection of the active UI culture.</summary>
    internal static void ShowCultureNotifyIcon()
    {
        if (_notifyIcon is not null)
        {
            return;
        }

        ToolStripMenuItem menuItem;

        _notifyIcon = new NotifyIcon
        {
            Icon = Resources.UICultureIcon
        };
        _notifyIcon.MouseClick += OnCultureNotifyIconMouseClick;
        _notifyIcon.MouseDoubleClick += OnCultureNotifyIconMouseDoubleClick;
        _notifyIcon.Text = Resources.UICultureSelectText;
        var menuStrip = new ContextMenuStrip();

        // separator (do not dispose immediately to avoid ObjectDisposed issues)
        var toolStripSeparator = new ToolStripSeparator();
        _ = menuStrip.Items.Add(toolStripSeparator);

        // add menu to open culture select window
        menuItem = new(Resources.OtherCulturesMenu);
        menuItem.Click += OnCultureSelectMenuClick;
        _ = menuStrip.Items.Add(menuItem);

        menuStrip.Opening += OnMenuStripOpening;
        _notifyIcon.ContextMenuStrip = menuStrip;
        _notifyIcon.Visible = true;

        // Save the window handle associated with the notify icon - note that the window is
        // destroyed before the ProcessExit event gets called so calling NotifyIcon.Dispose
        // within the ProcessExit event handler doesn't work because the window handle has
        // been set to zero by that stage
        var fieldInfo = typeof(NotifyIcon).GetField("window", BindingFlags.Instance | BindingFlags.NonPublic);
        if (fieldInfo?.GetValue(_notifyIcon) is NativeWindow iconWindow)
        {
            _notifyIconHandle = iconWindow.Handle;
        }

        AppDomain.CurrentDomain.ProcessExit += OnDesignerExit;
    }

    /// <summary>Add a menu item to the NotifyIcon for the current UICulture.</summary>
    /// <param name="culture">The culture.</param>
    private static void AddCultureMenuItem(CultureInfo culture)
    {
        if (CultureMenuExists(culture) || _notifyIcon?.ContextMenuStrip is not ContextMenuStrip menuStrip || menuStrip.Items.Count <= 1)
        {
            return;
        }

        var menuItem = new ToolStripMenuItem(culture.DisplayName)
        {
            Checked = true,
            CheckOnClick = true,
            Tag = culture
        };
        menuItem.CheckedChanged += OnCultureMenuCheckChanged;
        menuStrip.Items.Insert(menuStrip.Items.Count - CultureMenuInsertionOffset, menuItem);
    }

    /// <summary>Is there already an entry for the culture in the context menu.</summary>
    /// <param name="culture">The culture to check.</param>
    /// <returns>True if there is a menu.</returns>
    private static bool CultureMenuExists(CultureInfo culture)
    {
        if (_notifyIcon?.ContextMenuStrip is null)
        {
            return false;
        }

        foreach (ToolStripItem item in _notifyIcon.ContextMenuStrip.Items)
        {
            if (item.Tag is CultureInfo itemCulture && string.Equals(itemCulture.Name, culture.Name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Display the CultureSelectWindow to allow the user to select the UICulture.</summary>
    private static void DisplayCultureSelectWindow()
    {
        if (_cultureSelectWindow is not null)
        {
            return;
        }

        _cultureSelectWindow = new CultureSelectWindow
        {
            Title = _notifyIcon?.Text
        };
        _cultureSelectWindow.Closed += OnCultureSelectWindowClosed;
        _cultureSelectWindow.Show();
    }

    /// <summary>Handle change of culture via the NotifyIcon menu.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private static void OnCultureMenuCheckChanged(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem menuItem || !menuItem.Checked || menuItem.Tag is not CultureInfo culture)
        {
            return;
        }

        UICulture = culture;
    }

    /// <summary>Display the context menu for left clicks (right clicks are handled automatically).</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
    private static void OnCultureNotifyIconMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        var methodInfo = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
        methodInfo?.Invoke(_notifyIcon, null);
    }

    /// <summary>Display the CultureSelectWindow when the user double clicks on the NotifyIcon.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
    private static void OnCultureNotifyIconMouseDoubleClick(object? sender, MouseEventArgs e) => DisplayCultureSelectWindow();

    /// <summary>Display the CultureSelectWindow when the user selects the menu option.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private static void OnCultureSelectMenuClick(object? sender, EventArgs e) => DisplayCultureSelectWindow();

    /// <summary>Handle close of the culture select window.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private static void OnCultureSelectWindowClosed(object? sender, EventArgs e) => _cultureSelectWindow = null;

    /// <summary>Remove the culture notify icon when the designer process exits.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private static void OnDesignerExit(object? sender, EventArgs e)
    {
        // By the time the ProcessExit event is called the window associated with the notify icon
        // has been destroyed - and a bug in the NotifyIcon class means that the notify icon is
        // not removed. This works around the issue by saving the window handle when the
        // NotifyIcon is created and then calling the Shell_NotifyIcon method ourselves to remove
        // the icon from the tray
        if (_notifyIconHandle == IntPtr.Zero)
        {
            return;
        }

        var iconData = new NativeMethods.NotifyIconData(
            _notifyIconHandle,
            NotifyIconIdentifier,
            NotifyIconMessageFlag,
            NotifyIconCallbackMessage);

        _ = NativeMethods.ShellNotifyIcon(DeleteNotifyIconMessage, in iconData);
    }

    /// <summary>Updates the notification icon menu immediately before it opens.</summary>
    /// <param name="sender">The menu that is opening.</param>
    /// <param name="e">The opening event data.</param>
    private static void OnMenuStripOpening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // ensure the current culture is always on the menu
        AddCultureMenuItem(UICulture);

        // Add the design time cultures
        foreach (var culture in ResxExtension.GetDesignTimeCultures())
        {
            AddCultureMenuItem(culture);
        }

        if (_notifyIcon?.ContextMenuStrip is null)
        {
            return;
        }

        foreach (ToolStripItem item in _notifyIcon.ContextMenuStrip.Items)
        {
            if (item is ToolStripMenuItem menuItem)
            {
                menuItem.Checked = menuItem.Tag == UICulture;
            }
        }
    }

    /// <summary>Set the thread culture to the given culture.</summary>
    /// <param name="value">The culture to set.</param>
    /// <remarks>If the culture is neutral then creates a specific culture.</remarks>
    private static void SetThreadCulture(CultureInfo value)
    {
        Thread.CurrentThread.CurrentCulture = value.IsNeutralCulture ? CultureInfo.CreateSpecificCulture(value.Name) : value;
    }

    /// <summary>Applies a UI culture and notifies all localisation targets.</summary>
    /// <param name="value">The UI culture to apply.</param>
    private static void ApplyUICulture(CultureInfo? value)
    {
        if (value is null)
        {
            return;
        }

        _uiCulture = value;

        // Set current thread UI culture
        Thread.CurrentThread.CurrentUICulture = value;

        // Ensure new threads inherit the updated UI culture
        CultureInfo.DefaultThreadCurrentUICulture = value;

        // Optionally synchronize the CurrentCulture
        if (SynchronizeThreadCulture)
        {
            SetThreadCulture(value);
            CultureInfo.DefaultThreadCurrentCulture = Thread.CurrentThread.CurrentCulture;
        }

        // Apply updates on the WPF UI thread to avoid cross-thread access to DependencyObjects
        static void ApplyUpdates()
        {
            UICultureExtension.UpdateAllTargets();
            ResxExtension.UpdateAllTargets();
            UICultureChanged?.Invoke(null, EventArgs.Empty);
            _uiCultureChangedSignal.OnNext(RxVoid.Default);
        }

        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is not null)
        {
            if (dispatcher.CheckAccess())
            {
                ApplyUpdates();
            }
            else
            {
                dispatcher.Invoke(ApplyUpdates);
            }
        }
        else
        {
            // No WPF Application (e.g., during design-time host fallback)
            ApplyUpdates();
        }
    }
}
