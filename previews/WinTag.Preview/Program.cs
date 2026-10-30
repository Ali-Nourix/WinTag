using System.Diagnostics;
using System.Security.Principal;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace WinTag.Preview;
internal static class Program
{
    [STAThread]
    private static int Main()
    {
        if(!OperatingSystem.IsWindowsVersionAtLeast(10,0,22000)){MessageBox.Show("Windows 11 x64 required.");return 1;}
        using var identity=WindowsIdentity.GetCurrent();
        if(new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator))
        {MessageBox.Show("WinTag را بدون Run as administrator اجرا کنید.");return 2;}
        using var mutex=new Mutex(true,"Local\\WinTag.Preview.0.2",out var owned);
        if(!owned){MessageBox.Show("WinTag Preview در حال اجراست؛ از آیکن کنار ساعت بازش کن.");return 0;}
        try
        {
            var application=new Application { ShutdownMode=ShutdownMode.OnExplicitShutdown };
            using var controller=new Controller();
            controller.Open();application.Run();return 0;
        }
        catch(Exception error){MessageBox.Show("WinTag Preview: "+error.Message);return 1;}
        finally {mutex.ReleaseMutex();}
    }
}
internal sealed class Controller : IDisposable
{
    internal Inventory Inventory { get; }
    internal Config Config { get; private set; }
    internal bool Paused { get; private set; }
    private readonly Dictionary<Guid,string[]> _manual=[];
    private readonly PanelWindow _panel;
    private readonly Badge _badge;
    private readonly EdgeTrigger _edge;
    private readonly Shortcuts _keys;
    private readonly System.Windows.Forms.NotifyIcon _tray;
    private bool _disposed;
    internal Controller()
    {
        Store.EnsureLocalFilePath(Store.FilePath);
        // Preserve invalid existing settings; never silently overwrite them with defaults.
        Config=File.Exists(Store.FilePath)?Store.Parse(File.ReadAllText(Store.FilePath)):new Config();
        Inventory=new Inventory(System.Windows.Threading.Dispatcher.CurrentDispatcher);
        _panel=new PanelWindow(this);
        _badge=new Badge();
        _keys=new Shortcuts(Shortcut);
        try {_keys.Apply(Config,()=> {if(!File.Exists(Store.FilePath))Store.Save(Config);});}
        catch(Exception e){MessageBox.Show("شورت‌کات‌ها فعال نشدند؛ برنامه با ماوس و آیکن کنار ساعت قابل استفاده است.\n"+e.Message);}
        _edge=new EdgeTrigger(System.Windows.Threading.Dispatcher.CurrentDispatcher,()=>Config,s=>{if(!Paused && !_panel.SuppressAutoHide)_panel.Open(s,false);});
        Inventory.Changed+=()=>
        {
            var live=Inventory.Windows.Select(w=>w.Generation).ToHashSet();
            foreach(var id in _manual.Keys.Where(k=>!live.Contains(k)).ToArray())_manual.Remove(id);
            _panel.Refresh();UpdateBadge();
        };
        var menu=new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("بازکردن WinTag",null,(_,_)=>Open());
        menu.Items.Add("تنظیمات و شورت‌کات‌ها",null,(_,_)=>Settings());
        menu.Items.Add("نشانگر بسیار کوچک عنوان (آزمایشی)",null,(_,_)=>ToggleBadge());
        menu.Items.Add("توقف / ادامه",null,(_,_)=>TogglePause());
        menu.Items.Add("خروج",null,(_,_)=>Quit());
        _tray=new System.Windows.Forms.NotifyIcon { Text="WinTag Preview · بدون تغییر رنگ پنجره‌های واقعی",Icon=System.Drawing.SystemIcons.Application,ContextMenuStrip=menu,Visible=true };
        _tray.DoubleClick+=(_,_)=>Open();
    }
    internal IEnumerable<LiveWindow> Eligible()=>Inventory.Windows.Where(w=>!Config.IgnoreProcesses.Contains(w.Process,StringComparer.OrdinalIgnoreCase));
    internal Tag[] TagsFor(LiveWindow w)
    {
        var ids=Search.Match(Config,w.Process,w.Class,w.Title,_manual.GetValueOrDefault(w.Generation));
        return ids.Select(id=>Config.Tags.FirstOrDefault(t=>t.Id==id)).Where(t=>t is not null).Cast<Tag>().ToArray();
    }
    private void UpdateBadge()
    {
        if(Paused || !Config.ExperimentalTitleBadge){_badge.Hide();return;}
        var foreground=(nint)Windows.Win32.PInvoke.GetForegroundWindow();
        var w=Eligible().FirstOrDefault(w=>w.Handle==foreground);
        _badge.Update(w,w is null?null:TagsFor(w).FirstOrDefault());
    }
    private void ToggleBadge()
    {
        try
        {
            var candidate=Store.Parse(JsonSerializer.Serialize(Config,Config.Json));
            candidate.ExperimentalTitleBadge=!candidate.ExperimentalTitleBadge;
            Store.Save(candidate);Config=candidate;UpdateBadge();
        }
        catch(Exception e){MessageBox.Show("تنظیمات قبلی حفظ شد.\n"+e.Message);}
    }
    internal void Open()=>_panel.Open(System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position),true);
    internal void EditTags(LiveWindow? target)
    {
        if(target is null || !Inventory.Valid(target)){_panel.Say("ابتدا یک پنجره انتخاب کن.");return;}
        var dialog=new Window { Title="تگ‌های پنجره",Width=340,Height=400,WindowStartupLocation=WindowStartupLocation.CenterScreen,FlowDirection=FlowDirection.RightToLeft,Owner=_panel };
        var stack=new StackPanel { Margin=new Thickness(16) };dialog.Content=stack;
        stack.Children.Add(new TextBlock { Text="ترتیب تگ‌های دستی؛ اولین تگ، اصلی است.",TextWrapping=TextWrapping.Wrap });
        var selected=TagsFor(target).Select(t=>t.Id).ToList();
        var boxes=new List<(Tag,CheckBox)>();
        foreach(var t in Config.Tags.OrderBy(t=>selected.Contains(t.Id)?selected.IndexOf(t.Id):int.MaxValue))
        {var box=new CheckBox {Content=t.Name,IsChecked=selected.Contains(t.Id),Margin=new Thickness(0,6,0,6)};boxes.Add((t,box));stack.Children.Add(box);}
        var order=new TextBox { Text=string.Join(",",selected),Margin=new Thickness(0,8,0,8) };
        stack.Children.Add(new TextBlock {Text="ترتیب اختیاری با شناسه‌ها، جداشده با ویرگول:"});stack.Children.Add(order);
        var save=new Button {Content="ذخیرهٔ دستی",Padding=new Thickness(8)};stack.Children.Add(save);
        save.Click+=(_,_)=>
        {
            if(!Inventory.Valid(target)){dialog.Close();return;}
            var ids=boxes.Where(x=>x.Item2.IsChecked==true).Select(x=>x.Item1.Id).ToArray();
            var requested=order.Text.Split(',',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries);
            var sorted=requested.Where(ids.Contains).Concat(ids).Distinct().ToArray();
            _manual[target.Generation]=sorted;dialog.Close();_panel.Refresh();UpdateBadge();UpdateBadge();
        };
        stack.Children.Add(new TextBlock {Text="در این پیش‌نمایش، انتخاب دستی فقط تا پایان همین اجرا نگه داشته می‌شود؛ اجرای دوبارهٔ قوانین آن را عوض نمی‌کند.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,8,0,0)});
        _panel.SuppressAutoHide=true;try {dialog.ShowDialog();}finally {_panel.SuppressAutoHide=false;}
    }
    internal void TagAction(LiveWindow selected,bool close)
    {
        var primary=TagsFor(selected).FirstOrDefault();if(primary is null)return;
        var members=Eligible().Where(w=>!w.Remote && TagsFor(w).Any(t=>t.Id==primary.Id)).ToArray();
        if(close && MessageBox.Show($"بستن عادی {members.Length} پنجرهٔ محلی با تگ «{primary.Name}»؟ درخواست ذخیرهٔ برنامه‌ها حفظ می‌شود.","WinTag",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;
        foreach(var member in members)if(close)Inventory.CloseNormally(member);else Inventory.Minimize(member);
    }
    internal void Automatic(LiveWindow w){_manual.Remove(w.Generation);_panel.Refresh();UpdateBadge();}
    private void Shortcut(Shortcut s)
    {
        if(Paused)return;
        if(s.Action=="Picker"){Open();return;}
        if(s.Action=="Gather")
        {Open();_panel.Say("Gather A غیرفعال است: انتقال بین دسکتاپ‌ها روی این بیلد ویندوز تأیید نشده؛ به حالت B تبدیل نمی‌شود.");return;}
        var members=Eligible().Where(w=>TagsFor(w).Any(t=>t.Id==s.TagId)).ToArray();
        var local=members.Where(w=>!w.Remote).ToArray();_panel.Hide();
        int refused=0;foreach(var w in local)if(!Inventory.Activate(w))refused++;
        if(local.Length==0 || refused>0 || members.Length!=local.Length)
        {Open();_panel.Say($"Preserve B: {local.Length} پنجرهٔ محلی؛ {members.Length-local.Length} روی دسکتاپ دیگر؛ {refused} درخواست فوکوس رد شد. هیچ پنجره‌ای جابه‌جا نشد.");}
    }
    internal void TogglePause()
    {
        Paused=!Paused;_edge.Paused=Paused;
        if(Paused){_keys.Suspend();_panel.Hide();_badge.Hide();}
        else try{_keys.Apply(Config,()=>{});}catch(Exception e){MessageBox.Show(e.Message);}
    }
    internal void Settings()
    {
        var dialog=new Window {Title="WinTag · تنظیمات JSON و شورت‌کات‌ها",Width=760,Height=650,Owner=_panel,WindowStartupLocation=WindowStartupLocation.CenterScreen};
        var root=new DockPanel {Margin=new Thickness(12)};dialog.Content=root;
        var help=new TextBlock {Text="تگ، رنگ، قانون، لیست نادیده و کلیدها قابل تغییرند. Modifiers: Alt=1, Ctrl=2, Shift=4. Key کد مجازی ده‌دهی است: Space=32، 1=49. Preserve=B؛ Gather=A فعلاً غیرفعال.\nتنظیمات جدا از نسخهٔ نهایی ذخیره می‌شوند: %AppData%\\WinTag\\Preview\\config.json",TextWrapping=TextWrapping.Wrap,FlowDirection=FlowDirection.RightToLeft};DockPanel.SetDock(help,Dock.Top);root.Children.Add(help);
        var text=new TextBox {Text=JsonSerializer.Serialize(Config,Config.Json),AcceptsReturn=true,AcceptsTab=true,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,FontFamily=new System.Windows.Media.FontFamily("Consolas"),FontSize=13};
        var bottom=new WrapPanel();DockPanel.SetDock(bottom,Dock.Bottom);root.Children.Add(bottom);
        void Add(string label,Action action){var b=new Button {Content=label,Margin=new Thickness(3),Padding=new Thickness(8)};b.Click+=(_,_)=>{try{action();}catch(Exception e){MessageBox.Show("تنظیمات قبلی حفظ شد.\n"+e.Message);}};bottom.Children.Add(b);}
        Add("ذخیره و اعمال",()=> {var candidate=Store.Parse(text.Text);if(Paused)Store.Save(candidate);else _keys.Apply(candidate,()=>Store.Save(candidate));Config=candidate;_panel.Refresh();UpdateBadge();dialog.Close();});
        Add("خواندن فایل محلی",()=>text.Text=File.ReadAllText(Store.FilePath));
        Add("ورود JSON",()=>{var d=new Microsoft.Win32.OpenFileDialog{Filter="JSON|*.json"};if(d.ShowDialog(dialog)==true) {Store.EnsureLocalFilePath(d.FileName);if(new FileInfo(d.FileName).Length>1024*1024)throw new InvalidDataException("File too large.");var c=Store.Parse(File.ReadAllText(d.FileName));text.Text=JsonSerializer.Serialize(c,Config.Json);}});
        Add("خروجی JSON",()=> {var c=Store.Parse(text.Text);var d=new Microsoft.Win32.SaveFileDialog{Filter="JSON|*.json",FileName="WinTag-preview-settings.json"};if(d.ShowDialog(dialog)==true){Store.EnsureLocalFilePath(d.FileName);File.WriteAllText(d.FileName,JsonSerializer.Serialize(c,Config.Json));}});
        root.Children.Add(text);_panel.SuppressAutoHide=true;try {dialog.ShowDialog();}finally {_panel.SuppressAutoHide=false;}
    }
    internal void Quit(){Dispose();Application.Current.Shutdown();}
    public void Dispose(){if(_disposed)return;_disposed=true;_edge.Dispose();_keys.Dispose();Inventory.Dispose();_tray.Visible=false;_tray.Dispose();_badge.Close();_panel.Destroy();}
}
