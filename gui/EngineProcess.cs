using System.Diagnostics;
using System.Text;

namespace SdiConverter;

/// <summary>One run of sdi_converter_engine.exe: lines arrive on any thread, "stop" is sent on stdin.</summary>
public sealed class EngineProcess : IDisposable
{
    public static readonly string EnginePath = Path.Combine(AppContext.BaseDirectory, "sdi_converter_engine.exe");

    private readonly Process process;
    public event Action<string>? Line;
    public event Action<int>? Exited;
    public bool IsRunning => !process.HasExited;
    public Process Process => process;

    public EngineProcess(params string[] args)
    {
        if (!File.Exists(EnginePath)) throw new FileNotFoundException("The engine is missing from the application folder.", EnginePath);
        var start = new ProcessStartInfo(EnginePath)
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true, RedirectStandardInput = true,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8,
            StandardInputEncoding = new UTF8Encoding(false),
            WorkingDirectory = AppContext.BaseDirectory,
        };
        foreach (var arg in args) start.ArgumentList.Add(arg);
        process = new Process { StartInfo = start, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, e) => { if (e.Data is not null) Line?.Invoke(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) Line?.Invoke("Error: " + e.Data); };
    }

    public void Start()
    {
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        // Raised after the output streams are complete.
        process.Exited += (_, _) => { process.WaitForExit(); Exited?.Invoke(process.ExitCode); };
    }

    /// <summary>Asks the engine to stop; kills it if it does not end within the time limit.</summary>
    public async Task StopAsync(int milliseconds = 6000)
    {
        if (process.HasExited) return;
        try { process.StandardInput.WriteLine("stop"); process.StandardInput.Flush(); } catch (IOException) { }
        using var cts = new CancellationTokenSource(milliseconds);
        try { await process.WaitForExitAsync(cts.Token); }
        catch (OperationCanceledException) { try { process.Kill(true); } catch (InvalidOperationException) { } }
    }

    /// <summary>Runs a short query command and returns its output.</summary>
    public static async Task<string> QueryAsync(params string[] args)
    {
        using var engine = new EngineProcess(args);
        var text = new StringBuilder();
        var errors = new StringBuilder();
        engine.process.OutputDataReceived += (_, e) => { if (e.Data is not null) lock (text) text.AppendLine(e.Data); };
        engine.process.ErrorDataReceived += (_, e) => { if (e.Data is not null) lock (errors) errors.AppendLine(e.Data); };
        engine.process.Start();
        engine.process.BeginOutputReadLine();
        engine.process.BeginErrorReadLine();
        await engine.process.WaitForExitAsync();
        engine.process.WaitForExit();
        if (engine.process.ExitCode != 0) throw new InvalidOperationException(errors.ToString().Trim());
        return text.ToString();
    }

    public void Dispose() => process.Dispose();
}
