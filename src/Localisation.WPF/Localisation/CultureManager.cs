// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CP.Properties;

namespace CP.Localisation;

/// <summary>
/// Provides the ability to change the UICulture for WPF Windows and controls dynamically.
/// </summary>
/// <remarks>
/// XAML elements that use the <see cref="ResxExtension"/> are automatically updated when the
/// <see cref="UICulture"/> property is changed.
/// </remarks>
public static class CultureManager
{
    private static readonly Subject<Unit> _uICultureChangedSubject = new();
    private static CultureSelectWindow? _cultureSelectWindow;
    private static NotifyIcon? _notifyIcon;
    private static IntPtr _notifyIconHandle;
    private static bool _synchronizeThreadCulture = true;
    private static CultureInfo _uiCulture = Thread.CurrentThread.CurrentUICulture;

    /// <summary>
    /// Raised when the <see cref="UICulture"/> is changed
    /// </summary>
    /// <remarks>
    /// Since this event is static if the client object does not detach from the event a
    /// reference will be maintained to the client object preventing it from being garbage
    /// collected - thus causing a potential memory leak.
    /// </remarks>
    public static event EventHandler? UICultureChanged;

    /// <summary>
    /// Gets or sets a value indicating whether if set to true then the <see cref="Thread.CurrentCulture"/> property is changed to match
    /// the current <see cref="UICulture"/>.
    /// </summary>
    public static bool SynchronizeThreadCulture
    {
        get => _synchronizeThreadCulture;

        set
        {
            _synchronizeThreadCulture = value;
            if (value)
            {
                SetThreadCulture(UICulture);
            }
        }
    }

    /// <summary>
    /// Gets or sets the UICulture for the WPF application and raises the <see cref="UICultureChanged"/>
    /// event causing any XAML elements using the <see cref="ResxExtension"/> to automatically update.
    /// </summary>
    public static CultureInfo UICulture
    {
        get => _uiCulture ??= Thread.CurrentThread.CurrentUICulture;

        set
        {
            if (value != UICulture)
            {
                _uiCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
                if (SynchronizeThreadCulture && value != null)
                {
                    SetThreadCulture(value);
                }

                UICultureExtension.UpdateAllTargets();
                ResxExtension.UpdateAllTargets();
                UICulture.SyncCultureInfo();
                UICultureChanged?.Invoke(null, EventArgs.Empty);
                _uICultureChangedSubject.OnNext(Unit.Default);
            }
        }
    }

    /// <summary>
    /// Gets the UI culture changed observer.
    /// </summary>
    /// <value>
    /// The UI culture changed observer.
    /// </value>
    public static IObservable<Unit> UICultureChangedObserver => _uICultureChangedSubject;

    /// <summary>
    /// Show the UICultureSelector to allow selection of the active UI culture.
    /// </summary>
    internal static void ShowCultureNotifyIcon()
    {
        if (_notifyIcon == null)
        {
            ToolStripMenuItem menuItem;

            _notifyIcon = new NotifyIcon
            {
                Icon = Resources.UICultureIcon
            };
            _notifyIcon.MouseClick += OnCultureNotifyIconMouseClick;
            _notifyIcon.MouseDoubleClick += OnCultureNotifyIconMouseDoubleClick;
            _notifyIcon.Text = Resources.UICultureSelectText;
            var menuStrip = new ContextMenuStrip();

            // separator
            using (var toolStripSeparator = new ToolStripSeparator())
            {
                // separator
                menuStrip.Items.Add(toolStripSeparator);
            }

            // add menu to open culture select window
            menuItem = new ToolStripMenuItem(Resources.OtherCulturesMenu);
            menuItem.Click += OnCultureSelectMenuClick;
            menuStrip.Items.Add(menuItem);

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
    }

    /// <summary>
    /// Add a menu item to the NotifyIcon for the current UICulture.
    /// </summary>
    /// <param name="culture">The culture.</param>
    private static void AddCultureMenuItem(CultureInfo culture)
    {
        if (!CultureMenuExists(culture) && _notifyIcon?.ContextMenuStrip is ContextMenuStrip menuStrip && menuStrip.Items.Count > 1)
        {
            var menuItem = new ToolStripMenuItem(culture.DisplayName)
            {
                Checked = true,
                CheckOnClick = true,
                Tag = culture
            };
            menuItem.CheckedChanged += OnCultureMenuCheckChanged;
            menuStrip.Items.Insert(menuStrip.Items.Count - 2, menuItem);
        }
    }

    /// <summary>
    /// Is there already an entry for the culture in the context menu.
    /// </summary>
    /// <param name="culture">The culture to check.</param>
    /// <returns>True if there is a menu.</returns>
    private static bool CultureMenuExists(CultureInfo culture)
    {
        foreach (ToolStripItem item in _notifyIcon!.ContextMenuStrip!.Items!)
        {
            if (item.Tag is CultureInfo itemCulture && itemCulture.Name == culture.Name)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Display the CultureSelectWindow to allow the user to select the UICulture.
    /// </summary>
    private static void DisplayCultureSelectWindow()
    {
        if (_cultureSelectWindow == null)
        {
            _cultureSelectWindow = new CultureSelectWindow
            {
                Title = _notifyIcon?.Text
            };
            _cultureSelectWindow.Closed += OnCultureSelectWindowClosed;
            _cultureSelectWindow.Show();
        }
    }

    /// <summary>
    /// Handle change of culture via the NotifyIcon menu.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private static void OnCultureMenuCheckChanged(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem menuItem && menuItem.Checked && menuItem.Tag is CultureInfo culture)
        {
            UICulture = culture;
        }
    }

    /// <summary>
    /// Display the context menu for left clicks (right clicks are handled automatically).
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
    private static void OnCultureNotifyIconMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            var methodInfo = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo?.Invoke(_notifyIcon, null);
        }
    }

    /// <summary>
    /// Display the CultureSelectWindow when the user double clicks on the NotifyIcon.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
    private static void OnCultureNotifyIconMouseDoubleClick(object? sender, MouseEventArgs e) => DisplayCultureSelectWindow();

    /// <summary>
    /// Display the CultureSelectWindow when the user selects the menu option.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private static void OnCultureSelectMenuClick(object? sender, EventArgs e) => DisplayCultureSelectWindow();

    /// <summary>
    /// Handle close of the culture select window.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private static void OnCultureSelectWindowClosed(object? sender, EventArgs e) => _cultureSelectWindow = null;

    /// <summary>
    /// Remove the culture notify icon when the designer process exits.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private static void OnDesignerExit(object? sender, EventArgs e)
    {
        // By the time the ProcessExit event is called the window associated with the notify icon
        // has been destroyed - and a bug in the NotifyIcon class means that the notify icon is
        // not removed. This works around the issue by saving the window handle when the
        // NotifyIcon is created and then calling the Shell_NotifyIcon method ourselves to remove
        // the icon from the tray
        if (_notifyIconHandle != IntPtr.Zero)
        {
            var iconData = new NativeMethods.NOTIFYICONDATA()
            {
                UCallbackMessage = 2048,
                UFlags = 1,
                HWnd = _notifyIconHandle,
                UID = 1,
                HIcon = IntPtr.Zero,
                SzTip = null
            };

            _ = NativeMethods.Shell_NotifyIcon(2, iconData);
        }
    }

    private static void OnMenuStripOpening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // ensure the current culture is always on the menu
        AddCultureMenuItem(UICulture);

        // Add the design time cultures
        foreach (var culture in ResxExtension.GetDesignTimeCultures())
        {
            AddCultureMenuItem(culture);
        }

        foreach (ToolStripItem item in _notifyIcon!.ContextMenuStrip!.Items!)
        {
            if (item is ToolStripMenuItem menuItem)
            {
                menuItem.Checked = menuItem.Tag == UICulture;
            }
        }
    }

    /// <summary>
    /// Set the thread culture to the given culture.
    /// </summary>
    /// <param name="value">The culture to set.</param>
    /// <remarks>If the culture is neutral then creates a specific culture.</remarks>
    private static void SetThreadCulture(CultureInfo value) =>
        Thread.CurrentThread.CurrentCulture = value.IsNeutralCulture ? CultureInfo.CreateSpecificCulture(value.Name) : value;
}
