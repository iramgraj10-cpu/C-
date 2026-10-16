using CSharpMastery.Content;
using CSharpMastery.Domain.Assessments;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Assessments;
using CSharpMastery.Learning.Services;

namespace CSharpMastery.Tests.Learning;

public sealed class AssessmentTests
{
    private static readonly DateTimeOffset T0 = new(2026, 2, 1, 10, 0, 0, TimeSpan.Zero);
    private readonly AssessmentBuilder _builder = new(CurriculumContent.Catalog);

    [Fact]
    public void La_evaluacion_de_unidad_cubre_todos_los_tipos_de_evidencia()
    {
        var a = _builder.BuildUnit("n0.u2", LearnerSnapshot.Empty, 42, T0);
        var items = a.ItemIds.Select(CurriculumContent.Catalog.GetItem).ToList();
        foreach (var skill in CurriculumContent.Catalog.GetUnit("n0.u2").Skills)
        {
            var own = items.Where(i => i.SkillId == skill.Id).ToList();
            Assert.Contains(own, i => i.Purpose is ItemPurpose.Concept or ItemPurpose.Recall);
            Assert.Contains(own, i => i.Purpose == ItemPurpose.Prediction);
            Assert.Contains(own, i => i.Purpose == ItemPurpose.BugFinding);
            Assert.Contains(own, i => i.Purpose == ItemPurpose.Independent && i.IsCode);
        }

        Assert.Contains(items, i => i.Purpose == ItemPurpose.Transfer);
    }

    [Fact]
    public void Cada_intento_de_evaluacion_puede_tener_items_distintos()
    {
        var sets = Enumerable.Range(0, 8)
            .Select(seed => string.Join(",", _builder.BuildUnit("n0.u1", LearnerSnapshot.Empty, seed, T0).ItemIds.Order()))
            .Distinct()
            .Count();
        Assert.True(sets > 1, "Distintas semillas deben producir combinaciones distintas.");
    }

    [Fact]
    public void La_calificacion_detecta_habilidades_debiles_y_crea_ruta_de_recuperacion()
    {
        var assessment = _builder.BuildUnit("n0.u2", LearnerSnapshot.Empty, 7, T0);
        var attempts = assessment.ItemIds.Select(id =>
        {
            var item = CurriculumContent.Catalog.GetItem(id);
            var correct = item.SkillId != "n0.aritmetica";
            return Attempts.Make(item.SkillId, id, correct, T0, item.Purpose, item.IsCode,
                error: ErrorCategory.OperatorMisuse, context: AttemptContext.Assessment, contextId: assessment.Id.ToString());
        }).ToList();

        var result = new AssessmentScorer(CurriculumContent.Catalog, MasteryCriteria.Default).Score(assessment, attempts, LearnerSnapshot.Empty);

        Assert.False(result.Passed);
        Assert.Equal(["n0.aritmetica"], result.WeakSkillIds);
        Assert.Contains(result.Recovery.Steps, s => s.SkillId == "n0.aritmetica" && s.ItemIds.Count > 0);
        Assert.DoesNotContain(result.Recovery.Steps.SelectMany(s => s.ItemIds), assessment.ItemIds.Contains);
    }

    [Fact]
    public void Todo_correcto_aprueba_y_cuenta_solo_el_primer_intento()
    {
        var assessment = _builder.BuildUnit("n0.u1", LearnerSnapshot.Empty, 9, T0);
        var attempts = new List<CSharpMastery.Domain.Progress.AttemptRecord>();
        foreach (var id in assessment.ItemIds)
        {
            var item = CurriculumContent.Catalog.GetItem(id);
            attempts.Add(Attempts.Make(item.SkillId, id, true, T0, item.Purpose, item.IsCode, context: AttemptContext.Assessment, contextId: assessment.Id.ToString()));
        }

        var scorer = new AssessmentScorer(CurriculumContent.Catalog, MasteryCriteria.Default);
        Assert.True(scorer.Score(assessment, attempts, LearnerSnapshot.Empty).Passed);

        var firstWrong = attempts.ToList();
        var target = attempts.First(a => a.IsCode && a.Purpose == ItemPurpose.Independent);
        firstWrong.Insert(0, target with { Id = Guid.NewGuid(), Correct = false, At = T0.AddMinutes(-1) });
        var result = scorer.Score(assessment, firstWrong, LearnerSnapshot.Empty);
        Assert.False(result.Passed);
        Assert.Contains(target.SkillId, result.WeakSkillIds);
    }

    [Fact]
    public void Resolver_el_codigo_con_pistas_no_demuestra_independencia()
    {
        var assessment = _builder.BuildUnit("n0.u1", LearnerSnapshot.Empty, 11, T0);
        var attempts = assessment.ItemIds.Select(id =>
        {
            var item = CurriculumContent.Catalog.GetItem(id);
            return Attempts.Make(item.SkillId, id, true, T0, item.Purpose, item.IsCode, hints: item.IsCode ? 2 : 0,
                context: AttemptContext.Assessment, contextId: assessment.Id.ToString());
        }).ToList();
        var result = new AssessmentScorer(CurriculumContent.Catalog, MasteryCriteria.Default).Score(assessment, attempts, LearnerSnapshot.Empty);
        Assert.False(result.Passed);
    }

    [Fact]
    public void La_evaluacion_acumulativa_mezcla_varias_habilidades_practicadas()
    {
        var skills = new[] { "n0.programa", "n0.sintaxis", "n0.salida", "n0.variables", "n0.aritmetica" }
            .ToDictionary(id => id, id => SkillProgress.Initial(id) with { Level = MasteryLevel.Practiced });
        var a = _builder.BuildCumulative(new LearnerSnapshot(skills, []), 5, T0);
        var covered = a.ItemIds.Select(id => CurriculumContent.Catalog.GetItem(id).SkillId).Distinct().ToList();
        Assert.Equal(AssessmentKind.Cumulative, a.Kind);
        Assert.True(covered.Count >= 4);
        Assert.All(covered, s => Assert.Contains(s, skills.Keys));
        Assert.Contains(a.ItemIds, id => CurriculumContent.Catalog.GetItem(id).IsCode);
    }

    [Fact]
    public void Sin_practica_previa_no_hay_evaluacion_acumulativa() =>
        Assert.Throws<InvalidOperationException>(() => _builder.BuildCumulative(LearnerSnapshot.Empty, 1, T0));
}
