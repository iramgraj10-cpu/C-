using System.Globalization;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Presentation.Highlighting;

namespace CSharpMastery.App.Converters;

/// <summary>Convierte código C# en texto con resaltado de sintaxis.</summary>
public sealed class CodeHighlightConverter : IValueConverter
{
    private static readonly Color Text = Color.FromArgb("#E6E6EF");
    private static readonly Color Keyword = Color.FromArgb("#C792EA");
    private static readonly Color TypeName = Color.FromArgb("#82AAFF");
    private static readonly Color Str = Color.FromArgb("#C3E88D");
    private static readonly Color Number = Color.FromArgb("#F78C6C");
    private static readonly Color Comment = Color.FromArgb("#8A93A8");

    public static FormattedString Format(string? code)
    {
        var formatted = new FormattedString();
        foreach (var token in CSharpHighlighter.Tokenize(code))
        {
            formatted.Spans.Add(new Span
            {
                Text = token.Text,
                TextColor = token.Kind switch
                {
                    TokenKind.Keyword => Keyword,
                    TokenKind.Type => TypeName,
                    TokenKind.String => Str,
                    TokenKind.Number => Number,
                    TokenKind.Comment => Comment,
                    _ => Text,
                },
            });
        }

        return formatted;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => Format(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class InvertedBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is not true;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value is not true;
}

/// <summary>True si el valor no es nulo ni una cadena vacía (o una colección vacía).</summary>
public sealed class HasValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        null => false,
        string s => !string.IsNullOrWhiteSpace(s),
        System.Collections.ICollection c => c.Count > 0,
        _ => true,
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>bool? → color de fondo de retroalimentación (verde, rojo o neutro).</summary>
public sealed class FeedbackBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        return value switch
        {
            true => Color.FromArgb(dark ? "#1D3324" : "#E5F4E8"),
            false => Color.FromArgb(dark ? "#3A1E21" : "#FBE7E8"),
            _ => Color.FromArgb(dark ? "#2A2340" : "#EEE8F8"),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Color del borde de una opción tras corregir: verde si es correcta, rojo si fue elegida y es incorrecta.</summary>
public sealed class OptionStrokeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        true => Color.FromArgb("#2E8B47"),
        false => Color.FromArgb("#C0392B"),
        _ => Color.FromArgb("#8A87A0"),
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class SelectedBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        return value is true ? Color.FromArgb(dark ? "#2A2340" : "#EEE8F8") : Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class MasteryColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        MasteryLevel.Retained => Color.FromArgb("#2E8B47"),
        MasteryLevel.ProvisionallyMastered => Color.FromArgb("#3D7DCA"),
        MasteryLevel.Practiced => Color.FromArgb("#C48A1A"),
        MasteryLevel.Seen => Color.FromArgb("#8A87A0"),
        _ => Color.FromArgb("#B9B7C6"),
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>bool → texto de estado: "✔" o "✘".</summary>
public sealed class PassMarkConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is true ? "✔" : "○";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>True si el número es mayor que cero (útil con Count de colecciones observables).</summary>
public sealed class IsPositiveConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is int i && i > 0;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>True si todos los valores son true.</summary>
public sealed class AllTrueConverter : IMultiValueConverter
{
    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture) => values.All(v => v is true);

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
