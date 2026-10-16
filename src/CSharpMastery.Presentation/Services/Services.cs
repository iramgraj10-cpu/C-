using CSharpMastery.Domain.Common;

namespace CSharpMastery.Presentation.Services;

/// <summary>Navegación entre pantallas, independiente de MAUI para poder probar los flujos.</summary>
public interface INavigationService
{
    Task GoToAsync(string route, IDictionary<string, object>? parameters = null);

    Task GoBackAsync();
}

public interface IDialogService
{
    Task AlertAsync(string title, string message);

    Task<bool> ConfirmAsync(string title, string message, string accept = "Sí", string cancel = "Cancelar");
}

/// <summary>Exportar e importar archivos de respaldo.</summary>
public interface IFileExchangeService
{
    /// <summary>Guarda o comparte el contenido; devuelve una descripción del destino o null si se canceló.</summary>
    Task<string?> ExportAsync(string fileName, string content);

    /// <summary>Permite elegir un archivo y devuelve su contenido, o null si se canceló.</summary>
    Task<string?> ImportAsync();
}

public interface IThemeService
{
    void Apply(ThemePreference theme);
}

/// <summary>Navega a una página que necesita parámetros.</summary>
public interface INavigationAware
{
    Task OnNavigatedToAsync(IReadOnlyDictionary<string, object> parameters);
}

public static class Routes
{
    public const string Home = "//inicio";
    public const string Curriculum = "//ruta";
    public const string Progress = "//progreso";
    public const string Projects = "//proyectos";
    public const string Settings = "//ajustes";
    public const string Lesson = "leccion";
    public const string Activity = "actividad";
    public const string AssessmentResult = "resultado-evaluacion";
    public const string ProjectDetail = "proyecto";
    public const string Onboarding = "bienvenida";

    public const string SkillParameter = "skill";
    public const string RequestParameter = "request";
    public const string AssessmentParameter = "assessment";
    public const string ProjectParameter = "project";
}

public sealed class NoOpThemeService : IThemeService
{
    public void Apply(ThemePreference theme)
    {
    }
}
