using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Learning.Interactive;

/// <summary>
/// Convierte una lección en pasos cortos e interactivos: tarjetas breves alternadas con huecos que completar,
/// el ejemplo resuelto recorrido línea a línea, un ejercicio de ordenar líneas y la búsqueda del error habitual.
/// Es determinista: la misma lección produce siempre los mismos pasos, y las claves en <c>exclusions</c>
/// (huecos u órdenes que la validación encontró ambiguos) se saltan.
/// <c>outputs</c> da la salida real de un bloque de código: un hueco solo se genera si se puede mostrar qué debe
/// imprimir el programa, porque sin eso una ficha que compila y cambia el resultado también sería válida.
/// </summary>
public sealed class LessonFlowBuilder(IReadOnlySet<string>? exclusions = null, Func<string, string?>? outputs = null)
{
    /// <summary>Máximo de huecos por lección en la explicación.</summary>
    public const int MaxExplanationClozes = 2;

    /// <summary>Máximo de líneas que se reordenan (con más, el ejercicio se vuelve tedioso en el teléfono).</summary>
    public const int MaxParsonsLines = 5;

    private readonly IReadOnlySet<string> _exclusions = exclusions ?? new HashSet<string>();

    /// <summary>Introducción, tarjetas de explicación y huecos.</summary>
    public IReadOnlyList<MicroStep> Explanation(Skill skill, Lesson lesson)
    {
        var steps = new List<MicroStep> { new IntroStep($"{skill.Id}|intro", skill.Title, lesson.Objective, lesson.EstimatedMinutes) };
        var cards = Chunk(lesson.Explanation);
        var clozes = 0;
        for (var i = 0; i < cards.Count; i++)
        {
            steps.Add(new ConceptCardStep($"{skill.Id}|card|{i}", cards[i], i + 1, cards.Count));
            if (clozes >= MaxExplanationClozes)
            {
                continue;
            }

            var code = cards[i].OfType<CodeBlock>().FirstOrDefault(b => !IsFragment(b));
            if (code is not null && outputs?.Invoke(code.Code) is { Length: > 0 } output &&
                Cloze(skill.Id, $"card{i}", code.Code, output) is { } cloze)
            {
                steps.Add(cloze);
                clozes++;
            }
        }

        return steps;
    }

    /// <summary>Ejemplo resuelto: recorrido, (hueco si la explicación no tuvo), ordenar líneas y error habitual.</summary>
    public IReadOnlyList<MicroStep> Practice(Skill skill, Lesson lesson, bool includeCloze)
    {
        var example = lesson.WorkedExample;
        var steps = new List<MicroStep>
        {
            new WalkthroughStep($"{skill.Id}|walk", example.Title, example.Code, example.Notes, example.ExpectedOutput),
        };

        if (includeCloze && !string.IsNullOrWhiteSpace(example.ExpectedOutput) &&
            Cloze(skill.Id, "example", example.Code, example.ExpectedOutput) is { } cloze)
        {
            steps.Add(cloze);
        }

        if (Parsons(skill.Id, example.Code, example.ExpectedOutput) is { } parsons)
        {
            steps.Add(parsons);
        }

        steps.Add(Mistake(skill.Id, lesson.CommonMistake));
        return steps;
    }

    /// <summary>¿Cuántos huecos generó la explicación? (Si ninguno, el ejemplo resuelto aporta uno.)</summary>
    public static bool HasCloze(IEnumerable<MicroStep> steps) => steps.OfType<ClozeStep>().Any();

    public static bool IsFragment(CodeBlock block) => block.Caption?.StartsWith("Fragmento", StringComparison.Ordinal) == true;

    /// <summary>Agrupa la explicación en tarjetas: cada una termina tras un bloque de código o una nota.</summary>
    public static IReadOnlyList<IReadOnlyList<ContentBlock>> Chunk(IReadOnlyList<ContentBlock> blocks)
    {
        var cards = new List<IReadOnlyList<ContentBlock>>();
        var current = new List<ContentBlock>();
        var textLength = 0;
        foreach (var block in blocks)
        {
            if (block is Paragraph p)
            {
                // Un párrafo nuevo abre tarjeta si la actual ya tiene código/nota o demasiado texto.
                if (current.Count > 0 && (current[^1] is not Paragraph || textLength + p.Text.Length > 320))
                {
                    cards.Add(current);
                    current = [];
                    textLength = 0;
                }

                current.Add(p);
                textLength += p.Text.Length;
                continue;
            }

            current.Add(block);
        }

        if (current.Count > 0)
        {
            cards.Add(current);
        }

        return cards;
    }

    public ClozeStep? Cloze(string skillId, string source, string code, string output)
    {
        foreach (var candidate in ClozeGenerator.Candidates(code, skillId + source))
        {
            var key = $"{skillId}|cloze|{source}|{candidate.Start}";
            if (_exclusions.Contains(key))
            {
                continue;
            }

            var options = candidate.Distractors.Append(candidate.Answer)
                .OrderBy(o => ClozeGenerator.StableHash(key + o)).ToList();
            var prompt = $"Completa el código para que muestre:\n{output.TrimEnd()}";
            var explanation = $"La pieza correcta es «{candidate.Answer}». " + ClozeHint(candidate.Group);
            return new ClozeStep(key, prompt, code, candidate.Start, candidate.Answer, options, explanation);
        }

        return null;
    }

    private static string ClozeHint(string group) => group switch
    {
        "comparación" => "Fíjate en si el límite se incluye o no: es la fuente de los errores por uno.",
        "número" => "Revisa los límites: un número de más o de menos cambia el resultado.",
        "consola" => "WriteLine termina la línea; Write no; ReadLine lee una línea.",
        "tipo" => "El tipo debe poder guardar el valor que se le asigna.",
        _ => "Las otras opciones no compilan o cambian lo que hace el programa (lo comprobamos ejecutándolas).",
    };

    public ParsonsStep? Parsons(string skillId, string code, string? output)
    {
        // Sin la salida esperada, varios órdenes podrían parecer correctos: no se genera el ejercicio.
        if (string.IsNullOrWhiteSpace(output))
        {
            return null;
        }

        var raw = code.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');
        var candidates = raw.Select((text, i) => (text, i)).Where(x => IsMovable(x.text)).Select(x => x.i).ToList();
        if (candidates.Count < 3)
        {
            return null;
        }

        // Cadena de dependencias desde el final: cada línea depende de la siguiente (comparte datos, ambas escriben
        // en consola o es estructura de control). Así el orden correcto es el único que tiene sentido.
        // Si la validación encontró otro orden equivalente, se prueba con menos líneas.
        var chain = new List<int> { candidates[^1] };
        for (var k = candidates.Count - 2; k >= 0 && chain.Count < MaxParsonsLines; k--)
        {
            if (!LineDependency.Conflict(raw[candidates[k]], raw[chain[0]]))
            {
                break;
            }

            chain.Insert(0, candidates[k]);
        }

        for (var size = chain.Count; size >= 3; size--)
        {
            var key = $"{skillId}|parsons|{size}";
            if (_exclusions.Contains(key))
            {
                continue;
            }

            var movable = chain.TakeLast(size).ToHashSet();
            var lines = raw.Select((text, i) => new ParsonsLine(text, movable.Contains(i))).ToList();
            var correct = movable.Order().ToList();
            if (correct.Select(i => lines[i].Content).Distinct().Count() < 2)
            {
                continue;
            }

            var bank = Shuffle(correct, key);
            var prompt = $"Ordena las líneas para que el programa muestre:\n{output.TrimEnd()}";
            return new ParsonsStep(key, prompt, lines, bank, output);
        }

        return null;
    }

    private static bool IsMovable(string line)
    {
        var t = line.Trim();
        return t.Length > 0 && t is not ("{" or "}" or "};" or "else") && !t.StartsWith("//", StringComparison.Ordinal);
    }

    /// <summary>Mezcla determinista que nunca deja el orden correcto.</summary>
    private static List<int> Shuffle(IReadOnlyList<int> correct, string key)
    {
        var shuffled = correct.OrderBy(i => ClozeGenerator.StableHash(key + i)).ToList();
        if (shuffled.SequenceEqual(correct))
        {
            shuffled.Reverse();
        }

        return shuffled;
    }

    public MicroStep Mistake(string skillId, CommonMistake mistake)
    {
        var wrong = mistake.WrongCode.Replace("\r\n", "\n").Split('\n');
        var bugLines = DifferingLines(wrong, mistake.FixedCode.Replace("\r\n", "\n").Split('\n'));
        var contentLines = wrong.Count(l => l.Trim().Length > 0 && !l.Trim().StartsWith("//", StringComparison.Ordinal));
        if (bugLines.Count is >= 1 and <= 2 && contentLines >= 2)
        {
            return new SpotBugStep($"{skillId}|bug", mistake.Description, mistake.WrongCode, bugLines.Select(i => i + 1).ToList(), mistake.WhyItFails, mistake.FixedCode);
        }

        var key = $"{skillId}|compare";
        var aIsCorrect = ClozeGenerator.StableHash(key) % 2 == 0;
        return new CompareCodeStep(key, mistake.Description,
            aIsCorrect ? mistake.FixedCode : mistake.WrongCode,
            aIsCorrect ? mistake.WrongCode : mistake.FixedCode,
            aIsCorrect, mistake.WhyItFails);
    }

    /// <summary>Líneas del código con error que no están en la versión corregida (por subsecuencia común más larga).</summary>
    public static IReadOnlyList<int> DifferingLines(IReadOnlyList<string> wrong, IReadOnlyList<string> fixedLines)
    {
        static string Norm(string s) => string.Concat(s.Where(c => !char.IsWhiteSpace(c)));
        bool Ignored(string s)
        {
            var t = s.Trim();
            return t.Length == 0 || t.StartsWith("//", StringComparison.Ordinal);
        }

        var a = wrong.Select(Norm).ToArray();
        var b = fixedLines.Select(Norm).ToArray();
        var lcs = new int[a.Length + 1, b.Length + 1];
        for (var i = a.Length - 1; i >= 0; i--)
        {
            for (var j = b.Length - 1; j >= 0; j--)
            {
                lcs[i, j] = a[i] == b[j] ? lcs[i + 1, j + 1] + 1 : Math.Max(lcs[i + 1, j], lcs[i, j + 1]);
            }
        }

        var result = new List<int>();
        int x = 0, y = 0;
        while (x < a.Length)
        {
            if (y < b.Length && a[x] == b[y])
            {
                x++;
                y++;
            }
            else if (y < b.Length && lcs[x, y + 1] >= lcs[x + 1, y])
            {
                y++;
            }
            else
            {
                if (!Ignored(wrong[x]))
                {
                    result.Add(x);
                }

                x++;
            }
        }

        return result;
    }
}
