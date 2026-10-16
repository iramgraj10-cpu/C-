using CSharpMastery.Domain.Progress;

namespace CSharpMastery.Learning.Analytics;

public enum CalibrationFlag
{
    Calibrated,
    /// <summary>Cree dominarlo, pero falla: prioridad de repaso.</summary>
    Overconfident,
    /// <summary>Acierta, pero duda: conviene reforzar la confianza con práctica independiente.</summary>
    Underconfident,
    InsufficientData,
}

public sealed record SkillCalibration(string SkillId, int Samples, double MeanConfidence, double Accuracy, CalibrationFlag Flag);

/// <summary>Compara la confianza declarada (1-4) con el resultado real.</summary>
public sealed class CalibrationAnalyzer
{
    public int MinSamples { get; init; } = 3;

    public static double Normalize(int confidence) => (Math.Clamp(confidence, 1, 4) - 1) / 3.0;

    public IReadOnlyList<SkillCalibration> Analyze(IEnumerable<AttemptRecord> attempts) =>
        attempts
            .Where(a => a.Confidence is not null && a.AutoGraded)
            .GroupBy(a => a.SkillId)
            .Select(g =>
            {
                var n = g.Count();
                var conf = g.Average(a => Normalize(a.Confidence!.Value));
                var acc = g.Count(a => a.Correct) / (double)n;
                var flag = n < MinSamples ? CalibrationFlag.InsufficientData
                    : conf >= 0.66 && acc < 0.6 ? CalibrationFlag.Overconfident
                    : conf <= 0.34 && acc >= 0.85 ? CalibrationFlag.Underconfident
                    : CalibrationFlag.Calibrated;
                return new SkillCalibration(g.Key, n, conf, acc, flag);
            })
            .OrderBy(c => c.Flag == CalibrationFlag.Overconfident ? 0 : 1)
            .ThenBy(c => c.SkillId)
            .ToList();

    /// <summary>Retroalimentación inmediata para un intento con confianza declarada.</summary>
    public static string? Feedback(int? confidence, bool correct) => confidence switch
    {
        null => null,
        >= 3 when !correct => "Estabas seguro, pero la respuesta no era correcta. Es una señal valiosa: este concepto se repasará antes.",
        <= 2 when correct => "Acertaste aunque dudabas. Repasarlo pronto ayudará a consolidarlo.",
        _ => null,
    };
}
