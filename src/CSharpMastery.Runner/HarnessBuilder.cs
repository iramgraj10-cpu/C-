using System.Text;
using CSharpMastery.Execution;

namespace CSharpMastery.Runner;

/// <summary>
/// Genera el arnés de pruebas para ejercicios en modo función. Cada caso se convierte en un método
/// estático que devuelve el valor a comparar. Si la comprobación empieza con <c>{</c> se trata como bloque.
/// </summary>
internal static class HarnessBuilder
{
    public static string Build(IReadOnlyList<ExecutionTest> tests)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable disable");
        sb.AppendLine($"public static class {CodeCompiler.HarnessClassName}");
        sb.AppendLine("{");
        for (var i = 0; i < tests.Count; i++)
        {
            var check = tests[i].Check?.Trim();
            if (string.IsNullOrEmpty(check))
            {
                check = "null";
            }

            sb.AppendLine($"    public static object {MethodName(i)}()");
            if (check.StartsWith('{'))
            {
                sb.AppendLine(check);
            }
            else
            {
                sb.AppendLine("    {");
                sb.AppendLine($"        return (object)({check});");
                sb.AppendLine("    }");
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    public static string MethodName(int index) => $"__Test{index}";
}
