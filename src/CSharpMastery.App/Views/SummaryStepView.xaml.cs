namespace CSharpMastery.App.Views;

public partial class SummaryStepView : ContentView
{
    public SummaryStepView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            // Todas las mascotas aparecen una tras otra y luego dan un saltito de festejo juntas.
            var crew = new VisualElement[] { Mascot, Sleepy, Thinker, Photo };
            foreach (var m in crew)
            {
                m.Scale = 0;
            }

            foreach (var m in crew)
            {
                await m.ScaleToAsync(1.1, 200, Easing.CubicOut);
                _ = m.ScaleToAsync(1, 120, Easing.CubicIn);
            }

            for (var round = 0; round < 2; round++)
            {
                await Task.WhenAll(crew.Select((m, i) => Hop(m, i * 70)));
            }
        };
    }

    private static async Task Hop(VisualElement element, int delayMs)
    {
        await Task.Delay(delayMs);
        await element.TranslateToAsync(0, -14, 180, Easing.CubicOut);
        await element.TranslateToAsync(0, 0, 220, Easing.BounceOut);
    }
}
