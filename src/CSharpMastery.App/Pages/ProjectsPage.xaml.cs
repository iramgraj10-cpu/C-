using CSharpMastery.Presentation.ViewModels.Pages;

namespace CSharpMastery.App.Pages;

public abstract class ProjectsPageBase(ProjectsViewModel vm) : PageBase<ProjectsViewModel>(vm);

public partial class ProjectsPage : ProjectsPageBase
{
    public ProjectsPage(ProjectsViewModel vm) : base(vm) => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadAsync();
    }
}
