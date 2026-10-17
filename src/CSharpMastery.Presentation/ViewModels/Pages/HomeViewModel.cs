using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Domain.Assessments;
using CSharpMastery.Domain.Common;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Domain.Sessions;
using CSharpMastery.Learning.Analytics;
using CSharpMastery.Learning.Services;
using CSharpMastery.Learning.Sessions;
using CSharpMastery.Presentation.Formatting;
using CSharpMastery.Presentation.Services;

namespace CSharpMastery.Presentation.ViewModels.Pages;

public sealed record PlanBlockLine(string Label, string Minutes);

public sealed record PendingAssessmentLine(Guid Id, string Title, string Progress);

/// <summary>Un día de la semana en la tira de racha.</summary>
public sealed record WeekDayLine(string Label, bool Active, bool IsToday);

/// <summary>Inicio: retomar, repasos pendientes, nueva sesión con duración elegible y errores recurrentes.</summary>
public sealed partial class HomeViewModel(
    LearningService learning,
    INavigationService navigation,
    IDialogService dialogs) : ViewModelBase
{
    private StudySession? _unfinished;

    [ObservableProperty]
    public partial int Streak { get; set; }

    [ObservableProperty]
    public partial int TotalXp { get; set; }

    [ObservableProperty]
    public partial int TodayXp { get; set; }

    [ObservableProperty]
    public partial int DailyGoal { get; set; } = GamificationService.DefaultGoal;

    [ObservableProperty]
    public partial double GoalProgress { get; set; }

    [ObservableProperty]
    public partial string GoalText { get; set; } = "";

    [ObservableProperty]
    public partial string StreakText { get; set; } = "";

    [ObservableProperty]
    public partial bool PracticedToday { get; set; }

    /// <summary>Título de la lección para «Continuar» (la primera no dominada de la ruta).</summary>
    [ObservableProperty]
    public partial string? NextLessonTitle { get; set; }

    [ObservableProperty]
    public partial string? NextLessonUnit { get; set; }

    public ObservableCollection<WeekDayLine> Week { get; } = [];

    public IReadOnlyList<int> Presets { get; } = SessionPlanner.PresetMinutes;

    [ObservableProperty]
    public partial string Greeting { get; set; } = "Hola";

    [ObservableProperty]
    public partial int SelectedMinutes { get; set; } = 45;

    [ObservableProperty]
    public partial string CustomMinutes { get; set; } = "";

    [ObservableProperty]
    public partial string? MinutesError { get; set; }

    [ObservableProperty]
    public partial string? FocusText { get; set; }

    [ObservableProperty]
    public partial string? FocusSkillId { get; set; }

    [ObservableProperty]
    public partial bool HasUnfinishedSession { get; set; }

    [ObservableProperty]
    public partial string? UnfinishedText { get; set; }

    [ObservableProperty]
    public partial int DueReviews { get; set; }

    [ObservableProperty]
    public partial string? PlacementHint { get; set; }

    public bool HasDueReviews => DueReviews > 0;

    public string DueReviewsText => DueReviews switch
    {
        0 => "No tienes repasos pendientes hoy.",
        1 => "Tienes 1 habilidad para repasar hoy.",
        _ => $"Tienes {DueReviews} habilidades para repasar hoy.",
    };

    public ObservableCollection<PlanBlockLine> PlanPreview { get; } = [];

    public ObservableCollection<string> RecurringErrors { get; } = [];

    public ObservableCollection<PendingAssessmentLine> PendingAssessments { get; } = [];

    partial void OnDueReviewsChanged(int value)
    {
        OnPropertyChanged(nameof(HasDueReviews));
        OnPropertyChanged(nameof(DueReviewsText));
    }

    /// <summary>Devuelve false si no hay perfil y se debe mostrar la bienvenida.</summary>
    public async Task<bool> LoadAsync()
    {
        var hasProfile = true;
        await RunSafeAsync(async () =>
        {
            var profile = await learning.Store.GetProfileAsync();
            if (profile is null)
            {
                hasProfile = false;
                return;
            }

            Greeting = $"Hola, {profile.DisplayName}";
            if (SelectedMinutes == 45 && profile.PreferredSessionMinutes != 45)
            {
                SelectedMinutes = profile.PreferredSessionMinutes;
            }

            _unfinished = await learning.GetUnfinishedSessionAsync();
            HasUnfinishedSession = _unfinished is not null;
            UnfinishedText = _unfinished is null
                ? null
                : $"Sesión de {_unfinished.Plan.DurationMinutes} min en pausa: paso {_unfinished.CurrentIndex + 1} de {_unfinished.Plan.Activities.Count} ({Labels.Duration(_unfinished.ActiveTime)} activos).";

            DueReviews = (await learning.GetDueReviewSkillsAsync()).Count;

            PendingAssessments.Clear();
            foreach (var stored in (await learning.Store.GetAssessmentsAsync()).Where(a => a.Assessment.Status == AssessmentStatus.InProgress))
            {
                PendingAssessments.Add(new PendingAssessmentLine(stored.Assessment.Id, stored.Assessment.Title,
                    $"{stored.Assessment.CurrentIndex} de {stored.Assessment.ItemIds.Count} respondidas"));
            }

            var snapshot = await learning.GetSnapshotAsync();
            RecurringErrors.Clear();
            foreach (var error in new ErrorPatternAnalyzer().Find(snapshot.Attempts, learning.Clock.Now).Take(3))
            {
                RecurringErrors.Add($"{ErrorCategoryInfo.DisplayName(error.Category)}: {ErrorCategoryInfo.Advice(error.Category)}");
            }

            PlacementHint = profile.InitialLevel != StartingLevel.NeverProgrammed && snapshot.Attempts.Count == 0
                ? "Si ya programas, puedes presentar la evaluación de una unidad desde Ruta para demostrar lo que sabes y avanzar más rápido."
                : null;

            var game = await learning.Gamification.GetStateAsync();
            Streak = game.Streak;
            TotalXp = game.TotalXp;
            TodayXp = game.TodayXp;
            DailyGoal = game.DailyGoal;
            GoalProgress = game.GoalProgress;
            PracticedToday = game.PracticedToday;
            GoalText = game.GoalReached ? $"¡Meta cumplida! {game.TodayXp} XP hoy" : $"{game.TodayXp} / {game.DailyGoal} XP hoy";
            StreakText = game.Streak == 0 ? "Empieza tu racha hoy"
                : game.PracticedToday ? $"¡{game.Streak} {(game.Streak == 1 ? "día" : "días")} seguidos!"
                : $"Racha de {game.Streak} {(game.Streak == 1 ? "día" : "días")}: practica hoy para no perderla";
            var activeDays = new HashSet<DateOnly>(snapshot.Attempts.Select(a => DateOnly.FromDateTime(a.At.ToLocalTime().DateTime)));
            activeDays.UnionWith(game.LastWeek.Where(d => d.Xp > 0).Select(d => d.Day));
            Week.Clear();
            foreach (var day in game.LastWeek)
            {
                Week.Add(new WeekDayLine(Labels.WeekdayInitial(day.Day.DayOfWeek), activeDays.Contains(day.Day), day.Day == learning.Gamification.Today));
            }

            await RefreshPlanAsync();
            var next = new SessionPlanner(learning.Catalog).ChooseFocus(snapshot);
            NextLessonTitle = next?.Title;
            NextLessonUnit = next is null ? null : learning.Catalog.Modules.SelectMany(m => m.Units).FirstOrDefault(u => u.Id == next.UnitId)?.Title;
            _nextLessonId = next?.Id;
        }, "No se pudo cargar el inicio.");
        return hasProfile;
    }

    partial void OnSelectedMinutesChanged(int value) => _ = RefreshPlanAsync();

    private async Task RefreshPlanAsync()
    {
        try
        {
            SessionPlanner.Validate(SelectedMinutes);
            var plan = await learning.PlanSessionAsync(SelectedMinutes, seed: 1);
            PlanPreview.Clear();
            foreach (var block in plan.Blocks)
            {
                PlanPreview.Add(new PlanBlockLine(Labels.Block(block.Kind), $"{block.Minutes:0} min"));
            }

            FocusSkillId = plan.FocusSkillId;
            FocusText = plan.FocusSkillId is { } id && learning.Catalog.FindSkill(id) is { } skill
                ? $"Tema de hoy: {skill.Title}"
                : "Hoy: práctica intercalada y repasos.";
        }
        catch (ArgumentException ex)
        {
            MinutesError = ex.Message;
        }
    }

    [RelayCommand]
    private void SelectPreset(int minutes)
    {
        MinutesError = null;
        CustomMinutes = "";
        SelectedMinutes = minutes;
    }

    [RelayCommand]
    private void ApplyCustomMinutes()
    {
        if (!int.TryParse(CustomMinutes, out var minutes) || minutes < StudySession.MinMinutes || minutes > StudySession.MaxMinutes)
        {
            MinutesError = $"Elige entre {StudySession.MinMinutes} y {StudySession.MaxMinutes} minutos.";
            return;
        }

        MinutesError = null;
        SelectedMinutes = minutes;
    }

    [RelayCommand]
    private async Task StartSessionAsync()
    {
        if (_unfinished is not null
            && !await dialogs.ConfirmAsync("Sesión en pausa", "Tienes una sesión sin terminar. Si empiezas otra, la anterior se cerrará (lo que hiciste queda guardado). ¿Continuar?", "Empezar nueva", "Cancelar"))
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            var session = await learning.StartSessionAsync(SelectedMinutes);
            await navigation.GoToAsync(Routes.Activity, Params(new ActivityRequest(ActivityMode.Session, [], session.Id)));
        }, "No se pudo iniciar la sesión.");
    }

    [RelayCommand]
    private async Task ResumeSessionAsync()
    {
        if (_unfinished is not null)
        {
            await navigation.GoToAsync(Routes.Activity, Params(new ActivityRequest(ActivityMode.Session, [], _unfinished.Id)));
        }
    }

    [RelayCommand]
    private async Task StartReviewAsync()
    {
        await RunSafeAsync(async () =>
        {
            var queue = await learning.BuildReviewQueueAsync();
            if (queue.Count == 0)
            {
                await dialogs.AlertAsync("Repaso", "No hay repasos pendientes ahora.");
                return;
            }

            await navigation.GoToAsync(Routes.Activity, Params(new ActivityRequest(ActivityMode.Review, queue.Select(i => i.Id).ToList(), Title: "Repaso del día")));
        }, "No se pudo preparar el repaso.");
    }

    [RelayCommand]
    private async Task OpenFocusLessonAsync()
    {
        if (FocusSkillId is not null)
        {
            await navigation.GoToAsync(Routes.Lesson, new Dictionary<string, object> { [Routes.SkillParameter] = FocusSkillId });
        }
    }

    private string? _nextLessonId;

    /// <summary>Empieza la siguiente lección interactiva de la ruta.</summary>
    [RelayCommand]
    private Task StartNextLessonAsync() => _nextLessonId is null
        ? navigation.GoToAsync(Routes.Curriculum)
        : navigation.GoToAsync(Routes.Activity, Params(new ActivityRequest(ActivityMode.Lesson, [], LessonSkillId: _nextLessonId)));

    [RelayCommand]
    private Task ResumeAssessmentAsync(PendingAssessmentLine line) =>
        navigation.GoToAsync(Routes.Activity, Params(new ActivityRequest(ActivityMode.Assessment, [], AssessmentId: line.Id)));

    /// <summary>Reinicia una evaluación en curso: se descarta solo esa evaluación, no el progreso general.</summary>
    [RelayCommand]
    private async Task ResetAssessmentAsync(PendingAssessmentLine line)
    {
        if (!await dialogs.ConfirmAsync("Reiniciar evaluación", "Se descartará esta evaluación en curso. Tu progreso en las habilidades se conserva.", "Reiniciar", "Cancelar"))
        {
            return;
        }

        if (await learning.Store.GetAssessmentAsync(line.Id) is { } stored)
        {
            await learning.ResetAssessmentAsync(stored.Assessment);
        }

        PendingAssessments.Remove(line);
    }

    internal static Dictionary<string, object> Params(ActivityRequest request) => new() { [Routes.RequestParameter] = request };
}
