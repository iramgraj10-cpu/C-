using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Adaptation;
using CSharpMastery.Learning.Analytics;
using CSharpMastery.Learning.Answers;

namespace CSharpMastery.Tests.Learning;

public sealed class AdaptationAndAnalyticsTests
{
    private static readonly DateTimeOffset T0 = new(2026, 2, 1, 10, 0, 0, TimeSpan.Zero);
    private readonly DifficultyAdapter _adapter = new();

    private static List<AttemptRecord> Series(params bool[] results) =>
        results.Select((ok, i) => Attempts.Make("s", "i" + i, ok, T0.AddMinutes(i))).ToList();

    [Fact]
    public void Un_error_aislado_no_baja_la_dificultad()
    {
        var decision = _adapter.Decide(3, Series(true, true, false), _ => 2);
        Assert.Equal(3, decision.Difficulty);
    }

    [Fact]
    public void Un_solo_error_tampoco_baja_si_es_el_unico_intento()
    {
        Assert.Equal(3, _adapter.Decide(3, Series(false), _ => 2).Difficulty);
    }

    [Fact]
    public void Desempeno_bajo_sostenido_baja_la_dificultad()
    {
        Assert.Equal(2, _adapter.Decide(3, Series(false, false, true, false), _ => 2).Difficulty);
    }

    [Fact]
    public void Desempeno_alto_sostenido_sube_la_dificultad()
    {
        Assert.Equal(4, _adapter.Decide(3, Series(true, true, true, true), _ => 2).Difficulty);
    }

    [Fact]
    public void Acertar_con_muchas_pistas_no_sube_la_dificultad()
    {
        var withHints = Series(true, true, true, true, true).Select(a => a with { HintsUsed = 2 }).ToList();
        Assert.Equal(3, _adapter.Decide(3, withHints, _ => 2).Difficulty);
    }

    [Fact]
    public void Solo_cuentan_los_intentos_posteriores_al_ultimo_cambio()
    {
        var attempts = Series(true, true, true, true);
        Assert.Equal(3, _adapter.Decide(3, attempts, _ => 2, lastChangeAt: T0.AddMinutes(2)).Difficulty);
    }

    [Fact]
    public void Los_limites_de_dificultad_se_respetan()
    {
        Assert.Equal(5, _adapter.Decide(5, Series(true, true, true, true, true), _ => 2).Difficulty);
        Assert.Equal(1, _adapter.Decide(1, Series(false, false, false, false), _ => 2).Difficulty);
    }

    [Fact]
    public void Detecta_errores_recurrentes_recientes()
    {
        var attempts = new List<AttemptRecord>
        {
            Attempts.Make("a", "1", false, T0, error: ErrorCategory.IndexOutOfRange),
            Attempts.Make("b", "2", false, T0.AddDays(1), error: ErrorCategory.IndexOutOfRange),
            Attempts.Make("a", "3", false, T0.AddDays(2), error: ErrorCategory.IndexOutOfRange),
            Attempts.Make("a", "4", false, T0.AddDays(2), error: ErrorCategory.Syntax),
            Attempts.Make("a", "5", false, T0.AddDays(-30), error: ErrorCategory.Syntax),
            Attempts.Make("a", "6", false, T0.AddDays(-30), error: ErrorCategory.Syntax),
        };
        var recurring = new ErrorPatternAnalyzer().Find(attempts, T0.AddDays(3));
        var single = Assert.Single(recurring);
        Assert.Equal(ErrorCategory.IndexOutOfRange, single.Category);
        Assert.Equal(["a", "b"], single.SkillIds.OrderBy(x => x));
    }

    [Fact]
    public void La_calibracion_detecta_exceso_y_falta_de_confianza()
    {
        var attempts = new List<AttemptRecord>
        {
            Attempts.Make("sobre", "1", false, T0, confidence: 4),
            Attempts.Make("sobre", "2", false, T0, confidence: 4),
            Attempts.Make("sobre", "3", true, T0, confidence: 3),
            Attempts.Make("bajo", "4", true, T0, confidence: 1),
            Attempts.Make("bajo", "5", true, T0, confidence: 1),
            Attempts.Make("bajo", "6", true, T0, confidence: 2),
            Attempts.Make("poco", "7", true, T0, confidence: 4),
        };
        var result = new CalibrationAnalyzer().Analyze(attempts).ToDictionary(c => c.SkillId);
        Assert.Equal(CalibrationFlag.Overconfident, result["sobre"].Flag);
        Assert.Equal(CalibrationFlag.Underconfident, result["bajo"].Flag);
        Assert.Equal(CalibrationFlag.InsufficientData, result["poco"].Flag);
        Assert.NotNull(CalibrationAnalyzer.Feedback(4, false));
        Assert.Null(CalibrationAnalyzer.Feedback(4, true));
    }

    [Fact]
    public void Las_respuestas_cortas_se_comparan_con_tolerancia()
    {
        var item = new ShortAnswerItem
        {
            Id = "x", SkillId = "s", Purpose = ItemPurpose.Recall, Prompt = "p", Explanation = "e",
            AcceptedAnswers = ["Console.WriteLine"], KnownWrongAnswers = new Dictionary<string, string> { ["console.write"] = "casi" },
        };
        Assert.True(AnswerChecker.CheckShortAnswer(item, "  console.writeline ").Correct);
        var wrong = AnswerChecker.CheckShortAnswer(item, "Console.Write");
        Assert.False(wrong.Correct);
        Assert.Equal("casi", wrong.Feedback);

        Assert.True(AnswerChecker.Matches(AnswerMatch.Number, "2.5", "2,50"));
        Assert.True(AnswerChecker.Matches(AnswerMatch.Output, "a\nb", "a  \r\nb\n\n"));
        Assert.False(AnswerChecker.Matches(AnswerMatch.Output, "a\nb", "a b"));
    }

    [Fact]
    public void Las_opciones_dan_retroalimentacion_especifica()
    {
        var item = new ChoiceItem
        {
            Id = "x", SkillId = "s", Purpose = ItemPurpose.Prediction, Prompt = "p", Explanation = "e",
            Options = [new("1", true, "bien"), new("2", false, "mal")],
        };
        Assert.True(AnswerChecker.CheckChoice(item, 0).Correct);
        var wrong = AnswerChecker.CheckChoice(item, 1);
        Assert.Equal("mal", wrong.Feedback);
        Assert.Equal(ErrorCategory.PredictionError, wrong.Error);
    }
}
