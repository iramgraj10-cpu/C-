using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Learning.Interactive;

/// <summary>
/// Paso breve de una lección interactiva. Se genera de forma determinista a partir del contenido de la lección;
/// no cuenta como evidencia de dominio (solo da XP), porque la evidencia sale de los ítems verificados del banco.
/// La clave identifica el paso para excluirlo si la validación detecta que es ambiguo.
/// </summary>
public abstract record MicroStep(string Key);

/// <summary>Presentación de la lección: título y objetivo.</summary>
public sealed record IntroStep(string Key, string Title, string Objective, int EstimatedMinutes) : MicroStep(Key);

/// <summary>Tarjeta corta de explicación: un párrafo y, si lo hay, el código o la nota que lo acompaña.</summary>
public sealed record ConceptCardStep(string Key, IReadOnlyList<ContentBlock> Blocks, int Number, int Total) : MicroStep(Key);

/// <summary>Completar un hueco del código eligiendo entre fichas.</summary>
public sealed record ClozeStep(
    string Key,
    string Prompt,
    string Code,
    int BlankStart,
    string Answer,
    IReadOnlyList<string> Options,
    string Explanation) : MicroStep(Key)
{
    public string Before => Code[..BlankStart];

    public string After => Code[(BlankStart + Answer.Length)..];

    /// <summary>El código con la ficha elegida en el hueco (para comprobar la validez de los distractores).</summary>
    public string Fill(string option) => Before + option + After;
}

/// <summary>Una línea de un ejercicio de ordenar: las fijas se muestran en su lugar; las movibles las coloca el estudiante.</summary>
public sealed record ParsonsLine(string Text, bool Movable)
{
    public string Indent => Text[..(Text.Length - Text.TrimStart().Length)];

    public string Content => Text.Trim();
}

/// <summary>Ordenar las líneas de un programa.</summary>
public sealed record ParsonsStep(
    string Key,
    string Prompt,
    IReadOnlyList<ParsonsLine> Lines,
    IReadOnlyList<int> BankOrder,
    string? Output) : MicroStep(Key)
{
    /// <summary>Índices (en <see cref="Lines"/>) de las líneas movibles, en el orden correcto.</summary>
    public IReadOnlyList<int> MovableIndexes => Lines.Select((l, i) => (l, i)).Where(x => x.l.Movable).Select(x => x.i).ToList();

    /// <summary>Programa que resulta de colocar las líneas movibles en el orden indicado (por índice en <see cref="Lines"/>).</summary>
    public string Assemble(IReadOnlyList<int> movableOrder)
    {
        var slots = MovableIndexes;
        var result = Lines.Select(l => l.Text).ToArray();
        for (var k = 0; k < slots.Count; k++)
        {
            var source = Lines[movableOrder[k]];
            result[slots[k]] = Lines[slots[k]].Indent + source.Content;
        }

        return string.Join("\n", result);
    }

    /// <summary>Correcto si el texto de cada hueco coincide con el original (líneas idénticas son intercambiables).</summary>
    public bool IsCorrect(IReadOnlyList<int> movableOrder) =>
        movableOrder.Count == MovableIndexes.Count &&
        MovableIndexes.Select((slot, k) => Lines[slot].Content == Lines[movableOrder[k]].Content).All(ok => ok);
}

/// <summary>Recorrido del ejemplo resuelto: una nota a la vez con su línea resaltada.</summary>
public sealed record WalkthroughStep(string Key, string Title, string Code, IReadOnlyList<LineNote> Notes, string? Output) : MicroStep(Key);

/// <summary>Tocar la línea que contiene el error habitual.</summary>
public sealed record SpotBugStep(
    string Key,
    string Description,
    string WrongCode,
    IReadOnlyList<int> BugLines,
    string Why,
    string FixedCode) : MicroStep(Key)
{
    public IReadOnlyList<string> CodeLines => WrongCode.Replace("\r\n", "\n").Split('\n');
}

/// <summary>Elegir cuál de dos versiones es la correcta (cuando el error no está en una sola línea).</summary>
public sealed record CompareCodeStep(
    string Key,
    string Description,
    string OptionA,
    string OptionB,
    bool AIsCorrect,
    string Why) : MicroStep(Key);
