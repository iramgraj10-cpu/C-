using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Services;
using CSharpMastery.Presentation.Formatting;
using CSharpMastery.Presentation.ViewModels.Activity;
using CSharpMastery.Presentation.ViewModels.Display;

namespace CSharpMastery.Presentation.ViewModels.Items;

/// <summary>Cómo se presenta un ítem según el contexto (lección, repaso, evaluación...).</summary>
public sealed record ItemSettings(
    AttemptContext Context,
    string? ContextId,
    bool ShowFeedback,
    bool AllowHints,
    bool AskConfidence)
{
    public bool IsAssessment => Context == AttemptContext.Assessment;
}

/// <summary>
/// Base común de ítems: pistas progresivas, confianza opcional, retroalimentación inmediata y registro de un único
/// intento por presentación (los reintentos sirven para aprender, pero no inflan las estadísticas).
/// </summary>
public abstract partial class ItemViewModel : StepViewModel
{
    private readonly Func<AttemptRecord, Task<AttemptOutcome?>> _record;
    private readonly Stopwatch _watch = Stopwatch.StartNew();
    private readonly Func<DateTimeOffset> _now;

    protected ItemViewModel(LearningItem item, ItemSettings settings, Func<AttemptRecord, Task<AttemptOutcome?>> record, Func<DateTimeOffset> now)
    {
        Item = item;
        Settings = settings;
        _record = record;
        _now = now;
        ConfidenceOptions = Enumerable.Range(1, 4).Select(v => new OptionDisplay<int>(v, Labels.Confidence(v))).ToList();
    }

    public LearningItem Item { get; }

    public ItemSettings Settings { get; }

    public string PurposeLabel => Labels.Purpose(Item.Purpose);

    public string Prompt => Item.Prompt;

    public string? Code => Item.Code;

    public bool HasCode => !string.IsNullOrWhiteSpace(Item.Code);

    public string Explanation => Item.Explanation;

    public IReadOnlyList<OptionDisplay<int>> ConfidenceOptions { get; }

    public bool AskConfidence => Settings.AskConfidence;

    public ObservableCollection<string> RevealedHints { get; } = [];

    public ObservableCollection<string> Messages { get; } = [];

    [ObservableProperty]
    public partial OptionDisplay<int>? SelectedConfidence { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RevealHintCommand))]
    public partial bool IsSolved { get; set; }

    [ObservableProperty]
    public partial bool HasSubmitted { get; set; }

    [ObservableProperty]
    public partial string? Feedback { get; set; }

    [ObservableProperty]
    public partial bool? FeedbackPositive { get; set; }

    [ObservableProperty]
    public partial bool ShowExplanation { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    public partial bool CanContinue { get; set; }

    [ObservableProperty]
    public partial int Tries { get; set; }

    public int HintsUsed => RevealedHints.Count;

    public bool HasMoreHints => Settings.AllowHints && !IsSolved && RevealedHints.Count < Item.Hints.Count;

    public string HintButtonText => RevealedHints.Count == 0 ? "Pedir una pista" : $"Otra pista ({RevealedHints.Count}/{Item.Hints.Count})";

    public bool ConfidenceMissing => AskConfidence && SelectedConfidence is null;

    protected AttemptRecord? RecordedAttempt { get; private set; }

    protected TimeSpan Elapsed => _watch.Elapsed;

    protected DateTimeOffset Now => _now();

    [RelayCommand(CanExecute = nameof(HasMoreHints))]
    private void RevealHint()
    {
        if (!HasMoreHints)
        {
            return;
        }

        RevealedHints.Add(Item.Hints[RevealedHints.Count]);
        OnPropertyChanged(nameof(HintsUsed));
        OnPropertyChanged(nameof(HasMoreHints));
        OnPropertyChanged(nameof(HintButtonText));
        OnHintsChanged();
    }

    protected virtual void OnHintsChanged()
    {
    }

    partial void OnTriesChanged(int value)
    {
        BannerDismissed = false;
        OnHintsChanged();
    }

    partial void OnSelectedConfidenceChanged(OptionDisplay<int>? value) => OnPropertyChanged(nameof(ConfidenceMissing));

    partial void OnIsSolvedChanged(bool value) => OnPropertyChanged(nameof(HasMoreHints));

    [RelayCommand(CanExecute = nameof(CanContinue))]
    private async Task ContinueAsync()
    {
        await BeforeContinueAsync();
        if (RecordedAttempt is { Correct: true, AutoGraded: true })
        {
            XpEarned = Item.IsCode ? XpRewards.CodeExercise : XpRewards.Item;
        }

        await CompleteAsync(RecordedAttempt?.Correct, RecordedAttempt?.Id);
    }

    // ---------- Pie de la pantalla: Comprobar / Continuar y franja de retroalimentación ----------

    /// <summary>El estudiante cerró la franja de error para reintentar.</summary>
    protected bool BannerDismissed { get; set; }

    /// <summary>Hay un resultado a la vista y se puede seguir.</summary>
    protected bool ShowingResult => Feedback is not null && !BannerDismissed && CanContinue;

    protected virtual bool CanSubmitNow => false;

    protected virtual Task SubmitNowAsync() => Task.CompletedTask;

    protected override Task OnPrimaryAsync() => ShowingResult ? ContinueCommand.ExecuteAsync(null) : SubmitNowAsync();

    protected override Task OnSecondaryAsync()
    {
        BannerDismissed = true;
        UpdateFooter();
        return Task.CompletedTask;
    }

    private static readonly HashSet<string> FooterProperties =
    [
        nameof(Feedback), nameof(FeedbackPositive), nameof(CanContinue), nameof(IsSolved), nameof(HasSubmitted),
        nameof(ShowExplanation), nameof(Tries), nameof(IsBusy),
        "SolutionVisible", "SelfCheckVisible", "IsRunning", "ResultSummary",
    ];

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is { } name && (FooterProperties.Contains(name) || name.StartsWith("Selected", StringComparison.Ordinal) || name == "Answer"))
        {
            UpdateFooter();
        }
    }

    /// <summary>Recalcula el pie según el estado del ítem.</summary>
    protected virtual void UpdateFooter()
    {
        if (Feedback is null || BannerDismissed)
        {
            HideBanner();
        }
        else
        {
            var kind = FeedbackPositive switch { true => BannerKind.Correct, false => BannerKind.Incorrect, _ => BannerKind.Info };
            var title = kind switch
            {
                BannerKind.Correct => PraiseFor(Item.Id + Tries),
                BannerKind.Incorrect => "Respuesta incorrecta",
                _ => Settings.ShowFeedback ? null : "Respuesta registrada",
            };
            var text = Feedback;
            if (ShowExplanation && !string.IsNullOrWhiteSpace(Explanation))
            {
                text += "\n\n" + Explanation;
            }

            if (Messages.Count > 0)
            {
                text += "\n\n" + string.Join("\n", Messages.Select(m => "• " + m));
            }

            var link = kind == BannerKind.Incorrect && !IsSolved && Settings.ShowFeedback && OffersAnswer ? "Ver la respuesta" : null;
            ShowBanner(kind, title, text?.Trim(), link: link);
        }

        PrimaryText = ShowingResult ? "Continuar" : Settings.IsAssessment ? "Responder" : "Comprobar";
        PrimaryEnabled = ShowingResult ? CanContinue : CanSubmitNow && !IsBusy;
        SecondaryText = ShowingResult && FeedbackPositive == false && !IsSolved && Settings.ShowFeedback ? "Reintentar" : null;
    }

    /// <summary>Si la franja ofrece «Ver la respuesta» tras fallar.</summary>
    protected virtual bool OffersAnswer => false;

    protected virtual Task BeforeContinueAsync() => Task.CompletedTask;

    /// <summary>Registra el intento (una sola vez por presentación) y muestra los mensajes del sistema de aprendizaje.</summary>
    protected async Task RecordOnceAsync(Func<AttemptRecord, AttemptRecord> configure)
    {
        if (RecordedAttempt is not null)
        {
            return;
        }

        var attempt = configure(new AttemptRecord
        {
            ItemId = Item.Id,
            SkillId = Item.SkillId,
            RelatedSkillIds = Item.RelatedSkillIds,
            Purpose = Item.Purpose,
            Context = Settings.Context,
            ContextId = Settings.ContextId,
            Correct = false,
            HintsUsed = HintsUsed,
            Duration = Elapsed,
            Confidence = SelectedConfidence?.Value,
            IsCode = Item.IsCode,
            At = Now,
        });
        RecordedAttempt = attempt;
        var outcome = await _record(attempt);
        if (outcome is not null && Settings.ShowFeedback)
        {
            foreach (var message in outcome.Messages)
            {
                Messages.Add(message);
            }

            UpdateFooter();
        }
    }
}
