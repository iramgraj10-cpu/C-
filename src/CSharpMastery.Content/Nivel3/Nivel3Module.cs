using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content.Nivel3;

internal static class Nivel3Module
{
    public static CurriculumModule Build(ContentRegistry r) => new("n3", 3, "Programación orientada a objetos",
        "Modelar con clases y objetos, y saber cuándo usar (y cuándo evitar) cada mecanismo.",
        [
            U01Clases.Build(r),
            new Unit("n3.u2", "n3", "Relaciones entre tipos", "Composición, herencia, polimorfismo, clases abstractas e interfaces.",
            [
                Outline.Skill("n3.composicion", "n3.u2", "Composición", "Construir objetos a partir de otros objetos.", Tier.Essential, "n3.encapsulacion"),
                Outline.Skill("n3.herencia", "n3.u2", "Herencia", "Reutilizar y especializar; cuándo no usarla.", Tier.Intermediate, "n3.composicion"),
                Outline.Skill("n3.polimorfismo", "n3.u2", "Polimorfismo, sobrescritura y ocultamiento", "virtual, override y new.", Tier.Intermediate, "n3.herencia"),
                Outline.Skill("n3.abstractas-interfaces", "n3.u2", "Clases abstractas e interfaces", "Contratos y comportamiento compartido.", Tier.Essential, "n3.polimorfismo"),
            ]),
            new Unit("n3.u3", "n3", "Tipos modernos y diseño", "Records, structs, nulabilidad y SOLID.",
            [
                Outline.Skill("n3.records", "n3.u3", "Records y tipos por valor", "Igualdad por valor, inmutabilidad y struct.", Tier.Intermediate, "n3.encapsulacion"),
                Outline.Skill("n3.nullability", "n3.u3", "Nulabilidad y prevención de errores con null", "Tipos anulables, ?. y ??.", Tier.Essential, "n3.clases"),
                Outline.Skill("n3.solid", "n3.u3", "Principios SOLID con ejemplos", "Diseño mantenible sin dogmas.", Tier.Advanced, "n3.abstractas-interfaces"),
            ]),
        ]);
}
