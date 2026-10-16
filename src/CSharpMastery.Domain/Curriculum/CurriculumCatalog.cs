using CSharpMastery.Domain.Items;

namespace CSharpMastery.Domain.Curriculum;

/// <summary>Índice en memoria del banco de contenidos: niveles, unidades, habilidades, ítems y proyectos.</summary>
public sealed class CurriculumCatalog
{
    private readonly Dictionary<string, Skill> _skills;
    private readonly Dictionary<string, Unit> _units;
    private readonly Dictionary<string, LearningItem> _items;
    private readonly Dictionary<string, List<LearningItem>> _itemsBySkill;
    private readonly Dictionary<string, int> _skillOrder;

    public CurriculumCatalog(IReadOnlyList<CurriculumModule> modules, IEnumerable<LearningItem> items, IReadOnlyList<IntegrativeProject> projects)
    {
        Modules = modules;
        Projects = projects;
        _units = modules.SelectMany(m => m.Units).ToDictionary(u => u.Id);
        var orderedSkills = modules.OrderBy(m => m.Level).SelectMany(m => m.Units).SelectMany(u => u.Skills).ToList();
        _skills = orderedSkills.ToDictionary(s => s.Id);
        _skillOrder = orderedSkills.Select((s, i) => (s.Id, i)).ToDictionary(x => x.Id, x => x.i);
        OrderedSkills = orderedSkills;
        _items = items.ToDictionary(i => i.Id);
        _itemsBySkill = _items.Values.GroupBy(i => i.SkillId).ToDictionary(g => g.Key, g => g.ToList());
    }

    public IReadOnlyList<CurriculumModule> Modules { get; }

    public IReadOnlyList<IntegrativeProject> Projects { get; }

    /// <summary>Habilidades en el orden de la ruta.</summary>
    public IReadOnlyList<Skill> OrderedSkills { get; }

    public IEnumerable<LearningItem> Items => _items.Values;

    public Skill GetSkill(string id) => _skills.TryGetValue(id, out var s) ? s : throw new KeyNotFoundException($"Habilidad desconocida: {id}");

    public Skill? FindSkill(string id) => _skills.GetValueOrDefault(id);

    public Unit GetUnit(string id) => _units[id];

    public CurriculumModule GetModuleOfUnit(string unitId) => Modules.First(m => m.Units.Any(u => u.Id == unitId));

    public LearningItem GetItem(string id) => _items.TryGetValue(id, out var i) ? i : throw new KeyNotFoundException($"Ítem desconocido: {id}");

    public LearningItem? FindItem(string id) => _items.GetValueOrDefault(id);

    public IReadOnlyList<LearningItem> ItemsForSkill(string skillId) =>
        _itemsBySkill.TryGetValue(skillId, out var list) ? list : [];

    public int OrderOf(string skillId) => _skillOrder.TryGetValue(skillId, out var i) ? i : int.MaxValue;

    public IntegrativeProject? FindProject(string id) => Projects.FirstOrDefault(p => p.Id == id);
}
