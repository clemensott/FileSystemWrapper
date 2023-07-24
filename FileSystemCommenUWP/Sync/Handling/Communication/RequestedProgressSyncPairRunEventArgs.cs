using System;

namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public class RequestedProgressSyncPairRunEventArgs : EventArgs
    {
        public string RunToken { get; }

        public RequestedProgressSyncPairRunEventArgs(string runToken)
        {
            RunToken = runToken;
        }
    }
}
