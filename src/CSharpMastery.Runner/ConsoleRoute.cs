using System.Text;

namespace CSharpMastery.Runner;

/// <summary>
/// Consola por hilo de ejecución: cada ejecución de código del estudiante escribe y lee en sus propios
/// flujos, aunque haya terminado por tiempo y su hilo siga vivo. Sin esto, un hilo abandonado tras un
/// límite de tiempo podía escribir en la salida de la ejecución siguiente (pasaba con el ejecutor en proceso).
/// </summary>
internal static class ConsoleRoute
{
    private static readonly AsyncLocal<TextWriter?> CurrentOut = new();
    private static readonly AsyncLocal<TextReader?> CurrentIn = new();

    /// <summary>Asigna los flujos de la ejecución actual (se heredan en los hilos y tareas que cree).</summary>
    public static void Bind(TextWriter output, TextReader input)
    {
        CurrentOut.Value = output;
        CurrentIn.Value = input;
    }

    public static TextWriter Writer(TextWriter fallback) => new RoutedWriter(fallback);

    public static TextReader Reader(TextReader fallback) => new RoutedReader(fallback);

    private sealed class RoutedWriter(TextWriter fallback) : TextWriter
    {
        private TextWriter Target => CurrentOut.Value ?? fallback;

        public override Encoding Encoding => Target.Encoding;

        public override void Write(char value) => Target.Write(value);

        public override void Write(string? value) => Target.Write(value);

        public override void Write(char[] buffer, int index, int count) => Target.Write(buffer, index, count);

        public override void WriteLine(string? value) => Target.WriteLine(value);

        public override void WriteLine() => Target.WriteLine();

        public override void Flush() => Target.Flush();
    }

    private sealed class RoutedReader(TextReader fallback) : TextReader
    {
        private TextReader Source => CurrentIn.Value ?? fallback;

        public override int Peek() => Source.Peek();

        public override int Read() => Source.Read();

        public override string? ReadLine() => Source.ReadLine();

        public override string ReadToEnd() => Source.ReadToEnd();
    }
}
