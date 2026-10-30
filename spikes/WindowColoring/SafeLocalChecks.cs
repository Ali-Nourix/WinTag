using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace WinTag.ColoringSpike;

internal static class SafeLocalChecks
{
    internal static async Task<SafeCheckReport> RunAsync(bool includeControlledTarget)
    {
        var report = new SafeCheckReport(includeControlledTarget);
        report.Environment["osVersion"] = System.Environment.OSVersion.Version.ToString();
        report.Environment["runtime"] = RuntimeInformation.FrameworkDescription;
        report.Environment["processArchitecture"] = RuntimeInformation.ProcessArchitecture.ToString();
        report.Environment["osArchitecture"] = RuntimeInformation.OSArchitecture.ToString();
        report.Environment["probeVersion"] = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";
        report.Checks.Add(new("Windows11", OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000)
            ? CheckStatus.Pass : CheckStatus.Blocked, "OS version check only; not a desktop-wrapper certification."));
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var elevated = new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
            report.Checks.Add(new("LeastPrivilege", elevated ? CheckStatus.Blocked : CheckStatus.Pass,
                elevated ? "Close elevated instance and run as a standard user; native checks skipped."
                    : "Standard-user token. No elevation requested."));
        }
        catch (Exception error)
        {
            report.Checks.Add(new("LeastPrivilege", CheckStatus.Blocked, error.GetType().Name));
        }
        AddManualGates(report);
        if (report.HasOperationalFailures) return report;
        try
        {
            using var versionKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", writable: false);
            report.Environment["windowsUbr"] = versionKey?.GetValue("UBR")?.ToString() ?? "unknown";
            report.Checks.Add(new("PureProtocol", CheckStatus.Pass, $"{PortableChecks.Run()} pure checks; not native visual evidence."));
            report.Checks.Add(new("DwmComposition", Native.IsCompositionEnabled() ? CheckStatus.Pass : CheckStatus.Failed,
                "Read-only DwmIsCompositionEnabled result. No external window enumerated or modified."));
            using var self = Process.GetCurrentProcess();
            self.Refresh();
            report.Environment["controllerWorkingSetBytesSnapshot"] = self.WorkingSet64.ToString(System.Globalization.CultureInfo.InvariantCulture);
            report.Environment["controllerPrivateBytesSnapshot"] = self.PrivateMemorySize64.ToString(System.Globalization.CultureInfo.InvariantCulture);
            report.Checks.Add(new("ResourceSnapshot", CheckStatus.Info,
                "One controller-only sample, not combined-process idle/memory/performance acceptance."));
        }
        catch (Exception error) { report.Checks.Add(new("ReadOnlyChecks", CheckStatus.Failed, error.GetType().Name)); }

        if (includeControlledTarget && !report.HasOperationalFailures)
        {
            try
            {
                await using (var session = await ControlledSession.StartAsync())
                {
                    await session.Guardian.SendAsync(new("apply", Wire.ParseColor("#4D665A"), Wire.ParseColor("#789889")));
                    await session.Guardian.SendAsync(new("restore"));
                    report.Checks.Add(new("ControlledDwmReturnCodes", CheckStatus.Pass,
                        "New companion only: guardian ack, apply and restore returned successfully; appearance not observed."));
                }
                report.Checks.Add(new("ControlledCompanionCleanup", CheckStatus.Pass,
                    "Guardian disposal and normal close of this action's own companion completed. No existing app was closed."));
            }
            catch (Exception error)
            {
                report.Checks.Add(new("ControlledDwmOrCleanup", CheckStatus.Failed,
                    error.GetType().Name + ": inspect the disposable companion if still open. No force-kill attempted."));
            }
        }
        else if (!includeControlledTarget)
            report.Checks.Add(new("ControlledDwm", CheckStatus.NotRun, "Opt-in was not selected; no target or guardian spawned."));
        return report;
    }

    private static void AddManualGates(SafeCheckReport report)
    {
        foreach (var name in new[] { "ActualTitleBarAppearance", "RealAppClickThrough", "MainCrashRecovery",
            "GuardianCrashRecovery", "MixedDpiAndMonitors", "DesktopCompatibility", "IdleCpuAndCombinedMemory", "RuntimeNetworkTraffic" })
            report.Checks.Add(new(name, CheckStatus.NotRun, "Requires separate manual/instrumented verification; never auto-certified."));
    }
}
