namespace Polyvalent.Application;

public enum LedgerOrientationKind
{
    Missing,
    Empty,
    MigrationRequired,
    CurrentReference,
    NonCurrentOrAuthored,
}

public sealed record LedgerOrientation(
    LedgerOrientationKind Kind,
    string DatabasePath,
    int? SchemaVersion,
    string CurrentRecipe,
    int PropertyCount,
    int AcceptedClaimCount,
    int ProfileEntryCount)
{
    public string Outcome => Kind switch
    {
        LedgerOrientationKind.Missing => "missing",
        LedgerOrientationKind.Empty => "empty",
        LedgerOrientationKind.MigrationRequired => "migration-required",
        LedgerOrientationKind.CurrentReference => "current-reference",
        LedgerOrientationKind.NonCurrentOrAuthored => "non-current-or-authored",
        _ => throw new InvalidOperationException($"unknown ledger orientation '{Kind}'."),
    };

    public string Meaning => Kind switch
    {
        LedgerOrientationKind.Missing => "no ledger exists at this path",
        LedgerOrientationKind.Empty => "compatible empty cabinet",
        LedgerOrientationKind.MigrationRequired =>
            "exact known prior schema · additive migration required before research use",
        LedgerOrientationKind.CurrentReference => $"exact current reference corpus · {CurrentRecipe}",
        LedgerOrientationKind.NonCurrentOrAuthored =>
            "valid cabinet that does not exactly match today's reference recipe",
        _ => throw new InvalidOperationException($"unknown ledger orientation '{Kind}'."),
    };

    public string SeedDisposition => Kind switch
    {
        LedgerOrientationKind.Missing => "allowed; a separate seed command may create the reference ledger",
        LedgerOrientationKind.Empty => "allowed; a separate seed command may populate the reference ledger",
        LedgerOrientationKind.MigrationRequired => "refused until the ledger is migrated with an automatic backup",
        LedgerOrientationKind.CurrentReference => "idempotent",
        LedgerOrientationKind.NonCurrentOrAuthored => "refused; automatic reseeding remains disabled",
        _ => throw new InvalidOperationException($"unknown ledger orientation '{Kind}'."),
    };

    public string SafeNextAction => Kind switch
    {
        LedgerOrientationKind.Missing => "run seed explicitly for this path when a new reference cabinet is intended",
        LedgerOrientationKind.Empty => "run seed explicitly for this path when the reference corpus is intended",
        LedgerOrientationKind.MigrationRequired =>
            "run init deliberately; the prior ledger will be backed up before migration",
        LedgerOrientationKind.CurrentReference => "open this path with the private desktop",
        LedgerOrientationKind.NonCurrentOrAuthored =>
            "open this path deliberately with the private desktop; do not run automatic seed",
        _ => throw new InvalidOperationException($"unknown ledger orientation '{Kind}'."),
    };

    public string DesktopLabel => Kind switch
    {
        LedgerOrientationKind.Missing => "missing ledger",
        LedgerOrientationKind.Empty => "empty ledger",
        LedgerOrientationKind.MigrationRequired => "known prior ledger · migration required",
        LedgerOrientationKind.CurrentReference => $"reference corpus · {CurrentRecipe}",
        LedgerOrientationKind.NonCurrentOrAuthored =>
            "non-current or authored ledger · automatic reseeding disabled",
        _ => throw new InvalidOperationException($"unknown ledger orientation '{Kind}'."),
    };
}

public interface ILedgerOrientationReader
{
    Task<LedgerOrientation> InspectAsync(
        string databasePath,
        CancellationToken cancellationToken = default);
}
