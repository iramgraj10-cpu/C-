using CSharpMastery.Domain.Curriculum;

namespace CSharpMastery.Content.Infrastructure;

/// <summary>Habilidad del temario aún sin lección interactiva (se muestra como «en preparación»).</summary>
public static class Outline
{
    public static Skill Skill(string id, string unitId, string title, string summary, Tier tier, params string[] prerequisites) =>
        new(id, unitId, title, summary, tier, prerequisites, null);
}
