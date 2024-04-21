namespace FileSystemCommonUWP
{
    public enum BackgroundTaskStatus : uint
    {
        Unkown = 0,
        Triggered = 1,
        RunningA = 2, // to trigger progress by flip between these two
        RunningB = 3,
        WaitStoppingA = 4, // to trigger progress by flip between these two
        WaitStoppingB = 5,
        Stopping = 6,
        Stopped = 7,
    }
}
