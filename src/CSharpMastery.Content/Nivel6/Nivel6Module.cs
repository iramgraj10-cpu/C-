using CSharpMastery.Content.Infrastructure;
using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content.Nivel6;

internal static class Nivel6Module
{
    public static CurriculumModule Build(ContentRegistry r) => new("n6", 6, "Programación asíncrona y sistemas",
        "Task, async/await, cancelación, concurrencia, HTTP, JSON, SQLite y seguridad.",
        [
            new Unit("n6.u1", "n6", "Asincronía", "Task, async/await, cancelación y concurrencia.",
            [
                Outline.Skill("n6.tasks", "n6.u1", "Task y Task<T>", "Representar trabajo que termina en el futuro.", Tier.Essential, "n4.lambdas"),
                Outline.Skill("n6.async-await", "n6.u1", "async y await", "Esperar sin bloquear.", Tier.Essential, "n6.tasks"),
                Outline.Skill("n6.cancelacion", "n6.u1", "CancellationToken", "Cancelar operaciones de forma cooperativa.", Tier.Intermediate, "n6.async-await"),
                Outline.Skill("n6.excepciones-async", "n6.u1", "Excepciones en código asíncrono", "Dónde aparecen y cómo capturarlas.", Tier.Intermediate, "n6.async-await", "n4.excepciones"),
                Outline.Skill("n6.concurrencia", "n6.u1", "Concurrencia frente a paralelismo", "Task.WhenAll, Parallel y cuándo usar cada uno.", Tier.Advanced, "n6.async-await"),
                Outline.Skill("n6.hilos", "n6.u1", "Introducción a multithreading", "Hilos, condiciones de carrera y lock.", Tier.Advanced, "n6.concurrencia"),
            ]),
            new Unit("n6.u2", "n6", "Datos y red", "HTTP, JSON, SQLite y validación.",
            [
                Outline.Skill("n6.http", "n6.u2", "HTTP y consumo de APIs", "HttpClient, códigos de estado y errores de red.", Tier.Essential, "n6.async-await"),
                Outline.Skill("n6.json", "n6.u2", "JSON y modelos de datos", "Mapear respuestas a records.", Tier.Essential, "n4.archivos-json"),
                Outline.Skill("n6.sqlite", "n6.u2", "Persistencia con SQLite", "Consultas parametrizadas y migraciones.", Tier.Intermediate, "n6.async-await"),
                Outline.Skill("n6.seguridad", "n6.u2", "Fundamentos de seguridad y validación", "Nunca confiar en la entrada; inyección SQL y secretos.", Tier.Essential, "n6.sqlite"),
            ]),
        ]);
}
