using CSharpMastery.Domain.Progress;

namespace CSharpMastery.Learning.Adaptation;

public sealed record DifficultyDecision(int Difficulty, int Change, string Reason);

/// <summary>
/// Ajusta la dificultad (1-5) de una habilidad según precisión, ayuda usada y tiempo en una ventana de intentos.
/// No sube solo por completar una lección ni baja por un único error aislado.
/// </summary>
public sealed class DifficultyAdapter
{
    public int Window { get; init; } = 5;
    public int MinAttemptsToRaise { get; init; } = 4;
    public int MinAttemptsToLower { get; init; } = 3;
    public double RaiseThreshold { get; init; } = 0.85;
    public double LowerThreshold { get; init; } = 0.45;

    /// <summary>Desempeño de un intento entre 0 y 1. La rapidez nunca suma puntos.</summary>
    public static double Performance(AttemptRecord attempt, double expectedMinutes)
    {
        if (!attempt.Correct || attempt.SolutionViewed)
        {
            return 0;
        }

        var score = 1.0 - Math.Min(0.6, 0.2 * attempt.HintsUsed);
        if (expectedMinutes > 0 && attempt.Duration.TotalMinutes > expectedMinutes * 3)
        {
            score -= 0.1;
        }

        return Math.Max(0, score);
    }

    /// <param name="recentAttempts">Intentos de la habilidad, cualquier orden.</param>
    /// <param name="expectedMinutes">Función que da los minutos previstos de cada ítem.</param>
    public DifficultyDecision Decide(int current, IReadOnlyList<AttemptRecord> recentAttempts, Func<string, double> expectedMinutes, DateTimeOffset? lastChangeAt = null)
    {
        var window = recentAttempts
            .Where(a => a.AutoGraded && (lastChangeAt is null || a.At > lastChangeAt))
            .OrderByDescending(a => a.At)
            .Take(Window)
            .ToList();

        if (window.Count == 0)
        {
            return new DifficultyDecision(current, 0, "Sin intentos suficientes.");
        }

        var mean = window.Average(a => Performance(a, expectedMinutes(a.ItemId)));

        if (window.Count >= MinAttemptsToRaise && mean >= RaiseThreshold && window[0].Correct && current < SkillProgress.MaxDifficulty)
        {
            return new DifficultyDecision(current + 1, +1, $"Desempeño sostenido de {mean:P0} en {window.Count} intentos.");
        }

        if (window.Count >= MinAttemptsToLower && mean <= LowerThreshold && current > SkillProgress.MinDifficulty)
        {
            return new DifficultyDecision(current - 1, -1, $"Desempeño de {mean:P0} en {window.Count} intentos: se reduce la dificultad para consolidar.");
        }

        return new DifficultyDecision(current, 0, $"Desempeño de {mean:P0}: se mantiene la dificultad.");
    }
}
