using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;

namespace CSharpMastery.Learning.Mastery;

/// <summary>Qué tipos de evidencia puede aportar una habilidad según los ítems disponibles.</summary>
public sealed record SkillEvidenceProfile(bool HasCodeItems, bool HasTransferItems);

/// <summary>Resultado de evaluar el dominio, con lo que falta para el siguiente nivel.</summary>
public sealed record MasteryEvaluation(
    MasteryLevel Level,
    double WindowAccuracy,
    int GradedAttempts,
    int DistinctItemsCorrect,
    int IndependentCodeSolutions,
    int TransferSolutions,
    IReadOnlyList<string> Pending);

/// <summary>
/// Decide el nivel de dominio a partir de la evidencia. Nunca marca dominio por leer contenido:
/// exige precisión en intentos recientes calificados automáticamente, variedad de ítems,
/// resolución independiente de código y transferencia, y luego retención en un repaso posterior.
/// </summary>
public sealed class MasteryEvaluator(MasteryCriteria criteria)
{
    /// <summary>Mínimo de ítems distintos resueltos para que la precisión no provenga de repetir uno solo.</summary>
    public const int MinDistinctItems = 3;

    /// <summary>Soluciones independientes correctas exigidas tras olvidar o fallar una habilidad.</summary>
    public const int ReinforcementSolutions = 2;

    public MasteryCriteria Criteria => criteria;

    public MasteryEvaluation Evaluate(SkillProgress progress, IReadOnlyList<AttemptRecord> skillAttempts, SkillEvidenceProfile profile, DateTimeOffset now)
    {
        var graded = skillAttempts
            .Where(a => a.AutoGraded && a.SkillId == progress.SkillId)
            .OrderByDescending(a => a.At)
            .Take(criteria.AccuracyWindow)
            .ToList();

        var accuracy = graded.Count == 0 ? 0 : graded.Count(a => a.Correct) / (double)graded.Count;
        var distinctCorrect = graded.Where(a => a.Correct).Select(a => a.ItemId).Distinct().Count();

        var allOwn = skillAttempts.Where(a => a.AutoGraded && a.SkillId == progress.SkillId).ToList();
        var independentCode = allOwn
            .Where(a => a.IsCode && a.Correct && a.Independent && a.Purpose is ItemPurpose.Independent or ItemPurpose.Transfer)
            .Select(a => a.ItemId).Distinct().Count();
        var transfer = allOwn
            .Where(a => a.Correct && a.Independent && a.Purpose == ItemPurpose.Transfer)
            .Select(a => a.ItemId).Distinct().Count();

        var pending = new List<string>();
        if (graded.Count < criteria.MinGradedAttempts)
        {
            pending.Add($"Resolver al menos {criteria.MinGradedAttempts} ejercicios calificados (llevas {graded.Count}).");
        }

        if (accuracy < criteria.MinAccuracy)
        {
            pending.Add($"Alcanzar {criteria.MinAccuracy:P0} de precisión en los intentos recientes (llevas {accuracy:P0}).");
        }

        if (distinctCorrect < MinDistinctItems)
        {
            pending.Add($"Resolver correctamente al menos {MinDistinctItems} ejercicios distintos.");
        }

        if (profile.HasCodeItems && independentCode < criteria.MinIndependentCodeSolutions)
        {
            pending.Add("Escribir una solución de código correcta sin pistas.");
        }

        if (profile.HasTransferItems && transfer < criteria.MinTransferSolutions)
        {
            pending.Add("Resolver sin ayuda un problema de aplicación distinto a los ejemplos.");
        }

        if (progress.NeedsReinforcement && progress.ReinforcementSince is { } reinforcedAt)
        {
            var after = allOwn.Where(a => a.At > reinforcedAt).OrderByDescending(a => a.At).ToList();
            var correctIndependent = after.Count(a => a.Correct && a.Independent);
            if (correctIndependent < ReinforcementSolutions || after.Count == 0 || !after[0].Correct)
            {
                pending.Add($"Resolver sin ayuda {ReinforcementSolutions} ejercicios de refuerzo (llevas {correctIndependent}).");
            }
        }

        var meetsProvisional = pending.Count == 0;
        var level = progress.Level;

        if (graded.Count > 0 && level < MasteryLevel.Practiced)
        {
            level = MasteryLevel.Practiced;
        }

        if (meetsProvisional && level == MasteryLevel.Practiced)
        {
            level = MasteryLevel.ProvisionallyMastered;
        }

        if (level == MasteryLevel.ProvisionallyMastered)
        {
            var since = progress.ProvisionalMasteryAt ?? now;
            var retentionEvidence = allOwn.Any(a =>
                a.Context == AttemptContext.Review && a.Correct && a.Independent &&
                a.At >= since.AddDays(criteria.RetentionMinDays));
            if (retentionEvidence)
            {
                level = MasteryLevel.Retained;
            }
            else
            {
                pending.Add($"Recordarla correctamente en un repaso al menos {criteria.RetentionMinDays} días después del dominio provisional.");
            }
        }

        return new MasteryEvaluation(level, accuracy, graded.Count, distinctCorrect, independentCode, transfer, pending);
    }

    /// <summary>Aplica la evaluación al progreso, registrando fechas de transición.</summary>
    public SkillProgress Apply(SkillProgress progress, MasteryEvaluation evaluation, DateTimeOffset now)
    {
        var updated = progress with { Level = evaluation.Level };
        if (evaluation.Level >= MasteryLevel.Practiced && progress.FirstPracticedAt is null)
        {
            updated = updated with { FirstPracticedAt = now };
        }

        if (evaluation.Level >= MasteryLevel.ProvisionallyMastered && progress.Level < MasteryLevel.ProvisionallyMastered)
        {
            updated = updated with { ProvisionalMasteryAt = now, NeedsReinforcement = false, ReinforcementSince = null };
        }

        if (evaluation.Level == MasteryLevel.Retained && progress.Level < MasteryLevel.Retained)
        {
            updated = updated with { RetainedAt = now };
        }

        return updated;
    }

    /// <summary>Degrada una habilidad olvidada o fallada en una evaluación: vuelve a práctica con refuerzo.</summary>
    public static SkillProgress Demote(SkillProgress progress, DateTimeOffset now) =>
        progress.Level >= MasteryLevel.ProvisionallyMastered
            ? progress with { Level = MasteryLevel.Practiced, NeedsReinforcement = true, ReinforcementSince = now, ProvisionalMasteryAt = null, RetainedAt = null }
            : progress with { NeedsReinforcement = true, ReinforcementSince = progress.ReinforcementSince ?? now };
}
