namespace CSharpMastery.Domain.Common;

public enum StartingLevel
{
    /// <summary>Nunca ha programado.</summary>
    NeverProgrammed,
    /// <summary>Conoce otro lenguaje, pero no C#.</summary>
    OtherLanguage,
    /// <summary>Ha usado C# de forma básica.</summary>
    SomeCSharp,
}

public enum ThemePreference
{
    System,
    Light,
    Dark,
}

/// <summary>Perfil local. No requiere cuenta ni conexión.</summary>
public sealed record LearnerProfile(
    string DisplayName,
    StartingLevel InitialLevel,
    IReadOnlyList<string> Goals,
    int PreferredSessionMinutes,
    ThemePreference Theme,
    DateTimeOffset CreatedAt)
{
    public static LearnerProfile CreateDefault(DateTimeOffset now) =>
        new("Estudiante", StartingLevel.NeverProgrammed, [], 45, ThemePreference.System, now);
}

/// <summary>Abstracción del reloj para que la lógica temporal sea comprobable.</summary>
public interface IClock
{
    DateTimeOffset Now { get; }
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
