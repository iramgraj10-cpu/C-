using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content.Nivel3;

internal static class Nivel3Module
{
    public static CurriculumModule Build(ContentRegistry r) => new("n3", 3, "Programación orientada a objetos",
        "Modelar con clases y objetos, y saber cuándo usar (y cuándo evitar) cada mecanismo.",
        [
            new Unit("n3.u1", "n3", "Clases y objetos", "Clases, instancias, miembros, constructores y encapsulación.",
            [
                Outline.Skill("n3.clases", "n3.u1", "Clases e instancias", "Definir un tipo y crear objetos.", Tier.Essential, "n2.parametros"),
                Outline.Skill("n3.propiedades", "n3.u1", "Campos, propiedades y métodos", "Estado y comportamiento de un objeto.", Tier.Essential, "n3.clases"),
                Outline.Skill("n3.constructores", "n3.u1", "Constructores", "Crear objetos en un estado válido.", Tier.Essential, "n3.propiedades"),
                Outline.Skill("n3.encapsulacion", "n3.u1", "Encapsulación y modificadores de acceso", "Proteger invariantes con private, public, protected e internal.", Tier.Essential, "n3.constructores"),
                Outline.Skill("n3.static", "n3.u1", "Miembros estáticos y de instancia", "Qué pertenece al tipo y qué a cada objeto.", Tier.Essential, "n3.propiedades"),
            ]),
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
