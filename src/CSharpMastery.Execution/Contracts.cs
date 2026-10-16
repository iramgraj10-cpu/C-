using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpMastery.Domain.Items;

namespace CSharpMastery.Execution;

/// <summary>Límites aplicados a cada ejecución de código del estudiante.</summary>
public sealed record ExecutionLimits
{
    /// <summary>Tiempo máximo por caso de prueba.</summary>
    public int TestTimeoutMs { get; init; } = 3000;
    /// <summary>Máximo de caracteres de salida por caso.</summary>
    public int MaxOutputChars { get; init; } = 20_000;
    /// <summary>Límite de memoria administrada del proceso ejecutor (GCHeapHardLimit).</summary>
    public int MaxHeapMegabytes { get; init; } = 256;
    /// <summary>Tiempo máximo total, incluida la compilación.</summary>
    public int TotalTimeoutMs { get; init; } = 30_000;

    public static ExecutionLimits Default { get; } = new();
}

public sealed record ExecutionTest(string Name, string? Input, string? Check);

/// <summary>Solicitud que se envía al proceso ejecutor.</summary>
public sealed record ExecutionRequest
{
    public required string Code { get; init; }
    public required CodeMode Mode { get; init; }
    /// <summary>Casos a ejecutar. En modo Program sin pruebas, se ejecuta una vez con <see cref="Stdin"/>.</summary>
    public IReadOnlyList<ExecutionTest> Tests { get; init; } = [];
    public string? Stdin { get; init; }
    public ExecutionLimits Limits { get; init; } = ExecutionLimits.Default;
}

public enum ExecutionStatus
{
    /// <summary>Se compiló y se ejecutaron los casos (pueden haber fallado).</summary>
    Completed,
    CompilationFailed,
    /// <summary>El código usa API no permitidas en el entorno de ejercicios.</summary>
    Rejected,
    /// <summary>El proceso ejecutor terminó de forma anormal (p. ej. desbordamiento de pila).</summary>
    Crashed,
    /// <summary>Se superó el tiempo total.</summary>
    TimedOut,
    /// <summary>No hay motor de ejecución en esta plataforma o no se encontró.</summary>
    Unavailable,
    InternalError,
}

public sealed record CompilerDiagnostic(string Id, string Severity, string Message, int Line, int Column);

public sealed record SecurityViolation(int Line, string Api, string Reason);

public sealed record RuntimeFailure(string ExceptionType, IReadOnlyList<string> TypeChain, string Message, int? Line);

/// <summary>Resultado de ejecutar un caso.</summary>
public sealed record TestRunResult
{
    public required string Name { get; init; }
    public string Output { get; init; } = "";
    /// <summary>Valor devuelto formateado (modo Function).</summary>
    public string? ReturnValue { get; init; }
    public RuntimeFailure? Failure { get; init; }
    public bool TimedOut { get; init; }
    public bool OutputLimitExceeded { get; init; }
    public bool NotRun { get; init; }
    public long DurationMs { get; init; }
}

public sealed record ExecutionResult
{
    public required ExecutionStatus Status { get; init; }
    public IReadOnlyList<CompilerDiagnostic> Diagnostics { get; init; } = [];
    public IReadOnlyList<SecurityViolation> Violations { get; init; } = [];
    public IReadOnlyList<TestRunResult> Tests { get; init; } = [];
    public IReadOnlyList<CodeConstruct> Constructs { get; init; } = [];
    public string? Message { get; init; }
    public long DurationMs { get; init; }

    public IEnumerable<CompilerDiagnostic> Errors => Diagnostics.Where(d => d.Severity == "Error");

    public static ExecutionResult Unavailable(string message) => new() { Status = ExecutionStatus.Unavailable, Message = message };
}

/// <summary>Línea del protocolo entre la aplicación y el proceso ejecutor (JSON por línea).</summary>
public sealed record RunnerMessage
{
    public required string Type { get; init; }
    public ExecutionStatus? Status { get; init; }
    public IReadOnlyList<CompilerDiagnostic>? Diagnostics { get; init; }
    public IReadOnlyList<SecurityViolation>? Violations { get; init; }
    public IReadOnlyList<CodeConstruct>? Constructs { get; init; }
    public TestRunResult? Test { get; init; }
    public string? Message { get; init; }

    public const string Ready = "ready";
    public const string Compile = "compile";
    public const string TestResult = "test";
    public const string Done = "done";
}

public static class ProtocolJson
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };
}

/// <summary>Servicio que compila y ejecuta código del estudiante fuera del proceso principal.</summary>
public interface IExecutionService
{
    /// <summary>Indica si en esta plataforma se puede ejecutar código.</summary>
    bool IsAvailable { get; }

    /// <summary>Explicación para el estudiante cuando no está disponible.</summary>
    string AvailabilityMessage { get; }

    Task<ExecutionResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default);
}

/// <summary>Implementación para plataformas sin motor de ejecución (p. ej. Android/iOS en esta versión).</summary>
public sealed class UnavailableExecutionService(string message) : IExecutionService
{
    public bool IsAvailable => false;

    public string AvailabilityMessage { get; } = message;

    public Task<ExecutionResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult(ExecutionResult.Unavailable(AvailabilityMessage));
}
