using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content.Nivel3;

internal static class Nivel3Module
{
    public static CurriculumModule Build(ContentRegistry r) => new("n3", 3, "Programación orientada a objetos",
        "Modelar con clases y objetos, y saber cuándo usar (y cuándo evitar) cada mecanismo.",
        [
            U01Clases.Build(r),
            U02Relaciones.Build(r),
            U03Diseno.Build(r),
        ]);
}
