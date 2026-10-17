using System.Globalization;

namespace CSharpMastery.Learning.Interactive;

/// <summary>Hueco posible en un código: la ficha correcta y las que se ofrecen como distractores.</summary>
public sealed record ClozeCandidate(int Start, string Answer, IReadOnlyList<string> Distractors, string Group);

/// <summary>
/// Propone huecos en código de las lecciones. Solo usa piezas con significado (operadores de comparación,
/// palabras clave, métodos conocidos, números) y distractores del mismo grupo. Las pruebas de contenido
/// comprueban que cada distractor no compile o cambie el resultado; si alguno resulta equivalente, la clave
/// del hueco se excluye y se usa el siguiente candidato.
/// </summary>
public static class ClozeGenerator
{
    private sealed record Group(string Name, int Priority, string[] Members);

    // Prioridad menor = se prefiere. Los grupos reflejan lo que cada lección suele querer fijar.
    private static readonly Group[] Groups =
    [
        new("comparación", 0, ["<", "<=", ">", ">=", "==", "!="]),
        new("lógico", 0, ["&&", "||"]),
        new("asignación compuesta", 1, ["+=", "-=", "*=", "/="]),
        new("incremento", 1, ["++", "--"]),
        new("bucle", 1, ["for", "while", "foreach"]),
        new("salto", 1, ["break", "continue", "return"]),
        new("excepciones", 1, ["try", "catch", "finally", "throw"]),
        new("herencia", 1, ["virtual", "override", "abstract", "sealed"]),
        new("parámetro", 1, ["ref", "out", "in", "params"]),
        new("iterador", 1, ["yield"]),
        new("LINQ", 2, ["Where", "Select", "OrderBy", "OrderByDescending", "Any", "All", "Sum", "Max", "Min", "First", "Take", "Skip", "GroupBy", "Average"]),
        new("colección", 2, ["Add", "Remove", "Contains", "ContainsKey", "Insert", "Clear", "TryGetValue", "IndexOf"]),
        new("texto", 2, ["ToUpper", "ToLower", "Trim", "Split", "Substring", "Replace", "StartsWith", "EndsWith", "Join", "PadLeft"]),
        new("tamaño", 2, ["Length", "Count"]),
        new("consola", 3, ["WriteLine", "Write", "ReadLine"]),
        new("tipo", 3, ["int", "double", "string", "bool", "char", "decimal", "long"]),
        new("booleano", 3, ["true", "false"]),
        new("acceso", 4, ["public", "private", "protected"]),
        new("operador aritmético", 4, ["+", "-", "*", "/", "%"]),
        new("estático", 4, ["static", "const", "readonly"]),
    ];

    private static readonly Dictionary<string, Group> ByToken = BuildIndex();

    private static Dictionary<string, Group> BuildIndex()
    {
        var index = new Dictionary<string, Group>(StringComparer.Ordinal);
        foreach (var group in Groups)
        {
            foreach (var member in group.Members)
            {
                index.TryAdd(member, group);
            }
        }

        return index;
    }

    /// <summary>Candidatos ordenados de mejor a peor; <paramref name="seed"/> varía la elección de forma determinista.</summary>
    public static IReadOnlyList<ClozeCandidate> Candidates(string code, string seed)
    {
        var tokens = CodeLexer.Tokenize(code);
        var counts = tokens.Where(t => t.Kind is LexKind.Word or LexKind.Operator or LexKind.Number)
            .GroupBy(t => t.Text).ToDictionary(g => g.Key, g => g.Count());
        var hash = StableHash(seed);
        var generic = GenericBrackets(tokens);
        var list = new List<(ClozeCandidate Candidate, int Priority, int Order)>();
        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (generic.Contains(i) || Distractors(token, tokens, i, hash) is not { } found)
            {
                continue;
            }

            // Se prefieren piezas únicas: si la respuesta aparece varias veces, el hueco se adivina copiando.
            var priority = found.Priority + (counts.GetValueOrDefault(token.Text) > 1 ? 2 : 0);
            list.Add((new ClozeCandidate(token.Start, token.Text, found.Options, found.Group), priority, (i * 7919 + hash) % 1009));
        }

        return list.OrderBy(x => x.Priority).ThenBy(x => x.Order).Select(x => x.Candidate).ToList();
    }

    private static (int Priority, string Group, string[] Options)? Distractors(LexToken token, IReadOnlyList<LexToken> tokens, int index, int hash)
    {
        if (token.Kind == LexKind.Number)
        {
            if (!int.TryParse(token.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var n) || n > 1000)
            {
                return null;
            }

            string[] options = n == 0 ? ["1", "2"] : [(n - 1).ToString(CultureInfo.InvariantCulture), (n + 1).ToString(CultureInfo.InvariantCulture)];
            return (3, "número", options);
        }

        if (token.Kind is not (LexKind.Word or LexKind.Operator) || !ByToken.TryGetValue(token.Text, out var group))
        {
            return null;
        }

        // Un '-' o '+' unario o un '*' de puntero no son aritmética binaria clara.
        if (group.Name == "operador aritmético" && !IsBinary(tokens, index))
        {
            return null;
        }

        var others = group.Members.Where(m => m != token.Text).ToList();
        if (others.Count == 0)
        {
            return null;
        }

        var picked = others.OrderBy(m => StableHash(m + hash)).Take(group.Members.Length >= 4 ? 3 : 2).ToArray();
        return (group.Priority, group.Name, picked);
    }

    /// <summary>Índices de los '<' y '>' que delimitan argumentos de tipo (List&lt;int&gt;), que no son comparaciones.</summary>
    private static HashSet<int> GenericBrackets(IReadOnlyList<LexToken> tokens)
    {
        var result = new HashSet<int>();
        for (var i = 0; i < tokens.Count; i++)
        {
            if (tokens[i].Text != "<" || Previous(tokens, i) is not { Kind: LexKind.Word } prev || !char.IsUpper(prev.Text[0]) && prev.Text != "Func")
            {
                continue;
            }

            var depth = 0;
            var opened = new List<int>();
            for (var j = i; j < tokens.Count; j++)
            {
                var t = tokens[j];
                if (t.Text == "<")
                {
                    depth++;
                    opened.Add(j);
                }
                else if (t.Text == ">")
                {
                    depth--;
                    opened.Add(j);
                    if (depth == 0)
                    {
                        result.UnionWith(opened);
                        break;
                    }
                }
                else if (t.Text is ";" or "{" or "}" or "=" or "&&" or "||" || t.Kind is LexKind.Number or LexKind.String)
                {
                    break;
                }
            }
        }

        return result;
    }

    private static bool IsBinary(IReadOnlyList<LexToken> tokens, int index)
    {
        var prev = Previous(tokens, index);
        return prev is { Kind: LexKind.Word or LexKind.Number or LexKind.String } || prev is { Text: ")" or "]" };
    }

    private static LexToken? Previous(IReadOnlyList<LexToken> tokens, int index)
    {
        for (var j = index - 1; j >= 0; j--)
        {
            if (tokens[j].Kind is not (LexKind.Whitespace or LexKind.Comment))
            {
                return tokens[j];
            }
        }

        return null;
    }

    private static LexToken? Next(IReadOnlyList<LexToken> tokens, int index)
    {
        for (var j = index + 1; j < tokens.Count; j++)
        {
            if (tokens[j].Kind is not (LexKind.Whitespace or LexKind.Comment))
            {
                return tokens[j];
            }
        }

        return null;
    }

    /// <summary>Hash estable entre ejecuciones (string.GetHashCode cambia en cada proceso).</summary>
    public static int StableHash(string text)
    {
        unchecked
        {
            var h = 2166136261u;
            foreach (var c in text)
            {
                h = (h ^ c) * 16777619u;
            }

            return (int)(h & 0x7FFFFFFF);
        }
    }
}
