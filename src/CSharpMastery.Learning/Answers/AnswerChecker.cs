using System.Globalization;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;

namespace CSharpMastery.Learning.Answers;

public sealed record AnswerCheck(bool Correct, string Feedback, ErrorCategory Error);

/// <summary>Corrige ítems que no son de código (selección y respuesta corta).</summary>
public static class AnswerChecker
{
    public static AnswerCheck CheckChoice(ChoiceItem item, int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= item.Options.Count)
        {
            return new AnswerCheck(false, "Selecciona una opción.", ErrorCategory.None);
        }

        var option = item.Options[selectedIndex];
        var defaultError = item.Purpose == ItemPurpose.Prediction ? ErrorCategory.PredictionError : option.Error;
        return option.IsCorrect
            ? new AnswerCheck(true, option.Feedback, ErrorCategory.None)
            : new AnswerCheck(false, option.Feedback, option.Error == ErrorCategory.ConceptMisunderstanding ? defaultError : option.Error);
    }

    public static AnswerCheck CheckShortAnswer(ShortAnswerItem item, string? answer)
    {
        var given = answer ?? "";
        foreach (var accepted in item.AcceptedAnswers)
        {
            if (Matches(item.Match, accepted, given))
            {
                return new AnswerCheck(true, "Correcto.", ErrorCategory.None);
            }
        }

        foreach (var (wrong, feedback) in item.KnownWrongAnswers)
        {
            if (Matches(item.Match, wrong, given))
            {
                return new AnswerCheck(false, feedback, Error(item));
            }
        }

        return new AnswerCheck(false, "No es la respuesta esperada. Revisa la explicación.", Error(item));
    }

    private static ErrorCategory Error(ShortAnswerItem item) =>
        item.Purpose == ItemPurpose.Prediction ? ErrorCategory.PredictionError : ErrorCategory.ConceptMisunderstanding;

    public static bool Matches(AnswerMatch match, string expected, string given) => match switch
    {
        AnswerMatch.Output => Normalize(expected) == Normalize(given),
        AnswerMatch.Number => TryNumber(expected, out var e) && TryNumber(given, out var g) && Math.Abs(e - g) < 1e-9,
        _ => string.Equals(Collapse(expected), Collapse(given), StringComparison.OrdinalIgnoreCase),
    };

    private static string Collapse(string s) => string.Join(' ', s.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string Normalize(string s)
    {
        var lines = s.Replace("\r\n", "\n").Split('\n').Select(l => l.TrimEnd()).ToList();
        while (lines.Count > 0 && lines[^1].Length == 0)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        while (lines.Count > 0 && lines[0].Length == 0)
        {
            lines.RemoveAt(0);
        }

        return string.Join('\n', lines);
    }

    private static bool TryNumber(string s, out double value) =>
        double.TryParse(s.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
