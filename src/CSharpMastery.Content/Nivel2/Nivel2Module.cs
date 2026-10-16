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
            U03Texto.Build(r),
            U04Recursion.Build(r),
        ]);
}
