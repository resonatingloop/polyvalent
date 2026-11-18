using Microsoft.Data.Sqlite;
using Polyvalent.Domain;
using System.Security.Cryptography;

namespace Polyvalent.Data.Tests;

public sealed class MigrationTests
{
    [Fact]
    public async Task Fresh_database_migrates_and_reopens_without_mutation()
    {
        await using var fixture = TestDatabase.Create();

        var first = await fixture.Database.InitializeAsync();
        var second = await fixture.Database.InitializeAsync();

        Assert.Equal(0, first.PreviousVersion);
        Assert.Equal(2, first.CurrentVersion);
        Assert.Equal([1, 2], first.AppliedVersions);
        Assert.Empty(second.AppliedVersions);
        Assert.Null(second.BackupPath);
    }

    [Fact]
    public async Task Existing_nonempty_database_is_backed_up_before_migration()
    {
        await using var fixture = TestDatabase.Create();
        var builder = new SqliteConnectionStringBuilder { DataSource = fixture.Database.Path };
        await using (var connection = new SqliteConnection(builder.ConnectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE legacy(value TEXT NOT NULL); INSERT INTO legacy VALUES ('still here');";
            await command.ExecuteNonQueryAsync();
        }

        var result = await fixture.Database.InitializeAsync();

        Assert.NotNull(result.BackupPath);
        Assert.True(File.Exists(result.BackupPath));
        var backupBuilder = new SqliteConnectionStringBuilder { DataSource = result.BackupPath };
        await using var backup = new SqliteConnection(backupBuilder.ConnectionString);
        await backup.OpenAsync();
        await using var check = backup.CreateCommand();
        check.CommandText = "SELECT value FROM legacy;";
        Assert.Equal("still here", await check.ExecuteScalarAsync());
    }

    [Fact]
    public async Task Exact_v1_ledger_is_oriented_then_backed_up_before_additive_v2_migration()
    {
        await using var fixture = TestDatabase.Create();
        await CreateExactV1LedgerAsync(fixture.Database.Path);

        var orientation = await new LedgerInspector().InspectAsync(fixture.Database.Path);
        Assert.Equal(Polyvalent.Application.LedgerOrientationKind.MigrationRequired, orientation.Kind);
        Assert.Equal(1, orientation.SchemaVersion);

        var result = await fixture.Database.InitializeAsync();
        Assert.Equal(1, result.PreviousVersion);
        Assert.Equal(2, result.CurrentVersion);
        Assert.Equal([2], result.AppliedVersions);
        Assert.NotNull(result.BackupPath);
        Assert.True(File.Exists(result.BackupPath));

        await AssertLegacyTablesEqualAsync(fixture.Database, result.BackupPath!);

        var restoredPath = Path.Combine(Path.GetDirectoryName(fixture.Database.Path)!, "restored-v1.sqlite3");
        File.Copy(result.BackupPath!, restoredPath);
        Assert.Equal(
            SHA256.HashData(await File.ReadAllBytesAsync(result.BackupPath!)),
            SHA256.HashData(await File.ReadAllBytesAsync(restoredPath)));
        var restored = await new LedgerInspector().InspectAsync(restoredPath);
        Assert.Equal(Polyvalent.Application.LedgerOrientationKind.MigrationRequired, restored.Kind);
    }

    [Fact]
    public async Task Canonical_values_beyond_sqlite_integer_range_round_trip_as_text()
    {
        await using var fixture = TestDatabase.Create();
        await fixture.Database.InitializeAsync();
        const string huge = "900719925474099312345678901234567890";
        await using var connection = await fixture.Database.OpenConnectionAsync();
        await using var insert = connection.CreateCommand();
        insert.CommandText = "INSERT INTO integers(canonical_value) VALUES ($value);";
        insert.Parameters.AddWithValue("$value", IntegerValue.Parse(huge).ToString());
        await insert.ExecuteNonQueryAsync();
        await using var read = connection.CreateCommand();
        read.CommandText = "SELECT canonical_value FROM integers;";

        Assert.Equal(huge, await read.ExecuteScalarAsync());
    }

    [Fact]
    public async Task Reopen_rejects_a_migration_whose_recorded_checksum_has_drifted()
    {
        await using var fixture = TestDatabase.Create();
        await fixture.Database.InitializeAsync();
        await using (var connection = await fixture.Database.OpenConnectionAsync())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "UPDATE schema_migrations SET sha256 = $digest WHERE version = 1;";
            command.Parameters.AddWithValue("$digest", new string('0', 64));
            await command.ExecuteNonQueryAsync();
        }

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.Database.InitializeAsync());

        Assert.Contains("checksum", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("01")]
    [InlineData("-0")]
    [InlineData("+1")]
    [InlineData("1x")]
    public async Task Schema_rejects_noncanonical_integer_text(string value)
    {
        await using var fixture = TestDatabase.Create();
        await fixture.Database.InitializeAsync();
        await using var connection = await fixture.Database.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO integers(canonical_value) VALUES ($value);";
        command.Parameters.AddWithValue("$value", value);

        await Assert.ThrowsAsync<SqliteException>(() => command.ExecuteNonQueryAsync());
    }

    private static async Task CreateExactV1LedgerAsync(string path)
    {
        var assembly = typeof(SqliteDatabase).Assembly;
        const string resourceName = "Polyvalent.Data.Migrations.001_initial.sql";
        await using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"missing resource {resourceName}.");
        using var reader = new StreamReader(stream);
        var sql = await reader.ReadToEndAsync();
        var checksum = Convert.ToHexStringLower(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(sql)));

        var builder = new SqliteConnectionStringBuilder { DataSource = path, ForeignKeys = true };
        await using var connection = new SqliteConnection(builder.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS schema_migrations (
                version INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                sha256 TEXT NOT NULL CHECK (length(sha256) = 64),
                applied_at_utc TEXT NOT NULL
            );
            """ + sql + """
            INSERT INTO schema_migrations(version, name, sha256, applied_at_utc)
            VALUES (1, 'initial', $checksum, '2026-11-17T00:00:00.0000000Z');
            INSERT INTO integers(canonical_value) VALUES ('137'), ('773');
            INSERT INTO sources(slug, title, canonical_url, created_at_utc)
            VALUES ('fixture-source', 'fixture source', 'https://example.test/source', '2026-11-17T00:00:00.0000000Z');
            INSERT INTO source_captures(
                source_id, uri, retrieved_at_utc, media_type, sha256)
            VALUES (1, 'https://example.test/source', '2026-11-17T00:00:00.0000000Z',
                    'text/plain', 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa');
            INSERT INTO properties(slug, display_name)
            VALUES ('fixture-property', 'fixture property');
            INSERT INTO property_versions(
                property_id, version, status, definition, aliases_json, graph_mode,
                evidence_contract_json, validator_key, created_at_utc)
            VALUES (1, 1, 'draft', 'fictional migration fixture', '[]', 'none',
                    '{}', 'fixture', '2026-11-17T00:00:00.0000000Z');
            INSERT INTO property_version_citations(
                property_version_id, source_id, source_capture_id, role, label, public_safe)
            VALUES (1, 1, 1, 'definition', 'fixture source', 1);
            INSERT INTO claims(integer_id, property_id) VALUES (1, 1);
            INSERT INTO claim_revisions(
                claim_id, revision, property_version_id, origin_kind, qualifier_json,
                status, created_at_utc)
            VALUES (1, 1, 1, 'manual', '{}', 'draft', '2026-11-17T00:00:00.0000000Z');
            INSERT INTO claim_witnesses(
                claim_revision_id, kind, schema_version, payload_json, display_text)
            VALUES (1, 'fixture', 1, '{}', 'fixture witness');
            INSERT INTO claim_revision_citations(
                claim_revision_id, source_id, source_capture_id, role, label, public_safe)
            VALUES (1, 1, 1, 'evidence', 'fixture source', 1);
            INSERT INTO profile_entries(integer_id, slug) VALUES (1, 'fixture-profile');
            INSERT INTO profile_entry_revisions(
                profile_entry_id, revision, status, title, category,
                epistemic_register, body, created_at_utc)
            VALUES (1, 1, 'draft', 'fixture profile', 'fictional',
                    'migration proof', 'fixture body', '2026-11-17T00:00:00.0000000Z');
            INSERT INTO profile_entry_revision_citations(
                profile_entry_revision_id, source_id, source_capture_id, role, label, public_safe)
            VALUES (1, 1, 1, 'context', 'fixture source', 1);
            UPDATE property_versions
            SET status = 'accepted', accepted_at_utc = '2026-11-17T00:00:00.0000000Z'
            WHERE id = 1;
            UPDATE claim_revisions
            SET status = 'accepted', accepted_at_utc = '2026-11-17T00:00:00.0000000Z'
            WHERE id = 1;
            UPDATE profile_entry_revisions
            SET status = 'accepted', accepted_at_utc = '2026-11-17T00:00:00.0000000Z'
            WHERE id = 1;
            INSERT INTO publication_property_selections(property_version_id, selected_at_utc)
            VALUES (1, '2026-11-17T00:00:00.0000000Z');
            INSERT INTO publication_property_citation_selections(property_version_id, citation_id)
            VALUES (1, 1);
            INSERT INTO publication_claim_selections(claim_revision_id, selected_at_utc)
            VALUES (1, '2026-11-17T00:00:00.0000000Z');
            INSERT INTO publication_claim_citation_selections(claim_revision_id, citation_id)
            VALUES (1, 1);
            INSERT INTO publication_profile_selections(profile_entry_revision_id, selected_at_utc)
            VALUES (1, '2026-11-17T00:00:00.0000000Z');
            INSERT INTO publication_profile_citation_selections(profile_entry_revision_id, citation_id)
            VALUES (1, 1);
            INSERT INTO public_releases(
                id, schema_version, generated_at_utc, source_schema_version,
                data_sha256, artifact_path, status)
            VALUES ('fixture-v1', '1', '2026-11-17T00:00:00.0000000Z', 1,
                    'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb',
                    '/tmp/fixture-v1', 'building');
            INSERT INTO public_release_property_items VALUES ('fixture-v1', 1);
            INSERT INTO public_release_claim_items VALUES ('fixture-v1', 1);
            INSERT INTO public_release_profile_items VALUES ('fixture-v1', 1);
            INSERT INTO public_release_property_citation_items VALUES ('fixture-v1', 1);
            INSERT INTO public_release_claim_citation_items VALUES ('fixture-v1', 1);
            INSERT INTO public_release_profile_citation_items VALUES ('fixture-v1', 1);
            UPDATE public_releases SET status = 'sealed' WHERE id = 'fixture-v1';
            """;
        command.Parameters.AddWithValue("$checksum", checksum);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task AssertLegacyTablesEqualAsync(SqliteDatabase migrated, string backupPath)
    {
        await using var connection = await migrated.OpenConnectionAsync();
        await using (var attach = connection.CreateCommand())
        {
            attach.CommandText = "ATTACH DATABASE $path AS prior;";
            attach.Parameters.AddWithValue("$path", backupPath);
            await attach.ExecuteNonQueryAsync();
        }

        var tables = new List<string>();
        await using (var list = connection.CreateCommand())
        {
            list.CommandText = """
                SELECT name
                FROM prior.sqlite_schema
                WHERE type = 'table'
                  AND name NOT LIKE 'sqlite_%'
                  AND name <> 'schema_migrations'
                ORDER BY name;
                """;
            await using var reader = await list.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }
        }

        foreach (var table in tables)
        {
            var quoted = $"\"{table.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
            await using var compare = connection.CreateCommand();
            compare.CommandText = $"""
                SELECT
                    (SELECT COUNT(*) FROM main.{quoted}),
                    (SELECT COUNT(*) FROM prior.{quoted}),
                    (SELECT COUNT(*) FROM (
                        SELECT * FROM main.{quoted}
                        EXCEPT
                        SELECT * FROM prior.{quoted}));
                """;
            await using var reader = await compare.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.Equal(reader.GetInt64(1), reader.GetInt64(0));
            Assert.Equal(0, reader.GetInt64(2));
        }
    }
}
