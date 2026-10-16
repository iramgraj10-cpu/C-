namespace CSharpMastery.Domain.Sessions;

/// <summary>Bloques de una sesión de estudio.</summary>
public enum SessionBlockKind
{
    /// <summary>Recuperación activa de lo aprendido antes.</summary>
    Recall,
    /// <summary>Explicación breve del concepto nuevo.</summary>
    NewConcept,
    /// <summary>Ejemplo guiado y predicción de código.</summary>
    GuidedExample,
    /// <summary>Ejercicios de programación.</summary>
    Practice,
    /// <summary>Problema de aplicación sin ayuda.</summary>
    Application,
    /// <summary>Evaluación breve, registro de errores y plan de repasos.</summary>
    WrapUp,
}

public enum ActivityKind
{
    /// <summary>Leer la explicación de una lección (RefId = id de habilidad).</summary>
    ReadLesson,
    /// <summary>Estudiar el ejemplo resuelto (RefId = id de habilidad).</summary>
    StudyWorkedExample,
    /// <summary>Resolver un ítem (RefId = id de ítem).</summary>
    Item,
    /// <summary>Resumen de cierre de sesión.</summary>
    Summary,
}

public sealed record SessionActivity(
    int Index,
    SessionBlockKind Block,
    ActivityKind Kind,
    string RefId,
    double EstimatedMinutes,
    bool AskConfidence,
    string Reason);

public sealed record SessionBlockPlan(SessionBlockKind Kind, double Minutes);

public sealed record SessionPlan(
    int DurationMinutes,
    IReadOnlyList<SessionBlockPlan> Blocks,
    IReadOnlyList<SessionActivity> Activities,
    string? FocusSkillId);

public enum SessionStatus
{
    Active,
    Paused,
    Completed,
    Abandoned,
}

public sealed record ActivityOutcome(int Index, bool? Correct, Guid? AttemptId);

/// <summary>Sesión de estudio persistible: se puede pausar y retomar.</summary>
public sealed record StudySession(
    Guid Id,
    SessionPlan Plan,
    SessionStatus Status,
    int CurrentIndex,
    IReadOnlyList<ActivityOutcome> Outcomes,
    TimeSpan ActiveTime,
    DateTimeOffset StartedAt,
    DateTimeOffset LastActiveAt,
    DateTimeOffset? CompletedAt,
    string? DraftAnswer)
{
    public const int MaxMinutes = 90;
    public const int MinMinutes = 10;

    public SessionActivity? Current => CurrentIndex < Plan.Activities.Count ? Plan.Activities[CurrentIndex] : null;

    public bool IsFinished => Status is SessionStatus.Completed or SessionStatus.Abandoned;
}
