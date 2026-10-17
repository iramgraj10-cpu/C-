namespace CSharpMastery.App.Views;

public partial class SummaryStepView : ContentView
{
    public SummaryStepView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            // Pequeño salto de la mascota al aparecer.
            Mascot.Scale = 0.6;
            await Mascot.ScaleToAsync(1.08, 260, Easing.CubicOut);
            await Mascot.ScaleToAsync(1, 140, Easing.CubicIn);
        };
    }
}
