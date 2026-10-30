using System.Diagnostics;
using System.IO.Pipes;

namespace WinTag.ColoringSpike;

internal sealed class GuardianSession : IAsyncDisposable
{
    private readonly TargetSnapshot _target;
    private readonly NamedPipeServerStream _pipe;
    private readonly Process _process;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly SemaphoreSlim _commands = new(1);
    private bool _disposed;
    internal int ProcessId => _process.Id;

    private GuardianSession(TargetSnapshot target, NamedPipeServerStream pipe, Process process)
    {
        _target = target; _pipe = pipe; _process = process;
        _reader = new(pipe, leaveOpen: true);
        _writer = new(pipe, leaveOpen: true) { AutoFlush = true };
        _process.EnableRaisingEvents = true;
        _process.Exited += (_, _) =>
        {
            // Main is alive when guardian fails: restore, disable further requests.
            try { Native.Restore(_target); } catch (Exception) { /* Never guess another target. */ }
        };
    }

    internal static async Task<GuardianSession> StartAsync(TargetSnapshot target, Action<string>? stage = null)
    {
        stage?.Invoke("GuardianTargetIdentity");
        Native.ValidateLive(target);
        var name = "WinTag.Spike.Guardian." + Guid.NewGuid().ToString("N");
        var pipe = CreateServer(name);
        using var self = Process.GetCurrentProcess();
        stage?.Invoke("GuardianSpawn");
        var process = Program.Spawn("--guardian", name, self.Id.ToString(), self.StartTime.ToUniversalTime().Ticks.ToString());
        var session = new GuardianSession(target, pipe, process);
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            stage?.Invoke("GuardianPipeConnect");
            await pipe.WaitForConnectionAsync(timeout.Token);
            stage?.Invoke("GuardianHandshake");
            await session._writer.WriteLineAsync(Wire.Encode(target));
            var ack = Wire.Decode<GuardianReply>(await session._reader.ReadLineAsync(timeout.Token));
            if (!ack.Ok) throw new ProbeFailure(ack.ErrorCode ?? DiagnosticCode.GuardianRejected, ack.NativeHResult);
            return session;
        }
        catch
        {
            try { await session.DisposeAsync(); } catch (Exception) { /* Keep original startup error. */ }
            throw;
        }
    }

    internal static NamedPipeServerStream CreateServer(string name) => new(name, PipeDirection.InOut,
        1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

    internal async Task<string> SendAsync(GuardianCommand command)
    {
        await _commands.WaitAsync();
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_process.HasExited) throw new ProbeFailure(DiagnosticCode.GuardianExited);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _writer.WriteLineAsync(Wire.Encode(command).AsMemory(), timeout.Token);
            var reply = Wire.Decode<GuardianReply>(await _reader.ReadLineAsync(timeout.Token));
            if (!reply.Ok) throw new ProbeFailure(reply.ErrorCode ?? DiagnosticCode.GuardianRejected, reply.NativeHResult);
            return reply.Message;
        }
        catch
        {
            // Never permit a timed-out reply to be mistaken for a later command's reply.
            _disposed = true;
            _pipe.Dispose();
            try { Native.Restore(_target); } catch (Exception) { }
            throw;
        }
        finally { _commands.Release(); }
    }

    public async ValueTask DisposeAsync()
    {
        await _commands.WaitAsync();
        try
        {
            _disposed = true;
            _pipe.Dispose(); // guardian EOF => original-state restore, including normal shutdown
            if (!_process.HasExited)
            {
                try { await _process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(3)); }
                catch (TimeoutException) { Native.Restore(_target); /* Do not kill before restoring. */ }
            }
            _reader.Dispose();
            try { await _writer.DisposeAsync(); }
            catch (ObjectDisposedException) { }
            catch (IOException) { /* pipe already closed deliberately to signal guardian */ }
        }
        finally { _commands.Release(); }
    }
}
