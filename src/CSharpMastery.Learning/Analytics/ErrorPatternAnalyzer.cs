using CSharpMastery.Domain.Progress;

namespace CSharpMastery.Learning.Analytics;

public sealed record RecurringError(ErrorCategory Category, int Count, IReadOnlyList<string> SkillIds, DateTimeOffset LastSeen);

/// <summary>Detecta errores recurrentes para asignar práctica dirigida.</summary>
public sealed class ErrorPatternAnalyzer
{
    public int LookbackDays { get; init; } = 14;
    public int MinOccurrences { get; init; } = 3;

    public IReadOnlyList<RecurringError> Find(IEnumerable<AttemptRecord> attempts, DateTimeOffset now)
    {
        var since = now.AddDays(-LookbackDays);
        return attempts
            .Where(a => !a.Correct && a.Error != ErrorCategory.None && a.At >= since)
            .GroupBy(a => a.Error)
            .Where(g => g.Count() >= MinOccurrences)
            .Select(g => new RecurringError(g.Key, g.Count(), g.Select(a => a.SkillId).Distinct().ToList(), g.Max(a => a.At)))
            .OrderByDescending(e => e.Count)
            .ToList();
    }

    /// <summary>Errores más frecuentes en todo el historial (para el perfil).</summary>
    public static IReadOnlyList<(ErrorCategory Category, int Count)> MostFrequent(IEnumerable<AttemptRecord> attempts, int top = 5) =>
        attempts
            .Where(a => !a.Correct && a.Error != ErrorCategory.None)
            .GroupBy(a => a.Error)
            .Select(g => (g.Key, g.Count()))
            .OrderByDescending(x => x.Item2)
            .Take(top)
            .ToList();
}
