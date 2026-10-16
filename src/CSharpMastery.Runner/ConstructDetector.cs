using CSharpMastery.Domain.Items;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpMastery.Runner;

/// <summary>Detecta qué construcciones del lenguaje usa el código del estudiante.</summary>
internal static class ConstructDetector
{
    public static IReadOnlyList<CodeConstruct> Detect(SyntaxTree tree, SemanticModel model)
    {
        var found = new HashSet<CodeConstruct>();
        var root = tree.GetRoot();

        foreach (var node in root.DescendantNodes())
        {
            switch (node)
            {
                case ForStatementSyntax or WhileStatementSyntax or DoStatementSyntax or ForEachStatementSyntax:
                    found.Add(CodeConstruct.Loop);
                    break;
                case IfStatementSyntax or ConditionalExpressionSyntax:
                    found.Add(CodeConstruct.Conditional);
                    break;
                case SwitchStatementSyntax or SwitchExpressionSyntax:
                    found.Add(CodeConstruct.Switch);
                    found.Add(CodeConstruct.Conditional);
                    break;
                case MethodDeclarationSyntax or LocalFunctionStatementSyntax:
                    found.Add(CodeConstruct.Method);
                    break;
                case ClassDeclarationSyntax:
                    found.Add(CodeConstruct.Class);
                    break;
                case InterfaceDeclarationSyntax:
                    found.Add(CodeConstruct.Interface);
                    break;
                case RecordDeclarationSyntax:
                    found.Add(CodeConstruct.Record);
                    break;
                case LambdaExpressionSyntax or AnonymousMethodExpressionSyntax:
                    found.Add(CodeConstruct.Lambda);
                    break;
                case QueryExpressionSyntax:
                    found.Add(CodeConstruct.Linq);
                    break;
                case TryStatementSyntax:
                    found.Add(CodeConstruct.TryCatch);
                    break;
                case AwaitExpressionSyntax:
                    found.Add(CodeConstruct.Async);
                    break;
                case TypeParameterListSyntax:
                    found.Add(CodeConstruct.Generic);
                    break;
                case InvocationExpressionSyntax invocation:
                    if (model.GetSymbolInfo(invocation).Symbol is IMethodSymbol { ContainingType: { } t } &&
                        t.ContainingNamespace?.ToDisplayString() == "System.Linq" &&
                        t.Name is "Enumerable" or "Queryable")
                    {
                        found.Add(CodeConstruct.Linq);
                    }

                    if (IsRecursiveCall(invocation))
                    {
                        found.Add(CodeConstruct.Recursion);
                    }

                    break;
            }

            if (node is MemberDeclarationSyntax member && member.Modifiers.Any(SyntaxKind.AsyncKeyword))
            {
                found.Add(CodeConstruct.Async);
            }
        }

        return [.. found.OrderBy(c => c)];
    }

    private static bool IsRecursiveCall(InvocationExpressionSyntax invocation)
    {
        var calledName = invocation.Expression switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
            GenericNameSyntax g => g.Identifier.Text,
            _ => null,
        };
        if (calledName is null)
        {
            return false;
        }

        foreach (var ancestor in invocation.Ancestors())
        {
            switch (ancestor)
            {
                case MethodDeclarationSyntax m when m.Identifier.Text == calledName:
                case LocalFunctionStatementSyntax f when f.Identifier.Text == calledName:
                    return true;
                case MethodDeclarationSyntax or LocalFunctionStatementSyntax:
                    return false;
            }
        }

        return false;
    }
}
