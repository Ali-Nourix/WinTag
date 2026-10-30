using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinTag.ColoringSpike;

public enum CheckStatus { Pass, Info, NotRun, Failed, Blocked }
public sealed record LocalCheck(string Name, CheckStatus Status, string Note);

public sealed class SafeCheckReport(bool controlledTargetRequested)
{
    public int SchemaVersion => 1;
    public string RunId { get; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset CreatedUtc { get; } = DateTimeOffset.UtcNow;
    public bool ControlledTargetRequested { get; } = controlledTargetRequested;
    public bool ProductionAcceptance => false;
    public bool RequiresManualVerification => true;
    public bool ExternalWindowMutationAllowed => false;
    public bool CrashInjectionRequested => false;
    public Dictionary<string, string> Environment { get; } = [];
    public List<LocalCheck> Checks { get; } = [];
    public bool HasOperationalFailures => Checks.Any(c => c.Status is CheckStatus.Failed or CheckStatus.Blocked);
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
}
