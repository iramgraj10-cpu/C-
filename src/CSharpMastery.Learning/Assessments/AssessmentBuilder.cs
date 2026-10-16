using CSharpMastery.Domain.Assessments;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Services;

namespace CSharpMastery.Learning.Assessments;

/// <summary>
/// Arma evaluaciones con ítems elegidos al azar de un banco (cada intento es distinto), cubriendo
/// concepto, predicción, detección de errores, código desde cero y transferencia.
/// </summary>
public sealed class AssessmentBuilder(CurriculumCatalog catalog)
{
    private static readonly ItemPurpose[] ConceptPurposes = [ItemPurpose.Concept, ItemPurpose.Recall];

    public Assessment BuildUnit(string unitId, LearnerSnapshot snapshot, int seed, DateTimeOffset now)
    {
        var unit = catalog.GetUnit(unitId);
        var random = new Random(seed);
        var selector = new ItemSelector(catalog, snapshot, random);
        var used = new HashSet<string>();
        var items = new List<LearningItem>();
        var skills = unit.Skills.Where(s => s.Status == ContentStatus.Complete).ToList();
        if (skills.Count == 0)
        {
            throw new InvalidOperationException("La unidad no tiene lecciones completas para evaluar.");
        }

        foreach (var skill in skills)
        {
            PickRandom(skill.Id, ConceptPurposes);
            PickRandom(skill.Id, [ItemPurpose.Prediction]);
            PickRandom(skill.Id, [ItemPurpose.BugFinding]);
            PickRandom(skill.Id, [ItemPurpose.Independent], codeOnly: true);
        }

        foreach (var skill in skills.OrderBy(_ => random.Next()).Take(2))
        {
            PickRandom(skill.Id, [ItemPurpose.Transfer]);
        }

        var shuffled = items.OrderBy(_ => random.Next()).Select(i => i.Id).ToList();
        return new Assessment(Guid.NewGuid(), AssessmentKind.Unit, unitId, $"Evaluación: {unit.Title}", shuffled, seed, AssessmentStatus.InProgress, 0, now, null);

        void PickRandom(string skillId, ItemPurpose[] purposes, bool codeOnly = false)
        {
            var pool = catalog.ItemsForSkill(skillId)
                .Where(i => purposes.Contains(i.Purpose) && !used.Contains(i.Id) && (!codeOnly || i.IsCode))
                .ToList();
            if (pool.Count == 0)
            {
                return;
            }

            // Se prefieren ítems no vistos en los últimos 3 días para evaluar comprensión y no memoria del ítem.
            var fresh = pool.Where(i => snapshot.LastAttemptAt(i.Id) is not { } t || t < now.AddDays(-3)).ToList();
            var choice = (fresh.Count > 0 ? fresh : pool)[random.Next(fresh.Count > 0 ? fresh.Count : pool.Count)];
            used.Add(choice.Id);
            items.Add(choice);
        }
    }

    /// <summary>Evaluación acumulativa: mezcla habilidades ya practicadas, con prioridad a las débiles y antiguas.</summary>
    public Assessment BuildCumulative(LearnerSnapshot snapshot, int seed, DateTimeOffset now, int itemCount = 10)
    {
        var random = new Random(seed);
        var selector = new ItemSelector(catalog, snapshot, random);
        var skills = snapshot.Skills.Values
            .Where(p => p.Level >= MasteryLevel.Practiced && catalog.FindSkill(p.SkillId)?.Status == ContentStatus.Complete)
            .OrderBy(p => snapshot.SkillAccuracy(p.SkillId) ?? 0)
            .ThenBy(p => snapshot.LastSkillActivity(p.SkillId) ?? DateTimeOffset.MinValue)
            .Select(p => p.SkillId)
            .ToList();
        if (skills.Count < 2)
        {
            throw new InvalidOperationException("Practica al menos dos habilidades antes de una evaluación acumulativa.");
        }

        var used = new HashSet<string>();
        var picked = new List<LearningItem>();
        ItemPurpose[][] rotation =
        [
            [ItemPurpose.Independent],
            [ItemPurpose.Prediction],
            [ItemPurpose.BugFinding],
            [ItemPurpose.Concept, ItemPurpose.Recall],
            [ItemPurpose.Transfer],
        ];

        var round = 0;
        while (picked.Count < itemCount && round < 10)
        {
            for (var i = 0; i < skills.Count && picked.Count < itemCount; i++)
            {
                var purposes = rotation[(i + round) % rotation.Length];
                var candidates = catalog.ItemsForSkill(skills[i])
                    .Where(it => purposes.Contains(it.Purpose) && !used.Contains(it.Id))
                    .ToList();
                if (candidates.Count == 0)
                {
                    continue;
                }

                var item = candidates[random.Next(candidates.Count)];
                used.Add(item.Id);
                picked.Add(item);
            }

            round++;
        }

        // Garantiza al menos un problema mixto (que combina habilidades) si existe.
        var mixed = catalog.Items
            .Where(i => i.RelatedSkillIds.Count > 0 && !used.Contains(i.Id) && i.AllSkillIds().All(s => snapshot.Level(s) >= MasteryLevel.Practiced))
            .OrderBy(_ => random.Next())
            .FirstOrDefault();
        if (mixed is not null)
        {
            if (picked.Count >= itemCount)
            {
                picked.RemoveAt(picked.Count - 1);
            }

            picked.Add(mixed);
        }

        var ids = picked.OrderBy(_ => random.Next()).Select(i => i.Id).ToList();
        return new Assessment(Guid.NewGuid(), AssessmentKind.Cumulative, "acumulativa", "Evaluación acumulativa", ids, seed, AssessmentStatus.InProgress, 0, now, null);
    }
}
