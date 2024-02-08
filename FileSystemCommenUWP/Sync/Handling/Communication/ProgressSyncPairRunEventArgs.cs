using System;

namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public class ProgressSyncPairRunEventArgs : EventArgs
    {
        public SyncPairResponseInfo Response { get; }

        public ProgressSyncPairRunEventArgs(SyncPairResponseInfo response)
        {
            Response = response;
        }
    }
}
