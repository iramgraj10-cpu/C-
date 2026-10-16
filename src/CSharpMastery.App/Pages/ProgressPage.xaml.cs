using CSharpMastery.Presentation.ViewModels.Pages;

namespace CSharpMastery.App.Pages;

public abstract class ProgressPageBase(ProgressViewModel vm) : PageBase<ProgressViewModel>(vm);

public partial class ProgressPage : ProgressPageBase
{
    public ProgressPage(ProgressViewModel vm) : base(vm) => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadAsync();
    }
}
