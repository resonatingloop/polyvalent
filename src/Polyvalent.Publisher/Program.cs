using System.Globalization;
using Polyvalent.Data;

namespace Polyvalent.Publisher;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var options = CommandLine.Parse(args);
            if (options.Command == "help")
            {
                Console.WriteLine(CommandLine.Usage);
                return 0;
            }

            var databasePath = DatabasePathResolver.Resolve(options.DatabasePath, options.DataDirectory);
            var database = new SqliteDatabase(databasePath);
            if (options.Command is "init" or "seed" or "bootstrap")
            {
                var migration = await database.InitializeAsync();
                Console.WriteLine(
                    $"database schema {migration.CurrentVersion}; applied {migration.AppliedVersions.Count} migration(s).");
                if (migration.BackupPath is not null)
                {
                    Console.WriteLine($"migration backup: {migration.BackupPath}");
                }
            }

            if (options.Command is "seed" or "bootstrap")
            {
                var seed = await new CorpusSeeder(database).SeedAsync();
                Console.WriteLine(
                    $"corpus {(seed.AlreadySeeded ? "already present" : "seeded")}: "
                    + $"{seed.IntegerCount} integers, "
                    + $"{seed.PropertyCounts.Values.Sum()} accepted claims, "
                    + $"{seed.ProfileEntryCount} profile {(seed.ProfileEntryCount == 1 ? "entry" : "entries")}.");
                if (seed.Disagreements.Count > 0)
                {
                    Console.WriteLine(
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
                var output = options.OutputDirectory
                    ?? Path.Combine(Environment.CurrentDirectory, "artifacts", "public", releaseId);
                var result = await new PublicReleaseService(database).GenerateAsync(
                    output,
                    releaseId,
                    generatedAt);
                Console.WriteLine(
                    $"sealed {result.ReleaseId}: {result.PropertyCount} properties, "
                    + $"{result.IntegerCount} integers, {result.ClaimCount} claims, "
                    + $"sha-256 {result.DataSha256}.");
                Console.WriteLine($"artifact: {result.OutputDirectory}");
            }

            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"polyvalent publisher: {exception.Message}");
            return 1;
        }
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
            if (command is not ("init" or "seed" or "publish" or "bootstrap"))
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
