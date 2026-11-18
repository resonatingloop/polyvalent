using Microsoft.Data.Sqlite;
using Polyvalent.Application;
using Polyvalent.Domain;

namespace Polyvalent.Data.Tests;

public sealed class RelationalResearchTests
{
    [Fact]
    public async Task Direct_relation_round_trips_freezes_and_supersedes_as_one_aggregate()
    {
        await using var fixture = TestDatabase.Create();
        await fixture.Database.InitializeAsync();
        var store = new PolyvalentStore(fixture.Database);
        var input = RelationInput("prime-index-137", "773 is the 137th prime");

        var firstId = await store.CreateDirectRelationDraftAsync(input);
        var dossier = await store.GetDossierAsync(IntegerValue.Parse("137"));
        Assert.Equal(firstId, Assert.Single(dossier.DirectRelations).RevisionId);

        await store.AcceptDirectRelationDraftAsync(firstId);
        var accepted = await store.GetDirectRelationAsync(input.Slug);
        Assert.True(accepted.IsActive);
        Assert.Equal(AuthoredRevisionStatus.Accepted, accepted.Status);

        await Assert.ThrowsAsync<SqliteException>(() =>
            ReplaceParticipantRoleAsync(fixture.Database, firstId));

        var secondId = await store.PrepareDirectRelationRevisionAsync(firstId);
        Assert.Equal(secondId, (await store.GetDirectRelationAsync(input.Slug)).RevisionId);
        await store.UpdateDirectRelationDraftAsync(
            secondId,
            input with { Proposition = "773 is prime number 137" });
        await store.AcceptDirectRelationDraftAsync(secondId);

        var current = await store.GetDirectRelationAsync(input.Slug);
        Assert.Equal(secondId, current.RevisionId);
        Assert.Equal(firstId, current.SupersedesRevisionId);
        Assert.Equal("773 is prime number 137", current.Proposition);
    }

    [Fact]
    public async Task Constellation_is_collective_and_appears_in_each_member_neighborhood()
    {
        await using var fixture = TestDatabase.Create();
        await fixture.Database.InitializeAsync();
        var store = new PolyvalentStore(fixture.Database);
        var input = new ConstellationDraftInput(
            "permutation-rehearsal",
            "permutation rehearsal",
            "a plainly fictional company for testing collective membership",
            "fictional fixture",
            "fixture-only account",
            new[]
            {
                new ConstellationMember(IntegerValue.Parse("137"), "member", null, 0),
                new ConstellationMember(IntegerValue.Parse("173"), "member", null, 1),
                new ConstellationMember(IntegerValue.Parse("317"), "member", null, 2),
                new ConstellationMember(IntegerValue.Parse("713"), "member", null, 3),
            });

        var id = await store.CreateConstellationDraftAsync(input);
        await store.AcceptConstellationDraftAsync(id);

        foreach (var value in new[] { "137", "173", "317", "713" })
        {
            var dossier = await store.GetDossierAsync(IntegerValue.Parse(value));
            Assert.Equal(id, Assert.Single(dossier.Constellations).RevisionId);
        }

        await AssertSqliteFailureAsync(
            fixture.Database,
            "UPDATE constellation_revisions SET account = 'tampered' WHERE id = $id;",
            id);
        await AssertSqliteFailureAsync(
            fixture.Database,
            "DELETE FROM constellation_members WHERE constellation_revision_id = $id;",
            id);
        await AssertSqliteFailureAsync(
            fixture.Database,
            "DELETE FROM constellation_revisions WHERE id = $id;",
            id);

        await using var connection = await fixture.Database.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                (SELECT COUNT(*) FROM direct_relations),
                (SELECT COUNT(*) FROM claims),
                (SELECT COUNT(*) FROM coverage_claims);
            """;
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal(0, reader.GetInt32(0));
        Assert.Equal(0, reader.GetInt32(1));
        Assert.Equal(0, reader.GetInt32(2));
    }

    [Fact]
    public async Task Acceptance_is_atomic_when_database_invariants_reject_the_aggregate()
    {
        await using var fixture = TestDatabase.Create();
        await fixture.Database.InitializeAsync();
        var store = new PolyvalentStore(fixture.Database);
        var id = await store.CreateDirectRelationDraftAsync(
            RelationInput("incomplete-relation", "") with
            {
                Participants =
                [new DirectRelationParticipant(IntegerValue.Parse("137"), "member", 0)],
            });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.AcceptDirectRelationDraftAsync(id));
        var draft = await store.GetDirectRelationAsync("incomplete-relation");
        Assert.Equal(AuthoredRevisionStatus.Draft, draft.Status);
        Assert.False(draft.IsActive);
    }

    [Fact]
    public async Task Competing_superseding_drafts_cannot_claim_the_same_next_revision()
    {
        await using var fixture = TestDatabase.Create();
        await fixture.Database.InitializeAsync();
        var store = new PolyvalentStore(fixture.Database);
        var first = await store.CreateDirectRelationDraftAsync(
            RelationInput("competing-corrections", "773 is the 137th prime"));
        await store.AcceptDirectRelationDraftAsync(first);
        var correctionA = await store.PrepareDirectRelationRevisionAsync(first);
        await Assert.ThrowsAsync<SqliteException>(() =>
            store.PrepareDirectRelationRevisionAsync(first));

        await store.AcceptDirectRelationDraftAsync(correctionA);
        var current = await store.GetDirectRelationAsync("competing-corrections");
        Assert.Equal(correctionA, current.RevisionId);
    }

    [Fact]
    public async Task One_source_may_contextually_support_both_cousins_but_not_after_acceptance()
    {
        await using var fixture = TestDatabase.Create();
        await fixture.Database.InitializeAsync();
        var store = new PolyvalentStore(fixture.Database);
        var sourceId = await store.CreateSourceAsync(new SourceInput(
            "shared-context",
            "shared contextual source",
            null,
            "https://example.test/shared-context",
            null));
        var otherSourceId = await store.CreateSourceAsync(new SourceInput(
            "other-context",
            "other contextual source",
            null,
            "https://example.test/other-context",
            null));
        var captureId = await InsertCaptureAsync(fixture.Database, sourceId);
        var relationId = await store.CreateDirectRelationDraftAsync(
            RelationInput("shared-source-relation", "773 is the 137th prime"));
        var constellationId = await store.CreateConstellationDraftAsync(
            new ConstellationDraftInput(
                "shared-source-company",
                "shared source company",
                "two integers held in one account",
                "interpretive",
                null,
                new[]
                {
                    new ConstellationMember(IntegerValue.Parse("137"), "member", null, 0),
                    new ConstellationMember(IntegerValue.Parse("773"), "member", null, 1),
                }));

        await store.CiteDirectRelationAsync(
            relationId, sourceId, null, "witness", "shared source", null);
        await store.CiteConstellationAsync(
            constellationId, sourceId, null, "context", "shared source", null);
        await Assert.ThrowsAsync<SqliteException>(() =>
            store.CiteConstellationAsync(
                constellationId,
                otherSourceId,
                captureId,
                "mismatch",
                "wrong source capture",
                null));
        await store.AcceptDirectRelationDraftAsync(relationId);
        await store.AcceptConstellationDraftAsync(constellationId);

        Assert.Single((await store.GetDirectRelationAsync("shared-source-relation")).Citations);
        Assert.Single((await store.GetConstellationAsync("shared-source-company")).Citations);
        await Assert.ThrowsAsync<SqliteException>(() =>
            store.CiteDirectRelationAsync(
                relationId, sourceId, null, "late", "late citation", null));
    }

    [Fact]
    public async Task Database_rejects_terminal_inserts_cross_type_slugs_and_draft_reparenting()
    {
        await using var fixture = TestDatabase.Create();
        await fixture.Database.InitializeAsync();
        var store = new PolyvalentStore(fixture.Database);
        var first = await store.CreateDirectRelationDraftAsync(
            RelationInput("guarded-lineage", "first guarded relation"));
        var second = await store.CreateDirectRelationDraftAsync(
            RelationInput("second-guarded-lineage", "second guarded relation"));

        await Assert.ThrowsAsync<SqliteException>(() =>
            store.CreateConstellationDraftAsync(new ConstellationDraftInput(
                "guarded-lineage",
                "conflicting slug",
                "must not enter another object type",
                "fixture",
                null,
                new[]
                {
                    new ConstellationMember(IntegerValue.Parse("137"), "member", null, 0),
                    new ConstellationMember(IntegerValue.Parse("773"), "member", null, 1),
                })));

        await using var connection = await fixture.Database.OpenConnectionAsync();
        await using (var terminalInsert = connection.CreateCommand())
        {
            terminalInsert.CommandText = """
                INSERT INTO direct_relation_revisions(
                    direct_relation_id, revision, status, directionality,
                    proposition, warrant, epistemic_register, created_at_utc)
                SELECT direct_relation_id, 2, 'accepted', directionality,
                       proposition, warrant, epistemic_register, created_at_utc
                FROM direct_relation_revisions
                WHERE id = $id;
                """;
            terminalInsert.Parameters.AddWithValue("$id", first);
            await Assert.ThrowsAsync<SqliteException>(() => terminalInsert.ExecuteNonQueryAsync());
        }

        await using (var reparent = connection.CreateCommand())
        {
            reparent.CommandText = """
                UPDATE direct_relation_participants
                SET direct_relation_revision_id = $second
                WHERE direct_relation_revision_id = $first
                  AND ordinal = 0;
                """;
            reparent.Parameters.AddWithValue("$first", first);
            reparent.Parameters.AddWithValue("$second", second);
            await Assert.ThrowsAsync<SqliteException>(() => reparent.ExecuteNonQueryAsync());
        }


        await using (var moveRevision = connection.CreateCommand())
        {
            moveRevision.CommandText = """
                UPDATE direct_relation_revisions
                SET direct_relation_id = (
                    SELECT direct_relation_id
                    FROM direct_relation_revisions
                    WHERE id = $second)
                WHERE id = $first;
                """;
            moveRevision.Parameters.AddWithValue("$first", first);
            moveRevision.Parameters.AddWithValue("$second", second);
            await Assert.ThrowsAsync<SqliteException>(() => moveRevision.ExecuteNonQueryAsync());
        }
    }

    private static DirectRelationDraftInput RelationInput(string slug, string proposition) =>
        new(
            slug,
            DirectRelationDirectionality.Directed,
            proposition,
            "prime enumeration",
            "computed",
            null,
            new[]
            {
                new DirectRelationParticipant(IntegerValue.Parse("137"), "index", 0),
                new DirectRelationParticipant(IntegerValue.Parse("773"), "prime", 1),
            });

    private static async Task ReplaceParticipantRoleAsync(SqliteDatabase database, long revisionId)
    {
        await using var connection = await database.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE direct_relation_participants
            SET role = 'tampered'
            WHERE direct_relation_revision_id = $id;
            """;
        command.Parameters.AddWithValue("$id", revisionId);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<long> InsertCaptureAsync(SqliteDatabase database, long sourceId)
    {
        await using var connection = await database.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO source_captures(
                source_id, uri, retrieved_at_utc, media_type, sha256)
            VALUES ($sourceId, 'https://example.test/capture',
                    '2026-11-17T00:00:00.0000000Z', 'text/plain',
                    'cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc')
            RETURNING id;
            """;
        command.Parameters.AddWithValue("$sourceId", sourceId);
        return (long)(await command.ExecuteScalarAsync() ?? 0L);
    }

    private static async Task AssertSqliteFailureAsync(
        SqliteDatabase database,
        string sql,
        long id)
    {
        await using var connection = await database.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$id", id);
        await Assert.ThrowsAsync<SqliteException>(() => command.ExecuteNonQueryAsync());
    }
}
