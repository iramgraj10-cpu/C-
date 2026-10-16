using CSharpMastery.Presentation.ViewModels.Pages;

namespace CSharpMastery.App.Pages;

public abstract class CurriculumPageBase(CurriculumViewModel vm) : PageBase<CurriculumViewModel>(vm);

public partial class CurriculumPage : CurriculumPageBase
{
    public CurriculumPage(CurriculumViewModel vm) : base(vm) => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadAsync();
    }
}
