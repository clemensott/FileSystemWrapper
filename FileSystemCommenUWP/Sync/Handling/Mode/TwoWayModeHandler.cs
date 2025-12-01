using FileSystemCommon.Models.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling.CompareType;
using System;
using System.Threading.Tasks;

namespace FileSystemCommonUWP.Sync.Handling.Mode
{
    class TwoWayModeHandler : SyncModeHandler
    {
        private readonly SyncPairResult lastResult;

        public override SyncMode Mode => SyncMode.TwoWay;

        public TwoWayModeHandler(BaseSyncFileComparer fileComparer, SyncPairResult lastResult, SyncConflictHandlingType conflictHandlingType)
            : base(fileComparer, conflictHandlingType, true)
        {
            this.lastResult = lastResult;
        }

        public override async Task<SyncActionType> GetActionOfBothFiles(FilePair pair)
        {
            SyncPairResultFile last;

            pair.LocalCompareValue = await fileComparer.GetLocalCompareValue(pair.LocalFile);

            if (fileComparer.EqualsValue(pair.ServerCompareValue, pair.LocalCompareValue))
            {
                return SyncActionType.Equal;
            }
            else if (lastResult.TryGetFile(pair.RelativePath, out last))
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

        public override async Task<SyncActionType> GetActionOfSingleFiles(FilePair pair)
        {
            SyncPairResultFile last;

            if (pair.ServerFileExists)
            {
                if (lastResult.TryGetFile(pair.RelativePath, out last) &&
                    fileComparer.EqualsValue(last.ServerCompareValue, pair.ServerCompareValue))
                {
                    return SyncActionType.DeleteFromServer;
                }
                else return SyncActionType.CopyToLocal;
            }
            else if (pair.LocalFile != null)
            {
                pair.LocalCompareValue = await fileComparer.GetLocalCompareValue(pair.LocalFile);

                if (lastResult.TryGetFile(pair.RelativePath, out last) &&
                    fileComparer.EqualsValue(last.LocalCompareValue, pair.LocalCompareValue))
                {
                    return SyncActionType.DeleteFromLocal;
                }
                else return SyncActionType.CopyToServer;
            }

            throw new InvalidOperationException("Server or local file must exist");
        }
    }
}
