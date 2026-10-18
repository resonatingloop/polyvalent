using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Polyvalent.Domain;

namespace Polyvalent.Data;

public sealed record SeedResult(
    bool AlreadySeeded,
    int IntegerCount,
    IReadOnlyDictionary<string, int> PropertyCounts,
    int ProfileEntryCount,
    IReadOnlyList<string> Disagreements);

public sealed class CorpusSeeder
{
    public const string SeedVersion = "seed-v1";
    private readonly SqliteDatabase _database;
    private readonly TimeProvider _timeProvider;
    private readonly IReadOnlyList<string>? _narcissisticComparisonValuesOverride;

    public CorpusSeeder(SqliteDatabase database, TimeProvider? timeProvider = null)
        : this(database, timeProvider, null)
    {
    }

    internal CorpusSeeder(
        SqliteDatabase database,
        IReadOnlyList<string> narcissisticComparisonValuesOverride,
        TimeProvider? timeProvider = null)
        : this(database, timeProvider, narcissisticComparisonValuesOverride)
    {
    }

    private CorpusSeeder(
        SqliteDatabase database,
        TimeProvider? timeProvider,
        IReadOnlyList<string>? narcissisticComparisonValuesOverride)
    {
        _database = database;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _narcissisticComparisonValuesOverride = narcissisticComparisonValuesOverride;
    }

    public async Task<SeedResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        await _database.InitializeAsync(cancellationToken);
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        if (await CountAsync(connection, "properties", cancellationToken) > 0)
        {
            return await ReadExistingResultAsync(connection, cancellationToken);
        }

        var narcissisticSeed = ReadEmbeddedJson<NarcissisticSeed>("narcissistic.json");
        var friedmanSeed = ReadEmbeddedJson<FriedmanSeed>("friedman.json");
        ValidateSeedEnvelope(narcissisticSeed.SchemaVersion, narcissisticSeed.RangeMinimum, narcissisticSeed.RangeMaximum);
        ValidateSeedEnvelope(friedmanSeed.SchemaVersion, friedmanSeed.RangeMinimum, friedmanSeed.RangeMaximum);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var sqlTransaction = (SqliteTransaction)transaction;
        var now = UtcNowText();

        var integerIds = await InsertIntegersAsync(connection, sqlTransaction, cancellationToken);
        var sources = await InsertSourcesAsync(connection, sqlTransaction, now, cancellationToken);
        var captures = await InsertCapturesAsync(
            connection,
            sqlTransaction,
            sources,
            narcissisticSeed,
            friedmanSeed,
            cancellationToken);
        var evaluators = await InsertEvaluatorsAsync(connection, sqlTransaction, cancellationToken);
        var properties = await InsertPropertiesAsync(
            connection,
            sqlTransaction,
            sources,
            captures,
            now,
            cancellationToken);

        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        var disagreements = new List<string>();
        var deterministic = new IPropertyEvaluator[]
        {
            new PrimeEvaluator(),
            new TriangularEvaluator(),
            new DecimalPalindromeEvaluator(),
            new DecimalNarcissisticEvaluator(),
        };
        foreach (var evaluator in deterministic)
        {
            var property = InitialPropertyCatalog.Definitions.Single(item => item.EvaluatorKey == evaluator.Key);
            var evaluations = evaluator
                .Evaluate(IntegerValue.Parse("1"), IntegerValue.Parse("2000"))
                .ToArray();
            var runId = $"{SeedVersion}-{property.Slug}";
            await InsertPopulationRunAsync(
                connection,
                sqlTransaction,
                runId,
                properties[property.Slug].VersionId,
                evaluators[(evaluator.Key, evaluator.Version)],
                "deterministic",
                JsonSerializer.Serialize(new
                {
                    rangeMinimum = "1",
                    rangeMaximum = "2000",
                    evaluator = evaluator.Key,
                    evaluatorVersion = evaluator.Version,
                }),
                now,
                cancellationToken);

            foreach (var evaluation in evaluations)
            {
                EvidenceContractValidator.Validate(
                    property,
                    evaluation.Integer,
                    evaluation.QualifierJson,
                    evaluation.Witness);
                await InsertAcceptedClaimAsync(
                    connection,
                    sqlTransaction,
                    integerIds[evaluation.Integer.ToString()],
                    properties[property.Slug],
                    ClaimOrigin.Computed,
                    runId,
                    evaluation.QualifierJson,
                    evaluation.Witness,
                    [],
                    now,
                    cancellationToken);
            }

            await FinishPopulationRunAsync(connection, sqlTransaction, runId, now, cancellationToken);
            var comparisonMatched = true;
            if (property.Slug == PropertySlugs.DecimalNarcissistic)
            {
                comparisonMatched = await InsertNarcissisticComparisonAsync(
                    connection,
                    sqlTransaction,
                    properties[property.Slug].VersionId,
                    runId,
                    captures["oeis-a005188"],
                    evaluations.Select(item => item.Integer.ToString()).ToArray(),
                    _narcissisticComparisonValuesOverride ?? narcissisticSeed.Values,
                    now,
                    cancellationToken);
            }

            if (comparisonMatched)
            {
                await InsertCoverageAsync(
                    connection,
                    sqlTransaction,
                    properties[property.Slug].VersionId,
                    runId,
                    "deterministic",
                    now,
                    cancellationToken);
            }
            else
            {
                disagreements.Add(property.Slug);
            }

            counts[property.Slug] = evaluations.Length;
        }

        var friedmanCount = await InsertFriedmanPopulationAsync(
            connection,
            sqlTransaction,
            integerIds,
            properties[PropertySlugs.DecimalFriedman],
            sources,
            captures,
            friedmanSeed,
            now,
            cancellationToken);
        counts[PropertySlugs.DecimalFriedman] = friedmanCount;

        var profileRevisionId = await InsertReferenceProfileAsync(
            connection,
            sqlTransaction,
            integerIds["137"],
            sources["nist-alphinv"],
            now,
            cancellationToken);
        await StageEverythingAsync(connection, sqlTransaction, profileRevisionId, now, cancellationToken);

        AssertExpectedCounts(counts);
        await transaction.CommitAsync(cancellationToken);
        return new SeedResult(false, integerIds.Count, counts, 1, disagreements);
    }

    private static async Task<Dictionary<string, long>> InsertIntegersAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        var ids = new Dictionary<string, long>(StringComparer.Ordinal);
        for (var value = 1; value <= 2000; value++)
        {
            var text = value.ToString(CultureInfo.InvariantCulture);
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "INSERT INTO integers(canonical_value) VALUES ($value) RETURNING id;";
            command.Parameters.AddWithValue("$value", text);
            ids[text] = Convert.ToInt64(
                await command.ExecuteScalarAsync(cancellationToken),
                CultureInfo.InvariantCulture);
        }

        return ids;
    }

    private static async Task<Dictionary<string, long>> InsertSourcesAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string now,
        CancellationToken cancellationToken)
    {
        var definitions = new[]
        {
            new SourceSeed("oeis-a000040", "OEIS A000040 — the prime numbers", "OEIS Foundation Inc.", "https://oeis.org/A000040", "source terms apply; attribution retained"),
            new SourceSeed("oeis-a000217", "OEIS A000217 — triangular numbers", "OEIS Foundation Inc.", "https://oeis.org/A000217", "source terms apply; attribution retained"),
            new SourceSeed("oeis-a002113", "OEIS A002113 — palindromes in base 10", "OEIS Foundation Inc.", "https://oeis.org/A002113", "source terms apply; attribution retained"),
            new SourceSeed("oeis-a005188", "OEIS A005188 — narcissistic numbers", "OEIS Foundation Inc.", "https://oeis.org/A005188", "source terms apply; attribution retained"),
            new SourceSeed("oeis-a036057", "OEIS A036057 — Friedman numbers", "OEIS Foundation Inc.", "https://oeis.org/A036057", "source terms apply; attribution retained"),
            new SourceSeed("oeis-a080035", "OEIS A080035 — orderly Friedman numbers", "OEIS Foundation Inc.", "https://oeis.org/A080035", "source terms apply; attribution retained"),
            new SourceSeed("erich-friedman-2000", "Math Magic, August 2000", "Erich Friedman", "https://erich-friedman.github.io/mathmagic/0800.html", "copyright status not asserted; attribution retained"),
            new SourceSeed("nist-alphinv", "inverse fine-structure constant", "NIST", "https://physics.nist.gov/cgi-bin/cuu/Value?alphinv", "United States government reference; source terms apply"),
        };

        var ids = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var source in definitions)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO sources(
                    slug, title, author, canonical_url, license_metadata, private_notes, created_at_utc)
                VALUES ($slug, $title, $author, $url, $license, NULL, $now)
                RETURNING id;
                """;
            command.Parameters.AddWithValue("$slug", source.Slug);
            command.Parameters.AddWithValue("$title", source.Title);
            command.Parameters.AddWithValue("$author", source.Author);
            command.Parameters.AddWithValue("$url", source.Url);
            command.Parameters.AddWithValue("$license", source.LicenseMetadata);
            command.Parameters.AddWithValue("$now", now);
            ids[source.Slug] = Convert.ToInt64(
                await command.ExecuteScalarAsync(cancellationToken),
                CultureInfo.InvariantCulture);
        }

        return ids;
    }

    private static async Task<Dictionary<string, long>> InsertCapturesAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        IReadOnlyDictionary<string, long> sources,
        NarcissisticSeed narcissistic,
        FriedmanSeed friedman,
        CancellationToken cancellationToken)
    {
        var captures = new Dictionary<string, long>(StringComparer.Ordinal);
        captures["oeis-a005188"] = await InsertCaptureAsync(
            connection,
            transaction,
            sources["oeis-a005188"],
            narcissistic.Source,
            narcissistic.RetrievedAtUtc,
            EmbeddedBytes("narcissistic.json"),
            "bounded extract for integers 1 through 2000",
            cancellationToken);
        captures["oeis-a036057"] = await InsertCaptureAsync(
            connection,
            transaction,
            sources["oeis-a036057"],
            friedman.Source,
            friedman.RetrievedAtUtc,
            EmbeddedBytes("friedman.json"),
            "bounded membership and witness extract for integers 1 through 2000",
            cancellationToken);
        return captures;
    }

    private static async Task<long> InsertCaptureAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long sourceId,
        string uri,
        string retrievedAtUtc,
        byte[] boundedExtract,
        string revisionMetadata,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO source_captures(
                source_id,
                uri,
                retrieved_at_utc,
                media_type,
                sha256,
                revision_metadata,
                license_metadata,
                local_private_path)
            VALUES (
                $sourceId,
                $uri,
                $retrievedAtUtc,
                'application/vnd.polyvalent.bounded-sequence+json',
                $sha256,
                $revisionMetadata,
                'source terms apply; see THIRD_PARTY_NOTICES.md',
                NULL)
            RETURNING id;
            """;
        command.Parameters.AddWithValue("$sourceId", sourceId);
        command.Parameters.AddWithValue("$uri", uri);
        command.Parameters.AddWithValue("$retrievedAtUtc", retrievedAtUtc);
        command.Parameters.AddWithValue("$sha256", Convert.ToHexStringLower(SHA256.HashData(boundedExtract)));
        command.Parameters.AddWithValue("$revisionMetadata", revisionMetadata);
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
    }

    private static async Task<Dictionary<(string Key, string Version), long>> InsertEvaluatorsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evaluators = new IPropertyEvaluator[]
        {
            new PrimeEvaluator(),
            new TriangularEvaluator(),
            new DecimalPalindromeEvaluator(),
            new DecimalNarcissisticEvaluator(),
        };
        var ids = new Dictionary<(string Key, string Version), long>();
        foreach (var evaluator in evaluators)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO evaluators(evaluator_key, version, description)
                VALUES ($key, $version, $description)
                RETURNING id;
                """;
            command.Parameters.AddWithValue("$key", evaluator.Key);
            command.Parameters.AddWithValue("$version", evaluator.Version);
            command.Parameters.AddWithValue("$description", $"deterministic {evaluator.Key} evaluator");
            ids[(evaluator.Key, evaluator.Version)] = Convert.ToInt64(
                await command.ExecuteScalarAsync(cancellationToken),
                CultureInfo.InvariantCulture);
        }

        return ids;
    }

    private static async Task<Dictionary<string, SeededProperty>> InsertPropertiesAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        IReadOnlyDictionary<string, long> sources,
        IReadOnlyDictionary<string, long> captures,
        string now,
        CancellationToken cancellationToken)
    {
        var ids = new Dictionary<string, SeededProperty>(StringComparer.Ordinal);
        foreach (var definition in InitialPropertyCatalog.Definitions)
        {
            await using var propertyCommand = connection.CreateCommand();
            propertyCommand.Transaction = transaction;
            propertyCommand.CommandText = """
                INSERT INTO properties(slug, display_name)
                VALUES ($slug, $displayName)
                RETURNING id;
                """;
            propertyCommand.Parameters.AddWithValue("$slug", definition.Slug);
            propertyCommand.Parameters.AddWithValue("$displayName", definition.DisplayName);
            var propertyId = Convert.ToInt64(
                await propertyCommand.ExecuteScalarAsync(cancellationToken),
                CultureInfo.InvariantCulture);

            await using var versionCommand = connection.CreateCommand();
            versionCommand.Transaction = transaction;
            versionCommand.CommandText = """
                INSERT INTO property_versions(
                    property_id,
                    version,
                    status,
                    definition,
                    aliases_json,
                    graph_mode,
                    evidence_contract_json,
                    validator_key,
                    evaluator_key,
                    created_at_utc,
                    accepted_at_utc)
                VALUES (
                    $propertyId,
                    1,
                    'draft',
                    $definition,
                    $aliasesJson,
                    $graphMode,
                    $evidenceContractJson,
                    $validatorKey,
                    $evaluatorKey,
                    $now,
                    NULL)
                RETURNING id;
                """;
            versionCommand.Parameters.AddWithValue("$propertyId", propertyId);
            versionCommand.Parameters.AddWithValue("$definition", definition.Definition);
            versionCommand.Parameters.AddWithValue("$aliasesJson", JsonSerializer.Serialize(definition.Aliases));
            versionCommand.Parameters.AddWithValue(
                "$graphMode",
                definition.GraphMode == GraphMode.SharedMembership ? "shared-membership" : "none");
            versionCommand.Parameters.AddWithValue("$evidenceContractJson", definition.EvidenceContractJson);
            versionCommand.Parameters.AddWithValue("$validatorKey", definition.ValidatorKey);
            versionCommand.Parameters.AddValue("$evaluatorKey", definition.EvaluatorKey);
            versionCommand.Parameters.AddWithValue("$now", now);
            var versionId = Convert.ToInt64(
                await versionCommand.ExecuteScalarAsync(cancellationToken),
                CultureInfo.InvariantCulture);

            foreach (var sourceSlug in PropertySources(definition.Slug))
            {
                await InsertPropertyCitationAsync(
                    connection,
                    transaction,
                    versionId,
                    sources[sourceSlug],
                    captures.GetValueOrDefault(sourceSlug),
                    sourceSlug.StartsWith("oeis-", StringComparison.Ordinal) ? "sequence" : "definition",
                    SourceLabel(sourceSlug),
                    cancellationToken);
            }

            await using var acceptCommand = connection.CreateCommand();
            acceptCommand.Transaction = transaction;
            acceptCommand.CommandText = """
                UPDATE property_versions
                SET status = 'accepted', accepted_at_utc = $now
                WHERE id = $versionId AND status = 'draft';
                """;
            acceptCommand.Parameters.AddWithValue("$now", now);
            acceptCommand.Parameters.AddWithValue("$versionId", versionId);
            await acceptCommand.ExecuteNonQueryAsync(cancellationToken);
            ids[definition.Slug] = new SeededProperty(propertyId, versionId);
        }

        return ids;
    }

    private static async Task InsertPropertyCitationAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long propertyVersionId,
        long sourceId,
        long sourceCaptureId,
        string role,
        string label,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO property_version_citations(
                property_version_id,
                source_id,
                source_capture_id,
                role,
                label,
                locator,
                public_safe)
            VALUES ($propertyVersionId, $sourceId, $captureId, $role, $label, NULL, 1);
            """;
        command.Parameters.AddWithValue("$propertyVersionId", propertyVersionId);
        command.Parameters.AddWithValue("$sourceId", sourceId);
        command.Parameters.AddValue("$captureId", sourceCaptureId == 0 ? null : sourceCaptureId);
        command.Parameters.AddWithValue("$role", role);
        command.Parameters.AddWithValue("$label", label);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertPopulationRunAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string runId,
        long propertyVersionId,
        long? evaluatorId,
        string mode,
        string inputsJson,
        string now,
        CancellationToken cancellationToken)
    {
        await using var prepare = connection.CreateCommand();
        prepare.Transaction = transaction;
        prepare.CommandText = """
            INSERT INTO population_runs(
                id,
                property_version_id,
                evaluator_id,
                mode,
                range_min,
                range_max,
                inputs_json,
                tool_version,
                status,
                prepared_at_utc,
                started_at_utc,
                completed_at_utc,
                error_text)
            VALUES (
                $id,
                $propertyVersionId,
                $evaluatorId,
                $mode,
                '1',
                '2000',
                $inputsJson,
                $toolVersion,
                'prepared',
                $now,
                NULL,
                NULL,
                NULL);
            """;
        prepare.Parameters.AddWithValue("$id", runId);
        prepare.Parameters.AddWithValue("$propertyVersionId", propertyVersionId);
        prepare.Parameters.AddValue("$evaluatorId", evaluatorId);
        prepare.Parameters.AddWithValue("$mode", mode);
        prepare.Parameters.AddWithValue("$inputsJson", inputsJson);
        prepare.Parameters.AddWithValue("$toolVersion", SeedVersion);
        prepare.Parameters.AddWithValue("$now", now);
        await prepare.ExecuteNonQueryAsync(cancellationToken);

        await using var start = connection.CreateCommand();
        start.Transaction = transaction;
        start.CommandText = """
            UPDATE population_runs
            SET status = 'running', started_at_utc = $now
            WHERE id = $id;
            """;
        start.Parameters.AddWithValue("$id", runId);
        start.Parameters.AddWithValue("$now", now);
        await start.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task FinishPopulationRunAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string runId,
        string now,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE population_runs
            SET status = 'succeeded', completed_at_utc = $now
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", runId);
        command.Parameters.AddWithValue("$now", now);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<long> InsertAcceptedClaimAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long integerId,
        SeededProperty property,
        ClaimOrigin origin,
        string populationRunId,
        string qualifierJson,
        EvidenceWitness? witness,
        IReadOnlyList<SeedCitation> citations,
        string now,
        CancellationToken cancellationToken)
    {
        await using var claimCommand = connection.CreateCommand();
        claimCommand.Transaction = transaction;
        claimCommand.CommandText = """
            INSERT INTO claims(integer_id, property_id)
            VALUES ($integerId, $propertyId)
            RETURNING id;
            """;
        claimCommand.Parameters.AddWithValue("$integerId", integerId);
        claimCommand.Parameters.AddWithValue("$propertyId", property.PropertyId);
        var claimId = Convert.ToInt64(
            await claimCommand.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);

        await using var revisionCommand = connection.CreateCommand();
        revisionCommand.Transaction = transaction;
        revisionCommand.CommandText = """
            INSERT INTO claim_revisions(
                claim_id,
                revision,
                property_version_id,
                origin_kind,
                population_run_id,
                qualifier_json,
                status,
                private_notes,
                created_at_utc,
                accepted_at_utc,
                supersedes_revision_id)
            VALUES (
                $claimId,
                1,
                $propertyVersionId,
                $origin,
                $runId,
                $qualifierJson,
                'draft',
                NULL,
                $now,
                NULL,
                NULL)
            RETURNING id;
            """;
        revisionCommand.Parameters.AddWithValue("$claimId", claimId);
        revisionCommand.Parameters.AddWithValue("$propertyVersionId", property.VersionId);
        revisionCommand.Parameters.AddWithValue("$origin", OriginText(origin));
        revisionCommand.Parameters.AddWithValue("$runId", populationRunId);
        revisionCommand.Parameters.AddWithValue("$qualifierJson", qualifierJson);
        revisionCommand.Parameters.AddWithValue("$now", now);
        var revisionId = Convert.ToInt64(
            await revisionCommand.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);

        if (witness is not null)
        {
            await using var witnessCommand = connection.CreateCommand();
            witnessCommand.Transaction = transaction;
            witnessCommand.CommandText = """
                INSERT INTO claim_witnesses(
                    claim_revision_id, kind, schema_version, payload_json, display_text)
                VALUES ($revisionId, $kind, $schemaVersion, $payloadJson, $displayText);
                """;
            witnessCommand.Parameters.AddWithValue("$revisionId", revisionId);
            witnessCommand.Parameters.AddWithValue("$kind", witness.Kind);
            witnessCommand.Parameters.AddWithValue("$schemaVersion", witness.SchemaVersion);
            witnessCommand.Parameters.AddWithValue("$payloadJson", witness.PayloadJson);
            witnessCommand.Parameters.AddWithValue("$displayText", witness.DisplayText);
            await witnessCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var citation in citations)
        {
            await using var citationCommand = connection.CreateCommand();
            citationCommand.Transaction = transaction;
            citationCommand.CommandText = """
                INSERT INTO claim_revision_citations(
                    claim_revision_id,
                    source_id,
                    source_capture_id,
                    role,
                    label,
                    locator,
                    public_safe)
                VALUES (
                    $revisionId,
                    $sourceId,
                    $sourceCaptureId,
                    $role,
                    $label,
                    NULL,
                    1);
                """;
            citationCommand.Parameters.AddWithValue("$revisionId", revisionId);
            citationCommand.Parameters.AddWithValue("$sourceId", citation.SourceId);
            citationCommand.Parameters.AddValue("$sourceCaptureId", citation.SourceCaptureId);
            citationCommand.Parameters.AddWithValue("$role", citation.Role);
            citationCommand.Parameters.AddWithValue("$label", citation.Label);
            await citationCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var acceptCommand = connection.CreateCommand();
        acceptCommand.Transaction = transaction;
        acceptCommand.CommandText = """
            UPDATE claim_revisions
            SET status = 'accepted', accepted_at_utc = $now
            WHERE id = $revisionId;
            """;
        acceptCommand.Parameters.AddWithValue("$now", now);
        acceptCommand.Parameters.AddWithValue("$revisionId", revisionId);
        await acceptCommand.ExecuteNonQueryAsync(cancellationToken);
        return revisionId;
    }

    private static async Task InsertCoverageAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long propertyVersionId,
        string runId,
        string basis,
        string now,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO coverage_claims(
                property_version_id,
                population_run_id,
                range_min,
                range_max,
                basis,
                exclusions_json,
                created_at_utc)
            VALUES ($propertyVersionId, $runId, '1', '2000', $basis, '[]', $now);
            """;
        command.Parameters.AddWithValue("$propertyVersionId", propertyVersionId);
        command.Parameters.AddWithValue("$runId", runId);
        command.Parameters.AddWithValue("$basis", basis);
        command.Parameters.AddWithValue("$now", now);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> InsertNarcissisticComparisonAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long propertyVersionId,
        string runId,
        long sourceCaptureId,
        IReadOnlyCollection<string> localValues,
        IReadOnlyCollection<string> sourceValues,
        string now,
        CancellationToken cancellationToken)
    {
        var missing = sourceValues.Except(localValues, StringComparer.Ordinal).Order().ToArray();
        var unexpected = localValues.Except(sourceValues, StringComparer.Ordinal).Order().ToArray();
        var status = missing.Length == 0 && unexpected.Length == 0 ? "matched" : "mismatched";

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO comparison_runs(
                id,
                property_version_id,
                population_run_id,
                source_capture_id,
                compared_at_utc,
                local_count,
                source_count,
                missing_json,
                unexpected_json,
                status)
            VALUES (
                $id,
                $propertyVersionId,
                $runId,
                $sourceCaptureId,
                $now,
                $localCount,
                $sourceCount,
                $missingJson,
                $unexpectedJson,
                $status);
            """;
        command.Parameters.AddWithValue("$id", $"{SeedVersion}-narcissistic-oeis-comparison");
        command.Parameters.AddWithValue("$propertyVersionId", propertyVersionId);
        command.Parameters.AddWithValue("$runId", runId);
        command.Parameters.AddWithValue("$sourceCaptureId", sourceCaptureId);
        command.Parameters.AddWithValue("$now", now);
        command.Parameters.AddWithValue("$localCount", localValues.Count);
        command.Parameters.AddWithValue("$sourceCount", sourceValues.Count);
        command.Parameters.AddWithValue("$missingJson", JsonSerializer.Serialize(missing));
        command.Parameters.AddWithValue("$unexpectedJson", JsonSerializer.Serialize(unexpected));
        command.Parameters.AddWithValue("$status", status);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return status == "matched";
    }

    private static async Task<int> InsertFriedmanPopulationAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        IReadOnlyDictionary<string, long> integerIds,
        SeededProperty propertyIds,
        IReadOnlyDictionary<string, long> sources,
        IReadOnlyDictionary<string, long> captures,
        FriedmanSeed seed,
        string now,
        CancellationToken cancellationToken)
    {
        var runId = $"{SeedVersion}-{PropertySlugs.DecimalFriedman}";
        await InsertPopulationRunAsync(
            connection,
            transaction,
            runId,
            propertyIds.VersionId,
            null,
            "bounded-import",
            JsonSerializer.Serialize(new
            {
                source = seed.Source,
                sourceCapture = captures["oeis-a036057"],
                expressionSource = seed.ExpressionSource,
                rangeMinimum = seed.RangeMinimum,
                rangeMaximum = seed.RangeMaximum,
            }),
            now,
            cancellationToken);

        var property = InitialPropertyCatalog.Get(PropertySlugs.DecimalFriedman);
        foreach (var entry in seed.Entries)
        {
            var integer = IntegerValue.Parse(entry.Value);
            var validation = FriedmanExpression.Validate(integer, entry.Expression, entry.Orderly);
            var qualifier = JsonSerializer.Serialize(new { orderly = validation.IsOrderly });
            EvidenceContractValidator.Validate(property, integer, qualifier, validation.Witness);
            await InsertAcceptedClaimAsync(
                connection,
                transaction,
                integerIds[entry.Value],
                propertyIds,
                ClaimOrigin.Imported,
                runId,
                qualifier,
                validation.Witness,
                [
                    new SeedCitation(
                        sources["oeis-a036057"],
                        captures["oeis-a036057"],
                        "membership",
                        "OEIS A036057"),
                    new SeedCitation(
                        sources["erich-friedman-2000"],
                        null,
                        "witness",
                        "Erich Friedman — Math Magic, August 2000"),
                ],
                now,
                cancellationToken);
        }

        await FinishPopulationRunAsync(connection, transaction, runId, now, cancellationToken);
        await InsertCoverageAsync(
            connection,
            transaction,
            propertyIds.VersionId,
            runId,
            "source-asserted-bounded",
            now,
            cancellationToken);
        return seed.Entries.Count;
    }

    private static async Task<long> InsertReferenceProfileAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long integerId,
        long sourceId,
        string now,
        CancellationToken cancellationToken)
    {
        await using var entryCommand = connection.CreateCommand();
        entryCommand.Transaction = transaction;
        entryCommand.CommandText = """
            INSERT INTO profile_entries(integer_id, slug)
            VALUES ($integerId, 'fine-structure-near-integer')
            RETURNING id;
            """;
        entryCommand.Parameters.AddWithValue("$integerId", integerId);
        var entryId = Convert.ToInt64(
            await entryCommand.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);

        await using var revisionCommand = connection.CreateCommand();
        revisionCommand.Transaction = transaction;
        revisionCommand.CommandText = """
            INSERT INTO profile_entry_revisions(
                profile_entry_id,
                revision,
                status,
                title,
                category,
                epistemic_register,
                body,
                private_notes,
                created_at_utc,
                accepted_at_utc,
                supersedes_revision_id)
            VALUES (
                $entryId,
                1,
                'draft',
                'the almost-integer',
                'scientific context',
                'sourced scientific fact',
                'the inverse fine-structure constant is close to 137 rather than exactly 137; its measured value continues beyond the integer.',
                NULL,
                $now,
                NULL,
                NULL)
            RETURNING id;
            """;
        revisionCommand.Parameters.AddWithValue("$entryId", entryId);
        revisionCommand.Parameters.AddWithValue("$now", now);
        var revisionId = Convert.ToInt64(
            await revisionCommand.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);

        await using var citationCommand = connection.CreateCommand();
        citationCommand.Transaction = transaction;
        citationCommand.CommandText = """
            INSERT INTO profile_entry_revision_citations(
                profile_entry_revision_id,
                source_id,
                source_capture_id,
                role,
                label,
                locator,
                public_safe)
            VALUES ($revisionId, $sourceId, NULL, 'measurement', 'NIST inverse fine-structure constant', NULL, 1);
            """;
        citationCommand.Parameters.AddWithValue("$revisionId", revisionId);
        citationCommand.Parameters.AddWithValue("$sourceId", sourceId);
        await citationCommand.ExecuteNonQueryAsync(cancellationToken);

        await using var acceptCommand = connection.CreateCommand();
        acceptCommand.Transaction = transaction;
        acceptCommand.CommandText = """
            UPDATE profile_entry_revisions
            SET status = 'accepted', accepted_at_utc = $now
            WHERE id = $revisionId;
            """;
        acceptCommand.Parameters.AddWithValue("$now", now);
        acceptCommand.Parameters.AddWithValue("$revisionId", revisionId);
        await acceptCommand.ExecuteNonQueryAsync(cancellationToken);
        return revisionId;
    }

    private static async Task StageEverythingAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long profileRevisionId,
        string now,
        CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            "INSERT INTO publication_property_selections(property_version_id, selected_at_utc) SELECT version.id, $now FROM property_versions version WHERE version.status = 'accepted' AND NOT EXISTS (SELECT 1 FROM comparison_runs comparison WHERE comparison.property_version_id = version.id AND comparison.status = 'mismatched');",
            "INSERT INTO publication_property_citation_selections(property_version_id, citation_id) SELECT citation.property_version_id, citation.id FROM property_version_citations citation JOIN publication_property_selections selection ON selection.property_version_id = citation.property_version_id WHERE citation.public_safe = 1;",
            "INSERT INTO publication_claim_selections(claim_revision_id, selected_at_utc) SELECT revision.id, $now FROM claim_revisions revision JOIN publication_property_selections selection ON selection.property_version_id = revision.property_version_id WHERE revision.status = 'accepted';",
            "INSERT INTO publication_claim_citation_selections(claim_revision_id, citation_id) SELECT citation.claim_revision_id, citation.id FROM claim_revision_citations citation JOIN publication_claim_selections selection ON selection.claim_revision_id = citation.claim_revision_id WHERE citation.public_safe = 1;",
            "INSERT INTO publication_profile_selections(profile_entry_revision_id, selected_at_utc) VALUES ($profileRevisionId, $now);",
            "INSERT INTO publication_profile_citation_selections(profile_entry_revision_id, citation_id) SELECT citation.profile_entry_revision_id, citation.id FROM profile_entry_revision_citations citation WHERE citation.profile_entry_revision_id = $profileRevisionId AND citation.public_safe = 1;",
        };

        foreach (var statement in statements)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = statement;
            command.Parameters.AddWithValue("$now", now);
            command.Parameters.AddWithValue("$profileRevisionId", profileRevisionId);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static void AssertExpectedCounts(IReadOnlyDictionary<string, int> counts)
    {
        var expected = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [PropertySlugs.Prime] = 303,
            [PropertySlugs.Triangular] = 62,
            [PropertySlugs.DecimalPalindrome] = 118,
            [PropertySlugs.DecimalNarcissistic] = 14,
            [PropertySlugs.DecimalFriedman] = 27,
        };
        foreach (var pair in expected)
        {
            if (!counts.TryGetValue(pair.Key, out var count) || count != pair.Value)
            {
                throw new InvalidOperationException(
                    $"{pair.Key} produced {count} claims; expected {pair.Value}.");
            }
        }
    }

    private static async Task<SeedResult> ReadExistingResultAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT property.slug, COUNT(revision.id)
                FROM properties property
                JOIN claims claim ON claim.property_id = property.id
                JOIN claim_revisions revision ON revision.claim_id = claim.id AND revision.status = 'accepted'
                GROUP BY property.slug;
                """;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                counts[reader.GetString(0)] = reader.GetInt32(1);
            }
        }

        return new SeedResult(
            true,
            await CountAsync(connection, "integers", cancellationToken),
            counts,
            await CountAsync(connection, "profile_entry_revisions", cancellationToken),
            await ReadDisagreementsAsync(connection, cancellationToken));
    }

    private static async Task<IReadOnlyList<string>> ReadDisagreementsAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var disagreements = new List<string>();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT DISTINCT property.slug
            FROM comparison_runs comparison
            JOIN property_versions version ON version.id = comparison.property_version_id
            JOIN properties property ON property.id = version.property_id
            WHERE comparison.status = 'mismatched'
            ORDER BY property.slug;
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            disagreements.Add(reader.GetString(0));
        }

        return disagreements;
    }

    private static async Task<int> CountAsync(
        SqliteConnection connection,
        string table,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {table};";
        return Convert.ToInt32(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
    }

    private static string[] PropertySources(string slug) => slug switch
    {
        PropertySlugs.Prime => ["oeis-a000040"],
        PropertySlugs.Triangular => ["oeis-a000217"],
        PropertySlugs.DecimalPalindrome => ["oeis-a002113"],
        PropertySlugs.DecimalNarcissistic => ["oeis-a005188"],
        PropertySlugs.DecimalFriedman => ["oeis-a036057", "oeis-a080035", "erich-friedman-2000"],
        _ => throw new InvalidOperationException($"source routing is missing for property '{slug}'."),
    };

    private static string SourceLabel(string slug) => slug switch
    {
        "oeis-a000040" => "OEIS A000040",
        "oeis-a000217" => "OEIS A000217",
        "oeis-a002113" => "OEIS A002113",
        "oeis-a005188" => "OEIS A005188",
        "oeis-a036057" => "OEIS A036057",
        "oeis-a080035" => "OEIS A080035",
        "erich-friedman-2000" => "Erich Friedman — Math Magic, August 2000",
        _ => slug,
    };

    private static string OriginText(ClaimOrigin origin) => origin switch
    {
        ClaimOrigin.Manual => "manual",
        ClaimOrigin.Computed => "computed",
        ClaimOrigin.Imported => "imported",
        _ => throw new ArgumentOutOfRangeException(nameof(origin)),
    };

    private static void ValidateSeedEnvelope(int schemaVersion, string minimum, string maximum)
    {
        if (schemaVersion != 1 || minimum != "1" || maximum != "2000")
        {
            throw new InvalidOperationException("seed data does not describe schema 1 over range 1 through 2000.");
        }
    }

    private static T ReadEmbeddedJson<T>(string filename)
    {
        var bytes = EmbeddedBytes(filename);
        return JsonSerializer.Deserialize<T>(bytes, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        }) ?? throw new InvalidOperationException($"seed resource '{filename}' is empty.");
    }

    private static byte[] EmbeddedBytes(string filename)
    {
        var assembly = typeof(CorpusSeeder).Assembly;
        var resourceName = assembly.GetManifestResourceNames().Single(name =>
            name.EndsWith($"Seed.{filename}", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"seed resource '{filename}' could not be opened.");
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private string UtcNowText() =>
        _timeProvider.GetUtcNow().ToString("O", CultureInfo.InvariantCulture);

    private sealed record SourceSeed(
        string Slug,
        string Title,
        string Author,
        string Url,
        string LicenseMetadata);

    private sealed record SeededProperty(long PropertyId, long VersionId);

    private sealed record SeedCitation(
        long SourceId,
        long? SourceCaptureId,
        string Role,
        string Label);

    private sealed record NarcissisticSeed(
        int SchemaVersion,
        string Source,
        string RetrievedAtUtc,
        string RangeMinimum,
        string RangeMaximum,
        IReadOnlyList<string> Values);

    private sealed record FriedmanSeed(
        int SchemaVersion,
        string Source,
        string OrderlySource,
        string ExpressionSource,
        string RetrievedAtUtc,
        string RangeMinimum,
        string RangeMaximum,
        IReadOnlyList<FriedmanSeedEntry> Entries);

    private sealed record FriedmanSeedEntry(string Value, string Expression, bool Orderly);
}
