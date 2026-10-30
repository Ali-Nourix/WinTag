using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace WinTag.Preview;
internal sealed class Shortcuts : IDisposable
{
    private readonly HwndSource _source;
    private readonly Action<Shortcut> _action;
    private Dictionary<(uint,uint),(int,Shortcut)> _active=[];
    private int _next=10;
    internal Shortcuts(Action<Shortcut> action)
    {
        _action=action;
        _source=new HwndSource(new HwndSourceParameters("WinTag.Preview.Messages") { Width=0,Height=0,WindowStyle=0 });
        _source.AddHook(Handle);
    }
    private nint Handle(nint h,int msg,nint w,nint l,ref bool handled)
    {
        if (msg==0x312)
        {
            var binding=_active.Values.FirstOrDefault(x=>x.Item1==(int)w);
            if (binding.Item2 is not null) _action(binding.Item2);
            handled=true;
        }
        return 0;
    }
    internal void Apply(Config candidate, Action persist)
    {
        candidate.Validate();
        var proposed=new Dictionary<(uint,uint),(int,Shortcut)>();
        var staged=new List<int>();
        try
        {
            foreach (var key in candidate.Hotkeys)
            {
                var chord=(key.Modifiers,key.Key);
                var id=_active.TryGetValue(chord,out var old)?old.Item1:_next++;
                if (!_active.ContainsKey(chord))
                {
                    if (!PInvoke.RegisterHotKey((HWND)_source.Handle,id,(Windows.Win32.UI.Input.KeyboardAndMouse.HOT_KEY_MODIFIERS)(key.Modifiers|0x4000),key.Key))
                        throw new InvalidOperationException("Shortcut already used by Windows or another application: "+key.Modifiers+" / "+key.Key);
                    staged.Add(id);
                }
                proposed[chord]=(id,key);
            }
            persist(); // No existing chord is released until persistence succeeds.
        }
        catch { foreach(var id in staged) PInvoke.UnregisterHotKey((HWND)_source.Handle,id); throw; }
        foreach (var old in _active.Where(p=>!proposed.ContainsKey(p.Key))) PInvoke.UnregisterHotKey((HWND)_source.Handle,old.Value.Item1);
        _active=proposed;
    }
    internal void Suspend() { foreach(var entry in _active.Values) PInvoke.UnregisterHotKey((HWND)_source.Handle,entry.Item1); _active.Clear(); }
    public void Dispose() { Suspend(); _source.Dispose(); }
}
