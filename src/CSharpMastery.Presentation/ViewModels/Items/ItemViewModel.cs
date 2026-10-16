using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Services;
using CSharpMastery.Presentation.Formatting;
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

/// <summary>Resultado de un ítem al pulsar "Continuar". Correct es null si no se intentó.</summary>
public sealed record ItemResult(bool? Correct, Guid? AttemptId);

/// <summary>
/// Base común de ítems: pistas progresivas, confianza opcional, retroalimentación inmediata y registro de un único
/// intento por presentación (los reintentos sirven para aprender, pero no inflan las estadísticas).
/// </summary>
public abstract partial class ItemViewModel : ViewModelBase
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

    /// <summary>Lo que ocurre al pulsar "Continuar" (lo asigna quien presenta el ítem).</summary>
    public Func<ItemResult, Task>? OnCompleted { get; set; }

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

    partial void OnTriesChanged(int value) => OnHintsChanged();

    partial void OnSelectedConfidenceChanged(OptionDisplay<int>? value) => OnPropertyChanged(nameof(ConfidenceMissing));

    partial void OnIsSolvedChanged(bool value) => OnPropertyChanged(nameof(HasMoreHints));

    [RelayCommand(CanExecute = nameof(CanContinue))]
    private async Task ContinueAsync()
    {
        await BeforeContinueAsync();
        if (OnCompleted is not null)
        {
            await OnCompleted(new ItemResult(RecordedAttempt?.Correct, RecordedAttempt?.Id));
        }
    }

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
        }
    }
}
