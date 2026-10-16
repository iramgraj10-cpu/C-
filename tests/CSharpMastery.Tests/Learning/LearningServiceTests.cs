using CSharpMastery.Content;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Domain.Sessions;
using CSharpMastery.Learning.Services;

namespace CSharpMastery.Tests.Learning;

/// <summary>Flujos completos del servicio de aprendizaje con una base SQLite real.</summary>
public sealed class LearningServiceTests : IDisposable
{
    private readonly TempDatabase _db = new();
    private readonly FakeClock _clock = FakeClock.At2026();

    private LearningService NewService() => new(CurriculumContent.Catalog, _db.Open(), _clock);

    public void Dispose() => _db.Dispose();

    private async Task Solve(LearningService service, string itemId, bool correct = true, AttemptContext context = AttemptContext.Session, int hints = 0)
    {
        var item = CurriculumContent.Catalog.GetItem(itemId);
        await service.RecordAttemptAsync(Attempts.Make(item.SkillId, itemId, correct, _clock.Now, item.Purpose, item.IsCode, hints: hints, context: context));
        _clock.Advance(TimeSpan.FromMinutes(3));
    }

    [Fact]
    public async Task Ruta_completa_de_visto_a_retenido_y_olvido()
    {
        var service = NewService();
        const string skill = "n0.variables";

        await service.MarkLessonSeenAsync(skill);
        Assert.Equal(MasteryLevel.Seen, (await service.Store.GetSkillAsync(skill)).Level);

        foreach (var id in new[] { "n0.variables.pred1", "n0.variables.concepto1", "n0.variables.guiado", "n0.variables.indep", "n0.variables.app", "n0.variables.bug1" })
        {
            await Solve(service, id);
        }

        var provisional = await service.Store.GetSkillAsync(skill);
        Assert.Equal(MasteryLevel.ProvisionallyMastered, provisional.Level);
        Assert.NotNull(provisional.Review.DueAt);

        // Repaso tras 4 días: se confirma la retención.
        _clock.Advance(TimeSpan.FromDays(4));
        Assert.Contains(await service.GetDueReviewSkillsAsync(), s => s.Id == skill);
        await Solve(service, "n0.variables.recall1", context: AttemptContext.Review);
        var retained = await service.Store.GetSkillAsync(skill);
        Assert.Equal(MasteryLevel.Retained, retained.Level);
        Assert.True(retained.Review.DueAt > _clock.Now);

        // Olvidar en un repaso posterior: vuelve a práctica con refuerzo y repaso al día siguiente.
        _clock.Now = retained.Review.DueAt!.Value.AddHours(1);
        await Solve(service, "n0.variables.recall2", correct: false, context: AttemptContext.Review);
        var forgotten = await service.Store.GetSkillAsync(skill);
        Assert.Equal(MasteryLevel.Practiced, forgotten.Level);
        Assert.True(forgotten.NeedsReinforcement);
        Assert.Equal(1, forgotten.Review.IntervalDays);
    }

    [Fact]
    public async Task El_progreso_persiste_al_cerrar_y_reabrir_la_aplicacion()
    {
        var first = NewService();
        await first.MarkLessonSeenAsync("n0.programa");
        await Solve(first, "n0.programa.indep");
        var session = await first.StartSessionAsync(45, seed: 1);
        session = await first.CompleteActivityAsync(session, new ActivityOutcome(0, null, null));
        session = await first.SaveSessionDraftAsync(session, "Console.WriteLine(\"borrador\");");
        await first.PauseSessionAsync(session);

        // «Reinicio»: nueva instancia del almacén sobre el mismo archivo.
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        var second = NewService();
        Assert.Equal(MasteryLevel.Practiced, (await second.Store.GetSkillAsync("n0.programa")).Level);
        Assert.Single(await second.Store.GetAttemptsAsync("n0.programa"));

        var restored = await second.GetUnfinishedSessionAsync();
        Assert.NotNull(restored);
        Assert.Equal(session.Id, restored!.Id);
        Assert.Equal(SessionStatus.Paused, restored.Status);
        Assert.Equal(1, restored.CurrentIndex);
        Assert.Equal("Console.WriteLine(\"borrador\");", restored.DraftAnswer);
        Assert.Equal(session.Plan.Activities.Count, restored.Plan.Activities.Count);

        var resumed = await second.ResumeSessionAsync(restored);
        Assert.Equal(SessionStatus.Active, resumed.Status);
    }

    [Fact]
    public async Task Iniciar_una_sesion_nueva_abandona_la_anterior_sin_perder_intentos()
    {
        var service = NewService();
        var s1 = await service.StartSessionAsync(25, seed: 2);
        await Solve(service, "n0.programa.pred1");
        var s2 = await service.StartSessionAsync(25, seed: 3);
        var sessions = await service.Store.GetSessionsAsync();
        Assert.Equal(SessionStatus.Abandoned, sessions.Single(s => s.Id == s1.Id).Status);
        Assert.Equal(s2.Id, (await service.GetUnfinishedSessionAsync())!.Id);
        Assert.Single(await service.Store.GetAttemptsAsync());
    }

    [Fact]
    public async Task Fallar_una_evaluacion_degrada_la_habilidad_y_programa_repaso_inmediato()
    {
        var service = NewService();
        foreach (var id in new[] { "n0.programa.pred1", "n0.programa.concepto1", "n0.programa.guiado", "n0.programa.indep", "n0.programa.app", "n0.programa.bug1" })
        {
            await Solve(service, id);
        }

        Assert.Equal(MasteryLevel.ProvisionallyMastered, (await service.Store.GetSkillAsync("n0.programa")).Level);

        var assessment = await service.StartUnitAssessmentAsync("n0.u1", seed: 5);
        foreach (var id in assessment.ItemIds)
        {
            var item = CurriculumContent.Catalog.GetItem(id);
            await service.RecordAttemptAsync(Attempts.Make(item.SkillId, id, item.SkillId != "n0.programa", _clock.Now, item.Purpose, item.IsCode,
                context: AttemptContext.Assessment, contextId: assessment.Id.ToString()));
        }

        var result = await service.CompleteAssessmentAsync(assessment);
        Assert.Contains("n0.programa", result.WeakSkillIds);
        var after = await service.Store.GetSkillAsync("n0.programa");
        Assert.Equal(MasteryLevel.Practiced, after.Level);
        Assert.True(after.NeedsReinforcement);
        Assert.True(after.Review.IsDue(_clock.Now));

        var stored = await service.Store.GetAssessmentAsync(assessment.Id);
        Assert.NotNull(stored!.Result);
        Assert.Equal(result.WeakSkillIds, stored.Result!.WeakSkillIds);

        // Reiniciar la evaluación no borra el progreso general.
        await service.ResetAssessmentAsync(assessment);
        Assert.Equal(MasteryLevel.Practiced, (await service.Store.GetSkillAsync("n0.programa")).Level);
        Assert.NotEmpty(await service.Store.GetAttemptsAsync("n0.programa"));
    }

    [Fact]
    public async Task El_reporte_distingue_contenido_visto_practicado_y_dominado()
    {
        var service = NewService();
        await service.MarkLessonSeenAsync("n0.programa");
        await service.MarkLessonSeenAsync("n0.sintaxis");
        await Solve(service, "n0.sintaxis.indep", hints: 1);

        var report = await service.GetReportAsync();
        Assert.Equal(2, report.LessonsSeen);
        Assert.Equal(1, report.ExercisesCompleted);
        Assert.Equal(1, report.SkillsPracticedOnly);
        Assert.Equal(0, report.SkillsProvisional);
        Assert.Equal(0, report.SkillsRetained);
        Assert.True(report.SkillsPendingDemonstration >= 10);
        Assert.Equal(1.0, report.HintsPerAttempt);
        Assert.Equal(0.0, report.IndependentRate);
    }

    [Fact]
    public async Task Los_criterios_de_dominio_se_guardan_y_validan()
    {
        var service = NewService();
        await service.SaveCriteriaAsync(MasteryCriteria.Default with { MinAccuracy = 0.9 });
        Assert.Equal(0.9, (await service.GetCriteriaAsync()).MinAccuracy);
        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveCriteriaAsync(MasteryCriteria.Default with { MinAccuracy = 2 }));
    }

    [Fact]
    public async Task La_cola_de_repaso_toma_un_item_por_habilidad_vencida()
    {
        var service = NewService();
        await Solve(service, "n0.programa.indep");
        await Solve(service, "n0.sintaxis.indep");
        _clock.Advance(TimeSpan.FromDays(2));
        var queue = await service.BuildReviewQueueAsync();
        Assert.Equal(2, queue.Count);
        Assert.Equal(["n0.programa", "n0.sintaxis"], queue.Select(i => i.SkillId).Order());
        Assert.DoesNotContain(queue, i => i.Id is "n0.programa.indep" or "n0.sintaxis.indep");
    }
}
