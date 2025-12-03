using FileSystemCommon.Models.Sync.Handling;

namespace FileSystemCommonUWP.Sync.Handling
{
    public struct SyncPairResultFile: IBaseSyncPairStateFile
    {
        public string RelativePath { get; set; }

        public object LocalCompareValue { get; set; }

        public object ServerCompareValue { get; set; }
    }
}
