namespace CSharpMastery.Execution;

/// <summary>Compara salidas de consola de forma tolerante a diferencias irrelevantes.</summary>
public static class OutputComparer
{
    /// <summary>Normaliza saltos de línea, quita espacios al final de cada línea y líneas vacías finales.</summary>
    public static string Normalize(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').Select(l => l.TrimEnd()).ToList();
        while (lines.Count > 0 && lines[^1].Length == 0)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return string.Join('\n', lines);
    }

    public static bool AreEquivalent(string? expected, string? actual) => Normalize(expected) == Normalize(actual);

    /// <summary>Describe la primera diferencia, o null si son equivalentes.</summary>
    public static OutputDifference? FirstDifference(string? expected, string? actual)
    {
        var e = Normalize(expected).Split('\n');
        var a = Normalize(actual).Split('\n');
        var max = Math.Max(e.Length, a.Length);
        for (var i = 0; i < max; i++)
        {
            var el = i < e.Length ? e[i] : null;
            var al = i < a.Length ? a[i] : null;
            if (el != al)
            {
                var onlyCaseOrSpaces = el is not null && al is not null &&
                    string.Equals(Squash(el), Squash(al), StringComparison.OrdinalIgnoreCase);
                return new OutputDifference(i + 1, el, al, onlyCaseOrSpaces);
            }
        }

        return null;
    }

    private static string Squash(string s) => string.Concat(s.Where(c => !char.IsWhiteSpace(c)));
}

/// <summary>Primera línea distinta. <see cref="FormatOnly"/> indica que solo difieren mayúsculas o espacios.</summary>
public sealed record OutputDifference(int Line, string? Expected, string? Actual, bool FormatOnly);
