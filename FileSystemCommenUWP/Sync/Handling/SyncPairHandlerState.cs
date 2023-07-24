namespace FileSystemCommonUWP.Sync.Handling
{
    public enum SyncPairHandlerState
    {
        Loading,
        WaitForStart,
        Running,
        Finished,
        Error,
        Canceled,
    }
}
