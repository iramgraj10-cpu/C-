using CSharpMastery.App.Pages;
using CSharpMastery.Presentation.Services;

namespace CSharpMastery.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(Routes.Lesson, typeof(LessonPage));
        Routing.RegisterRoute(Routes.Activity, typeof(ActivityPage));
        Routing.RegisterRoute(Routes.AssessmentResult, typeof(AssessmentResultPage));
        Routing.RegisterRoute(Routes.ProjectDetail, typeof(ProjectDetailPage));
        Routing.RegisterRoute(Routes.Onboarding, typeof(OnboardingPage));
    }
}
