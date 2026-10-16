using CSharpMastery.Domain.Sessions;

namespace CSharpMastery.Learning.Sessions;

/// <summary>Transiciones puras del estado de una sesión (iniciar, avanzar, pausar, retomar, terminar).</summary>
public static class SessionTracker
{
    /// <summary>Si pasa más tiempo que esto sin actividad, no se cuenta como práctica real.</summary>
    public static TimeSpan IdleCap { get; } = TimeSpan.FromMinutes(20);

    public static TimeSpan HardLimit { get; } = TimeSpan.FromMinutes(StudySession.MaxMinutes);

    public static StudySession Start(SessionPlan plan, DateTimeOffset now) =>
        new(Guid.NewGuid(), plan, SessionStatus.Active, 0, [], TimeSpan.Zero, now, now, null, null);

    public static TimeSpan ActiveTime(StudySession session, DateTimeOffset now) =>
        session.ActiveTime + Elapsed(session, now);

    private static TimeSpan Elapsed(StudySession session, DateTimeOffset now)
    {
        if (session.Status != SessionStatus.Active || now <= session.LastActiveAt)
        {
            return TimeSpan.Zero;
        }

        var span = now - session.LastActiveAt;
        return span > IdleCap ? IdleCap : span;
    }

    public static StudySession CompleteActivity(StudySession session, ActivityOutcome outcome, DateTimeOffset now)
    {
        EnsureActive(session);
        var next = session with
        {
            Outcomes = [.. session.Outcomes.Where(o => o.Index != outcome.Index), outcome],
            CurrentIndex = Math.Max(session.CurrentIndex, outcome.Index + 1),
            ActiveTime = ActiveTime(session, now),
            LastActiveAt = now,
            DraftAnswer = null,
        };

        if (next.CurrentIndex >= next.Plan.Activities.Count)
        {
            next = next with { Status = SessionStatus.Completed, CompletedAt = now };
        }

        return next;
    }

    /// <summary>Guarda el borrador (código o respuesta) de la actividad actual.</summary>
    public static StudySession SaveDraft(StudySession session, string? draft, DateTimeOffset now) =>
        session with { DraftAnswer = draft, ActiveTime = ActiveTime(session, now), LastActiveAt = now };

    public static StudySession Pause(StudySession session, DateTimeOffset now) =>
        session.Status != SessionStatus.Active
            ? session
            : session with { Status = SessionStatus.Paused, ActiveTime = ActiveTime(session, now), LastActiveAt = now };

    public static StudySession Resume(StudySession session, DateTimeOffset now) =>
        session.Status is SessionStatus.Paused or SessionStatus.Active
            ? session with { Status = SessionStatus.Active, LastActiveAt = now }
            : throw new InvalidOperationException("Solo se puede retomar una sesión pausada.");

    /// <summary>Termina antes de tiempo: salta al resumen sin perder lo hecho.</summary>
    public static StudySession FinishEarly(StudySession session, DateTimeOffset now)
    {
        var summaryIndex = session.Plan.Activities.ToList().FindIndex(a => a.Kind == ActivityKind.Summary);
        return session with
        {
            Status = SessionStatus.Active,
            CurrentIndex = summaryIndex >= 0 ? summaryIndex : session.Plan.Activities.Count,
            ActiveTime = ActiveTime(session, now),
            LastActiveAt = now,
        };
    }

    public static StudySession Abandon(StudySession session, DateTimeOffset now) =>
        session with { Status = SessionStatus.Abandoned, ActiveTime = ActiveTime(session, now), CompletedAt = now };

    /// <summary>Se alcanzó la duración planificada: se sugiere cerrar al terminar la actividad actual.</summary>
    public static bool PlannedTimeReached(StudySession session, DateTimeOffset now) =>
        ActiveTime(session, now) >= TimeSpan.FromMinutes(session.Plan.DurationMinutes);

    /// <summary>Se alcanzó el máximo absoluto de 90 minutos: se debe ir al resumen.</summary>
    public static bool HardLimitReached(StudySession session, DateTimeOffset now) => ActiveTime(session, now) >= HardLimit;

    private static void EnsureActive(StudySession session)
    {
        if (session.Status != SessionStatus.Active)
        {
            throw new InvalidOperationException("La sesión no está activa.");
        }
    }
}
