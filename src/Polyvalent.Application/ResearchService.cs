using Polyvalent.Domain;

namespace Polyvalent.Application;

public sealed class ResearchService(IResearchRepository repository)
{
    public Task<IntegerDossier> OpenAsync(string canonicalInteger, CancellationToken cancellationToken = default) =>
        repository.GetDossierAsync(IntegerValue.Parse(canonicalInteger), cancellationToken);

    public Task<IntegerDossier> OpenAsync(IntegerValue integer, CancellationToken cancellationToken = default) =>
        repository.GetDossierAsync(integer, cancellationToken);

    public Task<IReadOnlyList<PropertyOption>> GetPropertiesAsync(CancellationToken cancellationToken = default) =>
        repository.GetPropertiesAsync(cancellationToken);

    public Task<long> CreateClaimDraftAsync(
        IntegerValue integer,
        string propertySlug,
        string qualifierJson,
        EvidenceWitness? witness,
        string? privateNotes,
        CancellationToken cancellationToken = default) =>
        repository.CreateManualClaimDraftAsync(
            integer,
            propertySlug,
            qualifierJson,
            witness,
            privateNotes,
            cancellationToken);

    public Task AcceptClaimAsync(long revisionId, CancellationToken cancellationToken = default) =>
        repository.AcceptClaimDraftAsync(revisionId, cancellationToken);

    public Task UpdateClaimDraftAsync(
        long revisionId,
        string qualifierJson,
        EvidenceWitness? witness,
        string? privateNotes,
        CancellationToken cancellationToken = default) =>
        repository.UpdateManualClaimDraftAsync(
            revisionId,
            qualifierJson,
            witness,
            privateNotes,
            cancellationToken);

    public Task WithdrawClaimAsync(long revisionId, CancellationToken cancellationToken = default) =>
        repository.WithdrawClaimDraftAsync(revisionId, cancellationToken);

    public Task<long> CreateSourceAsync(SourceInput input, CancellationToken cancellationToken = default) =>
        repository.CreateSourceAsync(input, cancellationToken);

    public Task<long> CiteClaimAsync(
        long revisionId,
        long sourceId,
        string role,
        string label,
        string? locator,
        bool publicSafe,
        CancellationToken cancellationToken = default) =>
        repository.CiteClaimAsync(
            revisionId,
            sourceId,
            role,
            label,
            locator,
            publicSafe,
            cancellationToken);

    public Task<long> CreateProfileDraftAsync(ProfileDraftInput input, CancellationToken cancellationToken = default) =>
        repository.CreateProfileDraftAsync(input, cancellationToken);

    public Task UpdateProfileDraftAsync(
        long revisionId,
        string title,
        string category,
        string epistemicRegister,
        string body,
        string? privateNotes,
        CancellationToken cancellationToken = default) =>
        repository.UpdateProfileDraftAsync(
            revisionId,
            title,
            category,
            epistemicRegister,
            body,
            privateNotes,
            cancellationToken);

    public Task AcceptProfileAsync(long revisionId, CancellationToken cancellationToken = default) =>
        repository.AcceptProfileDraftAsync(revisionId, cancellationToken);

    public Task WithdrawProfileAsync(long revisionId, CancellationToken cancellationToken = default) =>
        repository.WithdrawProfileDraftAsync(revisionId, cancellationToken);

    public Task<long> CiteProfileAsync(
        long revisionId,
        long sourceId,
        string role,
        string label,
        string? locator,
        bool publicSafe,
        CancellationToken cancellationToken = default) =>
        repository.CiteProfileAsync(
            revisionId,
            sourceId,
            role,
            label,
            locator,
            publicSafe,
            cancellationToken);

    public Task StageClaimAsync(long revisionId, CancellationToken cancellationToken = default) =>
        repository.StageClaimAsync(revisionId, cancellationToken);

    public Task StageProfileAsync(long revisionId, CancellationToken cancellationToken = default) =>
        repository.StageProfileAsync(revisionId, cancellationToken);

    public Task<StagingSummary> GetStagingSummaryAsync(CancellationToken cancellationToken = default) =>
        repository.GetStagingSummaryAsync(cancellationToken);
}
