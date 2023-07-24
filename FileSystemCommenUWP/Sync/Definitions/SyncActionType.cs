namespace FileSystemCommonUWP.Sync.Definitions
{
    public enum SyncActionType
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
