using System.Globalization;

namespace WinTag.ColoringSpike;

public static class OverlayAppearance
{
    public const double HeightDip = 18;
    public const double MaxWidthDip = 92;
    public static string CompactLabel(string tag)
    {
        var text = tag.Trim();
        if (text.Length == 0) return "تگ";
        var elements = StringInfo.ParseCombiningCharacters(text);
        return elements.Length <= 8 ? text : text[..elements[8]] + "…";
    }
}
