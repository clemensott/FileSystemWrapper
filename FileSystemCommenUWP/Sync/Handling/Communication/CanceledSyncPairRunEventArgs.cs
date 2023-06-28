using System;

namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public class CanceledSyncPairRunEventArgs : EventArgs
    {
        public string RunToken { get; }

        public CanceledSyncPairRunEventArgs(string runToken)
        {
            RunToken = runToken;
        }
    }
}
