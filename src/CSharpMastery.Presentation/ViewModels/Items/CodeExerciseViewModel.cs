using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Execution;
using CSharpMastery.Learning.Services;
using CSharpMastery.Presentation.Highlighting;
using CSharpMastery.Presentation.ViewModels.Activity;

namespace CSharpMastery.Presentation.ViewModels.Items;

public sealed record DiagnosticDisplay(string Location, string Code, string Message, string Explanation, bool IsError);

public sealed record TestDisplay(string Name, bool Passed, bool Hidden, bool EdgeCase, string? Input, string? Expected, string? Actual, string Message, string? Hint)
{
    public string Title => (Passed ? "✔ " : "✘ ") + Name + (Hidden ? " (oculta)" : "") + (EdgeCase ? " · caso límite" : "");

    public bool ShowComparison => !Hidden && !Passed && Expected is not null;
}

public sealed record SubmissionDisplay(DateTimeOffset At, bool Passed, double Score, string Summary, string Code)
{
    public string Title => $"{At.LocalDateTime:dd/MM HH:mm} · {(Passed ? "Aprobado" : $"{Score:P0} de pruebas")}";
}

/// <summary>
/// Ejercicio de programación: editor, ejecutar, comprobar con pruebas visibles y ocultas, pistas progresivas,
/// solución explicada (solo después de intentarlo y pedir pistas) e historial de entregas. Si el motor de ejecución
/// no está disponible, ofrece autoevaluación marcada como no verificada.
/// </summary>
public sealed partial class CodeExerciseViewModel : ItemViewModel
{
    private readonly CodeExercise _exercise;
    private readonly IExecutionService _execution;
    private readonly IProgressStore _store;
    private GradingResult? _last;
    private string? _savedDraft;

    public CodeExerciseViewModel(
        CodeExercise exercise,
        ItemSettings settings,
        IExecutionService execution,
        IProgressStore store,
        Func<AttemptRecord, Task<AttemptOutcome?>> record,
        Func<DateTimeOffset> now)
        : base(exercise, settings, record, now)
    {
        _exercise = exercise;
        _execution = execution;
        _store = store;
        SourceCode = exercise.StarterCode;
        UpdateFooter();
    }

    public CodeExercise Exercise => _exercise;

    public bool IsProgram => _exercise.Mode == CodeMode.Program;

    public bool EngineAvailable => _execution.IsAvailable;

    public string EngineMessage => _execution.AvailabilityMessage;

    public string CheckButtonText => Settings.IsAssessment ? "Entregar" : "Comprobar";

    public string RequiredConstructsText => _exercise.RequiredConstructs.Count == 0
        ? ""
        : "Debe usar: " + string.Join(", ", _exercise.RequiredConstructs.Select(CodeGrader.ConstructName));

    public IReadOnlyList<CodeTestCase> VisibleExamples => _exercise.Tests.Where(t => !t.Hidden).ToList();

    public int HiddenTestCount => _exercise.Tests.Count(t => t.Hidden);

    public ObservableCollection<DiagnosticDisplay> Diagnostics { get; } = [];

    public ObservableCollection<TestDisplay> TestResults { get; } = [];

    public ObservableCollection<SubmissionDisplay> History { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineNumbers))]
    public partial string SourceCode { get; set; }

    public string LineNumbers => CSharpHighlighter.LineNumbers(SourceCode);

    [ObservableProperty]
    public partial string Stdin { get; set; } = "";

    [ObservableProperty]
    public partial string ConsoleOutput { get; set; } = "";

    [ObservableProperty]
    public partial string? ResultSummary { get; set; }

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    [ObservableProperty]
    public partial bool SolutionVisible { get; set; }

    [ObservableProperty]
    public partial bool SelfCheckVisible { get; set; }

    public string ReferenceSolution => _exercise.ReferenceSolution;

    public string SolutionExplanation => string.IsNullOrWhiteSpace(_exercise.SolutionExplanation) ? _exercise.Explanation : _exercise.SolutionExplanation;

    /// <summary>La solución solo se ofrece tras al menos un intento y haber pedido las pistas disponibles (o una, si hay varias).</summary>
    public bool CanShowSolution => Settings.AllowHints && !Settings.IsAssessment && !SolutionVisible && EngineAvailable
                                   && Tries >= 1 && (RevealedHints.Count >= 1 || _exercise.Hints.Count == 0);

    public string SolutionGateText => Settings.IsAssessment || SolutionVisible || CanShowSolution || !EngineAvailable
        ? ""
        : Tries == 0 ? "La solución se habilita después de comprobar tu código al menos una vez." : "Pide al menos una pista antes de ver la solución.";

    protected override void OnHintsChanged() => RefreshGates();

    private void RefreshGates()
    {
        OnPropertyChanged(nameof(CanShowSolution));
        OnPropertyChanged(nameof(SolutionGateText));
        ShowSolutionCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Carga el borrador guardado y el historial de entregas.</summary>
    public async Task LoadAsync()
    {
        var draft = await _store.GetDraftAsync(_exercise.Id);
        if (!string.IsNullOrWhiteSpace(draft) && !Settings.IsAssessment)
        {
            SourceCode = draft;
            _savedDraft = draft;
        }

        History.Clear();
        foreach (var s in (await _store.GetSubmissionsAsync(_exercise.Id)).OrderByDescending(s => s.At).Take(10))
        {
            History.Add(new SubmissionDisplay(s.At, s.Passed, s.Score, s.Summary, s.Code));
        }

        // Se puede continuar sin resolver (queda pendiente y volverá en la práctica).
        CanContinue = true;
    }

    /// <summary>Guarda el borrador si cambió (la vista lo llama periódicamente y al salir).</summary>
    public async Task SaveDraftAsync()
    {
        if (SourceCode != _savedDraft)
        {
            _savedDraft = SourceCode;
            await _store.SaveDraftAsync(_exercise.Id, SourceCode);
        }
    }

    private bool CanExecuteCode() => EngineAvailable && !IsRunning;

    [RelayCommand(CanExecute = nameof(CanExecuteCode))]
    private async Task RunAsync()
    {
        await ExecuteGuardedAsync(async () =>
        {
            var result = await _execution.ExecuteAsync(CodeGrader.BuildRunRequest(_exercise, SourceCode, Stdin));
            var grading = CodeGrader.Grade(_exercise with { Tests = RunTests(result) }, result);
            ShowDiagnostics(grading);
            TestResults.Clear();
            ConsoleOutput = FormatRunOutput(result);
            ResultSummary = result.Status switch
            {
                ExecutionStatus.Completed => "Ejecución terminada. \"Ejecutar\" no corrige: usa \"" + CheckButtonText + "\" para pasar las pruebas.",
                _ => grading.Summary,
            };
        });
    }

    private IReadOnlyList<CodeTestCase> RunTests(ExecutionResult result) =>
        IsProgram ? [] : _exercise.Tests.Where(t => !t.Hidden).ToList();

    private static string FormatRunOutput(ExecutionResult result)
    {
        var sb = new StringBuilder();
        foreach (var test in result.Tests)
        {
            if (result.Tests.Count > 1 || test.ReturnValue is not null)
            {
                sb.AppendLine($"── {test.Name} ──");
            }

            sb.Append(test.Output);
            if (test.ReturnValue is not null)
            {
                sb.AppendLine($"Valor devuelto: {test.ReturnValue}");
            }

            if (test.Failure is { } f)
            {
                var explained = DiagnosticExplainer.ExplainException(f);
                sb.AppendLine($"Excepción {f.ExceptionType}{(f.Line is { } l ? $" en la línea {l}" : "")}: {f.Message}");
                sb.AppendLine(explained.Text);
            }

            if (test.TimedOut)
            {
                sb.AppendLine("Se detuvo por exceder el tiempo límite (¿un ciclo infinito?).");
            }

            if (test.OutputLimitExceeded)
            {
                sb.AppendLine("Se detuvo por producir demasiada salida.");
            }
        }

        return sb.ToString();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCode))]
    private async Task CheckAsync()
    {
        if (Settings.IsAssessment && HasSubmitted)
        {
            return;
        }

        if (ConfidenceMissing)
        {
            ResultSummary = "Antes de comprobar, indica qué tan seguro estás.";
            return;
        }

        await ExecuteGuardedAsync(async () =>
        {
            await SaveDraftAsync();
            var result = await _execution.ExecuteAsync(CodeGrader.BuildRequest(_exercise, SourceCode));
            var grading = CodeGrader.Grade(_exercise, result);
            if (!grading.Verified)
            {
                ResultSummary = grading.Summary;
                return;
            }

            _last = grading;
            Tries++;
            await _store.AddSubmissionAsync(new CodeSubmission(Guid.NewGuid(), _exercise.Id, SourceCode, grading.Passed, grading.Score, grading.Error, grading.Summary, Now));
            History.Insert(0, new SubmissionDisplay(Now, grading.Passed, grading.Score, grading.Summary, SourceCode));

            if (Settings.IsAssessment)
            {
                await RecordResultAsync(grading, solutionViewed: false);
                HasSubmitted = true;
                CanContinue = true;
                ResultSummary = "Entrega registrada. Verás los resultados al terminar la evaluación.";
                Diagnostics.Clear();
                TestResults.Clear();
                return;
            }

            ShowDiagnostics(grading);
            ShowTests(grading);
            ConsoleOutput = grading.ProgramOutput ?? "";
            ResultSummary = grading.Summary;
            FeedbackPositive = grading.Passed;
            HasSubmitted = true;
            CanContinue = true;
            if (grading.Passed)
            {
                IsSolved = true;
                ShowExplanation = true;
                await RecordResultAsync(grading, solutionViewed: false);
                Feedback = Tries == 1 && HintsUsed == 0
                    ? "¡Todas las pruebas pasan al primer intento y sin pistas!"
                    : "Todas las pruebas pasan.";
            }
            else
            {
                Feedback = "Revisa los resultados, corrige y vuelve a comprobar." + (HasMoreHints ? " Si te atoras, pide una pista." : "");
            }
        });
    }

    private Task RecordResultAsync(GradingResult grading, bool solutionViewed) =>
        RecordOnceAsync(a => a with
        {
            Correct = grading.Passed && !solutionViewed,
            Score = grading.Score,
            Error = grading.Passed ? ErrorCategory.None : grading.Error,
            Tries = Math.Max(1, Tries),
            SolutionViewed = solutionViewed,
            Answer = SourceCode,
        });

    [RelayCommand(CanExecute = nameof(CanShowSolution))]
    private async Task ShowSolutionAsync()
    {
        if (!CanShowSolution)
        {
            return;
        }

        SolutionVisible = true;
        ShowExplanation = true;
        CanContinue = true;
        if (!IsSolved && _last is not null)
        {
            await RecordResultAsync(_last, solutionViewed: true);
            Feedback = "Estudia la solución y vuelve a escribirla sin mirar: este ejercicio volverá a aparecer en tus repasos.";
        }

        RefreshGates();
    }

    [RelayCommand]
    private void ResetCode() => SourceCode = _exercise.StarterCode;

    // ---------- Autoevaluación cuando no hay motor de ejecución ----------

    [RelayCommand]
    private void StartSelfCheck()
    {
        SelfCheckVisible = true;
        SolutionVisible = true;
        ShowExplanation = true;
    }

    [RelayCommand]
    private async Task SelfAssessAsync(string? solved)
    {
        var ok = string.Equals(solved, "true", StringComparison.OrdinalIgnoreCase);
        await RecordOnceAsync(a => a with { Correct = ok, Score = ok ? 1 : 0, AutoGraded = false, Answer = SourceCode, SolutionViewed = true });
        Feedback = "Autoevaluación registrada como no verificada: no cuenta para el dominio de la habilidad.";
        CanContinue = true;
        SelfCheckVisible = false;
    }

    // ---------- Pie: Comprobar / Seguir intentando / Continuar ----------

    private enum FooterState
    {
        Editing,
        Failed,
        Solved,
        Finished,
        SelfCheck,
    }

    private FooterState State =>
        Settings.IsAssessment ? (HasSubmitted ? FooterState.Finished : FooterState.Editing)
        : !EngineAvailable ? (RecordedAttempt is not null ? FooterState.Finished : FooterState.SelfCheck)
        : IsSolved ? FooterState.Solved
        : SolutionVisible ? FooterState.Finished
        : HasSubmitted && Feedback is not null && !BannerDismissed ? FooterState.Failed
        : FooterState.Editing;

    protected override void UpdateFooter()
    {
        switch (State)
        {
            case FooterState.Solved:
                ShowBanner(BannerKind.Correct, PraiseFor(Item.Id + Tries), Feedback);
                PrimaryText = "Continuar";
                PrimaryEnabled = true;
                SecondaryText = null;
                break;
            case FooterState.Finished:
                ShowBanner(BannerKind.Info, Settings.IsAssessment ? "Entrega registrada" : SolutionVisible && !IsSolved ? "Estudia la solución" : null,
                    Settings.IsAssessment ? ResultSummary : Feedback);
                PrimaryText = "Continuar";
                PrimaryEnabled = true;
                SecondaryText = null;
                break;
            case FooterState.Failed:
                ShowBanner(BannerKind.Incorrect, "Aún no", string.Join("\n", new[] { ResultSummary, Feedback }.Where(t => !string.IsNullOrWhiteSpace(t))),
                    link: CanShowSolution ? "Ver la solución" : null);
                PrimaryText = "Seguir intentando";
                PrimaryEnabled = true;
                SecondaryText = "Saltar";
                break;
            case FooterState.SelfCheck:
                HideBanner();
                PrimaryText = SelfCheckVisible ? "Continuar sin autoevaluar" : "Ver solución y autoevaluar";
                PrimaryEnabled = true;
                SecondaryText = SelfCheckVisible ? null : "Saltar";
                break;
            default:
                HideBanner();
                PrimaryText = IsRunning ? "Comprobando…" : CheckButtonText;
                PrimaryEnabled = CanExecuteCode();
                SecondaryText = Settings.IsAssessment ? "Omitir" : "Saltar";
                break;
        }
    }

    protected override Task OnPrimaryAsync()
    {
        switch (State)
        {
            case FooterState.Solved:
            case FooterState.Finished:
                return ContinueCommand.ExecuteAsync(null);
            case FooterState.Failed:
                BannerDismissed = true;
                UpdateFooter();
                return Task.CompletedTask;
            case FooterState.SelfCheck when !SelfCheckVisible:
                StartSelfCheck();
                return Task.CompletedTask;
            case FooterState.SelfCheck:
                return ContinueCommand.ExecuteAsync(null);
            default:
                return CheckCommand.ExecuteAsync(null);
        }
    }

    /// <summary>Saltar: continuar sin resolver (el ejercicio volverá en la práctica).</summary>
    protected override Task OnSecondaryAsync() => ContinueCommand.ExecuteAsync(null);

    protected override Task OnBannerLinkAsync() => ShowSolutionCommand.ExecuteAsync(null);

    /// <summary>Al continuar sin resolver, el intento queda registrado con el último resultado.</summary>
    protected override Task BeforeContinueAsync() => FinalizeAsync();

    public async Task FinalizeAsync()
    {
        await SaveDraftAsync();
        if (RecordedAttempt is null && _last is not null)
        {
            await RecordResultAsync(_last, solutionViewed: false);
        }
    }

    private async Task ExecuteGuardedAsync(Func<Task> action)
    {
        IsRunning = true;
        RunCommand.NotifyCanExecuteChanged();
        CheckCommand.NotifyCanExecuteChanged();
        try
        {
            await action();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            ResultSummary = "No se pudo ejecutar el código: " + ex.Message;
        }
        finally
        {
            IsRunning = false;
            RunCommand.NotifyCanExecuteChanged();
            CheckCommand.NotifyCanExecuteChanged();
        }
    }

    private void ShowDiagnostics(GradingResult grading)
    {
        Diagnostics.Clear();
        foreach (var v in grading.Violations)
        {
            Diagnostics.Add(new DiagnosticDisplay($"Línea {v.Line}", "API", v.Api, v.Reason, true));
        }

        foreach (var d in grading.Diagnostics)
        {
            Diagnostics.Add(new DiagnosticDisplay(d.Raw.Line > 0 ? $"Línea {d.Raw.Line}, columna {d.Raw.Column}" : "", d.Raw.Id, d.Raw.Message, d.Explanation, d.Raw.Severity == "Error"));
        }
    }

    private void ShowTests(GradingResult grading)
    {
        TestResults.Clear();
        foreach (var t in grading.Tests)
        {
            TestResults.Add(new TestDisplay(t.Name, t.Passed, t.Hidden, t.EdgeCase, t.Input, t.Expected, t.Actual, t.Message, t.Hint));
        }
    }
}
