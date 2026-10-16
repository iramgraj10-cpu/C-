using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content.Nivel4;

internal static class Nivel4Module
{
    public static CurriculumModule Build(ContentRegistry r) => new("n4", 4, "C# moderno y profesional",
        "Excepciones, genéricos, delegados, LINQ, patrones modernos, recursos y serialización.",
        [
            U01ErroresGenericos.Build(r),
            U02Funciones.Build(r),
            new Unit("n4.u3", "n4", "Idiomas modernos", "Patrones, extensiones, recursos, archivos, JSON, inmutabilidad y organización.",
            [
                Outline.Skill("n4.patrones", "n4.u3", "Pattern matching y expresiones modernas", "is, switch con patrones, propiedades y listas.", Tier.Intermediate, "n1.switch", "n3.records"),
                Outline.Skill("n4.extension", "n4.u3", "Métodos de extensión", "Agregar operaciones a tipos existentes.", Tier.Intermediate, "n3.static"),
                Outline.Skill("n4.idisposable", "n4.u3", "IDisposable y using", "Liberar recursos de forma determinista.", Tier.Intermediate, "n4.excepciones"),
                Outline.Skill("n4.archivos-json", "n4.u3", "Archivos, directorios y JSON", "Leer y escribir archivos; serializar con System.Text.Json.", Tier.Essential, "n4.idisposable"),
                Outline.Skill("n4.inmutabilidad", "n4.u3", "Colecciones inmutables y mutabilidad", "Cuándo conviene no modificar datos.", Tier.Advanced, "n3.records"),
                Outline.Skill("n4.organizacion", "n4.u3", "Namespaces, proyectos y assemblies", "Organizar una solución.", Tier.Intermediate, "n3.encapsulacion"),
            ]),
        ]);
}
