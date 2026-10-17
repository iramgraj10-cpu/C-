using System.Globalization;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Presentation.Highlighting;
using CSharpMastery.Presentation.ViewModels.Activity;
using CSharpMastery.Presentation.ViewModels.Pages;

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

    /// <summary>Resalta la sintaxis; si se indica, marca el tramo [markStart, markStart+markLength) con un fondo (el hueco).</summary>
    public static FormattedString Format(string? code, int markStart = -1, int markLength = 0, Color? markBackground = null)
    {
        var formatted = new FormattedString();
        var position = 0;
        var markEnd = markStart + markLength;
        foreach (var token in CSharpHighlighter.Tokenize(code))
        {
            var color = token.Kind switch
            {
                TokenKind.Keyword => Keyword,
                TokenKind.Type => TypeName,
                TokenKind.String => Str,
                TokenKind.Number => Number,
                TokenKind.Comment => Comment,
                _ => Text,
            };
            var start = position;
            var end = position + token.Text.Length;
            position = end;
            if (markStart < 0 || markBackground is null || end <= markStart || start >= markEnd)
            {
                formatted.Spans.Add(new Span { Text = token.Text, TextColor = color });
                continue;
            }

            // El token cruza el tramo marcado: se parte en antes / marcado / después.
            var a = Math.Max(markStart, start) - start;
            var b = Math.Min(markEnd, end) - start;
            if (a > 0)
            {
                formatted.Spans.Add(new Span { Text = token.Text[..a], TextColor = color });
            }

            formatted.Spans.Add(new Span { Text = token.Text[a..b], TextColor = Colors.White, BackgroundColor = markBackground, FontAttributes = FontAttributes.Bold });
            if (b < token.Text.Length)
            {
                formatted.Spans.Add(new Span { Text = token.Text[b..], TextColor = color });
            }
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
            true => Color.FromArgb(dark ? "#1E3A14" : "#D7FFB8"),
            false => Color.FromArgb(dark ? "#3F1F22" : "#FFDFE0"),
            _ => Color.FromArgb(dark ? "#2B2552" : "#EDE7FF"),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Color del borde de una opción tras corregir: verde si es correcta, rojo si fue elegida y es incorrecta.</summary>
public sealed class OptionStrokeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        true => Color.FromArgb("#46C21C"),
        false => Color.FromArgb("#FF4B4B"),
        _ => Application.Current?.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#37464F") : Color.FromArgb("#E5E5E5"),
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class SelectedBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        return value is true ? Color.FromArgb(dark ? "#173A4C" : "#DDF4FF") : Color.FromArgb(dark ? "#202F36" : "#FFFFFF");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class MasteryColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        MasteryLevel.Retained => Color.FromArgb("#FFC800"),
        MasteryLevel.ProvisionallyMastered => Color.FromArgb("#46C21C"),
        MasteryLevel.Practiced => Color.FromArgb("#1CB0F6"),
        MasteryLevel.Seen => Color.FromArgb("#A98BFF"),
        _ => Color.FromArgb("#C9C9C9"),
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

/// <summary>Colores de una ficha, línea u opción según su estado. Parámetro: bg, border, ledge o text.</summary>
public sealed class ChipStateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var state = value as ChipState? ?? ChipState.Normal;
        var (bg, border, ledge, text) = state switch
        {
            ChipState.Selected => (dark ? "#173A4C" : "#DDF4FF", "#1CB0F6", "#1899D6", dark ? "#7FD3FA" : "#1899D6"),
            ChipState.Correct => (dark ? "#1E3A14" : "#D7FFB8", "#46C21C", "#36A00E", dark ? "#79E04F" : "#2B8A0E"),
            ChipState.Wrong => (dark ? "#3F1F22" : "#FFDFE0", "#FF4B4B", "#E03A3A", dark ? "#FF7B7B" : "#D33131"),
            ChipState.Used => (dark ? "#37464F" : "#E5E5E5", dark ? "#37464F" : "#E5E5E5", dark ? "#37464F" : "#E5E5E5", "#00000000"),
            _ => (dark ? "#202F36" : "#FFFFFF", dark ? "#37464F" : "#E5E5E5", dark ? "#2A363D" : "#E5E5E5", dark ? "#F1F7FB" : "#3C3C3C"),
        };
        return Color.FromArgb((parameter as string) switch
        {
            "border" => border,
            "ledge" => ledge,
            "text" => text,
            "mark" => state switch { ChipState.Correct => "#46C21C", ChipState.Wrong => "#FF4B4B", ChipState.Selected => "#1CB0F6", _ => "#7C4DFF" },
            _ => bg,
        });
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>
/// Aspecto del pie según la franja de retroalimentación. Parámetro: bg (fondo del pie), fg (título),
/// variant (color del botón principal), o icon (glifo).
/// </summary>
public sealed class BannerConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var kind = value as BannerKind? ?? BannerKind.None;
        return (parameter as string) switch
        {
            "fg" => Color.FromArgb(kind switch
            {
                BannerKind.Correct => dark ? "#79E04F" : "#2B8A0E",
                BannerKind.Incorrect => dark ? "#FF7B7B" : "#D33131",
                _ => dark ? "#A98BFF" : "#5B2FD6",
            }),
            "variant" => kind switch { BannerKind.Correct => "green", BannerKind.Incorrect => "red", _ => "brand" },
            "icon" => kind switch { BannerKind.Correct => "\ue5ca", BannerKind.Incorrect => "\ue5cd", _ => "\ue0f0" },
            _ => Color.FromArgb(kind switch
            {
                BannerKind.Correct => dark ? "#1E3A14" : "#D7FFB8",
                BannerKind.Incorrect => dark ? "#3F1F22" : "#FFDFE0",
                BannerKind.Info => dark ? "#2B2552" : "#EDE7FF",
                _ => dark ? "#131F24" : "#FFFFFF",
            }),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Aspecto de un nodo del camino. Parámetro: face, ledge, icon o fg.</summary>
public sealed class NodeStateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var state = value as NodeState? ?? NodeState.Locked;
        var gray = dark ? "#37464F" : "#E5E5E5";
        var grayLedge = dark ? "#2A363D" : "#C9C9C9";
        return (parameter as string) switch
        {
            "ledge" => Color.FromArgb(state switch
            {
                NodeState.Available or NodeState.InProgress => "#5B2FD6",
                NodeState.Completed => "#36A00E",
                NodeState.Legendary => "#E0A800",
                _ => grayLedge,
            }),
            "icon" => state switch
            {
                NodeState.Soon => "\ue425",
                NodeState.Locked => "\ue897",
                NodeState.Completed => "\ue5ca",
                NodeState.Legendary => "\ue7af",
                NodeState.InProgress => "\ue037",
                _ => "\ue838",
            },
            "fg" => Color.FromArgb(state is NodeState.Soon or NodeState.Locked ? (dark ? "#6F7E86" : "#AFAFAF") : "#FFFFFF"),
            _ => Color.FromArgb(state switch
            {
                NodeState.Available or NodeState.InProgress => "#7C4DFF",
                NodeState.Completed => "#46C21C",
                NodeState.Legendary => "#FFC800",
                _ => gray,
            }),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Color de cada nivel del temario (encabezados de unidad en el camino). Parámetro «ledge» para el tono oscuro.</summary>
public sealed class LevelColorConverter : IValueConverter
{
    private static readonly (string Face, string Ledge)[] Palette =
    [
        ("#7C4DFF", "#5B2FD6"),
        ("#46C21C", "#36A00E"),
        ("#1CB0F6", "#1899D6"),
        ("#FF9600", "#E07B00"),
        ("#FF4B8B", "#E0336F"),
        ("#00B8A9", "#00968A"),
        ("#CE82FF", "#A560E0"),
        ("#E89A00", "#C07F00"),
    ];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var level = value is int i ? i : 0;
        var (face, ledge) = Palette[((level % Palette.Length) + Palette.Length) % Palette.Length];
        return Color.FromArgb(parameter as string == "ledge" ? ledge : face);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Sangría de una línea de código (texto de espacios) → margen izquierdo.</summary>
public sealed class IndentMarginConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var indent = value as string ?? "";
        var width = indent.Sum(c => c == '\t' ? 4 : 1);
        return new Thickness(width * 7, 0, 0, 0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Igualdad con el parámetro (por ejemplo, para marcar la opción elegida).</summary>
public sealed class EqualsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Equals(value?.ToString(), parameter?.ToString());

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>True si los dos valores son iguales (por ejemplo, la duración elegida y la de la ficha).</summary>
public sealed class AreEqualConverter : IMultiValueConverter
{
    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture) =>
        values.Length == 2 && Equals(values[0], values[1]);

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Nombre de archivo de imagen (por ejemplo, la mascota del paso) → ImageSource.</summary>
public sealed class ImageNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string name && name.Length > 0 ? ImageSource.FromFile(name) : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
