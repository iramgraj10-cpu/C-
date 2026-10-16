namespace CSharpMastery.Domain.Curriculum;

/// <summary>
/// Lección completa de una habilidad. Sigue la secuencia: objetivo, explicación,
/// ejemplo resuelto, error habitual, predicción, ejercicio guiado, ejercicio independiente,
/// aplicación, recuperación activa y criterio de dominio.
/// </summary>
public sealed record Lesson(
    string SkillId,
    string Objective,
    IReadOnlyList<ContentBlock> Explanation,
    WorkedExample WorkedExample,
    CommonMistake CommonMistake,
    string PredictionItemId,
    string GuidedExerciseId,
    string IndependentExerciseId,
    string ApplicationExerciseId,
    IReadOnlyList<string> RecallItemIds,
    string MasteryCriterion,
    int EstimatedMinutes)
{
    /// <summary>Todos los ítems referenciados por la lección, en el orden pedagógico.</summary>
    public IEnumerable<string> AllItemIds()
    {
        yield return PredictionItemId;
        yield return GuidedExerciseId;
        yield return IndependentExerciseId;
        yield return ApplicationExerciseId;
        foreach (var id in RecallItemIds)
        {
            yield return id;
        }
    }
}

public abstract record ContentBlock;

public sealed record Paragraph(string Text) : ContentBlock;

public sealed record CodeBlock(string Code, string? Caption = null) : ContentBlock;

public enum CalloutKind
{
    Note,
    Tip,
    Warning,
    Design,
}

public sealed record Callout(CalloutKind Kind, string Text) : ContentBlock;

/// <summary>Ejemplo resuelto explicado línea por línea.</summary>
public sealed record WorkedExample(string Title, string Code, IReadOnlyList<LineNote> Notes, string? ExpectedOutput);

public sealed record LineNote(int Line, string Note);

/// <summary>Ejemplo de error habitual y su corrección.</summary>
public sealed record CommonMistake(string Description, string WrongCode, string WhyItFails, string FixedCode);
