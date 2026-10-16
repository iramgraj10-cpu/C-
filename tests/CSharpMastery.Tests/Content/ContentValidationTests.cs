using CSharpMastery.Content;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Execution;
using CSharpMastery.Runner;
using Xunit.Abstractions;

namespace CSharpMastery.Tests.Content;

/// <summary>
/// Verifica el banco de contenidos: estructura, referencias y, sobre todo, que cada solución modelo
/// pase sus pruebas, que el código inicial no pase, y que las salidas declaradas coincidan con la ejecución real.
/// </summary>
public sealed class ContentValidationTests(ITestOutputHelper output)
{
    private static readonly CurriculumCatalog Catalog = CurriculumContent.Catalog;

    [Fact]
    public void Ids_son_unicos_y_las_referencias_existen()
    {
        var skillIds = Catalog.OrderedSkills.Select(s => s.Id).ToList();
        Assert.Equal(skillIds.Count, skillIds.Distinct().Count());

        foreach (var skill in Catalog.OrderedSkills)
        {
            foreach (var prereq in skill.Prerequisites)
            {
                var p = Catalog.FindSkill(prereq);
                Assert.True(p is not null, $"{skill.Id}: prerrequisito inexistente {prereq}");
                Assert.True(Catalog.OrderOf(prereq) < Catalog.OrderOf(skill.Id), $"{skill.Id}: el prerrequisito {prereq} debe aparecer antes en la ruta");
            }
        }

        foreach (var item in Catalog.Items)
        {
            Assert.True(Catalog.FindSkill(item.SkillId) is not null, $"{item.Id}: habilidad inexistente {item.SkillId}");
            foreach (var related in item.RelatedSkillIds)
            {
                Assert.True(Catalog.FindSkill(related) is not null, $"{item.Id}: habilidad relacionada inexistente {related}");
            }
        }
    }

    [Fact]
    public void Cada_leccion_completa_tiene_todas_las_partes_y_variedad_de_items()
    {
        foreach (var skill in Catalog.OrderedSkills.Where(s => s.Lesson is not null))
        {
            var lesson = skill.Lesson!;
            Assert.False(string.IsNullOrWhiteSpace(lesson.Objective), skill.Id);
            Assert.NotEmpty(lesson.Explanation);
            Assert.False(string.IsNullOrWhiteSpace(lesson.WorkedExample.Code), skill.Id);
            Assert.NotEmpty(lesson.WorkedExample.Notes);
            Assert.False(string.IsNullOrWhiteSpace(lesson.CommonMistake.WhyItFails), skill.Id);
            Assert.False(string.IsNullOrWhiteSpace(lesson.MasteryCriterion), skill.Id);
            Assert.True(lesson.RecallItemIds.Count >= 2, $"{skill.Id}: se requieren al menos 2 preguntas de recuperación");

            foreach (var id in lesson.AllItemIds())
            {
                var item = Catalog.FindItem(id);
                Assert.True(item is not null, $"{skill.Id}: ítem inexistente {id}");
                Assert.Equal(skill.Id, item!.SkillId);
            }

            Assert.Equal(ItemPurpose.Prediction, Catalog.GetItem(lesson.PredictionItemId).Purpose);
            Assert.IsType<CodeExercise>(Catalog.GetItem(lesson.GuidedExerciseId));
            Assert.Equal(ItemPurpose.Guided, Catalog.GetItem(lesson.GuidedExerciseId).Purpose);
            Assert.IsType<CodeExercise>(Catalog.GetItem(lesson.IndependentExerciseId));
            Assert.Equal(ItemPurpose.Independent, Catalog.GetItem(lesson.IndependentExerciseId).Purpose);
            Assert.Equal(ItemPurpose.Transfer, Catalog.GetItem(lesson.ApplicationExerciseId).Purpose);

            var purposes = Catalog.ItemsForSkill(skill.Id).Select(i => i.Purpose).ToHashSet();
            Assert.Contains(ItemPurpose.BugFinding, purposes);
            Assert.True(purposes.Contains(ItemPurpose.Concept) || purposes.Contains(ItemPurpose.Recall), skill.Id);
        }
    }

    [Fact]
    public void Los_proyectos_tienen_hitos_evaluables_y_rubrica()
    {
        Assert.NotEmpty(Catalog.Projects);
        Assert.Equal(Catalog.Projects.Count, Catalog.Projects.Select(p => p.Id).Distinct().Count());
        foreach (var project in Catalog.Projects)
        {
            Assert.NotEmpty(project.Requirements);
            Assert.NotEmpty(project.AcceptanceCriteria);
            Assert.True(project.Rubric.Count >= 3, $"{project.Id}: la rúbrica necesita al menos 3 criterios");
            Assert.True(project.Milestones.Count >= 2, $"{project.Id}: se necesitan al menos 2 hitos");
            Assert.All(project.SkillIds, id => Assert.True(Catalog.FindSkill(id) is not null, $"{project.Id}: habilidad inexistente {id}"));
            foreach (var milestone in project.Milestones)
            {
                var exercise = Assert.IsType<CodeExercise>(Catalog.FindItem(milestone.ExerciseId));
                Assert.Equal(ItemPurpose.Project, exercise.Purpose);
                Assert.Contains(exercise.Tests, t => t.Hidden);
            }
        }

        // Los hitos de proyecto no deben aparecer en sesiones ni evaluaciones: solo se usan desde la página del proyecto.
        var milestoneIds = Catalog.Projects.SelectMany(p => p.Milestones).Select(m => m.ExerciseId).ToHashSet();
        Assert.All(Catalog.Items.Where(i => i.Purpose == ItemPurpose.Project), i => Assert.Contains(i.Id, milestoneIds));
    }

    [Fact]
    public void Las_preguntas_de_opcion_tienen_exactamente_una_respuesta_correcta_y_retroalimentacion()
    {
        foreach (var item in Catalog.Items.OfType<ChoiceItem>())
        {
            Assert.True(item.Options.Count >= 2, item.Id);
            Assert.True(item.Options.Count(o => o.IsCorrect) == 1, $"{item.Id}: debe haber exactamente una opción correcta");
            Assert.All(item.Options, o => Assert.False(string.IsNullOrWhiteSpace(o.Feedback), $"{item.Id}: opción sin retroalimentación"));
            Assert.Equal(item.Options.Count, item.Options.Select(o => o.Text).Distinct().Count());
        }

        foreach (var item in Catalog.Items.OfType<ShortAnswerItem>())
        {
            Assert.NotEmpty(item.AcceptedAnswers);
        }
    }

    [Fact]
    public void Las_soluciones_modelo_pasan_y_el_codigo_inicial_no()
    {
        var failures = new List<string>();
        foreach (var exercise in Catalog.Items.OfType<CodeExercise>())
        {
            Assert.True(exercise.Tests.Count > 0, $"{exercise.Id}: sin pruebas");
            Assert.True(exercise.Hints.Count >= 1, $"{exercise.Id}: sin pistas");

            var solution = CodeGrader.Grade(exercise, InProcessRunner.Execute(CodeGrader.BuildRequest(exercise, exercise.ReferenceSolution)));
            if (!solution.Passed)
            {
                failures.Add($"{exercise.Id}: la solución modelo falla → {solution.Summary} " +
                             string.Join(" | ", solution.Diagnostics.Select(d => $"{d.Raw.Id} L{d.Raw.Line}: {d.Raw.Message}")) + " " +
                             string.Join(" | ", solution.Tests.Where(t => !t.Passed).Select(t => $"{t.Name}: esperado «{t.Expected}» obtenido «{t.Actual}» {t.Message}")));
                continue;
            }

            var starter = CodeGrader.Grade(exercise, InProcessRunner.Execute(CodeGrader.BuildRequest(exercise, exercise.StarterCode)));
            if (starter.Passed)
            {
                failures.Add($"{exercise.Id}: el código inicial ya aprueba; las pruebas no exigen nada.");
            }
        }

        foreach (var f in failures)
        {
            output.WriteLine(f);
        }

        Assert.Empty(failures);
    }

    [Fact]
    public void Las_predicciones_coinciden_con_la_ejecucion_real()
    {
        var failures = new List<string>();
        foreach (var item in Catalog.Items.Where(i => i.Purpose == ItemPurpose.Prediction && i.Code is not null))
        {
            var result = InProcessRunner.RunProgram(item.Code!);
            var correct = item switch
            {
                ChoiceItem c => c.Options.Single(o => o.IsCorrect).Text,
                ShortAnswerItem s => s.AcceptedAnswers[0],
                _ => null,
            };
            if (correct is null)
            {
                continue;
            }

            if (result.Status == ExecutionStatus.CompilationFailed)
            {
                if (!correct.Contains("compila", StringComparison.OrdinalIgnoreCase))
                {
                    failures.Add($"{item.Id}: el código no compila pero la respuesta correcta es «{correct}»: {string.Join("; ", result.Errors.Select(e => e.Message))}");
                }

                continue;
            }

            var run = result.Tests.Single();
            if (run.Failure is { } failure)
            {
                if (!correct.Contains(failure.ExceptionType, StringComparison.Ordinal))
                {
                    failures.Add($"{item.Id}: lanza {failure.ExceptionType} pero la respuesta correcta es «{correct}»");
                }

                continue;
            }

            var matches = item is ShortAnswerItem { Match: AnswerMatch.Number }
                ? double.Parse(run.Output.Trim(), System.Globalization.CultureInfo.InvariantCulture) == double.Parse(correct, System.Globalization.CultureInfo.InvariantCulture)
                : OutputComparer.AreEquivalent(correct, run.Output);
            if (!matches)
            {
                failures.Add($"{item.Id}: la salida real «{run.Output.TrimEnd()}» no coincide con la respuesta «{correct}»");
            }
        }

        foreach (var f in failures)
        {
            output.WriteLine(f);
        }

        Assert.Empty(failures);
    }

    [Fact]
    public void Los_ejemplos_resueltos_y_bloques_de_codigo_compilan_y_producen_la_salida_declarada()
    {
        var failures = new List<string>();
        foreach (var skill in Catalog.OrderedSkills.Where(s => s.Lesson is not null))
        {
            var lesson = skill.Lesson!;
            var example = InProcessRunner.RunProgram(lesson.WorkedExample.Code);
            if (example.Status != ExecutionStatus.Completed || example.Tests.Single().Failure is not null)
            {
                failures.Add($"{skill.Id}: el ejemplo resuelto no se ejecuta ({example.Status}): {string.Join("; ", example.Errors.Select(e => $"L{e.Line} {e.Message}"))} {example.Tests.FirstOrDefault()?.Failure?.Message}");
            }
            else if (lesson.WorkedExample.ExpectedOutput is { } expected && !OutputComparer.AreEquivalent(expected, example.Tests.Single().Output))
            {
                failures.Add($"{skill.Id}: el ejemplo resuelto muestra «{example.Tests.Single().Output.TrimEnd()}» y la lección dice «{expected}»");
            }

            foreach (var block in lesson.Explanation.OfType<CodeBlock>().Where(b => b.Caption?.StartsWith("Fragmento", StringComparison.Ordinal) != true))
            {
                var run = InProcessRunner.RunProgram(block.Code);
                if (run.Status != ExecutionStatus.Completed || run.Tests.Single().Failure is not null)
                {
                    failures.Add($"{skill.Id}: un bloque de la explicación no se ejecuta: {string.Join("; ", run.Errors.Select(e => $"L{e.Line} {e.Message}"))} {run.Tests.FirstOrDefault()?.Failure?.ExceptionType}\n{block.Code}");
                }
            }

            var fixedRun = InProcessRunner.RunProgram(lesson.CommonMistake.FixedCode);
            if (fixedRun.Status == ExecutionStatus.CompilationFailed && !fixedRun.Errors.All(e => e.Id == "CS5001"))
            {
                failures.Add($"{skill.Id}: la versión corregida del error habitual no compila: {string.Join("; ", fixedRun.Errors.Select(e => $"{e.Id} {e.Message}"))}");
            }
        }

        foreach (var f in failures)
        {
            output.WriteLine(f);
        }

        Assert.Empty(failures);
    }
}
