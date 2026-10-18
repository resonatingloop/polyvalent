using System.Numerics;
using Polyvalent.Domain;

namespace Polyvalent.Domain.Tests;

public sealed class IntegerValueTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("-1")]
    [InlineData("900719925474099312345678901234567890")]
    public void Parse_round_trips_canonical_values(string text)
    {
        var value = IntegerValue.Parse(text);

        Assert.Equal(text, value.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" 1")]
    [InlineData("1 ")]
    [InlineData("+1")]
    [InlineData("01")]
    [InlineData("-0")]
    [InlineData("1.0")]
    [InlineData("1e3")]
    public void Parse_rejects_noncanonical_values(string text)
    {
        Assert.Throws<FormatException>(() => IntegerValue.Parse(text));
    }

    [Fact]
    public void Domain_has_no_machine_integer_ceiling()
    {
        var source = BigInteger.Pow(10, 200) + 153;

        Assert.Equal(source, IntegerValue.Parse(source.ToString()).Value);
    }
}
