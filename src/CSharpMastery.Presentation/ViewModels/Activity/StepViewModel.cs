using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CSharpMastery.Presentation.ViewModels.Activity;

/// <summary>Franja de retroalimentación que aparece abajo al comprobar.</summary>
public enum BannerKind
{
    None,
    Correct,
    Incorrect,
    Info,
}

/// <summary>Resultado de un paso al continuar. Correct es null si no se calificó; Xp es lo que ganó.</summary>
public sealed record StepResult(bool? Correct, Guid? AttemptId, int Xp = 0);

/// <summary>
/// Base de todos los pasos de una actividad. La pantalla tiene un pie fijo con un botón principal
/// (Comprobar / Continuar), uno secundario opcional y la franja de retroalimentación; cada paso decide
/// qué muestran según su estado.
/// </summary>
public abstract partial class StepViewModel : ViewModelBase
{
    private static readonly string[] Praise = ["¡Excelente!", "¡Muy bien!", "¡Correcto!", "¡Así se hace!", "¡Bien hecho!", "¡Perfecto!"];

    /// <summary>Lo que ocurre al terminar el paso (lo asigna quien lo presenta).</summary>
    public Func<StepResult, Task>? OnCompleted { get; set; }

    [ObservableProperty]
    public partial string PrimaryText { get; set; } = "Continuar";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PrimaryCommand))]
    public partial bool PrimaryEnabled { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSecondary))]
    public partial string? SecondaryText { get; set; }

    public bool HasSecondary => !string.IsNullOrEmpty(SecondaryText);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBanner), nameof(IsCorrectBanner), nameof(IsIncorrectBanner), nameof(IsInfoBanner))]
    public partial BannerKind Banner { get; set; }

    public bool HasBanner => Banner != BannerKind.None;

    public bool IsCorrectBanner => Banner == BannerKind.Correct;

    public bool IsIncorrectBanner => Banner == BannerKind.Incorrect;

    public bool IsInfoBanner => Banner == BannerKind.Info;

    [ObservableProperty]
    public partial string? BannerTitle { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBannerText))]
    public partial string? BannerText { get; set; }

    public bool HasBannerText => !string.IsNullOrWhiteSpace(BannerText);

    /// <summary>Código que acompaña la retroalimentación (versión correcta, orden correcto…).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBannerCode))]
    public partial string? BannerCode { get; set; }

    public bool HasBannerCode => !string.IsNullOrWhiteSpace(BannerCode);

    /// <summary>Enlace dentro de la franja (por ejemplo, «Ver la respuesta»).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBannerLink))]
    public partial string? BannerLinkText { get; set; }

    public bool HasBannerLink => !string.IsNullOrEmpty(BannerLinkText);

    /// <summary>XP que dio este paso (para el resumen).</summary>
    public int XpEarned { get; protected set; }

    /// <summary>Imagen de la mascota que acompaña el paso (ver <see cref="Mascots"/>).</summary>
    public virtual string Mascot => Mascots.Robot;

    /// <summary>Si el paso ocupa toda la pantalla sin desplazamiento (tarjetas de introducción y cierre).</summary>
    public virtual bool IsCelebration => false;

    [RelayCommand(CanExecute = nameof(PrimaryEnabled))]
    private Task PrimaryAsync() => OnPrimaryAsync();

    [RelayCommand]
    private Task SecondaryAsync() => OnSecondaryAsync();

    [RelayCommand]
    private Task BannerLinkAsync() => OnBannerLinkAsync();

    protected abstract Task OnPrimaryAsync();

    protected virtual Task OnSecondaryAsync() => Task.CompletedTask;

    protected virtual Task OnBannerLinkAsync() => Task.CompletedTask;

    protected Task CompleteAsync(bool? correct, Guid? attemptId = null) =>
        OnCompleted?.Invoke(new StepResult(correct, attemptId, XpEarned)) ?? Task.CompletedTask;

    /// <summary>Elogio variado pero determinista (las pruebas y la reanudación ven siempre el mismo).</summary>
    protected static string PraiseFor(string key) => Praise[Learning.Interactive.ClozeGenerator.StableHash(key) % Praise.Length];

    protected void ShowBanner(BannerKind kind, string? title, string? text, string? code = null, string? link = null)
    {
        BannerTitle = title;
        BannerText = text;
        BannerCode = code;
        BannerLinkText = link;
        Banner = kind;
    }

    protected void HideBanner()
    {
        Banner = BannerKind.None;
        BannerTitle = null;
        BannerText = null;
        BannerCode = null;
        BannerLinkText = null;
    }
}
