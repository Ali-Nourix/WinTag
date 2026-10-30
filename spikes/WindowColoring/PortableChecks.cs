namespace WinTag.ColoringSpike;

public static class PortableChecks
{
    public static int Run()
    {
        int passed = 0;
        void Check(bool condition, string name)
        {
            if (!condition) throw new Exception("FAIL: " + name);
            passed++;
        }
        void Reject(Action action, string name)
        {
            try { action(); } catch (Exception e) when (e is InvalidDataException or FormatException) { passed++; return; }
            throw new Exception("FAIL (accepted invalid input): " + name);
        }
        var valid = new TargetSnapshot(123, 42, 123456789, "WinTag.Spike." + Guid.NewGuid().ToString("N"), 0xffffffff, 0xfffffffe);
        valid.Validate(); passed++;
        Check(Wire.Decode<TargetSnapshot>(Wire.Encode(valid)) == valid, "snapshot round trip including sentinel originals");
        Reject(() => (valid with { Hwnd = 0 }).Validate(), "zero HWND");
        Reject(() => (valid with { ProcessId = 0 }).Validate(), "zero PID");
        Reject(() => (valid with { ProcessBirthTicks = 0 }).Validate(), "missing process birth");
        Reject(() => (valid with { MarkerName = null! }).Validate(), "null marker");
        Reject(() => (valid with { MarkerName = "foreign" }).Validate(), "foreign marker");
        Reject(() => (valid with { MarkerName = "WinTag.Spike.not-a-guid" }).Validate(), "malformed marker nonce");
        Reject(() => Wire.Decode<TargetSnapshot>(null), "missing message");
        Reject(() => Wire.Decode<TargetSnapshot>(new string('x', 8193)), "oversized message");
        Check(Wire.ParseColor("#123456") == 0x00563412, "COLORREF byte ordering");
        Check(Wire.ParseColor("#FFFFFF") == 0x00ffffff, "white");
        Check(Wire.ParseColor("#000000") == 0, "black");
        Reject(() => Wire.ParseColor("#FFF"), "short color");
        Reject(() => Wire.ParseColor("red"), "color name");
        Reject(() => Wire.ParseColor("#GG0000"), "invalid hex");
        var cmd = new GuardianCommand("apply", 0x00123456, 0x007890ab);
        Check(Wire.Decode<GuardianCommand>(Wire.Encode(cmd)) == cmd, "command round trip");
        Check(Wire.Decode<GuardianReply>(Wire.Encode(new GuardianReply(false, "خطا · error"))).Message == "خطا · error", "Persian protocol");
        return passed;
    }
}
