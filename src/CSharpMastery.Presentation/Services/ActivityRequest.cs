namespace CSharpMastery.Presentation.Services;

/// <summary>De dónde provienen las actividades que se van a resolver.</summary>
public enum ActivityMode
{
    /// <summary>Sesión de estudio planificada (se puede pausar y retomar).</summary>
    Session,
    /// <summary>Repasos programados.</summary>
    Review,
    /// <summary>Evaluación: sin retroalimentación de corrección hasta el final.</summary>
    Assessment,
    /// <summary>Práctica de una lección en orden pedagógico.</summary>
    Lesson,
    /// <summary>Práctica libre (por ejemplo, de una ruta de recuperación).</summary>
    Practice,
}

public sealed record ActivityRequest(
    ActivityMode Mode,
    IReadOnlyList<string> ItemIds,
    Guid? SessionId = null,
    Guid? AssessmentId = null,
    string? Title = null);
