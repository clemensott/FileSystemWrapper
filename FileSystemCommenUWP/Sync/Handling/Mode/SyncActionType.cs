﻿namespace FileSystemCommonUWP.Sync.Handling.Mode
{
    enum SyncActionType
    {
        CopyToLocal,
        CopyToServer,
        CopyToLocalByConflict,
        CopyToServerByConflict,
        DeleteFromLocal,
        DeleteFromServer,
        Equal,
        Ignore,
    }
}
