namespace Polyvalent.Domain;

public static class PropertySlugs
{
    public const string Prime = "prime";
    public const string Triangular = "triangular-number";
    public const string PerfectSquare = "perfect-square";
    public const string PerfectCube = "perfect-cube";
    public const string Fibonacci = "fibonacci-number";
    public const string TwinPrime = "twin-prime";
    public const string DecimalPalindrome = "decimal-palindrome";
    public const string DecimalNarcissistic = "decimal-narcissistic-number";
    public const string DecimalFriedman = "decimal-friedman-number";
}

public enum GraphMode
{
    None,
    SharedMembership,
}

public enum KnowledgeState
{
    Positive,
    CoveredNegative,
    Unknown,
}

public enum ClaimOrigin
{
    Manual,
    Computed,
    Imported,
}

public sealed record EvidenceWitness(
    string Kind,
    int SchemaVersion,
    string PayloadJson,
    string DisplayText);

public sealed record PropertyEvaluation(
    IntegerValue Integer,
    string QualifierJson,
    EvidenceWitness? Witness);

public sealed record PropertyDefinition(
    string Slug,
    string DisplayName,
    string Definition,
    IReadOnlyList<string> Aliases,
    GraphMode GraphMode,
    string ValidatorKey,
    string? EvaluatorKey,
    string EvidenceContractJson);

public interface IPropertyEvaluator
{
    string Key { get; }

    string Version { get; }

    IEnumerable<PropertyEvaluation> Evaluate(IntegerValue minimum, IntegerValue maximum);
}
