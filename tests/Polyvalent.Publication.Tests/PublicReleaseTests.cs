using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Polyvalent.Application;
using Polyvalent.Data;
using Polyvalent.Domain;

namespace Polyvalent.Publication.Tests;

public sealed class PublicReleaseTests
{
    private static readonly DateTimeOffset ReleaseTime =
        new(2026, 10, 18, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Release_is_deterministic_and_manifest_digest_matches_data()
    {
        await using var first = TestDatabase.Create();
        await using var second = TestDatabase.Create();
        await new CorpusSeeder(first.Database).SeedAsync();
        await new CorpusSeeder(second.Database).SeedAsync();
        var firstOutput = Path.Combine(first.Directory, "release");
        var secondOutput = Path.Combine(second.Directory, "release");

        var firstResult = await new PublicReleaseService(first.Database).GenerateAsync(
            firstOutput,
            "reference-v1",
            ReleaseTime);
        var secondResult = await new PublicReleaseService(second.Database).GenerateAsync(
            secondOutput,
            "reference-v1",
            ReleaseTime);
        var firstData = await File.ReadAllBytesAsync(Path.Combine(firstOutput, "data.json"));
        var secondData = await File.ReadAllBytesAsync(Path.Combine(secondOutput, "data.json"));
        var manifest = JsonDocument.Parse(
            await File.ReadAllBytesAsync(Path.Combine(firstOutput, "manifest.json")));

        Assert.Equal(firstData, secondData);
        Assert.Equal(firstResult.DataSha256, secondResult.DataSha256);
        Assert.Equal(
            Convert.ToHexStringLower(SHA256.HashData(firstData)),
            manifest.RootElement.GetProperty("dataSha256").GetString());

        await using var connection = await first.Database.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                (SELECT COUNT(*) FROM public_release_property_citation_items WHERE release_id = 'reference-v1'),
                (SELECT COUNT(*) FROM publication_property_citation_selections),
                (SELECT COUNT(*) FROM public_release_claim_citation_items WHERE release_id = 'reference-v1'),
                (SELECT COUNT(*) FROM publication_claim_citation_selections),
                (SELECT COUNT(*) FROM public_release_profile_citation_items WHERE release_id = 'reference-v1'),
                (SELECT COUNT(*) FROM publication_profile_citation_selections);
            """;
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal(reader.GetInt32(1), reader.GetInt32(0));
        Assert.Equal(reader.GetInt32(3), reader.GetInt32(2));
        Assert.Equal(reader.GetInt32(5), reader.GetInt32(4));
        Assert.True(reader.GetInt32(0) > 0);
        Assert.True(reader.GetInt32(2) > 0);
        Assert.True(reader.GetInt32(4) > 0);
    }

    [Fact]
    public async Task Release_contains_explained_153_to_343_friedman_relation()
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        var output = Path.Combine(fixture.Directory, "release");

        await new PublicReleaseService(fixture.Database).GenerateAsync(
            output,
            "reference-v1",
            ReleaseTime);
        using var data = JsonDocument.Parse(await File.ReadAllBytesAsync(Path.Combine(output, "data.json")));
        var integers = data.RootElement.GetProperty("integers").EnumerateArray().ToArray();
        var oneFiftyThree = integers.Single(item => item.GetProperty("value").GetString() == "153");
        var threeFortyThree = integers.Single(item => item.GetProperty("value").GetString() == "343");
        var firstClaim = FriedmanClaim(oneFiftyThree);
        var secondClaim = FriedmanClaim(threeFortyThree);

        Assert.Contains("3 × 51", firstClaim.GetProperty("witness").GetProperty("displayText").GetString(), StringComparison.Ordinal);
        Assert.Contains("(3 + 4) ^ 3", secondClaim.GetProperty("witness").GetProperty("displayText").GetString(), StringComparison.Ordinal);
        Assert.True(secondClaim.GetProperty("qualifier").GetProperty("orderly").GetBoolean());
        Assert.DoesNotContain("strong", secondClaim.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("weak", firstClaim.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Release_surfaces_the_enriched_137_and_343_profiles_with_provenance()
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        var output = Path.Combine(fixture.Directory, "release");

        await new PublicReleaseService(fixture.Database).GenerateAsync(
            output,
            "enrichment-v1",
            ReleaseTime);
        using var data = JsonDocument.Parse(await File.ReadAllBytesAsync(Path.Combine(output, "data.json")));
        var properties = data.RootElement.GetProperty("properties").EnumerateArray().ToArray();
        var integers = data.RootElement.GetProperty("integers").EnumerateArray().ToArray();
        var prime = properties.Single(item => item.GetProperty("slug").GetString() == PropertySlugs.Prime);
        var powerOfTwo = properties.Single(item => item.GetProperty("slug").GetString() == PropertySlugs.PowerOfTwo);
        var twinPrime = properties.Single(item => item.GetProperty("slug").GetString() == PropertySlugs.TwinPrime);
        var oneThirtySeven = integers.Single(item => item.GetProperty("value").GetString() == "137");
        var threeFortyThree = integers.Single(item => item.GetProperty("value").GetString() == "343");
        var oneThousandTwentyFour = integers.Single(item => item.GetProperty("value").GetString() == "1024");
        var twinClaim = Claim(oneThirtySeven, PropertySlugs.TwinPrime);
        var cubeClaim = Claim(threeFortyThree, PropertySlugs.PerfectCube);
        var powerClaim = Claim(oneThousandTwentyFour, PropertySlugs.PowerOfTwo);

        Assert.Equal(10, properties.Length);
        Assert.Equal("shared-membership", prime.GetProperty("graphMode").GetString());
        Assert.Equal("shared-membership", powerOfTwo.GetProperty("graphMode").GetString());
        Assert.Equal(
            PropertySlugs.Prime,
            Claim(oneThirtySeven, PropertySlugs.Prime).GetProperty("propertySlug").GetString());
        Assert.Equal("139", twinClaim.GetProperty("qualifier").GetProperty("higherTwin").GetString());
        Assert.Equal("twin with 139", twinClaim.GetProperty("witness").GetProperty("displayText").GetString());
        Assert.Equal("7^3 = 343", cubeClaim.GetProperty("witness").GetProperty("displayText").GetString());
        Assert.Equal("2^10 = 1024", powerClaim.GetProperty("witness").GetProperty("displayText").GetString());
        Assert.Contains(
            powerOfTwo.GetProperty("citations").EnumerateArray(),
            citation => citation.GetProperty("url").GetString() == "https://oeis.org/A000079");
        Assert.Contains(
            twinPrime.GetProperty("citations").EnumerateArray(),
            citation => citation.GetProperty("url").GetString() == "https://oeis.org/A001097");
    }

    [Fact]
    public async Task Release_surfaces_seeded_vignettes_without_creating_cultural_claims()
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        var output = Path.Combine(fixture.Directory, "release");

        var result = await new PublicReleaseService(fixture.Database).GenerateAsync(
            output,
            "vignettes-v1",
            ReleaseTime);
        using var data = JsonDocument.Parse(await File.ReadAllBytesAsync(Path.Combine(output, "data.json")));
        var integers = data.RootElement.GetProperty("integers").EnumerateArray().ToArray();

        Assert.Equal(10, result.PropertyCount);
        Assert.Equal(529, result.IntegerCount);
        Assert.Equal(728, result.ClaimCount);
        Assert.Equal(20, result.ProfileEntryCount);

        AssertProfile(integers, "137", "fine-structure-near-integer", 1);
        AssertProfile(integers, "153", "vesica-and-unbroken-net", 3);
        var ninetyThree = AssertProfile(integers, "93", "thelemic-greeting", 1);
        var oneFortySix = AssertProfile(integers, "146", "lucina-minor-planet", 2);
        AssertProfile(integers, "404", "http-not-found", 1);
        AssertProfile(integers, "808", "tr-808-afterlife", 1);
        var eightySix = AssertProfile(integers, "86", "eighty-sixed", 1);
        AssertProfile(integers, "137", "paulis-last-room", 2);
        AssertProfile(integers, "153", "sonnet-153", 1);
        AssertProfile(integers, "36", "thirty-six-righteous", 1);
        AssertProfile(integers, "88", "constellations-standardized", 1);
        var oneHundredEight = AssertProfile(integers, "108", "panchen-lama-prayer-beads", 1);
        var oneFiftySix = AssertProfile(integers, "156", "babalons-number", 1);
        AssertProfile(integers, "163", "gardners-almost-integer", 1);
        AssertProfile(integers, "231", "sefer-yetzirah-gates", 1);
        var threeEighteen = AssertProfile(integers, "318", "abrahams-army-cipher", 2);
        AssertProfile(integers, "343", "guilty-spark", 3);
        var fourEighteen = AssertProfile(integers, "418", "abrahadabra-great-work", 1);
        AssertProfile(integers, "616", "beasts-variant-number", 2);
        var seventeenTwentyNine = AssertProfile(integers, "1729", "ramanujans-taxi", 1);

        foreach (var profileOnly in new[]
                 {
                     eightySix,
                     ninetyThree,
                     oneHundredEight,
                     oneFortySix,
                     oneFiftySix,
                     threeEighteen,
                     fourEighteen,
                     seventeenTwentyNine,
                 })
        {
            Assert.Empty(profileOnly.GetProperty("claims").EnumerateArray());
        }

        Assert.DoesNotContain(
            data.RootElement.GetProperty("properties").EnumerateArray(),
            property => property.GetProperty("slug").GetString() is
                "thelemic-greeting"
                or "lucina-minor-planet"
                or "http-not-found"
                or "babalons-number"
                or "beasts-variant-number"
                or "ramanujans-taxi");

        var profiles153 = integers
            .Single(integer => integer.GetProperty("value").GetString() == "153")
            .GetProperty("profileEntries")
            .EnumerateArray()
            .ToArray();
        var profile153 = profiles153.Single(
            profile => profile.GetProperty("slug").GetString() == "vesica-and-unbroken-net");
        Assert.Contains(
            profile153.GetProperty("citations").EnumerateArray(),
            citation => citation.GetProperty("url").GetString() == "https://www.kingjamesbibleonline.org/John-21-11/");

        var profile616 = integers
            .Single(integer => integer.GetProperty("value").GetString() == "616")
            .GetProperty("profileEntries")
            .EnumerateArray()
            .Single(profile => profile.GetProperty("slug").GetString() == "beasts-variant-number");
        Assert.Contains("papyrus 115", profile616.GetProperty("body").GetString(), StringComparison.Ordinal);
        Assert.DoesNotContain("scholar's 666", data.RootElement.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Private_sentinels_in_selected_notes_source_metadata_and_neighboring_drafts_do_not_leak()
    {
        const string sentinel = "PRIVATE-SENTINEL-9f86d081884c";
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        var store = new PolyvalentStore(fixture.Database);
        await store.AddPrivacySentinelAsync(sentinel, "publication firewall regression");

        var primeDraft = await store.CreateManualClaimDraftAsync(
            IntegerValue.Parse("2017"),
            PropertySlugs.Prime,
            "{}",
            null,
            $"selected private note {sentinel}");
        await store.AcceptClaimDraftAsync(primeDraft);
        await store.StageClaimAsync(primeDraft);

        var sourceId = await store.CreateSourceAsync(new SourceInput(
            "private-metadata-source",
            $"private title {sentinel}",
            null,
            "https://example.test/public-safe-target",
            $"private license review {sentinel}"));
        var profileDraft = await store.CreateProfileDraftAsync(new ProfileDraftInput(
            IntegerValue.Parse("2017"),
            "public-shell-private-notes",
            "a public shell",
            "test fixture",
            "fictional",
            "this body is intentionally public-safe.",
            $"profile notes {sentinel}"));
        await store.CiteProfileAsync(
            profileDraft,
            sourceId,
            "fixture",
            "public-safe citation label",
            null,
            true);
        await store.AcceptProfileDraftAsync(profileDraft);
        await store.StageProfileAsync(profileDraft);

        _ = await store.CreateProfileDraftAsync(new ProfileDraftInput(
            IntegerValue.Parse("2018"),
            "unselected-neighbor",
            "private neighbor",
            "test fixture",
            "private draft",
            $"unselected body {sentinel}",
            null));
        var output = Path.Combine(fixture.Directory, "release");

        await new PublicReleaseService(fixture.Database).GenerateAsync(
            output,
            "sentinel-safe-v1",
            ReleaseTime);
        var bytes = await File.ReadAllTextAsync(Path.Combine(output, "data.json"));

        Assert.DoesNotContain(sentinel, bytes, StringComparison.Ordinal);
        Assert.Contains("this body is intentionally public-safe.", bytes, StringComparison.Ordinal);
        Assert.DoesNotContain("privateNotes", bytes, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Sentinel_in_selected_public_body_aborts_without_artifact_or_release_record()
    {
        const string sentinel = "PRIVATE-SENTINEL-FAIL-CLOSED";
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        var store = new PolyvalentStore(fixture.Database);
        await store.AddPrivacySentinelAsync(sentinel, "fail closed fixture");
        var sourceId = await store.CreateSourceAsync(new SourceInput(
            "sentinel-fixture-source",
            "sentinel fixture source",
            null,
            "https://example.test/sentinel",
            null));
        var revisionId = await store.CreateProfileDraftAsync(new ProfileDraftInput(
            IntegerValue.Parse("2019"),
            "sentinel-public-body",
            "sentinel test",
            "test fixture",
            "fictional",
            $"this selected body contains {sentinel}.",
            null));
        await store.CiteProfileAsync(revisionId, sourceId, "fixture", "fixture source", null, true);
        await store.AcceptProfileDraftAsync(revisionId);
        await store.StageProfileAsync(revisionId);
        var output = Path.Combine(fixture.Directory, "rejected-release");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new PublicReleaseService(fixture.Database).GenerateAsync(
                output,
                "rejected-v1",
                ReleaseTime));

        Assert.False(Directory.Exists(output));
        await using var connection = await fixture.Database.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM public_releases WHERE id = 'rejected-v1';";
        Assert.Equal(0L, await command.ExecuteScalarAsync());
    }

    [Fact]
    public async Task Sealed_release_rejects_mutation_and_target_overwrite()
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        var output = Path.Combine(fixture.Directory, "release");
        var service = new PublicReleaseService(fixture.Database);
        await service.GenerateAsync(output, "immutable-v1", ReleaseTime);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GenerateAsync(output, "immutable-v2", ReleaseTime));
        await using var connection = await fixture.Database.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE public_releases SET data_sha256 = $digest WHERE id = 'immutable-v1';";
        command.Parameters.AddWithValue("$digest", new string('0', 64));
        await Assert.ThrowsAsync<SqliteException>(() => command.ExecuteNonQueryAsync());
        command.Parameters.Clear();
        command.CommandText = "DELETE FROM public_release_claim_citation_items WHERE release_id = 'immutable-v1';";
        await Assert.ThrowsAsync<SqliteException>(() => command.ExecuteNonQueryAsync());
    }

    [Fact]
    public async Task A_late_source_disagreement_aborts_an_already_staged_property()
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        await using (var connection = await fixture.Database.OpenConnectionAsync())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO comparison_runs(
                    id, property_version_id, population_run_id, source_capture_id,
                    compared_at_utc, local_count, source_count, missing_json,
                    unexpected_json, status)
                SELECT
                    'late-mismatch', version.id, run.id, capture.id,
                    '2026-10-18T00:00:00Z', 14, 13, '["1634"]', '[]', 'mismatched'
                FROM property_versions version
                JOIN properties property ON property.id = version.property_id
                JOIN population_runs run ON run.property_version_id = version.id
                JOIN source_captures capture ON capture.id = (
                    SELECT comparison.source_capture_id
                    FROM comparison_runs comparison
                    WHERE comparison.property_version_id = version.id
                    LIMIT 1)
                WHERE property.slug = 'decimal-narcissistic-number';
                """;
            await command.ExecuteNonQueryAsync();
        }

        var output = Path.Combine(fixture.Directory, "disagreed-release");
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new PublicReleaseService(fixture.Database).GenerateAsync(
                output,
                "disagreed-v1",
                ReleaseTime));

        Assert.Contains("disagreement", error.Message, StringComparison.Ordinal);
        Assert.False(Directory.Exists(output));
    }

    [Fact]
    public async Task Unsafe_selected_source_url_aborts_without_an_artifact()
    {
        await using var fixture = TestDatabase.Create();
        await new CorpusSeeder(fixture.Database).SeedAsync();
        await using (var connection = await fixture.Database.OpenConnectionAsync())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                UPDATE sources
                SET canonical_url = 'file:///private/research/source'
                WHERE slug = 'oeis-a036057';
                """;
            await command.ExecuteNonQueryAsync();
        }

        var output = Path.Combine(fixture.Directory, "unsafe-url-release");
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new PublicReleaseService(fixture.Database).GenerateAsync(
                output,
                "unsafe-url-v1",
                ReleaseTime));

        Assert.Contains("not safe", error.Message, StringComparison.Ordinal);
        Assert.False(Directory.Exists(output));
    }

    private static JsonElement FriedmanClaim(JsonElement integer) =>
        Claim(integer, PropertySlugs.DecimalFriedman);

    private static JsonElement Claim(JsonElement integer, string propertySlug) =>
        integer
            .GetProperty("claims")
            .EnumerateArray()
            .Single(claim => claim.GetProperty("propertySlug").GetString() == propertySlug);

    private static JsonElement AssertProfile(
        IReadOnlyCollection<JsonElement> integers,
        string value,
        string slug,
        int citationCount)
    {
        var integer = integers.Single(item => item.GetProperty("value").GetString() == value);
        var profile = integer
            .GetProperty("profileEntries")
            .EnumerateArray()
            .Single(item => item.GetProperty("slug").GetString() == slug);
        Assert.Equal(citationCount, profile.GetProperty("citations").GetArrayLength());
        return integer;
    }
}
