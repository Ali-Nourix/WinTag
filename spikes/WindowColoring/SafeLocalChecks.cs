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
            var stage = "ControlledStart";
            void Enter(string name)
            {
                stage = name;
                report.Checks.Add(new(name, CheckStatus.Info, "Stage entered; not a success result."));
            }
            ControlledSession? session = null;
            try
            {
                session = await ControlledSession.StartAsync(Enter);
                Enter("ControlledApply");
                await session.Guardian.SendAsync(new("apply", Wire.ParseColor("#4D665A"), Wire.ParseColor("#789889")));
                report.Checks.Add(new("ControlledApply", CheckStatus.Pass, "Native return success only; appearance not observed."));
                Enter("ControlledRestore");
                await session.Guardian.SendAsync(new("restore"));
                report.Checks.Add(new("ControlledRestore", CheckStatus.Pass, "Native return success only; visual recovery not verified."));
            }
            catch (Exception error)
            {
                report.Checks.Add(new(stage, CheckStatus.Failed, SafeDiagnostics.Describe(error)));
            }
            finally
            {
                if (session is not null)
                {
                    Enter("ControlledCleanup");
                    try
                    {
                        await session.FinishAsync(Enter);
                        report.Checks.Add(new("ControlledCleanup", CheckStatus.Pass,
                            "Guardian disposal and normal close of this action's own companion completed."));
                    }
                    catch (CleanupFailures errors)
                    {
                        for (var i = 0; i < errors.Errors.Count; i++)
                            report.Checks.Add(new(errors.Stages[i], CheckStatus.Failed, SafeDiagnostics.Describe(errors.Errors[i])));
                    }
                    catch (Exception error)
                    {
                        report.Checks.Add(new(stage, CheckStatus.Failed, SafeDiagnostics.Describe(error)));
                    }
                }
                else
                    report.Checks.Add(new("ControlledCleanup", CheckStatus.NotRun,
                        "Session did not initialize; best-effort normal close was requested for its own child. Inspect any leftover companion manually."));
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
