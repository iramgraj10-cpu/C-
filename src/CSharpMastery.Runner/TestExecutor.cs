using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using CSharpMastery.Domain.Items;
using CSharpMastery.Execution;

namespace CSharpMastery.Runner;

/// <summary>Ejecuta cada caso en un contexto de carga nuevo (los campos estáticos se reinician entre casos).</summary>
internal sealed class TestExecutor(byte[] assembly, byte[] pdb, CodeMode mode, ExecutionLimits limits)
{
    private const int StudentStackBytes = 16 * 1024 * 1024;

    public TestRunResult Run(ExecutionTest test, int index)
    {
        var writer = new LimitedWriter(limits.MaxOutputChars);
        var context = new AssemblyLoadContext("estudiante-" + index, isCollectible: true);
        object? returnValue = null;
        Exception? failure = null;

        try
        {
            var loaded = context.LoadFromStream(new MemoryStream(assembly), new MemoryStream(pdb));
            var originalOut = Console.Out;
            var originalIn = Console.In;
            var originalErr = Console.Error;
            var input = new StringReader(test.Input ?? "");
            Console.SetOut(ConsoleRoute.Writer(writer));
            Console.SetError(ConsoleRoute.Writer(writer));
            Console.SetIn(ConsoleRoute.Reader(input));

            var stopwatch = Stopwatch.StartNew();
            var thread = new Thread(() =>
            {
                ConsoleRoute.Bind(writer, input);
                try
                {
                    returnValue = mode == CodeMode.Program ? RunProgram(loaded) : RunCheck(loaded, index);
                }
                catch (Exception ex)
                {
                    failure = Unwrap(ex);
                }
            }, StudentStackBytes)
            {
                IsBackground = true,
                Name = "codigo-estudiante",
            };

            thread.Start();
            var finished = thread.Join(limits.TestTimeoutMs);
            stopwatch.Stop();

            if (finished)
            {
                Console.SetOut(originalOut);
                Console.SetError(originalErr);
                Console.SetIn(originalIn);
            }

            if (!finished)
            {
                return new TestRunResult
                {
                    Name = test.Name,
                    Output = writer.Captured,
                    TimedOut = true,
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    OutputLimitExceeded = writer.LimitExceeded,
                };
            }

            string? formatted = null;
            if (failure is null && mode == CodeMode.Function)
            {
                try
                {
                    formatted = ValueFormatter.Format(returnValue);
                }
                catch (Exception ex)
                {
                    failure = ex;
                }
            }

            return new TestRunResult
            {
                Name = test.Name,
                Output = writer.Captured,
                ReturnValue = formatted,
                Failure = failure is null || failure is OutputLimitExceededException ? null : Describe(failure),
                OutputLimitExceeded = writer.LimitExceeded,
                DurationMs = stopwatch.ElapsedMilliseconds,
            };
        }
        finally
        {
            context.Unload();
        }
    }

    private static object? RunProgram(Assembly loaded)
    {
        var entry = loaded.EntryPoint ?? throw new InvalidOperationException("El programa no tiene punto de entrada (Main o instrucciones de nivel superior).");
        var args = entry.GetParameters().Length == 1 ? new object[] { Array.Empty<string>() } : null;
        var result = entry.Invoke(null, args);
        if (result is Task task)
        {
            task.GetAwaiter().GetResult();
        }

        return null;
    }

    private static object? RunCheck(Assembly loaded, int index)
    {
        var harness = loaded.GetType(CodeCompiler.HarnessClassName)
            ?? throw new InvalidOperationException("No se encontró el arnés de pruebas.");
        var method = harness.GetMethod(HarnessBuilder.MethodName(index), BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException("No se encontró la prueba.");
        return method.Invoke(null, null);
    }

    private static Exception Unwrap(Exception ex)
    {
        while (ex is TargetInvocationException { InnerException: { } inner })
        {
            ex = inner;
        }

        return ex is AggregateException { InnerExceptions.Count: 1 } agg ? Unwrap(agg.InnerExceptions[0]) : ex;
    }

    private static RuntimeFailure Describe(Exception ex)
    {
        var chain = new List<string>();
        for (var t = ex.GetType(); t is not null && t != typeof(object); t = t.BaseType)
        {
            chain.Add(t.Name);
        }

        int? line = null;
        foreach (var frame in new StackTrace(ex, fNeedFileInfo: true).GetFrames())
        {
            if (frame.GetFileName() is { } file && Path.GetFileName(file) == CodeCompiler.StudentFileName)
            {
                line = frame.GetFileLineNumber();
                break;
            }
        }

        return new RuntimeFailure(ex.GetType().Name, chain, ex.Message, line);
    }
}
