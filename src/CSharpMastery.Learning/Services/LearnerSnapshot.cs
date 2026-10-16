using CSharpMastery.Domain.Progress;

namespace CSharpMastery.Learning.Services;

/// <summary>Fotografía del estado del estudiante usada por los algoritmos (sin acceso a base de datos).</summary>
public sealed class LearnerSnapshot(IReadOnlyDictionary<string, SkillProgress> skills, IReadOnlyList<AttemptRecord> attempts)
{
    private readonly ILookup<string, AttemptRecord> _bySkill = attempts.ToLookup(a => a.SkillId);
    private readonly ILookup<string, AttemptRecord> _byItem = attempts.ToLookup(a => a.ItemId);

    public static LearnerSnapshot Empty { get; } = new(new Dictionary<string, SkillProgress>(), []);

    public IReadOnlyDictionary<string, SkillProgress> Skills { get; } = skills;

    public IReadOnlyList<AttemptRecord> Attempts { get; } = attempts;

    public SkillProgress Skill(string id) => Skills.TryGetValue(id, out var p) ? p : SkillProgress.Initial(id);

    public MasteryLevel Level(string id) => Skill(id).Level;

    public IEnumerable<AttemptRecord> AttemptsForSkill(string skillId) => _bySkill[skillId];

    public IEnumerable<AttemptRecord> AttemptsForItem(string itemId) => _byItem[itemId];

    public bool SolvedCorrectly(string itemId) => _byItem[itemId].Any(a => a.Correct && a.AutoGraded);

    public DateTimeOffset? LastAttemptAt(string itemId) => _byItem[itemId].Select(a => (DateTimeOffset?)a.At).DefaultIfEmpty(null).Max();

    public DateTimeOffset? LastSkillActivity(string skillId) => _bySkill[skillId].Select(a => (DateTimeOffset?)a.At).DefaultIfEmpty(null).Max();

    public double? SkillAccuracy(string skillId, int window = 12)
    {
        var recent = _bySkill[skillId].Where(a => a.AutoGraded).OrderByDescending(a => a.At).Take(window).ToList();
        return recent.Count == 0 ? null : recent.Count(a => a.Correct) / (double)recent.Count;
    }
}
