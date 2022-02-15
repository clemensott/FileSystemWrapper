using FileSystemCommon.Models.FileSystem;
using FileSystemUWP.Sync.Definitions;
using FileSystemUWP.Sync.Result;

namespace FileSystemUWP.SettingsStorage
{
    public class SyncPairStore
    {
        public bool WithSubfolders { get; set; }

        public string Token { get; set; }

        public string Name { get; set; }

        public PathPart[] ServerPath { get; set; }

        public SyncMode Mode { get; set; }

        public SyncCompareType CompareType { get; set; }

        public SyncConflictHandlingType ConflictHandlingType { get; set; }

        public string[] Whitelist { get; set; }

        public string[] Blacklist { get; set; }

        public SyncedItem[] Result { get; set; }
    }
}
