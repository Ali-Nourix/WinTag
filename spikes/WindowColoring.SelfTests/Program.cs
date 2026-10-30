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
// Regression: diagnostics must carry useful fixed codes without leaking raw error strings.
Check(SafeDiagnostics.Code(new ProbeFailure(DiagnosticCode.TargetLeaseInvalid)) == DiagnosticCode.TargetLeaseInvalid, "specific identity failure preserved");
Check(SafeDiagnostics.Describe(new InvalidOperationException("SECRET-TITLE C:\\Users\\private")) == "InvalidState; no exception message or private identifiers recorded.", "exception messages redacted");
Check(SafeDiagnostics.Code(new TimeoutException()) == DiagnosticCode.Timeout, "timeout classified");
Check(SafeDiagnostics.Code(new OperationCanceledException()) == DiagnosticCode.Timeout, "cancelled deadline classified");
Check(SafeDiagnostics.Code(new IOException("private path")) == DiagnosticCode.PipeIo, "pipe IO classified");
Check(SafeDiagnostics.Code(new UnauthorizedAccessException()) == DiagnosticCode.AccessDenied, "access classified without elevation");
Check(SafeDiagnostics.Code(new InvalidDataException()) == DiagnosticCode.InvalidPayload, "invalid payload classified");
Check(SafeDiagnostics.Code(new ProbeFailure((DiagnosticCode)999)) == DiagnosticCode.Unknown, "unknown remote enum rejected");
var failure = new ProbeFailure(DiagnosticCode.DwmSetFailed, unchecked((int)0x80070005));
Check(SafeDiagnostics.Describe(failure).Contains("0x80070005"), "DWM HRESULT retained");
var reply = Wire.Decode<GuardianReply>(Wire.Encode(new GuardianReply(false, "Command rejected.", failure.Code, failure.NativeHResult)));
Check(reply.ErrorCode == DiagnosticCode.DwmSetFailed && reply.NativeHResult == failure.NativeHResult, "guardian error roundtrip");
var hello = Wire.Decode<TargetHello>(Wire.Encode(new TargetHello(null, failure.Code, failure.NativeHResult)));
Check(hello.Target is null && hello.ErrorCode == failure.Code, "target startup failure roundtrip");
Check(report.SchemaVersion == 2, "diagnostic schema bumped");
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
// Reproduce the actual pre-connect AutoFlush bug using real named pipes (no Windows APIs).
using (var server = new System.IO.Pipes.NamedPipeServerStream("WinTag-test-" + Guid.NewGuid().ToString("N"),
    System.IO.Pipes.PipeDirection.InOut, 1, System.IO.Pipes.PipeTransmissionMode.Byte, System.IO.Pipes.PipeOptions.Asynchronous))
{
    bool reproduced = false;
    try { using var premature = new StreamWriter(server, leaveOpen: true) { AutoFlush = true }; }
    catch (InvalidOperationException) { reproduced = true; }
    Check(reproduced, "pre-connect AutoFlush reproduces reported InvalidState");
}
var pipeName = "WinTag-test-" + Guid.NewGuid().ToString("N");
using (var server = new System.IO.Pipes.NamedPipeServerStream(pipeName, System.IO.Pipes.PipeDirection.InOut,
    1, System.IO.Pipes.PipeTransmissionMode.Byte, System.IO.Pipes.PipeOptions.Asynchronous))
using (var client = new System.IO.Pipes.NamedPipeClientStream(".", pipeName, System.IO.Pipes.PipeDirection.InOut,
    System.IO.Pipes.PipeOptions.Asynchronous))
{
    using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    await Task.WhenAll(server.WaitForConnectionAsync(deadline.Token), client.ConnectAsync(deadline.Token));
    using var writer = new StreamWriter(server, leaveOpen: true) { AutoFlush = true };
    using var reader = new StreamReader(client, leaveOpen: true);
    await writer.WriteLineAsync("connected");
    Check(await reader.ReadLineAsync(deadline.Token) == "connected", "post-connect writer flush handshake works");
}
Console.WriteLine($"PASS: {passed} portable checks. NOT Windows/DWM/IPC/cleanup evidence.");
