using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace CSharpMastery.Execution;

/// <summary>Cómo lanzar el proceso ejecutor.</summary>
public sealed record RunnerLaunchInfo(string FileName, IReadOnlyList<string> Arguments)
{
    public const string RunnerAssemblyName = "CSharpMastery.Runner";

    /// <summary>Busca el ejecutor en un directorio (ejecutable nativo o .dll para el host dotnet).</summary>
    public static RunnerLaunchInfo? Locate(params string[] directories)
    {
        foreach (var dir in directories)
        {
            foreach (var candidate in new[] { dir, Path.Combine(dir, "runner") })
            {
                var exe = Path.Combine(candidate, RunnerAssemblyName + (OperatingSystem.IsWindows() ? ".exe" : ""));
                var dll = Path.Combine(candidate, RunnerAssemblyName + ".dll");
                if (File.Exists(dll) && File.Exists(exe) && !OperatingSystem.IsWindows())
                {
                    return ForDll(dll);
                }

                if (File.Exists(exe))
                {
                    return new RunnerLaunchInfo(exe, []);
                }

                if (File.Exists(dll))
                {
                    return ForDll(dll);
                }
            }
        }

        return null;
    }

    public static RunnerLaunchInfo ForDll(string dllPath)
    {
        var host = Environment.ProcessPath is { } p && Path.GetFileNameWithoutExtension(p) == "dotnet" ? p : "dotnet";
        return new RunnerLaunchInfo(host, [dllPath]);
    }
}

/// <summary>
/// Ejecuta el código del estudiante en un proceso separado de un solo uso. Aplica límite de tiempo total
/// (mata el árbol de procesos), límite de memoria administrada (DOTNET_GCHeapHardLimit) y un entorno mínimo.
/// Mantiene un proceso precalentado para reducir la latencia de la siguiente ejecución.
/// </summary>
public sealed class ProcessExecutionService : IExecutionService, IDisposable
{
    private readonly RunnerLaunchInfo _launch;
    private readonly bool _prewarm;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private RunnerProcess? _warm;

    public ProcessExecutionService(RunnerLaunchInfo launch, bool prewarm = true)
    {
        _launch = launch;
        _prewarm = prewarm;
        if (prewarm)
        {
            _warm = TryStart(ExecutionLimits.Default);
        }
    }

    public bool IsAvailable => true;

    public string AvailabilityMessage => "Los ejercicios se compilan y ejecutan en un proceso separado de este equipo, sin conexión.";

    public async Task<ExecutionResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var runner = TakeWarm(request.Limits) ?? TryStart(request.Limits);
            if (runner is null)
            {
                return ExecutionResult.Unavailable("No se pudo iniciar el motor de ejecución.");
            }

            try
            {
                return await runner.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                runner.Dispose();
                if (_prewarm)
                {
                    _warm = TryStart(ExecutionLimits.Default);
                }
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private RunnerProcess? TakeWarm(ExecutionLimits limits)
    {
        var warm = _warm;
        _warm = null;
        if (warm is null)
        {
            return null;
        }

        if (warm.HasExited || warm.HeapMegabytes != limits.MaxHeapMegabytes)
        {
            warm.Dispose();
            return null;
        }

        return warm;
    }

    private RunnerProcess? TryStart(ExecutionLimits limits)
    {
        try
        {
            return RunnerProcess.Start(_launch, limits.MaxHeapMegabytes);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public void Dispose()
    {
        _warm?.Dispose();
        _warm = null;
        _gate.Dispose();
    }
}

internal sealed class RunnerProcess : IDisposable
{
    private readonly Process _process;
    private readonly string _workDir;
    private readonly StringBuilder _stderr = new();

    private RunnerProcess(Process process, string workDir, int heapMegabytes)
    {
        _process = process;
        _workDir = workDir;
        HeapMegabytes = heapMegabytes;
    }

    public int HeapMegabytes { get; }

    public bool HasExited
    {
        get
        {
            try
            {
                return _process.HasExited;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
        }
    }

    public static RunnerProcess Start(RunnerLaunchInfo launch, int heapMegabytes)
    {
        var workDir = Path.Combine(Path.GetTempPath(), "csm-run-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);

        var psi = new ProcessStartInfo(launch.FileName)
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workDir,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        foreach (var arg in launch.Arguments)
        {
            psi.ArgumentList.Add(arg);
        }

        // Entorno mínimo: solo lo necesario para que arranque el runtime de .NET.
        var keep = new[] { "PATH", "DOTNET_ROOT", "DOTNET_ROOT_X64", "DOTNET_ROOT_ARM64", "SYSTEMROOT", "WINDIR", "HOME", "TMPDIR", "TEMP", "TMP" };
        var preserved = keep.Select(k => (k, v: Environment.GetEnvironmentVariable(k))).Where(x => x.v is not null).ToList();
        psi.Environment.Clear();
        foreach (var (k, v) in preserved)
        {
            psi.Environment[k] = v;
        }

        psi.Environment["DOTNET_GCHeapHardLimit"] = ((long)heapMegabytes * 1024 * 1024).ToString("X", CultureInfo.InvariantCulture);
        psi.Environment["DOTNET_gcServer"] = "0";
        psi.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        psi.Environment["DOTNET_NOLOGO"] = "1";

        var process = Process.Start(psi) ?? throw new InvalidOperationException("No se pudo iniciar el proceso ejecutor.");
        var runner = new RunnerProcess(process, workDir, heapMegabytes);
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                lock (runner._stderr)
                {
                    if (runner._stderr.Length < 8000)
                    {
                        runner._stderr.AppendLine(e.Data);
                    }
                }
            }
        };
        process.BeginErrorReadLine();
        return runner;
    }

    public async Task<ExecutionResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        // El arranque del proceso y el precalentamiento del compilador no cuentan como tiempo del ejercicio:
        // en un equipo lento (o recién encendido) pueden tardar más que el límite y el estudiante vería
        // «tardó demasiado» con una solución correcta. El límite empieza a correr cuando el ejecutor está listo.
        if (await WaitReadyAsync(cancellationToken).ConfigureAwait(false) is { } notReady)
        {
            return notReady;
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(request.Limits.TotalTimeoutMs);

        var diagnostics = new List<CompilerDiagnostic>();
        var violations = new List<SecurityViolation>();
        var constructs = new List<Domain.Items.CodeConstruct>();
        var tests = new List<TestRunResult>();
        ExecutionStatus? compileStatus = null;
        ExecutionStatus? finalStatus = null;
        string? message = null;

        try
        {
            var json = JsonSerializer.Serialize(request, ProtocolJson.Options);
            await _process.StandardInput.WriteAsync(json.AsMemory(), timeout.Token).ConfigureAwait(false);
            _process.StandardInput.Close();

            while (true)
            {
                var line = await _process.StandardOutput.ReadLineAsync(timeout.Token).ConfigureAwait(false);
                if (line is null)
                {
                    break;
                }

                RunnerMessage? msg;
                try
                {
                    msg = JsonSerializer.Deserialize<RunnerMessage>(line, ProtocolJson.Options);
                }
                catch (JsonException)
                {
                    continue;
                }

                switch (msg?.Type)
                {
                    case RunnerMessage.Compile:
                        compileStatus = msg.Status;
                        diagnostics.AddRange(msg.Diagnostics ?? []);
                        violations.AddRange(msg.Violations ?? []);
                        constructs.AddRange(msg.Constructs ?? []);
                        break;
                    case RunnerMessage.TestResult when msg.Test is not null:
                        tests.Add(msg.Test);
                        break;
                    case RunnerMessage.Done:
                        finalStatus = msg.Status;
                        message = msg.Message;
                        break;
                }

                if (finalStatus is not null)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            Kill();
            // En Windows, un desbordamiento de pila puede tardar en cerrar el proceso (informe de errores del
            // sistema) y agotar el tiempo: si stderr ya lo reporta, se trata como caída y no como bucle infinito.
            if (!IsStackOverflow(Stderr()))
            {
                return Build(ExecutionStatus.TimedOut, "Se superó el tiempo máximo de ejecución.");
            }
        }
        catch (IOException)
        {
            // El proceso terminó mientras se le escribía; se analiza abajo como caída.
        }

        if (finalStatus is null)
        {
            try
            {
                await _process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Kill();
            }

            var stackOverflow = IsStackOverflow(Stderr()) || ExitedWithStackOverflow();
            var expected = request.Tests.Count > 0 ? request.Tests.Select(t => t.Name).ToList() : ["Ejecución"];
            // Si el mensaje de compilación no llegó pero el proceso cayó por la pila, el código sí compiló y se ejecutó.
            if (compileStatus is null && stackOverflow)
            {
                compileStatus = ExecutionStatus.Completed;
            }

            if (compileStatus == ExecutionStatus.Completed && tests.Count < expected.Count)
            {
                tests.Add(new TestRunResult
                {
                    Name = expected[tests.Count],
                    Failure = stackOverflow
                        ? new RuntimeFailure("StackOverflowException", ["StackOverflowException", "SystemException", "Exception"], "Desbordamiento de pila.", null)
                        : new RuntimeFailure("ProcessCrash", ["ProcessCrash"], "El proceso del ejercicio terminó de forma inesperada.", null),
                });
                while (tests.Count < expected.Count)
                {
                    tests.Add(new TestRunResult { Name = expected[tests.Count], NotRun = true });
                }
            }

            return Build(compileStatus == ExecutionStatus.Completed ? ExecutionStatus.Completed : ExecutionStatus.Crashed,
                stackOverflow ? "Desbordamiento de pila: el proceso del ejercicio se detuvo." : "El proceso ejecutor terminó de forma inesperada.");
        }

        return Build(finalStatus.Value, message);

        ExecutionResult Build(ExecutionStatus status, string? msg) => new()
        {
            Status = status,
            Diagnostics = diagnostics,
            Violations = violations,
            Constructs = constructs.Distinct().ToList(),
            Tests = tests,
            Message = msg,
            DurationMs = stopwatch.ElapsedMilliseconds,
        };
    }

    /// <summary>Máximo para que el ejecutor arranque y precaliente el compilador.</summary>
    public const int StartupTimeoutMs = 120_000;

    /// <summary>Espera el mensaje «ready» del ejecutor; devuelve un resultado solo si no llegó.</summary>
    private async Task<ExecutionResult?> WaitReadyAsync(CancellationToken cancellationToken)
    {
        using var startup = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        startup.CancelAfter(StartupTimeoutMs);
        try
        {
            while (await _process.StandardOutput.ReadLineAsync(startup.Token).ConfigureAwait(false) is { } line)
            {
                try
                {
                    if (JsonSerializer.Deserialize<RunnerMessage>(line, ProtocolJson.Options)?.Type == RunnerMessage.Ready)
                    {
                        return null;
                    }
                }
                catch (JsonException)
                {
                }
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            Kill();
        }

        return ExecutionResult.Unavailable("El motor de ejecución no pudo iniciarse. Inténtalo de nuevo.");
    }

    private static bool IsStackOverflow(string stderr) => stderr.Contains("Stack overflow", StringComparison.OrdinalIgnoreCase);

    /// <summary>0xC00000FD (STATUS_STACK_OVERFLOW) es el código de salida de Windows para un desbordamiento de pila.</summary>
    private bool ExitedWithStackOverflow()
    {
        try
        {
            return _process.HasExited && unchecked((uint)_process.ExitCode) == 0xC00000FD;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private string Stderr()
    {
        lock (_stderr)
        {
            return _stderr.ToString();
        }
    }

    private void Kill()
    {
        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    public void Dispose()
    {
        Kill();
        _process.Dispose();
        try
        {
            Directory.Delete(_workDir, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
