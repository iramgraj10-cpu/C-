namespace CSharpMastery.Learning.Interactive;

public enum LexKind
{
    Whitespace,
    Comment,
    String,
    Char,
    Number,
    Word,
    Operator,
    Punctuation,
}

/// <summary>Fragmento léxico con su posición en el texto original.</summary>
public readonly record struct LexToken(int Start, string Text, LexKind Kind)
{
    public int End => Start + Text.Length;
}

/// <summary>
/// Analizador léxico mínimo para generar ejercicios interactivos a partir de código de las lecciones.
/// Distingue lo necesario para no proponer huecos dentro de cadenas o comentarios.
/// </summary>
public static class CodeLexer
{
    private static readonly string[] Operators =
    [
        "<<=", ">>=", "??=", "=>", "==", "!=", "<=", ">=", "&&", "||", "++", "--", "+=", "-=", "*=", "/=", "%=", "??", "?.",
        "<<", ">>", "+", "-", "*", "/", "%", "<", ">", "=", "!", "&", "|", "^", "~", "?", ":",
    ];

    public static IReadOnlyList<LexToken> Tokenize(string code)
    {
        var tokens = new List<LexToken>();
        var i = 0;
        while (i < code.Length)
        {
            var start = i;
            var c = code[i];
            if (char.IsWhiteSpace(c))
            {
                while (i < code.Length && char.IsWhiteSpace(code[i]))
                {
                    i++;
                }

                tokens.Add(new LexToken(start, code[start..i], LexKind.Whitespace));
                continue;
            }

            if (c == '/' && i + 1 < code.Length && code[i + 1] == '/')
            {
                while (i < code.Length && code[i] != '\n')
                {
                    i++;
                }

                tokens.Add(new LexToken(start, code[start..i], LexKind.Comment));
                continue;
            }

            if (c == '/' && i + 1 < code.Length && code[i + 1] == '*')
            {
                var end = code.IndexOf("*/", i + 2, StringComparison.Ordinal);
                i = end < 0 ? code.Length : end + 2;
                tokens.Add(new LexToken(start, code[start..i], LexKind.Comment));
                continue;
            }

            if (c == '"' || ((c == '$' || c == '@') && i + 1 < code.Length && (code[i + 1] == '"' || code[i + 1] is '$' or '@')))
            {
                i = SkipString(code, i);
                tokens.Add(new LexToken(start, code[start..i], LexKind.String));
                continue;
            }

            if (c == '\'')
            {
                i++;
                while (i < code.Length && code[i] != '\'' && code[i] != '\n')
                {
                    i += code[i] == '\\' ? 2 : 1;
                }

                i = Math.Min(code.Length, i + 1);
                tokens.Add(new LexToken(start, code[start..i], LexKind.Char));
                continue;
            }

            if (char.IsDigit(c))
            {
                while (i < code.Length && (char.IsLetterOrDigit(code[i]) || code[i] == '_' || (code[i] == '.' && i + 1 < code.Length && char.IsDigit(code[i + 1]))))
                {
                    i++;
                }

                tokens.Add(new LexToken(start, code[start..i], LexKind.Number));
                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                while (i < code.Length && (char.IsLetterOrDigit(code[i]) || code[i] == '_'))
                {
                    i++;
                }

                tokens.Add(new LexToken(start, code[start..i], LexKind.Word));
                continue;
            }

            var op = Operators.FirstOrDefault(o => string.CompareOrdinal(code, i, o, 0, o.Length) == 0);
            if (op is not null)
            {
                i += op.Length;
                tokens.Add(new LexToken(start, op, LexKind.Operator));
                continue;
            }

            i++;
            tokens.Add(new LexToken(start, code[start..i], LexKind.Punctuation));
        }

        return tokens;
    }

    /// <summary>Avanza sobre una cadena normal, interpolada, literal o sin procesar ("""...""").</summary>
    private static int SkipString(string code, int i)
    {
        var verbatim = false;
        while (i < code.Length && code[i] is '$' or '@')
        {
            verbatim |= code[i] == '@';
            i++;
        }

        if (string.CompareOrdinal(code, i, "\"\"\"", 0, 3) == 0)
        {
            var end = code.IndexOf("\"\"\"", i + 3, StringComparison.Ordinal);
            return end < 0 ? code.Length : end + 3;
        }

        i++; // comilla inicial
        var depth = 0;
        while (i < code.Length)
        {
            var c = code[i];
            if (!verbatim && c == '\\')
            {
                i += 2;
                continue;
            }

            if (c == '{')
            {
                depth++;
            }
            else if (c == '}' && depth > 0)
            {
                depth--;
            }
            else if (c == '"' && depth == 0)
            {
                if (verbatim && i + 1 < code.Length && code[i + 1] == '"')
                {
                    i += 2;
                    continue;
                }

                return i + 1;
            }
            else if (c == '\n' && !verbatim)
            {
                return i;
            }

            i++;
        }

        return i;
    }
}
