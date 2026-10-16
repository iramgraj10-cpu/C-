using CSharpMastery.App.Pages;
using CSharpMastery.App.Services;
using CSharpMastery.Content;
using CSharpMastery.Domain.Common;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Execution;
using CSharpMastery.Learning.Services;
using CSharpMastery.Persistence;
using CSharpMastery.Presentation.Services;
using CSharpMastery.Presentation.ViewModels.Activity;
using CSharpMastery.Presentation.ViewModels.Pages;

namespace CSharpMastery.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        var services = builder.Services;
        services.AddSingleton<CurriculumCatalog>(_ => CurriculumContent.Catalog);
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IProgressStore>(_ => new SqliteProgressStore(Path.Combine(FileSystem.AppDataDirectory, "progreso.db")));
        services.AddSingleton<LearningService>();
        services.AddSingleton<IExecutionService>(_ => CreateExecutionService());

        services.AddSingleton<INavigationService, ShellNavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IFileExchangeService, FileExchangeService>();

        services.AddSingleton<AppShell>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<CurriculumViewModel>();
        services.AddTransient<LessonViewModel>();
        services.AddTransient<ActivityViewModel>();
        services.AddTransient<ProgressViewModel>();
        services.AddTransient<AssessmentResultViewModel>();
        services.AddTransient<ProjectsViewModel>();
        services.AddTransient<ProjectDetailViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<OnboardingViewModel>();

        services.AddTransient<HomePage>();
        services.AddTransient<CurriculumPage>();
        services.AddTransient<LessonPage>();
        services.AddTransient<ActivityPage>();
        services.AddTransient<ProgressPage>();
        services.AddTransient<AssessmentResultPage>();
        services.AddTransient<ProjectsPage>();
        services.AddTransient<ProjectDetailPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<OnboardingPage>();

        return builder.Build();
    }

    /// <summary>
    /// En escritorio el código se ejecuta en un proceso aparte que se distribuye junto a la app. En Android e iOS no se
    /// pueden lanzar procesos .NET aparte, así que la ejecución no está disponible y se ofrece autoevaluación.
    /// </summary>
    private static IExecutionService CreateExecutionService()
    {
        if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
        {
            return new UnavailableExecutionService(
                "En este dispositivo no se puede ejecutar código de forma aislada todavía. Puedes estudiar teoría, resolver preguntas y " +
                "autoevaluar tus soluciones comparándolas con la solución explicada; para comprobar código con pruebas usa la versión de escritorio.");
        }

        var launch = RunnerLaunchInfo.Locate(AppContext.BaseDirectory);
        return launch is null
            ? new UnavailableExecutionService("No se encontró el ejecutor de código junto a la aplicación (carpeta \"runner\"). Reinstala la aplicación.")
            : new ProcessExecutionService(launch);
    }
}
