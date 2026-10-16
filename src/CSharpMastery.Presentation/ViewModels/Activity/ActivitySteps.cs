using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Presentation.ViewModels.Display;

namespace CSharpMastery.Presentation.ViewModels.Activity;

/// <summary>Paso de lectura dentro de una sesión: explicación o ejemplo resuelto.</summary>
public sealed partial class ReadingStepViewModel(LessonContent content, bool workedExample, Func<Task> onContinue) : ViewModelBase
{
    public LessonContent Content { get; } = content;

    public bool IsWorkedExample { get; } = workedExample;

    public bool IsExplanation => !IsWorkedExample;

    public string Heading => IsWorkedExample ? $"Ejemplo resuelto: {Content.ExampleTitle}" : Content.Title;

    [RelayCommand]
    private Task ContinueAsync() => onContinue();
}

public sealed record SummarySkillLine(string Title, int Correct, int Total, string MasteryLabel, string NextReview)
{
    public string Score => $"{Correct}/{Total}";
}

/// <summary>Cierre: aciertos, errores frecuentes y repasos programados.</summary>
public sealed partial class SummaryStepViewModel(Func<Task> onFinish) : ViewModelBase
{
    public string Title { get; init; } = "Resumen";

    public int Correct { get; init; }

    public int Attempted { get; init; }

    public string ScoreText => Attempted == 0 ? "No resolviste ítems en esta actividad." : $"Resolviste correctamente {Correct} de {Attempted} ítems al primer intento.";

    public string? TimeText { get; init; }

    public ObservableCollection<SummarySkillLine> Skills { get; } = [];

    public ObservableCollection<string> Errors { get; } = [];

    public ObservableCollection<string> Notes { get; } = [];

    public bool HasErrors => Errors.Count > 0;

    [RelayCommand]
    private Task FinishAsync() => onFinish();
}
