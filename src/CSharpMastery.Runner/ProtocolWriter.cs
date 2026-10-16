using System.Text.Json;
using CSharpMastery.Execution;

namespace CSharpMastery.Runner;

internal sealed class ProtocolWriter(TextWriter output)
{
    private readonly Lock _gate = new();

    public void Write(RunnerMessage message)
    {
        var line = JsonSerializer.Serialize(message, ProtocolJson.Options);
        lock (_gate)
        {
            output.WriteLine(line);
            output.Flush();
        }
    }
}
