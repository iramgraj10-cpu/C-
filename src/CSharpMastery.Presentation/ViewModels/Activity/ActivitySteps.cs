using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace CSharpMastery.Presentation.ViewModels.Activity;

public sealed record SummarySkillLine(string Title, int Correct, int Total, string MasteryLabel, string NextReview)
{
    public string Score => $"{Correct}/{Total}";
}

/// <summary>Cierre con celebración: XP ganados, precisión, racha y meta del día; luego errores frecuentes y repasos.</summary>
public sealed partial class SummaryStepViewModel : StepViewModel
{
    private readonly Func<Task> _onFinish;

    public SummaryStepViewModel(Func<Task> onFinish)
    {
        _onFinish = onFinish;
        PrimaryText = "Continuar";
    }

    public string Title { get; init; } = "Resumen";

    public int Correct { get; init; }

    public int Attempted { get; init; }

    public int Xp { get; init; }

    public int Streak { get; init; }

    public int TodayXp { get; init; }

    public int DailyGoal { get; init; }

    public bool GoalReachedNow { get; init; }

    public double GoalProgress => DailyGoal <= 0 ? 1 : Math.Min(1, TodayXp / (double)DailyGoal);

    public string XpText => $"+{Xp}";

    public string AccuracyText => Attempted == 0 ? "—" : $"{Math.Round(100.0 * Correct / Attempted)}%";

    public string StreakText => Streak == 1 ? "1 día" : $"{Streak} días";

    public string GoalText => GoalReachedNow
        ? "¡Cumpliste tu meta de hoy!"
        : TodayXp >= DailyGoal ? $"Meta del día cumplida ({TodayXp} XP)" : $"Meta del día: {TodayXp} de {DailyGoal} XP";

    public string ScoreText => Attempted == 0 ? "No resolviste ejercicios calificados en esta actividad." : $"Acertaste {Correct} de {Attempted} a la primera.";

    public string? TimeText { get; init; }

    public override bool IsCelebration => true;

    public ObservableCollection<SummarySkillLine> Skills { get; } = [];

    public ObservableCollection<string> Errors { get; } = [];

    public ObservableCollection<string> Notes { get; } = [];

    public bool HasErrors => Errors.Count > 0;

    public bool HasSkills => Skills.Count > 0;

    [RelayCommand]
    private Task FinishAsync() => _onFinish();

    protected override Task OnPrimaryAsync() => _onFinish();
}
