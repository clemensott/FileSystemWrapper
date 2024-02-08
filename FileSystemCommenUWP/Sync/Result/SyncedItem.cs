namespace FileSystemCommonUWP.Sync.Result
{
    public struct SyncedItem
    {
        public bool IsFile { get; set; }

        public string RelativePath { get; set; }

        public object ServerCompareValue { get; set; }

        public object LocalCompareValue { get; set; }
    }
}
