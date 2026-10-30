using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WinTag.Preview;
internal sealed class PanelWindow : Window
{
    private readonly Controller _app;
    private readonly TextBox _search=new() { Margin=new Thickness(0,8,0,8),Height=32,FontSize=14 };
    private readonly ListBox _list=new() { Background=Brushes.Transparent,BorderThickness=new Thickness(0),HorizontalContentAlignment=HorizontalAlignment.Stretch };
    private readonly TextBlock _status=new() { TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,8,0,4),Foreground=Brushes.LightGray,FontSize=11 };
    private readonly CheckBox _group=new() { Content="گروه‌بندی با تگ اصلی",Margin=new Thickness(0,4,0,6) };
    private bool _allowClose, _pointerEntered, _contextOpen;
    internal bool SuppressAutoHide { get; set; }
    internal PanelWindow(Controller app)
    {
        _app=app; Title="WinTag Preview 0.2"; Width=370; Height=620;
        WindowStyle=WindowStyle.None; ResizeMode=ResizeMode.NoResize; ShowInTaskbar=false;
        Background=new SolidColorBrush(Color.FromRgb(29,32,34)); Foreground=Brushes.WhiteSmoke; Topmost=true;
        FlowDirection=FlowDirection.RightToLeft;
        var root=new DockPanel { Margin=new Thickness(14) }; Content=root;
        var top=new StackPanel(); DockPanel.SetDock(top,Dock.Top); root.Children.Add(top);
        top.Children.Add(new TextBlock { Text="WinTag · پیش‌نمایش",FontSize=16 });
        top.Children.Add(new TextBlock { Text="جست‌وجوی عنوان و تگ…",Foreground=Brushes.Gray,FontSize=11 });
        top.Children.Add(_search); top.Children.Add(_group);
        var buttons=new WrapPanel(); top.Children.Add(buttons);
        Add(buttons,"تگ‌ها",()=>_app.EditTags(Selected));
        Add(buttons,"تنظیمات / شورت‌کات",_app.Settings);
        Add(buttons,"توقف / ادامه",_app.TogglePause);
        Add(buttons,"خروج",_app.Quit);
        DockPanel.SetDock(_status,Dock.Bottom); root.Children.Add(_status);root.Children.Add(_list);
        _search.TextChanged+=(_,_)=>Refresh(); _group.Checked+=(_,_)=>Refresh(); _group.Unchecked+=(_,_)=>Refresh();
        _list.MouseDoubleClick+=(_,_)=>ActivateSelected();
        PreviewKeyDown+=(_,e)=>
        {
            if(e.Key==Key.Escape){Hide();e.Handled=true;}
            else if(e.Key==Key.Enter){ActivateSelected();e.Handled=true;}
            else if(e.Key==Key.Down && _search.IsKeyboardFocusWithin){_list.Focus();_list.SelectedIndex=Math.Min(_list.Items.Count-1,0);e.Handled=true;}
            else if(e.Key==Key.F2){_app.EditTags(Selected);e.Handled=true;}
        };
        MouseEnter+=(_,_)=>_pointerEntered=true;
        MouseLeave+=(_,_)=> { if(!SuppressAutoHide && !_contextOpen && _pointerEntered && !IsKeyboardFocusWithin)Hide(); };
        Deactivated+=(_,_)=> { if(!SuppressAutoHide && !_contextOpen && !IsMouseOver) Hide(); };
        Closing+=(_,e)=> { if(!_allowClose){e.Cancel=true;Hide();} };
    }
    private LiveWindow? Selected => (_list.SelectedItem as ListBoxItem)?.Tag as LiveWindow;
    private static void Add(Panel parent,string label,Action action)
    {
        var button=new Button { Content=label,Padding=new Thickness(7,4,7,4),Margin=new Thickness(2) };
        button.Click+=(_,_)=>action();parent.Children.Add(button);
    }
    internal void Say(string text)=>_status.Text=text;
    private void ActivateSelected()
    {
        var w=Selected; if(w is null)return;
        Hide();
        if(!_app.Inventory.Activate(w)) { Show(); Say("این پنجره بسته است، روی دسکتاپ دیگر است یا ویندوز اجازهٔ فوکوس نداد."); }
    }
    internal void Refresh()
    {
        if(!IsVisible)return;
        var selected=Selected?.Generation;
        _list.Items.Clear();
        var data=_app.Eligible().Select(w=>(Window:w,Tags:_app.TagsFor(w))).Where(x=>Search.Contains(x.Window.Title+" "+string.Join(" ",x.Tags.Select(t=>t.Name)),_search.Text));
        if(_group.IsChecked==true)data=data.OrderBy(x=>x.Tags.FirstOrDefault()?.Name??"بدون تگ").ThenBy(x=>x.Window.Title);
        string? previous=null;
        foreach(var item in data)
        {
            var group=item.Tags.FirstOrDefault()?.Name??"بدون تگ";
            if(_group.IsChecked==true && previous!=group)
            { _list.Items.Add(new ListBoxItem { Content=group,IsEnabled=false,Foreground=Brushes.Gray }); previous=group; }
            var content=new StackPanel { Margin=new Thickness(2,5,2,5) };
            content.Children.Add(new TextBlock { Text=item.Window.Title,TextTrimming=TextTrimming.CharacterEllipsis,MaxWidth=300,ToolTip=item.Window.Title });
            var chips=new WrapPanel();content.Children.Add(chips);
            foreach(var tag in item.Tags)
                chips.Children.Add(new Border { Background=(Brush)new BrushConverter().ConvertFromString(tag.Color)!,CornerRadius=new CornerRadius(3),Padding=new Thickness(5,1,5,1),Margin=new Thickness(0,4,4,0),
                    Child=new TextBlock { Text=tag.Name,FontSize=11,Foreground=Brushes.Black } });
            content.Children.Add(new TextBlock { Text=item.Window.Process+(item.Window.Remote?" · دسکتاپ دیگر (فقط نمایش)":""),FontSize=10,Foreground=Brushes.Gray });
            var row=new ListBoxItem { Content=content,Tag=item.Window,Foreground=Brushes.WhiteSmoke,HorizontalContentAlignment=HorizontalAlignment.Stretch };
            var menu=new ContextMenu();
            menu.Opened+=(_,_)=>_contextOpen=true;menu.Closed+=(_,_)=>_contextOpen=false;
            void Action(string name,System.Action action){var m=new MenuItem { Header=name };m.Click+=(_,_)=>action();menu.Items.Add(m);}
            var captured=item.Window;
            Action("ویرایش تگ‌ها · F2",()=>_app.EditTags(captured));
            Action("کوچک‌کردن همهٔ پنجره‌های تگ اصلی",()=>_app.TagAction(captured,false));
            Action("بستن عادی همهٔ پنجره‌های تگ اصلی…",()=>_app.TagAction(captured,true));
            Action("بازگشت به قوانین خودکار",()=>_app.Automatic(captured));
            Action("کوچک‌کردن همین پنجره",()=> { if(!_app.Inventory.Minimize(captured))Say("انجام نشد؛ پنجره تغییر کرده یا روی دسکتاپ دیگر است."); });
            Action("بستن عادی همین پنجره",()=> { if(MessageBox.Show("بستن عادی این پنجره؟ برنامه می‌تواند درخواست ذخیره نشان دهد.","WinTag",MessageBoxButton.YesNo)==MessageBoxResult.Yes)_app.Inventory.CloseNormally(captured); });
            menu.Items.Add(new MenuItem { Header="دسکتاپ جدید: هنوز پشتیبانی نمی‌شود",IsEnabled=false });
            row.ContextMenu=menu;_list.Items.Add(row);
            if(captured.Generation==selected)_list.SelectedItem=row;
        }
        if(_list.SelectedIndex<0 && _list.Items.Count>0)_list.SelectedIndex=0;
    }
    internal void Open(System.Windows.Forms.Screen screen,bool keyboard)
    {
        _pointerEntered=false;ShowActivated=keyboard;
        var work=screen.WorkingArea;var h=new WindowInteropHelper(this).EnsureHandle();
        var monitor=PInvoke.MonitorFromPoint(new System.Drawing.Point(work.Left+1,work.Top+1),Windows.Win32.Graphics.Gdi.MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        uint dpi=96;
        if(PInvoke.GetDpiForMonitor(monitor,Windows.Win32.UI.HiDpi.MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI,out var dx,out _).Succeeded)dpi=dx;
        var width=Math.Min((int)(370*dpi/96d),work.Width);var height=Math.Min((int)(620*dpi/96d),work.Height);
        Show();
        PInvoke.SetWindowPos((HWND)h,(HWND)(nint)(-1),work.Right-width,work.Top+(work.Height-height)/2,width,height,keyboard?0:SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
        Refresh();Say(_app.Paused?"متوقف است":"دوبارکلیک / Enter: انتخاب · F2: تگ · Esc: بستن پنل");
        if(keyboard){Activate();_search.Focus();_search.SelectAll();}
    }
    internal void Destroy(){_allowClose=true;Close();}
}
