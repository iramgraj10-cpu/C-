using CSharpMastery.Content;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Domain.Sessions;
using CSharpMastery.Learning.Services;
using CSharpMastery.Learning.Sessions;

namespace CSharpMastery.Tests.Learning;

public sealed class SessionTests
{
    private static readonly DateTimeOffset T0 = new(2026, 2, 1, 10, 0, 0, TimeSpan.Zero);
    private readonly SessionPlanner _planner = new(CurriculumContent.Catalog);

    [Theory]
    [InlineData(25)]
    [InlineData(45)]
    [InlineData(75)]
    [InlineData(90)]
    public void La_plantilla_respeta_la_duracion(int minutes)
    {
        var total = SessionPlanner.BlockTemplate(minutes).Sum(b => b.Minutes);
        Assert.InRange(total, minutes - 1.5, minutes + 1.5);
    }

    [Fact]
    public void La_plantilla_de_45_minutos_es_la_documentada()
    {
        var blocks = SessionPlanner.BlockTemplate(45).Select(b => b.Minutes).ToArray();
        Assert.Equal([5, 8, 7, 15, 7, 3], blocks);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(91)]
    [InlineData(120)]
    public void No_se_permiten_sesiones_fuera_de_rango(int minutes) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => SessionPlanner.BlockTemplate(minutes));

    [Fact]
    public void La_primera_sesion_ensena_el_primer_concepto_con_retirada_de_ayuda()
    {
        var plan = _planner.Plan(45, LearnerSnapshot.Empty, T0, seed: 1);

        Assert.Equal("n0.programa", plan.FocusSkillId);
        Assert.DoesNotContain(plan.Activities, a => a.Block == SessionBlockKind.Recall);
        Assert.Equal(ActivityKind.ReadLesson, plan.Activities[0].Kind);
        Assert.Equal(ActivityKind.StudyWorkedExample, plan.Activities[1].Kind);

        var items = plan.Activities.Where(a => a.Kind == ActivityKind.Item).Select(a => CurriculumContent.Catalog.GetItem(a.RefId)).ToList();
        var guided = items.FindIndex(i => i.Purpose == ItemPurpose.Guided);
        var independent = items.FindIndex(i => i.Purpose == ItemPurpose.Independent);
        var transfer = items.FindIndex(i => i.Purpose == ItemPurpose.Transfer);
        Assert.True(guided >= 0 && independent > guided && transfer > independent, "El orden debe ser guiado → independiente → transferencia.");
        Assert.Equal(ActivityKind.Summary, plan.Activities[^1].Kind);
        var itemIds = plan.Activities.Where(a => a.Kind == ActivityKind.Item).Select(a => a.RefId).ToList();
        Assert.Equal(itemIds.Count, itemIds.Distinct().Count());
    }

    [Fact]
    public void Los_repasos_vencidos_van_primero()
    {
        var skills = new Dictionary<string, SkillProgress>
        {
            ["n0.programa"] = SkillProgress.Initial("n0.programa") with
            {
                Level = MasteryLevel.ProvisionallyMastered,
                Review = ReviewSchedule.New with { DueAt = T0.AddHours(-2), Repetitions = 1, IntervalDays = 1 },
            },
        };
        var plan = _planner.Plan(25, new LearnerSnapshot(skills, []), T0, seed: 3);

        var first = plan.Activities[0];
        Assert.Equal(SessionBlockKind.Recall, first.Block);
        Assert.Equal("n0.programa", CurriculumContent.Catalog.GetItem(first.RefId).SkillId);
        Assert.True(first.AskConfidence);
        Assert.Equal("n0.sintaxis", plan.FocusSkillId);
    }

    [Fact]
    public void La_practica_intercala_temas_ya_practicados()
    {
        var skills = new[] { "n0.programa", "n0.sintaxis", "n0.salida" }
            .ToDictionary(id => id, id => SkillProgress.Initial(id) with { Level = MasteryLevel.ProvisionallyMastered, Review = ReviewSchedule.New with { DueAt = T0.AddDays(5) } });
        var plan = _planner.Plan(75, new LearnerSnapshot(skills, []), T0, seed: 5);

        Assert.Equal("n0.variables", plan.FocusSkillId);
        var practiceSkills = plan.Activities
            .Where(a => a.Block == SessionBlockKind.Practice && a.Kind == ActivityKind.Item)
            .Select(a => CurriculumContent.Catalog.GetItem(a.RefId).SkillId)
            .ToHashSet();
        Assert.Contains("n0.variables", practiceSkills);
        Assert.True(practiceSkills.Count > 1, "Debe haber práctica intercalada con temas anteriores.");
    }

    [Fact]
    public void Los_errores_recurrentes_generan_practica_de_refuerzo()
    {
        var skills = new[] { "n0.programa", "n0.sintaxis", "n0.salida", "n0.variables" }
            .ToDictionary(id => id, id => SkillProgress.Initial(id) with { Level = MasteryLevel.Practiced });
        var attempts = Enumerable.Range(0, 3)
            .Select(i => Attempts.Make("n0.sintaxis", "x" + i, false, T0.AddHours(-i), error: ErrorCategory.Syntax))
            .ToList();
        var plan = _planner.Plan(75, new LearnerSnapshot(skills, attempts), T0, seed: 7);
        Assert.Contains(plan.Activities, a => a.Reason.Contains("error frecuente"));
    }

    [Fact]
    public void Una_sesion_se_puede_pausar_y_retomar_contando_solo_el_tiempo_activo()
    {
        var plan = _planner.Plan(25, LearnerSnapshot.Empty, T0, 1);
        var s = SessionTracker.Start(plan, T0);
        s = SessionTracker.CompleteActivity(s, new ActivityOutcome(0, null, null), T0.AddMinutes(5));
        s = SessionTracker.Pause(s, T0.AddMinutes(8));
        Assert.Equal(SessionStatus.Paused, s.Status);
        Assert.Equal(TimeSpan.FromMinutes(8), s.ActiveTime);

        // Dos horas en pausa no cuentan.
        s = SessionTracker.Resume(s, T0.AddHours(2));
        s = SessionTracker.CompleteActivity(s, new ActivityOutcome(1, true, null), T0.AddHours(2).AddMinutes(4));
        Assert.Equal(TimeSpan.FromMinutes(12), s.ActiveTime);
        Assert.Equal(2, s.CurrentIndex);
    }

    [Fact]
    public void La_inactividad_larga_no_se_cuenta_como_practica()
    {
        var s = SessionTracker.Start(_planner.Plan(25, LearnerSnapshot.Empty, T0, 1), T0);
        s = SessionTracker.CompleteActivity(s, new ActivityOutcome(0, null, null), T0.AddHours(3));
        Assert.Equal(SessionTracker.IdleCap, s.ActiveTime);
    }

    [Fact]
    public void Terminar_todas_las_actividades_completa_la_sesion_y_terminar_antes_va_al_resumen()
    {
        var plan = _planner.Plan(25, LearnerSnapshot.Empty, T0, 1);
        var s = SessionTracker.Start(plan, T0);
        var early = SessionTracker.FinishEarly(s, T0.AddMinutes(3));
        Assert.Equal(ActivityKind.Summary, early.Current!.Kind);

        for (var i = 0; i < plan.Activities.Count; i++)
        {
            s = SessionTracker.CompleteActivity(s, new ActivityOutcome(i, true, null), T0.AddMinutes(i + 1));
        }

        Assert.Equal(SessionStatus.Completed, s.Status);
        Assert.NotNull(s.CompletedAt);
        Assert.Throws<InvalidOperationException>(() => SessionTracker.CompleteActivity(s, new ActivityOutcome(0, true, null), T0));
    }

    [Fact]
    public void Se_detecta_el_limite_de_90_minutos()
    {
        var s = SessionTracker.Start(_planner.Plan(90, LearnerSnapshot.Empty, T0, 1), T0);
        for (var i = 1; i <= 5; i++)
        {
            s = SessionTracker.SaveDraft(s, "x", T0.AddMinutes(19 * i));
        }

        Assert.True(SessionTracker.HardLimitReached(s, T0.AddMinutes(95)));
        Assert.True(SessionTracker.PlannedTimeReached(s, T0.AddMinutes(95)));
    }
}
