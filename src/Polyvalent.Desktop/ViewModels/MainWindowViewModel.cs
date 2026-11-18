using System.Collections.ObjectModel;
using System.Numerics;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Polyvalent.Application;
using Polyvalent.Domain;

namespace Polyvalent.Desktop.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly ResearchService _research;
    private readonly ILedgerOrientationReader _orientationReader;

    public MainWindowViewModel(
        ResearchService research,
        string databasePath,
        ILedgerOrientationReader orientationReader,
        LedgerOrientation orientation)
    {
        _research = research;
        _orientationReader = orientationReader;
        DatabasePath = databasePath;
        LedgerOrientationLabel = orientation.DesktopLabel;
    }

    public string DatabasePath { get; }

    [ObservableProperty]
    private string _ledgerOrientationLabel;

    public ObservableCollection<ClaimView> Claims { get; } = [];

    public ObservableCollection<ProfileEntryView> ProfileEntries { get; } = [];

    public ObservableCollection<PropertyOption> Properties { get; } = [];

    public ObservableCollection<DirectRelationView> DirectRelations { get; } = [];

    public ObservableCollection<ConstellationView> Constellations { get; } = [];

    public ObservableCollection<ResearchObjectRef> HeldObjects { get; } = [];

    public ObservableCollection<ResearchObjectRef> VisitTrail { get; } = [];

    private readonly List<ResearchObjectRef> _trail = [];
    private int _trailIndex = -1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIntegerActive))]
    [NotifyPropertyChangedFor(nameof(IsDirectRelationActive))]
    [NotifyPropertyChangedFor(nameof(IsConstellationActive))]
    private ResearchObjectKind _activeObjectKind = ResearchObjectKind.Integer;

    public bool IsIntegerActive => ActiveObjectKind == ResearchObjectKind.Integer;

    public bool IsDirectRelationActive => ActiveObjectKind == ResearchObjectKind.DirectRelation;

    public bool IsConstellationActive => ActiveObjectKind == ResearchObjectKind.Constellation;

    [ObservableProperty]
    private string _activeObjectTitle = "—";

    [ObservableProperty]
    private string _activeObjectSubtitle = "integer dossier";

    [ObservableProperty]
    private DirectRelationView? _activeDirectRelation;

    public bool IsActiveDirectRelationDraft =>
        ActiveDirectRelation?.Status == AuthoredRevisionStatus.Draft;

    public bool IsActiveDirectRelationAccepted =>
        ActiveDirectRelation?.Status == AuthoredRevisionStatus.Accepted;

    [ObservableProperty]
    private ConstellationView? _activeConstellation;

    public bool IsActiveConstellationDraft =>
        ActiveConstellation?.Status == AuthoredRevisionStatus.Draft;

    public bool IsActiveConstellationAccepted =>
        ActiveConstellation?.Status == AuthoredRevisionStatus.Accepted;

    [ObservableProperty]
    private DirectRelationParticipant? _selectedDirectRelationParticipant;

    [ObservableProperty]
    private ConstellationMember? _selectedConstellationMember;

    [ObservableProperty]
    private DirectRelationView? _selectedDirectRelation;

    [ObservableProperty]
    private ConstellationView? _selectedConstellation;

    [ObservableProperty]
    private ResearchObjectRef? _selectedHeldObject;

    [ObservableProperty]
    private string _searchText = "153";

    [ObservableProperty]
    private string _selectedInteger = "—";

    [ObservableProperty]
    private ClaimView? _selectedClaim;

    [ObservableProperty]
    private ProfileEntryView? _selectedProfile;

    [ObservableProperty]
    private string _statusMessage = "opening the ledger…";

    [ObservableProperty]
    private string _stagingMessage = "nothing staged";

    [ObservableProperty]
    private string _draftPropertySlug = PropertySlugs.DecimalFriedman;

    [ObservableProperty]
    private string _draftQualifierJson = "{}";

    [ObservableProperty]
    private string _draftWitnessExpression = string.Empty;

    [ObservableProperty]
    private string _draftPrivateNotes = string.Empty;

    [ObservableProperty]
    private string _sourceSlug = string.Empty;

    [ObservableProperty]
    private string _sourceTitle = string.Empty;

    [ObservableProperty]
    private string _sourceUrl = "https://";

    [ObservableProperty]
    private string _sourceAuthor = string.Empty;

    [ObservableProperty]
    private string _sourceLicense = string.Empty;

    [ObservableProperty]
    private string _citationSourceId = string.Empty;

    [ObservableProperty]
    private string _citationLabel = string.Empty;

    [ObservableProperty]
    private string _citationRole = "evidence";

    [ObservableProperty]
    private string _citationLocator = string.Empty;

    [ObservableProperty]
    private bool _citationPublicSafe;

    [ObservableProperty]
    private string _profileSlug = string.Empty;

    [ObservableProperty]
    private string _profileTitle = string.Empty;

    [ObservableProperty]
    private string _profileCategory = "number lore";

    [ObservableProperty]
    private string _profileRegister = "sourced context";

    [ObservableProperty]
    private string _profileBody = string.Empty;

    [ObservableProperty]
    private string _profilePrivateNotes = string.Empty;

    [ObservableProperty]
    private long? _profileSupersedesRevisionId;

    [ObservableProperty]
    private string _directRelationSlug = string.Empty;

    [ObservableProperty]
    private DirectRelationDirectionality _directRelationDirectionality = DirectRelationDirectionality.Directed;

    public IReadOnlyList<DirectRelationDirectionality> DirectionalityOptions { get; } =
        Enum.GetValues<DirectRelationDirectionality>();

    [ObservableProperty]
    private string _directRelationFirstInteger = string.Empty;

    [ObservableProperty]
    private string _directRelationFirstRole = "participant";

    [ObservableProperty]
    private string _directRelationSecondInteger = string.Empty;

    [ObservableProperty]
    private string _directRelationSecondRole = "participant";

    [ObservableProperty]
    private string _directRelationProposition = string.Empty;

    [ObservableProperty]
    private string _directRelationWarrant = string.Empty;

    [ObservableProperty]
    private string _directRelationRegister = "authored assertion";

    [ObservableProperty]
    private string _directRelationPrivateNotes = string.Empty;

    [ObservableProperty]
    private string _constellationSlug = string.Empty;

    [ObservableProperty]
    private string _constellationName = string.Empty;

    [ObservableProperty]
    private string _constellationAccount = string.Empty;

    [ObservableProperty]
    private string _constellationRegister = "authored constellation";

    [ObservableProperty]
    private string _constellationPrivateNotes = string.Empty;

    [ObservableProperty]
    private string _constellationMembers = string.Empty;

    [ObservableProperty]
    private string _citationCaptureId = string.Empty;

    public async Task InitializeAsync()
    {
        await GuardAsync(async () =>
        {
            Properties.Clear();
            foreach (var property in await _research.GetPropertiesAsync())
            {
                Properties.Add(property);
            }

            await OpenCoreAsync();
        });
    }

    [RelayCommand]
    private Task OpenAsync() => GuardAsync(OpenCoreAsync);

    [RelayCommand]
    private Task PreviousAsync() => MoveAsync(-1);

    [RelayCommand]
    private Task NextAsync() => MoveAsync(1);

    [RelayCommand]
    private Task CreateClaimDraftAsync() => GuardAsync(async () =>
    {
        var integer = CurrentInteger();
        var witness = BuildWitness(integer, DraftPropertySlug, DraftQualifierJson, DraftWitnessExpression);
        await _research.CreateClaimDraftAsync(
            integer,
            DraftPropertySlug,
            string.IsNullOrWhiteSpace(DraftQualifierJson) ? "{}" : DraftQualifierJson,
            witness,
            EmptyToNull(DraftPrivateNotes));
        DraftPrivateNotes = string.Empty;
        DraftWitnessExpression = string.Empty;
        await OpenCoreAsync();
        StatusMessage = "private claim draft created; it changes neither coverage nor publication.";
    });

    [RelayCommand]
    private Task LoadSelectedClaimDraftAsync() => GuardAsync(() =>
    {
        var claim = RequireSelectedClaimDraft();
        DraftPropertySlug = claim.PropertySlug;
        DraftQualifierJson = claim.QualifierJson;
        DraftWitnessExpression = ReadFriedmanExpression(claim.Witness);
        DraftPrivateNotes = claim.PrivateNotes ?? string.Empty;
        StatusMessage = "selected manual claim draft loaded into the authoring bench.";
        return Task.CompletedTask;
    });

    [RelayCommand]
    private Task SaveSelectedClaimDraftAsync() => GuardAsync(async () =>
    {
        var claim = RequireSelectedClaimDraft();
        if (!string.Equals(claim.PropertySlug, DraftPropertySlug, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("a claim draft cannot change property identity; create another draft instead.");
        }

        var witness = BuildWitness(CurrentInteger(), claim.PropertySlug, DraftQualifierJson, DraftWitnessExpression);
        await _research.UpdateClaimDraftAsync(
            claim.RevisionId,
            string.IsNullOrWhiteSpace(DraftQualifierJson) ? "{}" : DraftQualifierJson,
            witness,
            EmptyToNull(DraftPrivateNotes));
        await OpenCoreAsync();
        StatusMessage = "manual claim draft updated in place; accepted evidence and publication remain unchanged.";
    });

    [RelayCommand]
    private Task AcceptSelectedClaimAsync() => GuardAsync(async () =>
    {
        var claim = RequireSelectedClaim();
        await _research.AcceptClaimAsync(claim.RevisionId);
        await OpenCoreAsync();
        StatusMessage = "claim accepted and frozen; publication remains unchanged.";
    });

    [RelayCommand]
    private Task WithdrawSelectedClaimAsync() => GuardAsync(async () =>
    {
        var claim = RequireSelectedClaim();
        await _research.WithdrawClaimAsync(claim.RevisionId);
        await OpenCoreAsync();
        StatusMessage = "draft withdrawn; its lineage remains in the ledger.";
    });

    [RelayCommand]
    private Task StageSelectedClaimAsync() => GuardAsync(async () =>
    {
        var claim = RequireSelectedClaim();
        await _research.StageClaimAsync(claim.RevisionId);
        await OpenCoreAsync();
        StatusMessage = "exact accepted claim staged; no release was generated.";
    });

    [RelayCommand]
    private Task CreateSourceAsync() => GuardAsync(async () =>
    {
        var id = await _research.CreateSourceAsync(new SourceInput(
            SourceSlug,
            SourceTitle,
            EmptyToNull(SourceAuthor),
            SourceUrl,
            EmptyToNull(SourceLicense)));
        CitationSourceId = id.ToString();
        await RefreshOrientationAsync();
        StatusMessage = $"source {id} created; its id is loaded into the citation bench.";
    });

    [RelayCommand]
    private Task CiteSelectedClaimAsync() => GuardAsync(async () =>
    {
        var claim = RequireSelectedClaim();
        await _research.CiteClaimAsync(
            claim.RevisionId,
            ParseSourceId(),
            CitationRole,
            CitationLabel,
            EmptyToNull(CitationLocator),
            CitationPublicSafe);
        await OpenCoreAsync();
        StatusMessage = "citation attached to the exact claim revision.";
    });

    [RelayCommand]
    private Task CreateProfileDraftAsync() => GuardAsync(async () =>
    {
        await _research.CreateProfileDraftAsync(new ProfileDraftInput(
            CurrentInteger(),
            ProfileSlug,
            ProfileTitle,
            ProfileCategory,
            ProfileRegister,
            ProfileBody,
            EmptyToNull(ProfilePrivateNotes),
            ProfileSupersedesRevisionId));
        ProfileSupersedesRevisionId = null;
        await OpenCoreAsync();
        StatusMessage = "private profile draft created; it produces no graph edge.";
    });

    [RelayCommand]
    private Task LoadSelectedProfileDraftAsync() => GuardAsync(() =>
    {
        var profile = RequireSelectedProfileDraft();
        LoadProfileBench(profile);
        ProfileSupersedesRevisionId = null;
        StatusMessage = "selected profile draft loaded into the authoring bench.";
        return Task.CompletedTask;
    });

    [RelayCommand]
    private Task SaveSelectedProfileDraftAsync() => GuardAsync(async () =>
    {
        var profile = RequireSelectedProfileDraft();
        if (!string.Equals(profile.Slug, ProfileSlug, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("a profile draft cannot change entry identity; create another entry instead.");
        }

        await _research.UpdateProfileDraftAsync(
            profile.RevisionId,
            ProfileTitle,
            ProfileCategory,
            ProfileRegister,
            ProfileBody,
            EmptyToNull(ProfilePrivateNotes));
        await OpenCoreAsync();
        StatusMessage = "profile draft updated in place; accepted content and publication remain unchanged.";
    });

    [RelayCommand]
    private Task PrepareProfileRevisionAsync() => GuardAsync(() =>
    {
        var profile = SelectedProfile is { Status: "accepted" } accepted
            ? accepted
            : throw new InvalidOperationException("select an accepted profile revision to prepare a correction.");
        LoadProfileBench(profile);
        ProfileSupersedesRevisionId = profile.RevisionId;
        StatusMessage = "profile content copied into a new draft; the accepted revision is untouched.";
        return Task.CompletedTask;
    });

    private void LoadProfileBench(ProfileEntryView profile)
    {
        ProfileSlug = profile.Slug;
        ProfileTitle = profile.Title;
        ProfileCategory = profile.Category;
        ProfileRegister = profile.EpistemicRegister;
        ProfileBody = profile.Body;
        ProfilePrivateNotes = profile.PrivateNotes ?? string.Empty;
    }

    [RelayCommand]
    private Task CiteSelectedProfileAsync() => GuardAsync(async () =>
    {
        var profile = RequireSelectedProfile();
        await _research.CiteProfileAsync(
            profile.RevisionId,
            ParseSourceId(),
            CitationRole,
            CitationLabel,
            EmptyToNull(CitationLocator),
            CitationPublicSafe);
        await OpenCoreAsync();
        StatusMessage = "citation attached to the exact profile revision.";
    });

    [RelayCommand]
    private Task AcceptSelectedProfileAsync() => GuardAsync(async () =>
    {
        var profile = RequireSelectedProfile();
        await _research.AcceptProfileAsync(profile.RevisionId);
        await OpenCoreAsync();
        StatusMessage = "profile entry accepted and frozen; it remains private until staged and released.";
    });

    [RelayCommand]
    private Task WithdrawSelectedProfileAsync() => GuardAsync(async () =>
    {
        var profile = RequireSelectedProfile();
        await _research.WithdrawProfileAsync(profile.RevisionId);
        await OpenCoreAsync();
        StatusMessage = "profile draft withdrawn.";
    });

    [RelayCommand]
    private Task StageSelectedProfileAsync() => GuardAsync(async () =>
    {
        var profile = RequireSelectedProfile();
        await _research.StageProfileAsync(profile.RevisionId);
        await OpenCoreAsync();
        StatusMessage = "exact accepted profile revision staged; no release was generated.";
    });

    [RelayCommand]
    private Task OpenSelectedDirectRelationAsync() => GuardAsync(async () =>
    {
        var selected = SelectedDirectRelation
            ?? throw new InvalidOperationException("select a direct relation first.");
        await OpenDirectRelationCoreAsync(selected.Slug, recordVisit: true);
    });

    [RelayCommand]
    private Task OpenSelectedConstellationAsync() => GuardAsync(async () =>
    {
        var selected = SelectedConstellation
            ?? throw new InvalidOperationException("select a constellation first.");
        await OpenConstellationCoreAsync(selected.Slug, recordVisit: true);
    });

    [RelayCommand]
    private Task OpenSelectedDirectRelationParticipantAsync() => GuardAsync(async () =>
    {
        var participant = SelectedDirectRelationParticipant
            ?? throw new InvalidOperationException("select a direct-relation participant first.");
        await OpenIntegerCoreAsync(participant.Integer.ToString(), recordVisit: true);
    });

    [RelayCommand]
    private Task OpenSelectedConstellationMemberAsync() => GuardAsync(async () =>
    {
        var member = SelectedConstellationMember
            ?? throw new InvalidOperationException("select a constellation member first.");
        await OpenIntegerCoreAsync(member.Integer.ToString(), recordVisit: true);
    });

    [RelayCommand]
    private Task OpenHeldAsync() => GuardAsync(async () =>
    {
        if (SelectedHeldObject is null)
        {
            throw new InvalidOperationException("select a held object first.");
        }

        await OpenReferenceAsync(SelectedHeldObject, recordVisit: true);
    });

    [RelayCommand]
    private Task HoldActiveAsync() => GuardAsync(() =>
    {
        Hold(CurrentReference());
        StatusMessage = $"held {ActiveObjectSubtitle} {ActiveObjectTitle}; no research state changed.";
        return Task.CompletedTask;
    });

    [RelayCommand]
    private Task RemoveHeldAsync() => GuardAsync(() =>
    {
        if (SelectedHeldObject is not null)
        {
            HeldObjects.Remove(SelectedHeldObject);
        }

        return Task.CompletedTask;
    });

    [RelayCommand]
    private Task TrailBackAsync() => MoveTrailAsync(-1);

    [RelayCommand]
    private Task TrailForwardAsync() => MoveTrailAsync(1);

    [RelayCommand]
    private Task CreateDirectRelationDraftAsync() => GuardAsync(async () =>
    {
        var id = await _research.CreateDirectRelationDraftAsync(BuildDirectRelationInput());
        await OpenDirectRelationCoreAsync(DirectRelationSlug, recordVisit: true);
        StatusMessage = $"direct-relation draft {id} created; it remains private and inference-silent.";
    });

    [RelayCommand]
    private Task LoadActiveDirectRelationDraftAsync() => GuardAsync(() =>
    {
        var relation = RequireActiveDirectRelation();
        LoadDirectRelationBench(relation);
        StatusMessage = "active direct-relation revision loaded into its contextual bench.";
        return Task.CompletedTask;
    });

    [RelayCommand]
    private Task SaveActiveDirectRelationDraftAsync() => GuardAsync(async () =>
    {
        var relation = RequireActiveDirectRelationDraft();
        await _research.UpdateDirectRelationDraftAsync(relation.RevisionId, BuildDirectRelationInput());
        await OpenDirectRelationCoreAsync(relation.Slug, recordVisit: false);
        StatusMessage = "direct-relation draft updated as one aggregate.";
    });

    [RelayCommand]
    private Task AcceptActiveDirectRelationAsync() => GuardAsync(async () =>
    {
        var relation = RequireActiveDirectRelationDraft();
        await _research.AcceptDirectRelationAsync(relation.RevisionId);
        await OpenDirectRelationCoreAsync(relation.Slug, recordVisit: false);
        StatusMessage = "direct relation accepted and frozen; publication remains unchanged.";
    });

    [RelayCommand]
    private Task WithdrawActiveDirectRelationAsync() => GuardAsync(async () =>
    {
        var relation = RequireActiveDirectRelationDraft();
        await _research.WithdrawDirectRelationAsync(relation.RevisionId);
        Hold(new ResearchObjectRef(ResearchObjectKind.Integer, SearchText, SearchText));
        await OpenIntegerCoreAsync(SearchText, recordVisit: true);
        StatusMessage = "direct-relation draft withdrawn; its lineage remains recorded.";
    });

    [RelayCommand]
    private Task PrepareActiveDirectRelationRevisionAsync() => GuardAsync(async () =>
    {
        var relation = RequireActiveDirectRelation();
        await _research.PrepareDirectRelationRevisionAsync(relation.RevisionId);
        await OpenDirectRelationCoreAsync(relation.Slug, recordVisit: false);
        LoadDirectRelationBench(RequireActiveDirectRelation());
        StatusMessage = "accepted direct relation copied into an explicit superseding draft.";
    });

    [RelayCommand]
    private Task CreateConstellationDraftAsync() => GuardAsync(async () =>
    {
        var id = await _research.CreateConstellationDraftAsync(BuildConstellationInput());
        await OpenConstellationCoreAsync(ConstellationSlug, recordVisit: true);
        StatusMessage = $"constellation draft {id} created; no pairwise edges were manufactured.";
    });

    [RelayCommand]
    private Task LoadActiveConstellationDraftAsync() => GuardAsync(() =>
    {
        var constellation = RequireActiveConstellation();
        LoadConstellationBench(constellation);
        StatusMessage = "active constellation revision loaded into its contextual bench.";
        return Task.CompletedTask;
    });

    [RelayCommand]
    private Task SaveActiveConstellationDraftAsync() => GuardAsync(async () =>
    {
        var constellation = RequireActiveConstellationDraft();
        await _research.UpdateConstellationDraftAsync(constellation.RevisionId, BuildConstellationInput());
        await OpenConstellationCoreAsync(constellation.Slug, recordVisit: false);
        StatusMessage = "constellation draft updated as one collective account.";
    });

    [RelayCommand]
    private Task AcceptActiveConstellationAsync() => GuardAsync(async () =>
    {
        var constellation = RequireActiveConstellationDraft();
        await _research.AcceptConstellationAsync(constellation.RevisionId);
        await OpenConstellationCoreAsync(constellation.Slug, recordVisit: false);
        StatusMessage = "constellation accepted and frozen; publication remains unchanged.";
    });

    [RelayCommand]
    private Task WithdrawActiveConstellationAsync() => GuardAsync(async () =>
    {
        var constellation = RequireActiveConstellationDraft();
        await _research.WithdrawConstellationAsync(constellation.RevisionId);
        await OpenIntegerCoreAsync(SearchText, recordVisit: true);
        StatusMessage = "constellation draft withdrawn; its lineage remains recorded.";
    });

    [RelayCommand]
    private Task PrepareActiveConstellationRevisionAsync() => GuardAsync(async () =>
    {
        var constellation = RequireActiveConstellation();
        await _research.PrepareConstellationRevisionAsync(constellation.RevisionId);
        await OpenConstellationCoreAsync(constellation.Slug, recordVisit: false);
        LoadConstellationBench(RequireActiveConstellation());
        StatusMessage = "accepted constellation copied into an explicit superseding draft.";
    });

    [RelayCommand]
    private Task CiteActiveDirectRelationAsync() => GuardAsync(async () =>
    {
        var relation = RequireActiveDirectRelationDraft();
        await _research.CiteDirectRelationAsync(
            relation.RevisionId,
            ParseSourceId(),
            ParseCaptureId(),
            CitationRole,
            CitationLabel,
            EmptyToNull(CitationLocator));
        await OpenDirectRelationCoreAsync(relation.Slug, recordVisit: false);
        StatusMessage = "citation attached to the exact direct-relation draft revision.";
    });

    [RelayCommand]
    private Task CiteActiveConstellationAsync() => GuardAsync(async () =>
    {
        var constellation = RequireActiveConstellationDraft();
        await _research.CiteConstellationAsync(
            constellation.RevisionId,
            ParseSourceId(),
            ParseCaptureId(),
            CitationRole,
            CitationLabel,
            EmptyToNull(CitationLocator));
        await OpenConstellationCoreAsync(constellation.Slug, recordVisit: false);
        StatusMessage = "citation attached to the exact constellation draft revision.";
    });

    private async Task MoveAsync(int direction)
    {
        await GuardAsync(async () =>
        {
            SearchText = (CurrentInteger().Value + direction).ToString();
            await OpenIntegerCoreAsync(SearchText, recordVisit: true);
        });
    }

    private Task OpenCoreAsync() => OpenIntegerCoreAsync(SearchText, recordVisit: true);

    private async Task OpenIntegerCoreAsync(string canonicalInteger, bool recordVisit)
    {
        if (recordVisit)
        {
            HoldCurrentBeforeTraversal();
        }

        await RefreshOrientationAsync();
        var dossier = await _research.OpenAsync(canonicalInteger);
        SelectedInteger = dossier.Integer.ToString();
        SearchText = SelectedInteger;
        ActiveObjectKind = ResearchObjectKind.Integer;
        ActiveObjectTitle = SelectedInteger;
        ActiveObjectSubtitle = "integer dossier";
        ActiveDirectRelation = null;
        ActiveConstellation = null;
        Claims.Clear();
        foreach (var claim in dossier.Claims)
        {
            Claims.Add(claim);
        }

        ProfileEntries.Clear();
        foreach (var profile in dossier.ProfileEntries)
        {
            ProfileEntries.Add(profile);
        }

        DirectRelations.Clear();
        foreach (var relation in dossier.DirectRelations)
        {
            DirectRelations.Add(relation);
        }

        Constellations.Clear();
        foreach (var constellation in dossier.Constellations)
        {
            Constellations.Add(constellation);
        }

        SelectedClaim = Claims.FirstOrDefault(claim => claim.RevisionId > 0);
        SelectedProfile = ProfileEntries.FirstOrDefault();
        SelectedDirectRelation = DirectRelations.FirstOrDefault();
        SelectedConstellation = Constellations.FirstOrDefault();
        if (recordVisit)
        {
            RecordVisit(CurrentReference());
        }
        var staging = await _research.GetStagingSummaryAsync();
        StagingMessage = $"{staging.Claims} claims · {staging.ProfileEntries} profiles · {staging.Citations} citations staged";
        StatusMessage = $"record {SelectedInteger} open · {Claims.Count} property states · {DirectRelations.Count} direct relations · {Constellations.Count} constellations";
    }

    private async Task OpenDirectRelationCoreAsync(string slug, bool recordVisit)
    {
        HoldCurrentBeforeTraversal();
        var relation = await _research.OpenDirectRelationAsync(slug);
        ActiveObjectKind = ResearchObjectKind.DirectRelation;
        ActiveDirectRelation = relation;
        ActiveConstellation = null;
        ActiveObjectTitle = relation.Proposition;
        ActiveObjectSubtitle = $"direct relation · {relation.Slug}";
        if (recordVisit)
        {
            RecordVisit(CurrentReference());
        }

        StatusMessage = $"direct relation {relation.Slug} open · revision {relation.Revision} · {relation.ActiveLabel}";
    }

    private async Task OpenConstellationCoreAsync(string slug, bool recordVisit)
    {
        HoldCurrentBeforeTraversal();
        var constellation = await _research.OpenConstellationAsync(slug);
        ActiveObjectKind = ResearchObjectKind.Constellation;
        ActiveConstellation = constellation;
        ActiveDirectRelation = null;
        ActiveObjectTitle = constellation.Name;
        ActiveObjectSubtitle = $"constellation · {constellation.Slug}";
        if (recordVisit)
        {
            RecordVisit(CurrentReference());
        }

        StatusMessage = $"constellation {constellation.Slug} open · revision {constellation.Revision} · {constellation.ActiveLabel}";
    }

    private async Task OpenReferenceAsync(ResearchObjectRef reference, bool recordVisit)
    {
        HoldCurrentBeforeTraversal();
        switch (reference.Kind)
        {
            case ResearchObjectKind.Integer:
                await OpenIntegerCoreAsync(reference.Key, recordVisit);
                break;
            case ResearchObjectKind.DirectRelation:
                await OpenDirectRelationCoreAsync(reference.Key, recordVisit);
                break;
            case ResearchObjectKind.Constellation:
                await OpenConstellationCoreAsync(reference.Key, recordVisit);
                break;
            default:
                throw new InvalidOperationException($"unknown research object kind '{reference.Kind}'.");
        }
    }

    private async Task MoveTrailAsync(int direction)
    {
        await GuardAsync(async () =>
        {
            var target = _trailIndex + direction;
            if (target < 0 || target >= _trail.Count)
            {
                throw new InvalidOperationException(direction < 0
                    ? "the visit trail has no earlier object."
                    : "the visit trail has no later object.");
            }

            _trailIndex = target;
            await OpenReferenceAsync(_trail[_trailIndex], recordVisit: false);
            StatusMessage = $"returned through the session trail to {_trail[_trailIndex].TypeLabel} {_trail[_trailIndex].Label}; no relation was created.";
        });
    }

    private ResearchObjectRef CurrentReference() => ActiveObjectKind switch
    {
        ResearchObjectKind.Integer => new(ResearchObjectKind.Integer, SelectedInteger, SelectedInteger),
        ResearchObjectKind.DirectRelation when ActiveDirectRelation is not null =>
            new(ResearchObjectKind.DirectRelation, ActiveDirectRelation.Slug, ActiveDirectRelation.Proposition),
        ResearchObjectKind.Constellation when ActiveConstellation is not null =>
            new(ResearchObjectKind.Constellation, ActiveConstellation.Slug, ActiveConstellation.Name),
        _ => throw new InvalidOperationException("the active research object is incomplete."),
    };

    private void HoldCurrentBeforeTraversal()
    {
        if (ActiveObjectTitle != "—")
        {
            Hold(CurrentReference());
        }
    }

    private void Hold(ResearchObjectRef reference)
    {
        if (HeldObjects.All(item => item.StableKey != reference.StableKey))
        {
            HeldObjects.Add(reference);
        }
    }

    private void RecordVisit(ResearchObjectRef reference)
    {
        if (_trailIndex >= 0 && _trail[_trailIndex].StableKey == reference.StableKey)
        {
            return;
        }

        if (_trailIndex + 1 < _trail.Count)
        {
            _trail.RemoveRange(_trailIndex + 1, _trail.Count - _trailIndex - 1);
        }

        _trail.Add(reference);
        if (_trail.Count > 64)
        {
            _trail.RemoveAt(0);
        }

        _trailIndex = _trail.Count - 1;
        VisitTrail.Clear();
        foreach (var item in _trail)
        {
            VisitTrail.Add(item);
        }
    }

    partial void OnActiveDirectRelationChanged(DirectRelationView? value)
    {
        OnPropertyChanged(nameof(IsActiveDirectRelationDraft));
        OnPropertyChanged(nameof(IsActiveDirectRelationAccepted));
        SelectedDirectRelationParticipant = value?.Participants.OrderBy(item => item.Ordinal).FirstOrDefault();
        if (value?.Status == AuthoredRevisionStatus.Draft)
        {
            LoadDirectRelationBench(value);
        }
    }

    partial void OnActiveConstellationChanged(ConstellationView? value)
    {
        OnPropertyChanged(nameof(IsActiveConstellationDraft));
        OnPropertyChanged(nameof(IsActiveConstellationAccepted));
        SelectedConstellationMember = value?.Members.OrderBy(item => item.Ordinal).FirstOrDefault();
        if (value?.Status == AuthoredRevisionStatus.Draft)
        {
            LoadConstellationBench(value);
        }
    }

    private async Task RefreshOrientationAsync() =>
        LedgerOrientationLabel = (await _orientationReader.InspectAsync(DatabasePath)).DesktopLabel;

    private async Task GuardAsync(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (Exception exception)
        {
            StatusMessage = $"halted · {exception.Message}";
        }
    }

    private IntegerValue CurrentInteger() => IntegerValue.Parse(SelectedInteger == "—" ? SearchText : SelectedInteger);

    private ClaimView RequireSelectedClaim() =>
        SelectedClaim is { RevisionId: > 0 } claim
            ? claim
            : throw new InvalidOperationException("select an evidence-bearing claim revision first.");

    private ClaimView RequireSelectedClaimDraft() =>
        SelectedClaim is { RevisionId: > 0, RevisionStatus: "draft", Origin: ClaimOrigin.Manual } claim
            ? claim
            : throw new InvalidOperationException("select a manual claim draft first.");

    private ProfileEntryView RequireSelectedProfile() =>
        SelectedProfile
            ?? throw new InvalidOperationException("select a profile revision first.");

    private ProfileEntryView RequireSelectedProfileDraft() =>
        SelectedProfile is { Status: "draft" } profile
            ? profile
            : throw new InvalidOperationException("select a profile draft first.");

    private long ParseSourceId() =>
        long.TryParse(CitationSourceId, out var id) && id > 0
            ? id
            : throw new InvalidOperationException("enter a source id from the source bench.");

    private long? ParseCaptureId() =>
        string.IsNullOrWhiteSpace(CitationCaptureId)
            ? null
            : long.TryParse(CitationCaptureId, out var id) && id > 0
                ? id
                : throw new InvalidOperationException("capture id must be a positive integer when supplied.");

    private DirectRelationView RequireActiveDirectRelation() =>
        ActiveDirectRelation
            ?? throw new InvalidOperationException("open a direct relation first.");

    private DirectRelationView RequireActiveDirectRelationDraft() =>
        ActiveDirectRelation is { Status: AuthoredRevisionStatus.Draft } relation
            ? relation
            : throw new InvalidOperationException("open a direct-relation draft first.");

    private ConstellationView RequireActiveConstellation() =>
        ActiveConstellation
            ?? throw new InvalidOperationException("open a constellation first.");

    private ConstellationView RequireActiveConstellationDraft() =>
        ActiveConstellation is { Status: AuthoredRevisionStatus.Draft } constellation
            ? constellation
            : throw new InvalidOperationException("open a constellation draft first.");

    private DirectRelationDraftInput BuildDirectRelationInput() =>
        new(
            ResearchLineageSlug.Parse(DirectRelationSlug).ToString(),
            DirectRelationDirectionality,
            DirectRelationProposition,
            DirectRelationWarrant,
            DirectRelationRegister,
            EmptyToNull(DirectRelationPrivateNotes),
            new[]
            {
                new DirectRelationParticipant(
                    IntegerValue.Parse(DirectRelationFirstInteger),
                    DirectRelationFirstRole,
                    0),
                new DirectRelationParticipant(
                    IntegerValue.Parse(DirectRelationSecondInteger),
                    DirectRelationSecondRole,
                    1),
            });

    private ConstellationDraftInput BuildConstellationInput() =>
        new(
            ResearchLineageSlug.Parse(ConstellationSlug).ToString(),
            ConstellationName,
            ConstellationAccount,
            ConstellationRegister,
            EmptyToNull(ConstellationPrivateNotes),
            ParseConstellationMembers(ConstellationMembers));

    private static IReadOnlyList<ConstellationMember> ParseConstellationMembers(string material) =>
        material.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select((line, ordinal) =>
            {
                var fields = line.Split('|', 3, StringSplitOptions.TrimEntries);
                if (fields.Length < 2)
                {
                    throw new FormatException("each constellation member line is integer | role | optional note.");
                }

                return new ConstellationMember(
                    IntegerValue.Parse(fields[0]),
                    fields[1],
                    fields.Length == 3 ? EmptyToNull(fields[2]) : null,
                    ordinal);
            })
            .ToArray();

    private void LoadDirectRelationBench(DirectRelationView relation)
    {
        DirectRelationSlug = relation.Slug;
        DirectRelationDirectionality = relation.Directionality;
        DirectRelationProposition = relation.Proposition;
        DirectRelationWarrant = relation.Warrant;
        DirectRelationRegister = relation.EpistemicRegister;
        DirectRelationPrivateNotes = relation.PrivateNotes ?? string.Empty;
        var participants = relation.Participants.OrderBy(item => item.Ordinal).ToArray();
        DirectRelationFirstInteger = participants.ElementAtOrDefault(0)?.Integer.ToString() ?? string.Empty;
        DirectRelationFirstRole = participants.ElementAtOrDefault(0)?.Role ?? "participant";
        DirectRelationSecondInteger = participants.ElementAtOrDefault(1)?.Integer.ToString() ?? string.Empty;
        DirectRelationSecondRole = participants.ElementAtOrDefault(1)?.Role ?? "participant";
    }

    private void LoadConstellationBench(ConstellationView constellation)
    {
        ConstellationSlug = constellation.Slug;
        ConstellationName = constellation.Name;
        ConstellationAccount = constellation.Account;
        ConstellationRegister = constellation.EpistemicRegister;
        ConstellationPrivateNotes = constellation.PrivateNotes ?? string.Empty;
        ConstellationMembers = string.Join(
            Environment.NewLine,
            constellation.Members.OrderBy(item => item.Ordinal).Select(item =>
                $"{item.Integer} | {item.Role} | {item.MemberNotes}"));
    }

    private static EvidenceWitness? BuildWitness(
        IntegerValue integer,
        string propertySlug,
        string qualifierJson,
        string expression)
    {
        if (propertySlug == PropertySlugs.DecimalFriedman)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return null;
            }

            using var qualifier = JsonDocument.Parse(
                string.IsNullOrWhiteSpace(qualifierJson) ? "{}" : qualifierJson);
            var orderly = qualifier.RootElement.TryGetProperty("orderly", out var node)
                && node.ValueKind == JsonValueKind.True;
            return FriedmanExpression.Validate(integer, expression, orderly).Witness;
        }

        IPropertyEvaluator? evaluator = propertySlug switch
        {
            PropertySlugs.Prime => new PrimeEvaluator(),
            PropertySlugs.Triangular => new TriangularEvaluator(),
            PropertySlugs.DecimalPalindrome => new DecimalPalindromeEvaluator(),
            PropertySlugs.DecimalNarcissistic => new DecimalNarcissisticEvaluator(),
            _ => null,
        };
        return evaluator?
            .Evaluate(integer, integer)
            .SingleOrDefault()?
            .Witness;
    }

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string ReadFriedmanExpression(EvidenceWitness? witness)
    {
        if (witness?.Kind != "digit_expression")
        {
            return string.Empty;
        }

        using var payload = JsonDocument.Parse(witness.PayloadJson);
        return payload.RootElement.TryGetProperty("expression", out var expression)
            && expression.ValueKind == JsonValueKind.String
                ? expression.GetString() ?? string.Empty
                : string.Empty;
    }
}
