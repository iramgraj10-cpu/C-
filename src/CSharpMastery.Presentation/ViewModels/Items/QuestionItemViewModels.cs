using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Answers;
using CSharpMastery.Learning.Services;

namespace CSharpMastery.Presentation.ViewModels.Items;

public sealed partial class ChoiceOptionViewModel(int index, ChoiceOption option) : ObservableObject
{
    public int Index { get; } = index;

    public string Text => option.Text;

    public string Letter => ((char)('A' + Index)).ToString();

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    /// <summary>null mientras no se revela; true/false tras corregir.</summary>
    [ObservableProperty]
    public partial bool? RevealedCorrect { get; set; }
}

/// <summary>Pregunta de opción múltiple (concepto, predicción, búsqueda de errores, recuerdo).</summary>
public sealed partial class ChoiceItemViewModel : ItemViewModel
{
    private readonly ChoiceItem _item;

    public ChoiceItemViewModel(ChoiceItem item, ItemSettings settings, Func<AttemptRecord, Task<AttemptOutcome?>> record, Func<DateTimeOffset> now)
        : base(item, settings, record, now)
    {
        _item = item;
        Options = item.Options.Select((o, i) => new ChoiceOptionViewModel(i, o)).ToList();
        UpdateFooter();
    }

    public IReadOnlyList<ChoiceOptionViewModel> Options { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial ChoiceOptionViewModel? Selected { get; set; }

    partial void OnSelectedChanged(ChoiceOptionViewModel? value)
    {
        foreach (var option in Options)
        {
            option.IsSelected = option == value;
        }
    }

    [RelayCommand]
    private void Select(ChoiceOptionViewModel option)
    {
        if (!IsSolved && !(Settings.IsAssessment && HasSubmitted))
        {
            Selected = option;
        }
    }

    private bool CanSubmit() => Selected is not null && !IsSolved && !(Settings.IsAssessment && HasSubmitted);

    protected override bool CanSubmitNow => CanSubmit();

    protected override Task SubmitNowAsync() => SubmitCommand.ExecuteAsync(null);

    protected override bool OffersAnswer => true;

    protected override Task OnBannerLinkAsync()
    {
        RevealAnswer();
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        if (Selected is null)
        {
            return;
        }

        if (ConfidenceMissing)
        {
            Feedback = "Antes de comprobar, indica qué tan seguro estás.";
            FeedbackPositive = null;
            return;
        }

        var check = AnswerChecker.CheckChoice(_item, Selected.Index);
        Tries++;
        await RecordOnceAsync(a => a with { Correct = check.Correct, Score = check.Correct ? 1 : 0, Error = check.Error, Answer = Selected.Text });
        HasSubmitted = true;
        Complete(check);
        SubmitCommand.NotifyCanExecuteChanged();
    }

    private void Complete(AnswerCheck check)
    {
        CanContinue = true;
        if (!Settings.ShowFeedback)
        {
            Feedback = "Respuesta registrada. Verás los resultados al terminar.";
            FeedbackPositive = null;
            return;
        }

        Feedback = check.Correct ? WithoutCorrectPrefix(check.Feedback) : check.Feedback;
        FeedbackPositive = check.Correct;
        if (check.Correct)
        {
            IsSolved = true;
            ShowExplanation = true;
            foreach (var option in Options)
            {
                option.RevealedCorrect = _item.Options[option.Index].IsCorrect ? true : option.IsSelected ? false : null;
            }
        }
        else if (Selected is not null)
        {
            Selected.RevealedCorrect = false;
        }
    }

    /// <summary>La franja ya dice «¡Correcto!»; se quita la repetición al inicio de la retroalimentación.</summary>
    private static string WithoutCorrectPrefix(string feedback)
    {
        var trimmed = feedback.Trim();
        if (!trimmed.StartsWith("Correcto", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var rest = trimmed["Correcto".Length..].TrimStart('.', ':', ',', ';', ' ');
        return rest.Length == 0 ? "" : char.ToUpper(rest[0]) + rest[1..];
    }

    /// <summary>Tras fallar se puede pedir la explicación completa (después de reintentar o usar pistas).</summary>
    [RelayCommand]
    private void RevealAnswer()
    {
        ShowExplanation = true;
        IsSolved = true;
        foreach (var option in Options)
        {
            option.RevealedCorrect = _item.Options[option.Index].IsCorrect ? true : option.RevealedCorrect;
        }

        SubmitCommand.NotifyCanExecuteChanged();
    }
}

/// <summary>Respuesta corta (predicción de salida, valor numérico o término).</summary>
public sealed partial class ShortAnswerItemViewModel : ItemViewModel
{
    private readonly ShortAnswerItem _item;

    public ShortAnswerItemViewModel(ShortAnswerItem item, ItemSettings settings, Func<AttemptRecord, Task<AttemptOutcome?>> record, Func<DateTimeOffset> now)
        : base(item, settings, record, now)
    {
        _item = item;
        UpdateFooter();
    }

    public bool IsMultiline => _item.Match == AnswerMatch.Output;

    public string Placeholder => _item.Match switch
    {
        AnswerMatch.Output => "Escribe exactamente lo que mostrará la consola…",
        AnswerMatch.Number => "Escribe un número…",
        _ => "Escribe tu respuesta…",
    };

    public string AcceptedAnswer => _item.AcceptedAnswers.FirstOrDefault() ?? "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial string Answer { get; set; } = "";

    private bool CanSubmit() => !string.IsNullOrWhiteSpace(Answer) && !IsSolved && !(Settings.IsAssessment && HasSubmitted);

    protected override bool CanSubmitNow => CanSubmit();

    protected override Task SubmitNowAsync() => SubmitCommand.ExecuteAsync(null);

    protected override bool OffersAnswer => true;

    protected override Task OnBannerLinkAsync()
    {
        RevealAnswer();
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        if (ConfidenceMissing)
        {
            Feedback = "Antes de comprobar, indica qué tan seguro estás.";
            FeedbackPositive = null;
            return;
        }

        var check = AnswerChecker.CheckShortAnswer(_item, Answer);
        Tries++;
        await RecordOnceAsync(a => a with { Correct = check.Correct, Score = check.Correct ? 1 : 0, Error = check.Error, Answer = Answer });
        HasSubmitted = true;
        CanContinue = true;
        if (!Settings.ShowFeedback)
        {
            Feedback = "Respuesta registrada. Verás los resultados al terminar.";
        }
        else if (check.Correct)
        {
            Feedback = Tries == 1 ? "" : "Lo lograste en este reintento. El primer intento quedó registrado como fallido para programar más práctica.";
            FeedbackPositive = true;
            IsSolved = true;
            ShowExplanation = true;
        }
        else
        {
            Feedback = check.Feedback + " Puedes intentarlo de nuevo o pedir una pista.";
            FeedbackPositive = false;
        }

        SubmitCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void RevealAnswer()
    {
        ShowExplanation = true;
        IsSolved = true;
        Feedback = $"Respuesta esperada: {AcceptedAnswer}";
        FeedbackPositive = null;
        SubmitCommand.NotifyCanExecuteChanged();
    }
}
