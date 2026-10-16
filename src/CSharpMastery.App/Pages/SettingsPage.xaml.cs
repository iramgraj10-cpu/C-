using CSharpMastery.Presentation.ViewModels.Pages;

namespace CSharpMastery.App.Pages;

public abstract class SettingsPageBase(SettingsViewModel vm) : PageBase<SettingsViewModel>(vm);

public partial class SettingsPage : SettingsPageBase
{
    public SettingsPage(SettingsViewModel vm) : base(vm) => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadAsync();
    }
}
