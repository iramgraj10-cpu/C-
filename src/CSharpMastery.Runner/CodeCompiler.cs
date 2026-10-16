using System.Collections.Immutable;
using System.Text;
using CSharpMastery.Domain.Items;
using CSharpMastery.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace CSharpMastery.Runner;

/// <summary>Compila el código del estudiante con Roslyn.</summary>
internal static class CodeCompiler
{
    public const string StudentFileName = "Solucion.cs";
    public const string HarnessFileName = "Pruebas.cs";
    public const string HarnessClassName = "__CsmHarness";

    private const string GlobalUsings = """
        global using System;
        global using System.Collections.Generic;
        global using System.IO;
        global using System.Linq;
        global using System.Threading;
        global using System.Threading.Tasks;
        """;

    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest, DocumentationMode.None);

    private static readonly Lazy<ImmutableArray<MetadataReference>> References = new(LoadReferences);

    public static void Warmup()
    {
        Compile(new ExecutionRequest
        {
            Code = "var x = new List<int> { 1, 2 }.Sum(); Console.WriteLine($\"{x}\");",
            Mode = CodeMode.Program,
        });
    }

    public static CompilationOutcome Compile(ExecutionRequest request)
    {
        var studentTree = CSharpSyntaxTree.ParseText(SourceText.From(request.Code, Encoding.UTF8), ParseOptions, StudentFileName);
        var usingsTree = CSharpSyntaxTree.ParseText(SourceText.From(GlobalUsings, Encoding.UTF8), ParseOptions, "GlobalUsings.cs");
        var trees = new List<SyntaxTree> { studentTree, usingsTree };

        if (request.Mode == CodeMode.Function)
        {
            var harness = HarnessBuilder.Build(request.Tests);
            trees.Add(CSharpSyntaxTree.ParseText(SourceText.From(harness, Encoding.UTF8), ParseOptions, HarnessFileName));
        }

        var options = new CSharpCompilationOptions(
                request.Mode == CodeMode.Program ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary)
            .WithOptimizationLevel(OptimizationLevel.Debug)
            .WithAllowUnsafe(false)
            .WithNullableContextOptions(NullableContextOptions.Enable)
            .WithConcurrentBuild(false);

        var compilation = CSharpCompilation.Create("Solucion_" + Guid.NewGuid().ToString("N"), trees, References.Value, options);

        var semanticModel = compilation.GetSemanticModel(studentTree);
        var constructs = ConstructDetector.Detect(studentTree, semanticModel);

        var diagnostics = compilation.GetDiagnostics()
            .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
            .Where(d => d.Location.SourceTree != usingsTree)
            .Select(ToDiagnostic)
            .Distinct()
            .Take(40)
            .ToList();

        // El análisis de API solo se aplica al código del estudiante (el arnés de pruebas es de confianza).
        var violations = ApiPolicy.Analyze(studentTree, semanticModel, compilation.Assembly);

        if (diagnostics.Any(d => d.Severity == "Error") || violations.Count > 0)
        {
            return new CompilationOutcome(null, null, diagnostics, violations, constructs);
        }

        using var peStream = new MemoryStream();
        using var pdbStream = new MemoryStream();
        var emit = compilation.Emit(peStream, pdbStream, options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb));
        if (!emit.Success)
        {
            var emitDiagnostics = emit.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(ToDiagnostic).ToList();
            return new CompilationOutcome(null, null, diagnostics.Concat(emitDiagnostics).ToList(), violations, constructs);
        }

        return new CompilationOutcome(peStream.ToArray(), pdbStream.ToArray(), diagnostics, violations, constructs);
    }

    private static CompilerDiagnostic ToDiagnostic(Diagnostic d)
    {
        var span = d.Location.GetLineSpan();
        var inHarness = span.Path == HarnessFileName;
        var line = d.Location.IsInSource && !inHarness ? span.StartLinePosition.Line + 1 : 0;
        var column = d.Location.IsInSource && !inHarness ? span.StartLinePosition.Character + 1 : 0;
        var message = d.GetMessage(System.Globalization.CultureInfo.InvariantCulture);
        if (inHarness)
        {
            message = "[pruebas] " + message;
        }

        return new CompilerDiagnostic(d.Id, d.Severity.ToString(), message, line, column);
    }

    private static ImmutableArray<MetadataReference> LoadReferences()
    {
        var tpa = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? "")
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        return
        [
            .. tpa
                .Where(path =>
                {
                    var name = Path.GetFileNameWithoutExtension(path);
                    return name.StartsWith("System.", StringComparison.Ordinal)
                        || name is "System" or "mscorlib" or "netstandard";
                })
                .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)),
        ];
    }
}
