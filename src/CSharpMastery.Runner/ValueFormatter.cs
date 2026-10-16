using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace CSharpMastery.Runner;

/// <summary>
/// Convierte valores devueltos por el código del estudiante en texto estable para compararlos con lo esperado.
/// Reglas: null → "null"; bool → "true"/"false"; números con cultura invariante; colecciones → "[a, b]";
/// diccionarios → "{k: v}"; tuplas → "(a, b)"; cadenas tal cual (dentro de colecciones, entre comillas).
/// </summary>
public static class ValueFormatter
{
    public static string Format(object? value) => Format(value, nested: false, depth: 0);

    private static string Format(object? value, bool nested, int depth)
    {
        if (depth > 8)
        {
            return "…";
        }

        switch (value)
        {
            case null:
                return "null";
            case string s:
                return nested ? $"\"{s}\"" : s;
            case char c:
                return nested ? $"'{c}'" : c.ToString();
            case bool b:
                return b ? "true" : "false";
            case IFormattable f when value.GetType().IsPrimitive || value is decimal:
                return f.ToString(null, CultureInfo.InvariantCulture);
            case IDictionary dict:
            {
                var parts = new List<string>();
                foreach (DictionaryEntry entry in dict)
                {
                    parts.Add($"{Format(entry.Key, true, depth + 1)}: {Format(entry.Value, true, depth + 1)}");
                }

                return "{" + string.Join(", ", parts) + "}";
            }
            case ITuple tuple when value.GetType().FullName?.StartsWith("System.ValueTuple", StringComparison.Ordinal) == true
                                   || value.GetType().FullName?.StartsWith("System.Tuple", StringComparison.Ordinal) == true:
            {
                var parts = new string[tuple.Length];
                for (var i = 0; i < tuple.Length; i++)
                {
                    parts[i] = Format(tuple[i], true, depth + 1);
                }

                return "(" + string.Join(", ", parts) + ")";
            }
            case IEnumerable enumerable:
            {
                var sb = new StringBuilder("[");
                var first = true;
                var count = 0;
                foreach (var item in enumerable)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }

                    if (++count > 1000)
                    {
                        sb.Append('…');
                        break;
                    }

                    sb.Append(Format(item, true, depth + 1));
                    first = false;
                }

                return sb.Append(']').ToString();
            }
            case IFormattable f:
                return f.ToString(null, CultureInfo.InvariantCulture);
            default:
                return value.ToString() ?? "";
        }
    }
}
