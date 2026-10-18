using System.Text.Json;
using System.Text.Json.Serialization;

namespace Polyvalent.Data;

public sealed record PublicCorpus(
    string SchemaVersion,
    string ReleaseId,
    string GeneratedAtUtc,
    IReadOnlyList<PublicProperty> Properties,
    IReadOnlyList<PublicInteger> Integers);

public sealed record PublicProperty(
    string Slug,
    string Name,
    string Definition,
    IReadOnlyList<string> Aliases,
    string GraphMode,
    IReadOnlyList<PublicCoverage> Coverage,
    IReadOnlyList<PublicCitation> Citations);

public sealed record PublicCoverage(
    string Minimum,
    string Maximum,
    string Basis,
    IReadOnlyList<string> Exclusions);

public sealed record PublicInteger(
    string Value,
    IReadOnlyList<PublicClaim> Claims,
    IReadOnlyList<PublicProfileEntry> ProfileEntries);

public sealed record PublicClaim(
    string PropertySlug,
    JsonElement Qualifier,
    PublicWitness? Witness,
    IReadOnlyList<PublicCitation> Citations);

public sealed record PublicWitness(
    string Kind,
    int SchemaVersion,
    string DisplayText);

public sealed record PublicProfileEntry(
    string Slug,
    string Title,
    string Category,
    string EpistemicRegister,
    string Body,
    IReadOnlyList<PublicCitation> Citations);

public sealed record PublicCitation(
    string Label,
    string? Locator,
    string Role,
    string Url);

public sealed record PublicManifest(
    string SchemaVersion,
    string ReleaseId,
    string GeneratedAtUtc,
    int SourceDatabaseSchemaVersion,
    string DataSha256);

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true)]
[JsonSerializable(typeof(PublicCorpus))]
[JsonSerializable(typeof(PublicManifest))]
internal sealed partial class PublicJsonContext : JsonSerializerContext;
