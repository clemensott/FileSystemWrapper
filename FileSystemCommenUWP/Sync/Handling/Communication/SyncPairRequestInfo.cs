using FileSystemCommonUWP.Sync.Definitions;

namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public struct SyncPairRequestInfo
    {
        public string RunToken { get; set; }

        public string Token { get; set; }

        public string ResultToken { get; set; }

        public SyncMode Mode { get; set; }

        public SyncCompareType CompareType { get; set; }

        public SyncConflictHandlingType ConflictHandlingType { get; set; }

        public bool WithSubfolders { get; set; }

        public string Name { get; set; }

        public string LocalPath { get; set; }

        public string ServerNamePath { get; set; }

        public string ServerPath { get; set; }

        public string[] Whitelist { get; set; }

        public string[] Blacklist { get; set; }

        public bool IsTestRun { get; set; }

        public string ApiBaseUrl { get; set; }
    }
}
