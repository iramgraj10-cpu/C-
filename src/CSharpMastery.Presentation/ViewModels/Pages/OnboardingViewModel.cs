using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Domain.Common;
using CSharpMastery.Learning.Services;
using CSharpMastery.Presentation.Formatting;
using CSharpMastery.Presentation.Services;
using CSharpMastery.Presentation.ViewModels.Display;

namespace CSharpMastery.Presentation.ViewModels.Pages;

public sealed partial class GoalOption(string text) : ObservableObject
{
    public string Text { get; } = text;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}

/// <summary>Bienvenida: crea el perfil local (sin cuenta) y explica cómo funciona el método.</summary>
public sealed partial class OnboardingViewModel(LearningService learning, INavigationService navigation, IThemeService theme) : ViewModelBase
{
    public IReadOnlyList<OptionDisplay<StartingLevel>> Levels { get; } =
        Enum.GetValues<StartingLevel>().Select(l => new OptionDisplay<StartingLevel>(l, Labels.StartingLevel(l))).ToList();

    public IReadOnlyList<GoalOption> Goals { get; } =
    [
        new("Conseguir mi primer trabajo como desarrollador"),
        new("Crear aplicaciones de escritorio o móviles"),
        new("Desarrollar videojuegos"),
        new("Programar backend y APIs"),
        new("Aprobar una materia o certificación"),
        new("Aprender por gusto"),
    ];

    public IReadOnlyList<int> SessionOptions { get; } = [25, 45, 75];

    public string MethodText =>
        "Cada sesión combina recuperar lo que ya viste, un concepto nuevo con ejemplo resuelto, práctica con código real que se compila y " +
        "se prueba, y un problema de aplicación. Lo que aprendes vuelve en repasos espaciados. Una habilidad se marca como dominada solo " +
        "cuando lo demuestras con varios ejercicios distintos, sin ayuda, y lo recuerdas días después. El ritmo depende de tu práctica; " +
        "la aplicación no promete un plazo fijo para llegar a un nivel.";

    [ObservableProperty]
    public partial string DisplayName { get; set; } = "";

    [ObservableProperty]
    public partial OptionDisplay<StartingLevel>? SelectedLevel { get; set; }

    [ObservableProperty]
    public partial int SessionMinutes { get; set; } = 45;

    [RelayCommand]
    private void SelectMinutes(int minutes) => SessionMinutes = minutes;

    [RelayCommand]
    private async Task StartAsync()
    {
        await RunSafeAsync(async () =>
        {
            var profile = new LearnerProfile(
                string.IsNullOrWhiteSpace(DisplayName) ? "Estudiante" : DisplayName.Trim(),
                SelectedLevel?.Value ?? StartingLevel.NeverProgrammed,
                Goals.Where(g => g.IsSelected).Select(g => g.Text).ToList(),
                SessionMinutes,
                ThemePreference.System,
                learning.Clock.Now);
            await learning.Store.SaveProfileAsync(profile);
            theme.Apply(profile.Theme);
            await navigation.GoToAsync(Routes.Home);
        }, "No se pudo guardar el perfil.");
    }
}
