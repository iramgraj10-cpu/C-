using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content;

/// <summary>Punto de entrada del banco de contenidos: construye el currículo completo en memoria.</summary>
public static class CurriculumContent
{
    private static readonly Lazy<CurriculumCatalog> Cached = new(Create);

    public static CurriculumCatalog Catalog => Cached.Value;

    /// <summary>Claves de pasos interactivos generados que no deben usarse (ver <see cref="InteractiveExclusionList"/>).</summary>
    public static IReadOnlySet<string> InteractiveExclusions => InteractiveExclusionList.Keys;

    /// <summary>Salida real de un bloque de código de la explicación (ver <see cref="CodeOutputs"/>).</summary>
    public static string? OutputOf(string code) => CodeOutputs.For(code);

    public static CurriculumCatalog Create()
    {
        var registry = new ContentRegistry();
        var modules = new List<CurriculumModule>
        {
            Nivel0.Nivel0Module.Build(registry),
            Nivel1.Nivel1Module.Build(registry),
            Nivel2.Nivel2Module.Build(registry),
            Nivel3.Nivel3Module.Build(registry),
            Nivel4.Nivel4Module.Build(registry),
            Nivel5.Nivel5Module.Build(registry),
            Nivel6.Nivel6Module.Build(registry),
            Nivel7.Nivel7Module.Build(registry),
        };

        var projects = Nivel7.Projects.Build(registry);
        return new CurriculumCatalog(modules, registry.Items, projects);
    }
}
