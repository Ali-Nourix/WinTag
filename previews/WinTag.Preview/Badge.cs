using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WinTag.Preview;
// Opt-in OWN HWND only. Does not change another process's title, styles, DWM or state.
internal sealed class Badge : Window
{
    private readonly TextBlock _label;
    private readonly Border _border;
    private readonly nint _handle;
    private readonly bool _safeStyle;
    internal Badge()
    {
        Width=88;Height=18;WindowStyle=WindowStyle.None;AllowsTransparency=true;Background=Brushes.Transparent;
        ShowInTaskbar=false;ShowActivated=false;Focusable=false;ResizeMode=ResizeMode.NoResize;
        _label=new TextBlock {FontSize=10,Foreground=Brushes.LightGray,FlowDirection=FlowDirection.RightToLeft,VerticalAlignment=VerticalAlignment.Center,TextTrimming=TextTrimming.CharacterEllipsis};
        _border=new Border {Background=new SolidColorBrush(Color.FromArgb(190,32,34,35)),BorderThickness=new Thickness(0,0,2,0),Padding=new Thickness(5,0,5,0),CornerRadius=new CornerRadius(3),Child=_label};Content=_border;
        _handle=new WindowInteropHelper(this).EnsureHandle();
        var ex=(long)PInvoke.GetWindowLongPtr((HWND)_handle,WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        PInvoke.SetWindowLongPtr((HWND)_handle,WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE,(nint)(ex|0x80000|0x20|0x80|0x08000000));
        _safeStyle=((long)PInvoke.GetWindowLongPtr((HWND)_handle,WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE) & 0x80800A0)==0x80800A0;
        HwndSource.FromHwnd(_handle)?.AddHook((nint h,int msg,nint w,nint l,ref bool handled)=>
        {
            if(msg==0x84){handled=true;return (nint)(-1);}
            if(msg==0x21){handled=true;return (nint)3;}
            return 0;
        });
    }
    internal void Update(LiveWindow? target,Tag? tag)
    {
        if(!_safeStyle || target is null || tag is null || target.Remote || PInvoke.GetForegroundWindow()!=(HWND)target.Handle ||
            PInvoke.IsIconic((HWND)target.Handle) || !PInvoke.GetWindowRect((HWND)target.Handle,out var r)) {Hide();return;}
        var text=StringInfo.GetTextElementEnumerator(tag.Name);var compact="";int count=0;
        while(text.MoveNext()){if(count++==8){compact+="…";break;}compact+=text.GetTextElement();}
        _label.Text=compact;System.Windows.Automation.AutomationProperties.SetName(this,tag.Name);
        var color=(Color)ColorConverter.ConvertFromString(tag.Color);color.A=180;_border.BorderBrush=new SolidColorBrush(color);
        var scale=Math.Max(96,PInvoke.GetDpiForWindow((HWND)target.Handle))/96d;
        var width=(int)Math.Min(88*scale,(r.right-r.left)*.2);
        if(width<32*scale){Hide();return;}
        if(!IsVisible)Show();
        PInvoke.SetWindowPos((HWND)_handle,(HWND)(nint)(-1),r.left+(r.right-r.left-width)/2,r.top+(int)(6*scale),width,(int)(18*scale),SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
    }
}
