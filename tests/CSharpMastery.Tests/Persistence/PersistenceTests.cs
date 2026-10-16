using CSharpMastery.Domain.Common;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Learning.Services;
using CSharpMastery.Persistence;
using Microsoft.Data.Sqlite;

namespace CSharpMastery.Tests.Persistence;

public sealed class PersistenceTests : IDisposable
{
    private readonly TempDatabase _db = new();
    private static readonly DateTimeOffset T0 = new(2026, 2, 1, 10, 0, 0, TimeSpan.Zero);

    public void Dispose() => _db.Dispose();

    [Fact]
    public void Las_migraciones_crean_el_esquema_completo()
    {
        var store = _db.Open();
        Assert.Equal(Migrations.LatestVersion, store.SchemaVersion);
        Assert.Equal(2, store.SchemaVersion);
    }

    [Fact]
    public async Task Migrar_de_la_version_1_a_la_2_conserva_los_datos()
    {
        using (var connection = new SqliteConnection($"Data Source={_db.Path};Pooling=False"))
        {
            connection.Open();
            Assert.Equal(1, Migrations.Apply(connection, targetVersion: 1));
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO settings(key, value) VALUES ('tema', 'oscuro');";
            cmd.ExecuteNonQuery();
        }

        var store = _db.Open();
        Assert.Equal(2, store.SchemaVersion);
        Assert.Equal("oscuro", await store.GetSettingAsync("tema"));
        await store.AddSubmissionAsync(new CodeSubmission(Guid.NewGuid(), "i", "code", true, 1, ErrorCategory.None, "ok", T0));
        Assert.Single(await store.GetSubmissionsAsync("i"));
    }

    [Fact]
    public void Una_base_de_datos_mas_nueva_se_rechaza()
    {
        using (var connection = new SqliteConnection($"Data Source={_db.Path};Pooling=False"))
        {
            connection.Open();
            Migrations.Apply(connection);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO schema_migrations(version, description, applied_at) VALUES (99, 'futura', 'x');";
            cmd.ExecuteNonQuery();
        }

        Assert.Throws<InvalidOperationException>(() => _db.Open());
    }

    [Fact]
    public async Task Los_datos_se_guardan_y_leen_sin_perdida()
    {
        var store = _db.Open();
        var progress = SkillProgress.Initial("s") with
        {
            Level = MasteryLevel.Retained,
            Difficulty = 3,
            Review = new ReviewSchedule(2.36, 15, 3, 1, T0.AddDays(15), T0),
            SeenAt = T0.AddDays(-10),
            FirstPracticedAt = T0.AddDays(-9),
            ProvisionalMasteryAt = T0.AddDays(-5),
            RetainedAt = T0,
            NeedsReinforcement = true,
            ReinforcementSince = T0.AddDays(-1),
            DifficultyChangedAt = T0.AddDays(-2),
        };
        await store.SaveSkillAsync(progress);
        Assert.Equal(progress, await store.GetSkillAsync("s"));

        var attempt = Attempts.Make("s", "i", false, T0, confidence: 3, error: ErrorCategory.IndexOutOfRange, contextId: "ctx") with
        {
            RelatedSkillIds = ["a", "b"],
            Answer = "código",
            Tries = 4,
        };
        await store.AddAttemptAsync(attempt);
        var read = Assert.Single(await store.GetAttemptsAsync(contextId: "ctx"));
        Assert.Equal(attempt.Id, read.Id);
        Assert.Equal(attempt.RelatedSkillIds, read.RelatedSkillIds);
        Assert.Equal(attempt with { RelatedSkillIds = read.RelatedSkillIds }, read);

        var profile = new LearnerProfile("Ada", StartingLevel.OtherLanguage, ["Crear apps"], 45, ThemePreference.Dark, T0);
        await store.SaveProfileAsync(profile);
        var readProfile = await store.GetProfileAsync();
        Assert.Equal(profile.DisplayName, readProfile!.DisplayName);
        Assert.Equal(profile.Goals, readProfile.Goals);

        await store.SaveDraftAsync("i", "borrador 1");
        await store.SaveDraftAsync("i", "borrador 2");
        Assert.Equal("borrador 2", await store.GetDraftAsync("i"));
    }

    [Fact]
    public async Task Exportar_y_restaurar_reproduce_el_progreso()
    {
        var source = _db.Open();
        await source.SaveProfileAsync(new LearnerProfile("Ada", StartingLevel.NeverProgrammed, [], 25, ThemePreference.Light, T0));
        await source.SaveSkillAsync(SkillProgress.Initial("s") with { Level = MasteryLevel.Practiced });
        await source.AddAttemptAsync(Attempts.Make("s", "i", true, T0));
        await source.SetSettingAsync("k", "v");
        await source.SaveProjectAsync(new ProjectProgress("p", new Dictionary<string, int> { ["Diseño"] = 2 }, "notas", true, T0));

        var json = SqliteProgressStore.SerializeExport(await source.ExportAsync(T0));

        using var otherDb = new TempDatabase();
        var target = otherDb.Open();
        await target.AddAttemptAsync(Attempts.Make("otra", "x", true, T0));
        await target.ImportAsync(SqliteProgressStore.DeserializeExport(json));

        Assert.Equal("Ada", (await target.GetProfileAsync())!.DisplayName);
        Assert.Equal(MasteryLevel.Practiced, (await target.GetSkillAsync("s")).Level);
        Assert.Single(await target.GetAttemptsAsync());
        Assert.Equal("v", await target.GetSettingAsync("k"));
        Assert.Equal(2, (await target.GetProjectsAsync()).Single().RubricSelfAssessment["Diseño"]);
    }

    [Fact]
    public async Task Un_respaldo_de_una_version_futura_se_rechaza()
    {
        var store = _db.Open();
        var export = await store.ExportAsync(T0) with { FormatVersion = ProgressExport.CurrentFormatVersion + 1 };
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.ImportAsync(export));
        Assert.Throws<InvalidDataException>(() => SqliteProgressStore.DeserializeExport("null"));
    }
}
