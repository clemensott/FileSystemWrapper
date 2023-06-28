using System;

namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public class DeletedSyncPairRunEventArgs : EventArgs
    {
        public string RunToken { get; }

        public DeletedSyncPairRunEventArgs(string runToken)
        {
            RunToken = runToken;
        }
    }
}
