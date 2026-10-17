namespace CSharpMastery.App.Controls;

/// <summary>Anillo de progreso (meta diaria) dibujado con Microsoft.Maui.Graphics.</summary>
public sealed class RingProgress : GraphicsView, IDrawable
{
    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(RingProgress), 0d, propertyChanged: (b, _, _) => ((RingProgress)b).Invalidate());

    public static readonly BindableProperty RingColorProperty =
        BindableProperty.Create(nameof(RingColor), typeof(Color), typeof(RingProgress), Color.FromArgb("#FFC800"), propertyChanged: (b, _, _) => ((RingProgress)b).Invalidate());

    public static readonly BindableProperty ThicknessProperty =
        BindableProperty.Create(nameof(Thickness), typeof(float), typeof(RingProgress), 10f, propertyChanged: (b, _, _) => ((RingProgress)b).Invalidate());

    public RingProgress()
    {
        Drawable = this;
        BackgroundColor = Colors.Transparent;
    }

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public Color RingColor
    {
        get => (Color)GetValue(RingColorProperty);
        set => SetValue(RingColorProperty, value);
    }

    public float Thickness
    {
        get => (float)GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        var size = Math.Min(rect.Width, rect.Height) - Thickness;
        var x = rect.Center.X - size / 2;
        var y = rect.Center.Y - size / 2;
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        canvas.StrokeSize = Thickness;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeColor = Color.FromArgb(dark ? "#37464F" : "#E5E5E5");
        canvas.DrawEllipse(x, y, size, size);

        var value = (float)Math.Clamp(Progress, 0, 1);
        if (value <= 0)
        {
            return;
        }

        canvas.StrokeColor = RingColor;
        if (value >= 1)
        {
            canvas.DrawEllipse(x, y, size, size);
            return;
        }

        // Empieza arriba (90°) y avanza en el sentido de las agujas del reloj.
        canvas.DrawArc(x, y, size, size, 90, 90 - 360 * value, true, false);
    }
}
