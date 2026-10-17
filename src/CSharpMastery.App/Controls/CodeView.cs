using CSharpMastery.App.Converters;
using CSharpMastery.Presentation.Highlighting;

namespace CSharpMastery.App.Controls;

/// <summary>Código de solo lectura con números de línea y resaltado de sintaxis; se desplaza horizontalmente.</summary>
public sealed class CodeView : ContentView
{
    public static readonly BindableProperty CodeProperty =
        BindableProperty.Create(nameof(Code), typeof(string), typeof(CodeView), null, propertyChanged: (b, _, n) => ((CodeView)b).Render(n as string));

    public static readonly BindableProperty CaptionProperty =
        BindableProperty.Create(nameof(Caption), typeof(string), typeof(CodeView), null, propertyChanged: (b, _, _) => ((CodeView)b).Render(((CodeView)b).Code));

    public static readonly BindableProperty MarkStartProperty =
        BindableProperty.Create(nameof(MarkStart), typeof(int), typeof(CodeView), -1, propertyChanged: (b, _, _) => ((CodeView)b).Render(((CodeView)b).Code));

    public static readonly BindableProperty MarkLengthProperty =
        BindableProperty.Create(nameof(MarkLength), typeof(int), typeof(CodeView), 0, propertyChanged: (b, _, _) => ((CodeView)b).Render(((CodeView)b).Code));

    public static readonly BindableProperty MarkColorProperty =
        BindableProperty.Create(nameof(MarkColor), typeof(Color), typeof(CodeView), null, propertyChanged: (b, _, _) => ((CodeView)b).Render(((CodeView)b).Code));

    private readonly Label _numbers;
    private readonly Label _code;
    private readonly Label _caption;

    public CodeView()
    {
        var mono = Application.Current?.Resources.TryGetValue("MonoFont", out var f) == true ? f as string : null;
        _numbers = new Label { FontFamily = mono, FontSize = 14, TextColor = Color.FromArgb("#7F7F95"), HorizontalTextAlignment = TextAlignment.End, LineBreakMode = LineBreakMode.NoWrap };
        _code = new Label { FontFamily = mono, FontSize = 14, LineBreakMode = LineBreakMode.NoWrap };
        _caption = new Label { FontFamily = "NunitoXBold", FontSize = 12, TextColor = Color.FromArgb("#B4B2BE"), IsVisible = false, Margin = new Thickness(0, 0, 0, 6) };
        SemanticProperties.SetDescription(_numbers, "Números de línea");

        var grid = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star)], ColumnSpacing = 12 };
        grid.Add(_numbers, 0);
        grid.Add(_code, 1);

        Content = new Border
        {
            BackgroundColor = Color.FromArgb("#1E1B33"),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
            Padding = new Thickness(14),
            Content = new VerticalStackLayout
            {
                Children = { _caption, new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = grid } },
            },
        };
    }

    public string? Code
    {
        get => (string?)GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    /// <summary>Inicio del tramo resaltado (el hueco de un ejercicio de completar); -1 si no hay.</summary>
    public int MarkStart
    {
        get => (int)GetValue(MarkStartProperty);
        set => SetValue(MarkStartProperty, value);
    }

    public int MarkLength
    {
        get => (int)GetValue(MarkLengthProperty);
        set => SetValue(MarkLengthProperty, value);
    }

    public Color? MarkColor
    {
        get => (Color?)GetValue(MarkColorProperty);
        set => SetValue(MarkColorProperty, value);
    }

    public string? Caption
    {
        get => (string?)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    private void Render(string? code)
    {
        _numbers.Text = CSharpHighlighter.LineNumbers(code);
        _code.FormattedText = CodeHighlightConverter.Format(code, MarkStart, MarkLength, MarkColor);
        SemanticProperties.SetDescription(_code, "Código: " + code);
        _caption.Text = Caption;
        _caption.IsVisible = !string.IsNullOrEmpty(Caption);
    }
}
