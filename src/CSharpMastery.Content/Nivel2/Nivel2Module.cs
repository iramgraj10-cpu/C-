using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content.Nivel2;

internal static class Nivel2Module
{
    public static CurriculumModule Build(ContentRegistry r) => new("n2", 2, "Métodos y estructuras de datos",
        "Organizar el código en métodos y elegir la estructura de datos adecuada.",
        [
            U01Metodos.Build(r),
            U02Colecciones.Build(r),
            new Unit("n2.u3", "n2", "Texto y tipos simples", "Manipulación de strings, enumeraciones y tuplas.",
            [
                Outline.Skill("n2.strings", "n2.u3", "Strings y manipulación de texto", "Recorrer, buscar, dividir y construir texto.", Tier.Essential, "n2.arrays"),
                Outline.Skill("n2.enums", "n2.u3", "Enumeraciones", "Conjuntos cerrados de valores con nombre.", Tier.Essential, "n1.switch"),
                Outline.Skill("n2.tuplas", "n2.u3", "Tuplas", "Agrupar valores sin crear un tipo.", Tier.Intermediate, "n2.parametros"),
            ]),
            new Unit("n2.u4", "n2", "Recursividad", "Problemas que se definen en términos de sí mismos.",
            [
                Outline.Skill("n2.recursion", "n2.u4", "Recursividad", "Caso base, caso recursivo y pila de llamadas.", Tier.Intermediate, "n2.parametros", "n2.arrays"),
            ]),
        ]);
}
