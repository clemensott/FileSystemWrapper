using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling.CompareType;
using System.Threading.Tasks;

namespace FileSystemCommonUWP.Sync.Handling.Mode
{
    class LocalToServerModeHandler : SyncModeHandler
    {
        public override SyncMode Mode => SyncMode.LocalToServer;

        public LocalToServerModeHandler(ISyncFileComparer fileComparer, SyncConflictHandlingType conflictHandlingType, Api api) 
            : base(fileComparer, conflictHandlingType, api)
        {
        }

        public override async Task<SyncActionType> GetActionOfBothFiles(FilePair pair)
        {
            Task<object> serverCompareValueTask = fileComparer.GetServerCompareValue(pair.ServerFullPath, api);
            Task<object> localCompareValueTask = fileComparer.GetLocalCompareValue(pair.LocalFile);

            pair.ServerCompareValue = await serverCompareValueTask;
            pair.LocalCompareValue = await localCompareValueTask;

            return fileComparer.Equals(pair.ServerCompareValue, pair.LocalCompareValue) ?
                SyncActionType.Equal : SyncActionType.CopyToServer;
        }

        public override Task<SyncActionType> GetActionOfSingleFiles(FilePair pair)
        {
            return Task.FromResult(pair.LocalFile != null ? SyncActionType.CopyToServer : SyncActionType.DeleteFromServer);
        }
    }
}
