namespace FileSystemCommonUWP.Sync.Handling
{
    public enum SyncPairRunFileType
    {
        All = 0,
        Compared = 1,
        Equal = 2,
        Conflict = 4,
        CopiedLocal = 8,
        CopiedServer = 16,
        DeletedLocal = 32,
        DeletedServer = 64,
        Error = 128,
        Ignore = 256,
    }
}
