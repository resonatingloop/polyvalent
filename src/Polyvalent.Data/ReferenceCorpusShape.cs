using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Polyvalent.Data;

internal sealed class ReferenceCorpusMismatchException(string message) : InvalidOperationException(message);

internal static class ReferenceCorpusShape
{
    public const string RecipeId = "seed-v6";

    // Release receipts are an operational overlay, not corpus recipe identity.
    private static readonly string[] CorpusTables =
    [
        "integers",
        "properties",
        "property_versions",
        "evaluators",
        "population_runs",
        "comparison_runs",
        "claims",
        "claim_revisions",
        "claim_witnesses",
        "coverage_claims",
        "sources",
        "source_captures",
        "property_version_citations",
        "claim_revision_citations",
        "profile_entries",
        "profile_entry_revisions",
        "profile_entry_revision_citations",
        "direct_relations",
        "direct_relation_revisions",
        "direct_relation_participants",
        "direct_relation_revision_citations",
        "constellations",
        "constellation_revisions",
        "constellation_members",
        "constellation_revision_citations",
        "publication_property_selections",
        "publication_property_citation_selections",
        "publication_claim_selections",
        "publication_claim_citation_selections",
        "publication_profile_selections",
        "publication_profile_citation_selections",
        "privacy_sentinels",
    ];

    private const string ExpectedFingerprint = "92865b34153bfe588805e715a1f6fa09a2ce7ef85f243d0e978a33d51b388803";

    public static async Task<bool> IsEmptyAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken = default)
    {
        foreach (var table in CorpusTables)
        {
            if (await CountAsync(connection, table, cancellationToken) != 0)
            {
                return false;
            }
        }

        return true;
    }

    public static async Task AssertCurrentAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken = default)
    {
        await SqliteDatabase.ValidateCurrentSchemaAsync(connection, cancellationToken);
        var fingerprint = await ComputeFingerprintAsync(connection, cancellationToken);
        if (!string.Equals(fingerprint, ExpectedFingerprint, StringComparison.Ordinal))
        {
            throw new ReferenceCorpusMismatchException(
                $"ledger does not exactly match the {RecipeId} semantic corpus shape ({fingerprint}).");
        }
    }

    internal static async Task<string> ComputeFingerprintAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken = default)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var table in CorpusTables)
        {
            var columns = await ReadSemanticColumnsAsync(connection, table, cancellationToken);
            Append(hash, $"table:{table}");
            foreach (var column in columns)
            {
                Append(hash, $"column:{column}");
            }

            var quotedColumns = string.Join(", ", columns.Select(QuoteIdentifier));
            await using var command = connection.CreateCommand();
            command.CommandText =
                $"SELECT {quotedColumns} FROM {QuoteIdentifier(table)} ORDER BY {quotedColumns};";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                Append(hash, "row");
                for (var index = 0; index < reader.FieldCount; index++)
                {
                    AppendValue(hash, reader.GetValue(index));
                }
            }
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    public static Task<int> CountPropertiesAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken = default) =>
        CountAsync(connection, "properties", cancellationToken);

    public static async Task<int> CountAcceptedClaimsAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken = default)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM claim_revisions WHERE status = 'accepted';";
        return Convert.ToInt32(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
    }

    public static Task<int> CountProfileEntriesAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken = default) =>
        CountAsync(connection, "profile_entries", cancellationToken);

    private static async Task<IReadOnlyList<string>> ReadSemanticColumnsAsync(
        SqliteConnection connection,
        string table,
        CancellationToken cancellationToken)
    {
        var columns = new List<string>();
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({QuoteIdentifier(table)});";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader.GetString(1);
            if (!name.EndsWith("_at_utc", StringComparison.Ordinal))
            {
                columns.Add(name);
            }
        }

        if (columns.Count == 0)
        {
            throw new InvalidOperationException($"required ledger table '{table}' is missing or incompatible.");
        }

        return columns;
    }

    private static async Task<int> CountAsync(
        SqliteConnection connection,
        string table,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {QuoteIdentifier(table)};";
        return Convert.ToInt32(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
    }

    private static string QuoteIdentifier(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

    private static void AppendValue(IncrementalHash hash, object value)
    {
        switch (value)
        {
            case DBNull:
                Append(hash, "null");
                break;
            case long integer:
                Append(hash, $"integer:{integer.ToString(CultureInfo.InvariantCulture)}");
                break;
            case double real:
                Append(hash, $"real:{real.ToString("R", CultureInfo.InvariantCulture)}");
                break;
            case string text:
                Append(hash, $"text:{text}");
                break;
            case byte[] blob:
                Append(hash, $"blob:{Convert.ToHexStringLower(blob)}");
                break;
            default:
                throw new InvalidOperationException(
                    $"unsupported sqlite value type '{value.GetType().Name}' in reference shape.");
        }
    }

    private static void Append(IncrementalHash hash, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        Span<byte> length = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(length, bytes.Length);
        hash.AppendData(length);
        hash.AppendData(bytes);
    }
}
