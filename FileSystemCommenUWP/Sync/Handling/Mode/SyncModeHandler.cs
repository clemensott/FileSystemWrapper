using FileSystemCommon.Models.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling.CompareType;
using System;
using System.Threading.Tasks;

namespace FileSystemCommonUWP.Sync.Handling.Mode
{
    abstract class SyncModeHandler
    {
        protected readonly BaseSyncFileComparer fileComparer;
        protected readonly SyncConflictHandlingType conflictHandlingType;

        public bool PreloadServerCompareValue { get; }

        public abstract SyncMode Mode { get; }

        protected SyncModeHandler(BaseSyncFileComparer fileComparer, SyncConflictHandlingType conflictHandlingType, bool preLoadServerCompareValue = false)
        {
            this.fileComparer = fileComparer;
            this.conflictHandlingType = conflictHandlingType;
            PreloadServerCompareValue = preLoadServerCompareValue;
        }

        public abstract Task<SyncActionType> GetActionOfBothFiles(FilePair pair);

        protected SyncActionType SolveConflict(FilePair pair)
        {
            switch (conflictHandlingType)
            {
                case SyncConflictHandlingType.PreferServer:
                    return SyncActionType.CopyToLocalByConflict;

                case SyncConflictHandlingType.PreferLocal:
                    return SyncActionType.CopyToServerByConflict;

                case SyncConflictHandlingType.Ignore:
                    return SyncActionType.Ignore;
            }

            throw new ArgumentException("Value not Implemented:" + conflictHandlingType, nameof(conflictHandlingType));
        }

        public abstract Task<SyncActionType> GetActionOfSingleFiles(FilePair pair);
    }
}
