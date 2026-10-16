using CSharpMastery.Presentation.ViewModels.Pages;

namespace CSharpMastery.App.Pages;

public abstract class OnboardingPageBase(OnboardingViewModel vm) : PageBase<OnboardingViewModel>(vm);

public partial class OnboardingPage : OnboardingPageBase
{
    public OnboardingPage(OnboardingViewModel vm) : base(vm) => InitializeComponent();
}
