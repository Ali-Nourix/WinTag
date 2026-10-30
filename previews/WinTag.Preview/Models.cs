using System.Text.Json;
using System.Text.RegularExpressions;

namespace WinTag.Preview;
public sealed record Tag(string Id, string Name, string Color);
public sealed record Rule(string Process, string WindowClass, string Title, bool Regex, int Priority, string[] Tags);
public sealed record Shortcut(string Action, string? TagId, uint Modifiers, uint Key);
public sealed class Config
{
    public string PreviewSchema { get; set; } = "wintag-preview-1";
    public List<Tag> Tags { get; set; } = [new("work", "کار", "#639D89"), new("personal", "شخصی", "#AE9ACA")];
    public List<Rule> Rules { get; set; } = [];
    public List<Shortcut> Hotkeys { get; set; } = [new("Picker", null, 3, 0x20),new("Preserve", "work", 7, 0x31),new("Preserve", "personal", 7, 0x32)];
    public List<string> IgnoreProcesses { get; set; } = [];
    public double HotZoneWidthDip { get; set; } = 6;
    public double HotZoneHeightFraction { get; set; } = .30;
    public int SharedDwellMs { get; set; } = 250;
    public bool ExperimentalTitleBadge { get; set; } = false;
    public static JsonSerializerOptions Json { get; } = new() { WriteIndented = true, PropertyNameCaseInsensitive = false };
    public void Validate()
    {
        if (PreviewSchema != "wintag-preview-1" || Tags is null || Rules is null || Hotkeys is null || IgnoreProcesses is null)
            throw new InvalidDataException("Invalid preview schema.");
        if (Tags.Count > 100 || Tags.Any(t => t is null || string.IsNullOrWhiteSpace(t.Id) || string.IsNullOrWhiteSpace(t.Name)
            || !Regex.IsMatch(t.Color ?? "", "^#[0-9A-Fa-f]{6}$"))
            || Tags.Select(t => t.Id).Distinct().Count() != Tags.Count
            || Tags.Select(t => Search.Normalize(t.Name.Trim())).Distinct().Count() != Tags.Count)
            throw new InvalidDataException("Tags require unique IDs/names and #RRGGBB colors.");
        if (!double.IsFinite(HotZoneWidthDip) || HotZoneWidthDip < 1 || HotZoneWidthDip > 30 ||
            !double.IsFinite(HotZoneHeightFraction) || HotZoneHeightFraction <= 0 || HotZoneHeightFraction > 1 || SharedDwellMs < 0 || SharedDwellMs > 5000)
            throw new InvalidDataException("Invalid hot-zone settings.");
        if (Hotkeys.Any(k => k is null || (k.Action != "Picker" && k.Action != "Preserve" && k.Action != "Gather") ||
            (k.Action != "Picker" && !Tags.Any(t => t.Id == k.TagId)) || k.Key < 0x20 || k.Key > 0xFE || k.Modifiers < 1 || k.Modifiers > 7) ||
            Hotkeys.Select(k => (k.Modifiers,k.Key)).Distinct().Count() != Hotkeys.Count)
            throw new InvalidDataException("Invalid or duplicate hotkeys. Modifiers: Alt=1, Ctrl=2, Shift=4; no Win key.");
        if (Rules.Count > 64) throw new InvalidDataException("At most 64 preview rules.");
        foreach (var r in Rules)
        {
            if (r is null || r.Process is null || r.WindowClass is null || r.Title is null || r.Tags is null || r.Tags.Distinct().Count() != r.Tags.Length || r.Tags.Any(id => !Tags.Any(t => t.Id == id)) ||
                string.IsNullOrWhiteSpace(r.Process + r.WindowClass + r.Title)) throw new InvalidDataException("Invalid rule.");
            if (r.Regex) _ = new Regex(r.Title, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(50));
        }
        if (IgnoreProcesses.Any(string.IsNullOrWhiteSpace)) throw new InvalidDataException("Empty ignore process.");
    }
}
public static class Search
{
    public static string Normalize(string text) => text.Normalize().Replace('ي','ی').Replace('ك','ک').Replace('\u200c',' ').ToUpperInvariant();
    public static bool Contains(string text, string query) => Normalize(text).Contains(Normalize(query), StringComparison.Ordinal);
    public static string[] Match(Config config, string process, string windowClass, string title, string[]? manual)
    {
        if (manual is not null) return manual; // Including deliberate empty manual list.
        foreach (var r in config.Rules.OrderBy(r => r.Priority))
        {
            if (r.Process.Length > 0 && !r.Process.Equals(process,StringComparison.OrdinalIgnoreCase)) continue;
            if (r.WindowClass.Length > 0 && !r.WindowClass.Equals(windowClass,StringComparison.OrdinalIgnoreCase)) continue;
            try
            {
                if (r.Title.Length > 0 && !(r.Regex ? Regex.IsMatch(title,r.Title,RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                    TimeSpan.FromMilliseconds(50)) : Contains(title,r.Title))) continue;
            }
            catch (RegexMatchTimeoutException) { continue; }
            return r.Tags;
        }
        return [];
    }
}
internal static class Store
{
    internal static string DirectoryPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"WinTag","Preview");
    internal static string FilePath => Path.Combine(DirectoryPath,"config.json");
    internal static Config Parse(string text) { var c = JsonSerializer.Deserialize<Config>(text,Config.Json) ?? throw new InvalidDataException("Empty JSON."); c.Validate(); return c; }
    internal static void EnsureLocalFilePath(string path)
    {
        if (path.StartsWith(@"\\",StringComparison.Ordinal)) throw new IOException("Use a local file, not a network/device path.");
        var full=Path.GetFullPath(path);
        var root=Path.GetPathRoot(full) ?? throw new IOException("Missing local drive.");
        if (OperatingSystem.IsWindows() && new DriveInfo(root).DriveType is not (DriveType.Fixed or DriveType.Removable))
            throw new IOException("Use a fixed/removable local drive.");
        for(var d=Path.GetDirectoryName(full);d is not null;d=Path.GetDirectoryName(d))
            if(Directory.Exists(d) && (File.GetAttributes(d)&FileAttributes.ReparsePoint)!=0)throw new IOException("Reparse-point paths are not supported.");
        if(File.Exists(full) && (File.GetAttributes(full)&FileAttributes.ReparsePoint)!=0)throw new IOException("Reparse-point files are not supported.");
    }
    internal static void Save(Config c)
    {
        c.Validate(); EnsureLocalFilePath(FilePath); Directory.CreateDirectory(DirectoryPath);
        var temp = Path.Combine(DirectoryPath,Guid.NewGuid().ToString("N")+".tmp");
        try
        {
            using (var f = new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None))
            { var bytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(c,Config.Json)); f.Write(bytes); f.Flush(true); }
            if (File.Exists(FilePath)) File.Replace(temp,FilePath,FilePath+".bak"); else File.Move(temp,FilePath);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}
