using CSharpMastery.Presentation.Services;
using CSharpMastery.Presentation.ViewModels.Pages;

namespace CSharpMastery.App.Pages;

public abstract class HomePageBase(HomeViewModel vm) : PageBase<HomeViewModel>(vm);

public partial class HomePage : HomePageBase
{
    public HomePage(HomeViewModel vm) : base(vm) => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!await ViewModel.LoadAsync())
        {
            await Shell.Current.GoToAsync(Routes.Onboarding);
        }
    }
}
