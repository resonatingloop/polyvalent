using Polyvalent.Domain;

namespace Polyvalent.Application;

public enum ResearchObjectKind
{
    Integer,
    DirectRelation,
    Constellation,
}

public sealed record ResearchObjectRef(
    ResearchObjectKind Kind,
    string Key,
    string Label)
{
    public string TypeLabel => Kind switch
    {
        ResearchObjectKind.Integer => "integer",
        ResearchObjectKind.DirectRelation => "direct relation",
        ResearchObjectKind.Constellation => "constellation",
        _ => throw new InvalidOperationException($"unknown research object kind '{Kind}'."),
    };

    public string StableKey => $"{Kind}:{Key}";
}

public sealed record AuthoredCitationView(
    long Id,
    long SourceId,
    long? SourceCaptureId,
    string Role,
    string Label,
    string? Locator,
    string SourceTitle,
    string SourceUrl);

public sealed record DirectRelationView(
    long LineageId,
    long RevisionId,
    string Slug,
    int Revision,
    AuthoredRevisionStatus Status,
    bool IsActive,
    DirectRelationDirectionality Directionality,
    string Proposition,
    string Warrant,
    string EpistemicRegister,
    string? PrivateNotes,
    long? SupersedesRevisionId,
    IReadOnlyList<DirectRelationParticipant> Participants,
    IReadOnlyList<AuthoredCitationView> Citations)
{
    public string StatusLabel => Status.ToStorage();

    public string DirectionalityLabel => Directionality.ToStorage();

    public string ActiveLabel => IsActive ? "current accepted revision" : StatusLabel;

    public string ParticipantLine => string.Join(
        " · ",
        Participants.OrderBy(participant => participant.Ordinal)
            .Select(participant => $"{participant.Integer} / {participant.Role}"));
}

public sealed record ConstellationView(
    long LineageId,
    long RevisionId,
    string Slug,
    int Revision,
    AuthoredRevisionStatus Status,
    bool IsActive,
    string Name,
    string Account,
    string EpistemicRegister,
    string? PrivateNotes,
    long? SupersedesRevisionId,
    IReadOnlyList<ConstellationMember> Members,
    IReadOnlyList<AuthoredCitationView> Citations)
{
    public string StatusLabel => Status.ToStorage();

    public string ActiveLabel => IsActive ? "current accepted revision" : StatusLabel;

    public string MemberLine => string.Join(
        " · ",
        Members.OrderBy(member => member.Ordinal)
            .Select(member => $"{member.Integer} / {member.Role}"));
}

public sealed record DirectRelationDraftInput(
    string Slug,
    DirectRelationDirectionality Directionality,
    string Proposition,
    string Warrant,
    string EpistemicRegister,
    string? PrivateNotes,
    IReadOnlyList<DirectRelationParticipant> Participants);

public sealed record ConstellationDraftInput(
    string Slug,
    string Name,
    string Account,
    string EpistemicRegister,
    string? PrivateNotes,
    IReadOnlyList<ConstellationMember> Members);
