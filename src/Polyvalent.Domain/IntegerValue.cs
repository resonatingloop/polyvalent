using System.Globalization;
using System.Numerics;

namespace Polyvalent.Domain;

public readonly record struct IntegerValue : IComparable<IntegerValue>
{
    private IntegerValue(BigInteger value)
    {
        Value = value;
    }

    public BigInteger Value { get; }

    public static IntegerValue From(BigInteger value) => new(value);

    public static IntegerValue Parse(string text)
    {
        if (!TryParse(text, out var value, out var error))
        {
            throw new FormatException(error);
        }

        return value;
    }

    public static bool TryParse(string? text, out IntegerValue value) =>
        TryParse(text, out value, out _);

    public static bool TryParse(
        string? text,
        out IntegerValue value,
        out string error)
    {
        value = default;
        error = string.Empty;

        if (string.IsNullOrEmpty(text))
        {
            error = "an integer is required.";
            return false;
        }

        if (!string.Equals(text, text.Trim(), StringComparison.Ordinal))
        {
            error = "whitespace is not part of a canonical integer.";
            return false;
        }

        if (text[0] == '+')
        {
            error = "a canonical integer never has a leading plus sign.";
            return false;
        }

        if (text == "0")
        {
            value = new IntegerValue(BigInteger.Zero);
            return true;
        }

        var negative = text[0] == '-';
        var digits = negative ? text.AsSpan(1) : text.AsSpan();
        if (digits.IsEmpty || digits[0] == '0')
        {
            error = negative && digits.SequenceEqual("0")
                ? "negative zero is not canonical."
                : "leading zeroes are not canonical.";
            return false;
        }

        foreach (var character in digits)
        {
            if (character is < '0' or > '9')
            {
                error = "canonical integers contain decimal digits only.";
                return false;
            }
        }

        if (!BigInteger.TryParse(
                text,
                NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out var parsed))
        {
            error = "the integer could not be parsed.";
            return false;
        }

        value = new IntegerValue(parsed);
        return true;
    }

    public int CompareTo(IntegerValue other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
