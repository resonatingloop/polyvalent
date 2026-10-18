using Polyvalent.Data;

namespace Polyvalent.Data.Tests;

internal sealed class TestDatabase : IAsyncDisposable
{
    private TestDatabase(string directory, SqliteDatabase database)
    {
        Directory = directory;
        Database = database;
    }

    public string Directory { get; }

    public SqliteDatabase Database { get; }

    public static TestDatabase Create()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            $"polyvalent-data-tests-{Guid.NewGuid():N}");
        System.IO.Directory.CreateDirectory(directory);
        return new TestDatabase(
            directory,
            new SqliteDatabase(
                Path.Combine(directory, "test.sqlite3"),
                new FixedTimeProvider(new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero))));
    }

    public ValueTask DisposeAsync()
    {
        if (System.IO.Directory.Exists(Directory))
        {
            System.IO.Directory.Delete(Directory, recursive: true);
        }

        return ValueTask.CompletedTask;
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
