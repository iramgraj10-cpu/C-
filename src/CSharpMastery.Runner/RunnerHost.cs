using CSharpMastery.Domain.Items;
using CSharpMastery.Execution;

namespace CSharpMastery.Runner;

/// <summary>Orquesta compilación, análisis de seguridad y ejecución de casos.</summary>
internal sealed class RunnerHost(ProtocolWriter protocol)
{
    public int Run(ExecutionRequest request)
    {
        var compilation = CodeCompiler.Compile(request);

        if (compilation.Violations.Count > 0)
        {
            protocol.Write(new RunnerMessage
            {
                Type = RunnerMessage.Compile,
                Status = ExecutionStatus.Rejected,
                Diagnostics = compilation.Diagnostics,
                Violations = compilation.Violations,
                Constructs = compilation.Constructs,
            });
            protocol.Write(new RunnerMessage { Type = RunnerMessage.Done, Status = ExecutionStatus.Rejected });
            return 0;
        }

        if (compilation.Assembly is null)
        {
            protocol.Write(new RunnerMessage
            {
                Type = RunnerMessage.Compile,
                Status = ExecutionStatus.CompilationFailed,
                Diagnostics = compilation.Diagnostics,
                Constructs = compilation.Constructs,
            });
            protocol.Write(new RunnerMessage { Type = RunnerMessage.Done, Status = ExecutionStatus.CompilationFailed });
            return 0;
        }

        protocol.Write(new RunnerMessage
        {
            Type = RunnerMessage.Compile,
            Status = ExecutionStatus.Completed,
            Diagnostics = compilation.Diagnostics,
            Constructs = compilation.Constructs,
        });

        var tests = request.Tests.Count > 0
            ? request.Tests
            : [new ExecutionTest("Ejecución", request.Stdin, null)];

        var executor = new TestExecutor(compilation.Assembly, compilation.Pdb!, request.Mode, request.Limits);
        for (var i = 0; i < tests.Count; i++)
        {
            var result = executor.Run(tests[i], i);
            protocol.Write(new RunnerMessage { Type = RunnerMessage.TestResult, Test = result });

            if (result.TimedOut)
            {
                // No se puede abortar un hilo en .NET moderno: se marca el resto como no ejecutado y se termina el proceso.
                for (var j = i + 1; j < tests.Count; j++)
                {
                    protocol.Write(new RunnerMessage
                    {
                        Type = RunnerMessage.TestResult,
                        Test = new TestRunResult { Name = tests[j].Name, NotRun = true },
                    });
                }

                protocol.Write(new RunnerMessage { Type = RunnerMessage.Done, Status = ExecutionStatus.Completed });
                return 3;
            }
        }

        protocol.Write(new RunnerMessage { Type = RunnerMessage.Done, Status = ExecutionStatus.Completed });
        return 0;
    }
}

internal sealed record CompilationOutcome(
    byte[]? Assembly,
    byte[]? Pdb,
    IReadOnlyList<CompilerDiagnostic> Diagnostics,
    IReadOnlyList<SecurityViolation> Violations,
    IReadOnlyList<CodeConstruct> Constructs);
