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

public sealed class PerfectSquareEvaluator : IPropertyEvaluator
{
    public string Key => "perfect-square/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        for (var root = BigInteger.One; root * root <= maximum.Value; root++)
        {
            var value = root * root;
            if (value < minimum.Value)
            {
                continue;
            }

            var rootText = root.ToString(CultureInfo.InvariantCulture);
            var valueText = value.ToString(CultureInfo.InvariantCulture);
            var payload = JsonSerializer.Serialize(new
            {
                @base = rootText,
                exponent = 2,
                result = valueText,
            });
            yield return new PropertyEvaluation(
                IntegerValue.From(value),
                "{}",
                new EvidenceWitness("power_identity", 1, payload, $"{rootText}^2 = {valueText}"));
        }
    }
}

public sealed class PerfectCubeEvaluator : IPropertyEvaluator
{
    public string Key => "perfect-cube/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        for (var root = BigInteger.One; root * root * root <= maximum.Value; root++)
        {
            var value = root * root * root;
            if (value < minimum.Value)
            {
                continue;
            }

            var rootText = root.ToString(CultureInfo.InvariantCulture);
            var valueText = value.ToString(CultureInfo.InvariantCulture);
            var payload = JsonSerializer.Serialize(new
            {
                @base = rootText,
                exponent = 3,
                result = valueText,
            });
            yield return new PropertyEvaluation(
                IntegerValue.From(value),
                "{}",
                new EvidenceWitness("power_identity", 1, payload, $"{rootText}^3 = {valueText}"));
        }
    }
}

public sealed class FibonacciEvaluator : IPropertyEvaluator
{
    public string Key => "fibonacci/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        if (maximum.Value < 1)
        {
            yield break;
        }

        var memberships = new SortedDictionary<BigInteger, List<BigInteger>>();
        var value = BigInteger.One;
        var next = BigInteger.One;
        for (var index = BigInteger.One; value <= maximum.Value; index++)
        {
            if (value >= minimum.Value)
            {
                if (!memberships.TryGetValue(value, out var indices))
                {
                    indices = [];
                    memberships[value] = indices;
                }

                indices.Add(index);
            }

            (value, next) = (next, value + next);
        }

        foreach (var membership in memberships)
        {
            var indexTexts = membership.Value
                .Select(index => index.ToString(CultureInfo.InvariantCulture))
                .ToArray();
            var valueText = membership.Key.ToString(CultureInfo.InvariantCulture);
            var payload = JsonSerializer.Serialize(new
            {
                indices = indexTexts,
                result = valueText,
            });
            var display = indexTexts.Length == 1
                ? $"f({indexTexts[0]}) = {valueText}"
                : $"{string.Join(" = ", indexTexts.Select(index => $"f({index})"))} = {valueText}";
            yield return new PropertyEvaluation(
                IntegerValue.From(membership.Key),
                "{}",
                new EvidenceWitness("sequence_indices", 1, payload, display));
        }
    }

    public static IReadOnlyList<BigInteger> FindIndices(BigInteger candidate)
    {
        if (candidate < 1)
        {
            return [];
        }

        var indices = new List<BigInteger>();
        var value = BigInteger.One;
        var next = BigInteger.One;
        for (var index = BigInteger.One; value <= candidate; index++)
        {
            if (value == candidate)
            {
                indices.Add(index);
            }

            (value, next) = (next, value + next);
        }

        return indices;
    }
}

public sealed class PowerOfTwoEvaluator : IPropertyEvaluator
{
    public string Key => "power-of-two/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        if (maximum.Value < 1)
        {
            yield break;
        }

        var value = BigInteger.One;
        for (var exponent = 0; value <= maximum.Value; exponent++, value *= 2)
        {
            if (value < minimum.Value)
            {
                continue;
            }

            var valueText = value.ToString(CultureInfo.InvariantCulture);
            var payload = JsonSerializer.Serialize(new
            {
                @base = "2",
                exponent,
                result = valueText,
            });
            yield return new PropertyEvaluation(
                IntegerValue.From(value),
                "{}",
                new EvidenceWitness("power_identity", 1, payload, $"2^{exponent} = {valueText}"));
        }
    }
}

public sealed class TwinPrimeEvaluator : IPropertyEvaluator
{
    public string Key => "twin-prime/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        for (var candidate = minimum.Value; candidate <= maximum.Value; candidate++)
        {
            if (!PrimeEvaluator.IsPrime(candidate))
            {
                continue;
            }

            var lower = PrimeEvaluator.IsPrime(candidate - 2) ? candidate - 2 : (BigInteger?)null;
            var higher = PrimeEvaluator.IsPrime(candidate + 2) ? candidate + 2 : (BigInteger?)null;
            if (lower is null && higher is null)
            {
                continue;
            }

            var partners = new Dictionary<string, string>(StringComparer.Ordinal);
            if (lower is not null)
            {
                partners["lowerTwin"] = lower.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (higher is not null)
            {
                partners["higherTwin"] = higher.Value.ToString(CultureInfo.InvariantCulture);
            }

            var partnerValues = partners.Values.ToArray();
            var display = partnerValues.Length == 1
                ? $"twin with {partnerValues[0]}"
                : $"twins with {string.Join(" and ", partnerValues)}";
            var json = JsonSerializer.Serialize(partners);
            yield return new PropertyEvaluation(
                IntegerValue.From(candidate),
                json,
                new EvidenceWitness("prime_partners", 1, json, display));
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
