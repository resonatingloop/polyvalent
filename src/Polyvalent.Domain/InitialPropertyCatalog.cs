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
    ];

    public static PropertyDefinition Get(string slug) =>
        Definitions.Single(definition =>
            string.Equals(definition.Slug, slug, StringComparison.Ordinal));
}

public static class EvidenceContractValidator
{
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
}
