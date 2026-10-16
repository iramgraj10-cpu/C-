using CSharpMastery.Domain.Common;
using CSharpMastery.Domain.Curriculum;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Domain.Sessions;
using CSharpMastery.Learning.Analytics;

namespace CSharpMastery.Presentation.Formatting;

/// <summary>Textos en español para valores del dominio.</summary>
public static class Labels
{
    public static string Mastery(MasteryLevel level) => level switch
    {
        MasteryLevel.NotStarted => "Sin empezar",
        MasteryLevel.Seen => "Vista",
        MasteryLevel.Practiced => "Practicada",
        MasteryLevel.ProvisionallyMastered => "Dominio provisional",
        MasteryLevel.Retained => "Retenida",
        _ => level.ToString(),
    };

    public static string MasteryHelp(MasteryLevel level) => level switch
    {
        MasteryLevel.NotStarted => "Aún no has estudiado esta habilidad.",
        MasteryLevel.Seen => "Leíste la lección, pero no hay práctica todavía.",
        MasteryLevel.Practiced => "Has practicado, pero falta evidencia suficiente de dominio.",
        MasteryLevel.ProvisionallyMastered => "Cumpliste los criterios. Falta demostrar que la recuerdas tras unos días.",
        MasteryLevel.Retained => "La recordaste correctamente en un repaso posterior.",
        _ => "",
    };

    public static string Purpose(ItemPurpose purpose) => purpose switch
    {
        ItemPurpose.Concept => "Pregunta conceptual",
        ItemPurpose.Prediction => "Predice el resultado",
        ItemPurpose.BugFinding => "Encuentra el error",
        ItemPurpose.Recall => "Recuerda sin apuntes",
        ItemPurpose.Guided => "Ejercicio guiado",
        ItemPurpose.Independent => "Ejercicio sin ayuda",
        ItemPurpose.Transfer => "Aplicación en un contexto nuevo",
        ItemPurpose.Project => "Hito de proyecto",
        _ => purpose.ToString(),
    };

    public static string Block(SessionBlockKind block) => block switch
    {
        SessionBlockKind.Recall => "Recuperación activa",
        SessionBlockKind.NewConcept => "Concepto nuevo",
        SessionBlockKind.GuidedExample => "Ejemplo guiado y predicción",
        SessionBlockKind.Practice => "Práctica",
        SessionBlockKind.Application => "Aplicación sin ayuda",
        SessionBlockKind.WrapUp => "Cierre",
        _ => block.ToString(),
    };

    public static string Tier(Tier tier) => tier switch
    {
        Domain.Curriculum.Tier.Essential => "Fundamental",
        Domain.Curriculum.Tier.Intermediate => "Intermedio",
        Domain.Curriculum.Tier.Advanced => "Avanzado",
        _ => tier.ToString(),
    };

    public static string StartingLevel(StartingLevel level) => level switch
    {
        Domain.Common.StartingLevel.NeverProgrammed => "Nunca he programado",
        Domain.Common.StartingLevel.OtherLanguage => "Programo en otro lenguaje",
        Domain.Common.StartingLevel.SomeCSharp => "Ya usé C# un poco",
        _ => level.ToString(),
    };

    public static string Theme(ThemePreference theme) => theme switch
    {
        ThemePreference.System => "Según el sistema",
        ThemePreference.Light => "Claro",
        ThemePreference.Dark => "Oscuro",
        _ => theme.ToString(),
    };

    public static string Calibration(CalibrationFlag flag) => flag switch
    {
        CalibrationFlag.Overconfident => "Crees dominarla, pero fallas con frecuencia",
        CalibrationFlag.Underconfident => "Aciertas aunque dudas",
        CalibrationFlag.Calibrated => "Confianza acorde a resultados",
        _ => "Pocos datos",
    };

    public static string Duration(TimeSpan span) =>
        span.TotalHours >= 1 ? $"{(int)span.TotalHours} h {span.Minutes} min" : $"{span.Minutes} min {span.Seconds:00} s";

    public static string RelativeDue(DateTimeOffset? due, DateTimeOffset now)
    {
        if (due is null)
        {
            return "Sin repaso programado";
        }

        var days = (due.Value.Date - now.Date).TotalDays;
        return days switch
        {
            <= 0 => "Repaso pendiente hoy",
            1 => "Repaso mañana",
            _ => $"Repaso en {days:0} días",
        };
    }

    public static string Confidence(int value) => value switch
    {
        1 => "Adivinando",
        2 => "Con dudas",
        3 => "Bastante seguro",
        4 => "Totalmente seguro",
        _ => "",
    };
}
