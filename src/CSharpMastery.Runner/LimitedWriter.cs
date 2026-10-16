using System.Text;

namespace CSharpMastery.Runner;

/// <summary>Captura la salida del estudiante y la corta al superar el límite.</summary>
internal sealed class LimitedWriter(int maxChars) : TextWriter
{
    private readonly StringBuilder _buffer = new();
    private readonly Lock _gate = new();

    public bool LimitExceeded { get; private set; }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        lock (_gate)
        {
            if (_buffer.Length >= maxChars)
            {
                LimitExceeded = true;
                throw new OutputLimitExceededException();
            }

            _buffer.Append(value);
        }
    }

    public override void Write(string? value)
    {
        if (value is null)
        {
            return;
        }

        lock (_gate)
        {
            if (_buffer.Length + value.Length > maxChars)
            {
                _buffer.Append(value.AsSpan(0, Math.Max(0, maxChars - _buffer.Length)));
                LimitExceeded = true;
                throw new OutputLimitExceededException();
            }

            _buffer.Append(value);
        }
    }

    public string Captured
    {
        get
        {
            lock (_gate)
            {
                return _buffer.ToString();
            }
        }
    }
}

internal sealed class OutputLimitExceededException() : Exception("Se superó el límite de salida de la consola.");
