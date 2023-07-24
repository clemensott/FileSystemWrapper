using FileSystemCommonUWP.API;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling.CompareType;
using System;
using System.Threading.Tasks;

namespace FileSystemCommonUWP.Sync.Handling.Mode
{
    abstract class SyncModeHandler
    {
        protected readonly ISyncFileComparer fileComparer;
        protected readonly SyncConflictHandlingType conflictHandlingType;
        protected readonly Api api;

        public abstract SyncMode Mode { get; }

        protected SyncModeHandler(ISyncFileComparer fileComparer, SyncConflictHandlingType conflictHandlingType, Api api)
        {
            this.fileComparer = fileComparer;
            this.conflictHandlingType = conflictHandlingType;
            this.api = api;
        }

        public abstract Task<SyncActionType> GetActionOfBothFiles(FilePair pair);

        protected SyncActionType SolveConflict(FilePair pair)
        {
            switch (conflictHandlingType)
            {
                case SyncConflictHandlingType.PreferServer:
                    return SyncActionType.CopyToServerByConflict;

                case SyncConflictHandlingType.PreferLocal:
                    return SyncActionType.CopyToLocalByConflict;

                case SyncConflictHandlingType.Igonre:
                    return SyncActionType.Ignore;
            }

            throw new ArgumentException("Value not Implemented:" + conflictHandlingType, nameof(conflictHandlingType));
        }

        public abstract Task<SyncActionType> GetActionOfSingleFiles(FilePair pair);
    }
}
