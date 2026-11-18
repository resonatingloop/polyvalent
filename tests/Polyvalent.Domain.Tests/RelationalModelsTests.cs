using Polyvalent.Domain;

namespace Polyvalent.Domain.Tests;

public sealed class RelationalModelsTests
{
    [Theory]
    [InlineData("permutation-rehearsal")]
    [InlineData("prime-index-137")]
    public void Research_lineage_slugs_are_canonical(string value) =>
        Assert.Equal(value, ResearchLineageSlug.Parse(value).ToString());

    [Theory]
    [InlineData("UPPERCASE")]
    [InlineData("two--hyphens")]
    [InlineData("two words")]
    [InlineData("relation/137")]
    public void Research_lineage_slugs_reject_ambiguous_identity(string value) =>
        Assert.Throws<FormatException>(() => ResearchLineageSlug.Parse(value));

    [Fact]
    public void Direct_relation_acceptance_requires_exactly_two_distinct_roles()
    {
        var participants = new[]
        {
            new DirectRelationParticipant(IntegerValue.Parse("137"), "prime index", 0),
            new DirectRelationParticipant(IntegerValue.Parse("773"), "indexed prime", 1),
        };

        AuthoredRelationalValidator.ValidateDirectRelationForAcceptance(
            DirectRelationDirectionality.Directed,
            "773 is the 137th prime",
            "prime enumeration",
            "computed",
            participants);

        Assert.Throws<InvalidOperationException>(() =>
            AuthoredRelationalValidator.ValidateDirectRelationForAcceptance(
                DirectRelationDirectionality.Directed,
                "773 is the 137th prime",
                "prime enumeration",
                "computed",
                participants[..1]));
    }

    [Fact]
    public void Constellation_acceptance_requires_collective_semantics()
    {
        var members = new[]
        {
            new ConstellationMember(IntegerValue.Parse("137"), "member", null, 0),
            new ConstellationMember(IntegerValue.Parse("173"), "member", null, 1),
        };

        Assert.Throws<InvalidOperationException>(() =>
            AuthoredRelationalValidator.ValidateConstellationForAcceptance(
                "permutation rehearsal",
                "",
                "synchronistic",
                members));
    }
}
