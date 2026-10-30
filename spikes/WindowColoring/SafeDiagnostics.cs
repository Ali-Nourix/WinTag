namespace WinTag.ColoringSpike;

// Fixed vocabulary only: never serialize exception.Message, stack, path, HWND, PID or title.
public enum DiagnosticCode
{
    Unknown, DwmSetFailed, TargetClosed, TargetPidChanged, TargetLeaseInvalid,
    MarkerWriteFailed, TargetHandshakeMismatch, GuardianExited, GuardianRejected,
    InvalidPayload, Timeout, PipeIo, AccessDenied, InvalidState, TargetCloseRefused
}
public sealed class ProbeFailure(DiagnosticCode code, int? nativeHResult = null)
    : InvalidOperationException(code.ToString())
{
    public DiagnosticCode Code { get; } = code;
    public int? NativeHResult { get; } = nativeHResult;
}
public static class SafeDiagnostics
{
    public static DiagnosticCode Code(Exception error) => error switch
    {
        ProbeFailure p => Enum.IsDefined(p.Code) ? p.Code : DiagnosticCode.Unknown,
        TimeoutException or OperationCanceledException => DiagnosticCode.Timeout,
        UnauthorizedAccessException => DiagnosticCode.AccessDenied,
        InvalidDataException or System.Text.Json.JsonException => DiagnosticCode.InvalidPayload,
        IOException => DiagnosticCode.PipeIo,
        InvalidOperationException => DiagnosticCode.InvalidState,
        _ => DiagnosticCode.Unknown
    };
    public static int? NativeResult(Exception error) => error is ProbeFailure p ? p.NativeHResult : null;
    public static string Describe(Exception error)
        => Code(error) + (NativeResult(error) is int hr ? $"; nativeHResult=0x{hr:X8}" : "")
            + "; no exception message or private identifiers recorded.";
}

// Only fixed stage labels are supplied internally. No raw aggregate message is persisted.
internal sealed class CleanupFailures(IReadOnlyList<Exception> errors, IReadOnlyList<string> stages)
    : Exception("Multiple controlled cleanup failures.")
{
    internal IReadOnlyList<Exception> Errors { get; } = errors;
    internal IReadOnlyList<string> Stages { get; } = stages;
}
