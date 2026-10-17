using CSharpMastery.Learning.Interactive;

namespace CSharpMastery.Presentation.ViewModels.Activity;

/// <summary>
/// Qué mascota aparece en cada paso. El robot es la principal y sale casi siempre; las fotos aparecen de vez en
/// cuando: dormida en algunas tarjetas con mucho texto, pensando en algunas preguntas y de frente en algunos saludos.
/// La elección es determinista (el mismo paso muestra siempre la misma mascota).
/// </summary>
public static class Mascots
{
    public const string Robot = "mascot_happy.png";
    public const string RobotParty = "mascot_party.png";
    public const string Photo = "mascot_main.png";
    public const string Sleeping = "mascot_sleep.png";
    public const string Thinking = "mascot_think.png";

    /// <summary>Tarjetas: la foto dormida en la mitad de las tarjetas largas.</summary>
    public static string ForReading(string key, bool longText) => longText && ClozeGenerator.StableHash(key) % 2 == 0 ? Sleeping : Robot;

    /// <summary>Preguntas y ejercicios: la foto pensando en uno de cada tres.</summary>
    public static string ForThinking(string key) => ClozeGenerator.StableHash(key) % 3 == 0 ? Thinking : Robot;

    /// <summary>Portadas y recorridos: la foto de frente en uno de cada cuatro.</summary>
    public static string ForGreeting(string key) => ClozeGenerator.StableHash(key) % 4 == 0 ? Photo : Robot;
}
