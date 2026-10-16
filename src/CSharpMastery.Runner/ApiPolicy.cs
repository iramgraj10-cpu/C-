using CSharpMastery.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpMastery.Runner;

/// <summary>
/// Lista de permitidos de API para el código del estudiante. Es una capa de defensa en profundidad
/// (junto con el proceso separado, el límite de tiempo, de memoria y de salida), no un sandbox del sistema operativo.
/// Se rechaza: archivos, red, procesos, reflexión, interoperabilidad nativa, código inseguro y control del entorno.
/// </summary>
internal static class ApiPolicy
{
    private static readonly HashSet<string> AllowedNamespaces =
    [
        "System",
        "System.Collections",
        "System.Collections.Generic",
        "System.Collections.Immutable",
        "System.Collections.ObjectModel",
        "System.Collections.Concurrent",
        "System.Linq",
        "System.Linq.Expressions",
        "System.Text",
        "System.Text.RegularExpressions",
        "System.Text.Json",
        "System.Text.Json.Nodes",
        "System.Text.Json.Serialization",
        "System.Threading",
        "System.Threading.Tasks",
        "System.Numerics",
        "System.Globalization",
        "System.Diagnostics.CodeAnalysis",
        "System.ComponentModel.DataAnnotations",
    ];

    /// <summary>Tipos permitidos fuera de los espacios de nombres anteriores (operan solo en memoria).</summary>
    private static readonly HashSet<string> AllowedExtraTypes =
    [
        "System.IO.StringReader",
        "System.IO.StringWriter",
        "System.IO.TextReader",
        "System.IO.TextWriter",
        "System.IO.MemoryStream",
        "System.IO.Stream",
        "System.IO.SeekOrigin",
        "System.IO.BinaryReader",
        "System.IO.BinaryWriter",
        "System.IO.IOException",
        "System.IO.InvalidDataException",
        "System.IO.EndOfStreamException",
        "System.IO.FileNotFoundException",
        "System.IO.DirectoryNotFoundException",
        "System.Diagnostics.Stopwatch",
        "System.Diagnostics.Debug",
        "System.Runtime.CompilerServices.ITuple",
    ];

    /// <summary>Tipos prohibidos aunque estén en un espacio de nombres permitido.</summary>
    private static readonly Dictionary<string, string> DeniedTypes = new()
    {
        ["System.AppDomain"] = "control del proceso",
        ["System.AppContext"] = "configuración del proceso",
        ["System.Activator"] = "creación dinámica de tipos",
        ["System.GC"] = "control del recolector de basura",
        ["System.TypedReference"] = "referencias no seguras",
        ["System.ArgIterator"] = "referencias no seguras",
        ["System.RuntimeTypeHandle"] = "reflexión",
        ["System.RuntimeMethodHandle"] = "reflexión",
        ["System.RuntimeFieldHandle"] = "reflexión",
        ["System.Threading.ExecutionContext"] = "control del contexto de ejecución",
        ["System.Threading.ThreadPool"] = "control del grupo de hilos",
        ["System.Linq.Expressions.Expression"] = "generación dinámica de código",
        ["System.Text.Json.Serialization.Metadata.JsonTypeInfo"] = "reflexión",
    };

    /// <summary>Tipos de los que solo se permiten ciertos miembros.</summary>
    private static readonly Dictionary<string, HashSet<string>> RestrictedMembers = new()
    {
        ["System.Environment"] = ["NewLine"],
        ["System.Console"] =
        [
            "Write", "WriteLine", "ReadLine", "Read", "Out", "Error", "In",
            "ForegroundColor", "BackgroundColor", "ResetColor", "Clear",
        ],
        ["System.Type"] =
        [
            "Name", "FullName", "Namespace", "IsValueType", "IsClass", "IsInterface", "IsEnum",
            "IsGenericType", "IsArray", "BaseType", "ToString", "Equals", "GetHashCode", "op_Equality", "op_Inequality",
        ],
        ["System.Delegate"] = ["Invoke", "Combine", "Remove", "Equals", "GetHashCode", "ToString", "op_Equality", "op_Inequality"],
    };

    public static IReadOnlyList<SecurityViolation> Analyze(SyntaxTree tree, SemanticModel model, IAssemblySymbol studentAssembly)
    {
        var violations = new List<SecurityViolation>();
        var root = tree.GetRoot();

        foreach (var node in root.DescendantNodesAndTokens())
        {
            if (node.IsToken)
            {
                var token = node.AsToken();
                if (token.IsKind(SyntaxKind.UnsafeKeyword) || token.IsKind(SyntaxKind.ExternKeyword) ||
                    token.IsKind(SyntaxKind.StackAllocKeyword) || token.IsKind(SyntaxKind.FixedKeyword) ||
                    token.IsKind(SyntaxKind.MakeRefKeyword) || token.IsKind(SyntaxKind.RefValueKeyword) ||
                    token.IsKind(SyntaxKind.RefTypeKeyword) || token.IsKind(SyntaxKind.ArgListKeyword))
                {
                    Add(violations, token.GetLocation(), token.Text, "Código inseguro o de bajo nivel no permitido en los ejercicios.");
                }

                continue;
            }

            var syntax = node.AsNode()!;
            if (syntax is FunctionPointerTypeSyntax or PointerTypeSyntax)
            {
                Add(violations, syntax.GetLocation(), syntax.ToString(), "Los punteros no están permitidos en los ejercicios.");
                continue;
            }

            if (syntax is not (ExpressionSyntax or TypeSyntax))
            {
                continue;
            }

            var info = model.GetSymbolInfo(syntax);
            var symbol = info.Symbol ?? (info.CandidateSymbols.Length == 1 ? info.CandidateSymbols[0] : null);
            if (symbol is not null)
            {
                CheckSymbol(symbol, syntax, studentAssembly, violations);
            }

            if (syntax is ExpressionSyntax expr)
            {
                var type = model.GetTypeInfo(expr).Type;
                if (type is not null)
                {
                    CheckType(type, syntax, studentAssembly, violations);
                }
            }
        }

        return violations
            .GroupBy(v => (v.Line, v.Api))
            .Select(g => g.First())
            .OrderBy(v => v.Line)
            .Take(20)
            .ToList();
    }

    private static void CheckSymbol(ISymbol symbol, SyntaxNode node, IAssemblySymbol studentAssembly, List<SecurityViolation> violations)
    {
        switch (symbol)
        {
            case ITypeSymbol type:
                CheckType(type, node, studentAssembly, violations);
                break;
            case IMethodSymbol or IPropertySymbol or IFieldSymbol or IEventSymbol:
            {
                var containing = symbol.ContainingType;
                if (containing is null)
                {
                    break;
                }

                CheckType(containing, node, studentAssembly, violations);
                var fullName = FullName(containing.OriginalDefinition);
                if (RestrictedMembers.TryGetValue(fullName, out var allowed) && !allowed.Contains(symbol.Name))
                {
                    Add(violations, node.GetLocation(), $"{containing.Name}.{symbol.Name}",
                        $"El miembro {containing.Name}.{symbol.Name} no está disponible en los ejercicios.");
                }

                if (symbol is IMethodSymbol method)
                {
                    CheckType(method.ReturnType, node, studentAssembly, violations);
                    if (method.GetAttributes().Any(a => a.AttributeClass?.Name == "DllImportAttribute"))
                    {
                        Add(violations, node.GetLocation(), method.Name, "La interoperabilidad nativa no está permitida.");
                    }
                }

                break;
            }
        }
    }

    private static void CheckType(ITypeSymbol type, SyntaxNode node, IAssemblySymbol studentAssembly, List<SecurityViolation> violations)
    {
        switch (type)
        {
            case IArrayTypeSymbol array:
                CheckType(array.ElementType, node, studentAssembly, violations);
                return;
            case IPointerTypeSymbol or IFunctionPointerTypeSymbol:
                Add(violations, node.GetLocation(), type.ToDisplayString(), "Los punteros no están permitidos en los ejercicios.");
                return;
            case IDynamicTypeSymbol:
                Add(violations, node.GetLocation(), "dynamic", "El tipo dynamic no está permitido en los ejercicios.");
                return;
            case ITypeParameterSymbol or IErrorTypeSymbol:
                return;
            case INamedTypeSymbol named:
            {
                if (SymbolEqualityComparer.Default.Equals(named.ContainingAssembly, studentAssembly))
                {
                    return;
                }

                foreach (var argument in named.TypeArguments)
                {
                    CheckType(argument, node, studentAssembly, violations);
                }

                var outer = named.OriginalDefinition;
                while (outer.ContainingType is not null)
                {
                    outer = outer.ContainingType;
                }

                var fullName = FullName(outer);
                if (DeniedTypes.TryGetValue(fullName, out var reason))
                {
                    Add(violations, node.GetLocation(), fullName, $"{fullName} no está permitido ({reason}).");
                    return;
                }

                if (AllowedExtraTypes.Contains(fullName))
                {
                    return;
                }

                var ns = outer.ContainingNamespace?.ToDisplayString() ?? "";
                if (!AllowedNamespaces.Contains(ns))
                {
                    Add(violations, node.GetLocation(), fullName,
                        $"{fullName} pertenece a {ns}, que no está disponible en el entorno de ejercicios (archivos, red, procesos y reflexión están bloqueados).");
                }

                return;
            }
        }
    }

    private static string FullName(INamedTypeSymbol type)
    {
        var ns = type.ContainingNamespace is { IsGlobalNamespace: false } n ? n.ToDisplayString() + "." : "";
        return ns + type.MetadataName.Split('`')[0];
    }

    private static void Add(List<SecurityViolation> list, Location location, string api, string reason)
    {
        var line = location.GetLineSpan().StartLinePosition.Line + 1;
        list.Add(new SecurityViolation(line, api, reason));
    }
}
