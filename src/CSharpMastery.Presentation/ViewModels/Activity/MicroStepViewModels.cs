using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Learning.Interactive;
using CSharpMastery.Learning.Services;
using CSharpMastery.Presentation.ViewModels.Display;

namespace CSharpMastery.Presentation.ViewModels.Activity;

/// <summary>Estado visual de una ficha, línea u opción.</summary>
public enum ChipState
{
    Normal,
    Selected,
    Correct,
    Wrong,
    Used,
}

/// <summary>Paso interactivo generado de la lección: da XP si se acierta a la primera.</summary>
public abstract partial class MicroStepViewModel(MicroStep step) : StepViewModel
{
    public MicroStep Step { get; } = step;

    public bool IsChecked { get; protected set; }

    public bool? WasCorrect { get; protected set; }

    /// <summary>Califica, muestra la franja y deja listo «Continuar».</summary>
    protected void Grade(bool correct, string? text, string? code = null)
    {
        IsChecked = true;
        WasCorrect = correct;
        XpEarned = correct ? XpRewards.MicroStep : 0;
        ShowBanner(correct ? BannerKind.Correct : BannerKind.Incorrect,
            correct ? PraiseFor(Step.Key) : "No exactamente", text, code);
        PrimaryText = "Continuar";
        PrimaryEnabled = true;
    }

    protected Task ContinueAsync() => CompleteAsync(WasCorrect);
}

// ---------------- Introducción y tarjetas ----------------

public sealed partial class IntroStepViewModel : MicroStepViewModel
{
    public IntroStepViewModel(IntroStep step, string unitTitle) : base(step)
    {
        Title = step.Title;
        Objective = step.Objective;
        UnitTitle = unitTitle;
        MinutesText = $"≈ {step.EstimatedMinutes} min";
        PrimaryText = "¡Empezar!";
    }

    public string Title { get; }

    public string Objective { get; }

    public string UnitTitle { get; }

    public string MinutesText { get; }

    public override bool IsCelebration => true;

    protected override Task OnPrimaryAsync() => CompleteAsync(null);
}

public sealed partial class ConceptCardViewModel : MicroStepViewModel
{
    public ConceptCardViewModel(ConceptCardStep step) : base(step)
    {
        Blocks = step.Blocks.Select(ContentBlockDisplay.From).ToList();
        CounterText = $"Idea {step.Number} de {step.Total}";
        PrimaryText = "Continuar";
    }

    public IReadOnlyList<ContentBlockDisplay> Blocks { get; }

    public string CounterText { get; }

    protected override Task OnPrimaryAsync() => CompleteAsync(null);
}

// ---------------- Completar el hueco ----------------

public sealed partial class TokenChipViewModel(string text) : ObservableObject
{
    public string Text { get; } = text;

    [ObservableProperty]
    public partial ChipState State { get; set; }
}

public sealed partial class ClozeViewModel : MicroStepViewModel
{
    private readonly ClozeStep _cloze;

    public ClozeViewModel(ClozeStep step) : base(step)
    {
        _cloze = step;
        Options = step.Options.Select(o => new TokenChipViewModel(o)).ToList();
        PrimaryText = "Comprobar";
        PrimaryEnabled = false;
    }

    public string Prompt => _cloze.Prompt;

    public IReadOnlyList<TokenChipViewModel> Options { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayCode))]
    public partial TokenChipViewModel? Selected { get; set; }

    /// <summary>El código con el hueco (o con la ficha elegida).</summary>
    public string DisplayCode => _cloze.Before + (Selected?.Text ?? Blank(_cloze.Answer)) + _cloze.After;

    private static string Blank(string answer) => new('_', Math.Max(3, answer.Length));

    [RelayCommand]
    private void Choose(TokenChipViewModel chip)
    {
        if (IsChecked)
        {
            return;
        }

        Selected = chip;
        foreach (var option in Options)
        {
            option.State = option == chip ? ChipState.Selected : ChipState.Normal;
        }

        PrimaryEnabled = true;
    }

    protected override Task OnPrimaryAsync()
    {
        if (IsChecked)
        {
            return ContinueAsync();
        }

        if (Selected is null)
        {
            return Task.CompletedTask;
        }

        var correct = Selected.Text == _cloze.Answer;
        foreach (var option in Options)
        {
            option.State = option.Text == _cloze.Answer ? ChipState.Correct : option == Selected ? ChipState.Wrong : ChipState.Normal;
        }

        Grade(correct, correct ? _cloze.Explanation : $"Respuesta correcta: {_cloze.Answer}\n{_cloze.Explanation}", correct ? null : _cloze.Fill(_cloze.Answer));
        return Task.CompletedTask;
    }
}

// ---------------- Ordenar líneas ----------------

public sealed partial class ParsonsSlotViewModel(int lineIndex, string indent, string? fixedText) : ObservableObject
{
    public int LineIndex { get; } = lineIndex;

    public string Indent { get; } = indent;

    public bool IsFixed => fixedText is not null;

    public bool IsSlot => fixedText is null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Text), nameof(IsEmpty))]
    public partial ParsonsBankLineViewModel? Filled { get; set; }

    [ObservableProperty]
    public partial ChipState State { get; set; }

    public bool IsEmpty => IsSlot && Filled is null;

    public string Text => fixedText ?? Indent + (Filled?.Text ?? "");
}

public sealed partial class ParsonsBankLineViewModel(int lineIndex, string text) : ObservableObject
{
    public int LineIndex { get; } = lineIndex;

    public string Text { get; } = text;

    [ObservableProperty]
    public partial bool IsUsed { get; set; }
}

public sealed partial class ParsonsViewModel : MicroStepViewModel
{
    private readonly ParsonsStep _parsons;

    public ParsonsViewModel(ParsonsStep step) : base(step)
    {
        _parsons = step;
        Slots = new(step.Lines.Select((l, i) => new ParsonsSlotViewModel(i, l.Indent, l.Movable ? null : l.Text)));
        Bank = new(step.BankOrder.Select(i => new ParsonsBankLineViewModel(i, step.Lines[i].Content)));
        PrimaryText = "Comprobar";
        PrimaryEnabled = false;
    }

    public string Prompt => _parsons.Prompt;

    public ObservableCollection<ParsonsSlotViewModel> Slots { get; }

    public ObservableCollection<ParsonsBankLineViewModel> Bank { get; }

    [RelayCommand]
    private void Place(ParsonsBankLineViewModel line)
    {
        if (IsChecked || line.IsUsed || Slots.FirstOrDefault(s => s.IsEmpty) is not { } slot)
        {
            return;
        }

        slot.Filled = line;
        line.IsUsed = true;
        PrimaryEnabled = Slots.All(s => !s.IsEmpty);
    }

    [RelayCommand]
    private void Remove(ParsonsSlotViewModel slot)
    {
        if (IsChecked || slot.Filled is not { } line)
        {
            return;
        }

        line.IsUsed = false;
        slot.Filled = null;
        PrimaryEnabled = false;
    }

    protected override Task OnPrimaryAsync()
    {
        if (IsChecked)
        {
            return ContinueAsync();
        }

        var order = Slots.Where(s => s.IsSlot).Select(s => s.Filled!.LineIndex).ToList();
        var correct = _parsons.IsCorrect(order);
        foreach (var slot in Slots.Where(s => s.IsSlot))
        {
            slot.State = _parsons.Lines[slot.LineIndex].Content == slot.Filled!.Text ? ChipState.Correct : ChipState.Wrong;
        }

        Grade(correct,
            correct ? "Cada línea depende de la anterior: por eso el orden importa." : "Así queda el orden correcto:",
            correct ? null : string.Join("\n", _parsons.Lines.Select(l => l.Text)));
        return Task.CompletedTask;
    }
}

// ---------------- Recorrido del ejemplo ----------------

public sealed partial class CodeLineViewModel(int number, string text) : ObservableObject
{
    public int Number { get; } = number;

    public string Text { get; } = text;

    [ObservableProperty]
    public partial ChipState State { get; set; }

    [ObservableProperty]
    public partial bool IsHighlighted { get; set; }
}

public sealed partial class WalkthroughViewModel : MicroStepViewModel
{
    private readonly WalkthroughStep _walk;
    private int _index = -1;

    public WalkthroughViewModel(WalkthroughStep step) : base(step)
    {
        _walk = step;
        Lines = step.Code.Replace("\r\n", "\n").TrimEnd('\n').Split('\n').Select((t, i) => new CodeLineViewModel(i + 1, t)).ToList();
        Advance();
    }

    public string Title => _walk.Title;

    public IReadOnlyList<CodeLineViewModel> Lines { get; }

    public string? Output => _walk.Output;

    [ObservableProperty]
    public partial string? Note { get; set; }

    [ObservableProperty]
    public partial string CounterText { get; set; } = "";

    [ObservableProperty]
    public partial bool ShowOutput { get; set; }

    private void Advance()
    {
        _index++;
        foreach (var line in Lines)
        {
            line.IsHighlighted = _index < _walk.Notes.Count && line.Number == _walk.Notes[_index].Line;
        }

        if (_index < _walk.Notes.Count)
        {
            Note = _walk.Notes[_index].Note;
            CounterText = $"Paso {_index + 1} de {_walk.Notes.Count}";
            PrimaryText = _index == _walk.Notes.Count - 1 && Output is null ? "Continuar" : "Siguiente";
        }
        else
        {
            Note = null;
            ShowOutput = Output is not null;
            CounterText = "Resultado";
            PrimaryText = "Continuar";
        }
    }

    protected override Task OnPrimaryAsync()
    {
        if (_index >= _walk.Notes.Count || (_index == _walk.Notes.Count - 1 && Output is null))
        {
            return CompleteAsync(null);
        }

        Advance();
        return Task.CompletedTask;
    }
}

// ---------------- Encontrar el error ----------------

public sealed partial class SpotBugViewModel : MicroStepViewModel
{
    private readonly SpotBugStep _bug;

    public SpotBugViewModel(SpotBugStep step) : base(step)
    {
        _bug = step;
        Lines = step.CodeLines.Select((t, i) => new CodeLineViewModel(i + 1, t)).ToList();
        PrimaryText = "Comprobar";
        PrimaryEnabled = false;
    }

    public string Description => _bug.Description;

    public IReadOnlyList<CodeLineViewModel> Lines { get; }

    [ObservableProperty]
    public partial CodeLineViewModel? Selected { get; set; }

    [RelayCommand]
    private void Tap(CodeLineViewModel line)
    {
        if (IsChecked || string.IsNullOrWhiteSpace(line.Text))
        {
            return;
        }

        Selected = line;
        foreach (var l in Lines)
        {
            l.State = l == line ? ChipState.Selected : ChipState.Normal;
        }

        PrimaryEnabled = true;
    }

    protected override Task OnPrimaryAsync()
    {
        if (IsChecked)
        {
            return ContinueAsync();
        }

        if (Selected is null)
        {
            return Task.CompletedTask;
        }

        var correct = _bug.BugLines.Contains(Selected.Number);
        foreach (var line in Lines)
        {
            line.State = _bug.BugLines.Contains(line.Number) ? ChipState.Correct : line == Selected ? ChipState.Wrong : ChipState.Normal;
        }

        Grade(correct, _bug.Why + "\nAsí queda corregido:", _bug.FixedCode);
        return Task.CompletedTask;
    }
}

public sealed partial class CodeOptionViewModel(string label, string code, bool isCorrect) : ObservableObject
{
    public string Label { get; } = label;

    public string Code { get; } = code;

    public bool IsCorrect { get; } = isCorrect;

    [ObservableProperty]
    public partial ChipState State { get; set; }
}

public sealed partial class CompareCodeViewModel : MicroStepViewModel
{
    private readonly CompareCodeStep _compare;

    public CompareCodeViewModel(CompareCodeStep step) : base(step)
    {
        _compare = step;
        Options = [new CodeOptionViewModel("A", step.OptionA, step.AIsCorrect), new CodeOptionViewModel("B", step.OptionB, !step.AIsCorrect)];
        PrimaryText = "Comprobar";
        PrimaryEnabled = false;
    }

    public string Description => _compare.Description;

    public IReadOnlyList<CodeOptionViewModel> Options { get; }

    [ObservableProperty]
    public partial CodeOptionViewModel? Selected { get; set; }

    [RelayCommand]
    private void Choose(CodeOptionViewModel option)
    {
        if (IsChecked)
        {
            return;
        }

        Selected = option;
        foreach (var o in Options)
        {
            o.State = o == option ? ChipState.Selected : ChipState.Normal;
        }

        PrimaryEnabled = true;
    }

    protected override Task OnPrimaryAsync()
    {
        if (IsChecked)
        {
            return ContinueAsync();
        }

        if (Selected is null)
        {
            return Task.CompletedTask;
        }

        foreach (var o in Options)
        {
            o.State = o.IsCorrect ? ChipState.Correct : o == Selected ? ChipState.Wrong : ChipState.Normal;
        }

        Grade(Selected.IsCorrect, _compare.Why);
        return Task.CompletedTask;
    }
}

/// <summary>Crea el ViewModel de cada paso generado.</summary>
public static class MicroStepFactory
{
    public static MicroStepViewModel Create(MicroStep step, string unitTitle) => step switch
    {
        IntroStep s => new IntroStepViewModel(s, unitTitle),
        ConceptCardStep s => new ConceptCardViewModel(s),
        ClozeStep s => new ClozeViewModel(s),
        ParsonsStep s => new ParsonsViewModel(s),
        WalkthroughStep s => new WalkthroughViewModel(s),
        SpotBugStep s => new SpotBugViewModel(s),
        CompareCodeStep s => new CompareCodeViewModel(s),
        _ => throw new NotSupportedException(step.GetType().Name),
    };
}
