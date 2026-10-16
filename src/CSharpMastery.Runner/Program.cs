using System.Text;
using System.Text.Json;
using CSharpMastery.Execution;
using CSharpMastery.Runner;

// Proceso ejecutor de un solo uso. Protocolo:
// 1. Precalienta el compilador y escribe {"type":"ready"}.
// 2. Lee una ExecutionRequest (JSON) completa desde la entrada estándar.
// 3. Escribe una línea JSON por evento (compilación, cada prueba, fin) en la salida estándar real.
// El código del estudiante nunca ve la salida estándar real: su Console.Out se redirige.
var protocolOut = new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(false)) { AutoFlush = true };
var protocol = new ProtocolWriter(protocolOut);

try
{
    CodeCompiler.Warmup();
    protocol.Write(new RunnerMessage { Type = RunnerMessage.Ready });

    string json;
    using (var stdin = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8))
    {
        json = await stdin.ReadToEndAsync();
    }

    var request = JsonSerializer.Deserialize<ExecutionRequest>(json, ProtocolJson.Options)
        ?? throw new InvalidOperationException("Solicitud vacía.");

    var exitCode = new RunnerHost(protocol).Run(request);
    protocolOut.Flush();
    Environment.Exit(exitCode);
}
catch (Exception ex)
{
    protocol.Write(new RunnerMessage { Type = RunnerMessage.Done, Status = ExecutionStatus.InternalError, Message = ex.ToString() });
    protocolOut.Flush();
    Environment.Exit(2);
}
