using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WinTag.ColoringSpike;

public sealed record WindowChoice(nint Handle, uint ProcessId, string Title)
{
    public override string ToString() => $"{ProcessId} · {Title}";
}

internal static unsafe class Native
{
    internal const uint CaptionAttribute = 35, BorderAttribute = 34;
    internal static HWND H(nint handle) => (HWND)handle;

    internal static List<WindowChoice> Enumerate()
    {
        var found = new List<WindowChoice>();
        PInvoke.EnumWindows((hwnd, _) =>
        {
            if (!PInvoke.IsWindowVisible(hwnd)) return true;
            var ex = (long)PInvoke.GetWindowLongPtr(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
            if ((ex & 0x80) != 0) return true; // WS_EX_TOOLWINDOW
            PInvoke.GetWindowThreadProcessId(hwnd, out var pid);
            if (pid == Environment.ProcessId) return true;
            var title = new char[1024];
            var length = PInvoke.GetWindowText(hwnd, title);
            if (length > 0) found.Add(new((nint)hwnd, pid, new string(title, 0, length)));
            return true;
        }, 0);
        return found;
    }

    internal static void SetColor(nint hwnd, uint attribute, uint color)
    {
        var hr = PInvoke.DwmSetWindowAttribute(H(hwnd), (DWMWINDOWATTRIBUTE)attribute, &color, sizeof(uint));
        if (hr.Failed) throw new InvalidOperationException($"DWM set {attribute}: 0x{hr.Value:X8}");
    }

    internal static string ReadOnlyColorProbe(nint hwnd)
    {
        uint caption = 0, border = 0;
        var a = PInvoke.DwmGetWindowAttribute(H(hwnd), (DWMWINDOWATTRIBUTE)CaptionAttribute, &caption, sizeof(uint));
        var b = PInvoke.DwmGetWindowAttribute(H(hwnd), (DWMWINDOWATTRIBUTE)BorderAttribute, &border, sizeof(uint));
        return $"READ ONLY. Caption HRESULT=0x{a.Value:X8}, value=0x{caption:X8}; " +
            $"border HRESULT=0x{b.Value:X8}, value=0x{border:X8}. " +
            "Even success is not proof of exact original-state semantics. External DWM mutation remains DISABLED.";
    }

    internal static void ValidateLive(TargetSnapshot target)
    {
        target.Validate();
        var hwnd = H((nint)target.Hwnd);
        if (!PInvoke.IsWindow(hwnd)) throw new InvalidOperationException("Target window closed.");
        PInvoke.GetWindowThreadProcessId(hwnd, out var pid);
        if (pid != target.ProcessId) throw new InvalidOperationException("Target PID changed.");
        using var process = Process.GetProcessById(target.ProcessId);
        if (process.StartTime.ToUniversalTime().Ticks != target.ProcessBirthTicks ||
            ReadMarker((nint)hwnd, target.MarkerName) != 1)
            throw new InvalidOperationException("Target lease/instance marker is no longer valid.");
    }

    internal static void Restore(TargetSnapshot target)
    {
        ValidateLive(target);
        SetColor((nint)target.Hwnd, CaptionAttribute, target.CaptionColor);
        SetColor((nint)target.Hwnd, BorderAttribute, target.BorderColor);
    }

    internal static nint ReadMarker(nint hwnd, string name)
    {
        // Request raw HANDLE, never the generated owning SafeFileHandle convenience overload.
        fixed (char* chars = name) return (nint)PInvoke.GetProp(H(hwnd), new PCWSTR(chars));
    }
    internal static void Mark(nint hwnd, string name)
    {
        fixed (char* chars = name)
            if (!PInvoke.SetProp(H(hwnd), new PCWSTR(chars), (HANDLE)(nint)1))
                throw new InvalidOperationException("Could not mark disposable target.");
    }
    internal static void Unmark(nint hwnd, string name)
    {
        fixed (char* chars = name) PInvoke.RemoveProp(H(hwnd), new PCWSTR(chars));
    }

    internal static bool GetBounds(nint hwnd, out RECT rect) => PInvoke.GetWindowRect(H(hwnd), out rect);
    internal static bool IsUsable(nint hwnd)
    {
        uint cloak = 0;
        var hr = PInvoke.DwmGetWindowAttribute(H(hwnd), (DWMWINDOWATTRIBUTE)14, &cloak, sizeof(uint));
        return PInvoke.IsWindow(H(hwnd)) && PInvoke.IsWindowVisible(H(hwnd)) &&
            !PInvoke.IsIconic(H(hwnd)) && hr.Succeeded && cloak == 0 &&
            PInvoke.GetForegroundWindow() == H(hwnd);
    }
    internal static uint ProcessId(nint hwnd)
    {
        PInvoke.GetWindowThreadProcessId(H(hwnd), out var pid); return pid;
    }
    internal static void Activate(nint hwnd) => PInvoke.SetForegroundWindow(H(hwnd));
    internal static uint Dpi(nint hwnd) => Math.Max(96, PInvoke.GetDpiForWindow(H(hwnd)));

    internal static void MakeClickThrough(nint hwnd)
    {
        var current = (long)PInvoke.GetWindowLongPtr(H(hwnd), WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        // Native layered + transparent + toolwindow + no-activate. Still needs physical input tests.
        PInvoke.SetWindowLongPtr(H(hwnd), WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE,
            (nint)(current | 0x80000 | 0x20 | 0x80 | 0x08000000));
    }
    internal static void PlaceOverlay(nint hwnd, int x, int y, int width, int height) =>
        PInvoke.SetWindowPos(H(hwnd), (HWND)(nint)(-1), x, y, width, height,
            SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOOWNERZORDER);

    internal static HWINEVENTHOOK Hook(WINEVENTPROC callback) => PInvoke.SetWinEventHook(
        1, 0x8018, (HMODULE)(nint)0, callback, 0, 0,
        0x0002 /* WINEVENT_OUTOFCONTEXT (0) | WINEVENT_SKIPOWNPROCESS */);
    internal static void Unhook(HWINEVENTHOOK hook) => PInvoke.UnhookWinEvent(hook);
}
