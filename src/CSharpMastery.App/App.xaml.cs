using CSharpMastery.App.Pages;
using CSharpMastery.Learning.Services;
using CSharpMastery.Presentation.Services;

namespace CSharpMastery.App;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(_services.GetRequiredService<AppShell>()) { Title = "C# Mastery", MinimumWidth = 360, MinimumHeight = 480 };

        // Al pasar a segundo plano se guarda y se pausa la sesión; el tiempo en pausa no cuenta.
        window.Stopped += async (_, _) =>
        {
            if (Shell.Current?.CurrentPage is ActivityPage page)
            {
                await page.ViewModel.OnAppSleepAsync();
            }
        };
        window.Resumed += async (_, _) =>
        {
            if (Shell.Current?.CurrentPage is ActivityPage page)
            {
                await page.ViewModel.OnAppResumeAsync();
            }
        };
        window.Created += async (_, _) =>
        {
            var learning = _services.GetRequiredService<LearningService>();
            if (await learning.Store.GetProfileAsync() is { } profile)
            {
                _services.GetRequiredService<IThemeService>().Apply(profile.Theme);
            }
        };
        return window;
    }
}
