using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling.CompareType;
using System.Threading.Tasks;

namespace FileSystemCommonUWP.Sync.Handling.Mode
{
    class ServerToLocalCreateOnlyModeHandler : SyncModeHandler
    {
        public override SyncMode Mode => SyncMode.ServerToLocalCreateOnly;

        public ServerToLocalCreateOnlyModeHandler(ISyncFileComparer fileComparer, SyncConflictHandlingType conflictHandlingType, Api api)
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
                SyncActionType.Equal : SyncActionType.CopyToLocal;
        }

        public override Task<SyncActionType> GetActionOfSingleFiles(FilePair pair)
        {
            return Task.FromResult(pair.ServerFileExists ? SyncActionType.CopyToLocal : SyncActionType.Ignore);
        }
    }
}
