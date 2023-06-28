using System;

namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public class RequestedSyncPairRunEventArgs : EventArgs
    {
        public string RunToken { get; }

        public RequestedSyncPairRunEventArgs(string runToken)
        {
            RunToken = runToken;
        }
    }
}
