using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;

namespace CSharpMastery.Execution;

public sealed record ExplainedDiagnostic(CompilerDiagnostic Raw, string Explanation, ErrorCategory Category);

/// <summary>Retroalimentación de un caso de prueba. En casos ocultos no se revelan entrada ni salida.</summary>
public sealed record TestFeedback(
    string Name,
    bool Passed,
    bool Hidden,
    bool EdgeCase,
    string? Input,
    string? Expected,
    string? Actual,
    string Message,
    string? Hint,
    ErrorCategory Error);

public sealed record GradingResult(
    ExecutionStatus Status,
    bool Passed,
    double Score,
    ErrorCategory Error,
    string Summary,
    IReadOnlyList<TestFeedback> Tests,
    IReadOnlyList<ExplainedDiagnostic> Diagnostics,
    IReadOnlyList<SecurityViolation> Violations,
    IReadOnlyList<CodeConstruct> MissingConstructs,
    string? ProgramOutput)
{
    /// <summary>False si no se pudo ejecutar el código (la corrección no es verificable).</summary>
    public bool Verified => Status is not (ExecutionStatus.Unavailable or ExecutionStatus.InternalError);

    public IEnumerable<ExplainedDiagnostic> Errors => Diagnostics.Where(d => d.Raw.Severity == "Error");
}

/// <summary>
/// Evalúa un ejercicio comparando el comportamiento del código con las pruebas, nunca el texto de la solución.
/// Cualquier solución alternativa que supere las pruebas aprueba.
/// </summary>
public static class CodeGrader
{
    public static ExecutionRequest BuildRequest(CodeExercise exercise, string code, ExecutionLimits? limits = null) => new()
    {
        Code = code,
        Mode = exercise.Mode,
        Tests = exercise.Tests.Select(t => new ExecutionTest(t.Name, t.Input, t.Check)).ToList(),
        Limits = limits ?? ExecutionLimits.Default,
    };

    /// <summary>Solicitud para el botón "Ejecutar": corre el programa una vez con la entrada indicada.</summary>
    public static ExecutionRequest BuildRunRequest(CodeExercise exercise, string code, string? stdin)
    {
        if (exercise.Mode == CodeMode.Program)
        {
            return new ExecutionRequest { Code = code, Mode = CodeMode.Program, Stdin = stdin };
        }

        var visible = exercise.Tests.Where(t => !t.Hidden).Select(t => new ExecutionTest(t.Name, t.Input, t.Check)).ToList();
        return new ExecutionRequest { Code = code, Mode = CodeMode.Function, Tests = visible };
    }

    public static GradingResult Grade(CodeExercise exercise, ExecutionResult result)
    {
        var explained = result.Diagnostics
            .Select(d =>
            {
                var e = DiagnosticExplainer.Explain(d);
                return new ExplainedDiagnostic(d, e.Text, e.Category);
            })
            .ToList();

        switch (result.Status)
        {
            case ExecutionStatus.Unavailable:
            case ExecutionStatus.InternalError:
                return Empty(result.Status, ErrorCategory.None, result.Message ?? "No fue posible ejecutar el código.");
            case ExecutionStatus.CompilationFailed:
            {
                var first = explained.FirstOrDefault(d => d.Raw.Severity == "Error");
                var category = first?.Category ?? ErrorCategory.Syntax;
                var where = first is { Raw.Line: > 0 } ? $" (línea {first.Raw.Line})" : "";
                return Empty(result.Status, category, $"El código no compila{where}: {first?.Explanation ?? "revisa los errores."}");
            }

            case ExecutionStatus.Rejected:
                return Empty(result.Status, ErrorCategory.ForbiddenApi,
                    "El código usa funciones que no están disponibles en los ejercicios (archivos, red, procesos o reflexión).");
            case ExecutionStatus.TimedOut:
                return Empty(result.Status, ErrorCategory.InfiniteLoopOrTimeout,
                    "La ejecución tardó demasiado. ¿Hay un bucle cuya condición nunca se vuelve falsa?");
        }

        var feedback = new List<TestFeedback>();
        for (var i = 0; i < exercise.Tests.Count; i++)
        {
            var test = exercise.Tests[i];
            var run = result.Tests.FirstOrDefault(r => r.Name == test.Name) ?? (i < result.Tests.Count ? result.Tests[i] : null);
            feedback.Add(GradeTest(exercise.Mode, test, run));
        }

        var missing = exercise.RequiredConstructs.Where(c => !result.Constructs.Contains(c)).ToList();
        var passedCount = feedback.Count(f => f.Passed);
        var score = exercise.Tests.Count == 0 ? 0 : (double)passedCount / exercise.Tests.Count;
        var allPass = feedback.Count > 0 && passedCount == feedback.Count;
        var passed = allPass && missing.Count == 0;

        ErrorCategory error;
        string summary;
        if (passed)
        {
            error = ErrorCategory.None;
            summary = $"¡Correcto! Superaste las {feedback.Count} pruebas.";
        }
        else if (allPass)
        {
            error = ErrorCategory.MissingConstruct;
            summary = "Las pruebas pasan, pero el ejercicio pide usar: " + string.Join(", ", missing.Select(ConstructName)) + ".";
        }
        else
        {
            var failures = feedback.Where(f => !f.Passed).ToList();
            var onlyEdge = failures.All(f => f.EdgeCase) && feedback.Any(f => f.Passed && !f.EdgeCase);
            error = onlyEdge && failures.All(f => f.Error is ErrorCategory.WrongLogic)
                ? ErrorCategory.EdgeCaseMissed
                : failures[0].Error;
            summary = $"Superaste {passedCount} de {feedback.Count} pruebas. {failures[0].Message}";
        }

        var output = exercise.Mode == CodeMode.Program ? result.Tests.FirstOrDefault()?.Output : null;
        return new GradingResult(result.Status, passed, score, error, summary, feedback, explained, result.Violations, missing, output);

        GradingResult Empty(ExecutionStatus status, ErrorCategory category, string summary) =>
            new(status, false, 0, category, summary,
                exercise.Tests.Select(t => new TestFeedback(t.Name, false, t.Hidden, t.EdgeCase, null, null, null, "No se ejecutó.", null, category)).ToList(),
                explained, result.Violations, [], null);
    }

    private static TestFeedback GradeTest(CodeMode mode, CodeTestCase test, TestRunResult? run)
    {
        var input = test.Hidden ? null : mode == CodeMode.Program ? test.Input : test.Check;
        var expected = test.Hidden ? null : test.ExpectedException is { } ex ? $"Excepción {ex}" : test.Expected;

        TestFeedback Fail(string message, ErrorCategory category, string? actual = null) =>
            new(test.Name, false, test.Hidden, test.EdgeCase, input, expected, test.Hidden ? null : actual, message, test.FailureHint, category);

        if (run is null || run.NotRun)
        {
            return Fail("No se ejecutó porque una prueba anterior se detuvo.", ErrorCategory.InfiniteLoopOrTimeout);
        }

        if (run.TimedOut)
        {
            return Fail("Tardó demasiado: posible bucle infinito o algoritmo muy lento.", ErrorCategory.InfiniteLoopOrTimeout, run.Output);
        }

        if (run.OutputLimitExceeded)
        {
            return Fail("Produjo demasiada salida: posible bucle infinito.", ErrorCategory.InfiniteLoopOrTimeout);
        }

        if (run.Failure is { } failure)
        {
            if (test.ExpectedException is { } expectedEx && failure.TypeChain.Contains(expectedEx))
            {
                return Pass();
            }

            var explanation = DiagnosticExplainer.ExplainException(failure);
            var where = failure.Line is { } line ? $" en la línea {line}" : "";
            return Fail($"Se lanzó {failure.ExceptionType}{where}. {explanation.Text}", explanation.Category, $"{failure.ExceptionType}: {failure.Message}");
        }

        if (test.ExpectedException is { } missingEx)
        {
            return Fail($"Se esperaba que el código lanzara {missingEx} para esta entrada, pero no lo hizo.", ErrorCategory.EdgeCaseMissed,
                mode == CodeMode.Function ? run.ReturnValue : run.Output);
        }

        if (mode == CodeMode.Program)
        {
            var diff = OutputComparer.FirstDifference(test.Expected, run.Output);
            if (diff is null)
            {
                return Pass();
            }

            var message = test.Hidden
                ? "La salida no coincide con lo esperado."
                : diff.FormatOnly
                    ? $"La línea {diff.Line} casi coincide: solo difieren espacios o mayúsculas."
                    : $"La línea {diff.Line} es distinta. Se esperaba «{diff.Expected ?? "(nada)"}» y se obtuvo «{diff.Actual ?? "(nada)"}».";
            return Fail(message, diff.FormatOnly ? ErrorCategory.OutputFormat : ErrorCategory.WrongLogic, run.Output);
        }

        var actualValue = (run.ReturnValue ?? "").Trim();
        if (actualValue == test.Expected.Trim())
        {
            return Pass();
        }

        var msg = test.Hidden
            ? "El valor devuelto no es el esperado."
            : $"Se esperaba {test.Expected} y se obtuvo {actualValue}.";
        return Fail(msg, ErrorCategory.WrongLogic, actualValue);

        TestFeedback Pass() => new(test.Name, true, test.Hidden, test.EdgeCase, input, expected,
            test.Hidden ? null : mode == CodeMode.Function ? run.ReturnValue : run.Output, "Correcto.", null, ErrorCategory.None);
    }

    public static string ConstructName(CodeConstruct construct) => construct switch
    {
        CodeConstruct.Loop => "un bucle",
        CodeConstruct.Conditional => "una condición",
        CodeConstruct.Method => "un método",
        CodeConstruct.Recursion => "recursividad",
        CodeConstruct.Class => "una clase",
        CodeConstruct.Interface => "una interfaz",
        CodeConstruct.Record => "un record",
        CodeConstruct.Lambda => "una expresión lambda",
        CodeConstruct.Linq => "LINQ",
        CodeConstruct.TryCatch => "try/catch",
        CodeConstruct.Async => "async/await",
        CodeConstruct.Switch => "switch",
        CodeConstruct.Generic => "genéricos",
        _ => construct.ToString(),
    };
}
