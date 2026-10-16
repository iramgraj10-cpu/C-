using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content.Nivel4;

internal static class Nivel4Module
{
    public static CurriculumModule Build(ContentRegistry r) => new("n4", 4, "C# moderno y profesional",
        "Excepciones, genéricos, delegados, LINQ, patrones modernos, recursos y serialización.",
        [
            U01ErroresGenericos.Build(r),
            new Unit("n4.u2", "n4", "Funciones como datos", "Delegados, eventos, lambdas, LINQ e iteradores.",
            [
                Outline.Skill("n4.delegados", "n4.u2", "Delegados y eventos", "Referencias a métodos y notificaciones.", Tier.Intermediate, "n3.clases"),
                Outline.Skill("n4.lambdas", "n4.u2", "Expresiones lambda", "Funciones anónimas, Func y Action.", Tier.Essential, "n4.delegados"),
                Outline.Skill("n4.linq", "n4.u2", "LINQ", "Consultar colecciones con Where, Select, OrderBy, GroupBy...", Tier.Essential, "n4.lambdas", "n2.listas"),
                Outline.Skill("n4.ienumerable-iqueryable", "n4.u2", "IEnumerable<T> frente a IQueryable<T>", "Ejecución diferida en memoria y consultas traducidas.", Tier.Advanced, "n4.linq"),
                Outline.Skill("n4.iteradores", "n4.u2", "Iteradores con yield", "Secuencias perezosas.", Tier.Intermediate, "n4.linq"),
            ]),
            new Unit("n4.u3", "n4", "Idiomas modernos", "Patrones, extensiones, recursos, archivos, JSON, inmutabilidad y organización.",
            [
                Outline.Skill("n4.patrones", "n4.u3", "Pattern matching y expresiones modernas", "is, switch con patrones, propiedades y listas.", Tier.Intermediate, "n1.switch", "n3.records"),
                Outline.Skill("n4.extension", "n4.u3", "Métodos de extensión", "Agregar operaciones a tipos existentes.", Tier.Intermediate, "n3.static"),
                Outline.Skill("n4.idisposable", "n4.u3", "IDisposable y using", "Liberar recursos de forma determinista.", Tier.Intermediate, "n4.excepciones"),
                Outline.Skill("n4.archivos-json", "n4.u3", "Archivos, directorios y JSON", "Leer y escribir archivos; serializar con System.Text.Json.", Tier.Essential, "n4.idisposable"),
                Outline.Skill("n4.inmutabilidad", "n4.u3", "Colecciones inmutables y mutabilidad", "Cuándo conviene no modificar datos.", Tier.Advanced, "n3.records"),
                Outline.Skill("n4.organizacion", "n4.u3", "Namespaces, proyectos y assemblies", "Organizar una solución.", Tier.Intermediate, "n3.encapsulacion"),
            ]),
        ]);
}
