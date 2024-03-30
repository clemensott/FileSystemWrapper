using FileSystemCommonUWP.Sync.Definitions;

namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public struct SyncPairRequestInfo
    {
        /// <summary>
        /// GUID that equals SyncPairResponseInfo.RunToken and is used to match with the response
        /// </summary>
        public string RunToken { get; set; }

        /// <summary>
        /// GUID to identify the sync pair and used to save the access to the local StorageFolder
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// GUID to save the Result (SyncedItems) of the sync run
        /// </summary>
        public string ResultToken { get; set; }

        public bool WithSubfolders { get; set; }

        public bool IsTestRun { get; set; }

        public bool IsCanceled { get; set; }

        public SyncMode Mode { get; set; }

        public SyncCompareType CompareType { get; set; }

        public SyncConflictHandlingType ConflictHandlingType { get; set; }

        public string Name { get; set; }

        public string LocalPath { get; set; }

        public string ServerNamePath { get; set; }

        public string ServerPath { get; set; }

        public string[] AllowList { get; set; }

        public string[] DenialList { get; set; }

        public string ApiBaseUrl { get; set; }
    }
}
