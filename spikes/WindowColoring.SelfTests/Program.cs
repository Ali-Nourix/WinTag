using System.Text.Json;
using WinTag.ColoringSpike;

int passed = PortableChecks.Run();
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    passed++;
}
void Reject(Action action, string name)
{
    try { action(); } catch (Exception e) when (e is InvalidDataException or ArgumentException or IOException)
    { passed++; return; }
    throw new Exception("FAIL (accepted invalid input): " + name);
}
Check(OverlayAppearance.CompactLabel(" کار ") == "کار", "trim short tag");
Check(OverlayAppearance.CompactLabel("") == "تگ", "empty retains a text label");
Check(OverlayAppearance.CompactLabel("123456789") == "12345678…", "compact text bounded");
Check(OverlayAppearance.CompactLabel("ا\u065412345678") == "ا\u06541234567…", "combining mark not split");
Check(OverlayAppearance.HeightDip == 18 && OverlayAppearance.MaxWidthDip == 92, "minimal overlay bounds");
var report = new SafeCheckReport(false);
report.Checks.Add(new("Example", CheckStatus.Pass, "Return code only."));
Check(!report.ProductionAcceptance && !report.ExternalWindowMutationAllowed && !report.CrashInjectionRequested,
    "passing smoke checks cannot certify production or authorize external mutation/crash");
Check(!report.HasOperationalFailures, "ordinary pass is not an operational error");
report.Checks.Add(new("Manual visual", CheckStatus.NotRun, "Needs a human."));
Check(!report.HasOperationalFailures, "NOT RUN is not mislabeled as failure or acceptance");
report.Checks.Add(new("Example failure", CheckStatus.Failed, "Type only."));
Check(report.HasOperationalFailures, "failure affects exit status");
var json = JsonSerializer.Serialize(report, SafeCheckReport.JsonOptions);
Check(json.Contains("NotRun") && json.Contains("productionAcceptance"), "readable report schema");
Check(!json.Contains("windowTitle") && !json.Contains("machineName") && !json.Contains("userName"), "report excludes private identifiers");
Reject(() => ReportFile.ValidateLeafName("x.txt"), "report must be JSON");
Reject(() => ReportFile.ValidateLeafName("x.json:stream"), "no alternate data stream");
Reject(() => ReportFile.ValidateLeafName("../x.json"), "no path separators in leaf");
Reject(() => ReportFile.ValidateLeafName("CON.json"), "no reserved DOS device");
var temp = Path.Combine(Path.GetTempPath(), "WinTag-safe-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(temp);
try
{
    var output = Path.Combine(temp, "report.json");
    ReportFile.SaveNew(output, report);
    Check(File.ReadAllText(output).Contains("productionAcceptance"), "new report saved");
    var bytes = File.ReadAllBytes(output);
    Reject(() => ReportFile.SaveNew(output, new SafeCheckReport(true)), "existing report never overwritten");
    Check(File.ReadAllBytes(output).SequenceEqual(bytes), "existing file unchanged");
    Check(Directory.GetFiles(temp).Length == 1, "no abandoned staged report");
}
finally { Directory.Delete(temp, true); }
Console.WriteLine($"PASS: {passed} portable checks. NOT Windows/DWM/IPC/cleanup evidence.");
