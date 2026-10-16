using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content.Nivel5;

internal static class Nivel5Module
{
    public static CurriculumModule Build(ContentRegistry r) => new("n5", 5, "Herramientas profesionales",
        "El ecosistema .NET, depuración, pruebas, control de versiones y calidad del código.",
        [
            new Unit("n5.u1", "n5", "Ecosistema .NET", "SDK, CLI, NuGet y depuración.",
            [
                Outline.Skill("n5.cli", "n5.u1", ".NET SDK y CLI", "dotnet new, build, run, test y publish.", Tier.Essential, "n2.metodos"),
                Outline.Skill("n5.nuget", "n5.u1", "NuGet", "Agregar y actualizar dependencias con criterio.", Tier.Intermediate, "n5.cli"),
                Outline.Skill("n5.depuracion", "n5.u1", "Depuración con puntos de interrupción", "Inspeccionar variables y la pila de llamadas.", Tier.Essential, "n2.metodos"),
            ]),
            new Unit("n5.u2", "n5", "Calidad", "Pruebas, refactorización, Git, documentación, rendimiento y arquitectura.",
            [
                Outline.Skill("n5.pruebas", "n5.u2", "Pruebas unitarias y diseño de casos", "xUnit, casos límite y particiones de equivalencia.", Tier.Essential, "n3.clases"),
                Outline.Skill("n5.refactorizacion", "n5.u2", "Refactorización", "Mejorar el diseño sin cambiar el comportamiento.", Tier.Intermediate, "n5.pruebas"),
                Outline.Skill("n5.git", "n5.u2", "Git y control de versiones", "Commits, ramas y fusiones.", Tier.Essential),
                Outline.Skill("n5.documentacion", "n5.u2", "Leer documentación técnica", "Aprovechar la documentación oficial de .NET.", Tier.Essential),
                Outline.Skill("n5.rendimiento", "n5.u2", "Complejidad y rendimiento básico", "Medir antes de optimizar.", Tier.Intermediate, "n1.complejidad"),
                Outline.Skill("n5.arquitectura", "n5.u2", "SOLID y separación de responsabilidades", "Principios básicos de arquitectura.", Tier.Advanced, "n3.solid"),
            ]),
        ]);
}
