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

    [Fact]
    public async Task Room_holds_and_traverses_typed_relational_objects_without_persisting_navigation()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"polyvalent-desktop-room-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var database = new SqliteDatabase(Path.Combine(directory, "test.sqlite3"));
            await database.InitializeAsync();
            var orientationReader = new LedgerInspector();
            var viewModel = new MainWindowViewModel(
                new ResearchService(new PolyvalentStore(database)),
                database.Path,
                orientationReader,
                await orientationReader.InspectAsync(database.Path));
            await viewModel.InitializeAsync();

            viewModel.DirectRelationSlug = "prime-index-137";
            viewModel.DirectRelationFirstInteger = "137";
            viewModel.DirectRelationFirstRole = "index";
            viewModel.DirectRelationSecondInteger = "773";
            viewModel.DirectRelationSecondRole = "prime";
            viewModel.DirectRelationProposition = "773 is the 137th prime";
            viewModel.DirectRelationWarrant = "prime enumeration";
            viewModel.DirectRelationRegister = "computed";
            await viewModel.CreateDirectRelationDraftCommand.ExecuteAsync(null);

            Assert.True(viewModel.IsDirectRelationActive);
            Assert.Equal("prime-index-137", viewModel.ActiveDirectRelation?.Slug);
            Assert.Contains(viewModel.HeldObjects, item =>
                item.Kind == ResearchObjectKind.Integer && item.Key == "153");

            await viewModel.TrailBackCommand.ExecuteAsync(null);
            Assert.True(viewModel.IsIntegerActive);
            Assert.Equal("153", viewModel.SelectedInteger);
            Assert.Contains("no relation was created", viewModel.StatusMessage, StringComparison.Ordinal);

            await viewModel.TrailForwardCommand.ExecuteAsync(null);
            Assert.True(viewModel.IsDirectRelationActive);
            Assert.Equal(2, viewModel.VisitTrail.Count);

            await viewModel.AcceptActiveDirectRelationCommand.ExecuteAsync(null);
            Assert.True(viewModel.IsActiveDirectRelationAccepted);
            await viewModel.PrepareActiveDirectRelationRevisionCommand.ExecuteAsync(null);
            Assert.True(viewModel.IsActiveDirectRelationDraft);
            Assert.Equal(2, viewModel.ActiveDirectRelation?.Revision);

            await viewModel.OpenSelectedDirectRelationParticipantCommand.ExecuteAsync(null);
            Assert.True(viewModel.IsIntegerActive);
            Assert.Equal("137", viewModel.SelectedInteger);
            Assert.Contains(viewModel.HeldObjects, item =>
                item.Kind == ResearchObjectKind.DirectRelation && item.Key == "prime-index-137");

            viewModel.SelectedHeldObject = viewModel.HeldObjects.Single(item =>
                item.Kind == ResearchObjectKind.DirectRelation);
            await viewModel.OpenHeldCommand.ExecuteAsync(null);
            Assert.True(viewModel.IsDirectRelationActive);
            Assert.Equal(2, viewModel.ActiveDirectRelation?.Revision);

            viewModel.SearchText = "137";
            await viewModel.OpenCommand.ExecuteAsync(null);
            viewModel.ConstellationSlug = "permutation-rehearsal";
            viewModel.ConstellationName = "permutation rehearsal";
            viewModel.ConstellationAccount = "a plainly fictional company";
            viewModel.ConstellationRegister = "fictional fixture";
            viewModel.ConstellationMembers = "137 | threshold | opens the room\n173 | return | closes the room";
            await viewModel.CreateConstellationDraftCommand.ExecuteAsync(null);
            Assert.True(viewModel.IsConstellationActive);
            await viewModel.OpenSelectedConstellationMemberCommand.ExecuteAsync(null);
            Assert.True(viewModel.IsIntegerActive);
            Assert.Equal("137", viewModel.SelectedInteger);
            Assert.Contains(viewModel.HeldObjects, item =>
                item.Kind == ResearchObjectKind.Constellation && item.Key == "permutation-rehearsal");

            await using var connection = await database.OpenConnectionAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT
                    (SELECT COUNT(*) FROM direct_relation_revisions),
                    (SELECT COUNT(*) FROM constellation_revisions);
                """;
            await using var reader = await command.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.Equal(2, reader.GetInt32(0));
            Assert.Equal(1, reader.GetInt32(1));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
