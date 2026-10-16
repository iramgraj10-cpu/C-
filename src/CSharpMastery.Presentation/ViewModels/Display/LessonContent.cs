using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Presentation.ViewModels.Display;

/// <summary>Contenido de una lección listo para mostrarse.</summary>
public sealed class LessonContent
{
    public LessonContent(Skill skill, Lesson lesson)
    {
        Skill = skill;
        Lesson = lesson;
        Blocks = lesson.Explanation.Select(ContentBlockDisplay.From).ToList();
        ExampleNotes = lesson.WorkedExample.Notes.Select(n => new LineNoteDisplay(n.Line, n.Note)).ToList();
    }

    public Skill Skill { get; }

    public Lesson Lesson { get; }

    public string Title => Skill.Title;

    public string Objective => Lesson.Objective;

    public IReadOnlyList<ContentBlockDisplay> Blocks { get; }

    public string ExampleTitle => Lesson.WorkedExample.Title;

    public string ExampleCode => Lesson.WorkedExample.Code;

    public IReadOnlyList<LineNoteDisplay> ExampleNotes { get; }

    public string? ExampleOutput => Lesson.WorkedExample.ExpectedOutput;

    public bool HasExampleOutput => !string.IsNullOrEmpty(ExampleOutput);

    public string MistakeDescription => Lesson.CommonMistake.Description;

    public string MistakeWrongCode => Lesson.CommonMistake.WrongCode;

    public string MistakeWhy => Lesson.CommonMistake.WhyItFails;

    public string MistakeFixedCode => Lesson.CommonMistake.FixedCode;

    public string MasteryCriterion => Lesson.MasteryCriterion;

    public string EstimatedTime => $"≈ {Lesson.EstimatedMinutes} min";
}
