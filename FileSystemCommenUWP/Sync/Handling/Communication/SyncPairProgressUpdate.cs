namespace FileSystemCommonUWP.Sync.Handling.Communication
{
    public struct SyncPairProgressUpdate
    {
        public string Token { get; set; }

        public string Prop { get; set; }

        public SyncPairProgressUpdateAction Action { get; set; }

        public int? Number { get; set; }

        public string Text { get; set; }

        public SyncPairHandlerState? State { get; set; }

        public FilePairInfo? File { get; set; }

        public ErrorFilePairInfo? ErrorFile { get; set; }
    }
}
