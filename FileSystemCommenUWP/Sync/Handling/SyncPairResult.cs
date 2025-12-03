using FileSystemCommon.Models.Sync.Handling;
using System.Collections.Generic;

namespace FileSystemCommonUWP.Sync.Handling
{
    public class SyncPairResult : BaseSyncPairState<SyncPairResultFile>
    {
        public SyncPairResult()
        {

        }

        public SyncPairResult(IEnumerable<SyncPairResultFile> files)
        {
            foreach (SyncPairResultFile file in files)
            {
                AddFile(file);
            }
        }
    }
}
