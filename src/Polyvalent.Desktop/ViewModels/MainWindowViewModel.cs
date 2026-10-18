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

    public MainWindowViewModel(ResearchService research, string databasePath)
    {
        _research = research;
        DatabasePath = databasePath;
    }

    public string DatabasePath { get; }

    public ObservableCollection<ClaimView> Claims { get; } = [];

    public ObservableCollection<ProfileEntryView> ProfileEntries { get; } = [];

    public ObservableCollection<PropertyOption> Properties { get; } = [];

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

    private async Task MoveAsync(int direction)
    {
        await GuardAsync(async () =>
        {
            SearchText = (CurrentInteger().Value + direction).ToString();
            await OpenCoreAsync();
        });
    }

    private async Task OpenCoreAsync()
    {
        var dossier = await _research.OpenAsync(SearchText);
        SelectedInteger = dossier.Integer.ToString();
        SearchText = SelectedInteger;
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

        SelectedClaim = Claims.FirstOrDefault(claim => claim.RevisionId > 0);
        SelectedProfile = ProfileEntries.FirstOrDefault();
        var staging = await _research.GetStagingSummaryAsync();
        StagingMessage = $"{staging.Claims} claims · {staging.ProfileEntries} profiles · {staging.Citations} citations staged";
        StatusMessage = $"record {SelectedInteger} open · {Claims.Count} property states · {ProfileEntries.Count} profile entries";
    }

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
