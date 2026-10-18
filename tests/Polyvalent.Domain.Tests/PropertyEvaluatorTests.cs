using Polyvalent.Domain;

namespace Polyvalent.Domain.Tests;

public sealed class PropertyEvaluatorTests
{
    public static TheoryData<IPropertyEvaluator, int> Counts => new()
    {
        { new PrimeEvaluator(), 303 },
        { new TriangularEvaluator(), 62 },
        { new DecimalPalindromeEvaluator(), 118 },
        { new DecimalNarcissisticEvaluator(), 14 },
    };

    [Theory]
    [MemberData(nameof(Counts))]
    public void Initial_range_has_expected_population(IPropertyEvaluator evaluator, int expected)
    {
        var results = evaluator.Evaluate(IntegerValue.Parse("1"), IntegerValue.Parse("2000")).ToArray();

        Assert.Equal(expected, results.Length);
    }

    [Fact]
    public void Triangular_population_ends_at_1953()
    {
        var results = new TriangularEvaluator()
            .Evaluate(IntegerValue.Parse("1"), IntegerValue.Parse("2000"))
            .ToArray();

        Assert.Equal("1953", results[^1].Integer.ToString());
        Assert.Contains("term 62", results[^1].Witness!.DisplayText, StringComparison.Ordinal);
    }

    [Fact]
    public void Narcissistic_population_matches_bounded_reference_set()
    {
        var expected = new[]
        {
            "1", "2", "3", "4", "5", "6", "7", "8", "9",
            "153", "370", "371", "407", "1634",
        };
        var actual = new DecimalNarcissisticEvaluator()
            .Evaluate(IntegerValue.Parse("1"), IntegerValue.Parse("2000"))
            .Select(result => result.Integer.ToString())
            .ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Optional_prime_witness_still_obeys_its_versioned_contract()
    {
        var property = InitialPropertyCatalog.Get(PropertySlugs.Prime);
        var witness = new EvidenceWitness("prime_certificate", 2, "{}", "test certificate");

        Assert.Throws<InvalidOperationException>(() =>
            EvidenceContractValidator.Validate(property, IntegerValue.Parse("137"), "{}", witness));
    }
}
