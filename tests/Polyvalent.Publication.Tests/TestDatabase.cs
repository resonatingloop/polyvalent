using Polyvalent.Data;

namespace Polyvalent.Publication.Tests;

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
            $"polyvalent-publication-tests-{Guid.NewGuid():N}");
        System.IO.Directory.CreateDirectory(directory);
        return new TestDatabase(directory, new SqliteDatabase(Path.Combine(directory, "test.sqlite3")));
    }

    public ValueTask DisposeAsync()
    {
        if (System.IO.Directory.Exists(Directory))
        {
            System.IO.Directory.Delete(Directory, recursive: true);
        }

        return ValueTask.CompletedTask;
    }
}
