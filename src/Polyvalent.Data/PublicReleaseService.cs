using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Polyvalent.Domain;

namespace Polyvalent.Data;

public sealed record PublicReleaseResult(
    string ReleaseId,
    string OutputDirectory,
    string DataSha256,
    int PropertyCount,
    int IntegerCount,
    int ClaimCount,
    int ProfileEntryCount);

public sealed class PublicReleaseService
{
    public const string PublicSchemaVersion = "1";
    private readonly SqliteDatabase _database;

    public PublicReleaseService(SqliteDatabase database)
    {
        _database = database;
    }

    public async Task<PublicReleaseResult> GenerateAsync(
        string outputDirectory,
        string releaseId,
        DateTimeOffset generatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        ValidateReleaseId(releaseId);
        var target = Path.GetFullPath(outputDirectory);
        if (Directory.Exists(target) || File.Exists(target))
        {
            throw new InvalidOperationException(
                $"release target already exists and will not be overwritten: {target}");
        }

        var parent = Path.GetDirectoryName(target)
            ?? throw new InvalidOperationException("the release target has no parent directory.");
        Directory.CreateDirectory(parent);
        var temporary = Path.Combine(parent, $".polyvalent-{releaseId}-{Guid.NewGuid():N}.tmp");
        Directory.CreateDirectory(temporary);

        ProjectionSnapshot? snapshot = null;
        string? dataSha256 = null;
        try
        {
            snapshot = await ReadProjectionAsync(
                releaseId,
                generatedAtUtc,
                cancellationToken);
            var dataBytes = JsonSerializer.SerializeToUtf8Bytes(
                snapshot.Corpus,
                PublicJsonContext.Default.PublicCorpus);
            dataSha256 = Convert.ToHexStringLower(SHA256.HashData(dataBytes));
            var manifest = new PublicManifest(
                PublicSchemaVersion,
                releaseId,
                generatedAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                snapshot.SchemaVersion,
                dataSha256);
            var manifestBytes = JsonSerializer.SerializeToUtf8Bytes(
                manifest,
                PublicJsonContext.Default.PublicManifest);

            ValidateArtifact(snapshot.Corpus, dataBytes, manifestBytes, snapshot.PrivacySentinels);
            await File.WriteAllBytesAsync(Path.Combine(temporary, "data.json"), dataBytes, cancellationToken);
            await File.WriteAllBytesAsync(Path.Combine(temporary, "manifest.json"), manifestBytes, cancellationToken);

            Directory.Move(temporary, target);
            try
            {
                await RecordReleaseAsync(
                    snapshot,
                    target,
                    dataSha256,
                    cancellationToken);
            }
            catch
            {
                Directory.Delete(target, recursive: true);
                throw;
            }

            return new PublicReleaseResult(
                releaseId,
                target,
                dataSha256,
                snapshot.Corpus.Properties.Count,
                snapshot.Corpus.Integers.Count,
                snapshot.ClaimRevisionIds.Count,
                snapshot.ProfileRevisionIds.Count);
        }
        catch
        {
            if (Directory.Exists(temporary))
            {
                Directory.Delete(temporary, recursive: true);
            }

            throw;
        }
    }

    private async Task<ProjectionSnapshot> ReadProjectionAsync(
        string releaseId,
        DateTimeOffset generatedAtUtc,
        CancellationToken cancellationToken)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        var schemaVersion = await ReadSchemaVersionAsync(connection, transaction, cancellationToken);
        var properties = await ReadPropertiesAsync(connection, transaction, cancellationToken);
        var claims = await ReadClaimsAsync(connection, transaction, properties, cancellationToken);
        var profiles = await ReadProfilesAsync(connection, transaction, cancellationToken);
        var sentinels = await ReadSentinelsAsync(connection, transaction, cancellationToken);

        var byInteger = new Dictionary<string, IntegerBuilder>(StringComparer.Ordinal);
        foreach (var claim in claims.Items)
        {
            Builder(byInteger, claim.IntegerValue).Claims.Add(claim.PublicClaim);
        }

        foreach (var profile in profiles.Items)
        {
            Builder(byInteger, profile.IntegerValue).Profiles.Add(profile.PublicProfile);
        }

        var integers = byInteger.Values
            .OrderBy(item => IntegerValue.Parse(item.Value))
            .Select(item => new PublicInteger(
                item.Value,
                item.Claims.OrderBy(claim => claim.PropertySlug, StringComparer.Ordinal).ToArray(),
                item.Profiles.OrderBy(profile => profile.Title, StringComparer.Ordinal).ToArray()))
            .ToArray();
        var corpus = new PublicCorpus(
            PublicSchemaVersion,
            releaseId,
            generatedAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            properties.Items.Select(item => item.PublicProperty).ToArray(),
            integers);
        await transaction.CommitAsync(cancellationToken);
        return new ProjectionSnapshot(
            corpus,
            schemaVersion,
            properties.VersionIds,
            claims.RevisionIds,
            profiles.RevisionIds,
            properties.CitationIds,
            claims.CitationIds,
            profiles.CitationIds,
            sentinels);
    }

    private static async Task<PropertyProjection> ReadPropertiesAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        var rows = new List<PropertyRow>();
        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                SELECT
                    version.id,
                    property.slug,
                    property.display_name,
                    version.definition,
                    version.aliases_json,
                    version.graph_mode,
                    version.status,
                    EXISTS(
                        SELECT 1 FROM comparison_runs comparison
                        WHERE comparison.property_version_id = version.id
                          AND comparison.status = 'mismatched')
                FROM publication_property_selections selection
                JOIN property_versions version ON version.id = selection.property_version_id
                JOIN properties property ON property.id = version.property_id
                ORDER BY property.slug;
                """;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                if (reader.GetString(6) != "accepted" || reader.GetBoolean(7))
                {
                    throw new InvalidOperationException(
                        "a staged property version is unaccepted or has an unresolved source disagreement.");
                }

                rows.Add(new PropertyRow(
                    reader.GetInt64(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    JsonSerializer.Deserialize<string[]>(reader.GetString(4)) ?? [],
                    reader.GetString(5)));
            }
        }

        var items = new List<PropertyItem>();
        var citationIds = new List<long>();
        foreach (var row in rows)
        {
            var coverage = await ReadCoverageAsync(
                connection,
                transaction,
                row.VersionId,
                cancellationToken);
            var citations = await ReadSelectedCitationsAsync(
                connection,
                transaction,
                "property_version_citations",
                "publication_property_citation_selections",
                "property_version_id",
                row.VersionId,
                cancellationToken);
            citationIds.AddRange(citations.Ids);
            items.Add(new PropertyItem(
                row.VersionId,
                row.Slug,
                new PublicProperty(
                    row.Slug,
                    row.Name,
                    row.Definition,
                    row.Aliases,
                    row.GraphMode,
                    coverage,
                    citations.Items)));
        }

        return new PropertyProjection(
            items,
            items.Select(item => item.VersionId).ToArray(),
            items.ToDictionary(item => item.Slug, StringComparer.Ordinal),
            citationIds);
    }

    private static async Task<ClaimProjection> ReadClaimsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        PropertyProjection properties,
        CancellationToken cancellationToken)
    {
        var rows = new List<ClaimRow>();
        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                SELECT
                    revision.id,
                    integer.canonical_value,
                    property.slug,
                    revision.property_version_id,
                    revision.qualifier_json,
                    revision.status,
                    witness.kind,
                    witness.schema_version,
                    witness.payload_json,
                    witness.display_text,
                    EXISTS(
                        SELECT 1 FROM claim_revisions newer
                        WHERE newer.supersedes_revision_id = revision.id
                          AND newer.status = 'accepted')
                FROM publication_claim_selections selection
                JOIN claim_revisions revision ON revision.id = selection.claim_revision_id
                JOIN claims claim ON claim.id = revision.claim_id
                JOIN integers integer ON integer.id = claim.integer_id
                JOIN properties property ON property.id = claim.property_id
                LEFT JOIN claim_witnesses witness ON witness.claim_revision_id = revision.id
                ORDER BY length(integer.canonical_value), integer.canonical_value, property.slug;
                """;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                if (reader.GetString(5) != "accepted" || reader.GetBoolean(10))
                {
                    throw new InvalidOperationException("a staged claim revision is disputed, superseded, or not accepted.");
                }

                var witness = reader.IsDBNull(6)
                    ? null
                    : new EvidenceWitness(
                        reader.GetString(6),
                        reader.GetInt32(7),
                        reader.GetString(8),
                        reader.GetString(9));
                rows.Add(new ClaimRow(
                    reader.GetInt64(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetInt64(3),
                    reader.GetString(4),
                    witness));
            }
        }

        var items = new List<ClaimItem>();
        var citationIds = new List<long>();
        foreach (var row in rows)
        {
            if (!properties.BySlug.TryGetValue(row.PropertySlug, out var property)
                || property.VersionId != row.PropertyVersionId)
            {
                throw new InvalidOperationException(
                    $"claim {row.RevisionId} references an unstaged property version.");
            }

            var definition = InitialPropertyCatalog.Get(row.PropertySlug);
            EvidenceContractValidator.Validate(
                definition,
                IntegerValue.Parse(row.IntegerValue),
                row.QualifierJson,
                row.Witness);
            var citations = await ReadSelectedCitationsAsync(
                connection,
                transaction,
                "claim_revision_citations",
                "publication_claim_citation_selections",
                "claim_revision_id",
                row.RevisionId,
                cancellationToken);
            if (row.PropertySlug == PropertySlugs.DecimalFriedman && citations.Items.Count == 0)
            {
                throw new InvalidOperationException("a staged friedman claim has no selected citation.");
            }

            citationIds.AddRange(citations.Ids);
            using var qualifier = JsonDocument.Parse(row.QualifierJson);
            items.Add(new ClaimItem(
                row.RevisionId,
                row.IntegerValue,
                new PublicClaim(
                    row.PropertySlug,
                    qualifier.RootElement.Clone(),
                    row.Witness is null
                        ? null
                        : new PublicWitness(
                            row.Witness.Kind,
                            row.Witness.SchemaVersion,
                            row.Witness.DisplayText),
                    citations.Items)));
        }

        return new ClaimProjection(
            items,
            items.Select(item => item.RevisionId).ToArray(),
            citationIds);
    }

    private static async Task<ProfileProjection> ReadProfilesAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        var rows = new List<ProfileRow>();
        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                SELECT
                    revision.id,
                    integer.canonical_value,
                    entry.slug,
                    revision.title,
                    revision.category,
                    revision.epistemic_register,
                    revision.body,
                    revision.status,
                    EXISTS(
                        SELECT 1 FROM profile_entry_revisions newer
                        WHERE newer.supersedes_revision_id = revision.id
                          AND newer.status = 'accepted')
                FROM publication_profile_selections selection
                JOIN profile_entry_revisions revision ON revision.id = selection.profile_entry_revision_id
                JOIN profile_entries entry ON entry.id = revision.profile_entry_id
                JOIN integers integer ON integer.id = entry.integer_id
                ORDER BY length(integer.canonical_value), integer.canonical_value, entry.slug;
                """;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                if (reader.GetString(7) != "accepted" || reader.GetBoolean(8))
                {
                    throw new InvalidOperationException("a staged profile revision is disputed, superseded, or not accepted.");
                }

                rows.Add(new ProfileRow(
                    reader.GetInt64(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetString(6)));
            }
        }

        var items = new List<ProfileItem>();
        var citationIds = new List<long>();
        foreach (var row in rows)
        {
            var citations = await ReadSelectedCitationsAsync(
                connection,
                transaction,
                "profile_entry_revision_citations",
                "publication_profile_citation_selections",
                "profile_entry_revision_id",
                row.RevisionId,
                cancellationToken);
            if (citations.Items.Count == 0)
            {
                throw new InvalidOperationException("a staged profile entry has no selected citation.");
            }

            citationIds.AddRange(citations.Ids);
            items.Add(new ProfileItem(
                row.RevisionId,
                row.IntegerValue,
                new PublicProfileEntry(
                    row.Slug,
                    row.Title,
                    row.Category,
                    row.EpistemicRegister,
                    row.Body,
                    citations.Items)));
        }

        return new ProfileProjection(
            items,
            items.Select(item => item.RevisionId).ToArray(),
            citationIds);
    }

    private static async Task<IReadOnlyList<PublicCoverage>> ReadCoverageAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long propertyVersionId,
        CancellationToken cancellationToken)
    {
        var coverage = new List<PublicCoverage>();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT claim.range_min, claim.range_max, claim.basis, claim.exclusions_json
            FROM coverage_claims claim
            JOIN population_runs run ON run.id = claim.population_run_id
            WHERE claim.property_version_id = $propertyVersionId
              AND run.status = 'succeeded'
            ORDER BY claim.range_min, claim.range_max;
            """;
        command.Parameters.AddWithValue("$propertyVersionId", propertyVersionId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            coverage.Add(new PublicCoverage(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                JsonSerializer.Deserialize<string[]>(reader.GetString(3)) ?? []));
        }

        return coverage;
    }

    private static async Task<SelectedCitationProjection> ReadSelectedCitationsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string citationTable,
        string selectionTable,
        string targetColumn,
        long targetId,
        CancellationToken cancellationToken)
    {
        var citations = new List<PublicCitation>();
        var citationIds = new List<long>();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"""
            SELECT citation.id, citation.label, citation.locator, citation.role,
                   source.canonical_url, citation.public_safe
            FROM {selectionTable} selection
            JOIN {citationTable} citation ON citation.id = selection.citation_id
            JOIN sources source ON source.id = citation.source_id
            WHERE selection.{targetColumn} = $targetId
            ORDER BY citation.label, source.canonical_url;
            """;
        command.Parameters.AddWithValue("$targetId", targetId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (!reader.GetBoolean(5))
            {
                throw new InvalidOperationException("a selected citation is not marked public-safe.");
            }

            var url = reader.GetString(4);
            ValidatePublicUrl(url);
            citationIds.Add(reader.GetInt64(0));
            citations.Add(new PublicCitation(
                reader.GetString(1),
                reader.NullableString(2),
                reader.GetString(3),
                url));
        }

        return new SelectedCitationProjection(citations, citationIds);
    }

    private static async Task<IReadOnlyList<string>> ReadSentinelsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        var sentinels = new List<string>();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT value FROM privacy_sentinels ORDER BY id;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sentinels.Add(reader.GetString(0));
        }

        return sentinels;
    }

    private async Task RecordReleaseAsync(
        ProjectionSnapshot snapshot,
        string artifactPath,
        string dataSha256,
        CancellationToken cancellationToken)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO public_releases(
                    id,
                    schema_version,
                    generated_at_utc,
                    source_schema_version,
                    data_sha256,
                    artifact_path,
                    status)
                VALUES (
                    $id,
                    $schemaVersion,
                    $generatedAtUtc,
                    $sourceSchemaVersion,
                    $dataSha256,
                    $artifactPath,
                    'building');
                """;
            command.Parameters.AddWithValue("$id", snapshot.Corpus.ReleaseId);
            command.Parameters.AddWithValue("$schemaVersion", snapshot.Corpus.SchemaVersion);
            command.Parameters.AddWithValue("$generatedAtUtc", snapshot.Corpus.GeneratedAtUtc);
            command.Parameters.AddWithValue("$sourceSchemaVersion", snapshot.SchemaVersion);
            command.Parameters.AddWithValue("$dataSha256", dataSha256);
            command.Parameters.AddWithValue("$artifactPath", artifactPath);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await InsertReleaseItemsAsync(
            connection,
            transaction,
            snapshot.Corpus.ReleaseId,
            "public_release_property_items",
            "property_version_id",
            snapshot.PropertyVersionIds,
            cancellationToken);
        await InsertReleaseItemsAsync(
            connection,
            transaction,
            snapshot.Corpus.ReleaseId,
            "public_release_claim_items",
            "claim_revision_id",
            snapshot.ClaimRevisionIds,
            cancellationToken);
        await InsertReleaseItemsAsync(
            connection,
            transaction,
            snapshot.Corpus.ReleaseId,
            "public_release_profile_items",
            "profile_entry_revision_id",
            snapshot.ProfileRevisionIds,
            cancellationToken);
        await InsertReleaseItemsAsync(
            connection,
            transaction,
            snapshot.Corpus.ReleaseId,
            "public_release_property_citation_items",
            "citation_id",
            snapshot.PropertyCitationIds,
            cancellationToken);
        await InsertReleaseItemsAsync(
            connection,
            transaction,
            snapshot.Corpus.ReleaseId,
            "public_release_claim_citation_items",
            "citation_id",
            snapshot.ClaimCitationIds,
            cancellationToken);
        await InsertReleaseItemsAsync(
            connection,
            transaction,
            snapshot.Corpus.ReleaseId,
            "public_release_profile_citation_items",
            "citation_id",
            snapshot.ProfileCitationIds,
            cancellationToken);

        await using (var seal = connection.CreateCommand())
        {
            seal.Transaction = transaction;
            seal.CommandText = "UPDATE public_releases SET status = 'sealed' WHERE id = $id AND status = 'building';";
            seal.Parameters.AddWithValue("$id", snapshot.Corpus.ReleaseId);
            await seal.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task InsertReleaseItemsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string releaseId,
        string table,
        string itemColumn,
        IReadOnlyList<long> ids,
        CancellationToken cancellationToken)
    {
        foreach (var id in ids)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"INSERT INTO {table}(release_id, {itemColumn}) VALUES ($releaseId, $itemId);";
            command.Parameters.AddWithValue("$releaseId", releaseId);
            command.Parameters.AddWithValue("$itemId", id);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static void ValidateArtifact(
        PublicCorpus corpus,
        byte[] dataBytes,
        byte[] manifestBytes,
        IReadOnlyList<string> sentinels)
    {
        if (corpus.Properties.Count == 0 || corpus.Integers.Count == 0)
        {
            throw new InvalidOperationException("a public release must contain selected properties and integers.");
        }

        var slugs = corpus.Properties.Select(property => property.Slug).ToHashSet(StringComparer.Ordinal);
        foreach (var integer in corpus.Integers)
        {
            _ = IntegerValue.Parse(integer.Value);
            foreach (var claim in integer.Claims)
            {
                if (!slugs.Contains(claim.PropertySlug))
                {
                    throw new InvalidOperationException("a public claim references a missing property.");
                }
            }
        }

        var dataText = Encoding.UTF8.GetString(dataBytes);
        var manifestText = Encoding.UTF8.GetString(manifestBytes);
        foreach (var sentinel in sentinels)
        {
            if (dataText.Contains(sentinel, StringComparison.Ordinal)
                || manifestText.Contains(sentinel, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("a registered private sentinel reached the public artifact.");
            }
        }

        var forbiddenNames = new[]
        {
            "privateNotes",
            "localPrivatePath",
            "sourceCaptureId",
            "populationRunId",
            "propertyVersionId",
            "claimRevisionId",
        };
        foreach (var forbidden in forbiddenNames)
        {
            if (dataText.Contains($"\"{forbidden}\"", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"forbidden persistence field '{forbidden}' reached the public artifact.");
            }
        }
    }

    private static async Task<int> ReadSchemaVersionAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COALESCE(MAX(version), 0) FROM schema_migrations;";
        return Convert.ToInt32(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
    }

    private static void ValidatePublicUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException($"public citation url '{value}' is not safe.");
        }
    }

    private static void ValidateReleaseId(string releaseId)
    {
        if (string.IsNullOrWhiteSpace(releaseId)
            || releaseId.Length > 80
            || releaseId.Any(character =>
                character is not (>= 'a' and <= 'z')
                && character is not (>= '0' and <= '9')
                && character is not '-' and not '_' and not '.'))
        {
            throw new ArgumentException(
                "release ids use lowercase ascii letters, digits, dots, hyphens, and underscores.",
                nameof(releaseId));
        }
    }

    private static IntegerBuilder Builder(
        IDictionary<string, IntegerBuilder> builders,
        string value)
    {
        if (!builders.TryGetValue(value, out var builder))
        {
            builder = new IntegerBuilder(value);
            builders[value] = builder;
        }

        return builder;
    }

    private sealed record ProjectionSnapshot(
        PublicCorpus Corpus,
        int SchemaVersion,
        IReadOnlyList<long> PropertyVersionIds,
        IReadOnlyList<long> ClaimRevisionIds,
        IReadOnlyList<long> ProfileRevisionIds,
        IReadOnlyList<long> PropertyCitationIds,
        IReadOnlyList<long> ClaimCitationIds,
        IReadOnlyList<long> ProfileCitationIds,
        IReadOnlyList<string> PrivacySentinels);

    private sealed record PropertyRow(
        long VersionId,
        string Slug,
        string Name,
        string Definition,
        IReadOnlyList<string> Aliases,
        string GraphMode);

    private sealed record PropertyItem(long VersionId, string Slug, PublicProperty PublicProperty);

    private sealed record PropertyProjection(
        IReadOnlyList<PropertyItem> Items,
        IReadOnlyList<long> VersionIds,
        IReadOnlyDictionary<string, PropertyItem> BySlug,
        IReadOnlyList<long> CitationIds);

    private sealed record ClaimRow(
        long RevisionId,
        string IntegerValue,
        string PropertySlug,
        long PropertyVersionId,
        string QualifierJson,
        EvidenceWitness? Witness);

    private sealed record ClaimItem(long RevisionId, string IntegerValue, PublicClaim PublicClaim);

    private sealed record ClaimProjection(
        IReadOnlyList<ClaimItem> Items,
        IReadOnlyList<long> RevisionIds,
        IReadOnlyList<long> CitationIds);

    private sealed record ProfileRow(
        long RevisionId,
        string IntegerValue,
        string Slug,
        string Title,
        string Category,
        string EpistemicRegister,
        string Body);

    private sealed record ProfileItem(
        long RevisionId,
        string IntegerValue,
        PublicProfileEntry PublicProfile);

    private sealed record ProfileProjection(
        IReadOnlyList<ProfileItem> Items,
        IReadOnlyList<long> RevisionIds,
        IReadOnlyList<long> CitationIds);

    private sealed record SelectedCitationProjection(
        IReadOnlyList<PublicCitation> Items,
        IReadOnlyList<long> Ids);

    private sealed class IntegerBuilder(string value)
    {
        public string Value { get; } = value;

        public List<PublicClaim> Claims { get; } = [];

        public List<PublicProfileEntry> Profiles { get; } = [];
    }
}
