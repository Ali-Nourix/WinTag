using System.Diagnostics;

namespace WinTag.ColoringSpike;

// All targets are freshly spawned by this action; never attach this workflow to a user's app.
internal sealed class ControlledSession : IAsyncDisposable
{
    internal Process Process { get; }
    internal TargetSnapshot Target { get; }
    internal GuardianSession Guardian { get; }
    private ControlledSession(Process process, TargetSnapshot target, GuardianSession guardian)
        => (Process, Target, Guardian) = (process, target, guardian);

    internal static async Task<ControlledSession> StartAsync()
    {
        var name = "WinTag.Spike.Target." + Guid.NewGuid().ToString("N");
        using var pipe = GuardianSession.CreateServer(name);
        var process = Program.Spawn("--target", name);
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await pipe.WaitForConnectionAsync(timeout.Token);
            using var reader = new StreamReader(pipe);
            var snapshot = Wire.Decode<TargetSnapshot>(await reader.ReadLineAsync(timeout.Token));
            if (snapshot.ProcessId != process.Id || snapshot.ProcessBirthTicks != process.StartTime.ToUniversalTime().Ticks)
                throw new InvalidDataException("Controlled child process handshake mismatch.");
            Native.ValidateLive(snapshot);
            return new(process, snapshot, await GuardianSession.StartAsync(snapshot));
        }
        catch
        {
            // Only the child created by us; never force terminate it or another application.
            try { if (!process.HasExited) process.CloseMainWindow(); } catch (InvalidOperationException) { }
            process.Dispose();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try { await Guardian.DisposeAsync(); }
        finally
        {
            try
            {
                if (!Process.HasExited)
                {
                    Native.ValidateLive(Target);
                    if (!Process.CloseMainWindow()) throw new IOException("Controlled target did not accept normal close.");
                    await Process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(3));
                }
            }
            finally { Process.Dispose(); }
        }
    }
}
