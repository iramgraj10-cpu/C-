using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Domain.Sessions;
using CSharpMastery.Learning.Analytics;
using CSharpMastery.Learning.Services;

namespace CSharpMastery.Learning.Sessions;

/// <summary>
/// Construye el plan de una sesión. La plantilla de 45 minutos (5/8/7/15/7/3) se escala a la duración elegida
/// y se redistribuye según el estado: si no hay repasos, ese tiempo pasa a práctica; si el concepto ya se
/// estudió, la explicación se sustituye por más práctica.
/// </summary>
public sealed class SessionPlanner(CurriculumCatalog catalog)
{
    public static IReadOnlyList<int> PresetMinutes { get; } = [25, 45, 75];

    private static readonly (SessionBlockKind Kind, double Minutes)[] Template45 =
    [
        (SessionBlockKind.Recall, 5),
        (SessionBlockKind.NewConcept, 8),
        (SessionBlockKind.GuidedExample, 7),
        (SessionBlockKind.Practice, 15),
        (SessionBlockKind.Application, 7),
        (SessionBlockKind.WrapUp, 3),
    ];

    private static readonly ItemPurpose[] RecallPurposes = [ItemPurpose.Recall, ItemPurpose.Concept, ItemPurpose.Prediction];
    private static readonly ItemPurpose[] PracticePurposes = [ItemPurpose.Guided, ItemPurpose.Independent, ItemPurpose.BugFinding];
    private static readonly ItemPurpose[] InterleavePurposes = [ItemPurpose.Independent, ItemPurpose.BugFinding, ItemPurpose.Prediction];

    public static void Validate(int minutes)
    {
        if (minutes < StudySession.MinMinutes || minutes > StudySession.MaxMinutes)
        {
            throw new ArgumentOutOfRangeException(nameof(minutes),
                $"La sesión debe durar entre {StudySession.MinMinutes} y {StudySession.MaxMinutes} minutos.");
        }
    }

    public static IReadOnlyList<SessionBlockPlan> BlockTemplate(int minutes)
    {
        Validate(minutes);
        return Template45
            .Select(t => new SessionBlockPlan(t.Kind, Math.Round(t.Minutes * minutes / 45.0 * 2, MidpointRounding.AwayFromZero) / 2))
            .ToList();
    }

    /// <summary>Habilidad en la que se centra la siguiente sesión (la primera de la ruta aún no dominada y con prerrequisitos practicados).</summary>
    public Skill? ChooseFocus(LearnerSnapshot snapshot) =>
        catalog.OrderedSkills.FirstOrDefault(s =>
            s.Status == ContentStatus.Complete &&
            snapshot.Level(s.Id) < MasteryLevel.ProvisionallyMastered &&
            s.Prerequisites.All(p => catalog.FindSkill(p) is not { Status: ContentStatus.Complete } || snapshot.Level(p) >= MasteryLevel.Practiced));

    public SessionPlan Plan(int minutes, LearnerSnapshot snapshot, DateTimeOffset now, int seed)
    {
        var template = BlockTemplate(minutes);
        var budget = template.ToDictionary(b => b.Kind, b => b.Minutes);
        var random = new Random(seed);
        var selector = new ItemSelector(catalog, snapshot, random);
        var used = new HashSet<string>();
        var activities = new List<(SessionBlockKind Block, ActivityKind Kind, string Ref, double Minutes, bool Ask, string Reason)>();
        var focus = ChooseFocus(snapshot);

        // 1. Recuperación activa: repasos vencidos o, si no hay, conceptos practicados hace más tiempo.
        var due = snapshot.Skills.Values
            .Where(p => p.Review.IsDue(now) && catalog.FindSkill(p.SkillId)?.Status == ContentStatus.Complete)
            .OrderBy(p => p.Review.DueAt)
            .Select(p => (p.SkillId, Reason: "Repaso programado"))
            .ToList();
        var recallSkills = due.Count > 0
            ? due
            : snapshot.Skills.Values
                .Where(p => p.Level >= MasteryLevel.Practiced && p.SkillId != focus?.Id)
                .OrderBy(p => snapshot.LastSkillActivity(p.SkillId) ?? DateTimeOffset.MinValue)
                .Select(p => (p.SkillId, Reason: "Recuperación de un concepto anterior"))
                .ToList();

        var recallBudget = budget[SessionBlockKind.Recall];
        foreach (var (skillId, reason) in recallSkills)
        {
            if (recallBudget <= 0)
            {
                break;
            }

            var item = selector.Pick(skillId, RecallPurposes, used);
            if (item is null)
            {
                continue;
            }

            Add(SessionBlockKind.Recall, item, activities.All(a => a.Block != SessionBlockKind.Recall), $"{reason}: {catalog.GetSkill(skillId).Title}");
            recallBudget -= item.EstimatedMinutes;
        }

        if (activities.Count == 0)
        {
            budget[SessionBlockKind.Practice] += budget[SessionBlockKind.Recall];
        }

        // 2 y 3. Concepto nuevo y ejemplo guiado.
        var focusLevel = focus is null ? MasteryLevel.NotStarted : snapshot.Level(focus.Id);
        if (focus?.Lesson is { } lesson && focusLevel < MasteryLevel.Practiced)
        {
            activities.Add((SessionBlockKind.NewConcept, ActivityKind.ReadLesson, focus.Id, budget[SessionBlockKind.NewConcept], false, "Concepto nuevo"));
            activities.Add((SessionBlockKind.GuidedExample, ActivityKind.StudyWorkedExample, focus.Id, budget[SessionBlockKind.GuidedExample] / 2, false, "Ejemplo resuelto línea por línea"));
            if (catalog.FindItem(lesson.PredictionItemId) is { } prediction)
            {
                Add(SessionBlockKind.GuidedExample, prediction, false, "Predice antes de ejecutar");
            }
        }
        else if (focus is not null)
        {
            var guidedBudget = budget[SessionBlockKind.GuidedExample];
            foreach (var item in selector.Rank(focus.Id, [ItemPurpose.Prediction, ItemPurpose.BugFinding], used))
            {
                if (guidedBudget <= 0)
                {
                    break;
                }

                Add(SessionBlockKind.GuidedExample, item, false, "Lectura y predicción de código");
                guidedBudget -= item.EstimatedMinutes;
            }

            budget[SessionBlockKind.Practice] += budget[SessionBlockKind.NewConcept] + Math.Max(0, guidedBudget);
        }
        else
        {
            budget[SessionBlockKind.Practice] += budget[SessionBlockKind.NewConcept] + budget[SessionBlockKind.GuidedExample];
        }

        // 4. Práctica: concepto foco con retirada gradual de ayuda, refuerzo de errores e intercalado.
        var focusQueue = new Queue<LearningItem>();
        if (focus?.Lesson is { } l)
        {
            foreach (var id in new[] { l.GuidedExerciseId, l.IndependentExerciseId })
            {
                if (catalog.FindItem(id) is { } it && !snapshot.SolvedCorrectly(id) && !used.Contains(id))
                {
                    focusQueue.Enqueue(it);
                }
            }
        }

        if (focus is not null)
        {
            foreach (var it in selector.Rank(focus.Id, PracticePurposes, used).Where(i => !focusQueue.Contains(i)).Take(6))
            {
                focusQueue.Enqueue(it);
            }
        }

        var remedialQueue = new Queue<(LearningItem Item, string Reason)>();
        foreach (var error in new ErrorPatternAnalyzer().Find(snapshot.Attempts, now).Take(2))
        {
            foreach (var it in selector.TargetingError(error.Category, used).Take(2))
            {
                remedialQueue.Enqueue((it, $"Refuerzo de un error frecuente: {ErrorCategoryInfo.DisplayName(error.Category)}"));
            }
        }

        var interleaveQueue = new Queue<LearningItem>();
        foreach (var p in snapshot.Skills.Values
                     .Where(p => p.Level >= MasteryLevel.Practiced && p.SkillId != focus?.Id)
                     .OrderBy(p => snapshot.SkillAccuracy(p.SkillId) ?? 0)
                     .ThenBy(_ => random.Next()))
        {
            if (selector.Pick(p.SkillId, InterleavePurposes, used) is { } it)
            {
                interleaveQueue.Enqueue(it);
            }
        }

        var practiceBudget = budget[SessionBlockKind.Practice];
        var turn = 0;
        while (practiceBudget > 0 && (focusQueue.Count + remedialQueue.Count + interleaveQueue.Count) > 0)
        {
            LearningItem? next = null;
            var reason = "";
            var slot = turn++ % 4;
            if (slot is 0 or 1 && focusQueue.Count > 0)
            {
                next = focusQueue.Dequeue();
                reason = "Práctica del concepto actual";
            }
            else if (slot == 2 && remedialQueue.Count > 0)
            {
                (next, reason) = remedialQueue.Dequeue();
            }
            else if (slot == 3 && interleaveQueue.Count > 0)
            {
                next = interleaveQueue.Dequeue();
                reason = "Práctica intercalada con temas anteriores";
            }
            else
            {
                if (focusQueue.Count > 0) { next = focusQueue.Dequeue(); reason = "Práctica del concepto actual"; }
                else if (remedialQueue.Count > 0) { (next, reason) = remedialQueue.Dequeue(); }
                else if (interleaveQueue.Count > 0) { next = interleaveQueue.Dequeue(); reason = "Práctica intercalada con temas anteriores"; }
            }

            if (next is null || used.Contains(next.Id))
            {
                continue;
            }

            Add(SessionBlockKind.Practice, next, next.Purpose == ItemPurpose.Independent && next.IsCode, reason);
            practiceBudget -= next.EstimatedMinutes;
        }

        // 5. Aplicación sin ayuda: transferencia del foco o problema mixto de temas ya practicados.
        LearningItem? application = null;
        if (focus?.Lesson is { } fl && catalog.FindItem(fl.ApplicationExerciseId) is { } app && !snapshot.SolvedCorrectly(app.Id) && !used.Contains(app.Id))
        {
            application = app;
        }

        application ??= focus is null ? null : selector.Pick(focus.Id, [ItemPurpose.Transfer], used);
        application ??= catalog.Items
            .Where(i => i.Purpose == ItemPurpose.Transfer && !used.Contains(i.Id) && !snapshot.SolvedCorrectly(i.Id))
            .Where(i => i.AllSkillIds().All(s => snapshot.Level(s) >= MasteryLevel.Practiced))
            .OrderByDescending(i => i.RelatedSkillIds.Count)
            .ThenBy(_ => random.Next())
            .FirstOrDefault();
        if (application is not null)
        {
            Add(SessionBlockKind.Application, application, true, "Aplicación sin ayuda en un contexto nuevo");
        }

        // 6. Cierre.
        activities.Add((SessionBlockKind.WrapUp, ActivityKind.Summary, "resumen", budget[SessionBlockKind.WrapUp], false, "Resultados, errores y próximos repasos"));

        var final = activities.Select((a, i) => new SessionActivity(i, a.Block, a.Kind, a.Ref, a.Minutes, a.Ask, a.Reason)).ToList();
        var blocks = template.Select(b => new SessionBlockPlan(b.Kind, budget[b.Kind])).ToList();
        return new SessionPlan(minutes, blocks, final, focus?.Id);

        void Add(SessionBlockKind block, LearningItem item, bool ask, string reason)
        {
            used.Add(item.Id);
            activities.Add((block, ActivityKind.Item, item.Id, item.EstimatedMinutes, ask, reason));
        }
    }
}
