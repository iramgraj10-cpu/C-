using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Domain.Assessments;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Domain.Sessions;
using CSharpMastery.Content;
using CSharpMastery.Execution;
using CSharpMastery.Learning.Interactive;
using CSharpMastery.Learning.Analytics;
using CSharpMastery.Learning.Services;
using CSharpMastery.Learning.Sessions;
using CSharpMastery.Presentation.Formatting;
using CSharpMastery.Presentation.Services;
using CSharpMastery.Presentation.ViewModels.Display;
using CSharpMastery.Presentation.ViewModels.Items;

namespace CSharpMastery.Presentation.ViewModels.Activity;

/// <summary>
/// Conduce cualquier secuencia de actividades: sesión planificada, repasos, evaluación, práctica de lección o práctica
/// libre. Registra cada resultado, guarda el avance tras cada paso (para poder retomarlo) y termina con un resumen.
/// </summary>
public sealed partial class ActivityViewModel(
    LearningService learning,
    IExecutionService execution,
    INavigationService navigation,
    IDialogService dialogs) : ViewModelBase, INavigationAware
{
    private readonly List<SessionActivity> _steps = [];
    private readonly List<(LearningItem Item, bool? Correct)> _results = [];
    private readonly List<string> _messages = [];
    private ActivityRequest _request = new(ActivityMode.Practice, []);
    private StudySession? _session;
    private Assessment? _assessment;
    private readonly List<MicroStepViewModel> _micro = [];
    private readonly LessonFlowBuilder _flow = new(CurriculumContent.InteractiveExclusions);
    private int _position;
    private int _microIndex;
    private bool _plannedNoticeShown;
    private int _goalBefore;
    private int _todayBefore;
    private bool _bonusAwarded;

    [ObservableProperty]
    public partial StepViewModel? CurrentStep { get; set; }

    /// <summary>Avance de 0 a 1 para la barra superior.</summary>
    [ObservableProperty]
    public partial double Progress { get; set; }

    /// <summary>XP ganados en esta actividad.</summary>
    [ObservableProperty]
    public partial int XpEarned { get; set; }

    /// <summary>Aciertos seguidos (para animar una racha dentro de la lección).</summary>
    [ObservableProperty]
    public partial int Combo { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = "";

    [ObservableProperty]
    public partial string ProgressText { get; set; } = "";

    [ObservableProperty]
    public partial string? BlockLabel { get; set; }

    [ObservableProperty]
    public partial string? ElapsedText { get; set; }

    [ObservableProperty]
    public partial string? TimeNotice { get; set; }

    [ObservableProperty]
    public partial bool IsFinished { get; set; }

    public bool IsSession => _request.Mode == ActivityMode.Session;

    public bool IsAssessment => _request.Mode == ActivityMode.Assessment;

    public ActivityMode Mode => _request.Mode;

    public StudySession? Session => _session;

    public async Task OnNavigatedToAsync(IReadOnlyDictionary<string, object> parameters)
    {
        if (parameters.TryGetValue(Routes.RequestParameter, out var value) && value is ActivityRequest request)
        {
            await LoadAsync(request);
        }
    }

    public async Task LoadAsync(ActivityRequest request)
    {
        _request = request;
        _steps.Clear();
        _results.Clear();
        _messages.Clear();
        _position = 0;
        _micro.Clear();
        _microIndex = 0;
        _plannedNoticeShown = false;
        XpEarned = 0;
        _bonusAwarded = false;
        Combo = 0;
        Progress = 0;
        IsFinished = false;
        OnPropertyChanged(nameof(IsSession));
        OnPropertyChanged(nameof(IsAssessment));
        OnPropertyChanged(nameof(Mode));

        await RunSafeAsync(async () =>
        {
            var before = await learning.Gamification.GetStateAsync();
            _goalBefore = before.DailyGoal;
            _todayBefore = before.TodayXp;
            switch (request.Mode)
            {
                case ActivityMode.Lesson when request.LessonSkillId is { } lessonSkill:
                    LoadLessonFlow(learning.Catalog.GetSkill(lessonSkill));
                    break;
                case ActivityMode.Session:
                    await LoadSessionAsync(request);
                    break;
                case ActivityMode.Assessment:
                    await LoadAssessmentAsync(request);
                    break;
                default:
                    Title = request.Title ?? (request.Mode == ActivityMode.Review ? "Repaso" : "Práctica");
                    var index = 0;
                    foreach (var id in request.ItemIds.Where(id => learning.Catalog.FindItem(id) is not null))
                    {
                        _steps.Add(new SessionActivity(index++, SessionBlockKind.Practice, ActivityKind.Item, id, 2, AskConfidenceFor(request.Mode, index), ""));
                    }

                    _steps.Add(new SessionActivity(index, SessionBlockKind.WrapUp, ActivityKind.Summary, "", 3, false, ""));
                    break;
            }

            await ShowCurrentAsync();
        }, "No se pudo abrir la actividad.");
    }

    /// <summary>
    /// Lección interactiva: explicación en tarjetas con huecos, predicción, ejemplo recorrido y ordenado, error
    /// habitual, búsqueda de errores, recuerdo y, al final, el primer ejercicio de código.
    /// </summary>
    private void LoadLessonFlow(Skill skill)
    {
        var lesson = skill.Lesson ?? throw new InvalidOperationException("Esta lección todavía está en preparación.");
        Title = skill.Title;
        var lessonIds = lesson.AllItemIds().ToHashSet();
        var extra = learning.Catalog.ItemsForSkill(skill.Id)
            .Where(i => !lessonIds.Contains(i.Id) && i.Purpose is ItemPurpose.BugFinding)
            .Select(i => i.Id).Take(1);
        var index = 0;
        void Add(ActivityKind kind, string refId, bool confidence = false) =>
            _steps.Add(new SessionActivity(index++, SessionBlockKind.NewConcept, kind, refId, 2, confidence, ""));

        Add(ActivityKind.ReadLesson, skill.Id);
        Add(ActivityKind.Item, lesson.PredictionItemId, confidence: true);
        Add(ActivityKind.StudyWorkedExample, skill.Id);
        foreach (var id in extra)
        {
            Add(ActivityKind.Item, id);
        }

        Add(ActivityKind.Item, lesson.RecallItemIds[0]);
        Add(ActivityKind.Item, lesson.GuidedExerciseId);
        Add(ActivityKind.Summary, "");
    }

    private string UnitTitle(Skill skill) =>
        learning.Catalog.Modules.SelectMany(m => m.Units).FirstOrDefault(u => u.Id == skill.UnitId)?.Title ?? "";

    private static bool AskConfidenceFor(ActivityMode mode, int index) => mode switch
    {
        // La confianza se pregunta de forma intermitente para practicar la metacognición sin volverla rutina.
        ActivityMode.Review => index % 2 == 1,
        ActivityMode.Lesson => index % 3 == 0,
        _ => index % 4 == 1,
    };

    private async Task LoadSessionAsync(ActivityRequest request)
    {
        var session = await learning.GetUnfinishedSessionAsync();
        if (session is null || (request.SessionId is { } id && session.Id != id))
        {
            throw new InvalidOperationException("La sesión ya no está disponible.");
        }

        if (session.Status == SessionStatus.Paused)
        {
            session = await learning.ResumeSessionAsync(session);
        }

        _session = session;
        Title = $"Sesión de {session.Plan.DurationMinutes} min";
        _steps.AddRange(session.Plan.Activities);
        _position = session.CurrentIndex;

        // Reconstruye los resultados de pasos previos para el resumen final.
        foreach (var outcome in session.Outcomes)
        {
            var activity = session.Plan.Activities.ElementAtOrDefault(outcome.Index);
            if (activity?.Kind == ActivityKind.Item && learning.Catalog.FindItem(activity.RefId) is { } item)
            {
                _results.Add((item, outcome.Correct));
            }
        }

        Tick();
    }

    private async Task LoadAssessmentAsync(ActivityRequest request)
    {
        var stored = request.AssessmentId is { } id ? await learning.Store.GetAssessmentAsync(id) : null;
        if (stored is null || stored.Assessment.Status != AssessmentStatus.InProgress)
        {
            throw new InvalidOperationException("La evaluación ya no está disponible.");
        }

        _assessment = stored.Assessment;
        Title = stored.Assessment.Title;
        for (var i = 0; i < stored.Assessment.ItemIds.Count; i++)
        {
            _steps.Add(new SessionActivity(i, SessionBlockKind.WrapUp, ActivityKind.Item, stored.Assessment.ItemIds[i], 2, i % 3 == 0, ""));
        }

        _position = stored.Assessment.CurrentIndex;
    }

    private ItemSettings SettingsFor(SessionActivity activity) => _request.Mode switch
    {
        ActivityMode.Assessment => new(AttemptContext.Assessment, _assessment?.Id.ToString(), ShowFeedback: false, AllowHints: false, activity.AskConfidence),
        ActivityMode.Session => new(AttemptContext.Session, _session?.Id.ToString(), true, true, activity.AskConfidence),
        ActivityMode.Review => new(AttemptContext.Review, null, true, true, activity.AskConfidence),
        ActivityMode.Lesson => new(AttemptContext.Lesson, null, true, true, activity.AskConfidence),
        _ => new(AttemptContext.FreePractice, null, true, true, activity.AskConfidence),
    };

    private async Task ShowCurrentAsync()
    {
        if (_position >= _steps.Count)
        {
            await FinishAsync();
            return;
        }

        var activity = _steps[_position];
        BlockLabel = IsAssessment ? "Evaluación" : _request.Mode == ActivityMode.Session ? Labels.Block(activity.Block) : null;
        var itemCount = _steps.Count(s => s.Kind == ActivityKind.Item);
        var itemNumber = _steps.Take(_position + 1).Count(s => s.Kind == ActivityKind.Item);
        ProgressText = activity.Kind == ActivityKind.Item ? $"Ítem {itemNumber} de {itemCount}" : $"Paso {_position + 1} de {_steps.Count}";

        switch (activity.Kind)
        {
            case ActivityKind.ReadLesson:
            case ActivityKind.StudyWorkedExample:
                var skill = learning.Catalog.FindSkill(activity.RefId);
                if (skill?.Lesson is null)
                {
                    await AdvanceAsync(null, null);
                    return;
                }

                if (activity.Kind == ActivityKind.ReadLesson)
                {
                    await learning.MarkLessonSeenAsync(skill.Id);
                }

                var explanation = _flow.Explanation(skill, skill.Lesson);
                var micro = activity.Kind == ActivityKind.ReadLesson
                    ? explanation
                    : _flow.Practice(skill, skill.Lesson, includeCloze: !LessonFlowBuilder.HasCloze(explanation));
                _micro.Clear();
                _micro.AddRange(micro.Select(m => MicroStepFactory.Create(m, UnitTitle(skill))));
                _microIndex = 0;
                ShowMicro();
                break;

            case ActivityKind.Item:
                var item = learning.Catalog.FindItem(activity.RefId);
                if (item is null)
                {
                    await AdvanceAsync(null, null);
                    return;
                }

                CurrentStep = await CreateItemAsync(item, SettingsFor(activity));
                break;

            default:
                CurrentStep = await BuildSummaryAsync();
                break;
        }

        UpdateProgress();
    }

    partial void OnCurrentStepChanged(StepViewModel? value)
    {
        if (value is SummaryStepViewModel)
        {
            Progress = 1;
        }
    }

    private void ShowMicro()
    {
        var step = _micro[_microIndex];
        step.OnCompleted = async result =>
        {
            await OnStepResultAsync(result);
            _microIndex++;
            if (_microIndex < _micro.Count)
            {
                ShowMicro();
                UpdateProgress();
            }
            else
            {
                _micro.Clear();
                await AdvanceAsync(null, null);
            }
        };
        CurrentStep = step;
    }

    private void UpdateProgress()
    {
        if (CurrentStep is SummaryStepViewModel)
        {
            Progress = 1;
            return;
        }

        var within = _micro.Count > 0 ? _microIndex / (double)_micro.Count : 0;
        Progress = Math.Clamp((_position + within) / Math.Max(1, _steps.Count(s => s.Kind != ActivityKind.Summary)), 0, 1);
    }

    /// <summary>Suma XP y lleva la cuenta de aciertos seguidos.</summary>
    private async Task OnStepResultAsync(StepResult result)
    {
        if (result.Correct == true)
        {
            Combo++;
        }
        else if (result.Correct == false)
        {
            Combo = 0;
        }

        if (result.Xp > 0)
        {
            XpEarned += result.Xp;
            await learning.Gamification.AwardAsync(result.Xp);
        }
    }

    private async Task<ItemViewModel> CreateItemAsync(LearningItem item, ItemSettings settings)
    {
        ItemViewModel vm = item switch
        {
            ChoiceItem c => new ChoiceItemViewModel(c, settings, RecordAsync, () => learning.Clock.Now),
            ShortAnswerItem s => new ShortAnswerItemViewModel(s, settings, RecordAsync, () => learning.Clock.Now),
            CodeExercise e => new CodeExerciseViewModel(e, settings, execution, learning.Store, RecordAsync, () => learning.Clock.Now),
            _ => throw new NotSupportedException(item.GetType().Name),
        };

        if (vm is CodeExerciseViewModel code)
        {
            await code.LoadAsync();
        }

        vm.OnCompleted = async result =>
        {
            _results.Add((item, result.Correct));
            await OnStepResultAsync(result);
            await AdvanceAsync(result.Correct, result.AttemptId);
        };
        return vm;
    }

    private async Task<AttemptOutcome?> RecordAsync(AttemptRecord attempt)
    {
        try
        {
            var outcome = await learning.RecordAttemptAsync(attempt);
            _messages.AddRange(outcome.Messages.Where(m => m.Contains("dominio", StringComparison.OrdinalIgnoreCase)
                                                            || m.Contains("Retención", StringComparison.OrdinalIgnoreCase)
                                                            || m.Contains("dificultad", StringComparison.OrdinalIgnoreCase)));
            return outcome;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            ErrorMessage = "No se pudo guardar el intento: " + ex.Message;
            return null;
        }
    }

    private async Task AdvanceAsync(bool? correct, Guid? attemptId)
    {
        var index = _position;
        if (_session is not null && !_session.IsFinished)
        {
            _session = await learning.CompleteActivityAsync(_session, new ActivityOutcome(index, correct, attemptId));
            if (!_session.IsFinished && SessionTracker.HardLimitReached(_session, learning.Clock.Now))
            {
                _session = await learning.FinishSessionEarlyAsync(_session);
                CurrentStep = await BuildSummaryAsync();
                return;
            }
        }
        else if (_assessment is not null)
        {
            _assessment = _assessment with { CurrentIndex = index + 1 };
            await learning.SaveAssessmentProgressAsync(_assessment);
        }

        _position++;
        await ShowCurrentAsync();
    }

    private async Task<SummaryStepViewModel> BuildSummaryAsync()
    {
        var snapshot = await learning.GetSnapshotAsync();
        var now = learning.Clock.Now;
        var attempted = _results.Where(r => r.Correct is not null).ToList();
        var correct = attempted.Count(r => r.Correct == true);
        if (_request.LessonSkillId is not null && !_bonusAwarded)
        {
            _bonusAwarded = true;
            var bonus = XpRewards.LessonCompleted + (attempted.Count > 0 && correct == attempted.Count ? XpRewards.PerfectLessonBonus : 0);
            XpEarned += bonus;
            await learning.Gamification.AwardAsync(bonus);
        }

        var state = await learning.Gamification.GetStateAsync();
        var summary = new SummaryStepViewModel(FinishAsync)
        {
            Title = _request.LessonSkillId is not null ? "¡Lección completada!" : IsSession ? "¡Sesión completada!" : "¡Buen trabajo!",
            Correct = correct,
            Attempted = attempted.Count,
            Xp = XpEarned,
            Streak = state.Streak,
            TodayXp = state.TodayXp,
            DailyGoal = state.DailyGoal,
            GoalReachedNow = _todayBefore < _goalBefore && state.TodayXp >= state.DailyGoal,
            TimeText = _session is null ? null : $"Tiempo activo: {Labels.Duration(SessionTracker.ActiveTime(_session, now))}",
        };

        foreach (var group in attempted.GroupBy(r => r.Item.SkillId))
        {
            var skill = learning.Catalog.FindSkill(group.Key);
            var progress = snapshot.Skill(group.Key);
            summary.Skills.Add(new SummarySkillLine(skill?.Title ?? group.Key, group.Count(r => r.Correct == true), group.Count(),
                Labels.Mastery(progress.Level), Labels.RelativeDue(progress.Review.DueAt, now)));
        }

        var recurring = new ErrorPatternAnalyzer().Find(snapshot.Attempts, now);
        foreach (var error in recurring.Take(3))
        {
            summary.Errors.Add($"{ErrorCategoryInfo.DisplayName(error.Category)} ({error.Count} veces en dos semanas): {ErrorCategoryInfo.Advice(error.Category)}");
        }

        foreach (var message in _messages.Distinct())
        {
            summary.Notes.Add(message);
        }

        if (summary.Attempted > 0 && summary.Correct < summary.Attempted)
        {
            summary.Notes.Add("Los ítems fallados volverán en próximas sesiones y repasos.");
        }

        return summary;
    }

    private async Task FinishAsync()
    {
        if (IsFinished)
        {
            return;
        }

        IsFinished = true;
        if (_assessment is not null)
        {
            var result = await learning.CompleteAssessmentAsync(_assessment);
            await navigation.GoToAsync(Routes.AssessmentResult, new Dictionary<string, object> { [Routes.AssessmentParameter] = result.AssessmentId });
            return;
        }

        if (_session is not null && !_session.IsFinished)
        {
            _session = await learning.CompleteActivityAsync(_session, new ActivityOutcome(_position, null, null));
        }

        await navigation.GoBackAsync();
    }

    /// <summary>Lo llama la vista una vez por segundo. El temporizador informa; no castiga.</summary>
    public void Tick()
    {
        if (_session is null || _session.IsFinished)
        {
            ElapsedText = null;
            return;
        }

        var now = learning.Clock.Now;
        var active = SessionTracker.ActiveTime(_session, now);
        ElapsedText = $"{Labels.Duration(active)} de {_session.Plan.DurationMinutes} min";
        if (SessionTracker.HardLimitReached(_session, now))
        {
            TimeNotice = "Llegaste al máximo de 90 minutos. Guarda lo que llevas y descansa: la sesión se cerrará al terminar este paso.";
        }
        else if (!_plannedNoticeShown && SessionTracker.PlannedTimeReached(_session, now))
        {
            _plannedNoticeShown = true;
            TimeNotice = "Se cumplió el tiempo planeado. Puedes terminar ahora o completar los pasos que faltan.";
        }
    }

    [RelayCommand]
    private async Task PauseAsync()
    {
        await SaveCurrentDraftAsync();
        if (_session is not null && !_session.IsFinished)
        {
            _session = await learning.PauseSessionAsync(_session);
        }

        await navigation.GoBackAsync();
    }

    [RelayCommand]
    private async Task FinishEarlyAsync()
    {
        if (_session is null || _session.IsFinished)
        {
            return;
        }

        if (!await dialogs.ConfirmAsync("Terminar sesión", "Se guardará lo que hiciste y verás el resumen. ¿Terminar ahora?", "Terminar", "Seguir"))
        {
            return;
        }

        await SaveCurrentDraftAsync();
        _session = await learning.FinishSessionEarlyAsync(_session);
        CurrentStep = await BuildSummaryAsync();
    }

    /// <summary>Salir de una práctica o evaluación (la evaluación se puede retomar desde Inicio).</summary>
    [RelayCommand]
    private async Task ExitAsync()
    {
        await SaveCurrentDraftAsync();
        if (_session is not null && !_session.IsFinished)
        {
            _session = await learning.PauseSessionAsync(_session);
        }

        await navigation.GoBackAsync();
    }

    public async Task SaveCurrentDraftAsync()
    {
        if (CurrentStep is CodeExerciseViewModel code)
        {
            await code.SaveDraftAsync();
            if (_session is not null && !_session.IsFinished)
            {
                _session = await learning.SaveSessionDraftAsync(_session, code.SourceCode);
            }
        }
    }

    /// <summary>La aplicación pasó a segundo plano: guarda y pausa sin perder nada.</summary>
    public async Task OnAppSleepAsync()
    {
        await SaveCurrentDraftAsync();
        if (_session is not null && _session.Status == SessionStatus.Active)
        {
            _session = await learning.PauseSessionAsync(_session);
        }
    }

    public async Task OnAppResumeAsync()
    {
        if (_session is not null && _session.Status == SessionStatus.Paused)
        {
            _session = await learning.ResumeSessionAsync(_session);
        }
    }
}
