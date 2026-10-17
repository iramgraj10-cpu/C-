using CSharpMastery.Content;
using CSharpMastery.Domain.Assessments;
using CSharpMastery.Domain.Common;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Domain.Sessions;
using CSharpMastery.Execution;
using CSharpMastery.Learning.Interactive;
using CSharpMastery.Learning.Services;
using CSharpMastery.Presentation.Services;
using CSharpMastery.Presentation.ViewModels.Activity;
using CSharpMastery.Presentation.ViewModels.Items;
using CSharpMastery.Presentation.ViewModels.Pages;

namespace CSharpMastery.Tests.Presentation;

public sealed class FakeNavigation : INavigationService
{
    public List<(string Route, IDictionary<string, object>? Parameters)> Visits { get; } = [];

    public int BackCount { get; private set; }

    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        Visits.Add((route, parameters));
        return Task.CompletedTask;
    }

    public Task GoBackAsync()
    {
        BackCount++;
        return Task.CompletedTask;
    }

    public T LastParameter<T>(string key) => (T)Visits[^1].Parameters![key];
}

public sealed class FakeDialogs(bool confirm = true) : IDialogService
{
    public List<string> Alerts { get; } = [];

    public Task AlertAsync(string title, string message)
    {
        Alerts.Add(message);
        return Task.CompletedTask;
    }

    public Task<bool> ConfirmAsync(string title, string message, string accept = "Sí", string cancel = "Cancelar") => Task.FromResult(confirm);
}

public sealed class FakeFiles : IFileExchangeService
{
    public string? Exported { get; private set; }

    public string? ToImport { get; set; }

    public Task<string?> ExportAsync(string fileName, string content)
    {
        Exported = content;
        return Task.FromResult<string?>(fileName);
    }

    public Task<string?> ImportAsync() => Task.FromResult(ToImport);
}

/// <summary>Flujos de navegación y de interfaz probados sobre los ViewModels, con SQLite real y el ejecutor real.</summary>
public sealed class FlowTests : IDisposable
{
    private readonly TempDatabase _db = new();
    private readonly FakeClock _clock = FakeClock.At2026();
    private readonly FakeNavigation _nav = new();
    private readonly FakeDialogs _dialogs = new();

    public void Dispose() => _db.Dispose();

    private LearningService NewService() => new(CurriculumContent.Catalog, _db.Open(), _clock);

    private ActivityViewModel NewActivity(LearningService service, IExecutionService? execution = null) =>
        new(service, execution ?? Runners.Process, _nav, _dialogs);

    /// <summary>Responde el paso actual correctamente (o mal) usando la información del ítem.</summary>
    private async Task SolveCurrentAsync(ActivityViewModel activity, bool correct = true)
    {
        _clock.Advance(TimeSpan.FromMinutes(2));
        switch (activity.CurrentStep)
        {
            case MicroStepViewModel micro:
                await SolveMicroAsync(micro, correct);
                break;
            case ChoiceItemViewModel choice:
                Confide(choice);
                choice.Selected = choice.Options.First(o => ((ChoiceItem)choice.Item).Options[o.Index].IsCorrect == correct);
                await choice.SubmitCommand.ExecuteAsync(null);
                await choice.ContinueCommand.ExecuteAsync(null);
                break;
            case ShortAnswerItemViewModel shortAnswer:
                Confide(shortAnswer);
                shortAnswer.Answer = correct ? ((ShortAnswerItem)shortAnswer.Item).AcceptedAnswers[0] : "respuesta equivocada 123";
                await shortAnswer.SubmitCommand.ExecuteAsync(null);
                await shortAnswer.ContinueCommand.ExecuteAsync(null);
                break;
            case CodeExerciseViewModel code:
                Confide(code);
                code.SourceCode = correct ? code.Exercise.ReferenceSolution : code.Exercise.StarterCode;
                await code.CheckCommand.ExecuteAsync(null);
                await code.ContinueCommand.ExecuteAsync(null);
                break;
            default:
                throw new InvalidOperationException($"Paso inesperado: {activity.CurrentStep}");
        }
    }

    /// <summary>Resuelve un paso interactivo generado (bien o mal) y continúa.</summary>
    private static async Task SolveMicroAsync(MicroStepViewModel micro, bool correct = true)
    {
        switch (micro)
        {
            case ClozeViewModel cloze:
                var answer = ((ClozeStep)cloze.Step).Answer;
                cloze.ChooseCommand.Execute(cloze.Options.First(o => (o.Text == answer) == correct));
                break;
            case ParsonsViewModel parsons:
                var step = (ParsonsStep)parsons.Step;
                var order = correct ? step.MovableIndexes : step.BankOrder;
                foreach (var index in order)
                {
                    parsons.PlaceCommand.Execute(parsons.Bank.First(b => b.LineIndex == index && !b.IsUsed));
                }

                break;
            case SpotBugViewModel bug:
                var bugLines = ((SpotBugStep)bug.Step).BugLines;
                bug.TapCommand.Execute(bug.Lines.First(l => bugLines.Contains(l.Number) == correct && !string.IsNullOrWhiteSpace(l.Text)));
                break;
            case CompareCodeViewModel compare:
                compare.ChooseCommand.Execute(compare.Options.First(o => o.IsCorrect == correct));
                break;
        }

        // Comprobar (si aplica) y continuar hasta que el paso termine.
        var completed = false;
        micro.OnCompleted = WrapCompletion(micro.OnCompleted, () => completed = true);
        for (var guard = 0; !completed; guard++)
        {
            if (guard > 50 || !micro.PrimaryEnabled)
            {
                throw new InvalidOperationException($"El paso {micro.Step.Key} no avanza.");
            }

            await micro.PrimaryCommand.ExecuteAsync(null);
        }
    }

    private static Func<StepResult, Task> WrapCompletion(Func<StepResult, Task>? inner, Action done) => async result =>
    {
        done();
        if (inner is not null)
        {
            await inner(result);
        }
    };

    private static void Confide(ItemViewModel item)
    {
        if (item.AskConfidence)
        {
            item.SelectedConfidence = item.ConfidenceOptions[2];
        }
    }

    [Fact]
    public async Task Bienvenida_crea_perfil_y_lleva_al_inicio()
    {
        var service = NewService();
        var home = new HomeViewModel(service, _nav, _dialogs);
        Assert.False(await home.LoadAsync());

        var onboarding = new OnboardingViewModel(service, _nav, new NoOpThemeService())
        {
            DisplayName = "Ana",
            SessionMinutes = 25,
        };
        onboarding.SelectedLevel = onboarding.Levels[1];
        onboarding.Goals[0].IsSelected = true;
        await onboarding.StartCommand.ExecuteAsync(null);

        Assert.Equal(Routes.Home, _nav.Visits[^1].Route);
        var profile = await service.Store.GetProfileAsync();
        Assert.Equal("Ana", profile!.DisplayName);
        Assert.Equal(StartingLevel.OtherLanguage, profile.InitialLevel);
        Assert.Single(profile.Goals);

        Assert.True(await home.LoadAsync());
        Assert.Equal("Hola, Ana", home.Greeting);
        Assert.Equal(25, home.SelectedMinutes);
        Assert.NotNull(home.PlacementHint);
        Assert.NotEmpty(home.PlanPreview);
    }

    [Fact]
    public async Task Duracion_personalizada_fuera_de_rango_se_rechaza()
    {
        var service = NewService();
        await service.GetOrCreateProfileAsync();
        var home = new HomeViewModel(service, _nav, _dialogs);
        await home.LoadAsync();

        home.CustomMinutes = "120";
        home.ApplyCustomMinutesCommand.Execute(null);
        Assert.NotNull(home.MinutesError);
        Assert.Equal(45, home.SelectedMinutes);

        home.CustomMinutes = "60";
        home.ApplyCustomMinutesCommand.Execute(null);
        Assert.Null(home.MinutesError);
        Assert.Equal(60, home.SelectedMinutes);
    }

    [Fact]
    public async Task Sesion_completa_desde_inicio_hasta_resumen()
    {
        var service = NewService();
        await service.GetOrCreateProfileAsync();
        var home = new HomeViewModel(service, _nav, _dialogs);
        await home.LoadAsync();
        home.SelectPresetCommand.Execute(25);
        await home.StartSessionCommand.ExecuteAsync(null);

        Assert.Equal(Routes.Activity, _nav.Visits[^1].Route);
        var request = _nav.LastParameter<ActivityRequest>(Routes.RequestParameter);
        Assert.Equal(ActivityMode.Session, request.Mode);

        var activity = NewActivity(service);
        await activity.LoadAsync(request);
        Assert.Null(activity.ErrorMessage);

        var guard = 0;
        while (activity.CurrentStep is not SummaryStepViewModel)
        {
            await SolveCurrentAsync(activity);
            Assert.True(++guard < 60, "La sesión no terminó.");
        }

        var summary = (SummaryStepViewModel)activity.CurrentStep;
        Assert.True(summary.Attempted > 0);
        Assert.Equal(summary.Attempted, summary.Correct);
        Assert.NotEmpty(summary.Skills);

        await summary.FinishCommand.ExecuteAsync(null);
        Assert.Equal(1, _nav.BackCount);
        Assert.Null(await service.GetUnfinishedSessionAsync());
        var sessions = await service.Store.GetSessionsAsync();
        Assert.Equal(SessionStatus.Completed, Assert.Single(sessions).Status);
        Assert.NotEmpty(await service.Store.GetAttemptsAsync(contextId: sessions[0].Id.ToString()));
    }

    [Fact]
    public async Task Sesion_se_pausa_y_se_retoma_en_el_mismo_paso()
    {
        var service = NewService();
        var session = await service.StartSessionAsync(25, seed: 3);
        var activity = NewActivity(service);
        await activity.LoadAsync(new ActivityRequest(ActivityMode.Session, [], session.Id));
        for (var guard = 0; activity.Session!.CurrentIndex < 2 && guard < 60; guard++)
        {
            // Un paso de la sesión puede tener varios pasos interactivos (la explicación en tarjetas).
            await SolveCurrentAsync(activity);
        }

        await activity.PauseCommand.ExecuteAsync(null);

        var paused = await service.GetUnfinishedSessionAsync();
        Assert.Equal(SessionStatus.Paused, paused!.Status);
        Assert.Equal(2, paused.CurrentIndex);

        // Tiempo en pausa no cuenta como tiempo activo.
        _clock.Advance(TimeSpan.FromHours(5));
        var resumed = NewActivity(service);
        await resumed.LoadAsync(new ActivityRequest(ActivityMode.Session, [], session.Id));
        Assert.Equal(SessionStatus.Active, resumed.Session!.Status);
        Assert.Equal(2, resumed.Session.CurrentIndex);
        Assert.True(resumed.Session.ActiveTime < TimeSpan.FromHours(2));

        var home = new HomeViewModel(service, _nav, _dialogs);
        await service.GetOrCreateProfileAsync();
        await home.LoadAsync();
        Assert.True(home.HasUnfinishedSession);
    }

    [Fact]
    public async Task Evaluacion_oculta_resultados_hasta_el_final_y_muestra_recuperacion()
    {
        var service = NewService();
        var unitId = CurriculumContent.Catalog.Modules[0].Units[1].Id;
        var curriculum = new CurriculumViewModel(service, _nav, _dialogs);
        await curriculum.LoadAsync();
        var unit = curriculum.Modules[0].Units.Single(u => u.Id == unitId);
        await curriculum.AssessUnitCommand.ExecuteAsync(unit);

        var request = _nav.LastParameter<ActivityRequest>(Routes.RequestParameter);
        Assert.Equal(ActivityMode.Assessment, request.Mode);
        var activity = NewActivity(service);
        await activity.LoadAsync(request);

        var answered = 0;
        while (_nav.Visits[^1].Route != Routes.AssessmentResult)
        {
            var item = Assert.IsAssignableFrom<ItemViewModel>(activity.CurrentStep);
            Assert.False(item.HasMoreHints);
            await SolveCurrentAsync(activity, correct: answered % 2 == 0);
            if (item is not CodeExerciseViewModel)
            {
                Assert.Null(item.FeedbackPositive);
            }

            answered++;
            Assert.True(answered < 40);
        }

        var id = _nav.LastParameter<Guid>(Routes.AssessmentParameter);
        var result = new AssessmentResultViewModel(service, _nav);
        await result.LoadAsync(id);
        Assert.Null(result.ErrorMessage);
        Assert.False(result.Passed);
        Assert.NotEmpty(result.Skills);
        Assert.True(result.HasRecovery);

        await result.RetakeCommand.ExecuteAsync(null);
        var retake = _nav.LastParameter<ActivityRequest>(Routes.RequestParameter);
        Assert.NotEqual(request.AssessmentId, retake.AssessmentId);
        var stored = await service.Store.GetAssessmentsAsync();
        Assert.Contains(stored, s => s.Assessment.Status == AssessmentStatus.Completed);
        Assert.Contains(stored, s => s.Assessment.Status == AssessmentStatus.InProgress);
    }

    [Fact]
    public async Task Reintentos_de_una_pregunta_registran_un_solo_intento()
    {
        var service = NewService();
        var item = (ChoiceItem)CurriculumContent.Catalog.GetItem("n0.variables.pred1");
        var settings = new ItemSettings(AttemptContext.Lesson, null, true, true, false);
        var vm = new ChoiceItemViewModel(item, settings, a => service.RecordAttemptAsync(a).ContinueWith(t => (AttemptOutcome?)t.Result), () => _clock.Now);

        vm.Selected = vm.Options.First(o => !item.Options[o.Index].IsCorrect);
        await vm.SubmitCommand.ExecuteAsync(null);
        Assert.False(vm.FeedbackPositive);
        Assert.False(vm.IsSolved);

        vm.Selected = vm.Options.First(o => item.Options[o.Index].IsCorrect);
        await vm.SubmitCommand.ExecuteAsync(null);
        Assert.True(vm.FeedbackPositive);
        Assert.True(vm.ShowExplanation);

        var attempt = Assert.Single(await service.Store.GetAttemptsAsync());
        Assert.False(attempt.Correct);
        Assert.Equal(ErrorCategory.PredictionError, attempt.Error);
    }

    [Fact]
    public async Task La_solucion_requiere_intentar_y_pedir_pista()
    {
        var service = NewService();
        var exercise = (CodeExercise)CurriculumContent.Catalog.GetItem("n0.variables.indep");
        var vm = new CodeExerciseViewModel(exercise, new ItemSettings(AttemptContext.Lesson, null, true, true, false), Runners.Process, service.Store,
            async a => await service.RecordAttemptAsync(a), () => _clock.Now);
        await vm.LoadAsync();

        Assert.False(vm.CanShowSolution);
        Assert.Contains("comprobar", vm.SolutionGateText);

        await vm.CheckCommand.ExecuteAsync(null);
        Assert.Equal(1, vm.Tries);
        Assert.False(vm.IsSolved);
        Assert.NotEmpty(vm.TestResults.Concat<object>(vm.Diagnostics));
        Assert.Equal(exercise.Hints.Count > 0, !vm.CanShowSolution);

        if (exercise.Hints.Count > 0)
        {
            vm.RevealHintCommand.Execute(null);
            Assert.True(vm.CanShowSolution);
        }

        await vm.ShowSolutionCommand.ExecuteAsync(null);
        Assert.True(vm.SolutionVisible);

        var attempt = Assert.Single(await service.Store.GetAttemptsAsync());
        Assert.False(attempt.Correct);
        Assert.True(attempt.SolutionViewed);
        Assert.Single(await service.Store.GetSubmissionsAsync(exercise.Id));
    }

    [Fact]
    public async Task Borrador_de_codigo_se_recupera()
    {
        var service = NewService();
        var exercise = (CodeExercise)CurriculumContent.Catalog.GetItem("n0.variables.indep");
        var settings = new ItemSettings(AttemptContext.Lesson, null, true, true, false);
        var vm = new CodeExerciseViewModel(exercise, settings, Runners.Process, service.Store, async a => await service.RecordAttemptAsync(a), () => _clock.Now);
        await vm.LoadAsync();
        vm.SourceCode = "// mi avance\n" + exercise.StarterCode;
        await vm.SaveDraftAsync();

        var again = new CodeExerciseViewModel(exercise, settings, Runners.Process, service.Store, async a => await service.RecordAttemptAsync(a), () => _clock.Now);
        await again.LoadAsync();
        Assert.StartsWith("// mi avance", again.SourceCode);
    }

    [Fact]
    public async Task Sin_motor_de_ejecucion_la_autoevaluacion_no_cuenta_como_verificada()
    {
        var service = NewService();
        var exercise = (CodeExercise)CurriculumContent.Catalog.GetItem("n0.variables.indep");
        var engine = new UnavailableExecutionService("No disponible en este dispositivo.");
        var vm = new CodeExerciseViewModel(exercise, new ItemSettings(AttemptContext.Lesson, null, true, true, false), engine, service.Store,
            async a => await service.RecordAttemptAsync(a), () => _clock.Now);
        await vm.LoadAsync();

        Assert.False(vm.EngineAvailable);
        Assert.False(vm.RunCommand.CanExecute(null));
        vm.StartSelfCheckCommand.Execute(null);
        Assert.True(vm.SolutionVisible);
        await vm.SelfAssessCommand.ExecuteAsync("true");

        var attempt = Assert.Single(await service.Store.GetAttemptsAsync());
        Assert.False(attempt.AutoGraded);
        Assert.True(attempt.Correct);
        Assert.True((await service.Store.GetSkillAsync(exercise.SkillId)).Level < MasteryLevel.ProvisionallyMastered);
    }

    [Fact]
    public async Task Repaso_desde_inicio_usa_habilidades_vencidas()
    {
        var service = NewService();
        await service.GetOrCreateProfileAsync();
        await service.RecordAttemptAsync(Attempts.Make("n0.variables", "n0.variables.pred1", true, _clock.Now, ItemPurpose.Prediction, false));
        _clock.Advance(TimeSpan.FromDays(2));

        var home = new HomeViewModel(service, _nav, _dialogs);
        await home.LoadAsync();
        Assert.Equal(1, home.DueReviews);
        await home.StartReviewCommand.ExecuteAsync(null);

        var request = _nav.LastParameter<ActivityRequest>(Routes.RequestParameter);
        Assert.Equal(ActivityMode.Review, request.Mode);
        Assert.All(request.ItemIds, id => Assert.Equal("n0.variables", CurriculumContent.Catalog.GetItem(id).SkillId));
    }

    [Fact]
    public async Task Ajustes_validan_criterios_y_exportan_e_importan()
    {
        var service = NewService();
        var files = new FakeFiles();
        var settings = new SettingsViewModel(service, Runners.Process, new NoOpThemeService(), files, _dialogs);
        await settings.LoadAsync();

        settings.MinAccuracyPercent = "150";
        await settings.SaveCriteriaCommand.ExecuteAsync(null);
        Assert.Equal(MasteryCriteria.Default, await service.GetCriteriaAsync());

        settings.MinAccuracyPercent = "90";
        await settings.SaveCriteriaCommand.ExecuteAsync(null);
        Assert.Equal(0.9, (await service.GetCriteriaAsync()).MinAccuracy);

        await service.RecordAttemptAsync(Attempts.Make("n0.variables", "n0.variables.pred1", true, _clock.Now, ItemPurpose.Prediction, false));
        await settings.ExportCommand.ExecuteAsync(null);
        Assert.NotNull(files.Exported);

        using var other = new TempDatabase();
        var target = new LearningService(CurriculumContent.Catalog, other.Open(), _clock);
        var targetSettings = new SettingsViewModel(target, Runners.Process, new NoOpThemeService(), new FakeFiles { ToImport = files.Exported }, _dialogs);
        await targetSettings.ImportCommand.ExecuteAsync(null);
        Assert.Null(targetSettings.ErrorMessage);
        Assert.Single(await target.Store.GetAttemptsAsync());
        Assert.Equal(0.9, (await target.GetCriteriaAsync()).MinAccuracy);

        await settings.TestEngineCommand.ExecuteAsync(null);
        Assert.StartsWith("Funciona", settings.EngineTestResult);
    }

    [Fact]
    public async Task Leccion_abre_tarjeta_y_lanza_leccion_interactiva_y_practica()
    {
        var service = NewService();
        var lesson = new LessonViewModel(service, _nav);
        await lesson.LoadAsync("n0.variables");
        Assert.True(lesson.HasLesson);
        Assert.Equal(MasteryLevel.NotStarted, (await service.Store.GetSkillAsync("n0.variables")).Level);

        await lesson.ToggleNotesCommand.ExecuteAsync(null);
        Assert.Equal(MasteryLevel.Seen, (await service.Store.GetSkillAsync("n0.variables")).Level);

        await lesson.StartLessonCommand.ExecuteAsync(null);
        var request = _nav.LastParameter<ActivityRequest>(Routes.RequestParameter);
        Assert.Equal(ActivityMode.Lesson, request.Mode);
        Assert.Equal("n0.variables", request.LessonSkillId);

        await lesson.PracticeCommand.ExecuteAsync(null);
        request = _nav.LastParameter<ActivityRequest>(Routes.RequestParameter);
        Assert.Equal("n0.variables.pred1", request.ItemIds[0]);

        var outline = new LessonViewModel(service, _nav);
        await outline.LoadAsync(CurriculumContent.Catalog.OrderedSkills.Last().Id);
        Assert.Null(outline.ErrorMessage);
    }

    [Fact]
    public async Task Leccion_interactiva_completa_da_xp_y_racha()
    {
        var service = NewService();
        var activity = NewActivity(service);
        await activity.LoadAsync(new ActivityRequest(ActivityMode.Lesson, [], LessonSkillId: "n1.for"));
        Assert.Null(activity.ErrorMessage);
        Assert.IsType<IntroStepViewModel>(activity.CurrentStep);
        Assert.Equal(0, activity.Progress);

        var kinds = new List<string>();
        var guard = 0;
        while (activity.CurrentStep is not SummaryStepViewModel && guard++ < 60)
        {
            kinds.Add(activity.CurrentStep!.GetType().Name);
            var progress = activity.Progress;
            await SolveCurrentAsync(activity);
            Assert.True(activity.Progress >= progress);
        }

        Assert.Contains(nameof(ConceptCardViewModel), kinds);
        Assert.Contains(nameof(ClozeViewModel), kinds);
        Assert.Contains(nameof(WalkthroughViewModel), kinds);
        Assert.Contains(kinds, k => k is nameof(SpotBugViewModel) or nameof(CompareCodeViewModel));
        Assert.Contains(nameof(CodeExerciseViewModel), kinds);

        var summary = Assert.IsType<SummaryStepViewModel>(activity.CurrentStep);
        Assert.Equal(1, activity.Progress);
        Assert.Equal("¡Lección completada!", summary.Title);
        Assert.True(summary.Xp > XpRewards.LessonCompleted);
        Assert.Equal(summary.Attempted, summary.Correct);
        Assert.Equal(1, summary.Streak);
        var state = await service.Gamification.GetStateAsync();
        Assert.Equal(summary.Xp, state.TotalXp);
        Assert.True((await service.Store.GetSkillAsync("n1.for")).Level >= MasteryLevel.Seen);
    }

    [Fact]
    public async Task Un_hueco_fallado_muestra_la_respuesta_y_no_da_xp()
    {
        var builder = new LessonFlowBuilder(CurriculumContent.InteractiveExclusions);
        var skill = CurriculumContent.Catalog.GetSkill("n1.for");
        var cloze = builder.Explanation(skill, skill.Lesson!).OfType<ClozeStep>().First();
        var vm = new ClozeViewModel(cloze);
        Assert.Equal("Comprobar", vm.PrimaryText);
        Assert.False(vm.PrimaryEnabled);
        vm.ChooseCommand.Execute(vm.Options.First(o => o.Text != cloze.Answer));
        await vm.PrimaryCommand.ExecuteAsync(null);
        Assert.Equal(BannerKind.Incorrect, vm.Banner);
        Assert.Contains(cloze.Answer, vm.BannerText);
        Assert.Equal("Continuar", vm.PrimaryText);
        Assert.Equal(ChipState.Correct, vm.Options.Single(o => o.Text == cloze.Answer).State);
        StepResult? result = null;
        vm.OnCompleted = r => { result = r; return Task.CompletedTask; };
        await vm.PrimaryCommand.ExecuteAsync(null);
        Assert.False(result!.Correct);
        Assert.Equal(0, result.Xp);
    }

    [Fact]
    public async Task El_pie_de_una_pregunta_pasa_de_comprobar_a_reintentar_y_continuar()
    {
        var service = NewService();
        var activity = NewActivity(service);
        await activity.LoadAsync(new ActivityRequest(ActivityMode.Practice, ["n1.for.recall1"]));
        var choice = Assert.IsType<ChoiceItemViewModel>(activity.CurrentStep);
        Assert.Equal("Comprobar", choice.PrimaryText);
        Assert.False(choice.PrimaryEnabled);
        Confide(choice);
        choice.SelectCommand.Execute(choice.Options.First(o => !((ChoiceItem)choice.Item).Options[o.Index].IsCorrect));
        Assert.True(choice.PrimaryEnabled);
        await choice.PrimaryCommand.ExecuteAsync(null);
        Assert.Equal(BannerKind.Incorrect, choice.Banner);
        Assert.Equal("Reintentar", choice.SecondaryText);
        Assert.Equal("Ver la respuesta", choice.BannerLinkText);
        await choice.SecondaryCommand.ExecuteAsync(null);
        Assert.Equal(BannerKind.None, choice.Banner);
        choice.SelectCommand.Execute(choice.Options.First(o => ((ChoiceItem)choice.Item).Options[o.Index].IsCorrect));
        await choice.PrimaryCommand.ExecuteAsync(null);
        Assert.Equal(BannerKind.Correct, choice.Banner);
        Assert.Equal("Continuar", choice.PrimaryText);
        await choice.PrimaryCommand.ExecuteAsync(null);
        var summary = Assert.IsType<SummaryStepViewModel>(activity.CurrentStep);
        Assert.Equal(0, summary.Correct); // el primer intento fue incorrecto
        Assert.Equal(0, summary.Xp);
    }

    [Fact]
    public async Task Panel_de_progreso_separa_estados()
    {
        var service = NewService();
        await service.MarkLessonSeenAsync("n0.programa");
        await service.RecordAttemptAsync(Attempts.Make("n0.variables", "n0.variables.pred1", true, _clock.Now, ItemPurpose.Prediction, false));
        var progress = new ProgressViewModel(service, _nav);
        await progress.LoadAsync();
        Assert.Null(progress.ErrorMessage);
        Assert.Contains(progress.Content, c => c.Label == "Lecciones vistas" && c.Value == "2");
        Assert.Contains(progress.Mastery, c => c.Label == "Retenidas");
        Assert.Equal(2, progress.Skills.Count);
        progress.ShowOnlyStarted = false;
        Assert.Equal(CurriculumContent.Catalog.OrderedSkills.Count, progress.Skills.Count);
    }
}
