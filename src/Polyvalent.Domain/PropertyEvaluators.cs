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

public sealed class CarmichaelEvaluator : IPropertyEvaluator
{
    public string Key => "carmichael/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        for (var candidate = minimum.Value; candidate <= maximum.Value; candidate++)
        {
            if (!IntegerArithmetic.IsCarmichael(candidate))
            {
                continue;
            }

            var valueText = candidate.ToString(CultureInfo.InvariantCulture);
            var factors = IntegerArithmetic.Factor(candidate);
            var checks = factors.Select(factor => new
            {
                prime = factor.Prime.ToString(CultureInfo.InvariantCulture),
                quotient = ((candidate - 1) / (factor.Prime - 1)).ToString(CultureInfo.InvariantCulture),
            }).ToArray();
            var payload = JsonSerializer.Serialize(new
            {
                factors = factors.Select(factor => factor.Prime.ToString(CultureInfo.InvariantCulture)).ToArray(),
                checks,
                result = valueText,
            });
            var divisibility = string.Join(
                ", ",
                checks.Select(check => $"({valueText} - 1) ÷ ({check.prime} - 1) = {check.quotient}"));
            yield return new PropertyEvaluation(
                IntegerValue.From(candidate),
                "{}",
                new EvidenceWitness(
                    "korselt_certificate",
                    1,
                    payload,
                    $"{valueText} = {IntegerArithmetic.FormatFactorization(factors)}; {divisibility}"));
        }
    }
}

public sealed class Base2FermatPseudoprimeEvaluator : IPropertyEvaluator
{
    public string Key => "base-2-fermat-pseudoprime/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        for (var candidate = minimum.Value; candidate <= maximum.Value; candidate++)
        {
            if (!IntegerArithmetic.IsBase2FermatPseudoprime(candidate))
            {
                continue;
            }

            var valueText = candidate.ToString(CultureInfo.InvariantCulture);
            var exponentText = (candidate - 1).ToString(CultureInfo.InvariantCulture);
            var factors = IntegerArithmetic.Factor(candidate);
            var payload = JsonSerializer.Serialize(new
            {
                @base = "2",
                exponent = exponentText,
                modulus = valueText,
                remainder = "1",
                factors = factors.Select(factor => new
                {
                    prime = factor.Prime.ToString(CultureInfo.InvariantCulture),
                    exponent = factor.Exponent,
                }).ToArray(),
            });
            yield return new PropertyEvaluation(
                IntegerValue.From(candidate),
                "{}",
                new EvidenceWitness(
                    "modular_congruence",
                    1,
                    payload,
                    $"{valueText} = {IntegerArithmetic.FormatFactorization(factors)}; 2^{exponentText} ≡ 1 (mod {valueText})"));
        }
    }
}

public sealed class AmicableEvaluator : IPropertyEvaluator
{
    public string Key => "amicable/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        for (var candidate = minimum.Value; candidate <= maximum.Value; candidate++)
        {
            var partner = IntegerArithmetic.SumProperDivisors(candidate);
            if (candidate <= 0
                || partner <= 0
                || partner == candidate
                || IntegerArithmetic.SumProperDivisors(partner) != candidate)
            {
                continue;
            }

            var valueText = candidate.ToString(CultureInfo.InvariantCulture);
            var partnerText = partner.ToString(CultureInfo.InvariantCulture);
            var qualifier = JsonSerializer.Serialize(new { partner = partnerText });
            var payload = JsonSerializer.Serialize(new
            {
                partner = partnerText,
                sumOfProperDivisors = partnerText,
                partnerSumOfProperDivisors = valueText,
            });
            yield return new PropertyEvaluation(
                IntegerValue.From(candidate),
                qualifier,
                new EvidenceWitness(
                    "amicable_pair",
                    1,
                    payload,
                    $"s({valueText}) = {partnerText}; s({partnerText}) = {valueText}"));
        }
    }
}

public sealed class SylvesterSequenceEvaluator : IPropertyEvaluator
{
    public string Key => "sylvester-sequence/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        var term = new BigInteger(2);
        BigInteger? previous = null;
        for (var index = 0; term <= maximum.Value; index++)
        {
            var termText = term.ToString(CultureInfo.InvariantCulture);
            var previousText = previous?.ToString(CultureInfo.InvariantCulture);
            var expression = index == 0
                ? "a(0) = 2"
                : $"{previousText}^2 - {previousText} + 1 = {termText}";
            if (term >= minimum.Value)
            {
                var payload = JsonSerializer.Serialize(new
                {
                    index,
                    previous = previousText,
                    result = termText,
                    expression,
                });
                var display = index == 0
                    ? expression
                    : $"a({index}) = {termText} from {previousText}^2 - {previousText} + 1";
                yield return new PropertyEvaluation(
                    IntegerValue.From(term),
                    "{}",
                    new EvidenceWitness("recurrence_term", 1, payload, display));
            }

            previous = term;
            term = term * term - term + 1;
        }
    }
}

public sealed class PrimaryPseudoperfectEvaluator : IPropertyEvaluator
{
    public string Key => "primary-pseudoperfect/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        for (var candidate = minimum.Value; candidate <= maximum.Value; candidate++)
        {
            if (!IntegerArithmetic.IsPrimaryPseudoperfect(candidate))
            {
                continue;
            }

            var valueText = candidate.ToString(CultureInfo.InvariantCulture);
            var primes = IntegerArithmetic.Factor(candidate).Select(factor => factor.Prime).ToArray();
            var denominators = new[] { candidate }.Concat(primes).ToArray();
            var payload = JsonSerializer.Serialize(new
            {
                primeDivisors = primes.Select(prime => prime.ToString(CultureInfo.InvariantCulture)).ToArray(),
                terms = denominators.Select(denominator => new
                {
                    numerator = "1",
                    denominator = denominator.ToString(CultureInfo.InvariantCulture),
                }).ToArray(),
                result = "1",
            });
            var display = $"{string.Join(" + ", denominators.Select(denominator => $"1/{denominator.ToString(CultureInfo.InvariantCulture)}"))} = 1";
            yield return new PropertyEvaluation(
                IntegerValue.From(candidate),
                "{}",
                new EvidenceWitness("reciprocal_identity", 1, payload, display));
        }
    }
}

public sealed class TwoWayTaxicabEvaluator : IPropertyEvaluator
{
    public string Key => "two-way-taxicab/deterministic";

    public string Version => "1";

    public IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum)
    {
        var sums = new SortedDictionary<BigInteger, List<CubePair>>();
        for (var left = BigInteger.One;
             BigInteger.Pow(left, 3) + BigInteger.Pow(left, 3) <= maximum.Value;
             left++)
        {
            for (var right = left; ; right++)
            {
                var sum = BigInteger.Pow(left, 3) + BigInteger.Pow(right, 3);
                if (sum > maximum.Value)
                {
                    break;
                }

                if (sum < minimum.Value)
                {
                    continue;
                }

                if (!sums.TryGetValue(sum, out var pairs))
                {
                    pairs = [];
                    sums[sum] = pairs;
                }

                pairs.Add(new CubePair(left, right));
            }
        }

        foreach (var (value, pairs) in sums.Where(item => item.Value.Count >= 2))
        {
            var valueText = value.ToString(CultureInfo.InvariantCulture);
            var payload = JsonSerializer.Serialize(new
            {
                representations = pairs.Select(pair => new
                {
                    left = pair.Left.ToString(CultureInfo.InvariantCulture),
                    right = pair.Right.ToString(CultureInfo.InvariantCulture),
                }).ToArray(),
                result = valueText,
            });
            var equalities = pairs.Select(pair =>
                $"{pair.Left.ToString(CultureInfo.InvariantCulture)}^3 + {pair.Right.ToString(CultureInfo.InvariantCulture)}^3");
            yield return new PropertyEvaluation(
                IntegerValue.From(value),
                "{}",
                new EvidenceWitness(
                    "cube_sum_representations",
                    1,
                    payload,
                    $"{string.Join(" = ", equalities)} = {valueText}"));
        }
    }
}

internal sealed record PrimePower(BigInteger Prime, int Exponent);

internal sealed record CubePair(BigInteger Left, BigInteger Right);

internal static class IntegerArithmetic
{
    public static IReadOnlyList<PrimePower> Factor(BigInteger value)
    {
        var factors = new List<PrimePower>();
        if (value < 2)
        {
            return factors;
        }

        var remaining = value;
        for (var divisor = new BigInteger(2); divisor * divisor <= remaining; divisor = divisor == 2 ? 3 : divisor + 2)
        {
            var exponent = 0;
            while (remaining % divisor == 0)
            {
                remaining /= divisor;
                exponent++;
            }

            if (exponent > 0)
            {
                factors.Add(new PrimePower(divisor, exponent));
            }
        }

        if (remaining > 1)
        {
            factors.Add(new PrimePower(remaining, 1));
        }

        return factors;
    }

    public static bool IsCarmichael(BigInteger value)
    {
        if (value < 2 || PrimeEvaluator.IsPrime(value))
        {
            return false;
        }

        var factors = Factor(value);
        return factors.Count > 1
            && factors.All(factor => factor.Exponent == 1)
            && factors.All(factor => (value - 1) % (factor.Prime - 1) == 0);
    }

    public static bool IsBase2FermatPseudoprime(BigInteger value) =>
        value > 2
        && !PrimeEvaluator.IsPrime(value)
        && BigInteger.ModPow(2, value - 1, value) == 1;

    public static BigInteger SumProperDivisors(BigInteger value)
    {
        if (value <= 1)
        {
            return BigInteger.Zero;
        }

        var sigma = BigInteger.One;
        foreach (var factor in Factor(value))
        {
            sigma *= (BigInteger.Pow(factor.Prime, factor.Exponent + 1) - 1) / (factor.Prime - 1);
        }

        return sigma - value;
    }

    public static bool IsPrimaryPseudoperfect(BigInteger value)
    {
        if (value <= 1)
        {
            return false;
        }

        var primes = Factor(value).Select(factor => factor.Prime).ToArray();
        return BigInteger.One + primes.Aggregate(BigInteger.Zero, (sum, prime) => sum + value / prime) == value;
    }

    public static IReadOnlyList<CubePair> FindCubeSumRepresentations(BigInteger value)
    {
        var pairs = new List<CubePair>();
        if (value <= 0)
        {
            return pairs;
        }

        for (var left = BigInteger.One;
             BigInteger.Pow(left, 3) + BigInteger.Pow(left, 3) <= value;
             left++)
        {
            for (var right = left; ; right++)
            {
                var sum = BigInteger.Pow(left, 3) + BigInteger.Pow(right, 3);
                if (sum > value)
                {
                    break;
                }

                if (sum == value)
                {
                    pairs.Add(new CubePair(left, right));
                    break;
                }
            }
        }

        return pairs;
    }

    public static string FormatFactorization(IEnumerable<PrimePower> factors) =>
        string.Join(
            " × ",
            factors.Select(factor => factor.Exponent == 1
                ? factor.Prime.ToString(CultureInfo.InvariantCulture)
                : $"{factor.Prime.ToString(CultureInfo.InvariantCulture)}^{factor.Exponent}"));
}
