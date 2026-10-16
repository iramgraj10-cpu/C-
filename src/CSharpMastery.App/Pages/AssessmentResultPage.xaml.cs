using CSharpMastery.Presentation.ViewModels.Pages;

namespace CSharpMastery.App.Pages;

public abstract class AssessmentResultPageBase(AssessmentResultViewModel vm) : PageBase<AssessmentResultViewModel>(vm);

public partial class AssessmentResultPage : AssessmentResultPageBase
{
    public AssessmentResultPage(AssessmentResultViewModel vm) : base(vm) => InitializeComponent();
}
