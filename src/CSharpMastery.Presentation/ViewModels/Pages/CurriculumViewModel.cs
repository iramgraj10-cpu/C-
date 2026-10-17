using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Services;
using CSharpMastery.Presentation.Formatting;
using CSharpMastery.Presentation.Services;

namespace CSharpMastery.Presentation.ViewModels.Pages;

/// <summary>Cómo se ve una lección en el camino.</summary>
public enum NodeState
{
    /// <summary>Sin lección todavía.</summary>
    Soon,
    /// <summary>Faltan prerrequisitos (se puede abrir igual).</summary>
    Locked,
    /// <summary>Disponible para empezar.</summary>
    Available,
    /// <summary>Vista o practicada, sin dominio todavía.</summary>
    InProgress,
    /// <summary>Dominada (al menos provisionalmente).</summary>
    Completed,
    /// <summary>Retenida tras un repaso posterior.</summary>
    Legendary,
}

public sealed record SkillLine(string Id, string Title, string Summary, string TierLabel, string MasteryLabel, MasteryLevel Mastery, bool HasLesson, bool PrerequisitesMet)
{
    public string StatusText => HasLesson ? MasteryLabel : "Contenido en preparación";

    public bool NeedsPrerequisites => HasLesson && !PrerequisitesMet;

    public NodeState State => !HasLesson ? NodeState.Soon
        : Mastery >= MasteryLevel.Retained ? NodeState.Legendary
        : Mastery >= MasteryLevel.ProvisionallyMastered ? NodeState.Completed
        : Mastery >= MasteryLevel.Seen ? NodeState.InProgress
        : PrerequisitesMet ? NodeState.Available
        : NodeState.Locked;

    /// <summary>La siguiente lección recomendada (lleva la etiqueta «Empezar»).</summary>
    public bool IsCurrent { get; init; }

    /// <summary>Desplazamiento horizontal para dibujar el camino en zigzag.</summary>
    public double Offset { get; init; }

    /// <summary>Nivel del módulo (decide el color de la unidad).</summary>
    public int Level { get; init; }
}

public sealed record UnitGroup(string Id, string Title, string Description, IReadOnlyList<SkillLine> Skills)
{
    public bool CanAssess => Skills.Any(s => s.HasLesson);

    public int Level { get; init; }

    public int Done => Skills.Count(s => s.State is NodeState.Completed or NodeState.Legendary);

    public int WithLesson => Skills.Count(s => s.HasLesson);

    public string ProgressText => WithLesson == 0 ? "Próximamente" : $"{Done} de {WithLesson} dominadas";
}

public sealed partial class ModuleGroup(CurriculumModule module, IReadOnlyList<UnitGroup> units) : ObservableObject
{
    public string Title => $"Nivel {module.Level}: {module.Title}";

    public string Description => module.Description;

    public IReadOnlyList<UnitGroup> Units { get; } = units;

    public int Level => module.Level;

    public string Summary
    {
        get
        {
            var skills = Units.SelectMany(u => u.Skills).ToList();
            var withLesson = skills.Count(s => s.HasLesson);
            var provisional = skills.Count(s => s.Mastery >= MasteryLevel.ProvisionallyMastered);
            if (skills.Count == 0)
            {
                return "Se trabaja desde la sección Proyectos";
            }

            return withLesson == 0
                ? $"{skills.Count} habilidades (contenido en preparación)"
                : $"{provisional} de {withLesson} lecciones dominadas";
        }
    }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }
}

/// <summary>Mapa del currículo por niveles, unidades y habilidades, con el estado de dominio de cada una.</summary>
public sealed partial class CurriculumViewModel(LearningService learning, INavigationService navigation, IDialogService dialogs) : ViewModelBase
{
    private static readonly double[] Zigzag = [0, 50, 80, 50, 0, -50, -80, -50];

    public ObservableCollection<ModuleGroup> Modules { get; } = [];

    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var snapshot = await learning.GetSnapshotAsync();
            var current = new Learning.Sessions.SessionPlanner(learning.Catalog).ChooseFocus(snapshot);
            var expanded = Modules.Where(m => m.IsExpanded).Select(m => m.Title).ToHashSet();
            Modules.Clear();
            var firstOpen = true;
            foreach (var module in learning.Catalog.Modules)
            {
                var units = module.Units.Select(u => new UnitGroup(u.Id, u.Title, u.Description, u.Skills.Select((s, i) =>
                {
                    var level = snapshot.Level(s.Id);
                    var prereqs = s.Prerequisites.All(p => learning.Catalog.FindSkill(p) is not { Lesson: not null } || snapshot.Level(p) >= MasteryLevel.Seen);
                    return new SkillLine(s.Id, s.Title, s.Summary, Labels.Tier(s.Tier), Labels.Mastery(level), level, s.Lesson is not null, prereqs)
                    {
                        IsCurrent = s.Id == current?.Id,
                        Offset = Zigzag[i % Zigzag.Length],
                        Level = module.Level,
                    };
                }).ToList()) { Level = module.Level }).ToList();
                var group = new ModuleGroup(module, units);
                var hasCurrent = units.Any(u => u.Skills.Any(k => k.IsCurrent));
                group.IsExpanded = expanded.Contains(group.Title) || (expanded.Count == 0 && (hasCurrent || (firstOpen && current is null && units.Count > 0)));
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
