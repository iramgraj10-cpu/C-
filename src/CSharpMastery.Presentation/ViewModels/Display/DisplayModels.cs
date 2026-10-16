using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Presentation.ViewModels.Display;

/// <summary>Bloque de contenido listo para mostrarse (texto, código o recuadro).</summary>
public sealed record ContentBlockDisplay(string Kind, string Text, string? Caption, string? CalloutLabel)
{
    public bool IsParagraph => Kind == "p";

    public bool IsCode => Kind == "code";

    public bool IsCallout => Kind == "callout";

    public static ContentBlockDisplay From(ContentBlock block) => block switch
    {
        Paragraph p => new("p", p.Text, null, null),
        CodeBlock c => new("code", c.Code, c.Caption == "Fragmento" ? null : c.Caption, null),
        Callout c => new("callout", c.Text, null, c.Kind switch
        {
            CalloutKind.Tip => "Consejo",
            CalloutKind.Warning => "Atención",
            CalloutKind.Design => "Diseño",
            _ => "Nota",
        }),
        _ => new("p", block.ToString() ?? "", null, null),
    };
}

public sealed record LineNoteDisplay(int Line, string Note)
{
    public string Label => $"Línea {Line}";
}

/// <summary>Elemento seleccionable genérico (opciones, presets, niveles).</summary>
public sealed record OptionDisplay<T>(T Value, string Label)
{
    public override string ToString() => Label;
}
