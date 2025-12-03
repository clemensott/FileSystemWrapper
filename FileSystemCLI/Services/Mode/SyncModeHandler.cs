using FileSystemCLI.Models;
using FileSystemCLI.Services.CompareType;
using FileSystemCommon.Models.Sync.Definitions;

namespace FileSystemCLI.Services.Mode;

abstract class SyncModeHandler(
    BaseSyncFileComparer fileComparer,
    SyncConflictHandlingType conflictHandlingType,
    bool preLoadServerCompareValue = false)
{
    protected readonly BaseSyncFileComparer fileComparer = fileComparer;
    protected readonly SyncConflictHandlingType conflictHandlingType = conflictHandlingType;

    public bool PreloadServerCompareValue { get; } = preLoadServerCompareValue;

    public abstract SyncMode Mode { get; }

    public abstract Task<SyncActionType> GetActionOfBothFiles(FilePairModel pair);

    protected SyncActionType SolveConflict(FilePairModel pair)
    {
        switch (conflictHandlingType)
        {
            case SyncConflictHandlingType.PreferServer:
                return SyncActionType.CopyToLocalByConflict;

            case SyncConflictHandlingType.PreferLocal:
                return SyncActionType.CopyToServerByConflict;

            case SyncConflictHandlingType.Ignore:
                return SyncActionType.Ignore;
        }

        throw new ArgumentException("Value not Implemented:" + conflictHandlingType, nameof(conflictHandlingType));
    }

    public abstract Task<SyncActionType> GetActionOfSingleFiles(FilePairModel pair);
}