using Polyvalent.Domain;

namespace Polyvalent.Application;

public sealed record CitationView(
    long Id,
    string Label,
    string? Locator,
    string Role,
    string Url,
    bool PublicSafe,
    bool IsStaged)
{
    public string PublicationLabel => IsStaged
        ? "staged citation"
        : PublicSafe ? "public-safe / unstaged" : "private-only";
}

public sealed record ClaimView(
    long RevisionId,
    long PropertyVersionId,
    string PropertySlug,
    string PropertyName,
    KnowledgeState State,
    ClaimOrigin? Origin,
    string? RevisionStatus,
    string QualifierJson,
    EvidenceWitness? Witness,
    string? PrivateNotes,
    bool IsStaged,
    IReadOnlyList<CitationView> Citations)
{
    public string StagingLabel => RevisionId == 0
        ? string.Empty
        : IsStaged ? "STAGED" : "not staged";
}

public sealed record ProfileEntryView(
    long RevisionId,
    string Slug,
    string Status,
    string Title,
    string Category,
    string EpistemicRegister,
    string Body,
    string? PrivateNotes,
    bool IsStaged,
    IReadOnlyList<CitationView> Citations)
{
    public string StagingLabel => IsStaged ? "STAGED" : "not staged";
}

public sealed record IntegerDossier(
    IntegerValue Integer,
    IReadOnlyList<ClaimView> Claims,
    IReadOnlyList<ProfileEntryView> ProfileEntries);

public sealed record PropertyOption(
    long PropertyVersionId,
    string Slug,
    string DisplayName,
    string Definition,
    GraphMode GraphMode);

public sealed record SourceInput(
    string Slug,
    string Title,
    string? Author,
    string Url,
    string? LicenseMetadata);

public sealed record ProfileDraftInput(
    IntegerValue Integer,
    string Slug,
    string Title,
    string Category,
    string EpistemicRegister,
    string Body,
    string? PrivateNotes,
    long? SupersedesRevisionId = null);

public sealed record StagingSummary(
    int PropertyVersions,
    int Claims,
    int ProfileEntries,
    int Citations);

public interface IResearchRepository
{
    Task<IntegerDossier> GetDossierAsync(IntegerValue integer, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PropertyOption>> GetPropertiesAsync(CancellationToken cancellationToken = default);

    Task<long> CreateManualClaimDraftAsync(
        IntegerValue integer,
        string propertySlug,
        string qualifierJson,
        EvidenceWitness? witness,
        string? privateNotes,
        CancellationToken cancellationToken = default);

    Task UpdateManualClaimDraftAsync(
        long revisionId,
        string qualifierJson,
        EvidenceWitness? witness,
        string? privateNotes,
        CancellationToken cancellationToken = default);

    Task AcceptClaimDraftAsync(long revisionId, CancellationToken cancellationToken = default);

    Task WithdrawClaimDraftAsync(long revisionId, CancellationToken cancellationToken = default);

    Task<long> CreateSourceAsync(SourceInput input, CancellationToken cancellationToken = default);

    Task<long> CiteClaimAsync(
        long revisionId,
        long sourceId,
        string role,
        string label,
        string? locator,
        bool publicSafe,
        CancellationToken cancellationToken = default);

    Task<long> CreateProfileDraftAsync(ProfileDraftInput input, CancellationToken cancellationToken = default);

    Task UpdateProfileDraftAsync(
        long revisionId,
        string title,
        string category,
        string epistemicRegister,
        string body,
        string? privateNotes,
        CancellationToken cancellationToken = default);

    Task AcceptProfileDraftAsync(long revisionId, CancellationToken cancellationToken = default);

    Task WithdrawProfileDraftAsync(long revisionId, CancellationToken cancellationToken = default);

    Task<long> CiteProfileAsync(
        long revisionId,
        long sourceId,
        string role,
        string label,
        string? locator,
        bool publicSafe,
        CancellationToken cancellationToken = default);

    Task StageClaimAsync(long revisionId, CancellationToken cancellationToken = default);

    Task StageProfileAsync(long revisionId, CancellationToken cancellationToken = default);

    Task<StagingSummary> GetStagingSummaryAsync(CancellationToken cancellationToken = default);
}
