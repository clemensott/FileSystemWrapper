using FileSystemUWP.Sync.Definitions;
using FileSystemUWP.Sync.Handling.CompareType;
using FileSystemUWP.Sync.Result;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileSystemUWP.Sync.Handling.Mode
{
    class TwoWayModeHandler : SyncModeHandler
    {
        public TwoWayModeHandler(ISyncFileComparer fileComparer, IDictionary<string, SyncedItem> lastResult,
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
            else if (lastResult.TryGetValue(pair.RelativePath, out last))
            {
                if (fileComparer.Equals(last.ServerCompareValue, pair.ServerCompareValue) &&
                    fileComparer.Equals(last.LocalCompareValue, pair.LocalCompareValue))
                {
                    return SyncActionType.Equal;
                }
                else if (fileComparer.Equals(last.ServerCompareValue, pair.ServerCompareValue))
                {
                    return SyncActionType.CopyToServer;
                }
                else if (fileComparer.Equals(last.LocalCompareValue, pair.LocalCompareValue))
                {
                    return SyncActionType.CopyToLocal;
                }
            }

            return SolveConflict(pair);
        }

        public override async Task<SyncActionType> GetActionOfSingleFiles(FilePair pair)
        {
            SyncedItem last;

            if (pair.ServerFileExists)
            {
                pair.ServerCompareValue = await fileComparer.GetServerCompareValue(pair.ServerFullPath, api);

                if (lastResult.TryGetValue(pair.RelativePath, out last) &&
                    fileComparer.Equals(last.ServerCompareValue, pair.ServerCompareValue))
                {
                    return SyncActionType.DeleteFromServer;
                }
                else return SyncActionType.CopyToLocal;
            }
            else if (pair.LocalFile != null)
            {
                pair.LocalCompareValue = await fileComparer.GetLocalCompareValue(pair.LocalFile);

                if (lastResult.TryGetValue(pair.RelativePath, out last) &&
                    fileComparer.Equals(last.LocalCompareValue, pair.LocalCompareValue))
                {
                    return SyncActionType.DeleteFromLocal;
                }
                else return SyncActionType.CopyToServer;
            }

            throw new InvalidOperationException("Server or local file must exist");
        }
    }
}
