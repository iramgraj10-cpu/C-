namespace CSharpMastery.Learning.Interactive;

/// <summary>
/// Aproximación léxica de si el orden de dos líneas importa: una escribe una variable que la otra usa,
/// ambas producen salida o alguna es una instrucción de control. Se usa para elegir qué líneas reordenar;
/// la validación con ejecución real confirma la elección.
/// </summary>
public static class LineDependency
{
    private static readonly HashSet<string> ControlWords =
    [
        "if", "else", "for", "foreach", "while", "do", "switch", "case", "default", "return", "break", "continue", "throw", "yield", "try", "catch", "finally",
    ];

    private static readonly HashSet<string> Mutators =
    [
        "Add", "AddRange", "Remove", "RemoveAt", "Insert", "Clear", "Push", "Pop", "Enqueue", "Dequeue", "Append", "AppendLine", "Sort", "Reverse",
        "TryAdd", "Set", "Incrementar", "Depositar", "Retirar", "Agregar", "Quitar",
    ];

    private static readonly HashSet<string> Keywords =
    [
        "int", "double", "string", "bool", "char", "decimal", "long", "float", "var", "new", "true", "false", "null", "static", "public", "private",
        "protected", "readonly", "const", "void", "class", "record", "struct", "interface", "enum", "this", "base", "out", "ref", "in", "is", "not", "and", "or",
    ];

    public static bool Conflict(string a, string b)
    {
        var la = Analyze(a);
        var lb = Analyze(b);
        return la.Control || lb.Control || (la.Io && lb.Io)
               || la.Writes.Overlaps(lb.Ids) || lb.Writes.Overlaps(la.Ids);
    }

    private sealed record LineInfo(HashSet<string> Ids, HashSet<string> Writes, bool Io, bool Control);

    private static LineInfo Analyze(string line)
    {
        var tokens = CodeLexer.Tokenize(line).Where(t => t.Kind is not (LexKind.Whitespace or LexKind.Comment)).ToList();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var writes = new HashSet<string>(StringComparer.Ordinal);
        var io = false;
        var control = false;
        for (var i = 0; i < tokens.Count; i++)
        {
            var t = tokens[i];
            if (t.Kind == LexKind.String && t.Text.StartsWith('$'))
            {
                // Variables usadas dentro de una cadena interpolada.
                foreach (var inner in CodeLexer.Tokenize(t.Text.TrimStart('$', '@').Trim('"')).Where(x => x.Kind == LexKind.Word))
                {
                    ids.Add(inner.Text);
                }

                continue;
            }

            if (t.Kind != LexKind.Word)
            {
                continue;
            }

            if (ControlWords.Contains(t.Text))
            {
                control = true;
                continue;
            }

            if (Keywords.Contains(t.Text))
            {
                continue;
            }

            if (t.Text == "Console")
            {
                io = true;
                continue;
            }

            ids.Add(t.Text);
            var next = i + 1 < tokens.Count ? tokens[i + 1].Text : "";
            if (next is "=" or "+=" or "-=" or "*=" or "/=" or "%=" or "??=" or "++" or "--")
            {
                writes.Add(t.Text);
            }
            else if (i > 0 && tokens[i - 1].Text is "++" or "--")
            {
                writes.Add(t.Text);
            }
            else if (next == "." && i + 2 < tokens.Count && Mutators.Contains(tokens[i + 2].Text))
            {
                writes.Add(t.Text);
            }
            else if (next == "[" && tokens.Skip(i + 1).SkipWhile(x => x.Text != "]").Skip(1).FirstOrDefault().Text is "=" or "+=" or "-=" or "++" or "--")
            {
                writes.Add(t.Text);
            }
        }

        return new LineInfo(ids, writes, io, control);
    }
}
