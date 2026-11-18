using Microsoft.Data.Sqlite;
using Polyvalent.Application;
using Polyvalent.Domain;

namespace Polyvalent.Data;

public sealed partial class PolyvalentStore
{
    public async Task<DirectRelationView> GetDirectRelationAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var canonical = ResearchLineageSlug.Parse(slug).ToString();
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        var revisionId = await FindPreferredRevisionAsync(
            connection,
            null,
            "direct_relations",
            "direct_relation_revisions",
            "direct_relation_id",
            canonical,
            cancellationToken);
        return await ReadDirectRelationAsync(connection, null, revisionId, cancellationToken);
    }

    public async Task<ConstellationView> GetConstellationAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var canonical = ResearchLineageSlug.Parse(slug).ToString();
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        var revisionId = await FindPreferredRevisionAsync(
            connection,
            null,
            "constellations",
            "constellation_revisions",
            "constellation_id",
            canonical,
            cancellationToken);
        return await ReadConstellationAsync(connection, null, revisionId, cancellationToken);
    }

    public async Task<long> CreateDirectRelationDraftAsync(
        DirectRelationDraftInput input,
        CancellationToken cancellationToken = default)
    {
        var slug = ResearchLineageSlug.Parse(input.Slug).ToString();
        AuthoredRelationalValidator.ValidateDraftParticipants(input.Participants);
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        var lineageId = await InsertLineageAsync(
            connection,
            transaction,
            "direct_relations",
            slug,
            cancellationToken);
        var revisionId = await InsertDirectRelationRevisionAsync(
            connection,
            transaction,
            lineageId,
            1,
            null,
            input,
            cancellationToken);
        await ReplaceDirectRelationParticipantsAsync(
            connection,
            transaction,
            revisionId,
            input.Participants,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return revisionId;
    }

    public async Task UpdateDirectRelationDraftAsync(
        long revisionId,
        DirectRelationDraftInput input,
        CancellationToken cancellationToken = default)
    {
        var slug = ResearchLineageSlug.Parse(input.Slug).ToString();
        AuthoredRelationalValidator.ValidateDraftParticipants(input.Participants);
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        await RequireRevisionSlugAsync(
            connection,
            transaction,
            "direct_relation_revisions",
            "direct_relation_id",
            "direct_relations",
            revisionId,
            slug,
            cancellationToken);

        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE direct_relation_revisions
                SET directionality = $directionality,
                    proposition = $proposition,
                    warrant = $warrant,
                    epistemic_register = $register,
                    private_notes = $notes
                WHERE id = $id AND status = 'draft';
                """;
            command.Parameters.AddWithValue("$directionality", input.Directionality.ToStorage());
            command.Parameters.AddWithValue("$proposition", input.Proposition.Trim());
            command.Parameters.AddWithValue("$warrant", input.Warrant.Trim());
            command.Parameters.AddWithValue("$register", input.EpistemicRegister.Trim());
            command.Parameters.AddValue("$notes", NullIfWhiteSpace(input.PrivateNotes));
            command.Parameters.AddWithValue("$id", revisionId);
            if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
            {
                throw new InvalidOperationException("the direct-relation draft is no longer mutable.");
            }
        }

        await ReplaceDirectRelationParticipantsAsync(
            connection,
            transaction,
            revisionId,
            input.Participants,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<long> PrepareDirectRelationRevisionAsync(
        long acceptedRevisionId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        var accepted = await ReadDirectRelationAsync(
            connection,
            transaction,
            acceptedRevisionId,
            cancellationToken);
        if (!accepted.IsActive || accepted.Status != AuthoredRevisionStatus.Accepted)
        {
            throw new InvalidOperationException("only the current accepted direct relation may be revised.");
        }

        var input = new DirectRelationDraftInput(
            accepted.Slug,
            accepted.Directionality,
            accepted.Proposition,
            accepted.Warrant,
            accepted.EpistemicRegister,
            accepted.PrivateNotes,
            accepted.Participants);
        var revisionId = await InsertDirectRelationRevisionAsync(
            connection,
            transaction,
            accepted.LineageId,
            accepted.Revision + 1,
            acceptedRevisionId,
            input,
            cancellationToken);
        await CopyRowsAsync(
            connection,
            transaction,
            """
            INSERT INTO direct_relation_participants(direct_relation_revision_id, integer_id, role, ordinal)
            SELECT $newId, integer_id, role, ordinal
            FROM direct_relation_participants
            WHERE direct_relation_revision_id = $oldId;
            """,
            acceptedRevisionId,
            revisionId,
            cancellationToken);
        await CopyRowsAsync(
            connection,
            transaction,
            """
            INSERT INTO direct_relation_revision_citations(
                direct_relation_revision_id, source_id, source_capture_id, role, label, locator)
            SELECT $newId, source_id, source_capture_id, role, label, locator
            FROM direct_relation_revision_citations
            WHERE direct_relation_revision_id = $oldId;
            """,
            acceptedRevisionId,
            revisionId,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return revisionId;
    }

    public Task AcceptDirectRelationDraftAsync(
        long revisionId,
        CancellationToken cancellationToken = default) =>
        AcceptDirectRelationCoreAsync(revisionId, cancellationToken);

    public Task WithdrawDirectRelationDraftAsync(
        long revisionId,
        CancellationToken cancellationToken = default) =>
        TransitionAuthoredDraftAsync(
            "direct_relation_revisions",
            revisionId,
            "withdrawn",
            cancellationToken);

    public Task<long> CiteDirectRelationAsync(
        long revisionId,
        long sourceId,
        long? sourceCaptureId,
        string role,
        string label,
        string? locator,
        CancellationToken cancellationToken = default) =>
        InsertAuthoredCitationAsync(
            "direct_relation_revision_citations",
            "direct_relation_revision_id",
            revisionId,
            sourceId,
            sourceCaptureId,
            role,
            label,
            locator,
            cancellationToken);

    public async Task<long> CreateConstellationDraftAsync(
        ConstellationDraftInput input,
        CancellationToken cancellationToken = default)
    {
        var slug = ResearchLineageSlug.Parse(input.Slug).ToString();
        AuthoredRelationalValidator.ValidateDraftMembers(input.Members);
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        var lineageId = await InsertLineageAsync(
            connection,
            transaction,
            "constellations",
            slug,
            cancellationToken);
        var revisionId = await InsertConstellationRevisionAsync(
            connection,
            transaction,
            lineageId,
            1,
            null,
            input,
            cancellationToken);
        await ReplaceConstellationMembersAsync(
            connection,
            transaction,
            revisionId,
            input.Members,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return revisionId;
    }

    public async Task UpdateConstellationDraftAsync(
        long revisionId,
        ConstellationDraftInput input,
        CancellationToken cancellationToken = default)
    {
        var slug = ResearchLineageSlug.Parse(input.Slug).ToString();
        AuthoredRelationalValidator.ValidateDraftMembers(input.Members);
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        await RequireRevisionSlugAsync(
            connection,
            transaction,
            "constellation_revisions",
            "constellation_id",
            "constellations",
            revisionId,
            slug,
            cancellationToken);

        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE constellation_revisions
                SET name = $name,
                    account = $account,
                    epistemic_register = $register,
                    private_notes = $notes
                WHERE id = $id AND status = 'draft';
                """;
            command.Parameters.AddWithValue("$name", input.Name.Trim());
            command.Parameters.AddWithValue("$account", input.Account.Trim());
            command.Parameters.AddWithValue("$register", input.EpistemicRegister.Trim());
            command.Parameters.AddValue("$notes", NullIfWhiteSpace(input.PrivateNotes));
            command.Parameters.AddWithValue("$id", revisionId);
            if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
            {
                throw new InvalidOperationException("the constellation draft is no longer mutable.");
            }
        }

        await ReplaceConstellationMembersAsync(
            connection,
            transaction,
            revisionId,
            input.Members,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<long> PrepareConstellationRevisionAsync(
        long acceptedRevisionId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        var accepted = await ReadConstellationAsync(
            connection,
            transaction,
            acceptedRevisionId,
            cancellationToken);
        if (!accepted.IsActive || accepted.Status != AuthoredRevisionStatus.Accepted)
        {
            throw new InvalidOperationException("only the current accepted constellation may be revised.");
        }

        var input = new ConstellationDraftInput(
            accepted.Slug,
            accepted.Name,
            accepted.Account,
            accepted.EpistemicRegister,
            accepted.PrivateNotes,
            accepted.Members);
        var revisionId = await InsertConstellationRevisionAsync(
            connection,
            transaction,
            accepted.LineageId,
            accepted.Revision + 1,
            acceptedRevisionId,
            input,
            cancellationToken);
        await CopyRowsAsync(
            connection,
            transaction,
            """
            INSERT INTO constellation_members(
                constellation_revision_id, integer_id, role, member_notes, ordinal)
            SELECT $newId, integer_id, role, member_notes, ordinal
            FROM constellation_members
            WHERE constellation_revision_id = $oldId;
            """,
            acceptedRevisionId,
            revisionId,
            cancellationToken);
        await CopyRowsAsync(
            connection,
            transaction,
            """
            INSERT INTO constellation_revision_citations(
                constellation_revision_id, source_id, source_capture_id, role, label, locator)
            SELECT $newId, source_id, source_capture_id, role, label, locator
            FROM constellation_revision_citations
            WHERE constellation_revision_id = $oldId;
            """,
            acceptedRevisionId,
            revisionId,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return revisionId;
    }

    public Task AcceptConstellationDraftAsync(
        long revisionId,
        CancellationToken cancellationToken = default) =>
        AcceptConstellationCoreAsync(revisionId, cancellationToken);

    public Task WithdrawConstellationDraftAsync(
        long revisionId,
        CancellationToken cancellationToken = default) =>
        TransitionAuthoredDraftAsync(
            "constellation_revisions",
            revisionId,
            "withdrawn",
            cancellationToken);

    public Task<long> CiteConstellationAsync(
        long revisionId,
        long sourceId,
        long? sourceCaptureId,
        string role,
        string label,
        string? locator,
        CancellationToken cancellationToken = default) =>
        InsertAuthoredCitationAsync(
            "constellation_revision_citations",
            "constellation_revision_id",
            revisionId,
            sourceId,
            sourceCaptureId,
            role,
            label,
            locator,
            cancellationToken);

    private async Task AcceptDirectRelationCoreAsync(long revisionId, CancellationToken cancellationToken)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        var relation = await ReadDirectRelationAsync(connection, transaction, revisionId, cancellationToken);
        if (relation.Status != AuthoredRevisionStatus.Draft)
        {
            throw new InvalidOperationException("only a direct-relation draft may be accepted.");
        }

        AuthoredRelationalValidator.ValidateDirectRelationForAcceptance(
            relation.Directionality,
            relation.Proposition,
            relation.Warrant,
            relation.EpistemicRegister,
            relation.Participants);
        await AcceptAuthoredRevisionAsync(
            connection,
            transaction,
            "direct_relation_revisions",
            revisionId,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task AcceptConstellationCoreAsync(long revisionId, CancellationToken cancellationToken)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        var constellation = await ReadConstellationAsync(connection, transaction, revisionId, cancellationToken);
        if (constellation.Status != AuthoredRevisionStatus.Draft)
        {
            throw new InvalidOperationException("only a constellation draft may be accepted.");
        }

        AuthoredRelationalValidator.ValidateConstellationForAcceptance(
            constellation.Name,
            constellation.Account,
            constellation.EpistemicRegister,
            constellation.Members);
        await AcceptAuthoredRevisionAsync(
            connection,
            transaction,
            "constellation_revisions",
            revisionId,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<DirectRelationView>> ReadDirectRelationsForIntegerAsync(
        SqliteConnection connection,
        long integerId,
        CancellationToken cancellationToken)
    {
        var ids = await ReadNeighborhoodRevisionIdsAsync(
            connection,
            "direct_relations",
            "direct_relation_revisions",
            "direct_relation_participants",
            "direct_relation_id",
            "direct_relation_revision_id",
            integerId,
            cancellationToken);
        var result = new List<DirectRelationView>(ids.Count);
        foreach (var id in ids)
        {
            result.Add(await ReadDirectRelationAsync(connection, null, id, cancellationToken));
        }

        return result;
    }

    private async Task<IReadOnlyList<ConstellationView>> ReadConstellationsForIntegerAsync(
        SqliteConnection connection,
        long integerId,
        CancellationToken cancellationToken)
    {
        var ids = await ReadNeighborhoodRevisionIdsAsync(
            connection,
            "constellations",
            "constellation_revisions",
            "constellation_members",
            "constellation_id",
            "constellation_revision_id",
            integerId,
            cancellationToken);
        var result = new List<ConstellationView>(ids.Count);
        foreach (var id in ids)
        {
            result.Add(await ReadConstellationAsync(connection, null, id, cancellationToken));
        }

        return result;
    }

    private static async Task<IReadOnlyList<long>> ReadNeighborhoodRevisionIdsAsync(
        SqliteConnection connection,
        string lineageTable,
        string revisionTable,
        string childTable,
        string lineageForeignKey,
        string revisionForeignKey,
        long integerId,
        CancellationToken cancellationToken)
    {
        var ids = new List<long>();
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT revision.id
            FROM {revisionTable} revision
            JOIN {lineageTable} lineage ON lineage.id = revision.{lineageForeignKey}
            JOIN {childTable} child ON child.{revisionForeignKey} = revision.id
            WHERE child.integer_id = $integerId
              AND (revision.status = 'draft' OR lineage.active_revision_id = revision.id)
            ORDER BY CASE revision.status WHEN 'draft' THEN 0 ELSE 1 END,
                     lineage.slug,
                     revision.revision DESC;
            """;
        command.Parameters.AddWithValue("$integerId", integerId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            ids.Add(reader.GetInt64(0));
        }

        return ids;
    }

    private static async Task<long> FindPreferredRevisionAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string lineageTable,
        string revisionTable,
        string lineageForeignKey,
        string slug,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"""
            SELECT COALESCE(
                (SELECT revision.id
                 FROM {revisionTable} revision
                 WHERE revision.{lineageForeignKey} = lineage.id
                   AND revision.status = 'draft'
                 ORDER BY revision.revision DESC
                 LIMIT 1),
                lineage.active_revision_id)
            FROM {lineageTable} lineage
            WHERE lineage.slug = $slug;
            """;
        command.Parameters.AddWithValue("$slug", slug);
        var value = await command.ExecuteScalarAsync(cancellationToken);
        if (value is null or DBNull)
        {
            throw new KeyNotFoundException($"authored research object '{slug}' was not found.");
        }

        return Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task<DirectRelationView> ReadDirectRelationAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        long revisionId,
        CancellationToken cancellationToken)
    {
        long lineageId;
        string slug;
        int revision;
        AuthoredRevisionStatus status;
        bool isActive;
        DirectRelationDirectionality directionality;
        string proposition;
        string warrant;
        string register;
        string? notes;
        long? supersedes;
        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                SELECT lineage.id, lineage.slug, revision.revision, revision.status,
                       COALESCE(lineage.active_revision_id = revision.id, 0),
                       revision.directionality, revision.proposition, revision.warrant,
                       revision.epistemic_register, revision.private_notes,
                       revision.supersedes_revision_id
                FROM direct_relation_revisions revision
                JOIN direct_relations lineage ON lineage.id = revision.direct_relation_id
                WHERE revision.id = $id;
                """;
            command.Parameters.AddWithValue("$id", revisionId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new KeyNotFoundException($"direct-relation revision {revisionId} was not found.");
            }

            lineageId = reader.GetInt64(0);
            slug = reader.GetString(1);
            revision = reader.GetInt32(2);
            status = AuthoredRevisionStatusExtensions.ParseStorage(reader.GetString(3));
            isActive = reader.GetInt64(4) == 1;
            directionality = DirectRelationDirectionalityExtensions.ParseStorage(reader.GetString(5));
            proposition = reader.GetString(6);
            warrant = reader.GetString(7);
            register = reader.GetString(8);
            notes = reader.IsDBNull(9) ? null : reader.GetString(9);
            supersedes = reader.IsDBNull(10) ? null : reader.GetInt64(10);
        }

        var participants = new List<DirectRelationParticipant>();
        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                SELECT integer.canonical_value, participant.role, participant.ordinal
                FROM direct_relation_participants participant
                JOIN integers integer ON integer.id = participant.integer_id
                WHERE participant.direct_relation_revision_id = $id
                ORDER BY participant.ordinal;
                """;
            command.Parameters.AddWithValue("$id", revisionId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                participants.Add(new DirectRelationParticipant(
                    IntegerValue.Parse(reader.GetString(0)),
                    reader.GetString(1),
                    reader.GetInt32(2)));
            }
        }

        var citations = await ReadAuthoredCitationsAsync(
            connection,
            transaction,
            "direct_relation_revision_citations",
            "direct_relation_revision_id",
            revisionId,
            cancellationToken);
        return new DirectRelationView(
            lineageId,
            revisionId,
            slug,
            revision,
            status,
            isActive,
            directionality,
            proposition,
            warrant,
            register,
            notes,
            supersedes,
            participants,
            citations);
    }

    private static async Task<ConstellationView> ReadConstellationAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        long revisionId,
        CancellationToken cancellationToken)
    {
        long lineageId;
        string slug;
        int revision;
        AuthoredRevisionStatus status;
        bool isActive;
        string name;
        string account;
        string register;
        string? notes;
        long? supersedes;
        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                SELECT lineage.id, lineage.slug, revision.revision, revision.status,
                       COALESCE(lineage.active_revision_id = revision.id, 0),
                       revision.name, revision.account, revision.epistemic_register,
                       revision.private_notes, revision.supersedes_revision_id
                FROM constellation_revisions revision
                JOIN constellations lineage ON lineage.id = revision.constellation_id
                WHERE revision.id = $id;
                """;
            command.Parameters.AddWithValue("$id", revisionId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new KeyNotFoundException($"constellation revision {revisionId} was not found.");
            }

            lineageId = reader.GetInt64(0);
            slug = reader.GetString(1);
            revision = reader.GetInt32(2);
            status = AuthoredRevisionStatusExtensions.ParseStorage(reader.GetString(3));
            isActive = reader.GetInt64(4) == 1;
            name = reader.GetString(5);
            account = reader.GetString(6);
            register = reader.GetString(7);
            notes = reader.IsDBNull(8) ? null : reader.GetString(8);
            supersedes = reader.IsDBNull(9) ? null : reader.GetInt64(9);
        }

        var members = new List<ConstellationMember>();
        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                SELECT integer.canonical_value, member.role, member.member_notes, member.ordinal
                FROM constellation_members member
                JOIN integers integer ON integer.id = member.integer_id
                WHERE member.constellation_revision_id = $id
                ORDER BY member.ordinal;
                """;
            command.Parameters.AddWithValue("$id", revisionId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                members.Add(new ConstellationMember(
                    IntegerValue.Parse(reader.GetString(0)),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? null : reader.GetString(2),
                    reader.GetInt32(3)));
            }
        }

        var citations = await ReadAuthoredCitationsAsync(
            connection,
            transaction,
            "constellation_revision_citations",
            "constellation_revision_id",
            revisionId,
            cancellationToken);
        return new ConstellationView(
            lineageId,
            revisionId,
            slug,
            revision,
            status,
            isActive,
            name,
            account,
            register,
            notes,
            supersedes,
            members,
            citations);
    }

    private static async Task<IReadOnlyList<AuthoredCitationView>> ReadAuthoredCitationsAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string table,
        string revisionForeignKey,
        long revisionId,
        CancellationToken cancellationToken)
    {
        var result = new List<AuthoredCitationView>();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"""
            SELECT citation.id, citation.source_id, citation.source_capture_id,
                   citation.role, citation.label, citation.locator,
                   source.title, source.canonical_url
            FROM {table} citation
            JOIN sources source ON source.id = citation.source_id
            WHERE citation.{revisionForeignKey} = $id
            ORDER BY citation.id;
            """;
        command.Parameters.AddWithValue("$id", revisionId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new AuthoredCitationView(
                reader.GetInt64(0),
                reader.GetInt64(1),
                reader.IsDBNull(2) ? null : reader.GetInt64(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.GetString(6),
                reader.GetString(7)));
        }

        return result;
    }

    private async Task<long> InsertLineageAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string table,
        string slug,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"""
            INSERT INTO {table}(slug, created_at_utc)
            VALUES ($slug, $createdAtUtc)
            RETURNING id;
            """;
        command.Parameters.AddWithValue("$slug", slug);
        command.Parameters.AddWithValue("$createdAtUtc", UtcNowText());
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(cancellationToken),
            System.Globalization.CultureInfo.InvariantCulture);
    }

    private async Task<long> InsertDirectRelationRevisionAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long lineageId,
        int revision,
        long? supersedesRevisionId,
        DirectRelationDraftInput input,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO direct_relation_revisions(
                direct_relation_id, revision, status, directionality,
                proposition, warrant, epistemic_register, private_notes,
                created_at_utc, supersedes_revision_id)
            VALUES ($lineageId, $revision, 'draft', $directionality,
                    $proposition, $warrant, $register, $notes,
                    $createdAtUtc, $supersedes)
            RETURNING id;
            """;
        command.Parameters.AddWithValue("$lineageId", lineageId);
        command.Parameters.AddWithValue("$revision", revision);
        command.Parameters.AddWithValue("$directionality", input.Directionality.ToStorage());
        command.Parameters.AddWithValue("$proposition", input.Proposition.Trim());
        command.Parameters.AddWithValue("$warrant", input.Warrant.Trim());
        command.Parameters.AddWithValue("$register", input.EpistemicRegister.Trim());
        command.Parameters.AddValue("$notes", NullIfWhiteSpace(input.PrivateNotes));
        command.Parameters.AddWithValue("$createdAtUtc", UtcNowText());
        command.Parameters.AddValue("$supersedes", supersedesRevisionId);
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(cancellationToken),
            System.Globalization.CultureInfo.InvariantCulture);
    }

    private async Task<long> InsertConstellationRevisionAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long lineageId,
        int revision,
        long? supersedesRevisionId,
        ConstellationDraftInput input,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO constellation_revisions(
                constellation_id, revision, status, name, account,
                epistemic_register, private_notes, created_at_utc,
                supersedes_revision_id)
            VALUES ($lineageId, $revision, 'draft', $name, $account,
                    $register, $notes, $createdAtUtc, $supersedes)
            RETURNING id;
            """;
        command.Parameters.AddWithValue("$lineageId", lineageId);
        command.Parameters.AddWithValue("$revision", revision);
        command.Parameters.AddWithValue("$name", input.Name.Trim());
        command.Parameters.AddWithValue("$account", input.Account.Trim());
        command.Parameters.AddWithValue("$register", input.EpistemicRegister.Trim());
        command.Parameters.AddValue("$notes", NullIfWhiteSpace(input.PrivateNotes));
        command.Parameters.AddWithValue("$createdAtUtc", UtcNowText());
        command.Parameters.AddValue("$supersedes", supersedesRevisionId);
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(cancellationToken),
            System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task ReplaceDirectRelationParticipantsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long revisionId,
        IReadOnlyList<DirectRelationParticipant> participants,
        CancellationToken cancellationToken)
    {
        await DeleteChildrenAsync(
            connection,
            transaction,
            "direct_relation_participants",
            "direct_relation_revision_id",
            revisionId,
            cancellationToken);
        foreach (var participant in participants.OrderBy(item => item.Ordinal))
        {
            var integerId = await GetOrCreateIntegerIdAsync(
                connection,
                transaction,
                participant.Integer,
                cancellationToken);
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO direct_relation_participants(
                    direct_relation_revision_id, integer_id, role, ordinal)
                VALUES ($revisionId, $integerId, $role, $ordinal);
                """;
            command.Parameters.AddWithValue("$revisionId", revisionId);
            command.Parameters.AddWithValue("$integerId", integerId);
            command.Parameters.AddWithValue("$role", participant.Role.Trim());
            command.Parameters.AddWithValue("$ordinal", participant.Ordinal);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task ReplaceConstellationMembersAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long revisionId,
        IReadOnlyList<ConstellationMember> members,
        CancellationToken cancellationToken)
    {
        await DeleteChildrenAsync(
            connection,
            transaction,
            "constellation_members",
            "constellation_revision_id",
            revisionId,
            cancellationToken);
        foreach (var member in members.OrderBy(item => item.Ordinal))
        {
            var integerId = await GetOrCreateIntegerIdAsync(
                connection,
                transaction,
                member.Integer,
                cancellationToken);
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO constellation_members(
                    constellation_revision_id, integer_id, role, member_notes, ordinal)
                VALUES ($revisionId, $integerId, $role, $notes, $ordinal);
                """;
            command.Parameters.AddWithValue("$revisionId", revisionId);
            command.Parameters.AddWithValue("$integerId", integerId);
            command.Parameters.AddWithValue("$role", member.Role.Trim());
            command.Parameters.AddValue("$notes", NullIfWhiteSpace(member.MemberNotes));
            command.Parameters.AddWithValue("$ordinal", member.Ordinal);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task DeleteChildrenAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string table,
        string revisionForeignKey,
        long revisionId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"DELETE FROM {table} WHERE {revisionForeignKey} = $id;";
        command.Parameters.AddWithValue("$id", revisionId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task RequireRevisionSlugAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string revisionTable,
        string lineageForeignKey,
        string lineageTable,
        long revisionId,
        string slug,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"""
            SELECT lineage.slug
            FROM {revisionTable} revision
            JOIN {lineageTable} lineage ON lineage.id = revision.{lineageForeignKey}
            WHERE revision.id = $id;
            """;
        command.Parameters.AddWithValue("$id", revisionId);
        var actual = await command.ExecuteScalarAsync(cancellationToken) as string
            ?? throw new KeyNotFoundException($"authored revision {revisionId} was not found.");
        if (!string.Equals(actual, slug, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("a revision cannot be moved to another lineage slug.");
        }
    }

    private async Task AcceptAuthoredRevisionAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string table,
        long revisionId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"""
            UPDATE {table}
            SET status = 'accepted', accepted_at_utc = $acceptedAtUtc
            WHERE id = $id AND status = 'draft';
            """;
        command.Parameters.AddWithValue("$acceptedAtUtc", UtcNowText());
        command.Parameters.AddWithValue("$id", revisionId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
        {
            throw new InvalidOperationException("the authored draft is no longer mutable.");
        }
    }

    private async Task TransitionAuthoredDraftAsync(
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
            throw new InvalidOperationException("the authored draft is no longer mutable.");
        }
    }

    private async Task<long> InsertAuthoredCitationAsync(
        string table,
        string revisionForeignKey,
        long revisionId,
        long sourceId,
        long? sourceCaptureId,
        string role,
        string label,
        string? locator,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(label))
        {
            throw new InvalidOperationException("an authored citation requires a role and label.");
        }

        await using var connection = await _database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            INSERT INTO {table}(
                {revisionForeignKey}, source_id, source_capture_id, role, label, locator)
            VALUES ($revisionId, $sourceId, $sourceCaptureId, $role, $label, $locator)
            RETURNING id;
            """;
        command.Parameters.AddWithValue("$revisionId", revisionId);
        command.Parameters.AddWithValue("$sourceId", sourceId);
        command.Parameters.AddValue("$sourceCaptureId", sourceCaptureId);
        command.Parameters.AddWithValue("$role", role.Trim());
        command.Parameters.AddWithValue("$label", label.Trim());
        command.Parameters.AddValue("$locator", NullIfWhiteSpace(locator));
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(cancellationToken),
            System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task CopyRowsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string sql,
        long oldRevisionId,
        long newRevisionId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.Parameters.AddWithValue("$oldId", oldRevisionId);
        command.Parameters.AddWithValue("$newId", newRevisionId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
