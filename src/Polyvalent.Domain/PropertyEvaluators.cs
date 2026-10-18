using System.Globalization;
using System.Numerics;
using System.Text.Json;

namespace Polyvalent.Domain;

public sealed class PrimeEvaluator : IPropertyEvaluator
{
    public string Key => "prime/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        for (var candidate = minimum.Value; candidate <= maximum.Value; candidate++)
        {
            if (IsPrime(candidate))
            {
                yield return new PropertyEvaluation(
                    IntegerValue.From(candidate),
                    "{}",
                    null);
            }
        }
    }

    public static bool IsPrime(BigInteger candidate)
    {
        if (candidate < 2)
        {
            return false;
        }

        if (candidate == 2)
        {
            return true;
        }

        if (candidate.IsEven)
        {
            return false;
        }

        for (var divisor = new BigInteger(3); divisor * divisor <= candidate; divisor += 2)
        {
            if (candidate % divisor == 0)
            {
                return false;
            }
        }

        return true;
    }
}

public sealed class TriangularEvaluator : IPropertyEvaluator
{
    public string Key => "triangular/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        for (var index = BigInteger.One; ; index++)
        {
            var value = index * (index + BigInteger.One) / 2;
            if (value > maximum.Value)
            {
                yield break;
            }

            if (value < minimum.Value)
            {
                continue;
            }

            var indexText = index.ToString(CultureInfo.InvariantCulture);
            var payload = JsonSerializer.Serialize(new
            {
                sequenceIndex = indexText,
                expression = $"{indexText} × ({indexText} + 1) ÷ 2",
            });

            yield return new PropertyEvaluation(
                IntegerValue.From(value),
                "{}",
                new EvidenceWitness(
                    "sequence_index",
                    1,
                    payload,
                    $"term {indexText}: {indexText} × ({indexText} + 1) ÷ 2"));
        }
    }
}

public sealed class DecimalPalindromeEvaluator : IPropertyEvaluator
{
    public string Key => "decimal-palindrome/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        for (var candidate = minimum.Value; candidate <= maximum.Value; candidate++)
        {
            if (candidate < 0)
            {
                continue;
            }

            var digits = candidate.ToString(CultureInfo.InvariantCulture);
            var reversed = new string(digits.Reverse().ToArray());
            if (!string.Equals(digits, reversed, StringComparison.Ordinal))
            {
                continue;
            }

            var payload = JsonSerializer.Serialize(new { digits, reversed });
            yield return new PropertyEvaluation(
                IntegerValue.From(candidate),
                "{}",
                new EvidenceWitness(
                    "digit_symmetry",
                    1,
                    payload,
                    $"{digits} reads identically in reverse"));
        }
    }
}

public sealed class DecimalNarcissisticEvaluator : IPropertyEvaluator
{
    public string Key => "decimal-narcissistic/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        for (var candidate = minimum.Value; candidate <= maximum.Value; candidate++)
        {
            if (candidate < 0)
            {
                continue;
            }

            var digits = candidate.ToString(CultureInfo.InvariantCulture);
            var power = digits.Length;
            var terms = new List<string>(digits.Length);
            var sum = BigInteger.Zero;
            foreach (var digitCharacter in digits)
            {
                var digit = digitCharacter - '0';
                sum += BigInteger.Pow(digit, power);
                terms.Add($"{digit}^{power}");
            }

            if (sum != candidate)
            {
                continue;
            }

            var payload = JsonSerializer.Serialize(new
            {
                power,
                terms,
                sum = sum.ToString(CultureInfo.InvariantCulture),
            });
            yield return new PropertyEvaluation(
                IntegerValue.From(candidate),
                "{}",
                new EvidenceWitness(
                    "digit_power_sum",
                    1,
                    payload,
                    $"{string.Join(" + ", terms)} = {digits}"));
        }
    }
}
