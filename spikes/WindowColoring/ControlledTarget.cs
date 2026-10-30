using System.Diagnostics;
using System.IO.Pipes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace WinTag.ColoringSpike;

internal sealed class ControlledTarget : Window
{
    private readonly string _pipe;
    private readonly string _marker = "WinTag.Spike." + Guid.NewGuid().ToString("N");
    private nint _hwnd;
    internal ControlledTarget(string pipe)
    {
        _pipe = pipe;
        Title = "WinTag disposable DWM target — پنجرهٔ آزمایشی";
        Width = 740; Height = 320;
        Content = new TextBlock
        {
            Text = "Controlled target in a SEPARATE process.\n" +
                "Seed caption #234567 and border #789ABC.\n" +
                "After killing the controller, these exact seeded colors must return.\n" +
                "Keep this window open to inspect recovery; closing it ends this test.\n\n" +
                "این پنجره متعلق به Spike است، نه یکی از برنامه‌های واقعی شما.",
            Margin = new Thickness(24), TextWrapping = TextWrapping.Wrap, FontSize = 17
        };
        SourceInitialized += InitializeTarget;
        Closed += (_, _) => { if (_hwnd != 0) Native.Unmark(_hwnd, _marker); };
    }

    private async void InitializeTarget(object? sender, EventArgs args)
    {
        TargetHello hello;
        try
        {
            _hwnd = new WindowInteropHelper(this).Handle;
            uint caption = Wire.ParseColor("#234567"), border = Wire.ParseColor("#789ABC");
            Native.Mark(_hwnd, _marker);
            Native.SetColor(_hwnd, Native.CaptionAttribute, caption);
            Native.SetColor(_hwnd, Native.BorderAttribute, border);
            using var self = Process.GetCurrentProcess();
            hello = new(new TargetSnapshot((long)_hwnd, self.Id,
                self.StartTime.ToUniversalTime().Ticks, _marker, caption, border));
        }
        catch (Exception error)
        {
            hello = new(null, SafeDiagnostics.Code(error), SafeDiagnostics.NativeResult(error));
            Content = new TextBlock { Text = "Target setup FAILED: " + SafeDiagnostics.Describe(error), Margin = new Thickness(24) };
        }
        try
        {
            using var pipe = new NamedPipeClientStream(".", _pipe, PipeDirection.Out, PipeOptions.Asynchronous);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await pipe.ConnectAsync(timeout.Token);
            await using var writer = new StreamWriter(pipe) { AutoFlush = true };
            await writer.WriteLineAsync(Wire.Encode(hello));
        }
        catch (Exception error)
        {
            Content = new TextBlock { Text = "Target handshake FAILED: " + SafeDiagnostics.Describe(error), Margin = new Thickness(24) };
        }
    }
}
