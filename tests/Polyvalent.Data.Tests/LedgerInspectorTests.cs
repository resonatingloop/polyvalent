using System.Security.Cryptography;
using Polyvalent.Application;

namespace Polyvalent.Data.Tests;

public sealed class LedgerInspectorTests
{
    [Fact]
    public async Task Missing_path_is_reported_without_creating_any_entry()
    {
        var root = Path.Combine(Path.GetTempPath(), $"polyvalent-inspect-missing-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "nested", DatabasePathResolver.DatabaseFilename);

        var result = await new LedgerInspector().InspectAsync(path);

        Assert.Equal(LedgerOrientationKind.Missing, result.Kind);
        Assert.False(Directory.Exists(root));
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task Inspector_distinguishes_empty_current_and_authored_ledgers()
    {
        await using var empty = TestDatabase.Create();
        await empty.Database.InitializeAsync();
        var emptyResult = await new LedgerInspector().InspectAsync(empty.Database.Path);
        Assert.Equal(LedgerOrientationKind.Empty, emptyResult.Kind);

        await using var current = TestDatabase.Create();
        await new CorpusSeeder(current.Database).SeedAsync();
        var before = Snapshot(current.Directory);
        var currentResult = await new LedgerInspector().InspectAsync(current.Database.Path);
        var after = Snapshot(current.Directory);

        Assert.Equal(LedgerOrientationKind.CurrentReference, currentResult.Kind);
        Assert.Equal(17, currentResult.PropertyCount);
        Assert.Equal(761, currentResult.AcceptedClaimCount);
        Assert.Equal(20, currentResult.ProfileEntryCount);
        Assert.Equal(before, after);

        await using (var connection = await current.Database.OpenConnectionAsync())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO sources(slug, title, canonical_url, created_at_utc)
                VALUES ('fictional-private-source', 'fictional private source', 'https://example.test/private',
                        '2026-11-13T00:00:00Z');
                """;
            await command.ExecuteNonQueryAsync();
        }

        var authoredResult = await new LedgerInspector().InspectAsync(current.Database.Path);
        Assert.Equal(LedgerOrientationKind.NonCurrentOrAuthored, authoredResult.Kind);
        await Assert.ThrowsAsync<NonCurrentOrAuthoredLedgerException>(
            () => new CorpusSeeder(current.Database).SeedAsync());
    }

    [Fact]
    public async Task Partial_non_property_state_is_not_empty_and_seed_is_refused()
    {
        await using var fixture = TestDatabase.Create();
        await fixture.Database.InitializeAsync();
        await using (var connection = await fixture.Database.OpenConnectionAsync())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO integers(canonical_value) VALUES ('9999');";
            await command.ExecuteNonQueryAsync();
        }

        var result = await new LedgerInspector().InspectAsync(fixture.Database.Path);

        Assert.Equal(LedgerOrientationKind.NonCurrentOrAuthored, result.Kind);
        await Assert.ThrowsAsync<NonCurrentOrAuthoredLedgerException>(
            () => new CorpusSeeder(fixture.Database).SeedAsync());
    }

    [Fact]
    public async Task Same_count_semantic_substitution_is_not_current_reference()
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        await using (var connection = await fixture.Database.OpenConnectionAsync())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                UPDATE properties
                SET display_name = 'fictional changed prime name'
                WHERE slug = 'prime';
                """;
            await command.ExecuteNonQueryAsync();
        }

        var result = await new LedgerInspector().InspectAsync(fixture.Database.Path);

        Assert.Equal(17, result.PropertyCount);
        Assert.Equal(761, result.AcceptedClaimCount);
        Assert.Equal(LedgerOrientationKind.NonCurrentOrAuthored, result.Kind);
        await Assert.ThrowsAsync<NonCurrentOrAuthoredLedgerException>(
            () => new CorpusSeeder(fixture.Database).SeedAsync());
    }

    [Fact]
    public async Task Release_history_is_an_operational_overlay_not_corpus_identity()
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        await using (var connection = await fixture.Database.OpenConnectionAsync())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO public_releases(
                    id, schema_version, generated_at_utc, source_schema_version,
                    data_sha256, artifact_path, status)
                VALUES (
                    'fictional-release', 'publication-003', '2026-11-13T00:00:00Z', 1,
                    'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa',
                    '/tmp/fictional-release', 'sealed');
                """;
            await command.ExecuteNonQueryAsync();
        }

        var result = await new LedgerInspector().InspectAsync(fixture.Database.Path);

        Assert.Equal(LedgerOrientationKind.CurrentReference, result.Kind);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("checksum")]
    public async Task Unknown_or_changed_migration_is_an_explicit_unchanged_error(string divergence)
    {
        await using var fixture = TestDatabase.Create();
        await fixture.Database.InitializeAsync();
        await using (var connection = await fixture.Database.OpenConnectionAsync())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = divergence == "unknown"
                ? """
                    INSERT INTO schema_migrations(version, name, sha256, applied_at_utc)
                    VALUES (99, 'unknown.sql',
                            'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa',
                            '2026-11-13T00:00:00Z');
                    """
                : """
                    UPDATE schema_migrations
                    SET sha256 = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
                    WHERE version = 1;
                    """;
            await command.ExecuteNonQueryAsync();
        }

        var before = Snapshot(fixture.Directory);
        await Assert.ThrowsAsync<LedgerInspectionException>(
            () => new LedgerInspector().InspectAsync(fixture.Database.Path));
        Assert.Equal(before, Snapshot(fixture.Directory));
    }

    [Theory]
    [InlineData("drop-trigger")]
    [InlineData("extra-table")]
    [InlineData("extra-column")]
    public async Task Schema_object_drift_is_incompatible_and_unchanged(string divergence)
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        await using (var connection = await fixture.Database.OpenConnectionAsync())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = divergence switch
            {
                "drop-trigger" => "DROP TRIGGER claim_revision_freeze_guard;",
                "extra-table" => "CREATE TABLE fictional_extra_table(value TEXT);",
                "extra-column" => "ALTER TABLE properties ADD COLUMN fictional_extra TEXT;",
                _ => throw new InvalidOperationException($"unknown divergence '{divergence}'."),
            };
            await command.ExecuteNonQueryAsync();
        }

        var before = Snapshot(fixture.Directory);
        await Assert.ThrowsAsync<LedgerInspectionException>(
            () => new LedgerInspector().InspectAsync(fixture.Database.Path));
        Assert.Equal(before, Snapshot(fixture.Directory));
    }

    [Fact]
    public async Task Non_sqlite_and_directory_inputs_are_explicit_errors_and_unchanged()
    {
        var root = Path.Combine(Path.GetTempPath(), $"polyvalent-inspect-invalid-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var invalid = Path.Combine(root, "not-sqlite.sqlite3");
            await File.WriteAllTextAsync(invalid, "fictional-private-sentinel");
            var before = Snapshot(root);

            await Assert.ThrowsAsync<LedgerInspectionException>(
                () => new LedgerInspector().InspectAsync(invalid));
            await Assert.ThrowsAsync<LedgerInspectionException>(
                () => new LedgerInspector().InspectAsync(root));

            Assert.Equal(before, Snapshot(root));

            var emptyFile = Path.Combine(root, "zero-byte.sqlite3");
            await File.WriteAllBytesAsync(emptyFile, []);
            var zeroByteBefore = Snapshot(root);
            await Assert.ThrowsAsync<LedgerInspectionException>(
                () => new LedgerInspector().InspectAsync(emptyFile));
            Assert.Equal(zeroByteBefore, Snapshot(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Active_wal_with_or_without_shm_is_refused_without_mutation()
    {
        await using var fixture = TestDatabase.Create();
        await fixture.Database.InitializeAsync();
        await using var writer = await fixture.Database.OpenConnectionAsync();
        await using (var command = writer.CreateCommand())
        {
            command.CommandText = """
                PRAGMA wal_autocheckpoint = 0;
                INSERT INTO integers(canonical_value) VALUES ('4242');
                """;
            await command.ExecuteNonQueryAsync();
        }

        var before = Snapshot(fixture.Directory);
        var error = await Assert.ThrowsAsync<LedgerInspectionException>(
            () => new LedgerInspector().InspectAsync(fixture.Database.Path));
        Assert.Contains("active write-ahead log", error.Message, StringComparison.Ordinal);
        Assert.Equal(before, Snapshot(fixture.Directory));

        var copyDirectory = Path.Combine(Path.GetTempPath(), $"polyvalent-wal-copy-{Guid.NewGuid():N}");
        Directory.CreateDirectory(copyDirectory);
        try
        {
            var copyPath = Path.Combine(copyDirectory, "copied.sqlite3");
            File.Copy(fixture.Database.Path, copyPath);
            File.Copy($"{fixture.Database.Path}-wal", $"{copyPath}-wal");
            var copyBefore = Snapshot(copyDirectory);

            await Assert.ThrowsAsync<LedgerInspectionException>(
                () => new LedgerInspector().InspectAsync(copyPath));

            Assert.Equal(copyBefore, Snapshot(copyDirectory));
            Assert.False(File.Exists($"{copyPath}-shm"));
        }
        finally
        {
            Directory.Delete(copyDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task Nonempty_rollback_journal_is_refused_without_mutation()
    {
        await using var fixture = TestDatabase.Create();
        await fixture.Database.InitializeAsync();
        await File.WriteAllBytesAsync($"{fixture.Database.Path}-journal", [1, 2, 3, 4]);
        var before = Snapshot(fixture.Directory);

        var error = await Assert.ThrowsAsync<LedgerInspectionException>(
            () => new LedgerInspector().InspectAsync(fixture.Database.Path));

        Assert.Contains("active rollback journal", error.Message, StringComparison.Ordinal);
        Assert.Equal(before, Snapshot(fixture.Directory));
    }

    [Fact]
    public async Task Inaccessible_existing_path_is_an_error_not_missing()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var root = Path.Combine(Path.GetTempPath(), $"polyvalent-inspect-unreadable-{Guid.NewGuid():N}");
        var locked = Path.Combine(root, "locked");
        var path = Path.Combine(locked, "ledger.sqlite3");
        Directory.CreateDirectory(locked);
        await File.WriteAllTextAsync(path, "fictional unreadable ledger");
        File.SetUnixFileMode(locked, UnixFileMode.None);
        try
        {
            await Assert.ThrowsAsync<LedgerInspectionException>(
                () => new LedgerInspector().InspectAsync(path));
        }
        finally
        {
            File.SetUnixFileMode(
                locked,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            Directory.Delete(root, recursive: true);
        }
    }

    private static string Snapshot(string directory)
    {
        return string.Join(
            "\n",
            Directory.GetFiles(directory)
                .Order(StringComparer.Ordinal)
                .Select(path =>
                {
                    var info = new FileInfo(path);
                    var digest = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)));
                    return $"{info.Name}|{info.Length}|{info.LastWriteTimeUtc.Ticks}|{digest}";
                }));
    }
}
