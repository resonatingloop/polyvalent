using Polyvalent.Domain;

namespace Polyvalent.Domain.Tests;

public sealed class FriedmanExpressionTests
{
    [Theory]
    [InlineData("153", "3*51", false)]
    [InlineData("343", "(3+4)^3", true)]
    [InlineData("127", "-1+2^7", true)]
    [InlineData("1024", "(4-2)^10", false)]
    public void Validates_bounded_arithmetic_witnesses(string value, string expression, bool orderly)
    {
        var validation = FriedmanExpression.Validate(IntegerValue.Parse(value), expression, orderly);

        Assert.Equal(orderly, validation.IsOrderly);
        Assert.Equal("digit_expression", validation.Witness.Kind);
    }

    [Theory]
    [InlineData("153", "153")]
    [InlineData("153", "+153")]
    [InlineData("153", "-(-153)")]
    [InlineData("153", "3*52")]
    [InlineData("343", "System.Console.WriteLine(343)")]
    [InlineData("343", "(3+4)^99")]
    [InlineData("343", "3/0+43")]
    public void Rejects_trivial_wrong_or_executable_text(string value, string expression)
    {
        Assert.ThrowsAny<Exception>(() =>
            FriedmanExpression.Validate(IntegerValue.Parse(value), expression, false));
    }

    [Fact]
    public void Rejects_a_false_orderly_qualifier()
    {
        Assert.Throws<FormatException>(() =>
            FriedmanExpression.Validate(IntegerValue.Parse("153"), "3*51", true));
    }

    [Fact]
    public void Evidence_contract_rejects_an_explicit_false_orderly_qualifier_for_an_orderly_expression()
    {
        var integer = IntegerValue.Parse("343");
        var witness = FriedmanExpression.Validate(integer, "(3+4)^3", false).Witness;

        Assert.Throws<InvalidOperationException>(() => EvidenceContractValidator.Validate(
            InitialPropertyCatalog.Get(PropertySlugs.DecimalFriedman),
            integer,
            "{\"orderly\":false}",
            witness));
    }

    [Fact]
    public void Evidence_contract_rejects_undeclared_qualifier_fields()
    {
        Assert.Throws<InvalidOperationException>(() => EvidenceContractValidator.Validate(
            InitialPropertyCatalog.Get(PropertySlugs.Prime),
            IntegerValue.Parse("137"),
            "{\"privateAside\":\"not a qualifier\"}",
            null));
    }
}
