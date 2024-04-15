using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling.CompareType;
using System;
using System.Threading.Tasks;

namespace FileSystemCommonUWP.Sync.Handling.Mode
{
    class TwoWayModeHandler : SyncModeHandler
    {
        private readonly SyncPairResult lastResult;

        public override SyncMode Mode => SyncMode.TwoWay;

        public TwoWayModeHandler(ISyncFileComparer fileComparer, SyncPairResult lastResult,
            SyncConflictHandlingType conflictHandlingType, Api api) : base(fileComparer, conflictHandlingType, api)
        {
            this.lastResult = lastResult;
        }

        public override async Task<SyncActionType> GetActionOfBothFiles(FilePair pair)
        {
            SyncPairResultFile last;

            Task<object> serverCompareValueTask = fileComparer.GetServerCompareValue(pair.ServerFullPath, api);
            Task<object> localCompareValueTask = fileComparer.GetLocalCompareValue(pair.LocalFile);

            pair.ServerCompareValue = await serverCompareValueTask;
            pair.LocalCompareValue = await localCompareValueTask;

            if (fileComparer.Equals(pair.ServerCompareValue, pair.LocalCompareValue))
            {
                return SyncActionType.Equal;
            }
            else if (lastResult.TryGetFile(pair.RelativePath, out last))
            {
                if (fileComparer.Equals(last.ServerCompareValue, pair.ServerCompareValue))
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
            SyncPairResultFile last;

            if (pair.ServerFileExists)
            {
                pair.ServerCompareValue = await fileComparer.GetServerCompareValue(pair.ServerFullPath, api);

                if (lastResult.TryGetFile(pair.RelativePath, out last) &&
                    fileComparer.Equals(last.ServerCompareValue, pair.ServerCompareValue))
                {
                    return SyncActionType.DeleteFromServer;
                }
                else return SyncActionType.CopyToLocal;
            }
            else if (pair.LocalFile != null)
            {
                pair.LocalCompareValue = await fileComparer.GetLocalCompareValue(pair.LocalFile);

                if (lastResult.TryGetFile(pair.RelativePath, out last) &&
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
