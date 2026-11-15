using Microsoft.Data.Sqlite;
using Polyvalent.Application;

namespace Polyvalent.Data;

public sealed class LedgerInspectionException(string message) : InvalidOperationException(message);

public sealed class LedgerInspector : ILedgerOrientationReader
{
    public async Task<LedgerOrientation> InspectAsync(
        string databasePath,
        CancellationToken cancellationToken = default)
    {
        var path = Path.GetFullPath(databasePath);
        FileAttributes attributes;
        try
        {
            attributes = File.GetAttributes(path);
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return new LedgerOrientation(
                LedgerOrientationKind.Missing,
                path,
                null,
                ReferenceCorpusShape.RecipeId,
                0,
                0,
                0);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw Incompatible();
        }

        if (attributes.HasFlag(FileAttributes.Directory))
        {
            throw Incompatible();
        }

        try
        {
            var before = InspectionFileSnapshot.Capture(path);
            before.AssertNoRecoveryState();

            var database = new SqliteDatabase(path);
            LedgerOrientation result;
            await using (var connection = await database.OpenReadOnlyConnectionAsync(cancellationToken))
            {
                var schemaVersion = await SqliteDatabase.ValidateCurrentSchemaAsync(connection, cancellationToken);
                var propertyCount = await ReferenceCorpusShape.CountPropertiesAsync(connection, cancellationToken);
                var claimCount = await ReferenceCorpusShape.CountAcceptedClaimsAsync(connection, cancellationToken);
                var profileCount = await ReferenceCorpusShape.CountProfileEntriesAsync(connection, cancellationToken);

                LedgerOrientationKind kind;
                if (await ReferenceCorpusShape.IsEmptyAsync(connection, cancellationToken))
                {
                    kind = LedgerOrientationKind.Empty;
                }
                else
                {
                    try
                    {
                        await ReferenceCorpusShape.AssertCurrentAsync(connection, cancellationToken);
                        kind = LedgerOrientationKind.CurrentReference;
                    }
                    catch (ReferenceCorpusMismatchException)
                    {
                        kind = LedgerOrientationKind.NonCurrentOrAuthored;
                    }
                }

                result = new LedgerOrientation(
                    kind,
                    path,
                    schemaVersion,
                    ReferenceCorpusShape.RecipeId,
                    propertyCount,
                    claimCount,
                    profileCount);
            }

            var after = InspectionFileSnapshot.Capture(path);
            if (before != after)
            {
                throw new LedgerInspectionException(
                    "selected ledger changed during inspection; close writers and retry.");
            }

            return result;
        }
        catch (LedgerInspectionException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is SqliteException
                or InvalidOperationException
                or IOException
                or UnauthorizedAccessException)
        {
            throw Incompatible();
        }
    }

    private static LedgerInspectionException Incompatible() =>
        new("selected path is unreadable or is not a compatible current-schema polyvalent ledger.");

    private sealed record InspectionFileSnapshot(
        FileStamp Database,
        FileStamp Wal,
        FileStamp Journal,
        FileStamp Shm)
    {
        public static InspectionFileSnapshot Capture(string path) =>
            new(
                FileStamp.Capture(path),
                FileStamp.Capture($"{path}-wal"),
                FileStamp.Capture($"{path}-journal"),
                FileStamp.Capture($"{path}-shm"));

        public void AssertNoRecoveryState()
        {
            if (Wal is { Exists: true, Length: > 0 })
            {
                throw new LedgerInspectionException(
                    "selected ledger has an active write-ahead log; close writers and retry inspection.");
            }

            if (Journal is { Exists: true, Length: > 0 })
            {
                throw new LedgerInspectionException(
                    "selected ledger has an active rollback journal; close writers and retry inspection.");
            }
        }
    }

    private sealed record FileStamp(bool Exists, long Length, long LastWriteTimeUtcTicks)
    {
        public static FileStamp Capture(string path)
        {
            try
            {
                var attributes = File.GetAttributes(path);
                if (attributes.HasFlag(FileAttributes.Directory))
                {
                    throw new IOException("expected a file but found a directory.");
                }

                var info = new FileInfo(path);
                return new FileStamp(true, info.Length, info.LastWriteTimeUtc.Ticks);
            }
            catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
            {
                return new FileStamp(false, 0, 0);
            }
        }
    }
}

public sealed class NonCurrentOrAuthoredLedgerException(string message, Exception innerException)
    : InvalidOperationException(message, innerException);
