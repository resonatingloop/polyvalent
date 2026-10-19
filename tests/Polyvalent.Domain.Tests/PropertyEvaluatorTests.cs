using Polyvalent.Domain;

namespace Polyvalent.Domain.Tests;

public sealed class PropertyEvaluatorTests
{
    public static TheoryData<IPropertyEvaluator, int> Counts => new()
    {
        { new PrimeEvaluator(), 303 },
        { new TriangularEvaluator(), 62 },
        { new PerfectSquareEvaluator(), 44 },
        { new PerfectCubeEvaluator(), 12 },
        { new FibonacciEvaluator(), 16 },
        { new TwinPrimeEvaluator(), 121 },
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

    [Fact]
    public void Popular_property_witnesses_are_exact_and_contract_valid()
    {
        var cases = new (IPropertyEvaluator Evaluator, string Slug, string Integer, string Evidence)[]
        {
            (new PerfectSquareEvaluator(), PropertySlugs.PerfectSquare, "64", "8^2 = 64"),
            (new PerfectCubeEvaluator(), PropertySlugs.PerfectCube, "343", "7^3 = 343"),
            (new FibonacciEvaluator(), PropertySlugs.Fibonacci, "1", "f(1) = f(2) = 1"),
            (new TwinPrimeEvaluator(), PropertySlugs.TwinPrime, "137", "twin with 139"),
        };

        foreach (var item in cases)
        {
            var integer = IntegerValue.Parse(item.Integer);
            var evaluation = item.Evaluator.Evaluate(integer, integer).Single();

            Assert.Equal(item.Evidence, evaluation.Witness!.DisplayText);
            EvidenceContractValidator.Validate(
                InitialPropertyCatalog.Get(item.Slug),
                integer,
                evaluation.QualifierJson,
                evaluation.Witness);
        }
    }

    [Fact]
    public void Five_records_both_twin_prime_partners()
    {
        var evaluation = new TwinPrimeEvaluator()
            .Evaluate(IntegerValue.Parse("5"), IntegerValue.Parse("5"))
            .Single();

        Assert.Contains("\"lowerTwin\":\"3\"", evaluation.QualifierJson, StringComparison.Ordinal);
        Assert.Contains("\"higherTwin\":\"7\"", evaluation.QualifierJson, StringComparison.Ordinal);
        Assert.Equal("twins with 3 and 7", evaluation.Witness!.DisplayText);
    }

    [Fact]
    public void Twin_prime_contract_rejects_an_invented_partner()
    {
        var property = InitialPropertyCatalog.Get(PropertySlugs.TwinPrime);
        var witness = new EvidenceWitness(
            "prime_partners",
            1,
            "{\"higherTwin\":\"149\"}",
            "twin with 149");

        Assert.Throws<InvalidOperationException>(() =>
            EvidenceContractValidator.Validate(
                property,
                IntegerValue.Parse("137"),
                "{\"higherTwin\":\"149\"}",
                witness));
    }
}
