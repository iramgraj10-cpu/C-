using Microsoft.Maui.Controls.Shapes;

namespace CSharpMastery.App.Controls;

/// <summary>Barra de progreso gruesa y redondeada, con brillo y avance animado.</summary>
public sealed class FatProgressBar : ContentView
{
    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(FatProgressBar), 0d, propertyChanged: (b, _, _) => ((FatProgressBar)b).Update(true));

    public static readonly BindableProperty BarColorProperty =
        BindableProperty.Create(nameof(BarColor), typeof(Color), typeof(FatProgressBar), Color.FromArgb("#46C21C"), propertyChanged: (b, _, n) => ((FatProgressBar)b)._fill.BackgroundColor = (Color)n);

    public static readonly BindableProperty BarHeightProperty =
        BindableProperty.Create(nameof(BarHeight), typeof(double), typeof(FatProgressBar), 16d, propertyChanged: (b, _, _) => ((FatProgressBar)b).Resize());

    private readonly Border _track;
    private readonly Border _fill;
    private readonly BoxView _shine;

    public FatProgressBar()
    {
        _fill = new Border { StrokeThickness = 0, HorizontalOptions = LayoutOptions.Start, BackgroundColor = BarColor, WidthRequest = 0 };
        _shine = new BoxView { Color = Color.FromArgb("#55FFFFFF"), HeightRequest = 4, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(8, 4, 8, 0), CornerRadius = 2 };
        _fill.Content = _shine;
        _track = new Border { StrokeThickness = 0, Content = _fill, Padding = 0 };
        _track.SetAppThemeColor(BackgroundColorProperty, Color.FromArgb("#E5E5E5"), Color.FromArgb("#37464F"));
        Content = _track;
        SemanticProperties.SetDescription(this, "Progreso");
        SizeChanged += (_, _) => Update(false);
        Resize();
    }

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public Color BarColor
    {
        get => (Color)GetValue(BarColorProperty);
        set => SetValue(BarColorProperty, value);
    }

    public double BarHeight
    {
        get => (double)GetValue(BarHeightProperty);
        set => SetValue(BarHeightProperty, value);
    }

    private void Resize()
    {
        HeightRequest = BarHeight;
        var shape = new RoundRectangle { CornerRadius = BarHeight / 2 };
        _track.StrokeShape = shape;
        _fill.StrokeShape = new RoundRectangle { CornerRadius = BarHeight / 2 };
        _shine.IsVisible = BarHeight >= 12;
    }

    private void Update(bool animate)
    {
        if (Width <= 0)
        {
            return;
        }

        var value = Math.Clamp(Progress, 0, 1);
        // Un mínimo visible para que el inicio no parezca vacío.
        var target = value <= 0 ? 0 : Math.Max(BarHeight, Width * value);
        SemanticProperties.SetDescription(this, $"Progreso: {Math.Round(value * 100)}%");
        _shine.IsVisible = BarHeight >= 12 && target > 3 * BarHeight;
        if (!animate)
        {
            _fill.WidthRequest = target;
            return;
        }

        var start = _fill.WidthRequest < 0 ? 0 : _fill.WidthRequest;
        this.AbortAnimation("fill");
        new Animation(v => _fill.WidthRequest = v, start, target, Easing.CubicOut).Commit(this, "fill", length: 450);
    }
}
