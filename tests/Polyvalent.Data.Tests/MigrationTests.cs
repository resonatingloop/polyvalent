using Microsoft.Data.Sqlite;
using Polyvalent.Domain;

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
        Assert.Equal(1, first.CurrentVersion);
        Assert.Equal([1], first.AppliedVersions);
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
}
