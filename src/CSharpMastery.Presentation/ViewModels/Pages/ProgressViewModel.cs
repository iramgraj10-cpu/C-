using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Analytics;
using CSharpMastery.Learning.Services;
using CSharpMastery.Presentation.Formatting;
using CSharpMastery.Presentation.Services;

namespace CSharpMastery.Presentation.ViewModels.Pages;

public sealed record StatCard(string Value, string Label, string Help);

public sealed record SkillProgressLine(string Id, string Title, string Module, string MasteryLabel, MasteryLevel Mastery, string Accuracy, string Review, string Pending, bool NeedsReinforcement);

public sealed record CalibrationLine(string Skill, string Label, string Detail, bool Overconfident);

/// <summary>
/// Panel de progreso. Separa visto, completado, practicado, dominio provisional, retenido y pendiente de demostrar;
/// deliberadamente no muestra una sola barra de porcentaje.
/// </summary>
public sealed partial class ProgressViewModel(LearningService learning, INavigationService navigation) : ViewModelBase
{
    public ObservableCollection<StatCard> Content { get; } = [];

    public ObservableCollection<StatCard> Mastery { get; } = [];

    public ObservableCollection<StatCard> Habits { get; } = [];

    public ObservableCollection<string> Errors { get; } = [];

    public ObservableCollection<CalibrationLine> Calibration { get; } = [];

    public ObservableCollection<SkillProgressLine> Skills { get; } = [];

    [ObservableProperty]
    public partial bool ShowOnlyStarted { get; set; } = true;

    private ProgressReport? _report;

    partial void OnShowOnlyStartedChanged(bool value) => FillSkills();

    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var r = _report = await learning.GetReportAsync();
            Content.Clear();
            Content.Add(new("" + r.LessonsSeen, "Lecciones vistas", $"De {r.SkillsWithLessons} lecciones disponibles. Ver una lección no equivale a dominarla."));
            Content.Add(new("" + r.ExercisesCompleted, "Ítems resueltos", $"Ítems distintos resueltos correctamente; {r.ItemsAttempted} intentados en total."));

            Mastery.Clear();
            Mastery.Add(new("" + r.SkillsPracticedOnly, "Practicadas", "Con práctica, pero sin evidencia suficiente de dominio."));
            Mastery.Add(new("" + r.SkillsProvisional, "Dominio provisional", "Cumplen los criterios; falta confirmarlas en un repaso posterior."));
            Mastery.Add(new("" + r.SkillsRetained, "Retenidas", "Recordadas correctamente días después."));
            Mastery.Add(new("" + r.SkillsPendingDemonstration, "Pendientes de demostrar", "Habilidades con lección que aún no alcanzan el dominio provisional."));
            Mastery.Add(new("" + r.SkillsIndependent, "Con código independiente", "Habilidades donde resolviste un ejercicio de código sin pistas."));

            Habits.Clear();
            Habits.Add(new("" + r.ReviewsDue, "Repasos pendientes", "Habilidades cuyo repaso ya venció."));
            Habits.Add(new(Labels.Duration(r.PracticeTime), "Tiempo de práctica", $"{r.SessionsCompleted} sesiones completadas."));
            Habits.Add(new($"{r.IndependentRate:P0}", "Resueltos sin ayuda", "Proporción de intentos sin pistas ni solución."));
            Habits.Add(new($"{r.HintsPerAttempt:0.0}", "Ayudas por intento", "Pistas o soluciones vistas, en promedio."));

            Errors.Clear();
            foreach (var (category, count) in r.FrequentErrors)
            {
                Errors.Add($"{ErrorCategoryInfo.DisplayName(category)} · {count} veces. {ErrorCategoryInfo.Advice(category)}");
            }

            Calibration.Clear();
            foreach (var c in r.Calibration.Where(c => c.Flag != CalibrationFlag.InsufficientData))
            {
                Calibration.Add(new CalibrationLine(learning.Catalog.FindSkill(c.SkillId)?.Title ?? c.SkillId, Labels.Calibration(c.Flag),
                    $"Confianza media {c.MeanConfidence:P0} · aciertos {c.Accuracy:P0} ({c.Samples} respuestas)", c.Flag == CalibrationFlag.Overconfident));
            }

            FillSkills();
        }, "No se pudo calcular el progreso.");
    }

    private void FillSkills()
    {
        Skills.Clear();
        if (_report is null)
        {
            return;
        }

        var now = learning.Clock.Now;
        foreach (var row in _report.Skills.Where(s => !ShowOnlyStarted || s.Mastery > MasteryLevel.NotStarted))
        {
            Skills.Add(new SkillProgressLine(row.SkillId, row.Title, $"Nivel {row.Level}", Labels.Mastery(row.Mastery), row.Mastery,
                row.Accuracy is { } a ? $"Precisión reciente {a:P0} ({row.Attempts} intentos)" : "Sin intentos calificados",
                row.NextReview is null ? "" : Labels.RelativeDue(row.NextReview, now),
                row.Pending.Count == 0 ? "" : "Falta: " + string.Join("; ", row.Pending),
                row.NeedsReinforcement));
        }
    }

    [RelayCommand]
    private Task OpenSkillAsync(SkillProgressLine line) =>
        navigation.GoToAsync(Routes.Lesson, new Dictionary<string, object> { [Routes.SkillParameter] = line.Id });
}
