using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32.UI.Accessibility;

namespace WinTag.ColoringSpike;

internal sealed class TagOverlay : Window, IDisposable
{
    private readonly nint _target;
    private readonly WINEVENTPROC _callback; // root until unhooked
    private Windows.Win32.UI.Accessibility.HWINEVENTHOOK _hook;
    private bool _disposed;
    private bool _targetDestroyed;
    private readonly uint _targetPid;
    private readonly long _targetBirth;
    private nint _hwnd;
    private readonly double _widthDip;

    internal TagOverlay(nint target, string tag)
    {
        _target = target;
        var choice = Native.Enumerate().FirstOrDefault(w => w.Handle == target)
            ?? throw new InvalidOperationException("Target is no longer eligible.");
        _targetPid = choice.ProcessId;
        using var targetProcess = System.Diagnostics.Process.GetProcessById((int)_targetPid);
        _targetBirth = targetProcess.StartTime.ToUniversalTime().Ticks;
        WindowStyle = WindowStyle.None; AllowsTransparency = true; Background = Brushes.Transparent;
        ShowInTaskbar = false; ShowActivated = false; ResizeMode = ResizeMode.NoResize;
        Focusable = false; Height = OverlayAppearance.HeightDip;
        var label = new TextBlock
        {
            Text = OverlayAppearance.CompactLabel(tag), Foreground = new SolidColorBrush(Color.FromRgb(232, 235, 232)),
            FontSize = 10.5, FlowDirection = FlowDirection.RightToLeft,
            TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center
        };
        System.Windows.Automation.AutomationProperties.SetName(this, tag);
        label.Measure(new Size(double.PositiveInfinity, OverlayAppearance.HeightDip));
        _widthDip = Math.Clamp(label.DesiredSize.Width + 14, 32, OverlayAppearance.MaxWidthDip);
        Width = _widthDip;
        Content = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(215, 34, 37, 35)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(112, 154, 132)),
            BorderThickness = new Thickness(0, 0, 2, 0),
            CornerRadius = new CornerRadius(3), Padding = new Thickness(5, 0, 5, 0),
            Child = label
        };
        SourceInitialized += (_, _) =>
        {
            _hwnd = new WindowInteropHelper(this).Handle;
            Native.MakeClickThrough(_hwnd);
            HwndSource.FromHwnd(_hwnd)?.AddHook((nint h, int msg, nint w, nint l, ref bool handled) =>
            {
                if (msg == 0x84) { handled = true; return (nint)(-1); } // HTTRANSPARENT
                if (msg == 0x21) { handled = true; return (nint)3; } // MA_NOACTIVATE
                return 0;
            });
        };
        _callback = (_, eventId, hwnd, objectId, _, _, _) =>
        {
            if (_disposed) return;
            if ((nint)hwnd == _target && eventId == 0x8001 && objectId == 0)
                _targetDestroyed = true; // invalidate before queued geometry work; HWND reuse is not a new target
            if (((nint)hwnd == _target && objectId == 0) || eventId == 3)
                Dispatcher.BeginInvoke(UpdatePosition);
        };
        _hook = Native.Hook(_callback);
        if (_hook.IsNull) throw new InvalidOperationException("WinEvent hook failed; overlay cannot track safely.");
        new WindowInteropHelper(this).EnsureHandle();
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (_disposed) return;
        if (_targetDestroyed || !SameProcessInstance()) { Dispose(); return; }
        if (!Native.IsUsable(_target) || !Native.GetBounds(_target, out var rect)) { Hide(); return; }
        var scale = Native.Dpi(_target) / 96.0;
        var width = (int)Math.Min(_widthDip * scale, (rect.right - rect.left) * .20);
        if (width < 30 * scale) { Hide(); return; }
        // Deliberately trial-only centered placement: custom tabs/caption buttons require manual audit.
        var x = rect.left + ((rect.right - rect.left) - width) / 2;
        var y = rect.top + (int)(6 * scale);
        if (!IsVisible) Show();
        Native.PlaceOverlay(_hwnd, x, y, width, (int)(OverlayAppearance.HeightDip * scale));
    }

    private bool SameProcessInstance()
    {
        try
        {
            using var process = System.Diagnostics.Process.GetProcessById((int)_targetPid);
            return !process.HasExited && process.StartTime.ToUniversalTime().Ticks == _targetBirth
                && Native.ProcessId(_target) == _targetPid;
        }
        catch (ArgumentException) { return false; }
        catch (System.ComponentModel.Win32Exception) { return false; }
        catch (InvalidOperationException) { return false; }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Native.Unhook(_hook);
        Close();
        GC.KeepAlive(_callback);
    }
}
