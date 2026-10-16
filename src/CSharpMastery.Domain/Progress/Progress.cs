namespace CSharpMastery.Domain.Progress;

/// <summary>
/// Estado de dominio de una habilidad. El orden importa: cada estado exige más evidencia que el anterior.
/// </summary>
public enum MasteryLevel
{
    /// <summary>Aún no se ha visto.</summary>
    NotStarted = 0,
    /// <summary>Se leyó la lección, pero no se ha practicado.</summary>
    Seen = 1,
    /// <summary>Se practicó, pero aún no hay evidencia suficiente de dominio.</summary>
    Practiced = 2,
    /// <summary>Superó los criterios de precisión e independencia en la misma etapa.</summary>
    ProvisionallyMastered = 3,
    /// <summary>Además, la recordó correctamente en un repaso posterior tras un intervalo.</summary>
    Retained = 4,
}

/// <summary>Categoría de error registrada en cada intento fallido.</summary>
public enum ErrorCategory
{
    None = 0,
    Syntax,
    UndeclaredIdentifier,
    TypeMismatch,
    UninitializedVariable,
    Scope,
    StaticVersusInstance,
    AccessModifier,
    InheritanceContract,
    AsyncMisuse,
    MissingReturn,
    WrongArguments,
    OperatorMisuse,
    NullReference,
    IndexOutOfRange,
    DivideByZero,
    ParsingFormat,
    InfiniteLoopOrTimeout,
    StackOverflowRecursion,
    OffByOne,
    WrongLogic,
    EdgeCaseMissed,
    OutputFormat,
    ConceptMisunderstanding,
    PredictionError,
    ForbiddenApi,
    MissingConstruct,
    UnhandledException,
    Other,
}

/// <summary>Estado de repetición espaciada de una habilidad.</summary>
public sealed record ReviewSchedule(
    double EaseFactor,
    int IntervalDays,
    int Repetitions,
    int Lapses,
    DateTimeOffset? DueAt,
    DateTimeOffset? LastReviewedAt)
{
    public const double DefaultEase = 2.5;

    public static ReviewSchedule New { get; } = new(DefaultEase, 0, 0, 0, null, null);

    public bool IsScheduled => DueAt is not null;

    public bool IsDue(DateTimeOffset now) => DueAt is { } due && due <= now;
}

/// <summary>Progreso del estudiante en una habilidad.</summary>
public sealed record SkillProgress(
    string SkillId,
    MasteryLevel Level,
    ReviewSchedule Review,
    int Difficulty,
    DateTimeOffset? SeenAt,
    DateTimeOffset? FirstPracticedAt,
    DateTimeOffset? ProvisionalMasteryAt,
    DateTimeOffset? RetainedAt,
    bool NeedsReinforcement,
    DateTimeOffset? ReinforcementSince = null,
    DateTimeOffset? DifficultyChangedAt = null)
{
    public const int MinDifficulty = 1;
    public const int MaxDifficulty = 5;

    public static SkillProgress Initial(string skillId) =>
        new(skillId, MasteryLevel.NotStarted, ReviewSchedule.New, MinDifficulty, null, null, null, null, false);
}

/// <summary>Dónde ocurrió un intento.</summary>
public enum AttemptContext
{
    Lesson,
    Session,
    Review,
    Assessment,
    Project,
    FreePractice,
}

/// <summary>Registro inmutable de un intento de resolver un ítem.</summary>
public sealed record AttemptRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string ItemId { get; init; }
    public required string SkillId { get; init; }
    public IReadOnlyList<string> RelatedSkillIds { get; init; } = [];
    public required Items.ItemPurpose Purpose { get; init; }
    public required AttemptContext Context { get; init; }
    public string? ContextId { get; init; }
    public required bool Correct { get; init; }
    /// <summary>Puntuación de 0 a 1 (p. ej. fracción de pruebas superadas).</summary>
    public double Score { get; init; }
    public int HintsUsed { get; init; }
    /// <summary>Número de comprobaciones hasta resolverlo o rendirse (solo código; en preguntas cuenta el primer intento).</summary>
    public int Tries { get; init; } = 1;
    public bool SolutionViewed { get; init; }
    public TimeSpan Duration { get; init; }
    /// <summary>Confianza declarada (1 = adivinando, 4 = totalmente seguro), si se preguntó.</summary>
    public int? Confidence { get; init; }
    public ErrorCategory Error { get; init; }
    /// <summary>True si la corrección fue automática y verificable (no autoevaluación).</summary>
    public bool AutoGraded { get; init; } = true;
    public bool IsCode { get; init; }
    public required DateTimeOffset At { get; init; }
    public string? Answer { get; init; }

    /// <summary>Resuelto sin pistas ni solución.</summary>
    public bool Independent => HintsUsed == 0 && !SolutionViewed;
}

/// <summary>Criterios configurables para declarar dominio. Son parámetros del producto, no umbrales científicos universales.</summary>
public sealed record MasteryCriteria
{
    /// <summary>Precisión mínima en la ventana de intentos recientes.</summary>
    public double MinAccuracy { get; init; } = 0.85;
    /// <summary>Número mínimo de intentos calificados en la ventana.</summary>
    public int MinGradedAttempts { get; init; } = 6;
    /// <summary>Tamaño de la ventana de intentos recientes considerada.</summary>
    public int AccuracyWindow { get; init; } = 12;
    /// <summary>Soluciones de código correctas e independientes requeridas (si la habilidad tiene ejercicios de código).</summary>
    public int MinIndependentCodeSolutions { get; init; } = 1;
    /// <summary>Problemas de transferencia correctos e independientes requeridos (si la habilidad los tiene).</summary>
    public int MinTransferSolutions { get; init; } = 1;
    /// <summary>Días mínimos tras el dominio provisional para que un repaso cuente como retención.</summary>
    public int RetentionMinDays { get; init; } = 3;
    /// <summary>Precisión mínima para aprobar una evaluación.</summary>
    public double AssessmentPassAccuracy { get; init; } = 0.85;

    public static MasteryCriteria Default { get; } = new();

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (MinAccuracy is <= 0 or > 1) errors.Add("La precisión mínima debe estar entre 0 y 1.");
        if (AssessmentPassAccuracy is <= 0 or > 1) errors.Add("La precisión para aprobar evaluaciones debe estar entre 0 y 1.");
        if (MinGradedAttempts < 1) errors.Add("Se requiere al menos un intento calificado.");
        if (AccuracyWindow < MinGradedAttempts) errors.Add("La ventana debe ser mayor o igual al mínimo de intentos.");
        if (MinIndependentCodeSolutions < 0 || MinTransferSolutions < 0) errors.Add("Los mínimos no pueden ser negativos.");
        if (RetentionMinDays < 1) errors.Add("El intervalo de retención debe ser de al menos un día.");
        return errors;
    }
}

/// <summary>Una comprobación de código enviada (historial de intentos de un ejercicio).</summary>
public sealed record CodeSubmission(
    Guid Id,
    string ItemId,
    string Code,
    bool Passed,
    double Score,
    ErrorCategory Error,
    string Summary,
    DateTimeOffset At);
