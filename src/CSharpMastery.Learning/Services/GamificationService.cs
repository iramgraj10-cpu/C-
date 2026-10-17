using System.Globalization;
using System.Text.Json;
using CSharpMastery.Domain.Common;

namespace CSharpMastery.Learning.Services;

/// <summary>Estado de motivación: XP, racha de días seguidos y meta diaria.</summary>
public sealed record GamificationState(
    int TotalXp,
    int TodayXp,
    int DailyGoal,
    int Streak,
    bool PracticedToday,
    IReadOnlyList<DayXp> LastWeek)
{
    public bool GoalReached => TodayXp >= DailyGoal;

    public double GoalProgress => DailyGoal <= 0 ? 1 : Math.Min(1, TodayXp / (double)DailyGoal);
}

public sealed record DayXp(DateOnly Day, int Xp);

/// <summary>Cuántos XP da cada cosa. La recompensa es por practicar y acertar, nunca por leer.</summary>
public static class XpRewards
{
    public const int MicroStep = 5;
    public const int Item = 10;
    public const int CodeExercise = 20;
    public const int LessonCompleted = 15;
    public const int PerfectLessonBonus = 10;
}

/// <summary>
/// XP, racha y meta diaria. Se guardan en los ajustes (una entrada por día), así que viajan con la exportación.
/// La racha cuenta días consecutivos con XP o con intentos registrados, terminando hoy o ayer
/// (si hoy aún no practicas, la racha sigue viva hasta medianoche).
/// </summary>
public sealed class GamificationService(IProgressStore store, IClock clock)
{
    public const string XpKey = "xp-por-dia";
    public const string GoalKey = "meta-diaria";
    public const int DefaultGoal = 50;
    public static IReadOnlyList<int> GoalOptions { get; } = [20, 50, 100, 150];

    public DateOnly Today => DateOnly.FromDateTime(clock.Now.ToLocalTime().DateTime);

    public async Task<int> GetDailyGoalAsync()
    {
        var value = await store.GetSettingAsync(GoalKey);
        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var goal) && goal > 0 ? goal : DefaultGoal;
    }

    public Task SetDailyGoalAsync(int goal)
    {
        if (goal <= 0 || goal > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(goal), "La meta diaria debe estar entre 1 y 1000 XP.");
        }

        return store.SetSettingAsync(GoalKey, goal.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>Suma XP al día de hoy y devuelve el nuevo estado.</summary>
    public async Task<GamificationState> AwardAsync(int xp)
    {
        if (xp > 0)
        {
            var days = await LoadAsync();
            var key = Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            days[key] = days.GetValueOrDefault(key) + xp;
            await store.SetSettingAsync(XpKey, JsonSerializer.Serialize(days));
        }

        return await GetStateAsync();
    }

    public async Task<GamificationState> GetStateAsync()
    {
        var days = (await LoadAsync()).ToDictionary(
            kv => DateOnly.ParseExact(kv.Key, "yyyy-MM-dd", CultureInfo.InvariantCulture), kv => kv.Value);
        var active = days.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToHashSet();
        foreach (var attempt in await store.GetAttemptsAsync())
        {
            active.Add(DateOnly.FromDateTime(attempt.At.ToLocalTime().DateTime));
        }

        var today = Today;
        var practicedToday = active.Contains(today);
        var streak = 0;
        for (var day = practicedToday ? today : today.AddDays(-1); active.Contains(day); day = day.AddDays(-1))
        {
            streak++;
        }

        var week = Enumerable.Range(0, 7).Select(i => today.AddDays(i - 6)).Select(d => new DayXp(d, days.GetValueOrDefault(d))).ToList();
        return new GamificationState(days.Values.Sum(), days.GetValueOrDefault(today), await GetDailyGoalAsync(), streak, practicedToday, week);
    }

    private async Task<Dictionary<string, int>> LoadAsync()
    {
        var json = await store.GetSettingAsync(XpKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
