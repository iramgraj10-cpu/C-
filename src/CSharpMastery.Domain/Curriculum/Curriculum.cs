namespace CSharpMastery.Domain.Curriculum;

/// <summary>Clasifica un tema según su importancia en la ruta.</summary>
public enum Tier
{
    /// <summary>Fundamento imprescindible: sin él no se puede avanzar.</summary>
    Essential,
    /// <summary>Conocimiento intermedio que se apoya en los fundamentos.</summary>
    Intermediate,
    /// <summary>Tema avanzado u opcional para un nivel sólido.</summary>
    Advanced,
}

/// <summary>Qué tan desarrollado está el contenido de una habilidad en el banco.</summary>
public enum ContentStatus
{
    /// <summary>Lección completa con ejercicios evaluables.</summary>
    Complete,
    /// <summary>Solo hay temario: objetivos y descripción, sin lección interactiva todavía.</summary>
    Outline,
}

/// <summary>Distingue qué tipo de conocimiento se ejercita.</summary>
public enum KnowledgeDimension
{
    Syntax,
    Semantics,
    Design,
    ProblemSolving,
}

/// <summary>Un nivel de la ruta (Nivel 0 a Nivel 7).</summary>
public sealed record CurriculumModule(
    string Id,
    int Level,
    string Title,
    string Description,
    IReadOnlyList<Unit> Units);

/// <summary>Una unidad dentro de un nivel. Cada unidad tiene su evaluación.</summary>
public sealed record Unit(
    string Id,
    string ModuleId,
    string Title,
    string Description,
    IReadOnlyList<Skill> Skills);

/// <summary>Un concepto o habilidad concreta que se puede practicar y dominar.</summary>
public sealed record Skill(
    string Id,
    string UnitId,
    string Title,
    string Summary,
    Tier Tier,
    IReadOnlyList<string> Prerequisites,
    Lesson? Lesson)
{
    public ContentStatus Status => Lesson is null ? ContentStatus.Outline : ContentStatus.Complete;
}

/// <summary>Proyecto integrador del nivel 7 (o de cualquier nivel).</summary>
public sealed record IntegrativeProject(
    string Id,
    string Title,
    string Description,
    IReadOnlyList<string> SkillIds,
    IReadOnlyList<string> Requirements,
    IReadOnlyList<string> AcceptanceCriteria,
    IReadOnlyList<ProjectMilestone> Milestones,
    IReadOnlyList<RubricCriterion> Rubric);

/// <summary>Un hito del proyecto, evaluado automáticamente mediante un ejercicio de código.</summary>
public sealed record ProjectMilestone(string Title, string Description, string ExerciseId);

/// <summary>Criterio de rúbrica con descriptores por nivel de logro.</summary>
public sealed record RubricCriterion(string Name, string Insufficient, string Adequate, string Excellent);
