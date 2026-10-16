using CSharpMastery.Presentation.ViewModels.Pages;

namespace CSharpMastery.App.Pages;

public abstract class ProjectDetailPageBase(ProjectDetailViewModel vm) : PageBase<ProjectDetailViewModel>(vm);

public partial class ProjectDetailPage : ProjectDetailPageBase
{
    public ProjectDetailPage(ProjectDetailViewModel vm) : base(vm) => InitializeComponent();
}
