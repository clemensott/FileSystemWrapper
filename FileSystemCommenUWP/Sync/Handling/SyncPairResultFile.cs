namespace FileSystemCommonUWP.Sync.Handling
{
    public struct SyncPairResultFile
    {
        public string RelativePath { get; set; }

        public object LocalCompareValue { get; set; }

        public object ServerCompareValue { get; set; }
    }
}
