namespace FileSystemCommonUWP.Sync.Handling
{
    public enum SyncPairHandlerState
    {
        Requesting,
        WaitForStart,
        Running,
        Finished,
        Error,
        Canceled,
    }
}
