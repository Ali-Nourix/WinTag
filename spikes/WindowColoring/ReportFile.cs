using System.Text.Json;

namespace WinTag.ColoringSpike;

public static class ReportFile
{
    public static void ValidateLeafName(string name)
    {
        var stem = Path.GetFileNameWithoutExtension(name).Split('.')[0].ToUpperInvariant();
        var devices = new[] { "CON", "PRN", "AUX", "NUL", "CONIN$", "CONOUT$" };
        if (name.Length <= 5 || !name.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
            name.IndexOfAny([':', '/', '\\', '"', '*', '?', '<', '>', '|']) >= 0 ||
            devices.Contains(stem) || (stem.Length == 4 && (stem.StartsWith("COM") || stem.StartsWith("LPT")) && stem[3] is >= '0' and <= '9'))
            throw new InvalidDataException("Choose a plain, non-device .json filename.");
    }

    public static string PrepareNew(string path)
    {
        if (path.StartsWith(@"\\", StringComparison.Ordinal))
            throw new InvalidDataException("Network/device report paths are not allowed.");
        var full = Path.GetFullPath(path);
        ValidateLeafName(Path.GetFileName(full));
        var parent = Path.GetDirectoryName(full) ?? throw new InvalidDataException("Missing directory.");
        EnsureLocalDirectory(parent);
        if (File.Exists(full) || Directory.Exists(full)) throw new IOException("Report already exists; choose a new filename.");
        return full;
    }

    public static void EnsureLocalDirectory(string directory)
    {
        var full = Path.GetFullPath(directory);
        if (full.StartsWith(@"\\", StringComparison.Ordinal))
            throw new InvalidDataException("Only a local non-network directory is allowed.");
        if (OperatingSystem.IsWindows())
        {
            var drive = new DriveInfo(Path.GetPathRoot(full)!);
            if (drive.DriveType is not (DriveType.Fixed or DriveType.Removable))
                throw new InvalidDataException("Use a fixed/removable local drive, not a network drive.");
        }
        // Reject junctions/symlinks instead of accidentally redirecting a report elsewhere.
        for (var p = new DirectoryInfo(full); p is not null; p = p.Parent)
            if (p.Exists && (p.Attributes & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Report directory must not contain a junction or symlink.");
        Directory.CreateDirectory(full);
    }

    public static void SaveNew(string path, SafeCheckReport report)
    {
        var full = PrepareNew(path);
        var temp = Path.Combine(Path.GetDirectoryName(full)!, ".wintag-" + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, report, SafeCheckReport.JsonOptions);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temp, full, overwrite: false);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}
