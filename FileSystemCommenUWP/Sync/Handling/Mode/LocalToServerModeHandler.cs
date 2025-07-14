using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling.CompareType;
using System.Threading.Tasks;

namespace FileSystemCommonUWP.Sync.Handling.Mode
{
    class LocalToServerModeHandler : SyncModeHandler
    {
        public override SyncMode Mode => SyncMode.LocalToServer;

        public LocalToServerModeHandler(BaseSyncFileComparer fileComparer, SyncConflictHandlingType conflictHandlingType) 
            : base(fileComparer, conflictHandlingType)
        {
        }

        public override async Task<SyncActionType> GetActionOfBothFiles(FilePair pair)
        {
            pair.LocalCompareValue = await fileComparer.GetLocalCompareValue(pair.LocalFile);

            return fileComparer.EqualsValue(pair.ServerCompareValue, pair.LocalCompareValue) ?
                SyncActionType.Equal : SyncActionType.CopyToServer;
        }

        public override Task<SyncActionType> GetActionOfSingleFiles(FilePair pair)
        {
            return Task.FromResult(pair.LocalFile != null ? SyncActionType.CopyToServer : SyncActionType.DeleteFromServer);
        }
    }
}
