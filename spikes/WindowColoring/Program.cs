using System.Diagnostics;
using System.Windows;

namespace WinTag.ColoringSpike;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
                throw new PlatformNotSupportedException("Windows 11 is required.");
            if (args.Length == 4 && args[0] == "--guardian")
                return Guardian.RunAsync(args[1], int.Parse(args[2]), long.Parse(args[3])).GetAwaiter().GetResult();
            if (args.Length == 2 && args[0] == "--target")
                return new Application().Run(new ControlledTarget(args[1]));
            if ((args.Length == 2 || args.Length == 3) && args[0] == "--safe-checks")
            {
                if (args.Length == 3 && args[2] != "--controlled-target")
                    throw new ArgumentException("Unsupported safe-check option.");
                var path = ReportFile.PrepareNew(args[1]); // validate local destination before probes
                var report = SafeLocalChecks.RunAsync(args.Length == 3).GetAwaiter().GetResult();
                ReportFile.SaveNew(path, report);
                return report.HasOperationalFailures ? 2 : 0;
            }
            if (args.Length != 0) throw new ArgumentException("Unsupported arguments.");
            return new Application { ShutdownMode = ShutdownMode.OnMainWindowClose }.Run(new ProbeWindow());
        }
        catch (Exception error)
        {
            // Headless modes do not initialize WPF or write window titles to logs.
            if (args.Length == 0) MessageBox.Show(error.Message, "WinTag Spike");
            return 1;
        }
    }

    internal static Process Spawn(params string[] args)
    {
        var executable = Environment.ProcessPath ?? throw new InvalidOperationException("No executable path.");
        var info = new ProcessStartInfo(executable) { UseShellExecute = false };
        if (Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            info.ArgumentList.Add(typeof(Program).Assembly.Location);
        foreach (var arg in args) info.ArgumentList.Add(arg);
        return Process.Start(info) ?? throw new InvalidOperationException("Child process did not start.");
    }
}
