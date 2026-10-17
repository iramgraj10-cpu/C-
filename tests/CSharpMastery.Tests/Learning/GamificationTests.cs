using CSharpMastery.Content;
using CSharpMastery.Learning.Services;

namespace CSharpMastery.Tests.Learning;

public sealed class GamificationTests : IDisposable
{
    private readonly TempDatabase _db = new();
    private readonly FakeClock _clock = FakeClock.At2026();

    public void Dispose() => _db.Dispose();

    private GamificationService New() => new(_db.Open(), _clock);

    [Fact]
    public async Task Sin_actividad_no_hay_racha_ni_xp()
    {
        var state = await New().GetStateAsync();
        Assert.Equal(0, state.TotalXp);
        Assert.Equal(0, state.Streak);
        Assert.False(state.PracticedToday);
        Assert.Equal(GamificationService.DefaultGoal, state.DailyGoal);
        Assert.Equal(7, state.LastWeek.Count);
    }

    [Fact]
    public async Task La_racha_cuenta_dias_seguidos_y_se_mantiene_hasta_terminar_el_dia()
    {
        var g = New();
        await g.AwardAsync(10);
        _clock.Advance(TimeSpan.FromDays(1));
        await g.AwardAsync(10);
        _clock.Advance(TimeSpan.FromDays(1));
        var state = await g.AwardAsync(30);
        Assert.Equal(3, state.Streak);
        Assert.Equal(50, state.TotalXp);
        Assert.Equal(30, state.TodayXp);

        // Al día siguiente, sin practicar todavía, la racha sigue (termina ayer).
        _clock.Advance(TimeSpan.FromDays(1));
        state = await g.GetStateAsync();
        Assert.Equal(3, state.Streak);
        Assert.False(state.PracticedToday);

        // Un día completo sin practicar la rompe.
        _clock.Advance(TimeSpan.FromDays(1));
        Assert.Equal(0, (await g.GetStateAsync()).Streak);
    }

    [Fact]
    public async Task Los_intentos_previos_tambien_cuentan_para_la_racha()
    {
        var store = _db.Open();
        await store.AddAttemptAsync(Attempts.Make("n0.programa", "n0.programa.pred1", true, _clock.Now.AddDays(-1), code: false));
        var state = await new GamificationService(store, _clock).AwardAsync(5);
        Assert.Equal(2, state.Streak);
    }

    [Fact]
    public async Task La_meta_diaria_se_guarda_y_se_valida()
    {
        var g = New();
        await g.SetDailyGoalAsync(20);
        var state = await g.AwardAsync(25);
        Assert.Equal(20, state.DailyGoal);
        Assert.True(state.GoalReached);
        Assert.Equal(1, state.GoalProgress);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => g.SetDailyGoalAsync(0));
    }

    [Fact]
    public async Task El_xp_viaja_en_la_exportacion()
    {
        var store = _db.Open();
        await new GamificationService(store, _clock).AwardAsync(40);
        var export = await store.ExportAsync(_clock.Now);
        using var other = new TempDatabase();
        var target = other.Open();
        await target.ImportAsync(export);
        Assert.Equal(40, (await new GamificationService(target, _clock).GetStateAsync()).TotalXp);
    }
}
