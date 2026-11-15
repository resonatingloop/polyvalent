using Polyvalent.Data;

namespace Polyvalent.Publisher.Tests;

public sealed class PublisherCommandTests
{
    [Fact]
    public async Task Inspect_missing_path_succeeds_without_creating_it()
    {
        var root = Path.Combine(Path.GetTempPath(), $"polyvalent-publisher-missing-{Guid.NewGuid():N}");
        var dataDirectory = Path.Combine(root, "cabinet");
        var output = new StringWriter();
        var error = new StringWriter();

        var exitCode = await Program.RunAsync(
            ["inspect", "--data-dir", dataDirectory],
            output,
            error);

        Assert.Equal(0, exitCode);
        Assert.Contains("orientation: missing", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("current recipe: seed-v6", output.ToString(), StringComparison.Ordinal);
        Assert.Empty(error.ToString());
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public async Task Inspect_outputs_only_bounded_evidence_and_never_private_content()
    {
        var root = CreateDirectory("bounded");
        try
        {
            var database = new SqliteDatabase(Path.Combine(root, DatabasePathResolver.DatabaseFilename));
            await new CorpusSeeder(database).SeedAsync();
            const string privateSentinel = "fictional-private-sentinel-never-print";
            await using (var connection = await database.OpenConnectionAsync())
            {
                await using var command = connection.CreateCommand();
                command.CommandText = """
                    INSERT INTO privacy_sentinels(value, label, created_at_utc)
                    VALUES ($value, 'fictional label', '2026-11-13T00:00:00Z');
                    """;
                command.Parameters.AddWithValue("$value", privateSentinel);
                await command.ExecuteNonQueryAsync();
            }

            var output = new StringWriter();
            var error = new StringWriter();
            var exitCode = await Program.RunAsync(
                ["inspect", "--db", database.Path],
                output,
                error);

            Assert.Equal(0, exitCode);
            Assert.Contains("orientation: non-current-or-authored", output.ToString(), StringComparison.Ordinal);
            Assert.DoesNotContain(privateSentinel, output.ToString(), StringComparison.Ordinal);
            Assert.DoesNotContain(privateSentinel, error.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Inspect_incompatible_input_fails_without_echoing_its_contents()
    {
        var root = CreateDirectory("invalid");
        try
        {
            const string privateSentinel = "fictional-private-invalid-file";
            var path = Path.Combine(root, "invalid.sqlite3");
            await File.WriteAllTextAsync(path, privateSentinel);
            var output = new StringWriter();
            var error = new StringWriter();

            var exitCode = await Program.RunAsync(["inspect", "--db", path], output, error);

            Assert.Equal(1, exitCode);
            Assert.Empty(output.ToString());
            Assert.Contains("unreadable or is not a compatible", error.ToString(), StringComparison.Ordinal);
            Assert.DoesNotContain(privateSentinel, error.ToString(), StringComparison.Ordinal);
            Assert.Equal(privateSentinel, await File.ReadAllTextAsync(path));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Seed_refusal_names_preserved_path_recipe_and_fresh_command()
    {
        var root = CreateDirectory("refusal");
        try
        {
            var database = new SqliteDatabase(Path.Combine(root, DatabasePathResolver.DatabaseFilename));
            await database.InitializeAsync();
            await using (var connection = await database.OpenConnectionAsync())
            {
                await using var command = connection.CreateCommand();
                command.CommandText =
                    "INSERT INTO properties(slug, display_name) VALUES ('fictional-legacy', 'fictional legacy');";
                await command.ExecuteNonQueryAsync();
            }

            var output = new StringWriter();
            var error = new StringWriter();
            var exitCode = await Program.RunAsync(["seed", "--db", database.Path], output, error);
            var message = error.ToString();

            Assert.Equal(1, exitCode);
            Assert.Contains($"preserved path: {database.Path}", message, StringComparison.Ordinal);
            Assert.Contains("current recipe: seed-v6", message, StringComparison.Ordinal);
            Assert.Contains("--data-dir ./var/reference/seed-v6", message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateDirectory(string label)
    {
        var path = Path.Combine(Path.GetTempPath(), $"polyvalent-publisher-{label}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
