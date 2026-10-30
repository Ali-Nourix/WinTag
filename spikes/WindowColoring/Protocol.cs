using System.Globalization;
using System.Text.Json;

namespace WinTag.ColoringSpike;

// Pure protocol: linked into the cross-platform self-test project, no Windows dependencies.
public sealed record TargetSnapshot(long Hwnd, int ProcessId, long ProcessBirthTicks,
    string MarkerName, uint CaptionColor, uint BorderColor)
{
    public void Validate()
    {
        if (Hwnd == 0 || ProcessId <= 0 || ProcessBirthTicks <= 0 ||
            string.IsNullOrEmpty(MarkerName) || !MarkerName.StartsWith("WinTag.Spike.", StringComparison.Ordinal) ||
            !Guid.TryParseExact(MarkerName[13..], "N", out _))
            throw new InvalidDataException("Invalid controlled-target identity.");
    }
}

public sealed record GuardianCommand(string Action, uint Caption = 0, uint Border = 0);
public sealed record GuardianReply(bool Ok, string Message);

public static class Wire
{
    public static string Encode<T>(T value) => JsonSerializer.Serialize(value);
    public static T Decode<T>(string? line) => line is { Length: <= 8192 }
        ? JsonSerializer.Deserialize<T>(line) ?? throw new InvalidDataException("Empty message.")
        : throw new InvalidDataException("Missing or oversized message.");

    public static uint ParseColor(string text)
    {
        if (text.Length != 7 || text[0] != '#' ||
            !uint.TryParse(text.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
            throw new FormatException("Use #RRGGBB.");
        // Win32 COLORREF is 0x00BBGGRR, not 0x00RRGGBB.
        return ((rgb & 0xff0000) >> 16) | (rgb & 0x00ff00) | ((rgb & 0xff) << 16);
    }
}
