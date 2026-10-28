using System.Globalization;
using System.Numerics;
using System.Text.Json;

namespace Polyvalent.Domain;

public static class InitialPropertyCatalog
{
    public static IReadOnlyList<PropertyDefinition> Definitions { get; } =
    [
        new(
            PropertySlugs.Prime,
            "prime",
            "a positive integer greater than one with exactly two positive divisors.",
            [],
            GraphMode.SharedMembership,
            "prime/evidence-v1",
            "prime/deterministic",
            """
            {"version":1,"witnessOptional":true,"acceptedWitnessKinds":["prime_certificate"],"runMaySuffice":true,"coverageKinds":["deterministic"]}
            """),
        new(
            PropertySlugs.Triangular,
            "triangular number",
            "an integer of the form n(n + 1) / 2 for a positive integer n.",
            ["triangle number"],
            GraphMode.SharedMembership,
            "triangular/evidence-v1",
            "triangular/deterministic",
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["sequence_index"],"runMaySuffice":false,"coverageKinds":["deterministic"]}
            """),
        new(
            PropertySlugs.PerfectSquare,
            "perfect square",
            "a positive integer equal to the second power of a positive integer.",
            ["square number"],
            GraphMode.SharedMembership,
            "perfect-square/evidence-v1",
            "perfect-square/deterministic",
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["power_identity"],"runMaySuffice":false,"coverageKinds":["deterministic"]}
            """),
        new(
            PropertySlugs.PerfectCube,
            "perfect cube",
            "a positive integer equal to the third power of a positive integer.",
            ["cube number"],
            GraphMode.SharedMembership,
            "perfect-cube/evidence-v1",
            "perfect-cube/deterministic",
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["power_identity"],"runMaySuffice":false,"coverageKinds":["deterministic"]}
            """),
        new(
            PropertySlugs.Fibonacci,
            "fibonacci number",
            "a positive integer appearing in the fibonacci sequence beginning 1, 1, 2, 3, 5.",
            ["fibonacci sequence member"],
            GraphMode.SharedMembership,
            "fibonacci/evidence-v1",
            "fibonacci/deterministic",
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["sequence_indices"],"runMaySuffice":false,"coverageKinds":["deterministic"]}
            """),
        new(
            PropertySlugs.PowerOfTwo,
            "power of two",
            "a positive integer equal to 2 raised to a nonnegative integer power.",
            ["power of 2"],
            GraphMode.SharedMembership,
            "power-of-two/evidence-v1",
            "power-of-two/deterministic",
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["power_identity"],"runMaySuffice":false,"coverageKinds":["deterministic"]}
            """),
        new(
            PropertySlugs.TwinPrime,
            "twin-prime member",
            "a prime differing by two from at least one other prime.",
            ["twin prime"],
            GraphMode.SharedMembership,
            "twin-prime/evidence-v1",
            "twin-prime/deterministic",
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["prime_partners"],"allowedQualifiers":{"lowerTwin":"integer text","higherTwin":"integer text"},"runMaySuffice":false,"coverageKinds":["deterministic"]}
            """),
        new(
            PropertySlugs.DecimalPalindrome,
            "decimal palindrome",
            "a nonnegative integer whose base-ten digits read identically in reverse.",
            ["palindromic number"],
            GraphMode.None,
            "decimal-palindrome/evidence-v1",
            "decimal-palindrome/deterministic",
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["digit_symmetry"],"runMaySuffice":false,"coverageKinds":["deterministic"]}
            """),
        new(
            PropertySlugs.DecimalNarcissistic,
            "decimal narcissistic number",
            "a nonnegative integer equal to the sum of its decimal digits, each raised to the number of digits.",
            ["armstrong number", "pluperfect digital invariant"],
            GraphMode.SharedMembership,
            "decimal-narcissistic/evidence-v1",
            "decimal-narcissistic/deterministic",
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["digit_power_sum"],"runMaySuffice":false,"externalComparison":"recommended","coverageKinds":["deterministic"]}
            """),
        new(
            PropertySlugs.DecimalFriedman,
            "decimal friedman number",
            "an integer expressible nontrivially from all of its decimal digits exactly once using permitted arithmetic operations.",
            ["friedman number", "nice friedman number", "good friedman number"],
            GraphMode.SharedMembership,
            "decimal-friedman/evidence-v1",
            null,
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["digit_expression"],"allowedQualifiers":{"orderly":[true,false]},"externalCitationRequired":true,"coverageKinds":["source-asserted-bounded"]}
            """),
        new(
            PropertySlugs.Carmichael,
            "carmichael number",
            "a composite squarefree positive integer n for which p - 1 divides n - 1 for every prime divisor p of n.",
            [],
            GraphMode.SharedMembership,
            "carmichael/evidence-v1",
            "carmichael/deterministic",
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["korselt_certificate"],"runMaySuffice":false,"externalComparison":"required","coverageKinds":["deterministic"]}
            """),
        new(
            PropertySlugs.Base2FermatPseudoprime,
            "fermat pseudoprime to base 2",
            "a composite positive integer n satisfying 2^(n - 1) congruent to 1 modulo n.",
            ["sarrus number", "poulet number"],
            GraphMode.SharedMembership,
            "base-2-fermat-pseudoprime/evidence-v1",
            "base-2-fermat-pseudoprime/deterministic",
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["modular_congruence"],"runMaySuffice":false,"externalComparison":"required","coverageKinds":["deterministic"]}
            """),
        new(
            PropertySlugs.Amicable,
            "amicable number",
            "a positive integer whose sum of positive proper divisors is a distinct integer whose own proper-divisor sum returns the first.",
            ["amicable-pair member"],
            GraphMode.SharedMembership,
            "amicable/evidence-v1",
            "amicable/deterministic",
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["amicable_pair"],"allowedQualifiers":{"partner":"integer text"},"runMaySuffice":false,"externalComparison":"required","coverageKinds":["deterministic"]}
            """),
        new(
            PropertySlugs.SylvesterSequenceMember,
            "sylvester sequence member",
            "a positive integer in the sequence beginning a(0) = 2 and continuing by a(n + 1) = a(n)^2 - a(n) + 1.",
            [],
            GraphMode.SharedMembership,
            "sylvester-sequence/evidence-v1",
            "sylvester-sequence/deterministic",
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["recurrence_term"],"runMaySuffice":false,"externalComparison":"required","coverageKinds":["deterministic"]}
            """),
        new(
            PropertySlugs.PrimaryPseudoperfect,
            "primary pseudoperfect number",
            "a positive integer n greater than one for which 1/n plus the sum of 1/p over its distinct prime divisors p equals 1.",
            [],
            GraphMode.SharedMembership,
            "primary-pseudoperfect/evidence-v1",
            "primary-pseudoperfect/deterministic",
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["reciprocal_identity"],"runMaySuffice":false,"externalComparison":"required","coverageKinds":["deterministic"]}
            """),
        new(
            PropertySlugs.Heegner,
            "heegner number",
            "a positive squarefree integer n for which the imaginary quadratic field q(sqrt(-n)) has class number one.",
            [],
            GraphMode.SharedMembership,
            "heegner/evidence-v1",
            null,
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["source_sequence_term"],"runMaySuffice":false,"externalCitationRequired":true,"coverageKinds":["source-asserted-bounded"]}
            """),
        new(
            PropertySlugs.TwoWayTaxicab,
            "two-way taxicab number",
            "a positive integer expressible as a sum of two positive cubes in at least two distinct unordered ways.",
            ["taxi-cab number"],
            GraphMode.SharedMembership,
            "two-way-taxicab/evidence-v1",
            "two-way-taxicab/deterministic",
            """
            {"version":1,"witnessRequired":true,"acceptedWitnessKinds":["cube_sum_representations"],"runMaySuffice":false,"externalComparison":"required","coverageKinds":["deterministic"]}
            """),
    ];

    public static PropertyDefinition Get(string slug) =>
        Definitions.Single(definition =>
            string.Equals(definition.Slug, slug, StringComparison.Ordinal));
}

public static class EvidenceContractValidator
{
    private static readonly string[] HeegnerValues =
    [
        "1", "2", "3", "7", "11", "19", "43", "67", "163",
    ];

    public static void Validate(
        PropertyDefinition property,
        IntegerValue integer,
        string qualifierJson,
        EvidenceWitness? witness)
    {
        ValidateQualifierFields(property, qualifierJson);

        switch (property.Slug)
        {
            case PropertySlugs.Prime:
                if (!PrimeEvaluator.IsPrime(integer.Value))
                {
                    throw new InvalidOperationException($"{integer} is not prime.");
                }

                if (witness is not null && witness.Kind != "prime_certificate")
                {
                    throw WrongWitness(property, witness, "prime_certificate");
                }

                if (witness is not null)
                {
                    if (witness.SchemaVersion != 1)
                    {
                        throw new InvalidOperationException("only witness schema version 1 is accepted.");
                    }

                    using var _ = ParsePayload(witness);
                }

                return;

            case PropertySlugs.Triangular:
                ValidateTriangular(integer, RequireWitness(property, witness, "sequence_index"));
                return;

            case PropertySlugs.PerfectSquare:
                ValidatePerfectPower(integer, RequireWitness(property, witness, "power_identity"), 2);
                return;

            case PropertySlugs.PerfectCube:
                ValidatePerfectPower(integer, RequireWitness(property, witness, "power_identity"), 3);
                return;

            case PropertySlugs.Fibonacci:
                ValidateFibonacci(integer, RequireWitness(property, witness, "sequence_indices"));
                return;

            case PropertySlugs.PowerOfTwo:
                ValidatePowerOfTwo(integer, RequireWitness(property, witness, "power_identity"));
                return;

            case PropertySlugs.TwinPrime:
                ValidateTwinPrime(
                    integer,
                    qualifierJson,
                    RequireWitness(property, witness, "prime_partners"));
                return;

            case PropertySlugs.DecimalPalindrome:
                ValidatePalindrome(integer, RequireWitness(property, witness, "digit_symmetry"));
                return;

            case PropertySlugs.DecimalNarcissistic:
                ValidateNarcissistic(integer, RequireWitness(property, witness, "digit_power_sum"));
                return;

            case PropertySlugs.DecimalFriedman:
                ValidateFriedman(integer, qualifierJson, RequireWitness(property, witness, "digit_expression"));
                return;

            case PropertySlugs.Carmichael:
                ValidateCarmichael(integer, RequireWitness(property, witness, "korselt_certificate"));
                return;

            case PropertySlugs.Base2FermatPseudoprime:
                ValidateBase2FermatPseudoprime(
                    integer,
                    RequireWitness(property, witness, "modular_congruence"));
                return;

            case PropertySlugs.Amicable:
                ValidateAmicable(integer, qualifierJson, RequireWitness(property, witness, "amicable_pair"));
                return;

            case PropertySlugs.SylvesterSequenceMember:
                ValidateSylvester(integer, RequireWitness(property, witness, "recurrence_term"));
                return;

            case PropertySlugs.PrimaryPseudoperfect:
                ValidatePrimaryPseudoperfect(
                    integer,
                    RequireWitness(property, witness, "reciprocal_identity"));
                return;

            case PropertySlugs.Heegner:
                ValidateHeegner(integer, RequireWitness(property, witness, "source_sequence_term"));
                return;

            case PropertySlugs.TwoWayTaxicab:
                ValidateTwoWayTaxicab(
                    integer,
                    RequireWitness(property, witness, "cube_sum_representations"));
                return;

            default:
                throw new InvalidOperationException($"no evidence validator is registered for {property.Slug}.");
        }
    }

    private static void ValidateTriangular(IntegerValue integer, EvidenceWitness witness)
    {
        using var payload = ParsePayload(witness);
        var indexText = RequiredString(payload.RootElement, "sequenceIndex");
        var index = IntegerValue.Parse(indexText).Value;
        if (index <= 0 || index * (index + BigInteger.One) / 2 != integer.Value)
        {
            throw new InvalidOperationException("the triangular sequence index does not produce the claimed integer.");
        }
    }

    private static void ValidatePalindrome(IntegerValue integer, EvidenceWitness witness)
    {
        using var payload = ParsePayload(witness);
        var digits = RequiredString(payload.RootElement, "digits");
        var reversed = RequiredString(payload.RootElement, "reversed");
        if (integer.Value < 0 || digits != integer.ToString() || reversed != new string(digits.Reverse().ToArray()))
        {
            throw new InvalidOperationException("the digit-symmetry witness does not prove the claimed integer.");
        }
    }

    private static void ValidatePerfectPower(
        IntegerValue integer,
        EvidenceWitness witness,
        int expectedExponent)
    {
        using var payload = ParsePayload(witness);
        var baseValue = IntegerValue.Parse(RequiredString(payload.RootElement, "base")).Value;
        var exponent = RequiredInt(payload.RootElement, "exponent");
        if (baseValue <= 0
            || exponent != expectedExponent
            || BigInteger.Pow(baseValue, exponent) != integer.Value
            || RequiredString(payload.RootElement, "result") != integer.ToString())
        {
            throw new InvalidOperationException("the power-identity witness does not prove the claimed integer.");
        }
    }

    private static void ValidateFibonacci(IntegerValue integer, EvidenceWitness witness)
    {
        using var payload = ParsePayload(witness);
        if (!payload.RootElement.TryGetProperty("indices", out var indicesNode)
            || indicesNode.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("the fibonacci witness requires an indices array.");
        }

        var actual = indicesNode
            .EnumerateArray()
            .Select(node => node.ValueKind == JsonValueKind.String
                ? IntegerValue.Parse(node.GetString()!).Value
                : throw new InvalidOperationException("fibonacci indices must be integer strings."))
            .ToArray();
        var expected = FibonacciEvaluator.FindIndices(integer.Value);
        if (actual.Length == 0
            || !actual.SequenceEqual(expected)
            || RequiredString(payload.RootElement, "result") != integer.ToString())
        {
            throw new InvalidOperationException("the fibonacci witness does not prove the claimed integer.");
        }
    }

    private static void ValidatePowerOfTwo(IntegerValue integer, EvidenceWitness witness)
    {
        using var payload = ParsePayload(witness);
        var baseValue = IntegerValue.Parse(RequiredString(payload.RootElement, "base")).Value;
        var exponent = RequiredInt(payload.RootElement, "exponent");
        if (baseValue != 2
            || exponent < 0
            || BigInteger.Pow(baseValue, exponent) != integer.Value
            || RequiredString(payload.RootElement, "result") != integer.ToString())
        {
            throw new InvalidOperationException("the power-of-two witness does not prove the claimed integer.");
        }
    }

    private static void ValidateTwinPrime(
        IntegerValue integer,
        string qualifierJson,
        EvidenceWitness witness)
    {
        using var qualifiers = JsonDocument.Parse(qualifierJson);
        using var payload = ParsePayload(witness);
        var expectedLower = PrimeEvaluator.IsPrime(integer.Value - 2)
            ? (integer.Value - 2).ToString(CultureInfo.InvariantCulture)
            : null;
        var expectedHigher = PrimeEvaluator.IsPrime(integer.Value + 2)
            ? (integer.Value + 2).ToString(CultureInfo.InvariantCulture)
            : null;
        if (!PrimeEvaluator.IsPrime(integer.Value)
            || (expectedLower is null && expectedHigher is null)
            || OptionalString(qualifiers.RootElement, "lowerTwin") != expectedLower
            || OptionalString(qualifiers.RootElement, "higherTwin") != expectedHigher
            || OptionalString(payload.RootElement, "lowerTwin") != expectedLower
            || OptionalString(payload.RootElement, "higherTwin") != expectedHigher)
        {
            throw new InvalidOperationException("the prime-partners witness does not prove the claimed integer.");
        }
    }

    private static void ValidateNarcissistic(IntegerValue integer, EvidenceWitness witness)
    {
        using var payload = ParsePayload(witness);
        var power = payload.RootElement.GetProperty("power").GetInt32();
        var digits = integer.ToString();
        if (integer.Value < 0 || power != digits.Length)
        {
            throw new InvalidOperationException("the digit-power witness uses the wrong exponent.");
        }

        var sum = digits.Aggregate(
            BigInteger.Zero,
            (current, digit) => current + BigInteger.Pow(digit - '0', power));
        if (sum != integer.Value || RequiredString(payload.RootElement, "sum") != integer.ToString())
        {
            throw new InvalidOperationException("the digit-power witness does not prove the claimed integer.");
        }
    }

    private static void ValidateFriedman(
        IntegerValue integer,
        string qualifierJson,
        EvidenceWitness witness)
    {
        using var payload = ParsePayload(witness);
        using var qualifiers = JsonDocument.Parse(qualifierJson);
        var expression = RequiredString(payload.RootElement, "expression");
        var hasOrderlyQualifier = qualifiers.RootElement.TryGetProperty("orderly", out var orderlyNode);
        if (hasOrderlyQualifier && orderlyNode.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            throw new InvalidOperationException("the orderly qualifier must be boolean when present.");
        }

        var claimedOrderly = hasOrderlyQualifier && orderlyNode.GetBoolean();
        var validation = FriedmanExpression.Validate(integer, expression, claimedOrderly);
        if (hasOrderlyQualifier && orderlyNode.GetBoolean() != validation.IsOrderly)
        {
            throw new InvalidOperationException("the friedman orderly qualifier is inconsistent with its expression.");
        }

        if (payload.RootElement.TryGetProperty("orderly", out var payloadOrderly)
            && payloadOrderly.GetBoolean() != validation.IsOrderly)
        {
            throw new InvalidOperationException("the friedman witness's orderly result is inconsistent.");
        }
    }

    private static void ValidateCarmichael(IntegerValue integer, EvidenceWitness witness)
    {
        if (!IntegerArithmetic.IsCarmichael(integer.Value))
        {
            throw new InvalidOperationException($"{integer} does not satisfy korselt's criterion.");
        }

        using var payload = ParsePayload(witness);
        var factors = IntegerArithmetic.Factor(integer.Value);
        var factorNodes = RequiredArray(payload.RootElement, "factors").EnumerateArray().ToArray();
        if (factorNodes.Length != factors.Count
            || factorNodes.Where(node => node.ValueKind != JsonValueKind.String).Any()
            || !factorNodes.Select(node => node.GetString()).SequenceEqual(
                factors.Select(factor => factor.Prime.ToString(CultureInfo.InvariantCulture))))
        {
            throw new InvalidOperationException("the korselt witness does not contain the complete squarefree factorization.");
        }

        var checks = RequiredArray(payload.RootElement, "checks").EnumerateArray().ToArray();
        if (checks.Length != factors.Count)
        {
            throw new InvalidOperationException("the korselt witness requires one divisibility check per prime factor.");
        }

        for (var index = 0; index < factors.Count; index++)
        {
            var prime = factors[index].Prime;
            var expectedPrime = prime.ToString(CultureInfo.InvariantCulture);
            var expectedQuotient = ((integer.Value - 1) / (prime - 1)).ToString(CultureInfo.InvariantCulture);
            if (RequiredString(checks[index], "prime") != expectedPrime
                || RequiredString(checks[index], "quotient") != expectedQuotient)
            {
                throw new InvalidOperationException("the korselt divisibility certificate is inconsistent.");
            }
        }

        if (RequiredString(payload.RootElement, "result") != integer.ToString())
        {
            throw new InvalidOperationException("the korselt witness names the wrong result.");
        }
    }

    private static void ValidateBase2FermatPseudoprime(IntegerValue integer, EvidenceWitness witness)
    {
        if (!IntegerArithmetic.IsBase2FermatPseudoprime(integer.Value))
        {
            throw new InvalidOperationException($"{integer} is not a fermat pseudoprime to base 2.");
        }

        using var payload = ParsePayload(witness);
        if (RequiredString(payload.RootElement, "base") != "2"
            || RequiredString(payload.RootElement, "exponent")
                != (integer.Value - 1).ToString(CultureInfo.InvariantCulture)
            || RequiredString(payload.RootElement, "modulus") != integer.ToString()
            || RequiredString(payload.RootElement, "remainder") != "1")
        {
            throw new InvalidOperationException("the modular-congruence witness is inconsistent.");
        }

        var expectedFactors = IntegerArithmetic.Factor(integer.Value);
        var factors = RequiredArray(payload.RootElement, "factors").EnumerateArray().ToArray();
        if (factors.Length != expectedFactors.Count)
        {
            throw new InvalidOperationException("the modular-congruence witness requires a complete factorization.");
        }

        for (var index = 0; index < expectedFactors.Count; index++)
        {
            if (RequiredString(factors[index], "prime")
                    != expectedFactors[index].Prime.ToString(CultureInfo.InvariantCulture)
                || RequiredInt(factors[index], "exponent") != expectedFactors[index].Exponent)
            {
                throw new InvalidOperationException("the modular-congruence factorization is inconsistent.");
            }
        }
    }

    private static void ValidateAmicable(
        IntegerValue integer,
        string qualifierJson,
        EvidenceWitness witness)
    {
        using var qualifiers = JsonDocument.Parse(qualifierJson);
        using var payload = ParsePayload(witness);
        var partnerText = RequiredString(qualifiers.RootElement, "partner");
        var partner = IntegerValue.Parse(partnerText).Value;
        var forward = IntegerArithmetic.SumProperDivisors(integer.Value);
        var reverse = IntegerArithmetic.SumProperDivisors(partner);
        if (integer.Value <= 0
            || partner <= 0
            || partner == integer.Value
            || forward != partner
            || reverse != integer.Value
            || RequiredString(payload.RootElement, "partner") != partnerText
            || RequiredString(payload.RootElement, "sumOfProperDivisors") != partnerText
            || RequiredString(payload.RootElement, "partnerSumOfProperDivisors") != integer.ToString())
        {
            throw new InvalidOperationException("the amicable-pair witness does not prove the claimed integer.");
        }
    }

    private static void ValidateSylvester(IntegerValue integer, EvidenceWitness witness)
    {
        using var payload = ParsePayload(witness);
        var index = RequiredInt(payload.RootElement, "index");
        if (index < 0)
        {
            throw new InvalidOperationException("the sylvester sequence index cannot be negative.");
        }

        var term = new BigInteger(2);
        BigInteger? previous = null;
        for (var position = 0; position < index; position++)
        {
            previous = term;
            term = term * term - term + 1;
            if (term > integer.Value && position + 1 < index)
            {
                throw new InvalidOperationException("the sylvester sequence index cannot produce the claimed integer.");
            }
        }

        if (!payload.RootElement.TryGetProperty("previous", out var previousNode))
        {
            throw new InvalidOperationException("the recurrence-term witness requires field 'previous'.");
        }

        var expectedPrevious = previous?.ToString(CultureInfo.InvariantCulture);
        var actualPrevious = previousNode.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => previousNode.GetString(),
            _ => throw new InvalidOperationException("the sylvester preceding term must be a string or null."),
        };
        var expectedExpression = index == 0
            ? "a(0) = 2"
            : $"{expectedPrevious}^2 - {expectedPrevious} + 1 = {term.ToString(CultureInfo.InvariantCulture)}";
        if (term != integer.Value
            || actualPrevious != expectedPrevious
            || RequiredString(payload.RootElement, "result") != integer.ToString()
            || RequiredString(payload.RootElement, "expression") != expectedExpression)
        {
            throw new InvalidOperationException("the recurrence-term witness does not prove the claimed integer.");
        }
    }

    private static void ValidatePrimaryPseudoperfect(IntegerValue integer, EvidenceWitness witness)
    {
        if (!IntegerArithmetic.IsPrimaryPseudoperfect(integer.Value))
        {
            throw new InvalidOperationException($"{integer} is not primary pseudoperfect.");
        }

        using var payload = ParsePayload(witness);
        var primes = IntegerArithmetic.Factor(integer.Value).Select(factor => factor.Prime).ToArray();
        var primeNodes = RequiredArray(payload.RootElement, "primeDivisors").EnumerateArray().ToArray();
        if (primeNodes.Length != primes.Length
            || primeNodes.Where(node => node.ValueKind != JsonValueKind.String).Any()
            || !primeNodes.Select(node => node.GetString()).SequenceEqual(
                primes.Select(prime => prime.ToString(CultureInfo.InvariantCulture))))
        {
            throw new InvalidOperationException("the reciprocal-identity witness has the wrong prime divisors.");
        }

        var denominators = new[] { integer.Value }.Concat(primes).ToArray();
        var terms = RequiredArray(payload.RootElement, "terms").EnumerateArray().ToArray();
        if (terms.Length != denominators.Length)
        {
            throw new InvalidOperationException("the reciprocal-identity witness has the wrong number of terms.");
        }

        for (var index = 0; index < terms.Length; index++)
        {
            if (RequiredString(terms[index], "numerator") != "1"
                || RequiredString(terms[index], "denominator")
                    != denominators[index].ToString(CultureInfo.InvariantCulture))
            {
                throw new InvalidOperationException("the reciprocal-identity terms are inconsistent.");
            }
        }

        if (RequiredString(payload.RootElement, "result") != "1")
        {
            throw new InvalidOperationException("the reciprocal-identity witness names the wrong result.");
        }
    }

    private static void ValidateHeegner(IntegerValue integer, EvidenceWitness witness)
    {
        using var payload = ParsePayload(witness);
        var sourceIndex = RequiredInt(payload.RootElement, "sourceIndex");
        if (RequiredString(payload.RootElement, "accession") != "A003173"
            || sourceIndex < 1
            || sourceIndex > HeegnerValues.Length
            || HeegnerValues[sourceIndex - 1] != integer.ToString()
            || RequiredString(payload.RootElement, "value") != integer.ToString())
        {
            throw new InvalidOperationException("the source-sequence witness does not match the heegner registry.");
        }
    }

    private static void ValidateTwoWayTaxicab(IntegerValue integer, EvidenceWitness witness)
    {
        var expected = IntegerArithmetic.FindCubeSumRepresentations(integer.Value);
        if (expected.Count < 2)
        {
            throw new InvalidOperationException($"{integer} does not have two positive cube-sum representations.");
        }

        using var payload = ParsePayload(witness);
        var representations = RequiredArray(payload.RootElement, "representations").EnumerateArray().ToArray();
        if (representations.Length != expected.Count)
        {
            throw new InvalidOperationException("the cube-sum witness is incomplete.");
        }

        var seen = new HashSet<(BigInteger Left, BigInteger Right)>();
        for (var index = 0; index < representations.Length; index++)
        {
            var left = IntegerValue.Parse(RequiredString(representations[index], "left")).Value;
            var right = IntegerValue.Parse(RequiredString(representations[index], "right")).Value;
            if (left <= 0
                || right < left
                || BigInteger.Pow(left, 3) + BigInteger.Pow(right, 3) != integer.Value
                || !seen.Add((left, right))
                || left != expected[index].Left
                || right != expected[index].Right)
            {
                throw new InvalidOperationException("the cube-sum witness contains an invalid representation.");
            }
        }

        if (RequiredString(payload.RootElement, "result") != integer.ToString())
        {
            throw new InvalidOperationException("the cube-sum witness names the wrong result.");
        }
    }

    private static EvidenceWitness RequireWitness(
        PropertyDefinition property,
        EvidenceWitness? witness,
        string expectedKind)
    {
        if (witness is null)
        {
            throw new InvalidOperationException($"{property.DisplayName} requires a witness.");
        }

        if (!string.Equals(witness.Kind, expectedKind, StringComparison.Ordinal))
        {
            throw WrongWitness(property, witness, expectedKind);
        }

        if (witness.SchemaVersion != 1)
        {
            throw new InvalidOperationException("only witness schema version 1 is accepted.");
        }

        return witness;
    }

    private static InvalidOperationException WrongWitness(
        PropertyDefinition property,
        EvidenceWitness witness,
        string expectedKind) =>
        new($"{property.DisplayName} expects {expectedKind}, not {witness.Kind}.");

    private static JsonDocument ParsePayload(EvidenceWitness witness)
    {
        try
        {
            var document = JsonDocument.Parse(witness.PayloadJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                document.Dispose();
                throw new InvalidOperationException("witness payloads must be json objects.");
            }

            return document;
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("the witness payload is not valid json.", exception);
        }
    }

    private static void EnsureJsonObject(string json, string label)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException($"the {label} payload must be a json object.");
            }
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"the {label} payload is not valid json.", exception);
        }
    }

    private static void ValidateQualifierFields(PropertyDefinition property, string qualifierJson)
    {
        EnsureJsonObject(qualifierJson, "qualifier");
        using var document = JsonDocument.Parse(qualifierJson);
        var allowed = property.Slug switch
        {
            PropertySlugs.DecimalFriedman => new HashSet<string>(["orderly"], StringComparer.Ordinal),
            PropertySlugs.TwinPrime => new HashSet<string>(["lowerTwin", "higherTwin"], StringComparer.Ordinal),
            PropertySlugs.Amicable => new HashSet<string>(["partner"], StringComparer.Ordinal),
            _ => [],
        };
        foreach (var field in document.RootElement.EnumerateObject())
        {
            if (!allowed.Contains(field.Name))
            {
                throw new InvalidOperationException(
                    $"qualifier field '{field.Name}' is not allowed for {property.DisplayName}.");
            }
        }
    }

    private static string RequiredString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var node) || node.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException($"the witness payload requires string field '{name}'.");
        }

        return node.GetString()!;
    }

    private static string? OptionalString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var node))
        {
            return null;
        }

        if (node.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException($"the witness payload field '{name}' must be a string.");
        }

        return node.GetString();
    }

    private static int RequiredInt(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var node) || !node.TryGetInt32(out var value))
        {
            throw new InvalidOperationException($"the witness payload requires integer field '{name}'.");
        }

        return value;
    }

    private static JsonElement RequiredArray(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var node) || node.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException($"the witness payload requires array field '{name}'.");
        }

        return node;
    }
}
