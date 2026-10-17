using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Services;
using CSharpMastery.Presentation.Formatting;
using CSharpMastery.Presentation.Services;
using CSharpMastery.Presentation.ViewModels.Display;

namespace CSharpMastery.Presentation.ViewModels.Pages;

public sealed record PrerequisiteLine(string Id, string Title, string MasteryLabel, bool Met);

/// <summary>Lección completa: objetivo, explicación, ejemplo resuelto, error común, criterio de dominio y práctica.</summary>
public sealed partial class LessonViewModel(LearningService learning, INavigationService navigation) : ViewModelBase, INavigationAware
{
    private Skill? _skill;

    [ObservableProperty]
    public partial LessonContent? Content { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = "";

    [ObservableProperty]
    public partial string Summary { get; set; } = "";

    [ObservableProperty]
    public partial string MasteryLabel { get; set; } = "";

    [ObservableProperty]
    public partial string MasteryHelp { get; set; } = "";

    [ObservableProperty]
    public partial string? PendingText { get; set; }

    [ObservableProperty]
    public partial string? ReviewText { get; set; }

    [ObservableProperty]
    public partial string UnitTitle { get; set; } = "";

    [ObservableProperty]
    public partial MasteryLevel Level { get; set; }

    [ObservableProperty]
    public partial string StartText { get; set; } = "Empezar lección";

    /// <summary>Apuntes completos de la lección (referencia para consultar, no la forma principal de aprender).</summary>
    [ObservableProperty]
    public partial bool ShowNotes { get; set; }

    public bool HasLesson => Content is not null;

    public bool IsOutline => Content is null;

    public ObservableCollection<PrerequisiteLine> Prerequisites { get; } = [];

    partial void OnContentChanged(LessonContent? value)
    {
        OnPropertyChanged(nameof(HasLesson));
        OnPropertyChanged(nameof(IsOutline));
    }

    public async Task OnNavigatedToAsync(IReadOnlyDictionary<string, object> parameters)
    {
        if (parameters.TryGetValue(Routes.SkillParameter, out var value) && value is string id)
        {
            await LoadAsync(id);
        }
    }

    public async Task LoadAsync(string skillId)
    {
        await RunSafeAsync(async () =>
        {
            _skill = learning.Catalog.GetSkill(skillId);
            Title = _skill.Title;
            Summary = _skill.Summary;
            Content = _skill.Lesson is { } lesson ? new LessonContent(_skill, lesson) : null;
            UnitTitle = learning.Catalog.Modules.SelectMany(m => m.Units).FirstOrDefault(u => u.Id == _skill.UnitId)?.Title ?? "";
            ShowNotes = false;

            var snapshot = await learning.GetSnapshotAsync();
            var progress = snapshot.Skill(skillId);
            MasteryLabel = Labels.Mastery(progress.Level);
            Level = progress.Level;
            StartText = progress.Level >= MasteryLevel.Seen ? "Repetir lección" : "Empezar lección";
            MasteryHelp = Labels.MasteryHelp(progress.Level);
            ReviewText = progress.Review.IsScheduled ? Labels.RelativeDue(progress.Review.DueAt, learning.Clock.Now) : null;

            var report = await learning.GetReportAsync();
            var row = report.Skills.FirstOrDefault(r => r.SkillId == skillId);
            PendingText = row is { Pending.Count: > 0 } && _skill.Lesson is not null ? "Para demostrar dominio falta: " + string.Join("; ", row.Pending) : null;

            Prerequisites.Clear();
            foreach (var id in _skill.Prerequisites)
            {
                var level = snapshot.Level(id);
                Prerequisites.Add(new PrerequisiteLine(id, learning.Catalog.FindSkill(id)?.Title ?? id, Labels.Mastery(level), level >= MasteryLevel.Practiced));
            }
        }, "No se pudo abrir la lección.");
    }

    /// <summary>Lección interactiva: tarjetas cortas, huecos, ordenar líneas, encontrar el error y ejercicios.</summary>
    [RelayCommand]
    private Task StartLessonAsync() => _skill?.Lesson is null
        ? Task.CompletedTask
        : navigation.GoToAsync(Routes.Activity, HomeViewModel.Params(new ActivityRequest(ActivityMode.Lesson, [], Title: _skill.Title, LessonSkillId: _skill.Id)));

    /// <summary>Práctica de código en orden de dificultad: guiado → independiente → aplicación, con predicción y errores.</summary>
    [RelayCommand]
    private Task PracticeAsync()
    {
        if (_skill?.Lesson is not { } lesson)
        {
            return Task.CompletedTask;
        }

        var bug = learning.Catalog.ItemsForSkill(_skill.Id).Where(i => i.Purpose is Domain.Items.ItemPurpose.BugFinding or Domain.Items.ItemPurpose.Concept)
            .Select(i => i.Id).Where(id => !lesson.AllItemIds().Contains(id));
        var ids = new List<string> { lesson.PredictionItemId };
        ids.AddRange(bug.Take(1));
        ids.AddRange([lesson.GuidedExerciseId, lesson.IndependentExerciseId, lesson.ApplicationExerciseId]);
        return navigation.GoToAsync(Routes.Activity, HomeViewModel.Params(new ActivityRequest(ActivityMode.Lesson, ids, Title: $"Práctica: {_skill.Title}")));
    }

    /// <summary>Muestra u oculta los apuntes completos. Consultarlos cuenta como lección vista.</summary>
    [RelayCommand]
    private async Task ToggleNotesAsync()
    {
        ShowNotes = !ShowNotes;
        if (ShowNotes && _skill?.Lesson is not null)
        {
            await learning.MarkLessonSeenAsync(_skill.Id);
        }
    }

    /// <summary>Preguntas de recuperación activa de la lección (útiles al día siguiente).</summary>
    [RelayCommand]
    private Task RecallAsync()
    {
        if (_skill?.Lesson is not { } lesson)
        {
            return Task.CompletedTask;
        }

        return navigation.GoToAsync(Routes.Activity, HomeViewModel.Params(new ActivityRequest(ActivityMode.Practice, lesson.RecallItemIds, Title: $"Recuerdo: {_skill.Title}")));
    }

    [RelayCommand]
    private Task OpenPrerequisiteAsync(PrerequisiteLine line) =>
        navigation.GoToAsync(Routes.Lesson, new Dictionary<string, object> { [Routes.SkillParameter] = line.Id });
}
