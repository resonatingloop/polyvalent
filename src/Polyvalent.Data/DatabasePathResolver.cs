namespace Polyvalent.Data;

public static class DatabasePathResolver
{
    public const string DataDirectoryEnvironmentVariable = "POLYVALENT_DATA_DIR";
    public const string DatabaseFilename = "polyvalent.sqlite3";

    public static string Resolve(string? explicitDatabasePath = null, string? explicitDataDirectory = null)
    {
        if (!string.IsNullOrWhiteSpace(explicitDatabasePath))
        {
            return Path.GetFullPath(explicitDatabasePath);
        }

        var dataDirectory = !string.IsNullOrWhiteSpace(explicitDataDirectory)
            ? explicitDataDirectory
            : Environment.GetEnvironmentVariable(DataDirectoryEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(dataDirectory))
        {
            dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "polyvalent");
        }

        return Path.GetFullPath(Path.Combine(dataDirectory, DatabaseFilename));
    }
}
