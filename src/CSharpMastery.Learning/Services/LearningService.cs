using CSharpMastery.Domain.Assessments;
using CSharpMastery.Domain.Common;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Domain.Sessions;
using CSharpMastery.Learning.Adaptation;
using CSharpMastery.Learning.Analytics;
using CSharpMastery.Learning.Assessments;
using CSharpMastery.Learning.Mastery;
using CSharpMastery.Learning.Scheduling;
using CSharpMastery.Learning.Sessions;
using System.Text.Json;

namespace CSharpMastery.Learning.Services;

public sealed record AttemptOutcome(
    SkillProgress Before,
    SkillProgress After,
    MasteryEvaluation Evaluation,
    DifficultyDecision Difficulty,
    IReadOnlyList<string> Messages);

public sealed record ProjectStatus(
    IntegrativeProject Project,
    IReadOnlyList<(ProjectMilestone Milestone, bool Passed)> Milestones,
    ProjectProgress Progress)
{
    public bool AllMilestonesPassed => Milestones.All(m => m.Passed);
}

/// <summary>
/// Servicio de aplicación: registra intentos y actualiza repasos, dificultad y dominio; planifica sesiones;
/// administra evaluaciones y proyectos. Toda la lógica pedagógica vive aquí y en los algoritmos que usa.
/// </summary>
public sealed class LearningService(CurriculumCatalog catalog, IProgressStore store, IClock clock)
{
    private const string CriteriaKey = "mastery-criteria";
    private readonly Sm2Scheduler _scheduler = new();
    private readonly DifficultyAdapter _adapter = new();

    public CurriculumCatalog Catalog => catalog;

    public IProgressStore Store => store;

    public IClock Clock => clock;

    /// <summary>XP, racha y meta diaria.</summary>
    public GamificationService Gamification { get; } = new(store, clock);

    public async Task<MasteryCriteria> GetCriteriaAsync()
    {
        var json = await store.GetSettingAsync(CriteriaKey);
        if (json is null)
        {
            return MasteryCriteria.Default;
        }

        try
        {
            return JsonSerializer.Deserialize<MasteryCriteria>(json) ?? MasteryCriteria.Default;
        }
        catch (JsonException)
        {
            return MasteryCriteria.Default;
        }
    }

    public async Task SaveCriteriaAsync(MasteryCriteria criteria)
    {
        var errors = criteria.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors), nameof(criteria));
        }

        await store.SetSettingAsync(CriteriaKey, JsonSerializer.Serialize(criteria));
    }

    public async Task<LearnerSnapshot> GetSnapshotAsync() =>
        new(await store.GetSkillsAsync(), await store.GetAttemptsAsync());

    public SkillEvidenceProfile EvidenceProfile(string skillId)
    {
        var items = catalog.ItemsForSkill(skillId);
        return new SkillEvidenceProfile(items.Any(i => i.IsCode), items.Any(i => i.Purpose == ItemPurpose.Transfer));
    }

    public async Task MarkLessonSeenAsync(string skillId)
    {
        var progress = await store.GetSkillAsync(skillId);
        var now = clock.Now;
        progress = progress with
        {
            SeenAt = progress.SeenAt ?? now,
            Level = progress.Level < MasteryLevel.Seen ? MasteryLevel.Seen : progress.Level,
        };
        await store.SaveSkillAsync(progress);
    }

    public async Task<AttemptOutcome> RecordAttemptAsync(AttemptRecord attempt)
    {
        var now = attempt.At;
        var before = await store.GetSkillAsync(attempt.SkillId);
        await store.AddAttemptAsync(attempt);

        var progress = before with { SeenAt = before.SeenAt ?? now };
        var messages = new List<string>();
        var expected = catalog.FindItem(attempt.ItemId)?.EstimatedMinutes ?? 2;

        if (attempt.AutoGraded)
        {
            if (attempt.Context == AttemptContext.Review || progress.Review.IsDue(now))
            {
                var quality = ReviewQuality.FromAttempt(attempt, expected);
                progress = progress with { Review = _scheduler.Apply(progress.Review, quality, now) };
                messages.Add(quality >= 3
                    ? $"Próximo repaso en {progress.Review.IntervalDays} día(s)."
                    : "Lo volverás a repasar mañana.");
            }
            else if (!progress.Review.IsScheduled)
            {
                progress = progress with { Review = _scheduler.ScheduleFirst(progress.Review, now) };
                messages.Add("Primer repaso programado para mañana.");
            }
            else if (attempt.Context == AttemptContext.Assessment && !attempt.Correct && progress.Review.Repetitions > 0)
            {
                progress = progress with { Review = _scheduler.Apply(progress.Review, 1, now) };
            }

            var forgot = attempt.Context == AttemptContext.Review && !attempt.Correct;
            if (forgot && progress.Level >= MasteryLevel.ProvisionallyMastered)
            {
                progress = MasteryEvaluator.Demote(progress, now);
                messages.Add("No la recordaste en el repaso: vuelve a práctica con refuerzo.");
            }
        }

        var attempts = await store.GetAttemptsAsync(attempt.SkillId);
        var decision = _adapter.Decide(progress.Difficulty, attempts, id => catalog.FindItem(id)?.EstimatedMinutes ?? 2, progress.DifficultyChangedAt);
        if (decision.Change != 0)
        {
            progress = progress with { Difficulty = decision.Difficulty, DifficultyChangedAt = now };
            messages.Add(decision.Change > 0 ? "Subirá la dificultad de los próximos ejercicios." : "Los próximos ejercicios serán un poco más accesibles para consolidar.");
        }

        var evaluator = new MasteryEvaluator(await GetCriteriaAsync());
        var evaluation = evaluator.Evaluate(progress, attempts, EvidenceProfile(attempt.SkillId), now);
        var after = evaluator.Apply(progress, evaluation, now);

        if (after.Level == MasteryLevel.ProvisionallyMastered && before.Level < MasteryLevel.ProvisionallyMastered)
        {
            messages.Add("Dominio provisional alcanzado. Se confirmará cuando la recuerdes en un repaso posterior.");
        }

        if (after.Level == MasteryLevel.Retained && before.Level < MasteryLevel.Retained)
        {
            messages.Add("Retención confirmada: la recordaste tras varios días.");
        }

        if (CalibrationAnalyzer.Feedback(attempt.Confidence, attempt.Correct) is { } note)
        {
            messages.Add(note);
        }

        await store.SaveSkillAsync(after);
        return new AttemptOutcome(before, after, evaluation, decision, messages);
    }

    // ---------- Sesiones ----------

    public async Task<SessionPlan> PlanSessionAsync(int minutes, int? seed = null)
    {
        var snapshot = await GetSnapshotAsync();
        return new SessionPlanner(catalog).Plan(minutes, snapshot, clock.Now, seed ?? Environment.TickCount);
    }

    public async Task<StudySession> StartSessionAsync(int minutes, int? seed = null)
    {
        if (await store.GetUnfinishedSessionAsync() is { } unfinished)
        {
            await store.SaveSessionAsync(SessionTracker.Abandon(unfinished, clock.Now));
        }

        var plan = await PlanSessionAsync(minutes, seed);
        var session = SessionTracker.Start(plan, clock.Now);
        await store.SaveSessionAsync(session);
        return session;
    }

    public Task<StudySession?> GetUnfinishedSessionAsync() => store.GetUnfinishedSessionAsync();

    public async Task<StudySession> SaveSessionAsync(StudySession session)
    {
        await store.SaveSessionAsync(session);
        return session;
    }

    public Task<StudySession> CompleteActivityAsync(StudySession session, ActivityOutcome outcome) =>
        SaveSessionAsync(SessionTracker.CompleteActivity(session, outcome, clock.Now));

    public Task<StudySession> PauseSessionAsync(StudySession session) => SaveSessionAsync(SessionTracker.Pause(session, clock.Now));

    public Task<StudySession> ResumeSessionAsync(StudySession session) => SaveSessionAsync(SessionTracker.Resume(session, clock.Now));

    public Task<StudySession> FinishSessionEarlyAsync(StudySession session) => SaveSessionAsync(SessionTracker.FinishEarly(session, clock.Now));

    public Task<StudySession> SaveSessionDraftAsync(StudySession session, string? draft) => SaveSessionAsync(SessionTracker.SaveDraft(session, draft, clock.Now));

    // ---------- Repasos ----------

    public async Task<IReadOnlyList<Skill>> GetDueReviewSkillsAsync()
    {
        var now = clock.Now;
        var skills = await store.GetSkillsAsync();
        return skills.Values
            .Where(p => p.Review.IsDue(now))
            .OrderBy(p => p.Review.DueAt)
            .Select(p => catalog.FindSkill(p.SkillId))
            .OfType<Skill>()
            .Where(s => s.Status == ContentStatus.Complete)
            .ToList();
    }

    /// <summary>Un ítem de recuperación activa por habilidad vencida, evitando repetir el último visto.</summary>
    public async Task<IReadOnlyList<LearningItem>> BuildReviewQueueAsync(int maxItems = 10)
    {
        var snapshot = await GetSnapshotAsync();
        var selector = new ItemSelector(catalog, snapshot, new Random());
        var used = new HashSet<string>();
        var queue = new List<LearningItem>();
        foreach (var skill in await GetDueReviewSkillsAsync())
        {
            var item = selector.Rank(skill.Id, [ItemPurpose.Recall, ItemPurpose.Concept, ItemPurpose.Prediction, ItemPurpose.BugFinding, ItemPurpose.Independent], used)
                .OrderBy(i => snapshot.LastAttemptAt(i.Id) ?? DateTimeOffset.MinValue)
                .FirstOrDefault();
            if (item is null)
            {
                continue;
            }

            used.Add(item.Id);
            queue.Add(item);
            if (queue.Count >= maxItems)
            {
                break;
            }
        }

        return queue;
    }

    // ---------- Evaluaciones ----------

    public async Task<Assessment> StartUnitAssessmentAsync(string unitId, int? seed = null)
    {
        var snapshot = await GetSnapshotAsync();
        var assessment = new AssessmentBuilder(catalog).BuildUnit(unitId, snapshot, seed ?? Environment.TickCount, clock.Now);
        await store.SaveAssessmentAsync(assessment, null);
        return assessment;
    }

    public async Task<Assessment> StartCumulativeAssessmentAsync(int? seed = null)
    {
        var snapshot = await GetSnapshotAsync();
        var assessment = new AssessmentBuilder(catalog).BuildCumulative(snapshot, seed ?? Environment.TickCount, clock.Now);
        await store.SaveAssessmentAsync(assessment, null);
        return assessment;
    }

    public Task SaveAssessmentProgressAsync(Assessment assessment) => store.SaveAssessmentAsync(assessment, null);

    public async Task<AssessmentResult> CompleteAssessmentAsync(Assessment assessment)
    {
        var now = clock.Now;
        var attempts = await store.GetAttemptsAsync(contextId: assessment.Id.ToString());
        var snapshot = await GetSnapshotAsync();
        var result = new AssessmentScorer(catalog, await GetCriteriaAsync()).Score(assessment, attempts, snapshot);

        foreach (var skillId in result.WeakSkillIds)
        {
            var progress = MasteryEvaluator.Demote(await store.GetSkillAsync(skillId), now);
            progress = progress with { Review = progress.Review with { DueAt = now } };
            await store.SaveSkillAsync(progress);
        }

        await store.SaveAssessmentAsync(assessment with { Status = AssessmentStatus.Completed, CompletedAt = now }, result);
        return result;
    }

    /// <summary>Reinicia una evaluación: queda marcada como reiniciada y no borra el progreso general.</summary>
    public async Task ResetAssessmentAsync(Assessment assessment) =>
        await store.SaveAssessmentAsync(assessment with { Status = AssessmentStatus.Reset, CompletedAt = clock.Now }, null);

    // ---------- Progreso ----------

    public async Task<ProgressReport> GetReportAsync()
    {
        var snapshot = await GetSnapshotAsync();
        var sessions = await store.GetSessionsAsync();
        var practice = sessions.Aggregate(TimeSpan.Zero, (acc, s) => acc + s.ActiveTime);
        var completed = sessions.Count(s => s.Status == SessionStatus.Completed);
        return new ProgressReportBuilder(catalog, new MasteryEvaluator(await GetCriteriaAsync())).Build(snapshot, clock.Now, practice, completed);
    }

    public async Task<LearnerProfile> GetOrCreateProfileAsync()
    {
        if (await store.GetProfileAsync() is { } profile)
        {
            return profile;
        }

        profile = LearnerProfile.CreateDefault(clock.Now);
        await store.SaveProfileAsync(profile);
        return profile;
    }

    // ---------- Proyectos ----------

    public async Task<ProjectStatus> GetProjectStatusAsync(string projectId)
    {
        var project = catalog.FindProject(projectId) ?? throw new KeyNotFoundException(projectId);
        var attempts = await store.GetAttemptsAsync();
        var solved = attempts.Where(a => a.Correct && a.AutoGraded).Select(a => a.ItemId).ToHashSet();
        var progress = (await store.GetProjectsAsync()).FirstOrDefault(p => p.ProjectId == projectId)
            ?? new ProjectProgress(projectId, new Dictionary<string, int>(), "", false, null);
        return new ProjectStatus(project, project.Milestones.Select(m => (m, solved.Contains(m.ExerciseId))).ToList(), progress);
    }

    /// <summary>
    /// Guarda la autoevaluación con la rúbrica (0 insuficiente, 1 adecuado, 2 excelente). El proyecto se completa
    /// solo si todos los hitos pasaron sus pruebas automáticas y ningún criterio de la rúbrica es insuficiente.
    /// </summary>
    public async Task<ProjectStatus> SaveProjectReviewAsync(string projectId, IReadOnlyDictionary<string, int> rubric, string notes)
    {
        var status = await GetProjectStatusAsync(projectId);
        var complete = status.AllMilestonesPassed
                       && status.Project.Rubric.All(c => rubric.TryGetValue(c.Name, out var v) && v >= 1);
        var progress = new ProjectProgress(projectId, rubric, notes, complete,
            complete ? status.Progress.CompletedAt ?? clock.Now : null);
        await store.SaveProjectAsync(progress);
        return status with { Progress = progress };
    }
}
