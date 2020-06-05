using FileSystemUWP.Sync.Definitions;
using FileSystemUWP.Sync.Handling.CompareType;
using FileSystemUWP.Sync.Result;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileSystemUWP.Sync.Handling.Mode
{
    class ServerToLocalCreateOnlyModeHandler : SyncModeHandler
    {
        public ServerToLocalCreateOnlyModeHandler(ISyncFileComparer fileComparer, IDictionary<string, SyncedItem> lastResult,
            SyncConflictHandlingType conflictHandlingType, Api api) :
            base(fileComparer, lastResult, conflictHandlingType, api)
        {
        }

        public override async Task<SyncActionType> GetActionOfBothFiles(FilePair pair)
        {
            SyncedItem last;

            Task<object> serverCompareValueTask = fileComparer.GetServerCompareValue(pair.ServerFullPath, api);
            Task<object> localCompareValueTask = fileComparer.GetLocalCompareValue(pair.LocalFile);

            pair.ServerCompareValue = await serverCompareValueTask;
            pair.LocalCompareValue = await localCompareValueTask;

            if (fileComparer.Equals(pair.ServerCompareValue, pair.LocalCompareValue))
            {
                return SyncActionType.Equal;
            }
            else if (lastResult.TryGetValue(pair.RelativePath, out last) &&
                fileComparer.Equals(last.ServerCompareValue, pair.ServerCompareValue) &&
                fileComparer.Equals(last.LocalCompareValue, pair.LocalCompareValue))
            {
                return SyncActionType.Equal;
            }
            else return SyncActionType.CopyToLocal;
        }

        public override Task<SyncActionType> GetActionOfSingleFiles(FilePair pair)
        {
            return Task.FromResult(pair.ServerFileExists ? SyncActionType.CopyToLocal : SyncActionType.Ignore);
        }
    }
}
