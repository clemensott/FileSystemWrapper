namespace FileSystemCommonUWP.Sync.Handling
{
    public struct SyncPairRunErrorFile
    {
        public string Name { get; set; }

        public string RelativePath { get; set; }

        public string Message { get; set; }

        public string Stacktrace { get; set; }

        public string Exception { get; set; }
    }
}
