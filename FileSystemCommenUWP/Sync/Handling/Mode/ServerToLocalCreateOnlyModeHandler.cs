﻿using FileSystemCommonUWP.Sync.Definitions;
using FileSystemCommonUWP.Sync.Handling.CompareType;
using System.Threading.Tasks;

namespace FileSystemCommonUWP.Sync.Handling.Mode
{
    class ServerToLocalCreateOnlyModeHandler : SyncModeHandler
    {
        public override SyncMode Mode => SyncMode.ServerToLocalCreateOnly;

        public ServerToLocalCreateOnlyModeHandler(BaseSyncFileComparer fileComparer, SyncConflictHandlingType conflictHandlingType)
            : base(fileComparer, conflictHandlingType)
        {
        }

        public override async Task<SyncActionType> GetActionOfBothFiles(FilePair pair)
        {
            pair.LocalCompareValue = await fileComparer.GetLocalCompareValue(pair.LocalFile);

            return fileComparer.EqualsValue(pair.ServerCompareValue, pair.LocalCompareValue) ?
                SyncActionType.Equal : SyncActionType.CopyToLocal;
        }

        public override Task<SyncActionType> GetActionOfSingleFiles(FilePair pair)
        {
            return Task.FromResult(pair.ServerFileExists ? SyncActionType.CopyToLocal : SyncActionType.Ignore);
        }
    }
}
