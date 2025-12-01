using FileSystemCLI.Models;
using FileSystemCLI.Services.CompareType;
using FileSystemCommon.Models.Sync.Definitions;

namespace FileSystemCLI.Services.Mode;

class LocalToServerModeHandler(BaseSyncFileComparer fileComparer, SyncConflictHandlingType conflictHandlingType)
    : SyncModeHandler(fileComparer, conflictHandlingType)
{
    public override SyncMode Mode => SyncMode.LocalToServer;

    public override async Task<SyncActionType> GetActionOfBothFiles(FilePairModel pair)
    {
        pair.LocalCompareValue = pair.LocalFileExists
            ? await fileComparer.GetLocalCompareValue(pair.LocalFilePath) : null;

        return fileComparer.EqualsValue(pair.ServerCompareValue, pair.LocalCompareValue) 
            ? SyncActionType.Equal : SyncActionType.CopyToServer;
    }

    public override Task<SyncActionType> GetActionOfSingleFiles(FilePairModel pair)
    {
        return Task.FromResult(pair.LocalFileExists ? SyncActionType.CopyToServer : SyncActionType.DeleteFromServer);
    }
}