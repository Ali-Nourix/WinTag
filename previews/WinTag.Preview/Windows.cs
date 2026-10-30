using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WinTag.Preview;
internal sealed record LiveWindow(nint Handle, uint Pid, long Birth, Guid Generation, string Process, string Class, string Title, bool Remote);
internal sealed class Inventory : IDisposable
{
    private readonly Dispatcher _dispatcher;
    private readonly WINEVENTPROC _callback;
    private readonly HWINEVENTHOOK _hook;
    private readonly Dictionary<nint,LiveWindow> _windows = [];
    private readonly HashSet<nint> _pending = [];
    private bool _scheduled, _disposed;
    internal event Action? Changed;
    internal IReadOnlyCollection<LiveWindow> Windows => _windows.Values;
    internal Inventory(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        _callback = (_, evt, hwnd, obj, child, _, _) =>
        {
            if (_disposed || obj != 0 || child != 0) return;
            if (evt is not (3 or 0x8000 or 0x8001 or 0x8002 or 0x8003 or 0x800B or 0x800C or 0x8017 or 0x8018)) return;
            if (evt == 0x8001) _windows.Remove((nint)hwnd); // Retire generation before any queued action.
            _pending.Add((nint)hwnd);
            if (_scheduled) return;
            _scheduled = true;
            _dispatcher.BeginInvoke(Flush);
        };
        _hook = PInvoke.SetWinEventHook(1,0x8018,(HMODULE)(nint)0,_callback,0,0,0x0002);
        if (_hook.IsNull) throw new InvalidOperationException("Window event hook unavailable.");
        // Hook first, then snapshot; queued events reconcile the dictionary, never append duplicates.
        PInvoke.EnumWindows((h,_) => { Update((nint)h); return true; },0);
    }
    private void Flush()
    {
        _scheduled = false;
        if (_disposed) return;
        foreach (var h in _pending.ToArray()) Update(h);
        _pending.Clear(); Changed?.Invoke();
    }
    private void Update(nint handle)
    {
        var old = _windows.GetValueOrDefault(handle);
        var w = Read(handle,old?.Generation ?? Guid.NewGuid());
        if (w is null) _windows.Remove(handle);
        else
        {
            if (old is not null && (old.Pid != w.Pid || old.Birth != w.Birth)) w = w with { Generation = Guid.NewGuid() };
            _windows[handle] = w;
        }
    }
    private static unsafe LiveWindow? Read(nint handle, Guid generation)
    {
        try
        {
            var h=(HWND)handle;
            if (!PInvoke.IsWindow(h) || !PInvoke.IsWindowVisible(h)) return null;
            PInvoke.GetWindowThreadProcessId(h,out var pid);
            if (pid == Environment.ProcessId || pid == 0) return null;
            var style = (long)PInvoke.GetWindowLongPtr(h,WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
            if ((style & 0x80) != 0 || ((style & 0x40000)==0 && !PInvoke.GetWindow(h,GET_WINDOW_CMD.GW_OWNER).IsNull)) return null;
            uint cloak=0;
            if (PInvoke.DwmGetWindowAttribute(h,(DWMWINDOWATTRIBUTE)14,&cloak,4).Failed || (cloak & 1)!=0) return null;
            var title = new char[1024]; var length = PInvoke.GetWindowText(h,title);
            if (length == 0) return null;
            var cls = new char[256]; var clength = PInvoke.GetClassName(h,cls);
            var className = new string(cls,0,clength);
            if (className is "Progman" or "WorkerW" or "Shell_TrayWnd" or "Shell_SecondaryTrayWnd" or "Ghost") return null;
            using var process=Process.GetProcessById((int)pid);
            return new(handle,pid,process.StartTime.ToUniversalTime().Ticks,generation,process.ProcessName,className,new string(title,0,length),cloak!=0);
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception) { return null; }
    }
    internal bool Valid(LiveWindow w) => !_disposed && _windows.TryGetValue(w.Handle,out var current) &&
        current.Generation == w.Generation && Read(w.Handle,w.Generation) is { } now && now.Pid == w.Pid && now.Birth == w.Birth;
    internal bool Activate(LiveWindow w)
    {
        if (!Valid(w) || w.Remote) return false;
        if (PInvoke.IsIconic((HWND)w.Handle)) PInvoke.ShowWindowAsync((HWND)w.Handle,SHOW_WINDOW_CMD.SW_RESTORE);
        return PInvoke.SetForegroundWindow((HWND)w.Handle);
    }
    internal bool Minimize(LiveWindow w) => Valid(w) && !w.Remote && PInvoke.ShowWindowAsync((HWND)w.Handle,SHOW_WINDOW_CMD.SW_MINIMIZE);
    internal bool CloseNormally(LiveWindow w) => Valid(w) && !w.Remote && PInvoke.PostMessage((HWND)w.Handle,0x10,default,default);
    public void Dispose() { if (_disposed) return; _disposed=true; PInvoke.UnhookWinEvent(_hook); _windows.Clear(); GC.KeepAlive(_callback); }
}
internal sealed class EdgeTrigger : IDisposable
{
    private readonly HOOKPROC _callback;
    private readonly HHOOK _hook;
    private readonly DispatcherTimer _dwell;
    private readonly Action<System.Windows.Forms.Screen> _open;
    private readonly Func<Config> _config;
    private System.Windows.Forms.Screen? _candidate;
    private bool _disposed;
    private (System.Windows.Forms.Screen Screen, double Scale)[] _topology=[];
    private readonly EventHandler _displayChanged;
    internal bool Paused { get; set; }
    internal unsafe EdgeTrigger(Dispatcher dispatcher, Func<Config> config, Action<System.Windows.Forms.Screen> open)
    {
        _open=open; _config=config;
        void RefreshTopology()
        {
            _topology=System.Windows.Forms.Screen.AllScreens.Select(s=>
            {
                var b=s.Bounds;
                var monitor=PInvoke.MonitorFromPoint(new System.Drawing.Point(b.Left+1,b.Top+1),MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
                var ok=PInvoke.GetDpiForMonitor(monitor,Windows.Win32.UI.HiDpi.MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI,out var dx,out _).Succeeded;
                return (s,(ok?dx:96)/96d);
            }).ToArray();
        }
        RefreshTopology();
        _displayChanged=(_,_)=>dispatcher.BeginInvoke(()=>{if(!_disposed)RefreshTopology();});
        Microsoft.Win32.SystemEvents.DisplaySettingsChanged+=_displayChanged;
        _dwell=new DispatcherTimer(DispatcherPriority.Input,dispatcher); _dwell.Tick += (_,_) =>
        {
            _dwell.Stop();
            if (_candidate is not null && PInvoke.GetCursorPos(out var point) && Eligible(point.X,point.Y)?.DeviceName == _candidate.DeviceName)
                _open(_candidate);
        };
        _callback=(code,w,l)=>
        {
            if (code>=0 && w.Value==0x200 && !_disposed) // WM_MOUSEMOVE only; wheel never triggers.
            {
                var data=Marshal.PtrToStructure<MSLLHOOKSTRUCT>((nint)l.Value);
                var next=Eligible(data.pt.X,data.pt.Y);
                if (next?.DeviceName != _candidate?.DeviceName)
                {
                    _dwell.Stop(); _candidate=next;
                    if (next is not null)
                    {
                        var b=next.Bounds;
                        var shared=_topology.Any(s=>s.Screen.DeviceName!=next.DeviceName && s.Screen.Bounds.Contains(b.Right,data.pt.Y));
                        if (shared && _config().SharedDwellMs>0) { _dwell.Interval=TimeSpan.FromMilliseconds(_config().SharedDwellMs); _dwell.Start(); }
                        else dispatcher.BeginInvoke(()=> { if (_candidate?.DeviceName==next.DeviceName && !Paused) _open(next); });
                    }
                }
            }
            return PInvoke.CallNextHookEx(default,code,w,l);
        };
        _hook=PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_MOUSE_LL,_callback,(HINSTANCE)(nint)PInvoke.GetModuleHandle((PCWSTR)null),0);
        if (_hook.IsNull) throw new InvalidOperationException("Mouse hook unavailable.");
    }
    private System.Windows.Forms.Screen? Eligible(int x,int y)
    {
        if (Paused || _disposed) return null;
        foreach (var display in _topology)
        {
            var screen=display.Screen;
            var b=screen.Bounds;
            var scale=display.Scale;
            var c=_config();
            if (x<b.Right-Math.Ceiling(c.HotZoneWidthDip*scale) || x>=b.Right || y<b.Top+b.Height*(1-c.HotZoneHeightFraction)/2 || y>=b.Bottom-b.Height*(1-c.HotZoneHeightFraction)/2) continue;
            var fg=PInvoke.GetForegroundWindow();
            if (!fg.IsNull && !PInvoke.IsIconic(fg) && PInvoke.GetWindowRect(fg,out var r) && r.left<=b.Left && r.top<=b.Top && r.right>=b.Right && r.bottom>=b.Bottom) return null;
            return screen;
        }
        return null;
    }
    public void Dispose() { if (_disposed)return; _disposed=true; Microsoft.Win32.SystemEvents.DisplaySettingsChanged-=_displayChanged; _dwell.Stop(); PInvoke.UnhookWindowsHookEx(_hook); GC.KeepAlive(_callback); }
}
