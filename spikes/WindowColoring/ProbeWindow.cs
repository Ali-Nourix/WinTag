using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace WinTag.ColoringSpike;

internal sealed class ProbeWindow : Window
{
    private readonly ListBox _windows = new() { Height = 165 };
    private readonly TextBox _tag = new() { Text = "کار · Work", Width = 220, Margin = new Thickness(6) };
    private readonly TextBox _status = new() { IsReadOnly = true, TextWrapping = TextWrapping.Wrap,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Height = 135 };
    private readonly StackPanel _actions = new();
    private TargetSnapshot? _target;
    private Process? _targetProcess;
    private GuardianSession? _guardian;
    private TagOverlay? _overlay;
    private bool _closing;

    internal ProbeWindow()
    {
        Title = "WinTag — T001 coloring Spike (NOT production)";
        Width = 930; Height = 850;
        var panel = new StackPanel { Margin = new Thickness(18) };
        Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        panel.Children.Add(new TextBlock { Text = "Safe boundary: DWM mutations ONLY on the disposable companion target.\n" +
            "Real apps: read-only color probe + active-window overlay only. Windows evidence is NOT RUN.",
            TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 12) });
        panel.Children.Add(_actions);
        AddAction("1. Launch separate controlled target + start guardian", LaunchControlledAsync);
        AddAction("2. Apply DWM colors to CONTROLLED target", async () =>
        {
            RequireGuardian();
            Status(await _guardian!.SendAsync(new("apply", Wire.ParseColor("#166B4B"), Wire.ParseColor("#45D9A3"))));
        });
        AddAction("3. Restore controlled target via guardian", async () =>
        {
            RequireGuardian(); Status(await _guardian!.SendAsync(new("restore")));
        });
        panel.Children.Add(new TextBlock { Text = "External windows — explicit Refresh (no background inventory polling):", Margin = new Thickness(0, 12, 0, 6) });
        panel.Children.Add(_windows);
        AddAction("Refresh eligible candidates", () => { _windows.ItemsSource = Native.Enumerate(); return Task.CompletedTask; });
        AddAction("READ-ONLY DWM getter probe on selected window", () =>
        {
            var selected = Selected(); Status(Native.ReadOnlyColorProbe(selected.Handle)); return Task.CompletedTask;
        });
        panel.Children.Add(_tag);
        AddAction("Show trial tag overlay on selected app (no DWM mutation)", () =>
        {
            var selected = Selected();
            _overlay?.Dispose(); _overlay = new TagOverlay(selected.Handle, _tag.Text);
            Native.Activate(selected.Handle);
            Status("Overlay enabled for the selected foreground target only. Test click-through, drag, DPI and captions. No success inferred.");
            return Task.CompletedTask;
        });
        AddAction("Remove overlay", () => { _overlay?.Dispose(); _overlay = null; Status("Overlay removed."); return Task.CompletedTask; });
        AddAction("FAULT INJECTION: abruptly kill THIS controller (target + guardian survive)", () =>
        {
            if (MessageBox.Show("Kill this controller now? Inspect the separate target for original colors within 1 s.\n" +
                "Do not kill the entire process tree.", "Explicit crash test", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Process.GetCurrentProcess().Kill();
            return Task.CompletedTask;
        });
        panel.Children.Add(_status);
        Status("Ready. No Windows check is automatically marked PASS. Original custom colors on foreign apps are never guessed.");
        Closing += OnClosing;
    }

    private void AddAction(string text, Func<Task> action)
    {
        var button = new Button { Content = text, Margin = new Thickness(0, 3, 0, 3), Padding = new Thickness(8) };
        button.Click += async (_, _) =>
        {
            _actions.IsEnabled = false;
            try { await action(); }
            catch (Exception error) { Status("FAILED: " + error.Message); }
            finally { _actions.IsEnabled = true; }
        };
        _actions.Children.Add(button);
    }
    private WindowChoice Selected() => _windows.SelectedItem as WindowChoice ?? throw new InvalidOperationException("Select a candidate window first.");
    private void RequireGuardian()
    {
        if (_guardian is null || _target is null) throw new InvalidOperationException("Launch the controlled target first.");
        Native.ValidateLive(_target);
    }
    private void Status(string text) => _status.Text = text; // titles/handles never automatically persisted

    private async Task LaunchControlledAsync()
    {
        if (_guardian is not null) throw new InvalidOperationException("Controlled session already exists; restart the probe for another.");
        var pipeName = "WinTag.Spike.Target." + Guid.NewGuid().ToString("N");
        using var pipe = GuardianSession.CreateServer(pipeName);
        _targetProcess = Program.Spawn("--target", pipeName);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await pipe.WaitForConnectionAsync(timeout.Token);
        using var reader = new StreamReader(pipe);
        var snapshot = Wire.Decode<TargetSnapshot>(await reader.ReadLineAsync(timeout.Token));
        if (snapshot.ProcessId != _targetProcess.Id || snapshot.ProcessBirthTicks != _targetProcess.StartTime.ToUniversalTime().Ticks)
            throw new InvalidDataException("Target handshake process mismatch.");
        Native.ValidateLive(snapshot);
        _target = snapshot;
        _guardian = await GuardianSession.StartAsync(snapshot);
        Status($"Controlled target PID={snapshot.ProcessId}, guardian PID={_guardian.ProcessId}. " +
            "Exact originals were seeded by our companion, not read from an arbitrary app. Inspect colors manually.");
    }

    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs args)
    {
        if (_closing) return;
        args.Cancel = true; _closing = true; IsEnabled = false;
        _overlay?.Dispose();
        try { if (_guardian is not null) await _guardian.DisposeAsync(); }
        catch (Exception error) { MessageBox.Show("Cleanup needs inspection: " + error.Message); }
        // Leave companion alive so normal-exit restoration can be inspected too.
        Close();
    }
}
