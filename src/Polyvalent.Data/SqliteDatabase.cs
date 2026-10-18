using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Polyvalent.Data;

public sealed record MigrationResult(
    int PreviousVersion,
    int CurrentVersion,
    string? BackupPath,
    IReadOnlyList<int> AppliedVersions);

public sealed class SqliteDatabase
{
    private const string MigrationResourcePrefix = "Polyvalent.Data.Migrations.";
    private readonly TimeProvider _timeProvider;

    public SqliteDatabase(string path, TimeProvider? timeProvider = null)
    {
        Path = System.IO.Path.GetFullPath(path);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public string Path { get; }

    public async Task<MigrationResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        var directory = System.IO.Path.GetDirectoryName(Path)
            ?? throw new InvalidOperationException("the database path has no parent directory.");
        Directory.CreateDirectory(directory);

        var migrations = LoadMigrations();
        var existedWithData = File.Exists(Path) && new FileInfo(Path).Length > 0;
        var applied = existedWithData
            ? await ReadAppliedMigrationsIfPresentAsync(cancellationToken)
            : new Dictionary<int, string>();
        ValidateAppliedMigrations(applied, migrations);
        var pending = migrations.Where(migration => !applied.ContainsKey(migration.Version)).ToArray();

        string? backupPath = null;
        if (existedWithData && pending.Length > 0)
        {
            backupPath = await CreateBackupAsync(cancellationToken);
        }

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await EnsureMigrationLedgerAsync(connection, cancellationToken);
        await EnableWalAsync(connection, cancellationToken);

        var newlyApplied = new List<int>();
        foreach (var migration in pending)
        {
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                await using var migrationCommand = connection.CreateCommand();
                migrationCommand.Transaction = (SqliteTransaction)transaction;
                migrationCommand.CommandText = migration.Sql;
                await migrationCommand.ExecuteNonQueryAsync(cancellationToken);

                await using var ledgerCommand = connection.CreateCommand();
                ledgerCommand.Transaction = (SqliteTransaction)transaction;
                ledgerCommand.CommandText = """
                    INSERT INTO schema_migrations(version, name, sha256, applied_at_utc)
                    VALUES ($version, $name, $sha256, $appliedAtUtc);
                    """;
                ledgerCommand.Parameters.AddWithValue("$version", migration.Version);
                ledgerCommand.Parameters.AddWithValue("$name", migration.Name);
                ledgerCommand.Parameters.AddWithValue("$sha256", migration.Sha256);
                ledgerCommand.Parameters.AddWithValue("$appliedAtUtc", UtcNowText());
                await ledgerCommand.ExecuteNonQueryAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                newlyApplied.Add(migration.Version);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        RestrictDatabasePermissions();
        return new MigrationResult(
            applied.Count == 0 ? 0 : applied.Keys.Max(),
            migrations.Count == 0 ? 0 : migrations.Max(migration => migration.Version),
            backupPath,
            newlyApplied);
    }

    public async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = Path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,
            Pooling = false,
        };
        var connection = new SqliteConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA busy_timeout = 5000;";
        await command.ExecuteNonQueryAsync(cancellationToken);
        return connection;
    }

    public async Task<int> GetSchemaVersionAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COALESCE(MAX(version), 0) FROM schema_migrations;";
        return Convert.ToInt32(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
    }

    private async Task<Dictionary<int, string>> ReadAppliedMigrationsIfPresentAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var tableCommand = connection.CreateCommand();
        tableCommand.CommandText = """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table' AND name = 'schema_migrations';
            """;
        var exists = Convert.ToInt32(
            await tableCommand.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture) == 1;
        if (!exists)
        {
            return [];
        }

        var applied = new Dictionary<int, string>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT version, sha256 FROM schema_migrations ORDER BY version;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            applied.Add(reader.GetInt32(0), reader.GetString(1));
        }

        return applied;
    }

    private static void ValidateAppliedMigrations(
        IReadOnlyDictionary<int, string> applied,
        IReadOnlyList<Migration> available)
    {
        var byVersion = available.ToDictionary(migration => migration.Version);
        foreach (var item in applied)
        {
            if (!byVersion.TryGetValue(item.Key, out var migration))
            {
                throw new InvalidOperationException(
                    $"database contains unknown migration version {item.Key}.");
            }

            if (!string.Equals(item.Value, migration.Sha256, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"migration {item.Key} checksum does not match the recorded schema ledger.");
            }
        }
    }

    private async Task<string> CreateBackupAsync(CancellationToken cancellationToken)
    {
        var timestamp = _timeProvider.GetUtcNow().ToString("yyyyMMddTHHmmssfffZ", CultureInfo.InvariantCulture);
        var backupPath = $"{Path}.backup-{timestamp}";

        await using var source = await OpenConnectionAsync(cancellationToken);
        var backupBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = backupPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,
            Pooling = false,
        };
        await using var destination = new SqliteConnection(backupBuilder.ConnectionString);
        await destination.OpenAsync(cancellationToken);
        source.BackupDatabase(destination);
        RestrictFilePermissions(backupPath);
        return backupPath;
    }

    private static async Task EnsureMigrationLedgerAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS schema_migrations (
                version INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                sha256 TEXT NOT NULL CHECK (length(sha256) = 64),
                applied_at_utc TEXT NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnableWalAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode = WAL;";
        await command.ExecuteScalarAsync(cancellationToken);
    }

    private static IReadOnlyList<Migration> LoadMigrations()
    {
        var assembly = typeof(SqliteDatabase).Assembly;
        return assembly
            .GetManifestResourceNames()
            .Where(name => name.StartsWith(MigrationResourcePrefix, StringComparison.Ordinal)
                && name.EndsWith(".sql", StringComparison.Ordinal))
            .Select(name => ReadMigration(assembly, name))
            .OrderBy(migration => migration.Version)
            .ToArray();
    }

    private static Migration ReadMigration(Assembly assembly, string resourceName)
    {
        var filename = resourceName[MigrationResourcePrefix.Length..];
        var separator = filename.IndexOf('_');
        if (separator <= 0
            || !int.TryParse(filename[..separator], NumberStyles.None, CultureInfo.InvariantCulture, out var version))
        {
            throw new InvalidOperationException($"migration resource '{resourceName}' has an invalid name.");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"migration resource '{resourceName}' could not be opened.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var sql = reader.ReadToEnd();
        var digest = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(sql)));
        return new Migration(version, filename, sql, digest);
    }

    private string UtcNowText() =>
        _timeProvider.GetUtcNow().ToString("O", CultureInfo.InvariantCulture);

    private void RestrictDatabasePermissions()
    {
        RestrictFilePermissions(Path);
        RestrictFilePermissions($"{Path}-wal");
        RestrictFilePermissions($"{Path}-shm");
    }

    private static void RestrictFilePermissions(string path)
    {
        if (!OperatingSystem.IsWindows() && File.Exists(path))
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    private sealed record Migration(int Version, string Name, string Sql, string Sha256);
}
