using Polyvalent.Application;
using Polyvalent.Data;
using Polyvalent.Desktop.ViewModels;
using Polyvalent.Domain;

namespace Polyvalent.Desktop.Tests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public async Task Rolodex_opens_navigates_and_preserves_evidence_state()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"polyvalent-desktop-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var database = new SqliteDatabase(Path.Combine(directory, "test.sqlite3"));
            await new CorpusSeeder(database).SeedAsync();
            var orientationReader = new LedgerInspector();
            var orientation = await orientationReader.InspectAsync(database.Path);
            var viewModel = new MainWindowViewModel(
                new ResearchService(new PolyvalentStore(database)),
                database.Path,
                orientationReader,
                orientation);

            await viewModel.InitializeAsync();

            Assert.Equal("153", viewModel.SelectedInteger);
            Assert.Equal("reference corpus · seed-v6", viewModel.LedgerOrientationLabel);
            Assert.Contains(viewModel.Claims, claim =>
                claim.PropertySlug == PropertySlugs.DecimalFriedman
                && claim.State == KnowledgeState.Positive);

            viewModel.SourceSlug = "fictional-desktop-source";
            viewModel.SourceTitle = "fictional desktop source";
            viewModel.SourceUrl = "https://example.test/desktop-source";
            await viewModel.CreateSourceCommand.ExecuteAsync(null);
            Assert.Equal(
                "non-current or authored ledger · automatic reseeding disabled",
                viewModel.LedgerOrientationLabel);

            await viewModel.NextCommand.ExecuteAsync(null);

            Assert.Equal("154", viewModel.SelectedInteger);
            Assert.Equal(17, viewModel.Claims.Count);
            Assert.Contains("record 154 open", viewModel.StatusMessage, StringComparison.Ordinal);

            viewModel.DraftPropertySlug = PropertySlugs.Prime;
            viewModel.DraftPrivateNotes = "first draft note";
            await viewModel.CreateClaimDraftCommand.ExecuteAsync(null);
            viewModel.SelectedClaim = viewModel.Claims.Single(claim => claim.RevisionStatus == "draft");
            await viewModel.LoadSelectedClaimDraftCommand.ExecuteAsync(null);
            viewModel.DraftPrivateNotes = "edited draft note";
            await viewModel.SaveSelectedClaimDraftCommand.ExecuteAsync(null);

            Assert.Contains(viewModel.Claims, claim =>
                claim.RevisionStatus == "draft"
                && claim.PrivateNotes == "edited draft note");

            viewModel.ProfileSlug = "desktop-draft";
            viewModel.ProfileTitle = "first title";
            viewModel.ProfileBody = "first body";
            await viewModel.CreateProfileDraftCommand.ExecuteAsync(null);
            viewModel.SelectedProfile = viewModel.ProfileEntries.Single();
            await viewModel.LoadSelectedProfileDraftCommand.ExecuteAsync(null);
            viewModel.ProfileBody = "edited body";
            await viewModel.SaveSelectedProfileDraftCommand.ExecuteAsync(null);

            Assert.Equal("edited body", viewModel.ProfileEntries.Single().Body);
            Assert.Equal(
                "non-current or authored ledger · automatic reseeding disabled",
                viewModel.LedgerOrientationLabel);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
