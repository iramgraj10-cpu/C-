using Microsoft.Data.Sqlite;

namespace CSharpMastery.Persistence;

/// <summary>Migración de esquema numerada. Se aplican en orden y dentro de una transacción.</summary>
public sealed record Migration(int Version, string Description, string Sql);

public static class Migrations
{
    public static IReadOnlyList<Migration> All { get; } =
    [
        new(1, "Esquema inicial", """
            CREATE TABLE profile (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                json TEXT NOT NULL
            );
            CREATE TABLE skill_progress (
                skill_id TEXT PRIMARY KEY,
                level INTEGER NOT NULL,
                ease REAL NOT NULL,
                interval_days INTEGER NOT NULL,
                repetitions INTEGER NOT NULL,
                lapses INTEGER NOT NULL,
                due_at INTEGER NULL,
                last_reviewed_at INTEGER NULL,
                difficulty INTEGER NOT NULL,
                seen_at INTEGER NULL,
                first_practiced_at INTEGER NULL,
                provisional_at INTEGER NULL,
                retained_at INTEGER NULL,
                needs_reinforcement INTEGER NOT NULL,
                reinforcement_since INTEGER NULL,
                difficulty_changed_at INTEGER NULL
            );
            CREATE TABLE attempts (
                id TEXT PRIMARY KEY,
                item_id TEXT NOT NULL,
                skill_id TEXT NOT NULL,
                related_json TEXT NOT NULL,
                purpose INTEGER NOT NULL,
                context INTEGER NOT NULL,
                context_id TEXT NULL,
                correct INTEGER NOT NULL,
                score REAL NOT NULL,
                hints INTEGER NOT NULL,
                solution_viewed INTEGER NOT NULL,
                tries INTEGER NOT NULL,
                duration_ms INTEGER NOT NULL,
                confidence INTEGER NULL,
                error INTEGER NOT NULL,
                auto_graded INTEGER NOT NULL,
                is_code INTEGER NOT NULL,
                at INTEGER NOT NULL,
                answer TEXT NULL
            );
            CREATE INDEX ix_attempts_skill ON attempts(skill_id);
            CREATE INDEX ix_attempts_item ON attempts(item_id);
            CREATE INDEX ix_attempts_context ON attempts(context_id);
            CREATE TABLE sessions (
                id TEXT PRIMARY KEY,
                status INTEGER NOT NULL,
                started_at INTEGER NOT NULL,
                updated_at INTEGER NOT NULL,
                json TEXT NOT NULL
            );
            CREATE TABLE assessments (
                id TEXT PRIMARY KEY,
                kind INTEGER NOT NULL,
                scope_id TEXT NOT NULL,
                status INTEGER NOT NULL,
                created_at INTEGER NOT NULL,
                json TEXT NOT NULL,
                result_json TEXT NULL
            );
            CREATE TABLE projects (
                project_id TEXT PRIMARY KEY,
                json TEXT NOT NULL
            );
            CREATE TABLE settings (
                key TEXT PRIMARY KEY,
                value TEXT NOT NULL
            );
            CREATE TABLE drafts (
                item_id TEXT PRIMARY KEY,
                text TEXT NOT NULL,
                updated_at INTEGER NOT NULL
            );
            """),
        new(2, "Historial de envíos de código", """
            CREATE TABLE submissions (
                id TEXT PRIMARY KEY,
                item_id TEXT NOT NULL,
                code TEXT NOT NULL,
                passed INTEGER NOT NULL,
                score REAL NOT NULL,
                error INTEGER NOT NULL,
                summary TEXT NOT NULL,
                at INTEGER NOT NULL
            );
            CREATE INDEX ix_submissions_item ON submissions(item_id);
            """),
    ];

    public static int LatestVersion => All[^1].Version;

    public static int GetVersion(SqliteConnection connection)
    {
        using var create = connection.CreateCommand();
        create.CommandText = "CREATE TABLE IF NOT EXISTS schema_migrations (version INTEGER PRIMARY KEY, description TEXT NOT NULL, applied_at TEXT NOT NULL);";
        create.ExecuteNonQuery();

        using var query = connection.CreateCommand();
        query.CommandText = "SELECT COALESCE(MAX(version), 0) FROM schema_migrations;";
        return Convert.ToInt32(query.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Aplica las migraciones pendientes hasta <paramref name="targetVersion"/> (por defecto, la última).</summary>
    public static int Apply(SqliteConnection connection, int? targetVersion = null)
    {
        var current = GetVersion(connection);
        var target = targetVersion ?? LatestVersion;
        if (current > LatestVersion)
        {
            throw new InvalidOperationException($"La base de datos (versión {current}) es más nueva que esta aplicación (versión {LatestVersion}).");
        }

        foreach (var migration in All.Where(m => m.Version > current && m.Version <= target))
        {
            using var tx = connection.BeginTransaction();
            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = migration.Sql;
                cmd.ExecuteNonQuery();
            }

            using (var record = connection.CreateCommand())
            {
                record.Transaction = tx;
                record.CommandText = "INSERT INTO schema_migrations(version, description, applied_at) VALUES ($v, $d, $a);";
                record.Parameters.AddWithValue("$v", migration.Version);
                record.Parameters.AddWithValue("$d", migration.Description);
                record.Parameters.AddWithValue("$a", DateTimeOffset.UtcNow.ToString("O"));
                record.ExecuteNonQuery();
            }

            tx.Commit();
            current = migration.Version;
        }

        return current;
    }
}
