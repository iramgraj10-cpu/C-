using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content.Nivel0;

internal static class Nivel0Module
{
    public static CurriculumModule Build(ContentRegistry r) => new("n0", 0, "Fundamentos de programación",
        "Qué es un programa, cómo se compila y se ejecuta, variables, tipos, operadores, entrada, salida y lectura de errores.",
        [
            U01PrimerPrograma.Build(r),
            U02Datos.Build(r),
            U03EntradaErrores.Build(r),
        ]);
}
