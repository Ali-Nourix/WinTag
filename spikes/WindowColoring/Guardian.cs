using System.Diagnostics;
using System.IO.Pipes;

namespace WinTag.ColoringSpike;

internal static class Guardian
{
    internal static async Task<int> RunAsync(string pipeName, int parentPid, long parentBirth)
    {
        using var parent = Process.GetProcessById(parentPid);
        if (parent.StartTime.ToUniversalTime().Ticks != parentBirth) return 2;
        using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        using var connectTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await pipe.ConnectAsync(connectTimeout.Token);
        var reader = new StreamReader(pipe, leaveOpen: true);
        var writer = new StreamWriter(pipe, leaveOpen: true) { AutoFlush = true };
        var snapshot = Wire.Decode<TargetSnapshot>(await reader.ReadLineAsync(connectTimeout.Token));
        try { Native.ValidateLive(snapshot); }
        catch (Exception error)
        {
            await writer.WriteLineAsync(Wire.Encode(new GuardianReply(false, "Identity rejected.",
                SafeDiagnostics.Code(error), SafeDiagnostics.NativeResult(error))));
            return 2;
        }
        // The separate controlled target itself seeded these exact originals. Never a foreign app.
        await writer.WriteLineAsync(Wire.Encode(new GuardianReply(true, "Snapshot acknowledged; DWM mutations are guardian-owned.")));
        var commands = ProcessCommands(reader, writer, snapshot);
        var exited = parent.WaitForExitAsync(); // kernel process wait, no periodic heartbeat
        try
        {
            await Task.WhenAny(commands, exited);
            if (!commands.IsCompleted) pipe.Dispose(); // interrupts read if main died first
            try { await commands; } catch (IOException) { } catch (ObjectDisposedException) { }
        }
        finally
        {
            try { Native.Restore(snapshot); }
            catch (InvalidOperationException) { /* Closed/stale target: never touch another window. */ }
        }
        return 0;
    }

    private static async Task ProcessCommands(StreamReader reader, StreamWriter writer, TargetSnapshot snapshot)
    {
        while (await reader.ReadLineAsync() is { } line) // blocks awaiting IPC; NOT polling
        {
            var command = Wire.Decode<GuardianCommand>(line);
            try
            {
                Native.ValidateLive(snapshot);
                switch (command.Action)
                {
                    case "apply":
                        Native.SetColor((nint)snapshot.Hwnd, Native.CaptionAttribute, command.Caption);
                        Native.SetColor((nint)snapshot.Hwnd, Native.BorderAttribute, command.Border);
                        break;
                    case "restore": Native.Restore(snapshot); break;
                    case "stop": return;
                    default: throw new InvalidDataException("Unknown command.");
                }
                await writer.WriteLineAsync(Wire.Encode(new GuardianReply(true, command.Action + " completed; inspect visually.")));
            }
            catch (Exception error) when (error is InvalidOperationException or InvalidDataException)
            {
                try { Native.Restore(snapshot); } catch (InvalidOperationException) { }
                await writer.WriteLineAsync(Wire.Encode(new GuardianReply(false, "Command rejected.", SafeDiagnostics.Code(error), SafeDiagnostics.NativeResult(error))));
            }
        }
    }
}
