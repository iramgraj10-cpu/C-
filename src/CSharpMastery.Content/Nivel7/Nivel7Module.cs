using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content.Nivel7;

internal static class Nivel7Module
{
    public static CurriculumModule Build(ContentRegistry r) => new("n7", 7, "Proyectos integradores",
        "Proyectos con requisitos, criterios de aceptación, hitos con pruebas y rúbrica que integran lo aprendido. Se abren desde la sección Proyectos.",
        []);
}
