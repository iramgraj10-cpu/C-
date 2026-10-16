namespace CSharpMastery.Presentation.Highlighting;

public enum TokenKind
{
    Plain,
    Keyword,
    Type,
    String,
    Comment,
    Number,
}

public readonly record struct CodeToken(string Text, TokenKind Kind);

/// <summary>
/// Resaltador léxico sencillo para mostrar código. No es un analizador completo de C#:
/// reconoce comentarios, cadenas (incluidas interpoladas y literales), caracteres, números,
/// palabras clave y nombres de tipos comunes.
/// </summary>
public static class CSharpHighlighter
{
    private static readonly HashSet<string> Keywords =
    [
        "abstract", "as", "async", "await", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
        "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false",
        "finally", "fixed", "float", "for", "foreach", "get", "goto", "if", "implicit", "in", "init", "int", "interface", "internal", "is",
        "lock", "long", "nameof", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private",
        "protected", "public", "readonly", "record", "ref", "required", "return", "sbyte", "sealed", "set", "short", "sizeof", "static",
        "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
        "ushort", "using", "var", "virtual", "void", "volatile", "when", "where", "while", "yield", "with", "and", "or", "not",
    ];

    private static readonly HashSet<string> Types =
    [
        "Console", "Math", "List", "Dictionary", "HashSet", "Queue", "Stack", "String", "Int32", "Task", "Func", "Action",
        "Exception", "IEnumerable", "IQueryable", "DateTime", "TimeSpan", "StringBuilder", "Array", "Enumerable", "CancellationToken",
        "ArgumentException", "InvalidOperationException", "Random", "Convert", "JsonSerializer", "IDisposable",
    ];

    public static IReadOnlyList<CodeToken> Tokenize(string? code)
    {
        var tokens = new List<CodeToken>();
        if (string.IsNullOrEmpty(code))
        {
            return tokens;
        }

        var i = 0;
        var plainStart = 0;

        void FlushPlain(int end)
        {
            if (end > plainStart)
            {
                tokens.Add(new CodeToken(code[plainStart..end], TokenKind.Plain));
            }
        }

        void Emit(int start, int end, TokenKind kind)
        {
            FlushPlain(start);
            tokens.Add(new CodeToken(code[start..end], kind));
            plainStart = end;
        }

        while (i < code.Length)
        {
            var c = code[i];
            if (c == '/' && i + 1 < code.Length && code[i + 1] == '/')
            {
                var end = code.IndexOf('\n', i);
                end = end < 0 ? code.Length : end;
                Emit(i, end, TokenKind.Comment);
                i = end;
            }
            else if (c == '/' && i + 1 < code.Length && code[i + 1] == '*')
            {
                var end = code.IndexOf("*/", i + 2, StringComparison.Ordinal);
                end = end < 0 ? code.Length : end + 2;
                Emit(i, end, TokenKind.Comment);
                i = end;
            }
            else if (c == '"' || ((c == '$' || c == '@') && i + 1 < code.Length && (code[i + 1] == '"' || code[i + 1] is '$' or '@')))
            {
                var start = i;
                var verbatim = false;
                while (i < code.Length && code[i] is '$' or '@')
                {
                    verbatim |= code[i] == '@';
                    i++;
                }

                if (i < code.Length && code[i] == '"')
                {
                    i++;
                    while (i < code.Length)
                    {
                        if (!verbatim && code[i] == '\\' && i + 1 < code.Length)
                        {
                            i += 2;
                            continue;
                        }

                        if (code[i] == '"')
                        {
                            if (verbatim && i + 1 < code.Length && code[i + 1] == '"')
                            {
                                i += 2;
                                continue;
                            }

                            i++;
                            break;
                        }

                        if (code[i] == '\n' && !verbatim)
                        {
                            break;
                        }

                        i++;
                    }

                    Emit(start, i, TokenKind.String);
                }
            }
            else if (c == '\'')
            {
                var start = i++;
                while (i < code.Length && code[i] != '\'' && code[i] != '\n')
                {
                    i += code[i] == '\\' ? 2 : 1;
                }

                i = Math.Min(code.Length, i + 1);
                Emit(start, i, TokenKind.String);
            }
            else if (char.IsDigit(c))
            {
                var start = i;
                while (i < code.Length && (char.IsLetterOrDigit(code[i]) || code[i] is '.' or '_'))
                {
                    i++;
                }

                Emit(start, i, TokenKind.Number);
            }
            else if (char.IsLetter(c) || c == '_')
            {
                var start = i;
                while (i < code.Length && (char.IsLetterOrDigit(code[i]) || code[i] == '_'))
                {
                    i++;
                }

                var word = code[start..i];
                if (Keywords.Contains(word))
                {
                    Emit(start, i, TokenKind.Keyword);
                }
                else if (Types.Contains(word))
                {
                    Emit(start, i, TokenKind.Type);
                }
            }
            else
            {
                i++;
            }
        }

        FlushPlain(code.Length);
        return tokens;
    }

    /// <summary>Números de línea («1\n2\n3») para mostrar junto al editor.</summary>
    public static string LineNumbers(string? code)
    {
        var count = string.IsNullOrEmpty(code) ? 1 : code.Count(ch => ch == '\n') + 1;
        return string.Join('\n', Enumerable.Range(1, count));
    }
}
