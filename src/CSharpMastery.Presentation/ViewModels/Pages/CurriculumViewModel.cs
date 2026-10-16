using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Services;
using CSharpMastery.Presentation.Formatting;
using CSharpMastery.Presentation.Services;

namespace CSharpMastery.Presentation.ViewModels.Pages;

public sealed record SkillLine(string Id, string Title, string Summary, string TierLabel, string MasteryLabel, MasteryLevel Mastery, bool HasLesson, bool PrerequisitesMet)
{
    public string StatusText => HasLesson ? MasteryLabel : "Contenido en preparación";

    public bool NeedsPrerequisites => HasLesson && !PrerequisitesMet;
}

public sealed record UnitGroup(string Id, string Title, string Description, IReadOnlyList<SkillLine> Skills)
{
    public bool CanAssess => Skills.Any(s => s.HasLesson);
}

public sealed partial class ModuleGroup(CurriculumModule module, IReadOnlyList<UnitGroup> units) : ObservableObject
{
    public string Title => $"Nivel {module.Level}: {module.Title}";

    public string Description => module.Description;

    public IReadOnlyList<UnitGroup> Units { get; } = units;

    public string Summary
    {
        get
        {
            var skills = Units.SelectMany(u => u.Skills).ToList();
            var withLesson = skills.Count(s => s.HasLesson);
            var provisional = skills.Count(s => s.Mastery >= MasteryLevel.ProvisionallyMastered);
            return withLesson == 0
                ? $"{skills.Count} habilidades (contenido en preparación)"
                : $"{skills.Count} habilidades · {withLesson} con lección · {provisional} dominadas al menos provisionalmente";
        }
    }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }
}

/// <summary>Mapa del currículo por niveles, unidades y habilidades, con el estado de dominio de cada una.</summary>
public sealed partial class CurriculumViewModel(LearningService learning, INavigationService navigation, IDialogService dialogs) : ViewModelBase
{
    public ObservableCollection<ModuleGroup> Modules { get; } = [];

    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var snapshot = await learning.GetSnapshotAsync();
            var expanded = Modules.Where(m => m.IsExpanded).Select(m => m.Title).ToHashSet();
            Modules.Clear();
            var firstOpen = true;
            foreach (var module in learning.Catalog.Modules)
            {
                var units = module.Units.Select(u => new UnitGroup(u.Id, u.Title, u.Description, u.Skills.Select(s =>
                {
                    var level = snapshot.Level(s.Id);
                    var prereqs = s.Prerequisites.All(p => snapshot.Level(p) >= MasteryLevel.Practiced);
                    return new SkillLine(s.Id, s.Title, s.Summary, Labels.Tier(s.Tier), Labels.Mastery(level), level, s.Lesson is not null, prereqs);
                }).ToList())).ToList();
                var group = new ModuleGroup(module, units);
                group.IsExpanded = expanded.Contains(group.Title) || (expanded.Count == 0 && firstOpen && units.Count > 0);
                firstOpen &= !group.IsExpanded;
                Modules.Add(group);
            }
        }, "No se pudo cargar la ruta.");
    }

    [RelayCommand]
    private static void Toggle(ModuleGroup module) => module.IsExpanded = !module.IsExpanded;

    [RelayCommand]
    private Task OpenSkillAsync(SkillLine skill) =>
        navigation.GoToAsync(Routes.Lesson, new Dictionary<string, object> { [Routes.SkillParameter] = skill.Id });

    [RelayCommand]
    private async Task AssessUnitAsync(UnitGroup unit)
    {
        if (!await dialogs.ConfirmAsync("Evaluación de unidad",
                $"\"{unit.Title}\": preguntas de concepto, predicción, corrección de errores, código desde cero y transferencia. No verás si acertaste hasta el final. ¿Empezar?",
                "Empezar", "Ahora no"))
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            var assessment = await learning.StartUnitAssessmentAsync(unit.Id);
            await navigation.GoToAsync(Routes.Activity, HomeViewModel.Params(new ActivityRequest(ActivityMode.Assessment, [], AssessmentId: assessment.Id)));
        }, "No se pudo crear la evaluación.");
    }

    [RelayCommand]
    private async Task AssessCumulativeAsync()
    {
        await RunSafeAsync(async () =>
        {
            var assessment = await learning.StartCumulativeAssessmentAsync();
            if (assessment.ItemIds.Count == 0)
            {
                await learning.ResetAssessmentAsync(assessment);
                await dialogs.AlertAsync("Evaluación acumulativa", "Aún no hay suficientes habilidades practicadas para una evaluación acumulativa.");
                return;
            }

            await navigation.GoToAsync(Routes.Activity, HomeViewModel.Params(new ActivityRequest(ActivityMode.Assessment, [], AssessmentId: assessment.Id)));
        }, "No se pudo crear la evaluación.");
    }
}
