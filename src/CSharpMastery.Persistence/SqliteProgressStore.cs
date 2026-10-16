using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpMastery.Domain.Assessments;
using CSharpMastery.Domain.Common;
using CSharpMastery.Domain.Items;
using CSharpMastery.Domain.Progress;
using CSharpMastery.Domain.Sessions;
using CSharpMastery.Learning.Services;
using Microsoft.Data.Sqlite;

namespace CSharpMastery.Persistence;

/// <summary>
/// Almacén local en SQLite. Cada escritura es atómica (una transacción); se usa WAL para que una
/// interrupción de la app no corrompa el archivo. Las fechas se guardan como milisegundos UTC.
/// </summary>
public sealed class SqliteProgressStore : IProgressStore
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public static JsonSerializerOptions Json { get; } = new()
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false,
    };

    public SqliteProgressStore(string databasePath)
    {
        DatabasePath = databasePath;
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Private,
            Pooling = false,
        }.ToString();

        using var connection = Open();
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA foreign_keys=ON;";
            pragma.ExecuteNonQuery();
        }

        SchemaVersion = Migrations.Apply(connection);
    }

    public string DatabasePath { get; }

    public int SchemaVersion { get; }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static long? ToMs(DateTimeOffset? value) => value?.ToUnixTimeMilliseconds();

    private static DateTimeOffset? FromMs(object value) =>
        value is DBNull or null ? null : DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture));

    private async Task WriteAsync(Action<SqliteConnection, SqliteTransaction> action)
    {
        await _writeLock.WaitAsync().ConfigureAwait(false);
        try
        {
            using var connection = Open();
            using var tx = connection.BeginTransaction();
            action(connection, tx);
            tx.Commit();
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private static SqliteCommand Command(SqliteConnection connection, SqliteTransaction? tx, string sql, params (string Name, object? Value)[] parameters)
    {
        var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        return cmd;
    }

    private List<T> Query<T>(string sql, Func<SqliteDataReader, T> map, params (string, object?)[] parameters)
    {
        using var connection = Open();
        using var cmd = Command(connection, null, sql, parameters);
        using var reader = cmd.ExecuteReader();
        var list = new List<T>();
        while (reader.Read())
        {
            list.Add(map(reader));
        }

        return list;
    }

    // ---------- Perfil ----------

    public Task<LearnerProfile?> GetProfileAsync()
    {
        var json = Query("SELECT json FROM profile WHERE id = 1;", r => r.GetString(0)).FirstOrDefault();
        return Task.FromResult(json is null ? null : JsonSerializer.Deserialize<LearnerProfile>(json, Json));
    }

    public Task SaveProfileAsync(LearnerProfile profile) => WriteAsync((c, tx) =>
    {
        using var cmd = Command(c, tx, "INSERT INTO profile(id, json) VALUES (1, $j) ON CONFLICT(id) DO UPDATE SET json = excluded.json;",
            ("$j", JsonSerializer.Serialize(profile, Json)));
        cmd.ExecuteNonQuery();
    });

    // ---------- Habilidades ----------

    private const string SkillColumns = "skill_id, level, ease, interval_days, repetitions, lapses, due_at, last_reviewed_at, difficulty, seen_at, first_practiced_at, provisional_at, retained_at, needs_reinforcement, reinforcement_since, difficulty_changed_at";

    private static SkillProgress MapSkill(SqliteDataReader r) => new(
        r.GetString(0),
        (MasteryLevel)r.GetInt32(1),
        new ReviewSchedule(r.GetDouble(2), r.GetInt32(3), r.GetInt32(4), r.GetInt32(5), FromMs(r.GetValue(6)), FromMs(r.GetValue(7))),
        r.GetInt32(8),
        FromMs(r.GetValue(9)),
        FromMs(r.GetValue(10)),
        FromMs(r.GetValue(11)),
        FromMs(r.GetValue(12)),
        r.GetInt32(13) != 0,
        FromMs(r.GetValue(14)),
        FromMs(r.GetValue(15)));

    public Task<IReadOnlyDictionary<string, SkillProgress>> GetSkillsAsync() =>
        Task.FromResult<IReadOnlyDictionary<string, SkillProgress>>(
            Query($"SELECT {SkillColumns} FROM skill_progress;", MapSkill).ToDictionary(s => s.SkillId));

    public Task<SkillProgress> GetSkillAsync(string skillId) =>
        Task.FromResult(Query($"SELECT {SkillColumns} FROM skill_progress WHERE skill_id = $id;", MapSkill, ("$id", skillId)).FirstOrDefault()
                        ?? SkillProgress.Initial(skillId));

    public Task SaveSkillAsync(SkillProgress p) => WriteAsync((c, tx) => InsertSkill(c, tx, p));

    private static void InsertSkill(SqliteConnection c, SqliteTransaction tx, SkillProgress p)
    {
        using var cmd = Command(c, tx, $"""
            INSERT INTO skill_progress({SkillColumns})
            VALUES ($id, $level, $ease, $interval, $reps, $lapses, $due, $last, $diff, $seen, $first, $prov, $ret, $needs, $rsince, $dchanged)
            ON CONFLICT(skill_id) DO UPDATE SET
                level = excluded.level, ease = excluded.ease, interval_days = excluded.interval_days,
                repetitions = excluded.repetitions, lapses = excluded.lapses, due_at = excluded.due_at,
                last_reviewed_at = excluded.last_reviewed_at, difficulty = excluded.difficulty, seen_at = excluded.seen_at,
                first_practiced_at = excluded.first_practiced_at, provisional_at = excluded.provisional_at,
                retained_at = excluded.retained_at, needs_reinforcement = excluded.needs_reinforcement,
                reinforcement_since = excluded.reinforcement_since, difficulty_changed_at = excluded.difficulty_changed_at;
            """,
            ("$id", p.SkillId), ("$level", (int)p.Level), ("$ease", p.Review.EaseFactor), ("$interval", p.Review.IntervalDays),
            ("$reps", p.Review.Repetitions), ("$lapses", p.Review.Lapses), ("$due", ToMs(p.Review.DueAt)), ("$last", ToMs(p.Review.LastReviewedAt)),
            ("$diff", p.Difficulty), ("$seen", ToMs(p.SeenAt)), ("$first", ToMs(p.FirstPracticedAt)), ("$prov", ToMs(p.ProvisionalMasteryAt)),
            ("$ret", ToMs(p.RetainedAt)), ("$needs", p.NeedsReinforcement ? 1 : 0), ("$rsince", ToMs(p.ReinforcementSince)),
            ("$dchanged", ToMs(p.DifficultyChangedAt)));
        cmd.ExecuteNonQuery();
    }

    // ---------- Intentos ----------

    private const string AttemptColumns = "id, item_id, skill_id, related_json, purpose, context, context_id, correct, score, hints, solution_viewed, tries, duration_ms, confidence, error, auto_graded, is_code, at, answer";

    private static AttemptRecord MapAttempt(SqliteDataReader r) => new()
    {
        Id = Guid.Parse(r.GetString(0)),
        ItemId = r.GetString(1),
        SkillId = r.GetString(2),
        RelatedSkillIds = JsonSerializer.Deserialize<List<string>>(r.GetString(3)) ?? [],
        Purpose = (ItemPurpose)r.GetInt32(4),
        Context = (AttemptContext)r.GetInt32(5),
        ContextId = r.IsDBNull(6) ? null : r.GetString(6),
        Correct = r.GetInt32(7) != 0,
        Score = r.GetDouble(8),
        HintsUsed = r.GetInt32(9),
        SolutionViewed = r.GetInt32(10) != 0,
        Tries = r.GetInt32(11),
        Duration = TimeSpan.FromMilliseconds(r.GetInt64(12)),
        Confidence = r.IsDBNull(13) ? null : r.GetInt32(13),
        Error = (ErrorCategory)r.GetInt32(14),
        AutoGraded = r.GetInt32(15) != 0,
        IsCode = r.GetInt32(16) != 0,
        At = FromMs(r.GetValue(17))!.Value,
        Answer = r.IsDBNull(18) ? null : r.GetString(18),
    };

    public Task AddAttemptAsync(AttemptRecord attempt) => WriteAsync((c, tx) => InsertAttempt(c, tx, attempt));

    private static void InsertAttempt(SqliteConnection c, SqliteTransaction tx, AttemptRecord a)
    {
        using var cmd = Command(c, tx, $"""
            INSERT INTO attempts({AttemptColumns})
            VALUES ($id, $item, $skill, $related, $purpose, $context, $cid, $correct, $score, $hints, $sol, $tries, $dur, $conf, $err, $auto, $code, $at, $answer);
            """,
            ("$id", a.Id.ToString()), ("$item", a.ItemId), ("$skill", a.SkillId), ("$related", JsonSerializer.Serialize(a.RelatedSkillIds)),
            ("$purpose", (int)a.Purpose), ("$context", (int)a.Context), ("$cid", a.ContextId), ("$correct", a.Correct ? 1 : 0),
            ("$score", a.Score), ("$hints", a.HintsUsed), ("$sol", a.SolutionViewed ? 1 : 0), ("$tries", a.Tries),
            ("$dur", (long)a.Duration.TotalMilliseconds), ("$conf", a.Confidence), ("$err", (int)a.Error),
            ("$auto", a.AutoGraded ? 1 : 0), ("$code", a.IsCode ? 1 : 0), ("$at", ToMs(a.At)), ("$answer", a.Answer));
        cmd.ExecuteNonQuery();
    }

    public Task<IReadOnlyList<AttemptRecord>> GetAttemptsAsync(string? skillId = null, string? contextId = null)
    {
        var sql = $"SELECT {AttemptColumns} FROM attempts WHERE ($s IS NULL OR skill_id = $s) AND ($c IS NULL OR context_id = $c) ORDER BY at;";
        return Task.FromResult<IReadOnlyList<AttemptRecord>>(Query(sql, MapAttempt, ("$s", skillId), ("$c", contextId)));
    }

    // ---------- Envíos de código ----------

    public Task AddSubmissionAsync(CodeSubmission s) => WriteAsync((c, tx) => InsertSubmission(c, tx, s));

    private static void InsertSubmission(SqliteConnection c, SqliteTransaction tx, CodeSubmission s)
    {
        using var cmd = Command(c, tx, "INSERT INTO submissions(id, item_id, code, passed, score, error, summary, at) VALUES ($id, $item, $code, $p, $score, $err, $sum, $at);",
            ("$id", s.Id.ToString()), ("$item", s.ItemId), ("$code", s.Code), ("$p", s.Passed ? 1 : 0), ("$score", s.Score),
            ("$err", (int)s.Error), ("$sum", s.Summary), ("$at", ToMs(s.At)));
        cmd.ExecuteNonQuery();
    }

    public Task<IReadOnlyList<CodeSubmission>> GetSubmissionsAsync(string itemId) =>
        Task.FromResult<IReadOnlyList<CodeSubmission>>(Query(
            "SELECT id, item_id, code, passed, score, error, summary, at FROM submissions WHERE item_id = $i ORDER BY at DESC;",
            r => new CodeSubmission(Guid.Parse(r.GetString(0)), r.GetString(1), r.GetString(2), r.GetInt32(3) != 0, r.GetDouble(4),
                (ErrorCategory)r.GetInt32(5), r.GetString(6), FromMs(r.GetValue(7))!.Value),
            ("$i", itemId)));

    // ---------- Sesiones ----------

    public Task SaveSessionAsync(StudySession session) => WriteAsync((c, tx) => InsertSession(c, tx, session));

    private static void InsertSession(SqliteConnection c, SqliteTransaction tx, StudySession s)
    {
        using var cmd = Command(c, tx, """
            INSERT INTO sessions(id, status, started_at, updated_at, json) VALUES ($id, $st, $start, $upd, $json)
            ON CONFLICT(id) DO UPDATE SET status = excluded.status, updated_at = excluded.updated_at, json = excluded.json;
            """,
            ("$id", s.Id.ToString()), ("$st", (int)s.Status), ("$start", ToMs(s.StartedAt)), ("$upd", ToMs(s.LastActiveAt)),
            ("$json", JsonSerializer.Serialize(s, Json)));
        cmd.ExecuteNonQuery();
    }

    public Task<StudySession?> GetUnfinishedSessionAsync() =>
        Task.FromResult(Query("SELECT json FROM sessions WHERE status IN ($a, $p) ORDER BY updated_at DESC LIMIT 1;",
                r => JsonSerializer.Deserialize<StudySession>(r.GetString(0), Json)!,
                ("$a", (int)SessionStatus.Active), ("$p", (int)SessionStatus.Paused))
            .FirstOrDefault());

    public Task<IReadOnlyList<StudySession>> GetSessionsAsync() =>
        Task.FromResult<IReadOnlyList<StudySession>>(Query("SELECT json FROM sessions ORDER BY started_at;",
            r => JsonSerializer.Deserialize<StudySession>(r.GetString(0), Json)!));

    // ---------- Evaluaciones ----------

    public Task SaveAssessmentAsync(Assessment assessment, AssessmentResult? result) => WriteAsync((c, tx) => InsertAssessment(c, tx, assessment, result));

    private static void InsertAssessment(SqliteConnection c, SqliteTransaction tx, Assessment a, AssessmentResult? result)
    {
        using var cmd = Command(c, tx, """
            INSERT INTO assessments(id, kind, scope_id, status, created_at, json, result_json) VALUES ($id, $k, $scope, $st, $created, $json, $res)
            ON CONFLICT(id) DO UPDATE SET status = excluded.status, json = excluded.json, result_json = COALESCE(excluded.result_json, assessments.result_json);
            """,
            ("$id", a.Id.ToString()), ("$k", (int)a.Kind), ("$scope", a.ScopeId), ("$st", (int)a.Status), ("$created", ToMs(a.CreatedAt)),
            ("$json", JsonSerializer.Serialize(a, Json)), ("$res", result is null ? null : JsonSerializer.Serialize(result, Json)));
        cmd.ExecuteNonQuery();
    }

    private static StoredAssessment MapAssessment(SqliteDataReader r) => new(
        JsonSerializer.Deserialize<Assessment>(r.GetString(0), Json)!,
        r.IsDBNull(1) ? null : JsonSerializer.Deserialize<AssessmentResult>(r.GetString(1), Json));

    public Task<StoredAssessment?> GetAssessmentAsync(Guid id) =>
        Task.FromResult(Query("SELECT json, result_json FROM assessments WHERE id = $id;", MapAssessment, ("$id", id.ToString())).FirstOrDefault());

    public Task<IReadOnlyList<StoredAssessment>> GetAssessmentsAsync() =>
        Task.FromResult<IReadOnlyList<StoredAssessment>>(Query("SELECT json, result_json FROM assessments ORDER BY created_at;", MapAssessment));

    // ---------- Proyectos, ajustes y borradores ----------

    public Task SaveProjectAsync(ProjectProgress project) => WriteAsync((c, tx) => InsertProject(c, tx, project));

    private static void InsertProject(SqliteConnection c, SqliteTransaction tx, ProjectProgress p)
    {
        using var cmd = Command(c, tx, "INSERT INTO projects(project_id, json) VALUES ($id, $j) ON CONFLICT(project_id) DO UPDATE SET json = excluded.json;",
            ("$id", p.ProjectId), ("$j", JsonSerializer.Serialize(p, Json)));
        cmd.ExecuteNonQuery();
    }

    public Task<IReadOnlyList<ProjectProgress>> GetProjectsAsync() =>
        Task.FromResult<IReadOnlyList<ProjectProgress>>(Query("SELECT json FROM projects;", r => JsonSerializer.Deserialize<ProjectProgress>(r.GetString(0), Json)!));

    public Task<string?> GetSettingAsync(string key) =>
        Task.FromResult(Query("SELECT value FROM settings WHERE key = $k;", r => r.GetString(0), ("$k", key)).FirstOrDefault());

    public Task SetSettingAsync(string key, string value) => WriteAsync((c, tx) => InsertSetting(c, tx, key, value));

    private static void InsertSetting(SqliteConnection c, SqliteTransaction tx, string key, string value)
    {
        using var cmd = Command(c, tx, "INSERT INTO settings(key, value) VALUES ($k, $v) ON CONFLICT(key) DO UPDATE SET value = excluded.value;", ("$k", key), ("$v", value));
        cmd.ExecuteNonQuery();
    }

    public Task<string?> GetDraftAsync(string itemId) =>
        Task.FromResult(Query("SELECT text FROM drafts WHERE item_id = $i;", r => r.GetString(0), ("$i", itemId)).FirstOrDefault());

    public Task SaveDraftAsync(string itemId, string text) => WriteAsync((c, tx) =>
    {
        using var cmd = Command(c, tx, "INSERT INTO drafts(item_id, text, updated_at) VALUES ($i, $t, $u) ON CONFLICT(item_id) DO UPDATE SET text = excluded.text, updated_at = excluded.updated_at;",
            ("$i", itemId), ("$t", text), ("$u", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        cmd.ExecuteNonQuery();
    });

    // ---------- Exportación y restauración ----------

    public async Task<ProgressExport> ExportAsync(DateTimeOffset now)
    {
        var settings = Query("SELECT key, value FROM settings;", r => (r.GetString(0), r.GetString(1))).ToDictionary(x => x.Item1, x => x.Item2);
        return new ProgressExport(
            ProgressExport.CurrentFormatVersion,
            now,
            await GetProfileAsync(),
            (await GetSkillsAsync()).Values.ToList(),
            await GetAttemptsAsync(),
            await GetSessionsAsync(),
            await GetAssessmentsAsync(),
            await GetProjectsAsync(),
            settings);
    }

    /// <summary>Reemplaza todo el progreso por el de la copia, en una sola transacción.</summary>
    public Task ImportAsync(ProgressExport export)
    {
        if (export.FormatVersion > ProgressExport.CurrentFormatVersion)
        {
            throw new InvalidOperationException("La copia de seguridad proviene de una versión más nueva de la aplicación.");
        }

        return WriteAsync((c, tx) =>
        {
            foreach (var table in new[] { "profile", "skill_progress", "attempts", "sessions", "assessments", "projects", "settings", "drafts", "submissions" })
            {
                using var del = Command(c, tx, $"DELETE FROM {table};");
                del.ExecuteNonQuery();
            }

            if (export.Profile is not null)
            {
                using var cmd = Command(c, tx, "INSERT INTO profile(id, json) VALUES (1, $j);", ("$j", JsonSerializer.Serialize(export.Profile, Json)));
                cmd.ExecuteNonQuery();
            }

            foreach (var s in export.Skills) InsertSkill(c, tx, s);
            foreach (var a in export.Attempts) InsertAttempt(c, tx, a);
            foreach (var s in export.Sessions) InsertSession(c, tx, s);
            foreach (var a in export.Assessments) InsertAssessment(c, tx, a.Assessment, a.Result);
            foreach (var p in export.Projects) InsertProject(c, tx, p);
            foreach (var (k, v) in export.Settings) InsertSetting(c, tx, k, v);
        });
    }

    public static string SerializeExport(ProgressExport export) => JsonSerializer.Serialize(export, new JsonSerializerOptions(Json) { WriteIndented = true });

    public static ProgressExport DeserializeExport(string json) =>
        JsonSerializer.Deserialize<ProgressExport>(json, Json) ?? throw new InvalidDataException("El archivo de respaldo está vacío o dañado.");
}
