using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace CSharpMastery.App.Controls;

/// <summary>
/// Botón en relieve: la cara se apoya en una «repisa» de un tono más oscuro y baja al pulsarla.
/// Variantes: brand (violeta), green, red, blue, gold y outline (blanco con borde gris).
/// </summary>
public sealed class ChunkyButton : ContentView
{
    private const double Depth = 4;

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ChunkyButton), null, propertyChanged: (b, _, n) => ((ChunkyButton)b)._face.Text = n as string);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ChunkyButton), null, propertyChanged: (b, _, n) => ((ChunkyButton)b)._face.Command = n as ICommand);

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ChunkyButton), null, propertyChanged: (b, _, n) => ((ChunkyButton)b)._face.CommandParameter = n);

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(string), typeof(ChunkyButton), "brand", propertyChanged: (b, _, _) => ((ChunkyButton)b).Apply());

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(ChunkyButton), 16d, propertyChanged: (b, _, n) => ((ChunkyButton)b)._face.FontSize = (double)n);

    /// <summary>Estilo vacío: evita que el estilo implícito de Button (y sus estados visuales) pise los colores.</summary>
    private static readonly Style Plain = new(typeof(Button));

    private readonly Border _ledge;
    private readonly Button _face;

    public ChunkyButton()
    {
        _face = new Button
        {
            Style = Plain,
            CornerRadius = 16,
            FontFamily = "NunitoXBold",
            FontSize = 16,
            CharacterSpacing = 0.6,
            TextTransform = TextTransform.Uppercase,
            Padding = new Thickness(18, 12),
            MinimumHeightRequest = 50,
            BorderWidth = 0,
        };
        _face.Pressed += (_, _) => _face.TranslationY = Depth - 1;
        _face.Released += (_, _) => _face.TranslationY = 0;
        _face.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(IsEnabled))
            {
                Apply();
            }
        };

        _ledge = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(0, 0, 0, Depth),
            Content = _face,
        };
        Content = _ledge;
        Apply();
    }

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public string Variant
    {
        get => (string)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    private void Apply()
    {
        _face.BorderWidth = 0;
        if (!_face.IsEnabled)
        {
            Paint(("#E5E5E5", "#37464F"), ("#E5E5E5", "#37464F"), ("#AFAFAF", "#6F7E86"));
            return;
        }

        switch (Variant)
        {
            case "green":
                Paint(("#46C21C", "#46C21C"), ("#36A00E", "#36A00E"), ("#FFFFFF", "#FFFFFF"));
                break;
            case "red":
                Paint(("#FF4B4B", "#FF4B4B"), ("#E03A3A", "#E03A3A"), ("#FFFFFF", "#FFFFFF"));
                break;
            case "blue":
                Paint(("#1CB0F6", "#1CB0F6"), ("#1899D6", "#1899D6"), ("#FFFFFF", "#FFFFFF"));
                break;
            case "gold":
                Paint(("#FFC800", "#FFC800"), ("#E0A800", "#E0A800"), ("#5A3E00", "#5A3E00"));
                break;
            case "outline":
                Paint(("#FFFFFF", "#202F36"), ("#E5E5E5", "#37464F"), ("#7C4DFF", "#A98BFF"));
                _face.BorderWidth = 2;
                _face.SetAppThemeColor(Button.BorderColorProperty, Color.FromArgb("#E5E5E5"), Color.FromArgb("#37464F"));
                break;
            case "ghost":
                Paint(("#00000000", "#00000000"), ("#00000000", "#00000000"), ("#7C4DFF", "#A98BFF"));
                break;
            default:
                Paint(("#7C4DFF", "#7C4DFF"), ("#5B2FD6", "#5B2FD6"), ("#FFFFFF", "#FFFFFF"));
                break;
        }
    }

    private void Paint((string Light, string Dark) face, (string Light, string Dark) ledge, (string Light, string Dark) text)
    {
        _face.SetAppThemeColor(Button.BackgroundColorProperty, Color.FromArgb(face.Light), Color.FromArgb(face.Dark));
        _face.SetAppThemeColor(Button.TextColorProperty, Color.FromArgb(text.Light), Color.FromArgb(text.Dark));
        _ledge.SetAppThemeColor(BackgroundColorProperty, Color.FromArgb(ledge.Light), Color.FromArgb(ledge.Dark));
    }
}
