using System.Globalization;
using Polyvalent.Data;

namespace Polyvalent.Publisher;

internal static class Program
{
    public static Task<int> Main(string[] args) => RunAsync(args, Console.Out, Console.Error);

    internal static async Task<int> RunAsync(string[] args, TextWriter output, TextWriter error)
    {
        string? databasePath = null;
        try
        {
            var options = CommandLine.Parse(args);
            if (options.Command == "help")
            {
                await output.WriteLineAsync(CommandLine.Usage);
                return 0;
            }

            databasePath = DatabasePathResolver.Resolve(options.DatabasePath, options.DataDirectory);
            if (options.Command == "inspect")
            {
                var orientation = await new LedgerInspector().InspectAsync(databasePath);
                await WriteInspectionAsync(output, orientation);
                return 0;
            }

            var database = new SqliteDatabase(databasePath);
            if (options.Command is "init" or "seed" or "bootstrap")
            {
                var migration = await database.InitializeAsync();
                await output.WriteLineAsync(
                    $"database schema {migration.CurrentVersion}; applied {migration.AppliedVersions.Count} migration(s).");
                if (migration.BackupPath is not null)
                {
                    await output.WriteLineAsync($"migration backup: {migration.BackupPath}");
                }
            }

            if (options.Command is "seed" or "bootstrap")
            {
                var seed = await new CorpusSeeder(database).SeedAsync();
                await output.WriteLineAsync(
                    $"corpus {(seed.AlreadySeeded ? "already present" : "seeded")}: "
                    + $"{seed.IntegerCount} integers, "
                    + $"{seed.PropertyCounts.Values.Sum()} accepted claims, "
                    + $"{seed.ProfileEntryCount} profile {(seed.ProfileEntryCount == 1 ? "entry" : "entries")}.");
                if (seed.Disagreements.Count > 0)
                {
                    await output.WriteLineAsync(
                        $"publication held for disagreed properties: {string.Join(", ", seed.Disagreements)}.");
                }
            }

            if (options.Command is "publish" or "bootstrap")
            {
                await database.InitializeAsync();
                var releaseId = options.ReleaseId
                    ?? throw new ArgumentException("publish requires --release-id.");
                var generatedAt = options.GeneratedAtUtc
                    ?? DateTimeOffset.UtcNow;
                var outputDirectory = options.OutputDirectory
                    ?? Path.Combine(Environment.CurrentDirectory, "artifacts", "public", releaseId);
                var result = await new PublicReleaseService(database).GenerateAsync(
                    outputDirectory,
                    releaseId,
                    generatedAt);
                await output.WriteLineAsync(
                    $"sealed {result.ReleaseId}: {result.PropertyCount} properties, "
                    + $"{result.IntegerCount} integers, {result.ClaimCount} claims, "
                    + $"sha-256 {result.DataSha256}.");
                await output.WriteLineAsync($"artifact: {result.OutputDirectory}");
            }

            return 0;
        }
        catch (NonCurrentOrAuthoredLedgerException exception)
        {
            await error.WriteLineAsync($"polyvalent publisher: {exception.Message}");
            await error.WriteLineAsync($"preserved path: {databasePath}");
            await error.WriteLineAsync($"current recipe: {CorpusSeeder.SeedVersion}");
            await error.WriteLineAsync(
                "fresh reference: dotnet run --project src/Polyvalent.Publisher/Polyvalent.Publisher.csproj "
                + "-c Release -- seed --data-dir ./var/reference/seed-v6");
            return 1;
        }
        catch (Exception exception)
        {
            await error.WriteLineAsync($"polyvalent publisher: {exception.Message}");
            return 1;
        }
    }

    private static async Task WriteInspectionAsync(TextWriter output, Polyvalent.Application.LedgerOrientation result)
    {
        await output.WriteLineAsync($"path: {result.DatabasePath}");
        await output.WriteLineAsync($"schema: {result.SchemaVersion?.ToString(CultureInfo.InvariantCulture) ?? "not present"}");
        await output.WriteLineAsync($"current recipe: {result.CurrentRecipe}");
        await output.WriteLineAsync(
            $"evidence: {result.PropertyCount} properties · {result.AcceptedClaimCount} accepted claims · "
            + $"{result.ProfileEntryCount} profile entries");
        await output.WriteLineAsync($"orientation: {result.Outcome}");
        await output.WriteLineAsync($"meaning: {result.Meaning}");
        await output.WriteLineAsync($"seed: {result.SeedDisposition}");
        await output.WriteLineAsync($"next: {result.SafeNextAction}");
    }

    private sealed record CommandLine(
        string Command,
        string? DatabasePath,
        string? DataDirectory,
        string? OutputDirectory,
        string? ReleaseId,
        DateTimeOffset? GeneratedAtUtc)
    {
        public const string Usage = """
            polyvalent publisher

            commands:
              inspect    report ledger orientation without creating or modifying it
              init       migrate the canonical local database
              seed       migrate and reproducibly populate the first corpus
              publish    emit one immutable public release from staged records
              bootstrap  migrate, seed, and emit one public release

            options:
              --db <path>             explicit sqlite database path
              --data-dir <path>       data directory when --db is absent
              --output <path>         new release directory; never overwritten
              --release-id <id>       lowercase release identity
              --generated-at <time>   injected utc timestamp for reproducible output
            """;

        public static CommandLine Parse(string[] args)
        {
            if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
            {
                return new CommandLine("help", null, null, null, null, null);
            }

            var command = args[0];
            if (command is not ("inspect" or "init" or "seed" or "publish" or "bootstrap"))
            {
                throw new ArgumentException($"unknown command '{command}'.\n\n{Usage}");
            }

            string? database = null;
            string? dataDirectory = null;
            string? output = null;
            string? releaseId = null;
            DateTimeOffset? generatedAt = null;
            for (var index = 1; index < args.Length; index += 2)
            {
                if (index + 1 >= args.Length)
                {
                    throw new ArgumentException($"option '{args[index]}' requires a value.");
                }

                var value = args[index + 1];
                switch (args[index])
                {
                    case "--db":
                        database = value;
                        break;
                    case "--data-dir":
                        dataDirectory = value;
                        break;
                    case "--output":
                        output = value;
                        break;
                    case "--release-id":
                        releaseId = value;
                        break;
                    case "--generated-at":
                        generatedAt = DateTimeOffset.Parse(
                            value,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                        break;
                    default:
                        throw new ArgumentException($"unknown option '{args[index]}'.");
                }
            }

            if (database is not null && dataDirectory is not null)
            {
                throw new ArgumentException("use --db or --data-dir, not both.");
            }

            return new CommandLine(command, database, dataDirectory, output, releaseId, generatedAt);
        }
    }
}
