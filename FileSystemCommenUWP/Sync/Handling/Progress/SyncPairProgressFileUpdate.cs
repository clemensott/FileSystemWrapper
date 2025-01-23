using FileSystemCommonUWP.Sync.Handling;

namespace FileSystemCommonUWP.Sync.Handling.Progress
{
    public struct SyncPairProgressFileUpdate
    {
        public string RelativePath { get; }

        public SyncPairRunFileType Type { get; }

        public bool IncreaseCurrentCount { get; }

        public SyncPairProgressFileUpdate(string relativePath, SyncPairRunFileType type, bool increaseCurrentCount) : this()
        {
            RelativePath = relativePath;
            Type = type;
            IncreaseCurrentCount = increaseCurrentCount;
        }
    }
}
