using CSharpMastery.Presentation.ViewModels.Pages;

namespace CSharpMastery.App.Pages;

public abstract class LessonPageBase(LessonViewModel vm) : PageBase<LessonViewModel>(vm);

public partial class LessonPage : LessonPageBase
{
    public LessonPage(LessonViewModel vm) : base(vm) => InitializeComponent();
}
