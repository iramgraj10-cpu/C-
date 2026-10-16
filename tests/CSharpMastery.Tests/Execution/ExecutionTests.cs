using CSharpMastery.Content;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Execution;
using CSharpMastery.Runner;

namespace CSharpMastery.Tests.Execution;

/// <summary>
/// Pruebas del motor real: se lanza el proceso ejecutor como hijo, igual que en la aplicación de escritorio.
/// Verifican compilación, evaluación por comportamiento y los controles de aislamiento.
/// </summary>
[Collection("runner")]
public sealed class ExecutionTests
{
    private static readonly IExecutionService Service = Runners.Process;

    private static Task<ExecutionResult> Run(string code, string? stdin = null, CodeMode mode = CodeMode.Program, ExecutionLimits? limits = null, params ExecutionTest[] tests) =>
        Service.ExecuteAsync(new ExecutionRequest { Code = code, Mode = mode, Stdin = stdin, Tests = tests, Limits = limits ?? ExecutionLimits.Default });

    [Fact]
    public async Task Compila_y_ejecuta_un_programa_con_entrada()
    {
        var result = await Run("var n = int.Parse(Console.ReadLine()!);\nConsole.WriteLine(n * 2);", "21");
        Assert.Equal(ExecutionStatus.Completed, result.Status);
        Assert.Equal("42", result.Tests.Single().Output.Trim());
    }

    [Fact]
    public async Task Los_errores_de_compilacion_se_explican_en_espanol_con_linea()
    {
        var result = await Run("Console.WriteLine(1)\nConsole.WriteLine(y);");
        Assert.Equal(ExecutionStatus.CompilationFailed, result.Status);
        var first = result.Errors.First();
        Assert.Equal("CS1002", first.Id);
        Assert.Equal(1, first.Line);
        Assert.Contains("punto y coma", DiagnosticExplainer.Explain(first).Text);
        Assert.Contains(result.Errors, e => e.Id == "CS0103" && e.Line == 2);
    }

    [Theory]
    [InlineData("System.IO.File.ReadAllText(\"/etc/hostname\");", "System.IO.File")]
    [InlineData("System.Diagnostics.Process.Start(\"ls\");", "System.Diagnostics.Process")]
    [InlineData("Environment.Exit(1);", "Environment.Exit")]
    [InlineData("var t = typeof(string).Assembly;", "System.Reflection.Assembly")]
    [InlineData("var c = new System.Net.Http.HttpClient();", "System.Net.Http.HttpClient")]
    [InlineData("Console.WriteLine(Environment.GetEnvironmentVariable(\"PATH\"));", "Environment.GetEnvironmentVariable")]
    [InlineData("dynamic d = 1; Console.WriteLine(d);", "dynamic")]
    [InlineData("var x = Activator.CreateInstance(typeof(object));", "System.Activator")]
    [InlineData("Console.SetOut(new System.IO.StringWriter());", "Console.SetOut")]
    [InlineData("unsafe { int* p = null; }", "unsafe")]
    public async Task Las_API_peligrosas_se_rechazan_antes_de_ejecutar(string code, string api)
    {
        var result = await Run(code);
        Assert.Equal(ExecutionStatus.Rejected, result.Status);
        Assert.Contains(result.Violations, v => v.Api.Contains(api, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Las_API_en_memoria_permitidas_funcionan()
    {
        var code = """
            using System.Text;
            using System.Text.Json;
            var sb = new StringBuilder();
            using var writer = new StringWriter(sb);
            writer.Write(JsonSerializer.Serialize(new { Nombre = "Ada", Edad = 36 }));
            Console.WriteLine(sb.ToString());
            Console.WriteLine(new List<int> { 3, 1, 2 }.Order().First());
            """;
        var result = await Run(code);
        Assert.Equal(ExecutionStatus.Completed, result.Status);
        Assert.Equal("{\"Nombre\":\"Ada\",\"Edad\":36}\n1", OutputComparer.Normalize(result.Tests.Single().Output));
    }

    [Fact]
    public async Task Un_bucle_infinito_se_detiene_por_tiempo()
    {
        var limits = ExecutionLimits.Default with { TestTimeoutMs = 1000 };
        var result = await Run("while (true) { }", limits: limits, tests: [new ExecutionTest("a", "", null), new ExecutionTest("b", "", null)]);
        Assert.True(result.Tests[0].TimedOut);
        Assert.True(result.Tests[1].NotRun);
    }

    [Fact]
    public async Task La_salida_excesiva_se_limita()
    {
        var result = await Run("for (long i = 0; ; i++) Console.WriteLine(i);");
        var test = result.Tests.Single();
        Assert.True(test.OutputLimitExceeded);
        Assert.True(test.Output.Length <= ExecutionLimits.Default.MaxOutputChars);
    }

    [Fact]
    public async Task Un_desbordamiento_de_pila_no_afecta_a_la_aplicacion()
    {
        var code = "public static class S { public static int F(int n) => F(n + 1) + 1; }";
        var result = await Run(code, mode: CodeMode.Function, tests: [new ExecutionTest("rec", null, "S.F(1)")]);
        var failure = result.Tests.Single().Failure;
        Assert.NotNull(failure);
        Assert.Equal("StackOverflowException", failure!.ExceptionType);
        Assert.Equal(ErrorCategory.StackOverflowRecursion, DiagnosticExplainer.ExplainException(failure).Category);

        // El servicio sigue funcionando después de la caída del proceso hijo.
        Assert.Equal(ExecutionStatus.Completed, (await Run("Console.WriteLine(1);")).Status);
    }

    [Fact]
    public async Task El_consumo_excesivo_de_memoria_se_detiene()
    {
        var code = "var bloques = new List<byte[]>(); while (true) bloques.Add(new byte[10_000_000]);";
        var result = await Run(code);
        var test = result.Tests.Single();
        Assert.NotNull(test.Failure);
        Assert.Contains(test.Failure!.ExceptionType, new[] { "OutOfMemoryException", "ProcessCrash" });
    }

    [Fact]
    public async Task Las_excepciones_indican_tipo_y_linea()
    {
        var result = await Run("int[] a = new int[3];\nConsole.WriteLine(a[3]);");
        var failure = result.Tests.Single().Failure!;
        Assert.Equal("IndexOutOfRangeException", failure.ExceptionType);
        Assert.Equal(2, failure.Line);
    }

    [Fact]
    public async Task Soluciones_alternativas_validas_aprueban()
    {
        var exercise = (CodeExercise)CurriculumContent.Catalog.GetItem("n0.aritmetica.indep");
        const string alternative = """
            string texto = Console.ReadLine()!;
            int n = int.Parse(texto);
            int suma = 0;
            suma += n % 10;
            n /= 10;
            suma += n % 10;
            n /= 10;
            suma += n;
            Console.WriteLine(suma);
            """;
        var grading = CodeGrader.Grade(exercise, await Service.ExecuteAsync(CodeGrader.BuildRequest(exercise, alternative)));
        Assert.True(grading.Passed, grading.Summary);
    }

    [Fact]
    public async Task Escribir_la_salida_de_memoria_no_aprueba_las_pruebas_ocultas()
    {
        var exercise = (CodeExercise)CurriculumContent.Catalog.GetItem("n0.aritmetica.indep");
        var grading = CodeGrader.Grade(exercise, await Service.ExecuteAsync(CodeGrader.BuildRequest(exercise, "Console.WriteLine(13);")));
        Assert.False(grading.Passed);
        Assert.Contains(grading.Tests, t => t.Passed && !t.Hidden);
        Assert.Contains(grading.Tests, t => !t.Passed && t.Hidden);
        Assert.All(grading.Tests.Where(t => t.Hidden), t => Assert.Null(t.Expected));
    }

    [Fact]
    public async Task Se_exigen_las_construcciones_pedidas()
    {
        var exercise = new CodeExercise
        {
            Id = "t", SkillId = "n1.for", Purpose = ItemPurpose.Independent, Mode = CodeMode.Function, Prompt = "p", Explanation = "e",
            StarterCode = "", ReferenceSolution = "", RequiredConstructs = [CodeConstruct.Loop],
            Tests = [new CodeTestCase { Name = "suma", Check = "S.Suma(3)", Expected = "6" }],
        };
        var sinBucle = CodeGrader.Grade(exercise, await Service.ExecuteAsync(CodeGrader.BuildRequest(exercise, "public static class S { public static int Suma(int n) => n * (n + 1) / 2; }")));
        Assert.False(sinBucle.Passed);
        Assert.Equal(ErrorCategory.MissingConstruct, sinBucle.Error);

        var conBucle = CodeGrader.Grade(exercise, await Service.ExecuteAsync(CodeGrader.BuildRequest(exercise,
            "public static class S { public static int Suma(int n) { var t = 0; for (var i = 1; i <= n; i++) t += i; return t; } }")));
        Assert.True(conBucle.Passed);
    }

    [Fact]
    public async Task Las_pruebas_de_funcion_comparan_valores_y_excepciones_esperadas()
    {
        var exercise = new CodeExercise
        {
            Id = "t", SkillId = "s", Purpose = ItemPurpose.Independent, Mode = CodeMode.Function, Prompt = "p", Explanation = "e",
            StarterCode = "", ReferenceSolution = "",
            Tests =
            [
                new CodeTestCase { Name = "lista", Check = "S.Pares(5)", Expected = "[0, 2, 4]" },
                new CodeTestCase { Name = "negativo", Check = "S.Pares(-1)", ExpectedException = "ArgumentException" },
                new CodeTestCase { Name = "tupla", Check = "{ var r = S.MinMax(new[] { 4, 1, 9 }); return r; }", Expected = "(1, 9)" },
            ],
        };
        const string code = """
            public static class S
            {
                public static List<int> Pares(int n)
                {
                    if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
                    var r = new List<int>();
                    for (var i = 0; i < n; i += 2) r.Add(i);
                    return r;
                }

                public static (int Min, int Max) MinMax(int[] a) => (a.Min(), a.Max());
            }
            """;
        var grading = CodeGrader.Grade(exercise, await Service.ExecuteAsync(CodeGrader.BuildRequest(exercise, code)));
        Assert.True(grading.Passed, string.Join(" | ", grading.Tests.Select(t => $"{t.Name}: {t.Message} {t.Actual}")));
    }

    [Fact]
    public async Task Un_nombre_incorrecto_en_modo_funcion_se_explica()
    {
        var exercise = new CodeExercise
        {
            Id = "t", SkillId = "s", Purpose = ItemPurpose.Independent, Mode = CodeMode.Function, Prompt = "p", Explanation = "e",
            StarterCode = "", ReferenceSolution = "", Tests = [new CodeTestCase { Name = "a", Check = "Calc.Sumar(1, 2)", Expected = "3" }],
        };
        var grading = CodeGrader.Grade(exercise, await Service.ExecuteAsync(CodeGrader.BuildRequest(exercise, "public static class Calc { public static int Suma(int a, int b) => a + b; }")));
        Assert.False(grading.Passed);
        Assert.Contains(grading.Diagnostics, d => d.Explanation.Contains("nombres de clases y métodos"));
    }

    [Fact]
    public async Task Los_campos_estaticos_se_reinician_entre_casos()
    {
        var code = "public static class C { static int n; public static int Next() => ++n; }";
        var result = await Run(code, mode: CodeMode.Function, tests: [new ExecutionTest("1", null, "C.Next()"), new ExecutionTest("2", null, "C.Next()")]);
        Assert.All(result.Tests, t => Assert.Equal("1", t.ReturnValue));
    }

    [Fact]
    public async Task El_servicio_no_disponible_lo_informa_sin_fingir_resultados()
    {
        var unavailable = new UnavailableExecutionService("Sin motor en esta plataforma.");
        var exercise = (CodeExercise)CurriculumContent.Catalog.GetItem("n0.programa.indep");
        var grading = CodeGrader.Grade(exercise, await unavailable.ExecuteAsync(CodeGrader.BuildRequest(exercise, exercise.ReferenceSolution)));
        Assert.False(grading.Passed);
        Assert.False(grading.Verified);
    }
}

public sealed class FormattingTests
{
    [Theory]
    [InlineData(null, "null")]
    [InlineData(true, "true")]
    [InlineData(2.5, "2.5")]
    [InlineData("hola", "hola")]
    public void Valores_simples(object? value, string expected) => Assert.Equal(expected, ValueFormatter.Format(value));

    [Fact]
    public void Colecciones_diccionarios_y_tuplas()
    {
        Assert.Equal("[1, 2, 3]", ValueFormatter.Format(new List<int> { 1, 2, 3 }));
        Assert.Equal("[\"a\", \"b\"]", ValueFormatter.Format(new[] { "a", "b" }));
        Assert.Equal("{\"x\": 1, \"y\": 2}", ValueFormatter.Format(new Dictionary<string, int> { ["x"] = 1, ["y"] = 2 }));
        Assert.Equal("(1, \"a\")", ValueFormatter.Format((1, "a")));
        Assert.Equal("[]", ValueFormatter.Format(Array.Empty<int>()));
        Assert.Equal("12.5", ValueFormatter.Format(12.5m));
    }

    [Fact]
    public void La_comparacion_de_salida_ignora_espacios_finales_pero_no_el_contenido()
    {
        Assert.True(OutputComparer.AreEquivalent("a\nb", "a \r\nb\r\n\n"));
        Assert.False(OutputComparer.AreEquivalent("a\nb", "a\n b"));
        var diff = OutputComparer.FirstDifference("Total: 5", "total: 5")!;
        Assert.Equal(1, diff.Line);
        Assert.True(diff.FormatOnly);
        Assert.False(OutputComparer.FirstDifference("1\n2", "1\n3")!.FormatOnly);
    }
}
