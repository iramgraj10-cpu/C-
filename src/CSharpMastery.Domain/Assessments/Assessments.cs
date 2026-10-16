namespace CSharpMastery.Domain.Assessments;

public enum AssessmentKind
{
    /// <summary>Evaluación de una unidad.</summary>
    Unit,
    /// <summary>Evaluación acumulativa que mezcla temas anteriores.</summary>
    Cumulative,
}

public enum AssessmentStatus
{
    InProgress,
    Completed,
    /// <summary>Reiniciada: no cuenta, pero el progreso general se conserva.</summary>
    Reset,
}

/// <summary>Instancia concreta de una evaluación (con ítems elegidos al azar de un banco).</summary>
public sealed record Assessment(
    Guid Id,
    AssessmentKind Kind,
    string ScopeId,
    string Title,
    IReadOnlyList<string> ItemIds,
    int Seed,
    AssessmentStatus Status,
    int CurrentIndex,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

public sealed record SkillScore(
    string SkillId,
    int Correct,
    int Total,
    bool IndependentCodeOk,
    bool TransferOk,
    bool HasCodeItems,
    bool HasTransferItems)
{
    public double Accuracy => Total == 0 ? 0 : (double)Correct / Total;
}

public sealed record AssessmentResult(
    Guid AssessmentId,
    double OverallAccuracy,
    bool Passed,
    IReadOnlyList<SkillScore> PerSkill,
    IReadOnlyList<string> WeakSkillIds,
    RecoveryPlan Recovery);

public sealed record RecoveryStep(string SkillId, string Action, IReadOnlyList<string> ItemIds);

/// <summary>Ruta de recuperación personalizada tras una evaluación.</summary>
public sealed record RecoveryPlan(IReadOnlyList<RecoveryStep> Steps)
{
    public static RecoveryPlan Empty { get; } = new([]);
}
