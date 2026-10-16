using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Domain.Assessments;
using CSharpMastery.Learning.Services;
using CSharpMastery.Presentation.Services;

namespace CSharpMastery.Presentation.ViewModels.Pages;

public sealed record SkillScoreLine(string Title, string Score, bool Weak, string Detail);

public sealed record RecoveryLine(string SkillId, string Title, string Action, IReadOnlyList<string> ItemIds)
{
    public bool HasItems => ItemIds.Count > 0;
}

/// <summary>Resultados de una evaluación con la ruta de recuperación para habilidades débiles.</summary>
public sealed partial class AssessmentResultViewModel(LearningService learning, INavigationService navigation) : ViewModelBase, INavigationAware
{
    private Assessment? _assessment;

    [ObservableProperty]
    public partial string Title { get; set; } = "";

    [ObservableProperty]
    public partial string Verdict { get; set; } = "";

    [ObservableProperty]
    public partial bool Passed { get; set; }

    [ObservableProperty]
    public partial string Accuracy { get; set; } = "";

    public ObservableCollection<SkillScoreLine> Skills { get; } = [];

    public ObservableCollection<RecoveryLine> Recovery { get; } = [];

    public bool HasRecovery => Recovery.Count > 0;

    public async Task OnNavigatedToAsync(IReadOnlyDictionary<string, object> parameters)
    {
        if (parameters.TryGetValue(Routes.AssessmentParameter, out var value) && value is Guid id)
        {
            await LoadAsync(id);
        }
    }

    public async Task LoadAsync(Guid id)
    {
        await RunSafeAsync(async () =>
        {
            var stored = await learning.Store.GetAssessmentAsync(id) ?? throw new KeyNotFoundException("Evaluación no encontrada.");
            _assessment = stored.Assessment;
            var result = stored.Result ?? throw new InvalidOperationException("La evaluación no tiene resultados todavía.");
            var criteria = await learning.GetCriteriaAsync();
            Title = stored.Assessment.Title;
            Passed = result.Passed;
            Accuracy = $"{result.OverallAccuracy:P0} de aciertos (se requiere {criteria.AssessmentPassAccuracy:P0} y evidencia de código y transferencia)";
            Verdict = result.Passed
                ? "Aprobada. Las habilidades evaluadas seguirán en tus repasos para confirmar que las retienes."
                : "Aún no. No es un fracaso: muestra exactamente qué reforzar. Sigue la ruta de recuperación y vuelve a intentarlo.";

            Skills.Clear();
            foreach (var s in result.PerSkill)
            {
                var detail = new List<string>();
                if (s.HasCodeItems)
                {
                    detail.Add(s.IndependentCodeOk ? "código desde cero: bien" : "código desde cero: pendiente");
                }

                if (s.HasTransferItems)
                {
                    detail.Add(s.TransferOk ? "transferencia: bien" : "transferencia: pendiente");
                }

                Skills.Add(new SkillScoreLine(learning.Catalog.FindSkill(s.SkillId)?.Title ?? s.SkillId, $"{s.Correct}/{s.Total}",
                    result.WeakSkillIds.Contains(s.SkillId), string.Join(" · ", detail)));
            }

            Recovery.Clear();
            foreach (var step in result.Recovery.Steps)
            {
                Recovery.Add(new RecoveryLine(step.SkillId, learning.Catalog.FindSkill(step.SkillId)?.Title ?? step.SkillId, step.Action, step.ItemIds));
            }

            OnPropertyChanged(nameof(HasRecovery));
        }, "No se pudieron cargar los resultados.");
    }

    [RelayCommand]
    private Task PracticeRecoveryAsync(RecoveryLine line) =>
        navigation.GoToAsync(Routes.Activity, HomeViewModel.Params(new ActivityRequest(ActivityMode.Practice, line.ItemIds, Title: $"Recuperación: {line.Title}")));

    [RelayCommand]
    private Task OpenLessonAsync(RecoveryLine line) =>
        navigation.GoToAsync(Routes.Lesson, new Dictionary<string, object> { [Routes.SkillParameter] = line.SkillId });

    /// <summary>Nueva evaluación del mismo alcance con otros ítems (no se aprueba memorizando una solución).</summary>
    [RelayCommand]
    private async Task RetakeAsync()
    {
        if (_assessment is null)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            var next = _assessment.Kind == AssessmentKind.Unit
                ? await learning.StartUnitAssessmentAsync(_assessment.ScopeId)
                : await learning.StartCumulativeAssessmentAsync();
            await navigation.GoToAsync(Routes.Activity, HomeViewModel.Params(new ActivityRequest(ActivityMode.Assessment, [], AssessmentId: next.Id)));
        }, "No se pudo crear la evaluación.");
    }

    [RelayCommand]
    private Task DoneAsync() => navigation.GoToAsync(Routes.Home);
}
