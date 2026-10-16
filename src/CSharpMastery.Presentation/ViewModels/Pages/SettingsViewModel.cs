using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpMastery.Domain.Common;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Execution;
using CSharpMastery.Learning.Services;
using CSharpMastery.Persistence;
using CSharpMastery.Presentation.Formatting;
using CSharpMastery.Presentation.Services;
using CSharpMastery.Presentation.ViewModels.Display;

namespace CSharpMastery.Presentation.ViewModels.Pages;

/// <summary>Perfil, tema, criterios de dominio, respaldo y estado del motor de ejecución.</summary>
public sealed partial class SettingsViewModel(
    LearningService learning,
    IExecutionService execution,
    IThemeService theme,
    IFileExchangeService files,
    IDialogService dialogs) : ViewModelBase
{
    private LearnerProfile? _profile;

    public IReadOnlyList<OptionDisplay<ThemePreference>> Themes { get; } =
        Enum.GetValues<ThemePreference>().Select(t => new OptionDisplay<ThemePreference>(t, Labels.Theme(t))).ToList();

    public IReadOnlyList<OptionDisplay<StartingLevel>> Levels { get; } =
        Enum.GetValues<StartingLevel>().Select(l => new OptionDisplay<StartingLevel>(l, Labels.StartingLevel(l))).ToList();

    [ObservableProperty]
    public partial string DisplayName { get; set; } = "";

    [ObservableProperty]
    public partial OptionDisplay<ThemePreference>? SelectedTheme { get; set; }

    [ObservableProperty]
    public partial OptionDisplay<StartingLevel>? SelectedLevel { get; set; }

    [ObservableProperty]
    public partial string PreferredMinutes { get; set; } = "45";

    [ObservableProperty]
    public partial string MinAccuracyPercent { get; set; } = "85";

    [ObservableProperty]
    public partial string MinGradedAttempts { get; set; } = "6";

    [ObservableProperty]
    public partial string AccuracyWindow { get; set; } = "12";

    [ObservableProperty]
    public partial string MinIndependentCode { get; set; } = "1";

    [ObservableProperty]
    public partial string MinTransfer { get; set; } = "1";

    [ObservableProperty]
    public partial string RetentionDays { get; set; } = "3";

    [ObservableProperty]
    public partial string AssessmentPassPercent { get; set; } = "85";

    [ObservableProperty]
    public partial string? StatusMessage { get; set; }

    [ObservableProperty]
    public partial string? EngineTestResult { get; set; }

    public string EngineStatus => execution.IsAvailable ? "Disponible" : "No disponible en este dispositivo";

    public string EngineMessage => execution.AvailabilityMessage;

    public string PrivacyText =>
        "Todo tu progreso se guarda solo en este dispositivo, en una base de datos local. La aplicación no tiene cuentas, no envía tu código ni tus " +
        "respuestas a ningún servidor y no usa servicios de IA de pago. El código de los ejercicios se compila y ejecuta en un proceso separado " +
        "con límites de tiempo, memoria, salida y una lista de APIs permitidas. Ese aislamiento reduce riesgos, pero no es un sandbox de seguridad " +
        "certificado: está pensado para tu propio código de práctica.";

    partial void OnSelectedThemeChanged(OptionDisplay<ThemePreference>? value)
    {
        if (value is not null)
        {
            theme.Apply(value.Value);
        }
    }

    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            _profile = await learning.GetOrCreateProfileAsync();
            DisplayName = _profile.DisplayName;
            SelectedTheme = Themes.First(t => t.Value == _profile.Theme);
            SelectedLevel = Levels.First(l => l.Value == _profile.InitialLevel);
            PreferredMinutes = _profile.PreferredSessionMinutes.ToString(CultureInfo.InvariantCulture);
            LoadCriteria(await learning.GetCriteriaAsync());
        }, "No se pudieron cargar los ajustes.");
    }

    private void LoadCriteria(MasteryCriteria c)
    {
        MinAccuracyPercent = Math.Round(c.MinAccuracy * 100).ToString(CultureInfo.InvariantCulture);
        MinGradedAttempts = c.MinGradedAttempts.ToString(CultureInfo.InvariantCulture);
        AccuracyWindow = c.AccuracyWindow.ToString(CultureInfo.InvariantCulture);
        MinIndependentCode = c.MinIndependentCodeSolutions.ToString(CultureInfo.InvariantCulture);
        MinTransfer = c.MinTransferSolutions.ToString(CultureInfo.InvariantCulture);
        RetentionDays = c.RetentionMinDays.ToString(CultureInfo.InvariantCulture);
        AssessmentPassPercent = Math.Round(c.AssessmentPassAccuracy * 100).ToString(CultureInfo.InvariantCulture);
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (_profile is null)
        {
            return;
        }

        if (!int.TryParse(PreferredMinutes, out var minutes) || minutes is < StudySessionLimits.Min or > StudySessionLimits.Max)
        {
            StatusMessage = $"La duración preferida debe estar entre {StudySessionLimits.Min} y {StudySessionLimits.Max} minutos.";
            return;
        }

        _profile = _profile with
        {
            DisplayName = string.IsNullOrWhiteSpace(DisplayName) ? "Estudiante" : DisplayName.Trim(),
            Theme = SelectedTheme?.Value ?? ThemePreference.System,
            InitialLevel = SelectedLevel?.Value ?? StartingLevel.NeverProgrammed,
            PreferredSessionMinutes = minutes,
        };
        await learning.Store.SaveProfileAsync(_profile);
        StatusMessage = "Perfil guardado.";
    }

    [RelayCommand]
    private async Task SaveCriteriaAsync()
    {
        static bool Int(string s, out int v) => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v);
        if (!Int(MinAccuracyPercent, out var acc) || !Int(MinGradedAttempts, out var min) || !Int(AccuracyWindow, out var window)
            || !Int(MinIndependentCode, out var code) || !Int(MinTransfer, out var transfer) || !Int(RetentionDays, out var days)
            || !Int(AssessmentPassPercent, out var pass))
        {
            StatusMessage = "Todos los criterios deben ser números enteros.";
            return;
        }

        var criteria = new MasteryCriteria
        {
            MinAccuracy = acc / 100.0,
            MinGradedAttempts = min,
            AccuracyWindow = window,
            MinIndependentCodeSolutions = code,
            MinTransferSolutions = transfer,
            RetentionMinDays = days,
            AssessmentPassAccuracy = pass / 100.0,
        };
        var errors = criteria.Validate();
        if (errors.Count > 0)
        {
            StatusMessage = string.Join(" ", errors);
            return;
        }

        await learning.SaveCriteriaAsync(criteria);
        StatusMessage = "Criterios guardados. Se aplican a partir de tu próximo intento.";
    }

    [RelayCommand]
    private async Task RestoreDefaultCriteriaAsync()
    {
        LoadCriteria(MasteryCriteria.Default);
        await learning.SaveCriteriaAsync(MasteryCriteria.Default);
        StatusMessage = "Se restauraron los criterios recomendados.";
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        await RunSafeAsync(async () =>
        {
            var export = await learning.Store.ExportAsync(learning.Clock.Now);
            var json = SqliteProgressStore.SerializeExport(export);
            var target = await files.ExportAsync($"csharp-mastery-respaldo-{learning.Clock.Now:yyyyMMdd-HHmm}.json", json);
            StatusMessage = target is null ? "Exportación cancelada." : $"Respaldo exportado: {target}";
        }, "No se pudo exportar.");
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        if (!await dialogs.ConfirmAsync("Restaurar respaldo", "El progreso actual se reemplazará por el del archivo. ¿Continuar?", "Restaurar", "Cancelar"))
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            var json = await files.ImportAsync();
            if (json is null)
            {
                StatusMessage = "Importación cancelada.";
                return;
            }

            var export = SqliteProgressStore.DeserializeExport(json);
            await learning.Store.ImportAsync(export);
            await LoadAsync();
            StatusMessage = $"Progreso restaurado ({export.Attempts.Count} intentos, {export.Skills.Count} habilidades).";
        }, "No se pudo importar el respaldo. El progreso actual no se modificó.");
    }

    /// <summary>Comprueba el motor de ejecución con un programa mínimo.</summary>
    [RelayCommand]
    private async Task TestEngineAsync()
    {
        if (!execution.IsAvailable)
        {
            EngineTestResult = execution.AvailabilityMessage;
            return;
        }

        var started = DateTime.UtcNow;
        var result = await execution.ExecuteAsync(new ExecutionRequest { Code = "Console.WriteLine(6 * 7);", Mode = CodeMode.Program });
        var ms = (DateTime.UtcNow - started).TotalMilliseconds;
        EngineTestResult = result.Status == ExecutionStatus.Completed && result.Tests.FirstOrDefault()?.Output.Trim() == "42"
            ? $"Funciona: compiló y ejecutó un programa de prueba en {ms:0} ms."
            : $"Falló la prueba ({result.Status}): {result.Message}";
    }
}

/// <summary>Límites de duración de sesión expuestos para la UI.</summary>
public static class StudySessionLimits
{
    public const int Min = Domain.Sessions.StudySession.MinMinutes;
    public const int Max = Domain.Sessions.StudySession.MaxMinutes;
}
