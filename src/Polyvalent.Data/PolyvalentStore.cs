using System.Globalization;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Polyvalent.Application;
using Polyvalent.Domain;

namespace Polyvalent.Data;

public sealed class PolyvalentStore : IResearchRepository
{
    private readonly SqliteDatabase _database;
    private readonly TimeProvider _timeProvider;

    public PolyvalentStore(SqliteDatabase database, TimeProvider? timeProvider = null)
    {
        _database = database;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SqliteDatabase Database => _database;

    public async Task<IntegerDossier> GetDossierAsync(
        IntegerValue integer,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        var properties = await ReadPropertiesAsync(connection, cancellationToken);
        var integerId = await FindIntegerIdAsync(connection, integer, cancellationToken);
        var claims = new List<ClaimView>();

        foreach (var property in properties)
        {
            var covered = await IsCoveredAsync(
                connection,
                property.PropertyVersionId,
                integer,
                cancellationToken);
            var actual = integerId is null
                ? []
                : await ReadClaimsAsync(
                    connection,
                    integerId.Value,
                    property,
                    covered,
                    cancellationToken);

            if (actual.Count == 0)
            {
                claims.Add(new ClaimView(
                    0,
                    property.PropertyVersionId,
                    property.Slug,
                    property.DisplayName,
                    covered ? KnowledgeState.CoveredNegative : KnowledgeState.Unknown,
                    null,
                    null,
                    "{}",
                    null,
                    null,
                    false,
                    []));
            }
            else
            {
                claims.AddRange(actual);
            }
        }

        var profiles = integerId is null
            ? []
            : await ReadProfilesAsync(connection, integerId.Value, cancellationToken);
        return new IntegerDossier(integer, claims, profiles);
    }

    public async Task<IReadOnlyList<PropertyOption>> GetPropertiesAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        return await ReadPropertiesAsync(connection, cancellationToken);
    }

    public async Task<long> CreateManualClaimDraftAsync(
        IntegerValue integer,
        string propertySlug,
        string qualifierJson,
        EvidenceWitness? witness,
        string? privateNotes,
        CancellationToken cancellationToken = default)
    {
        EnsureJsonObject(qualifierJson, "qualifier");
        if (witness is not null)
        {
            EnsureJsonObject(witness.PayloadJson, "witness");
        }

        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var sqlTransaction = (SqliteTransaction)transaction;

        var integerId = await GetOrCreateIntegerIdAsync(
            connection,
            sqlTransaction,
            integer,
            cancellationToken);
        var (propertyId, propertyVersionId) = await FindAcceptedPropertyAsync(
            connection,
            sqlTransaction,
            propertySlug,
            cancellationToken);
        var claimId = await GetOrCreateClaimIdAsync(
            connection,
            sqlTransaction,
            integerId,
            propertyId,
            cancellationToken);
        var revision = await NextRevisionAsync(
            connection,
            sqlTransaction,
            "claim_revisions",
            "claim_id",
            claimId,
            cancellationToken);
        var supersedesId = await FindActiveAcceptedClaimRevisionAsync(
            connection,
            sqlTransaction,
            claimId,
            cancellationToken);

        await using var insertRevision = connection.CreateCommand();
        insertRevision.Transaction = sqlTransaction;
        insertRevision.CommandText = """
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
                $revision,
                $propertyVersionId,
                'manual',
                NULL,
                $qualifierJson,
                'draft',
                $privateNotes,
                $createdAtUtc,
                NULL,
                $supersedesRevisionId)
            RETURNING id;
            """;
        insertRevision.Parameters.AddWithValue("$claimId", claimId);
        insertRevision.Parameters.AddWithValue("$revision", revision);
        insertRevision.Parameters.AddWithValue("$propertyVersionId", propertyVersionId);
        insertRevision.Parameters.AddWithValue("$qualifierJson", qualifierJson);
        insertRevision.Parameters.AddValue("$privateNotes", privateNotes);
        insertRevision.Parameters.AddWithValue("$createdAtUtc", UtcNowText());
        insertRevision.Parameters.AddValue("$supersedesRevisionId", supersedesId);
        var revisionId = Convert.ToInt64(
            await insertRevision.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);

        if (witness is not null)
        {
            await InsertWitnessAsync(
                connection,
                sqlTransaction,
                revisionId,
                witness,
                cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return revisionId;
    }

    public async Task UpdateManualClaimDraftAsync(
        long revisionId,
        string qualifierJson,
        EvidenceWitness? witness,
        string? privateNotes,
        CancellationToken cancellationToken = default)
    {
        EnsureJsonObject(qualifierJson, "qualifier");
        if (witness is not null)
        {
            EnsureJsonObject(witness.PayloadJson, "witness");
        }

        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var sqlTransaction = (SqliteTransaction)transaction;
        var material = await ReadClaimValidationMaterialAsync(
            connection,
            sqlTransaction,
            revisionId,
            cancellationToken);
        if (material.Status != "draft" || material.Origin != ClaimOrigin.Manual)
        {
            throw new InvalidOperationException("only manual draft claims can be edited.");
        }

        await using (var update = connection.CreateCommand())
        {
            update.Transaction = sqlTransaction;
            update.CommandText = """
                UPDATE claim_revisions
                SET qualifier_json = $qualifierJson, private_notes = $privateNotes
                WHERE id = $revisionId AND status = 'draft' AND origin_kind = 'manual';
                """;
            update.Parameters.AddWithValue("$qualifierJson", qualifierJson);
            update.Parameters.AddValue("$privateNotes", NullIfWhiteSpace(privateNotes));
            update.Parameters.AddWithValue("$revisionId", revisionId);
            if (await update.ExecuteNonQueryAsync(cancellationToken) != 1)
            {
                throw new InvalidOperationException("the manual claim draft is no longer mutable.");
            }
        }

        await using (var removeWitness = connection.CreateCommand())
        {
            removeWitness.Transaction = sqlTransaction;
            removeWitness.CommandText = "DELETE FROM claim_witnesses WHERE claim_revision_id = $revisionId;";
            removeWitness.Parameters.AddWithValue("$revisionId", revisionId);
            await removeWitness.ExecuteNonQueryAsync(cancellationToken);
        }

        if (witness is not null)
        {
            await InsertWitnessAsync(
                connection,
                sqlTransaction,
                revisionId,
                witness,
                cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task AcceptClaimDraftAsync(
        long revisionId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var sqlTransaction = (SqliteTransaction)transaction;
        var material = await ReadClaimValidationMaterialAsync(
            connection,
            sqlTransaction,
            revisionId,
            cancellationToken);
        if (material.Status != "draft" || material.Origin != ClaimOrigin.Manual)
        {
            throw new InvalidOperationException("only manual draft claims can be accepted through the research service.");
        }

        var property = InitialPropertyCatalog.Get(material.PropertySlug);
        EvidenceContractValidator.Validate(
            property,
            material.Integer,
            material.QualifierJson,
            material.Witness);

        if (property.Slug == PropertySlugs.DecimalFriedman)
        {
            await RequireCitationAsync(
                connection,
                sqlTransaction,
                "claim_revision_citations",
                "claim_revision_id",
                revisionId,
                cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.Transaction = sqlTransaction;
        command.CommandText = """
            UPDATE claim_revisions
            SET status = 'accepted', accepted_at_utc = $acceptedAtUtc
            WHERE id = $revisionId AND status = 'draft';
            """;
        command.Parameters.AddWithValue("$acceptedAtUtc", UtcNowText());
        command.Parameters.AddWithValue("$revisionId", revisionId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
        {
            throw new InvalidOperationException("the claim draft was not found or is no longer mutable.");
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public Task WithdrawClaimDraftAsync(long revisionId, CancellationToken cancellationToken = default) =>
        TransitionDraftAsync("claim_revisions", revisionId, "withdrawn", cancellationToken);

    public async Task<long> CreateSourceAsync(
        SourceInput input,
        CancellationToken cancellationToken = default)
    {
        ValidateSlug(input.Slug, nameof(input.Slug));
        ValidatePublicUrl(input.Url);
        if (string.IsNullOrWhiteSpace(input.Title))
        {
            throw new ArgumentException("a source title is required.", nameof(input));
        }

        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO sources(
                slug,
                title,
                author,
                canonical_url,
                license_metadata,
                private_notes,
                created_at_utc)
            VALUES ($slug, $title, $author, $url, $licenseMetadata, NULL, $createdAtUtc)
            RETURNING id;
            """;
        command.Parameters.AddWithValue("$slug", input.Slug);
        command.Parameters.AddWithValue("$title", input.Title.Trim());
        command.Parameters.AddValue("$author", NullIfWhiteSpace(input.Author));
        command.Parameters.AddWithValue("$url", input.Url);
        command.Parameters.AddValue("$licenseMetadata", NullIfWhiteSpace(input.LicenseMetadata));
        command.Parameters.AddWithValue("$createdAtUtc", UtcNowText());
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
    }

    public Task<long> CiteClaimAsync(
        long revisionId,
        long sourceId,
        string role,
        string label,
        string? locator,
        bool publicSafe,
        CancellationToken cancellationToken = default) =>
        InsertCitationAsync(
            "claim_revision_citations",
            "claim_revision_id",
            revisionId,
            sourceId,
            role,
            label,
            locator,
            publicSafe,
            cancellationToken);

    public async Task<long> CreateProfileDraftAsync(
        ProfileDraftInput input,
        CancellationToken cancellationToken = default)
    {
        ValidateSlug(input.Slug, nameof(input.Slug));
        if (string.IsNullOrWhiteSpace(input.Title)
            || string.IsNullOrWhiteSpace(input.Category)
            || string.IsNullOrWhiteSpace(input.EpistemicRegister))
        {
            throw new ArgumentException("profile title, category, and epistemic register are required.", nameof(input));
        }

        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var sqlTransaction = (SqliteTransaction)transaction;
        var integerId = await GetOrCreateIntegerIdAsync(
            connection,
            sqlTransaction,
            input.Integer,
            cancellationToken);

        await using var entryCommand = connection.CreateCommand();
        entryCommand.Transaction = sqlTransaction;
        entryCommand.CommandText = """
            INSERT INTO profile_entries(integer_id, slug)
            VALUES ($integerId, $slug)
            ON CONFLICT(integer_id, slug) DO UPDATE SET slug = excluded.slug
            RETURNING id;
            """;
        entryCommand.Parameters.AddWithValue("$integerId", integerId);
        entryCommand.Parameters.AddWithValue("$slug", input.Slug);
        var entryId = Convert.ToInt64(
            await entryCommand.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);

        var revision = await NextRevisionAsync(
            connection,
            sqlTransaction,
            "profile_entry_revisions",
            "profile_entry_id",
            entryId,
            cancellationToken);
        var supersedesId = input.SupersedesRevisionId
            ?? await FindActiveAcceptedProfileRevisionAsync(
                connection,
                sqlTransaction,
                entryId,
                cancellationToken);

        if (supersedesId is not null)
        {
            await EnsureProfileRevisionBelongsToEntryAsync(
                connection,
                sqlTransaction,
                supersedesId.Value,
                entryId,
                cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.Transaction = sqlTransaction;
        command.CommandText = """
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
                $revision,
                'draft',
                $title,
                $category,
                $epistemicRegister,
                $body,
                $privateNotes,
                $createdAtUtc,
                NULL,
                $supersedesRevisionId)
            RETURNING id;
            """;
        command.Parameters.AddWithValue("$entryId", entryId);
        command.Parameters.AddWithValue("$revision", revision);
        command.Parameters.AddWithValue("$title", input.Title.Trim());
        command.Parameters.AddWithValue("$category", input.Category.Trim());
        command.Parameters.AddWithValue("$epistemicRegister", input.EpistemicRegister.Trim());
        command.Parameters.AddWithValue("$body", input.Body);
        command.Parameters.AddValue("$privateNotes", NullIfWhiteSpace(input.PrivateNotes));
        command.Parameters.AddWithValue("$createdAtUtc", UtcNowText());
        command.Parameters.AddValue("$supersedesRevisionId", supersedesId);
        var revisionId = Convert.ToInt64(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
        await transaction.CommitAsync(cancellationToken);
        return revisionId;
    }

    public async Task UpdateProfileDraftAsync(
        long revisionId,
        string title,
        string category,
        string epistemicRegister,
        string body,
        string? privateNotes,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title)
            || string.IsNullOrWhiteSpace(category)
            || string.IsNullOrWhiteSpace(epistemicRegister))
        {
            throw new ArgumentException("profile title, category, and epistemic register are required.");
        }

        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE profile_entry_revisions
            SET title = $title,
                category = $category,
                epistemic_register = $epistemicRegister,
                body = $body,
                private_notes = $privateNotes
            WHERE id = $revisionId AND status = 'draft';
            """;
        command.Parameters.AddWithValue("$title", title.Trim());
        command.Parameters.AddWithValue("$category", category.Trim());
        command.Parameters.AddWithValue("$epistemicRegister", epistemicRegister.Trim());
        command.Parameters.AddWithValue("$body", body);
        command.Parameters.AddValue("$privateNotes", NullIfWhiteSpace(privateNotes));
        command.Parameters.AddWithValue("$revisionId", revisionId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
        {
            throw new InvalidOperationException("the profile draft was not found or is no longer mutable.");
        }
    }

    public async Task AcceptProfileDraftAsync(
        long revisionId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var sqlTransaction = (SqliteTransaction)transaction;
        await RequireCitationAsync(
            connection,
            sqlTransaction,
            "profile_entry_revision_citations",
            "profile_entry_revision_id",
            revisionId,
            cancellationToken);

        await using var command = connection.CreateCommand();
        command.Transaction = sqlTransaction;
        command.CommandText = """
            UPDATE profile_entry_revisions
            SET status = 'accepted', accepted_at_utc = $acceptedAtUtc
            WHERE id = $revisionId
              AND status = 'draft'
              AND length(trim(title)) > 0
              AND length(trim(body)) > 0;
            """;
        command.Parameters.AddWithValue("$acceptedAtUtc", UtcNowText());
        command.Parameters.AddWithValue("$revisionId", revisionId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
        {
            throw new InvalidOperationException("the profile draft is incomplete, missing, or no longer mutable.");
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public Task WithdrawProfileDraftAsync(long revisionId, CancellationToken cancellationToken = default) =>
        TransitionDraftAsync("profile_entry_revisions", revisionId, "withdrawn", cancellationToken);

    public Task<long> CiteProfileAsync(
        long revisionId,
        long sourceId,
        string role,
        string label,
        string? locator,
        bool publicSafe,
        CancellationToken cancellationToken = default) =>
        InsertCitationAsync(
            "profile_entry_revision_citations",
            "profile_entry_revision_id",
            revisionId,
            sourceId,
            role,
            label,
            locator,
            publicSafe,
            cancellationToken);

    public async Task StageClaimAsync(long revisionId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var sqlTransaction = (SqliteTransaction)transaction;
        var propertyVersionId = await ScalarLongAsync(
            connection,
            sqlTransaction,
            "SELECT property_version_id FROM claim_revisions WHERE id = $id;",
            revisionId,
            cancellationToken);

        await StagePropertyAsync(connection, sqlTransaction, propertyVersionId, cancellationToken);
        await ExecuteAsync(
            connection,
            sqlTransaction,
            """
            DELETE FROM publication_claim_selections
            WHERE claim_revision_id <> $id
              AND claim_revision_id IN (
                  SELECT sibling.id
                  FROM claim_revisions sibling
                  WHERE sibling.claim_id = (
                      SELECT current.claim_id
                      FROM claim_revisions current
                      WHERE current.id = $id));
            """,
            revisionId,
            cancellationToken,
            includeNow: false);
        await ExecuteAsync(
            connection,
            sqlTransaction,
            """
            INSERT INTO publication_claim_selections(claim_revision_id, selected_at_utc)
            VALUES ($id, $now)
            ON CONFLICT(claim_revision_id) DO UPDATE SET selected_at_utc = excluded.selected_at_utc;
            """,
            revisionId,
            cancellationToken);
        await ExecuteAsync(
            connection,
            sqlTransaction,
            """
            INSERT OR IGNORE INTO publication_claim_citation_selections(claim_revision_id, citation_id)
            SELECT $id, id
            FROM claim_revision_citations
            WHERE claim_revision_id = $id AND public_safe = 1;
            """,
            revisionId,
            cancellationToken,
            includeNow: false);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task StageProfileAsync(long revisionId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var sqlTransaction = (SqliteTransaction)transaction;
        await ExecuteAsync(
            connection,
            sqlTransaction,
            """
            DELETE FROM publication_profile_selections
            WHERE profile_entry_revision_id <> $id
              AND profile_entry_revision_id IN (
                  SELECT sibling.id
                  FROM profile_entry_revisions sibling
                  WHERE sibling.profile_entry_id = (
                      SELECT current.profile_entry_id
                      FROM profile_entry_revisions current
                      WHERE current.id = $id));
            """,
            revisionId,
            cancellationToken,
            includeNow: false);
        await ExecuteAsync(
            connection,
            sqlTransaction,
            """
            INSERT INTO publication_profile_selections(profile_entry_revision_id, selected_at_utc)
            VALUES ($id, $now)
            ON CONFLICT(profile_entry_revision_id) DO UPDATE SET selected_at_utc = excluded.selected_at_utc;
            """,
            revisionId,
            cancellationToken);
        await ExecuteAsync(
            connection,
            sqlTransaction,
            """
            INSERT OR IGNORE INTO publication_profile_citation_selections(profile_entry_revision_id, citation_id)
            SELECT $id, id
            FROM profile_entry_revision_citations
            WHERE profile_entry_revision_id = $id AND public_safe = 1;
            """,
            revisionId,
            cancellationToken,
            includeNow: false);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<StagingSummary> GetStagingSummaryAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        return new StagingSummary(
            await CountAsync(connection, "publication_property_selections", cancellationToken),
            await CountAsync(connection, "publication_claim_selections", cancellationToken),
            await CountAsync(connection, "publication_profile_selections", cancellationToken),
            await CountAsync(connection, "publication_property_citation_selections", cancellationToken)
                + await CountAsync(connection, "publication_claim_citation_selections", cancellationToken)
                + await CountAsync(connection, "publication_profile_citation_selections", cancellationToken));
    }

    public async Task AddPrivacySentinelAsync(
        string value,
        string label,
        CancellationToken cancellationToken = default)
    {
        if (value.Length < 8)
        {
            throw new ArgumentException("privacy sentinels must be at least eight characters.", nameof(value));
        }

        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO privacy_sentinels(value, label, created_at_utc)
            VALUES ($value, $label, $now);
            """;
        command.Parameters.AddWithValue("$value", value);
        command.Parameters.AddWithValue("$label", label);
        command.Parameters.AddWithValue("$now", UtcNowText());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<IReadOnlyList<PropertyOption>> ReadPropertiesAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var properties = new List<PropertyOption>();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT pv.id, p.slug, p.display_name, pv.definition, pv.graph_mode
            FROM property_versions pv
            JOIN properties p ON p.id = pv.property_id
            WHERE pv.status = 'accepted'
            ORDER BY p.display_name;
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            properties.Add(new PropertyOption(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4) == "shared-membership"
                    ? GraphMode.SharedMembership
                    : GraphMode.None));
        }

        return properties;
    }

    private static async Task<List<ClaimView>> ReadClaimsAsync(
        SqliteConnection connection,
        long integerId,
        PropertyOption property,
        bool covered,
        CancellationToken cancellationToken)
    {
        var claims = new List<ClaimView>();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                cr.id,
                cr.origin_kind,
                cr.status,
                cr.qualifier_json,
                cr.private_notes,
                cw.kind,
                cw.schema_version,
                cw.payload_json,
                cw.display_text,
                EXISTS(
                    SELECT 1 FROM publication_claim_selections selection
                    WHERE selection.claim_revision_id = cr.id)
            FROM claims c
            JOIN claim_revisions cr ON cr.claim_id = c.id
            LEFT JOIN claim_witnesses cw ON cw.claim_revision_id = cr.id
            WHERE c.integer_id = $integerId
              AND c.property_id = (
                  SELECT property_id FROM property_versions WHERE id = $propertyVersionId)
              AND cr.status <> 'withdrawn'
              AND (
                  cr.status = 'draft'
                  OR cr.status = 'disputed'
                  OR (
                      cr.status = 'accepted'
                      AND NOT EXISTS (
                          SELECT 1
                          FROM claim_revisions newer
                          WHERE newer.supersedes_revision_id = cr.id
                            AND newer.status = 'accepted')))
            ORDER BY
                CASE cr.status WHEN 'draft' THEN 0 WHEN 'accepted' THEN 1 ELSE 2 END,
                cr.revision DESC;
            """;
        command.Parameters.AddWithValue("$integerId", integerId);
        command.Parameters.AddWithValue("$propertyVersionId", property.PropertyVersionId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var revisionId = reader.GetInt64(0);
            var status = reader.GetString(2);
            var witness = reader.IsDBNull(5)
                ? null
                : new EvidenceWitness(
                    reader.GetString(5),
                    reader.GetInt32(6),
                    reader.GetString(7),
                    reader.GetString(8));
            var citations = await ReadClaimCitationsAsync(
                connection,
                revisionId,
                cancellationToken);
            claims.Add(new ClaimView(
                revisionId,
                property.PropertyVersionId,
                property.Slug,
                property.DisplayName,
                status == "accepted"
                    ? KnowledgeState.Positive
                    : covered ? KnowledgeState.CoveredNegative : KnowledgeState.Unknown,
                ParseOrigin(reader.GetString(1)),
                status,
                reader.GetString(3),
                witness,
                reader.NullableString(4),
                reader.GetBoolean(9),
                citations));
        }

        return claims;
    }

    private static async Task<List<ProfileEntryView>> ReadProfilesAsync(
        SqliteConnection connection,
        long integerId,
        CancellationToken cancellationToken)
    {
        var profiles = new List<ProfileEntryView>();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                revision.id,
                entry.slug,
                revision.status,
                revision.title,
                revision.category,
                revision.epistemic_register,
                revision.body,
                revision.private_notes,
                EXISTS(
                    SELECT 1 FROM publication_profile_selections selection
                    WHERE selection.profile_entry_revision_id = revision.id)
            FROM profile_entries entry
            JOIN profile_entry_revisions revision ON revision.profile_entry_id = entry.id
            WHERE entry.integer_id = $integerId
              AND revision.status <> 'withdrawn'
              AND (
                  revision.status = 'draft'
                  OR revision.status = 'disputed'
                  OR (
                      revision.status = 'accepted'
                      AND NOT EXISTS (
                          SELECT 1
                          FROM profile_entry_revisions newer
                          WHERE newer.supersedes_revision_id = revision.id
                            AND newer.status = 'accepted')))
            ORDER BY revision.created_at_utc DESC;
            """;
        command.Parameters.AddWithValue("$integerId", integerId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var revisionId = reader.GetInt64(0);
            var citations = await ReadProfileCitationsAsync(
                connection,
                revisionId,
                cancellationToken);
            profiles.Add(new ProfileEntryView(
                revisionId,
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6),
                reader.NullableString(7),
                reader.GetBoolean(8),
                citations));
        }

        return profiles;
    }

    private static Task<IReadOnlyList<CitationView>> ReadClaimCitationsAsync(
        SqliteConnection connection,
        long revisionId,
        CancellationToken cancellationToken) =>
        ReadCitationsAsync(
            connection,
            "claim_revision_citations",
            "publication_claim_citation_selections",
            "claim_revision_id",
            revisionId,
            cancellationToken);

    private static Task<IReadOnlyList<CitationView>> ReadProfileCitationsAsync(
        SqliteConnection connection,
        long revisionId,
        CancellationToken cancellationToken) =>
        ReadCitationsAsync(
            connection,
            "profile_entry_revision_citations",
            "publication_profile_citation_selections",
            "profile_entry_revision_id",
            revisionId,
            cancellationToken);

    private static async Task<IReadOnlyList<CitationView>> ReadCitationsAsync(
        SqliteConnection connection,
        string table,
        string selectionTable,
        string targetColumn,
        long targetId,
        CancellationToken cancellationToken)
    {
        var citations = new List<CitationView>();
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT citation.id, citation.label, citation.locator, citation.role,
                   source.canonical_url, citation.public_safe,
                   EXISTS(
                       SELECT 1 FROM {selectionTable} selection
                       WHERE selection.citation_id = citation.id)
            FROM {table} citation
            JOIN sources source ON source.id = citation.source_id
            WHERE citation.{targetColumn} = $targetId
            ORDER BY citation.id;
            """;
        command.Parameters.AddWithValue("$targetId", targetId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            citations.Add(new CitationView(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.NullableString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetBoolean(5),
                reader.GetBoolean(6)));
        }

        return citations;
    }

    private static async Task<bool> IsCoveredAsync(
        SqliteConnection connection,
        long propertyVersionId,
        IntegerValue integer,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT coverage.range_min, coverage.range_max, coverage.exclusions_json
            FROM coverage_claims coverage
            JOIN population_runs run ON run.id = coverage.population_run_id
            WHERE coverage.property_version_id = $propertyVersionId
              AND run.status = 'succeeded';
            """;
        command.Parameters.AddWithValue("$propertyVersionId", propertyVersionId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var minimum = IntegerValue.Parse(reader.GetString(0));
            var maximum = IntegerValue.Parse(reader.GetString(1));
            var exclusions = JsonSerializer.Deserialize<string[]>(reader.GetString(2)) ?? [];
            if (integer.CompareTo(minimum) >= 0
                && integer.CompareTo(maximum) <= 0
                && !exclusions.Contains(integer.ToString(), StringComparer.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<long?> FindIntegerIdAsync(
        SqliteConnection connection,
        IntegerValue integer,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id FROM integers WHERE canonical_value = $value;";
        command.Parameters.AddWithValue("$value", integer.ToString());
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null ? null : Convert.ToInt64(result, CultureInfo.InvariantCulture);
    }

    private static async Task<long> GetOrCreateIntegerIdAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        IntegerValue integer,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO integers(canonical_value)
            VALUES ($value)
            ON CONFLICT(canonical_value) DO UPDATE SET canonical_value = excluded.canonical_value
            RETURNING id;
            """;
        command.Parameters.AddWithValue("$value", integer.ToString());
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
    }

    private static async Task<(long PropertyId, long PropertyVersionId)> FindAcceptedPropertyAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string propertySlug,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT property.id, version.id
            FROM properties property
            JOIN property_versions version ON version.property_id = property.id
            WHERE property.slug = $slug AND version.status = 'accepted'
            ORDER BY version.version DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$slug", propertySlug);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException($"accepted property '{propertySlug}' was not found.");
        }

        return (reader.GetInt64(0), reader.GetInt64(1));
    }

    private static async Task<long> GetOrCreateClaimIdAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long integerId,
        long propertyId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO claims(integer_id, property_id)
            VALUES ($integerId, $propertyId)
            ON CONFLICT(integer_id, property_id) DO UPDATE SET property_id = excluded.property_id
            RETURNING id;
            """;
        command.Parameters.AddWithValue("$integerId", integerId);
        command.Parameters.AddWithValue("$propertyId", propertyId);
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
    }

    private static async Task<int> NextRevisionAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string table,
        string parentColumn,
        long parentId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"SELECT COALESCE(MAX(revision), 0) + 1 FROM {table} WHERE {parentColumn} = $parentId;";
        command.Parameters.AddWithValue("$parentId", parentId);
        return Convert.ToInt32(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
    }

    private static async Task<long?> FindActiveAcceptedClaimRevisionAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long claimId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT revision.id
            FROM claim_revisions revision
            WHERE revision.claim_id = $claimId
              AND revision.status = 'accepted'
              AND NOT EXISTS (
                  SELECT 1 FROM claim_revisions newer
                  WHERE newer.supersedes_revision_id = revision.id
                    AND newer.status = 'accepted')
            ORDER BY revision.revision DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$claimId", claimId);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null ? null : Convert.ToInt64(result, CultureInfo.InvariantCulture);
    }

    private static async Task<long?> FindActiveAcceptedProfileRevisionAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long entryId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT revision.id
            FROM profile_entry_revisions revision
            WHERE revision.profile_entry_id = $entryId
              AND revision.status = 'accepted'
              AND NOT EXISTS (
                  SELECT 1 FROM profile_entry_revisions newer
                  WHERE newer.supersedes_revision_id = revision.id
                    AND newer.status = 'accepted')
            ORDER BY revision.revision DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$entryId", entryId);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null ? null : Convert.ToInt64(result, CultureInfo.InvariantCulture);
    }

    private static async Task EnsureProfileRevisionBelongsToEntryAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long revisionId,
        long entryId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT COUNT(*)
            FROM profile_entry_revisions
            WHERE id = $revisionId
              AND profile_entry_id = $entryId
              AND status = 'accepted';
            """;
        command.Parameters.AddWithValue("$revisionId", revisionId);
        command.Parameters.AddWithValue("$entryId", entryId);
        if (Convert.ToInt32(
                await command.ExecuteScalarAsync(cancellationToken),
                CultureInfo.InvariantCulture) != 1)
        {
            throw new InvalidOperationException("the superseded profile revision is not an accepted revision of this entry.");
        }
    }

    private static async Task InsertWitnessAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long revisionId,
        EvidenceWitness witness,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO claim_witnesses(
                claim_revision_id, kind, schema_version, payload_json, display_text)
            VALUES ($revisionId, $kind, $schemaVersion, $payloadJson, $displayText);
            """;
        command.Parameters.AddWithValue("$revisionId", revisionId);
        command.Parameters.AddWithValue("$kind", witness.Kind);
        command.Parameters.AddWithValue("$schemaVersion", witness.SchemaVersion);
        command.Parameters.AddWithValue("$payloadJson", witness.PayloadJson);
        command.Parameters.AddWithValue("$displayText", witness.DisplayText);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<ClaimValidationMaterial> ReadClaimValidationMaterialAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long revisionId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT
                integer.canonical_value,
                property.slug,
                revision.origin_kind,
                revision.status,
                revision.qualifier_json,
                witness.kind,
                witness.schema_version,
                witness.payload_json,
                witness.display_text
            FROM claim_revisions revision
            JOIN claims claim ON claim.id = revision.claim_id
            JOIN integers integer ON integer.id = claim.integer_id
            JOIN properties property ON property.id = claim.property_id
            LEFT JOIN claim_witnesses witness ON witness.claim_revision_id = revision.id
            WHERE revision.id = $revisionId;
            """;
        command.Parameters.AddWithValue("$revisionId", revisionId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("the claim revision was not found.");
        }

        var witness = reader.IsDBNull(5)
            ? null
            : new EvidenceWitness(
                reader.GetString(5),
                reader.GetInt32(6),
                reader.GetString(7),
                reader.GetString(8));
        return new ClaimValidationMaterial(
            IntegerValue.Parse(reader.GetString(0)),
            reader.GetString(1),
            ParseOrigin(reader.GetString(2)),
            reader.GetString(3),
            reader.GetString(4),
            witness);
    }

    private async Task TransitionDraftAsync(
        string table,
        long revisionId,
        string status,
        CancellationToken cancellationToken)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"UPDATE {table} SET status = $status WHERE id = $id AND status = 'draft';";
        command.Parameters.AddWithValue("$status", status);
        command.Parameters.AddWithValue("$id", revisionId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
        {
            throw new InvalidOperationException("the draft was not found or is no longer mutable.");
        }
    }

    private async Task<long> InsertCitationAsync(
        string table,
        string targetColumn,
        long targetId,
        long sourceId,
        string role,
        string label,
        string? locator,
        bool publicSafe,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("citation role and label are required.");
        }

        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            INSERT INTO {table}(
                {targetColumn}, source_id, source_capture_id, role, label, locator, public_safe)
            VALUES ($targetId, $sourceId, NULL, $role, $label, $locator, $publicSafe)
            RETURNING id;
            """;
        command.Parameters.AddWithValue("$targetId", targetId);
        command.Parameters.AddWithValue("$sourceId", sourceId);
        command.Parameters.AddWithValue("$role", role.Trim());
        command.Parameters.AddWithValue("$label", label.Trim());
        command.Parameters.AddValue("$locator", NullIfWhiteSpace(locator));
        command.Parameters.AddWithValue("$publicSafe", publicSafe ? 1 : 0);
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
    }

    private static async Task RequireCitationAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string table,
        string targetColumn,
        long revisionId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"SELECT COUNT(*) FROM {table} WHERE {targetColumn} = $revisionId;";
        command.Parameters.AddWithValue("$revisionId", revisionId);
        if (Convert.ToInt32(
                await command.ExecuteScalarAsync(cancellationToken),
                CultureInfo.InvariantCulture) == 0)
        {
            throw new InvalidOperationException("accepted evidence requires at least one citation.");
        }
    }

    private async Task StagePropertyAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long propertyVersionId,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            connection,
            transaction,
            """
            INSERT INTO publication_property_selections(property_version_id, selected_at_utc)
            VALUES ($id, $now)
            ON CONFLICT(property_version_id) DO UPDATE SET selected_at_utc = excluded.selected_at_utc;
            """,
            propertyVersionId,
            cancellationToken);
        await ExecuteAsync(
            connection,
            transaction,
            """
            INSERT OR IGNORE INTO publication_property_citation_selections(property_version_id, citation_id)
            SELECT $id, id
            FROM property_version_citations
            WHERE property_version_id = $id AND public_safe = 1;
            """,
            propertyVersionId,
            cancellationToken,
            includeNow: false);
    }

    private async Task ExecuteAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sql,
        long id,
        CancellationToken cancellationToken,
        bool includeNow = true)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.Parameters.AddWithValue("$id", id);
        if (includeNow)
        {
            command.Parameters.AddWithValue("$now", UtcNowText());
        }

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<long> ScalarLongAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sql,
        long id,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.Parameters.AddWithValue("$id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken)
            ?? throw new InvalidOperationException("the selected record was not found.");
        return Convert.ToInt64(result, CultureInfo.InvariantCulture);
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

    private static void ValidatePublicUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("source urls must be absolute http or https urls.", nameof(value));
        }
    }

    private static void ValidateSlug(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value[0] is < 'a' or > 'z'
            || value.Any(character =>
                character is not (>= 'a' and <= 'z')
                && character is not (>= '0' and <= '9')
                && character != '-'))
        {
            throw new ArgumentException("slugs use lowercase ascii letters, digits, and hyphens.", parameterName);
        }
    }

    private static void EnsureJsonObject(string json, string label)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException($"the {label} payload must be a json object.", nameof(json));
        }
    }

    private static ClaimOrigin ParseOrigin(string value) => value switch
    {
        "manual" => ClaimOrigin.Manual,
        "computed" => ClaimOrigin.Computed,
        "imported" => ClaimOrigin.Imported,
        _ => throw new InvalidOperationException($"unknown claim origin '{value}'."),
    };

    private static object? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private string UtcNowText() =>
        _timeProvider.GetUtcNow().ToString("O", CultureInfo.InvariantCulture);

    private sealed record ClaimValidationMaterial(
        IntegerValue Integer,
        string PropertySlug,
        ClaimOrigin Origin,
        string Status,
        string QualifierJson,
        EvidenceWitness? Witness);
}
