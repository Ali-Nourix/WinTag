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

    internal static async Task<ControlledSession> StartAsync(Action<string>? stage = null)
    {
        var name = "WinTag.Spike.Target." + Guid.NewGuid().ToString("N");
        using var pipe = GuardianSession.CreateServer(name);
        stage?.Invoke("TargetSpawn");
        var process = Program.Spawn("--target", name);
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            stage?.Invoke("TargetPipeConnect");
            await pipe.WaitForConnectionAsync(timeout.Token);
            using var reader = new StreamReader(pipe);
            stage?.Invoke("TargetHandshake");
            var hello = Wire.Decode<TargetHello>(await reader.ReadLineAsync(timeout.Token));
            if (hello.ErrorCode is { } errorCode) throw new ProbeFailure(errorCode, hello.NativeHResult);
            var snapshot = hello.Target ?? throw new InvalidDataException("Missing target snapshot.");
            stage?.Invoke("TargetIdentity");
            if (snapshot.ProcessId != process.Id || snapshot.ProcessBirthTicks != process.StartTime.ToUniversalTime().Ticks)
                throw new ProbeFailure(DiagnosticCode.TargetHandshakeMismatch);
            Native.ValidateLive(snapshot);
            return new(process, snapshot, await GuardianSession.StartAsync(snapshot, stage));
        }
        catch
        {
            // Only the child created by us; never force terminate it or another application.
            try { if (!process.HasExited) process.CloseMainWindow(); } catch (Exception) { /* Preserve original startup failure; never force-close. */ }
            process.Dispose();
            throw;
        }
    }

    public ValueTask DisposeAsync() => FinishAsync();

    internal async ValueTask FinishAsync(Action<string>? stage = null)
    {
        // Preserve and surface each failure; never let cleanup mask an earlier failure.
        var errors = new List<Exception>();
        var failedStages = new List<string>();
        stage?.Invoke("GuardianCleanup");
        try { await Guardian.DisposeAsync(); }
        catch (Exception error) { errors.Add(error); failedStages.Add("GuardianCleanup"); }
        try
        {
            if (!Process.HasExited)
            {
                stage?.Invoke("TargetCloseIdentity");
                Native.ValidateLive(Target);
                stage?.Invoke("TargetNormalClose");
                if (!Process.CloseMainWindow()) throw new ProbeFailure(DiagnosticCode.TargetCloseRefused);
                stage?.Invoke("TargetExitWait");
                await Process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(3));
            }
        }
        catch (Exception error) { errors.Add(error); failedStages.Add("TargetCleanup"); }
        finally { Process.Dispose(); }
        if (errors.Count == 1)
        {
            if (failedStages[0] == "GuardianCleanup") stage?.Invoke("GuardianCleanup");
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(errors[0]).Throw();
        }
        if (errors.Count > 1) throw new CleanupFailures(errors, failedStages);
    }
}
