using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content.Nivel1;

internal static class Nivel1Module
{
    public static CurriculumModule Build(ContentRegistry r) => new("n1", 1, "Lógica de programación",
        "Decisiones, repetición y resolución de problemas paso a paso.",
        [
            U01Decisiones.Build(r),
            U02Repeticion.Build(r),
            U03Problemas.Build(r),
        ]);
}
