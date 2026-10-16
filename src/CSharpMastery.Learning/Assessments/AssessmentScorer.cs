using CSharpMastery.Domain.Assessments;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Services;

namespace CSharpMastery.Learning.Assessments;

/// <summary>
/// Califica una evaluación. Cuenta solo el primer intento por ítem. Una habilidad es débil si su precisión queda
/// por debajo del umbral o si falló su código independiente o su transferencia. Se aprueba con precisión global
/// suficiente, ninguna habilidad por debajo de 50 % y todo el código independiente correcto.
/// </summary>
public sealed class AssessmentScorer(CurriculumCatalog catalog, MasteryCriteria criteria)
{
    public AssessmentResult Score(Assessment assessment, IReadOnlyList<AttemptRecord> attempts, LearnerSnapshot snapshot)
    {
        var firstByItem = attempts
            .Where(a => a.ContextId == assessment.Id.ToString())
            .GroupBy(a => a.ItemId)
            .ToDictionary(g => g.Key, g => g.OrderBy(a => a.At).First());

        var rows = new List<(LearningItem Item, AttemptRecord? Attempt)>();
        foreach (var id in assessment.ItemIds)
        {
            rows.Add((catalog.GetItem(id), firstByItem.GetValueOrDefault(id)));
        }

        var perSkill = rows
            .GroupBy(r => r.Item.SkillId)
            .Select(g =>
            {
                var code = g.Where(r => r.Item.IsCode && r.Item.Purpose is ItemPurpose.Independent).ToList();
                var transfer = g.Where(r => r.Item.Purpose == ItemPurpose.Transfer).ToList();
                return new SkillScore(
                    g.Key,
                    g.Count(r => r.Attempt?.Correct == true),
                    g.Count(),
                    code.All(r => r.Attempt is { Correct: true, Independent: true }),
                    transfer.All(r => r.Attempt is { Correct: true, Independent: true }),
                    code.Count > 0,
                    transfer.Count > 0);
            })
            .OrderBy(s => catalog.OrderOf(s.SkillId))
            .ToList();

        var total = rows.Count;
        var correct = rows.Count(r => r.Attempt?.Correct == true);
        var overall = total == 0 ? 0 : correct / (double)total;

        var weak = perSkill
            .Where(s => s.Accuracy < criteria.AssessmentPassAccuracy || (s.HasCodeItems && !s.IndependentCodeOk) || (s.HasTransferItems && !s.TransferOk))
            .Select(s => s.SkillId)
            .ToList();

        var passed = overall >= criteria.AssessmentPassAccuracy
                     && perSkill.All(s => s.Accuracy >= 0.5)
                     && perSkill.All(s => !s.HasCodeItems || s.IndependentCodeOk);

        var errors = rows.Where(r => r.Attempt is { Correct: false }).Select(r => (r.Item.SkillId, r.Attempt!.Error)).ToList();
        var recovery = new RecoveryPlanner(catalog).Build(weak, errors, assessment.ItemIds, snapshot);
        return new AssessmentResult(assessment.Id, overall, passed, perSkill, weak, recovery);
    }
}

/// <summary>Crea una ruta de recuperación para las habilidades débiles.</summary>
public sealed class RecoveryPlanner(CurriculumCatalog catalog)
{
    public RecoveryPlan Build(IReadOnlyList<string> weakSkills, IReadOnlyList<(string SkillId, ErrorCategory Error)> errors, IReadOnlyCollection<string> excludeItems, LearnerSnapshot snapshot)
    {
        var steps = new List<RecoveryStep>();
        var selector = new ItemSelector(catalog, snapshot, new Random(17));
        var exclude = new HashSet<string>(excludeItems);

        foreach (var skillId in weakSkills)
        {
            var skill = catalog.GetSkill(skillId);
            steps.Add(new RecoveryStep(skillId, $"Vuelve a estudiar el ejemplo resuelto y el error habitual de «{skill.Title}».", []));

            var practice = selector.Rank(skillId, [ItemPurpose.Guided, ItemPurpose.Independent, ItemPurpose.BugFinding], exclude).Take(3).Select(i => i.Id).ToList();
            foreach (var id in practice)
            {
                exclude.Add(id);
            }

            if (practice.Count > 0)
            {
                steps.Add(new RecoveryStep(skillId, "Practica de nuevo con ayuda decreciente: primero el ejercicio guiado y luego sin pistas.", practice));
            }

            foreach (var error in errors.Where(e => e.SkillId == skillId && e.Error != ErrorCategory.None).Select(e => e.Error).Distinct())
            {
                var targeted = selector.TargetingError(error, exclude).Take(2).Select(i => i.Id).ToList();
                foreach (var id in targeted)
                {
                    exclude.Add(id);
                }

                if (targeted.Count > 0)
                {
                    steps.Add(new RecoveryStep(skillId, $"Ejercicios dirigidos al error «{ErrorCategoryInfo.DisplayName(error)}».", targeted));
                }
            }

            steps.Add(new RecoveryStep(skillId, "Se programó un repaso para hoy. Después, vuelve a intentar la evaluación.", []));
        }

        return new RecoveryPlan(steps);
    }
}
