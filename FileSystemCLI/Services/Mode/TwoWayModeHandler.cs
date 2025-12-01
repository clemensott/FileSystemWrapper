using FileSystemCLI.Models;
using FileSystemCLI.Services.CompareType;
using FileSystemCommon.Models.Sync.Definitions;

namespace FileSystemCLI.Services.Mode;

class TwoWayModeHandler(
    BaseSyncFileComparer fileComparer,
    SyncPairState lastState,
    SyncConflictHandlingType conflictHandlingType)
    : SyncModeHandler(fileComparer, conflictHandlingType, true)
{
    public override SyncMode Mode => SyncMode.TwoWay;

    public override async Task<SyncActionType> GetActionOfBothFiles(FilePairModel pair)
    {
        pair.LocalCompareValue = pair.LocalFileExists
            ? await fileComparer.GetLocalCompareValue(pair.LocalFilePath) : null;

        if (fileComparer.EqualsValue(pair.ServerCompareValue, pair.LocalCompareValue))
        {
            return SyncActionType.Equal;
        }

        if (lastState.TryGetFile(pair.RelativePath, out SyncPairStateFileModel last))
        {
            if (fileComparer.EqualsValue(last.ServerCompareValue, pair.ServerCompareValue))
            {
                return SyncActionType.CopyToServer;
            }
            else if (fileComparer.EqualsValue(last.LocalCompareValue, pair.LocalCompareValue))
            {
                return SyncActionType.CopyToLocal;
            }
        }

        return SolveConflict(pair);
    }

    public override async Task<SyncActionType> GetActionOfSingleFiles(FilePairModel pair)
    {
        SyncPairStateFileModel last;

        if (pair.ServerFileExists)
        {
            if (lastState.TryGetFile(pair.RelativePath, out last) &&
                fileComparer.EqualsValue(last.ServerCompareValue, pair.ServerCompareValue))
            {
                return SyncActionType.DeleteFromServer;
            }

            return SyncActionType.CopyToLocal;
        }

        if (pair.LocalFileExists)
        {
            pair.LocalCompareValue = await fileComparer.GetLocalCompareValue(pair.LocalFilePath);

            if (lastState.TryGetFile(pair.RelativePath, out last) &&
                fileComparer.EqualsValue(last.LocalCompareValue, pair.LocalCompareValue))
            {
                return SyncActionType.DeleteFromLocal;
            }

            return SyncActionType.CopyToServer;
        }

        throw new InvalidOperationException("Server or local file must exist");
    }
}