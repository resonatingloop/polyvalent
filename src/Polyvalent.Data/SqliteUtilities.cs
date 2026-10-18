using Microsoft.Data.Sqlite;

namespace Polyvalent.Data;

internal static class SqliteUtilities
{
    public static SqliteParameter AddValue(this SqliteParameterCollection parameters, string name, object? value) =>
        parameters.AddWithValue(name, value ?? DBNull.Value);

    public static string? NullableString(this SqliteDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
}
