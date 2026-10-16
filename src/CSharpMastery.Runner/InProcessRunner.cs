using CSharpMastery.Domain.Items;
using CSharpMastery.Execution;

namespace CSharpMastery.Runner;

/// <summary>
/// Ejecuta una solicitud dentro del proceso actual. Solo para herramientas y pruebas automatizadas
/// del banco de contenidos (código de confianza). La aplicación siempre usa el proceso separado.
/// </summary>
public static class InProcessRunner
{
    private static readonly Lock Gate = new();

    public static ExecutionResult Execute(ExecutionRequest request)
    {
        lock (Gate)
        {
            var compilation = CodeCompiler.Compile(request);
            if (compilation.Violations.Count > 0)
            {
                return new ExecutionResult
                {
                    Status = ExecutionStatus.Rejected,
                    Diagnostics = compilation.Diagnostics,
                    Violations = compilation.Violations,
                    Constructs = compilation.Constructs,
                };
            }

            if (compilation.Assembly is null)
            {
                return new ExecutionResult
                {
                    Status = ExecutionStatus.CompilationFailed,
                    Diagnostics = compilation.Diagnostics,
                    Constructs = compilation.Constructs,
                };
            }

            var tests = request.Tests.Count > 0 ? request.Tests : [new ExecutionTest("Ejecución", request.Stdin, null)];
            var executor = new TestExecutor(compilation.Assembly, compilation.Pdb!, request.Mode, request.Limits);
            var results = new List<TestRunResult>();
            var stdout = Console.Out;
            var stdin = Console.In;
            var stderr = Console.Error;
            try
            {
                for (var i = 0; i < tests.Count; i++)
                {
                    var result = executor.Run(tests[i], i);
                    results.Add(result);
                    if (result.TimedOut)
                    {
                        results.AddRange(tests.Skip(i + 1).Select(t => new TestRunResult { Name = t.Name, NotRun = true }));
                        break;
                    }
                }
            }
            finally
            {
                Console.SetOut(stdout);
                Console.SetIn(stdin);
                Console.SetError(stderr);
            }

            return new ExecutionResult
            {
                Status = ExecutionStatus.Completed,
                Diagnostics = compilation.Diagnostics,
                Constructs = compilation.Constructs,
                Tests = results,
            };
        }
    }

    public static ExecutionResult RunProgram(string code, string? stdin = null) =>
        Execute(new ExecutionRequest { Code = code, Mode = CodeMode.Program, Stdin = stdin });
}
