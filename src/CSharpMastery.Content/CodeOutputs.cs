using System.Security.Cryptography;
using System.Text;

namespace CSharpMastery.Content;

/// <summary>
/// Salida real (ejecutada) de los bloques de código de las explicaciones. Los ejercicios de completar huecos la
/// muestran en el enunciado: sin ella, una ficha que compila y cambia el resultado sería tan válida como la correcta.
/// Las entradas viven en <see cref="CodeOutputData"/>, generada por <c>scripts/update-code-outputs.sh</c>.
/// </summary>
public static class CodeOutputs
{
    /// <summary>Clave estable de un fragmento: SHA-256 (16 primeros dígitos hexadecimales) del código normalizado.</summary>
    public static string Key(string code)
    {
        var normalized = code.Replace("\r\n", "\n").TrimEnd();
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)))[..16];
    }

    /// <summary>Salida registrada del fragmento, o null si no se conoce (o el programa no muestra nada).</summary>
    public static string? For(string code) => CodeOutputData.ByKey.TryGetValue(Key(code), out var output) ? output : null;
}
