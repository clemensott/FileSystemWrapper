using FileSystemUWP.API;
using FileSystemUWP.Sync.Definitions;
using FileSystemUWP.Sync.Handling.CompareType;
using FileSystemUWP.Sync.Result;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileSystemUWP.Sync.Handling.Mode
{
    class ServerToLocalModeHandler : SyncModeHandler
    {
        public ServerToLocalModeHandler(ISyncFileComparer fileComparer, IDictionary<string, SyncedItem> lastResult,
            SyncConflictHandlingType conflictHandlingType, Api api) :
            base(fileComparer, lastResult, conflictHandlingType, api)
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
            return Task.FromResult(pair.ServerFileExists ? SyncActionType.CopyToLocal : SyncActionType.DeleteFromLocal);
        }
    }
}
