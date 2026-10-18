using Microsoft.Data.Sqlite;
using Polyvalent.Domain;

namespace Polyvalent.Data.Tests;

public sealed class CorpusSeederTests
{
    [Fact]
    public async Task Seed_population_has_expected_counts_and_is_idempotent()
    {
        await using var fixture = TestDatabase.Create();
        var seeder = new CorpusSeeder(fixture.Database);

        var first = await seeder.SeedAsync();
        var second = await seeder.SeedAsync();

        Assert.False(first.AlreadySeeded);
        Assert.True(second.AlreadySeeded);
        Assert.Equal(2000, first.IntegerCount);
        Assert.Equal(303, first.PropertyCounts[PropertySlugs.Prime]);
        Assert.Equal(62, first.PropertyCounts[PropertySlugs.Triangular]);
        Assert.Equal(118, first.PropertyCounts[PropertySlugs.DecimalPalindrome]);
        Assert.Equal(14, first.PropertyCounts[PropertySlugs.DecimalNarcissistic]);
        Assert.Equal(27, first.PropertyCounts[PropertySlugs.DecimalFriedman]);
        Assert.Empty(first.Disagreements);
    }

    [Fact]
    public async Task Dossier_distinguishes_positive_covered_negative_and_unknown()
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        var store = new PolyvalentStore(fixture.Database);

        var known = await store.GetDossierAsync(IntegerValue.Parse("153"));
        var outsideCoverage = await store.GetDossierAsync(IntegerValue.Parse("2001"));

        var friedman = Assert.Single(known.Claims, claim =>
            claim.PropertySlug == PropertySlugs.DecimalFriedman
            && claim.State == KnowledgeState.Positive
            && claim.Witness!.DisplayText.Contains("3 × 51", StringComparison.Ordinal));
        Assert.True(friedman.IsStaged);
        Assert.NotEmpty(friedman.Citations);
        Assert.All(friedman.Citations, citation => Assert.True(citation.IsStaged));
        Assert.Contains(known.Claims, claim =>
            claim.PropertySlug == PropertySlugs.Prime
            && claim.State == KnowledgeState.CoveredNegative);
        Assert.All(outsideCoverage.Claims, claim => Assert.Equal(KnowledgeState.Unknown, claim.State));
    }

    [Fact]
    public async Task Accepted_ledger_records_reject_in_place_mutation()
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        await using var connection = await fixture.Database.OpenConnectionAsync();

        await AssertSqliteFailureAsync(connection, "UPDATE claim_revisions SET private_notes = 'rewrite' WHERE id = (SELECT id FROM claim_revisions WHERE status = 'accepted' LIMIT 1);");
        await AssertSqliteFailureAsync(connection, "UPDATE source_captures SET uri = 'https://example.test/drift';");
        await AssertSqliteFailureAsync(connection, "UPDATE population_runs SET error_text = 'rewrite' WHERE id = (SELECT id FROM population_runs WHERE status = 'succeeded' LIMIT 1);");
    }

    [Fact]
    public async Task Incomplete_private_draft_does_not_become_positive_or_stageable()
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        var store = new PolyvalentStore(fixture.Database);
        var revisionId = await store.CreateManualClaimDraftAsync(
            IntegerValue.Parse("154"),
            PropertySlugs.DecimalFriedman,
            "{}",
            null,
            "private unfinished thought");

        await Assert.ThrowsAsync<InvalidOperationException>(() => store.AcceptClaimDraftAsync(revisionId));
        await Assert.ThrowsAsync<SqliteException>(() => store.StageClaimAsync(revisionId));
        var dossier = await store.GetDossierAsync(IntegerValue.Parse("154"));

        Assert.DoesNotContain(dossier.Claims, claim =>
            claim.PropertySlug == PropertySlugs.DecimalFriedman
            && claim.State == KnowledgeState.Positive);
    }

    [Fact]
    public async Task Staging_a_superseding_claim_replaces_the_stale_selection()
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        var store = new PolyvalentStore(fixture.Database);
        var first = await store.CreateManualClaimDraftAsync(
            IntegerValue.Parse("2017"),
            PropertySlugs.Prime,
            "{}",
            null,
            "first private reading");
        await store.AcceptClaimDraftAsync(first);
        await store.StageClaimAsync(first);
        var second = await store.CreateManualClaimDraftAsync(
            IntegerValue.Parse("2017"),
            PropertySlugs.Prime,
            "{}",
            null,
            "corrected private reading");
        await store.AcceptClaimDraftAsync(second);
        await store.StageClaimAsync(second);

        await using var connection = await fixture.Database.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT selection.claim_revision_id, revision.supersedes_revision_id
            FROM publication_claim_selections selection
            JOIN claim_revisions revision ON revision.id = selection.claim_revision_id
            JOIN claims claim ON claim.id = revision.claim_id
            JOIN integers integer ON integer.id = claim.integer_id
            WHERE integer.canonical_value = '2017';
            """;
        await using var reader = await command.ExecuteReaderAsync();

        Assert.True(await reader.ReadAsync());
        Assert.Equal(second, reader.GetInt64(0));
        Assert.Equal(first, reader.GetInt64(1));
        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task Source_disagreement_is_preserved_and_held_out_of_coverage_and_staging()
    {
        await using var fixture = TestDatabase.Create();
        var seeder = new CorpusSeeder(fixture.Database, ["1", "2", "3"]);

        var result = await seeder.SeedAsync();

        Assert.Equal([PropertySlugs.DecimalNarcissistic], result.Disagreements);
        await using var connection = await fixture.Database.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                comparison.status,
                (SELECT COUNT(*) FROM coverage_claims coverage WHERE coverage.property_version_id = version.id),
                (SELECT COUNT(*) FROM publication_property_selections selection WHERE selection.property_version_id = version.id),
                (SELECT COUNT(*) FROM publication_claim_selections selection JOIN claim_revisions revision ON revision.id = selection.claim_revision_id WHERE revision.property_version_id = version.id)
            FROM comparison_runs comparison
            JOIN property_versions version ON version.id = comparison.property_version_id
            JOIN properties property ON property.id = version.property_id
            WHERE property.slug = $slug;
            """;
        command.Parameters.AddWithValue("$slug", PropertySlugs.DecimalNarcissistic);
        await using var reader = await command.ExecuteReaderAsync();

        Assert.True(await reader.ReadAsync());
        Assert.Equal("mismatched", reader.GetString(0));
        Assert.Equal(0, reader.GetInt32(1));
        Assert.Equal(0, reader.GetInt32(2));
        Assert.Equal(0, reader.GetInt32(3));
        await reader.DisposeAsync();

        var store = new PolyvalentStore(fixture.Database);
        var dossier = await store.GetDossierAsync(IntegerValue.Parse("153"));
        var revisionId = dossier.Claims.Single(claim =>
            claim.PropertySlug == PropertySlugs.DecimalNarcissistic
            && claim.RevisionStatus == "accepted").RevisionId;
        await Assert.ThrowsAsync<SqliteException>(() => store.StageClaimAsync(revisionId));
    }

    [Fact]
    public async Task Coverage_exclusions_remain_unknown_in_the_private_dossier()
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        await using (var connection = await fixture.Database.OpenConnectionAsync())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO population_runs(
                    id, property_version_id, evaluator_id, mode, range_min, range_max,
                    inputs_json, tool_version, status, prepared_at_utc)
                SELECT 'test-prime-exclusion', version.id, NULL, 'deterministic',
                       '2001', '2001', '{}', 'test', 'prepared', '2026-10-18T00:00:00Z'
                FROM property_versions version
                JOIN properties property ON property.id = version.property_id
                WHERE property.slug = 'prime' AND version.status = 'accepted';

                UPDATE population_runs
                SET status = 'running', started_at_utc = '2026-10-18T00:00:01Z'
                WHERE id = 'test-prime-exclusion';

                UPDATE population_runs
                SET status = 'succeeded', completed_at_utc = '2026-10-18T00:00:02Z'
                WHERE id = 'test-prime-exclusion';

                INSERT INTO coverage_claims(
                    property_version_id, population_run_id, range_min, range_max,
                    basis, exclusions_json, created_at_utc)
                SELECT version.id, 'test-prime-exclusion', '2001', '2001',
                       'deterministic', '["2001"]', '2026-10-18T00:00:03Z'
                FROM property_versions version
                JOIN properties property ON property.id = version.property_id
                WHERE property.slug = 'prime' AND version.status = 'accepted';
                """;
            await command.ExecuteNonQueryAsync();
        }

        var dossier = await new PolyvalentStore(fixture.Database)
            .GetDossierAsync(IntegerValue.Parse("2001"));

        Assert.Contains(dossier.Claims, claim =>
            claim.PropertySlug == PropertySlugs.Prime
            && claim.State == KnowledgeState.Unknown);
    }

    [Fact]
    public async Task Draft_content_is_editable_but_freezes_on_acceptance()
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        var store = new PolyvalentStore(fixture.Database);
        var revisionId = await store.CreateManualClaimDraftAsync(
            IntegerValue.Parse("2017"),
            PropertySlugs.Prime,
            "{}",
            null,
            "first note");

        await store.UpdateManualClaimDraftAsync(revisionId, "{}", null, "revised note");
        var draft = (await store.GetDossierAsync(IntegerValue.Parse("2017")))
            .Claims.Single(claim => claim.RevisionId == revisionId);
        Assert.Equal("revised note", draft.PrivateNotes);

        await store.AcceptClaimDraftAsync(revisionId);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.UpdateManualClaimDraftAsync(revisionId, "{}", null, "late rewrite"));

        var profileId = await store.CreateProfileDraftAsync(new Polyvalent.Application.ProfileDraftInput(
            IntegerValue.Parse("2017"),
            "test-profile",
            "first title",
            "test fixture",
            "fictional",
            "first body",
            "first private note"));
        await store.UpdateProfileDraftAsync(
            profileId,
            "revised title",
            "test fixture",
            "fictional",
            "revised body",
            "revised private note");
        var profile = (await store.GetDossierAsync(IntegerValue.Parse("2017")))
            .ProfileEntries.Single(item => item.RevisionId == profileId);

        Assert.Equal("revised title", profile.Title);
        Assert.Equal("revised body", profile.Body);
        Assert.Equal("revised private note", profile.PrivateNotes);
    }

    private static async Task AssertSqliteFailureAsync(SqliteConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await Assert.ThrowsAsync<SqliteException>(() => command.ExecuteNonQueryAsync());
    }
}
