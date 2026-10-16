using CSharpMastery.Domain.Progress;

namespace CSharpMastery.Learning.Scheduling;

/// <summary>Parámetros del planificador de repasos.</summary>
public sealed record SchedulerOptions
{
    public double MinEase { get; init; } = 1.3;
    public double MaxEase { get; init; } = 3.0;
    /// <summary>Intervalo tras el primer repaso correcto.</summary>
    public int FirstIntervalDays { get; init; } = 1;
    /// <summary>Intervalo tras el segundo repaso correcto (SM-2 original usa 6).</summary>
    public int SecondIntervalDays { get; init; } = 6;
    /// <summary>Tope de intervalo para no perder de vista un concepto durante meses.</summary>
    public int MaxIntervalDays { get; init; } = 120;
    /// <summary>Penalización de facilidad al olvidar (variación respecto a SM-2, similar a Anki).</summary>
    public double LapseEasePenalty { get; init; } = 0.2;

    public static SchedulerOptions Default { get; } = new();
}

/// <summary>
/// Variante de SM-2 (Wozniak, 1987) aplicada por habilidad. Diferencias documentadas respecto al original:
/// (1) al fallar se resta <see cref="SchedulerOptions.LapseEasePenalty"/> a la facilidad;
/// (2) el intervalo tiene un tope; (3) la calidad 0-5 se deriva del intento (corrección, ayuda, confianza),
/// no de una autoevaluación. No se afirma que sea óptimo: los parámetros son configurables y están cubiertos por pruebas.
/// </summary>
public sealed class Sm2Scheduler(SchedulerOptions? options = null)
{
    private readonly SchedulerOptions _options = options ?? SchedulerOptions.Default;

    public SchedulerOptions Options => _options;

    /// <summary>Programa el primer repaso cuando la habilidad se practica por primera vez.</summary>
    public ReviewSchedule ScheduleFirst(ReviewSchedule current, DateTimeOffset now) =>
        current.IsScheduled ? current : current with { DueAt = now.AddDays(_options.FirstIntervalDays) };

    /// <summary>Aplica un repaso con calidad <paramref name="quality"/> (0-5).</summary>
    public ReviewSchedule Apply(ReviewSchedule current, int quality, DateTimeOffset now)
    {
        if (quality is < 0 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(quality), "La calidad debe estar entre 0 y 5.");
        }

        if (quality < 3)
        {
            var ease = Math.Max(_options.MinEase, current.EaseFactor - _options.LapseEasePenalty);
            return current with
            {
                EaseFactor = ease,
                Repetitions = 0,
                IntervalDays = _options.FirstIntervalDays,
                Lapses = current.Repetitions > 0 || current.IntervalDays > 0 ? current.Lapses + 1 : current.Lapses,
                DueAt = now.AddDays(_options.FirstIntervalDays),
                LastReviewedAt = now,
            };
        }

        var repetitions = current.Repetitions + 1;
        var interval = repetitions switch
        {
            1 => _options.FirstIntervalDays,
            2 => _options.SecondIntervalDays,
            _ => (int)Math.Round(Math.Max(current.IntervalDays, 1) * current.EaseFactor),
        };
        interval = Math.Clamp(interval, 1, _options.MaxIntervalDays);

        var q = quality;
        var newEase = current.EaseFactor + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02));
        newEase = Math.Clamp(newEase, _options.MinEase, _options.MaxEase);

        return current with
        {
            EaseFactor = newEase,
            Repetitions = repetitions,
            IntervalDays = interval,
            DueAt = now.AddDays(interval),
            LastReviewedAt = now,
        };
    }
}

/// <summary>Convierte un intento en una calidad de repaso 0-5.</summary>
public static class ReviewQuality
{
    /// <summary>
    /// 5: correcto, sin ayuda y con seguridad. 4: correcto sin ayuda pero con duda o mucho más lento de lo previsto.
    /// 3: correcto con pistas. 2: incorrecto sin ayuda. 1: incorrecto tras pedir pistas. 0: tuvo que ver la solución.
    /// La rapidez nunca suma: solo una lentitud extrema baja de 5 a 4.
    /// </summary>
    public static int FromAttempt(AttemptRecord attempt, double expectedMinutes)
    {
        if (attempt.SolutionViewed)
        {
            return attempt.Correct ? 2 : 0;
        }

        if (!attempt.Correct)
        {
            return attempt.HintsUsed > 0 ? 1 : 2;
        }

        if (attempt.HintsUsed > 0)
        {
            return 3;
        }

        var hesitant = attempt.Confidence is <= 2;
        var verySlow = expectedMinutes > 0 && attempt.Duration.TotalMinutes > expectedMinutes * 3;
        return hesitant || verySlow ? 4 : 5;
    }
}
