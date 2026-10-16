using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;

namespace CSharpMastery.Learning.Services;

/// <summary>Elige ítems priorizando los no resueltos, cercanos a la dificultad actual y menos recientes.</summary>
public sealed class ItemSelector(CurriculumCatalog catalog, LearnerSnapshot snapshot, Random random)
{
    public IEnumerable<LearningItem> Rank(string skillId, IReadOnlyCollection<ItemPurpose> purposes, ISet<string> exclude, int? targetDifficulty = null)
    {
        var target = targetDifficulty ?? snapshot.Skill(skillId).Difficulty;
        return catalog.ItemsForSkill(skillId)
            .Where(i => purposes.Contains(i.Purpose) && !exclude.Contains(i.Id))
            .Select(i => (item: i, tie: random.Next()))
            .OrderBy(x => snapshot.SolvedCorrectly(x.item.Id) ? 1 : 0)
            .ThenBy(x => Math.Abs(x.item.Difficulty - target))
            .ThenBy(x => snapshot.LastAttemptAt(x.item.Id) ?? DateTimeOffset.MinValue)
            .ThenBy(x => x.tie)
            .Select(x => x.item);
    }

    public LearningItem? Pick(string skillId, IReadOnlyCollection<ItemPurpose> purposes, ISet<string> exclude, int? targetDifficulty = null) =>
        Rank(skillId, purposes, exclude, targetDifficulty).FirstOrDefault();

    /// <summary>Ítems que entrenan una categoría de error, solo de habilidades ya desbloqueadas.</summary>
    public IEnumerable<LearningItem> TargetingError(ErrorCategory error, ISet<string> exclude) =>
        catalog.Items
            .Where(i => i.TargetsErrors.Contains(error) && !exclude.Contains(i.Id))
            .Where(i => i.AllSkillIds().All(s => snapshot.Level(s) >= MasteryLevel.Seen))
            .OrderBy(i => snapshot.SolvedCorrectly(i.Id) ? 1 : 0)
            .ThenBy(i => snapshot.LastAttemptAt(i.Id) ?? DateTimeOffset.MinValue)
            .ThenBy(_ => random.Next());
}
