using CommunityToolkit.Mvvm.ComponentModel;

namespace CSharpMastery.Presentation.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    /// <summary>Ejecuta una operación mostrando un mensaje comprensible si falla, sin cerrar la aplicación.</summary>
    protected async Task RunSafeAsync(Func<Task> action, string failureMessage = "No se pudo completar la operación.")
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await action();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            ErrorMessage = $"{failureMessage} ({ex.Message})";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
