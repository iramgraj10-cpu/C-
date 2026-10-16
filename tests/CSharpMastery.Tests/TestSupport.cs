using CSharpMastery.Domain.Common;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Execution;
using CSharpMastery.Persistence;

namespace CSharpMastery.Tests;

public sealed class FakeClock(DateTimeOffset start) : IClock
{
    public DateTimeOffset Now { get; set; } = start;

    public void Advance(TimeSpan by) => Now += by;

    public static FakeClock At2026() => new(new DateTimeOffset(2026, 3, 2, 9, 0, 0, TimeSpan.Zero));
}

public sealed class TempDatabase : IDisposable
{
    public TempDatabase()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"csm-test-{Guid.NewGuid():N}.db");
    }

    public string Path { get; }

    public SqliteProgressStore Open() => new(Path);

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            try
            {
                File.Delete(Path + suffix);
            }
            catch (IOException)
            {
            }
        }
    }
}

public static class Attempts
{
    public static AttemptRecord Make(
        string skill, string item, bool correct, DateTimeOffset at, ItemPurpose purpose = ItemPurpose.Independent,
        bool code = true, int hints = 0, bool solution = false, AttemptContext context = AttemptContext.Session,
        int? confidence = null, ErrorCategory error = ErrorCategory.None, bool autoGraded = true, string? contextId = null) =>
        new()
        {
            ItemId = item,
            SkillId = skill,
            Purpose = purpose,
            Context = context,
            ContextId = contextId,
            Correct = correct,
            Score = correct ? 1 : 0,
            HintsUsed = hints,
            SolutionViewed = solution,
            Duration = TimeSpan.FromMinutes(2),
            Confidence = confidence,
            Error = correct ? ErrorCategory.None : (error == ErrorCategory.None ? ErrorCategory.WrongLogic : error),
            AutoGraded = autoGraded,
            IsCode = code,
            At = at,
        };
}

/// <summary>Un único servicio de ejecución por proceso de pruebas (lanza el ejecutor real como proceso hijo).</summary>
public static class Runners
{
    private static readonly Lazy<ProcessExecutionService> Shared = new(() =>
        new ProcessExecutionService(RunnerLaunchInfo.ForDll(System.IO.Path.Combine(AppContext.BaseDirectory, "CSharpMastery.Runner.dll"))));

    public static ProcessExecutionService Process => Shared.Value;
}
