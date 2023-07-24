using System;
using System.Collections.Generic;

namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public class ProgressUpdatesSyncPairRunEventArgs : EventArgs
    {
        public ICollection<SyncPairProgressUpdate> Updates { get; }

        public ProgressUpdatesSyncPairRunEventArgs(ICollection<SyncPairProgressUpdate> updates)
        {
            Updates = updates;
        }
    }
}
