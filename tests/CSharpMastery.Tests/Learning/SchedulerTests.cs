using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Scheduling;

namespace CSharpMastery.Tests.Learning;

public sealed class SchedulerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);
    private readonly Sm2Scheduler _scheduler = new();

    [Fact]
    public void Los_intervalos_crecen_con_respuestas_correctas()
    {
        var s = _scheduler.Apply(ReviewSchedule.New, 5, Now);
        Assert.Equal(1, s.IntervalDays);
        Assert.Equal(Now.AddDays(1), s.DueAt);

        s = _scheduler.Apply(s, 5, Now.AddDays(1));
        Assert.Equal(6, s.IntervalDays);

        var ease = s.EaseFactor;
        s = _scheduler.Apply(s, 5, Now.AddDays(7));
        Assert.Equal((int)Math.Round(6 * ease), s.IntervalDays);
        Assert.Equal(3, s.Repetitions);
    }

    [Fact]
    public void Olvidar_reinicia_el_intervalo_y_reduce_la_facilidad()
    {
        var s = _scheduler.Apply(ReviewSchedule.New, 5, Now);
        s = _scheduler.Apply(s, 5, Now.AddDays(1));
        s = _scheduler.Apply(s, 4, Now.AddDays(7));
        var before = s;

        s = _scheduler.Apply(s, 1, Now.AddDays(30));

        Assert.Equal(0, s.Repetitions);
        Assert.Equal(1, s.IntervalDays);
        Assert.Equal(before.Lapses + 1, s.Lapses);
        Assert.True(s.EaseFactor < before.EaseFactor);
        Assert.Equal(Now.AddDays(31), s.DueAt);
    }

    [Fact]
    public void La_facilidad_tiene_un_minimo_y_el_intervalo_un_maximo()
    {
        var s = ReviewSchedule.New;
        for (var i = 0; i < 20; i++)
        {
            s = _scheduler.Apply(s, 0, Now);
        }

        Assert.Equal(_scheduler.Options.MinEase, s.EaseFactor, 3);

        s = ReviewSchedule.New;
        var t = Now;
        for (var i = 0; i < 30; i++)
        {
            s = _scheduler.Apply(s, 5, t);
            t = s.DueAt!.Value;
        }

        Assert.Equal(_scheduler.Options.MaxIntervalDays, s.IntervalDays);
    }

    [Fact]
    public void Calidad_3_aprueba_pero_baja_la_facilidad()
    {
        var s = _scheduler.Apply(ReviewSchedule.New, 3, Now);
        Assert.Equal(1, s.Repetitions);
        Assert.True(s.EaseFactor < ReviewSchedule.DefaultEase);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(6)]
    public void Calidad_fuera_de_rango_se_rechaza(int quality) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => _scheduler.Apply(ReviewSchedule.New, quality, Now));

    [Fact]
    public void El_primer_repaso_se_programa_una_sola_vez()
    {
        var first = _scheduler.ScheduleFirst(ReviewSchedule.New, Now);
        var again = _scheduler.ScheduleFirst(first, Now.AddDays(5));
        Assert.Equal(Now.AddDays(1), again.DueAt);
    }

    [Fact]
    public void La_calidad_refleja_ayuda_y_seguridad_pero_no_premia_la_rapidez()
    {
        var baseAttempt = Attempts.Make("s", "i", true, Now);
        Assert.Equal(5, ReviewQuality.FromAttempt(baseAttempt, 2));
        Assert.Equal(5, ReviewQuality.FromAttempt(baseAttempt with { Duration = TimeSpan.FromSeconds(5) }, 2));
        Assert.Equal(4, ReviewQuality.FromAttempt(baseAttempt with { Confidence = 1 }, 2));
        Assert.Equal(4, ReviewQuality.FromAttempt(baseAttempt with { Duration = TimeSpan.FromMinutes(20) }, 2));
        Assert.Equal(3, ReviewQuality.FromAttempt(baseAttempt with { HintsUsed = 1 }, 2));
        Assert.Equal(2, ReviewQuality.FromAttempt(baseAttempt with { Correct = false }, 2));
        Assert.Equal(1, ReviewQuality.FromAttempt(baseAttempt with { Correct = false, HintsUsed = 2 }, 2));
        Assert.Equal(0, ReviewQuality.FromAttempt(baseAttempt with { Correct = false, SolutionViewed = true }, 2));
    }
}
