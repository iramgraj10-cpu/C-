using CSharpMastery.Domain.Common;
using CSharpMastery.Presentation.Services;

namespace CSharpMastery.App.Services;

public sealed class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null) =>
        MainThread.InvokeOnMainThreadAsync(() => parameters is null
            ? Shell.Current.GoToAsync(route)
            : Shell.Current.GoToAsync(route, parameters));

    public Task GoBackAsync() => MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(".."));
}

public sealed class DialogService : IDialogService
{
    public Task AlertAsync(string title, string message) =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlertAsync(title, message, "Aceptar"));

    public Task<bool> ConfirmAsync(string title, string message, string accept = "Sí", string cancel = "Cancelar") =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlertAsync(title, message, accept, cancel));
}

public sealed class ThemeService : IThemeService
{
    public void Apply(ThemePreference theme)
    {
        if (Application.Current is { } app)
        {
            app.UserAppTheme = theme switch
            {
                ThemePreference.Light => AppTheme.Light,
                ThemePreference.Dark => AppTheme.Dark,
                _ => AppTheme.Unspecified,
            };
        }
    }
}

/// <summary>Respaldo: guarda una copia en la carpeta de la app y abre el diálogo de compartir para llevarla a otro lugar.</summary>
public sealed class FileExchangeService : IFileExchangeService
{
    public async Task<string?> ExportAsync(string fileName, string content)
    {
        var folder = Path.Combine(FileSystem.AppDataDirectory, "Respaldos");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, fileName);
        await File.WriteAllTextAsync(path, content);
        try
        {
            await Share.Default.RequestAsync(new ShareFileRequest { Title = "Respaldo de C# Mastery", File = new ShareFile(path) });
        }
        catch (Exception ex) when (ex is FeatureNotSupportedException or NotImplementedException)
        {
            // Sin diálogo de compartir: el archivo queda en la carpeta indicada.
        }

        return path;
    }

    public async Task<string?> ImportAsync()
    {
        var json = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            [DevicePlatform.WinUI] = [".json"],
            [DevicePlatform.Android] = ["application/json", "text/plain"],
            [DevicePlatform.iOS] = ["public.json"],
            [DevicePlatform.MacCatalyst] = ["public.json"],
        });
        var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Elige un respaldo (.json)", FileTypes = json });
        if (file is null)
        {
            return null;
        }

        await using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
