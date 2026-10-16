using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content.Nivel1;

internal static class Nivel1Module
{
    public static CurriculumModule Build(ContentRegistry r) => new("n1", 1, "Lógica de programación",
        "Decisiones, repetición y resolución de problemas paso a paso.",
        [
            new Unit("n1.u1", "n1", "Decisiones", "Booleanos, if/else, switch y cortocircuito.",
            [
                Outline.Skill("n1.booleanos", "n1.u1", "Booleanos, expresiones y cortocircuito", "Evaluación de condiciones compuestas y evaluación en cortocircuito de && y ||.", Tier.Essential, "n0.relacionales"),
                Outline.Skill("n1.if-else", "n1.u1", "if, else if y else", "Tomar decisiones y cubrir todos los casos.", Tier.Essential, "n1.booleanos"),
                Outline.Skill("n1.switch", "n1.u1", "switch y expresiones switch", "Elegir entre muchos casos discretos.", Tier.Essential, "n1.if-else"),
            ]),
            new Unit("n1.u2", "n1", "Repetición", "Bucles, contadores y acumuladores.",
            [
                Outline.Skill("n1.while", "n1.u2", "Bucles while y do-while", "Repetir mientras se cumpla una condición.", Tier.Essential, "n1.if-else"),
                Outline.Skill("n1.for", "n1.u2", "Bucle for", "Repetir un número conocido de veces.", Tier.Essential, "n1.while"),
                Outline.Skill("n1.break-continue", "n1.u2", "break y continue", "Salir de un bucle o saltar una iteración.", Tier.Essential, "n1.for"),
                Outline.Skill("n1.acumuladores", "n1.u2", "Contadores y acumuladores", "Contar, sumar, encontrar máximos y mínimos.", Tier.Essential, "n1.for"),
            ]),
            new Unit("n1.u3", "n1", "Resolver problemas", "Validación, descomposición y costo de los algoritmos.",
            [
                Outline.Skill("n1.validacion", "n1.u3", "Validación de datos", "Rechazar entradas inválidas y volver a pedirlas.", Tier.Essential, "n1.while", "n0.conversion"),
                Outline.Skill("n1.descomposicion", "n1.u3", "Resolución de problemas paso a paso", "Entender, planear, ejecutar y verificar.", Tier.Essential, "n1.acumuladores"),
                Outline.Skill("n1.complejidad", "n1.u3", "Análisis intuitivo de complejidad", "Cuántas operaciones hace un algoritmo según el tamaño de la entrada.", Tier.Intermediate, "n1.for"),
            ]),
        ]);
}
