using System.Collections.Concurrent;
using CSharpMastery.Content;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Execution;
using CSharpMastery.Learning.Interactive;
using CSharpMastery.Runner;
using Xunit.Abstractions;

namespace CSharpMastery.Tests.Content;

/// <summary>
/// Valida los pasos interactivos generados a partir de las lecciones ejecutando el código de verdad:
/// cada distractor de un hueco debe no compilar o cambiar lo que hace el programa, y en los ejercicios de ordenar
/// ningún intercambio de líneas vecinas distintas debe producir el mismo resultado. Las ejecuciones usan el
/// proceso ejecutor real (con límite de tiempo), porque una variante puede contener un bucle infinito.
/// </summary>
[Collection("runner")]
public sealed class InteractiveValidationTests(ITestOutputHelper output)
{
    private const string SampleInput = "5\n3\n7\n2\n4\n6\n1\n8\n9\n10\n";

    private static readonly ExecutionLimits Limits = ExecutionLimits.Default with { TestTimeoutMs = 1500, TotalTimeoutMs = 10_000 };

    private static readonly Lazy<ProcessExecutionService[]> Pool = new(() => Enumerable.Range(0, 4)
        .Select(_ => new ProcessExecutionService(RunnerLaunchInfo.ForDll(Path.Combine(AppContext.BaseDirectory, "CSharpMastery.Runner.dll"))))
        .ToArray());

    private sealed record Check(string Key, string Description, string Original, string Variant);

    private static IEnumerable<(Skill Skill, IReadOnlyList<MicroStep> Steps)> AllFlows()
    {
        var builder = new LessonFlowBuilder(CurriculumContent.InteractiveExclusions);
        foreach (var skill in CurriculumContent.Catalog.OrderedSkills.Where(s => s.Lesson is not null))
        {
            var explanation = builder.Explanation(skill, skill.Lesson!);
            var practice = builder.Practice(skill, skill.Lesson!, includeCloze: !LessonFlowBuilder.HasCloze(explanation));
            yield return (skill, explanation.Concat(practice).ToList());
        }
    }

    [Fact]
    public void Cada_leccion_genera_pasos_interactivos()
    {
        foreach (var (skill, steps) in AllFlows())
        {
            Assert.IsType<IntroStep>(steps[0]);
            Assert.Contains(steps, s => s is ConceptCardStep);
            Assert.Contains(steps, s => s is WalkthroughStep);
            Assert.Contains(steps, s => s is SpotBugStep or CompareCodeStep);
            var interactive = steps.Count(s => s is ClozeStep or ParsonsStep or SpotBugStep or CompareCodeStep);
            Assert.True(interactive >= 2, $"{skill.Id}: solo {interactive} pasos interactivos");
            Assert.Equal(steps.Count, steps.Select(s => s.Key).Distinct().Count());
            foreach (var step in steps.OfType<SpotBugStep>())
            {
                Assert.All(step.BugLines, l => Assert.InRange(l, 1, step.CodeLines.Count));
            }

            foreach (var step in steps.OfType<ParsonsStep>())
            {
                Assert.True(step.IsCorrect(step.MovableIndexes), skill.Id);
                Assert.False(step.IsCorrect(step.BankOrder), $"{skill.Id}: el banco ya está en orden");
                Assert.Equal(string.Join("\n", step.Lines.Select(l => l.Text)), step.Assemble(step.MovableIndexes));
            }

            foreach (var step in steps.OfType<ClozeStep>())
            {
                Assert.Contains(step.Answer, step.Options);
                Assert.Equal(step.Options.Count, step.Options.Distinct().Count());
            }
        }

        output.WriteLine($"Pasos generados: {AllFlows().Sum(f => f.Steps.Count)}; huecos: {AllFlows().Sum(f => f.Steps.Count(s => s is ClozeStep))}; " +
                         $"ordenar: {AllFlows().Sum(f => f.Steps.Count(s => s is ParsonsStep))}; tocar el error: {AllFlows().Sum(f => f.Steps.Count(s => s is SpotBugStep))}");
    }

    [Fact]
    public async Task Los_distractores_y_los_ordenes_alternativos_no_son_equivalentes()
    {
        var checks = new List<Check>();
        foreach (var (skill, steps) in AllFlows())
        {
            foreach (var cloze in steps.OfType<ClozeStep>())
            {
                checks.AddRange(cloze.Options.Where(o => o != cloze.Answer)
                    .Select(o => new Check(cloze.Key, $"«{o}» en lugar de «{cloze.Answer}»", cloze.Code, cloze.Fill(o))));
            }

            foreach (var parsons in steps.OfType<ParsonsStep>())
            {
                var order = parsons.MovableIndexes;
                var original = parsons.Assemble(order);
                for (var k = 0; k + 1 < order.Count; k++)
                {
                    if (parsons.Lines[order[k]].Content == parsons.Lines[order[k + 1]].Content)
                    {
                        continue;
                    }

                    var swapped = order.ToArray();
                    (swapped[k], swapped[k + 1]) = (swapped[k + 1], swapped[k]);
                    checks.Add(new Check(parsons.Key, $"intercambiar «{parsons.Lines[order[k]].Content}» y «{parsons.Lines[order[k + 1]].Content}»", original, parsons.Assemble(swapped)));
                }
            }
        }

        // Las variantes que no compilan son claramente distintas; solo se ejecutan las que compilan.
        var toRun = checks.Where(c => InProcessRunner.Compiles(c.Variant)).ToList();
        var originals = new ConcurrentDictionary<string, string>();
        var failures = new ConcurrentBag<string>();
        var queue = new ConcurrentQueue<Check>(toRun);
        var workers = Pool.Value.Select(async service =>
        {
            while (queue.TryDequeue(out var check))
            {
                var expected = originals.TryGetValue(check.Original, out var cached) ? cached : await Signature(service, check.Original);
                originals[check.Original] = expected;
                var actual = await Signature(service, check.Variant);
                if (actual == expected)
                {
                    failures.Add($"\"{check.Key}\", // {check.Description}: mismo resultado ({Shorten(expected)})");
                }
            }
        });
        await Task.WhenAll(workers);

        output.WriteLine($"Variantes revisadas: {checks.Count}; ejecutadas: {toRun.Count}");
        foreach (var f in failures.OrderBy(f => f))
        {
            output.WriteLine(f);
        }

        Assert.Empty(failures);
    }

    private static async Task<string> Signature(IExecutionService service, string code)
    {
        var result = await service.ExecuteAsync(new ExecutionRequest { Code = code, Mode = Domain.Items.CodeMode.Program, Stdin = SampleInput, Limits = Limits });
        if (result.Status != ExecutionStatus.Completed)
        {
            return $"[{result.Status}]";
        }

        var run = result.Tests.Single();
        return run.TimedOut ? "[tiempo]" : run.Failure is { } f ? $"[{f.ExceptionType}] {run.Output}" : OutputComparer.Normalize(run.Output);
    }

    private static string Shorten(string s) => (s.Length > 60 ? s[..60] + "…" : s).Replace("\n", "⏎");
}
