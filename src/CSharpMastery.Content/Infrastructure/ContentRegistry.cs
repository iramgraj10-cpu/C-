using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;

namespace CSharpMastery.Content.Infrastructure;

/// <summary>Acumula los ítems del banco y ofrece fábricas concisas para escribir contenido.</summary>
public sealed class ContentRegistry
{
    private readonly List<LearningItem> _items = [];

    public IReadOnlyList<LearningItem> Items => _items;

    private T Add<T>(T item) where T : LearningItem
    {
        if (_items.Any(i => i.Id == item.Id))
        {
            throw new InvalidOperationException($"Ítem duplicado: {item.Id}");
        }

        _items.Add(item);
        return item;
    }

    public static ChoiceOption Ok(string text, string feedback) => new(text, true, feedback, ErrorCategory.None);

    public static ChoiceOption No(string text, string feedback, ErrorCategory error = ErrorCategory.ConceptMisunderstanding) => new(text, false, feedback, error);

    public ChoiceItem Choice(
        string id, string skill, ItemPurpose purpose, string prompt, string explanation, ChoiceOption[] options,
        string? code = null, int difficulty = 1, string[]? hints = null, ErrorCategory[]? targets = null,
        string[]? related = null, KnowledgeDimension dimension = KnowledgeDimension.Semantics, double minutes = 1.5) =>
        Add(new ChoiceItem
        {
            Id = id,
            SkillId = skill,
            Purpose = purpose,
            Prompt = prompt,
            Explanation = explanation,
            Options = options,
            Code = code,
            Difficulty = difficulty,
            Hints = hints ?? [],
            TargetsErrors = targets ?? [],
            RelatedSkillIds = related ?? [],
            Dimension = dimension,
            EstimatedMinutes = minutes,
        });

    public ShortAnswerItem Short(
        string id, string skill, ItemPurpose purpose, string prompt, string explanation, string[] accepted,
        AnswerMatch match = AnswerMatch.Text, string? code = null, int difficulty = 1, string[]? hints = null,
        Dictionary<string, string>? wrong = null, ErrorCategory[]? targets = null, string[]? related = null,
        KnowledgeDimension dimension = KnowledgeDimension.Semantics, double minutes = 1.5) =>
        Add(new ShortAnswerItem
        {
            Id = id,
            SkillId = skill,
            Purpose = purpose,
            Prompt = prompt,
            Explanation = explanation,
            AcceptedAnswers = accepted,
            Match = match,
            Code = code,
            Difficulty = difficulty,
            Hints = hints ?? [],
            KnownWrongAnswers = wrong ?? [],
            TargetsErrors = targets ?? [],
            RelatedSkillIds = related ?? [],
            Dimension = dimension,
            EstimatedMinutes = minutes,
        });

    public CodeExercise Code(
        string id, string skill, ItemPurpose purpose, CodeMode mode, string prompt, string starter, string solution,
        CodeTestCase[] tests, string explanation, string[] hints, int difficulty = 1, CodeConstruct[]? requires = null,
        ErrorCategory[]? targets = null, string[]? related = null, string? solutionExplanation = null,
        KnowledgeDimension dimension = KnowledgeDimension.ProblemSolving, double minutes = 5) =>
        Add(new CodeExercise
        {
            Id = id,
            SkillId = skill,
            Purpose = purpose,
            Mode = mode,
            Prompt = prompt,
            StarterCode = starter,
            ReferenceSolution = solution,
            Tests = tests,
            Explanation = explanation,
            Hints = hints,
            Difficulty = difficulty,
            RequiredConstructs = requires ?? [],
            TargetsErrors = targets ?? [],
            RelatedSkillIds = related ?? [],
            SolutionExplanation = solutionExplanation ?? explanation,
            Dimension = dimension,
            EstimatedMinutes = minutes,
        });

    /// <summary>Caso de prueba para un programa: entrada estándar y salida esperada.</summary>
    public static CodeTestCase In(string name, string input, string expected, bool hidden = false, bool edge = false, string? hint = null) =>
        new() { Name = name, Input = input, Expected = expected, Hidden = hidden, EdgeCase = edge, FailureHint = hint };

    /// <summary>Caso de prueba para una función: expresión a evaluar y valor esperado.</summary>
    public static CodeTestCase Is(string name, string check, string expected, bool hidden = false, bool edge = false, string? hint = null) =>
        new() { Name = name, Check = check, Expected = expected, Hidden = hidden, EdgeCase = edge, FailureHint = hint };

    /// <summary>Caso de prueba que espera una excepción.</summary>
    public static CodeTestCase Throws(string name, string check, string exception, bool hidden = false, string? hint = null) =>
        new() { Name = name, Check = check, ExpectedException = exception, Hidden = hidden, EdgeCase = true, FailureHint = hint };
}

/// <summary>Atajos para construir lecciones.</summary>
public static class L
{
    public static Paragraph P(string text) => new(text);

    public static CodeBlock Code(string code, string? caption = null) => new(code, caption);

    /// <summary>Fragmento ilustrativo que no se compila como programa completo.</summary>
    public static CodeBlock Fragment(string code, string? caption = null) => new(code, "Fragmento" + (caption is null ? "" : ": " + caption));

    public static Callout Note(string text) => new(CalloutKind.Note, text);

    public static Callout Tip(string text) => new(CalloutKind.Tip, text);

    public static Callout Warn(string text) => new(CalloutKind.Warning, text);

    public static Callout Design(string text) => new(CalloutKind.Design, text);

    public static LineNote Line(int line, string note) => new(line, note);
}
