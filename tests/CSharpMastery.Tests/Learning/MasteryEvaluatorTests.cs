using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Mastery;

namespace CSharpMastery.Tests.Learning;

public sealed class MasteryEvaluatorTests
{
    private const string Skill = "n0.variables";
    private static readonly DateTimeOffset T0 = new(2026, 2, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly SkillEvidenceProfile Full = new(HasCodeItems: true, HasTransferItems: true);
    private readonly MasteryEvaluator _evaluator = new(MasteryCriteria.Default);

    private static List<AttemptRecord> StrongEvidence(DateTimeOffset start) =>
    [
        Attempts.Make(Skill, "a", true, start, ItemPurpose.Prediction, code: false),
        Attempts.Make(Skill, "b", true, start.AddMinutes(1), ItemPurpose.Concept, code: false),
        Attempts.Make(Skill, "c", true, start.AddMinutes(2), ItemPurpose.Guided, hints: 1),
        Attempts.Make(Skill, "d", true, start.AddMinutes(3), ItemPurpose.Independent),
        Attempts.Make(Skill, "e", true, start.AddMinutes(4), ItemPurpose.Transfer),
        Attempts.Make(Skill, "f", true, start.AddMinutes(5), ItemPurpose.BugFinding, code: false),
    ];

    [Fact]
    public void Leer_la_leccion_no_es_dominio()
    {
        var progress = SkillProgress.Initial(Skill) with { Level = MasteryLevel.Seen, SeenAt = T0 };
        var eval = _evaluator.Evaluate(progress, [], Full, T0);
        Assert.Equal(MasteryLevel.Seen, eval.Level);
        Assert.NotEmpty(eval.Pending);
    }

    [Fact]
    public void Evidencia_completa_da_dominio_provisional()
    {
        var eval = _evaluator.Evaluate(SkillProgress.Initial(Skill), StrongEvidence(T0), Full, T0.AddMinutes(10));
        Assert.Equal(MasteryLevel.ProvisionallyMastered, eval.Level);
        Assert.Equal(1.0, eval.WindowAccuracy);
    }

    [Fact]
    public void Sin_codigo_independiente_no_hay_dominio_aunque_la_precision_sea_alta()
    {
        var attempts = StrongEvidence(T0).Select(a => a.Purpose is ItemPurpose.Independent or ItemPurpose.Transfer ? a with { HintsUsed = 2 } : a).ToList();
        var eval = _evaluator.Evaluate(SkillProgress.Initial(Skill), attempts, Full, T0.AddMinutes(10));
        Assert.Equal(MasteryLevel.Practiced, eval.Level);
        Assert.Contains(eval.Pending, p => p.Contains("sin pistas"));
    }

    [Fact]
    public void Repetir_el_mismo_item_no_cuenta_como_variedad()
    {
        var attempts = Enumerable.Range(0, 8)
            .Select(i => Attempts.Make(Skill, "mismo", true, T0.AddMinutes(i), ItemPurpose.Transfer))
            .ToList();
        var eval = _evaluator.Evaluate(SkillProgress.Initial(Skill), attempts, Full, T0.AddHours(1));
        Assert.Equal(MasteryLevel.Practiced, eval.Level);
        Assert.Equal(1, eval.DistinctItemsCorrect);
    }

    [Fact]
    public void Precision_por_debajo_del_umbral_impide_el_dominio()
    {
        var attempts = StrongEvidence(T0);
        attempts.Add(Attempts.Make(Skill, "g", false, T0.AddMinutes(6)));
        attempts.Add(Attempts.Make(Skill, "h", false, T0.AddMinutes(7)));
        var eval = _evaluator.Evaluate(SkillProgress.Initial(Skill), attempts, Full, T0.AddMinutes(10));
        Assert.True(eval.WindowAccuracy < 0.85);
        Assert.Equal(MasteryLevel.Practiced, eval.Level);
    }

    [Fact]
    public void Las_autoevaluaciones_no_cuentan_como_evidencia()
    {
        var attempts = StrongEvidence(T0).Select(a => a with { AutoGraded = false }).ToList();
        var eval = _evaluator.Evaluate(SkillProgress.Initial(Skill), attempts, Full, T0.AddMinutes(10));
        Assert.Equal(MasteryLevel.NotStarted, eval.Level);
    }

    [Fact]
    public void La_retencion_exige_un_repaso_correcto_tras_el_intervalo()
    {
        var provisional = SkillProgress.Initial(Skill) with { Level = MasteryLevel.ProvisionallyMastered, ProvisionalMasteryAt = T0 };
        var attempts = StrongEvidence(T0.AddMinutes(-30));

        var tooSoon = attempts.Append(Attempts.Make(Skill, "r1", true, T0.AddDays(1), ItemPurpose.Recall, code: false, context: AttemptContext.Review)).ToList();
        Assert.Equal(MasteryLevel.ProvisionallyMastered, _evaluator.Evaluate(provisional, tooSoon, Full, T0.AddDays(1)).Level);

        var later = attempts.Append(Attempts.Make(Skill, "r1", true, T0.AddDays(4), ItemPurpose.Recall, code: false, context: AttemptContext.Review)).ToList();
        Assert.Equal(MasteryLevel.Retained, _evaluator.Evaluate(provisional, later, Full, T0.AddDays(4)).Level);

        var notInReview = attempts.Append(Attempts.Make(Skill, "r1", true, T0.AddDays(4), ItemPurpose.Recall, code: false, context: AttemptContext.Session)).ToList();
        Assert.Equal(MasteryLevel.ProvisionallyMastered, _evaluator.Evaluate(provisional, notInReview, Full, T0.AddDays(4)).Level);
    }

    [Fact]
    public void Tras_olvidar_se_exige_refuerzo_nuevo()
    {
        var demoted = MasteryEvaluator.Demote(SkillProgress.Initial(Skill) with { Level = MasteryLevel.Retained }, T0.AddDays(10));
        Assert.Equal(MasteryLevel.Practiced, demoted.Level);
        Assert.True(demoted.NeedsReinforcement);

        var attempts = StrongEvidence(T0);
        Assert.Equal(MasteryLevel.Practiced, _evaluator.Evaluate(demoted, attempts, Full, T0.AddDays(10)).Level);

        attempts.Add(Attempts.Make(Skill, "x", true, T0.AddDays(11)));
        attempts.Add(Attempts.Make(Skill, "y", true, T0.AddDays(11).AddMinutes(5)));
        var eval = _evaluator.Evaluate(demoted, attempts, Full, T0.AddDays(11));
        Assert.Equal(MasteryLevel.ProvisionallyMastered, eval.Level);

        var applied = _evaluator.Apply(demoted, eval, T0.AddDays(11));
        Assert.False(applied.NeedsReinforcement);
        Assert.Null(applied.ReinforcementSince);
    }

    [Fact]
    public void Los_criterios_se_pueden_configurar_y_validar()
    {
        var lenient = new MasteryEvaluator(MasteryCriteria.Default with { MinAccuracy = 0.5, MinGradedAttempts = 3 });
        var attempts = new List<AttemptRecord>
        {
            Attempts.Make(Skill, "a", true, T0, ItemPurpose.Independent),
            Attempts.Make(Skill, "b", true, T0.AddMinutes(1), ItemPurpose.Transfer),
            Attempts.Make(Skill, "c", true, T0.AddMinutes(2), ItemPurpose.Concept, code: false),
            Attempts.Make(Skill, "d", false, T0.AddMinutes(3), ItemPurpose.Concept, code: false),
        };
        Assert.Equal(MasteryLevel.ProvisionallyMastered, lenient.Evaluate(SkillProgress.Initial(Skill), attempts, Full, T0).Level);
        Assert.Equal(MasteryLevel.Practiced, _evaluator.Evaluate(SkillProgress.Initial(Skill), attempts, Full, T0).Level);

        Assert.NotEmpty((MasteryCriteria.Default with { MinAccuracy = 1.5 }).Validate());
        Assert.NotEmpty((MasteryCriteria.Default with { AccuracyWindow = 2 }).Validate());
        Assert.Empty(MasteryCriteria.Default.Validate());
    }
}
