using CSharpMastery.Presentation.Services;

namespace CSharpMastery.App.Pages;

/// <summary>Página que recibe parámetros de navegación y los entrega a su ViewModel.</summary>
public abstract class PageBase<TViewModel> : ContentPage, IQueryAttributable
    where TViewModel : class
{
    protected PageBase(TViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
    }

    public TViewModel ViewModel { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (ViewModel is INavigationAware aware)
        {
            var copy = new Dictionary<string, object>(query);
            Dispatcher.Dispatch(async () => await aware.OnNavigatedToAsync(copy));
        }
    }
}
