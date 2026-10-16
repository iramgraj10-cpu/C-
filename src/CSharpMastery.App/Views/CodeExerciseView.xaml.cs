namespace CSharpMastery.App.Views;

public partial class CodeExerciseView : ContentView
{
    public CodeExerciseView() => InitializeComponent();

    public void FocusEditor() => CodeEditor.Focus();
}
